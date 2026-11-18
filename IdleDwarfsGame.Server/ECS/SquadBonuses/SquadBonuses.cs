// Механики бонусов стихий отряда (SquadBonusBook). Каждый метод — точка, в которой бой спрашивает бонусы:
// наложение статуса, тик, удар, каст, начало раунда, урон и лечение, смерть.
// Бонус действует от имени своей команды: статусы и удары его отряда по врагам, лечение и защита его дворфов.
// Записи бонуса в логе — Phase = "Bonus", ActionType/SkillName — Id бонуса (фронт покажет их позже)
public static class SquadBonuses
{
    public const string Phase = "Bonus";

    // ===================== Подключение =====================

    // Отряд игрока (EntityFactory.CreateSquad): бонус по стихиям его дворфов. Стихия считается только у дворфов —
    // монстр в отряде (если его так собрать) идёт как Basic
    public const string DwarfUnitType = "Dwarf";

    public static void Attach(World world, int teamId, IEnumerable<Hero> heroes)
    {
        var elements = heroes
            .Select(h => h.UnitType == DwarfUnitType ? HeroLevels.NormalizeElement(h.Level, h.Element) : HeroElement.Basic)
            .ToList();
        if (SquadBonusBook.Resolve(elements) is not { } bonus) return;
        world.SquadBonuses[teamId] = new SquadBonusState(teamId, bonus, elements.Where(e => e != HeroElement.Basic).ToList());
    }

    public static SquadBonusState? Of(World world, int entityId) =>
        world.SquadBonuses.Count > 0 && entityId > 0 && world.Team.TryGetValue(entityId, out var team)
            ? world.SquadBonuses.GetValueOrDefault(team.TeamId)
            : null;

    // Бонус команды того, кто наложил большую часть статуса
    private static SquadBonusState? OfSource(World world, StatusEffectBase effect) =>
        world.SquadBonuses.Count == 0 ? null : Of(world, effect.MainSource());

    // ===================== Наложение статусов =====================

    // До сложения с висящим: бонус может усилить наложение своего отряда
    public static void BeforeApply(World world, int targetId, int sourceId, StatusEffectBase effect)
    {
        if (Of(world, sourceId) is not { Reacting: false } state) return;
        bool enemy = Enemies(world, sourceId, targetId);

        int percent = state.Id switch
        {
            SquadBonusId.Frost when enemy && effect is FrostEffect => SquadBonusBook.ByTier(SquadBonusBook.FrostSlowPercent, state.Tier),
            SquadBonusId.Nature when enemy && effect is PoisonEffect => SquadBonusBook.ByTier(SquadBonusBook.NaturePoisonPercent, state.Tier),
            SquadBonusId.NatureRadiance when !enemy && effect is RegenEffect => SquadBonusBook.NatureRadianceRegenPercent,
            _ => 0
        };
        if (percent > 0)
        {
            effect.Strengthen(percent);
            state.Triggers++;
        }

        // ❄🩸 «Кровавый лёд»: стак на замороженной цели — в полную силу
        if (state.Is(SquadBonusId.FrostBlood) && enemy && effect is BleedEffect bleed && Frosted(world, targetId))
        {
            bleed.FullStrength = true;
            state.Triggers++;
        }
    }

    // Статус уже висит на цели: реакции на сочетания статусов
    public static void StatusApplied(World world, CombatContext context, int targetId, string effect, int sourceId)
    {
        if (Of(world, sourceId) is not { Reacting: false } state || !world.IsAlive(targetId)) return;
        if (!Enemies(world, sourceId, targetId)) return;

        switch (state.Id)
        {
            case SquadBonusId.FireFrost when effect is "Burn" or "Frost":
                FireAndIce(world, context, state, targetId, sourceId);
                break;
            case SquadBonusId.Frost when effect == "Frost":
                Permafrost(world, context, state, targetId, sourceId);
                break;
            case SquadBonusId.FrostRadiance when effect == "Frost" && state.LastHealAction != context.CurrentActionId:
                state.LastHealAction = context.CurrentActionId;
                if (MostWounded(world, state.TeamId) is { } ally)
                    React(state, () => Heal(world, context, state, sourceId, ally,
                        Percent(world.Health[ally].Max, SquadBonusBook.FrostRadianceHealPercent)));
                break;
        }
    }

    // 🔥❄ Горение и мороз на одной цели: оба снимаются, взрыв бьёт всех врагов
    private static void FireAndIce(World world, CombatContext context, SquadBonusState state, int targetId, int sourceId)
    {
        if (Find<BurnEffect>(world, targetId) is not { } burn || Find<FrostEffect>(world, targetId) is not { } frost) return;
        // Один взрыв за раунд на весь отряд (ключ 0 — отряд, а не цель)
        if (!Ready(state, 0, cooldown: 1)) return;

        long byHp = (long)world.Health[targetId].Max * SquadBonusBook.FireFrostMaxHpPercent / 100;
        long cap = (long)world.Attack[sourceId].Damage * SquadBonusBook.FireFrostAttackCap / 100;
        int amount = (int)Math.Min(byHp, cap);

        Remove(world, context, targetId, burn);
        Remove(world, context, targetId, frost);
        int team = world.Team[targetId].TeamId;
        React(state, () =>
        {
            Strike(world, context, state, sourceId, targetId, amount, DamageType.Fire);
            foreach (int enemy in AliveOf(world, team).Where(id => id != targetId))
                Strike(world, context, state, sourceId, enemy, Percent(amount, SquadBonusBook.FireFrostSplashPercent), DamageType.Fire);
        });
    }

    // ❄ Мороз набрал порог: снимается, цель вмерзает на ход (не чаще раза в FrostFreezeCooldown раундов)
    private static void Permafrost(World world, CombatContext context, SquadBonusState state, int targetId, int sourceId)
    {
        if (Find<FrostEffect>(world, targetId) is not { } frost) return;
        if (frost.EnergySlowPercent < SquadBonusBook.ByTier(SquadBonusBook.FrostFreezeThreshold, state.Tier)) return;
        if (!Ready(state, targetId, SquadBonusBook.FrostFreezeCooldown)) return;

        Remove(world, context, targetId, frost);
        React(state, () => new FreezeEffect(turns: 1).ApplyEffect(world, targetId, context, sourceId));
    }

    // Выжигание энергии (аура тьмы, проклятие, эффекты бонусов): 🌑 отдаёт часть сожжённого союзнику
    public static void EnergyBurned(World world, int targetId, int sourceId, int burned)
    {
        if (burned <= 0 || Of(world, sourceId) is not { } state || !state.Is(SquadBonusId.Dark)) return;
        if (!Enemies(world, sourceId, targetId)) return;

        int gain = burned * SquadBonusBook.ByTier(SquadBonusBook.DarkStealPercent, state.Tier) / 100;
        var ally = AliveOf(world, state.TeamId)
            .Where(id => world.Energy.ContainsKey(id))
            .OrderBy(id => world.Energy[id].Current * 100 / Math.Max(1, world.Energy[id].Max))
            .FirstOrDefault();
        if (gain <= 0 || ally == 0) return;

        var energy = world.Energy[ally];
        energy.Current = Math.Min(energy.Max, energy.Current + gain);
        state.Triggers++;
    }

    // ===================== Тики статусов =====================

    // Урон тика статуса до прибавки арены
    public static int TickDamage(World world, int targetId, StatusEffectBase effect, int damage)
    {
        if (OfSource(world, effect) is not { } state) return damage;
        int sourceId = effect.MainSource();
        if (!Enemies(world, sourceId, targetId)) return damage;

        int percent = TeamPercent(world, state, sourceId, targetId, hit: false);
        int own = state.Id switch
        {
            SquadBonusId.Fire when effect is BurnEffect => SquadBonusBook.ByTier(SquadBonusBook.FireDamagePercent, state.Tier),
            SquadBonusId.Dark when effect is CurseEffect => SquadBonusBook.ByTier(SquadBonusBook.DarkDamagePercent, state.Tier),
            SquadBonusId.FrostNature when effect is PoisonEffect && Frosted(world, targetId) => SquadBonusBook.FrostNaturePoisonPercent,
            SquadBonusId.FrostBlood when effect is BleedEffect && Frosted(world, targetId) => SquadBonusBook.FrostBloodTickPercent,
            SquadBonusId.NatureBlood when effect is PoisonEffect or BleedEffect => SquadBonusBook.NatureBloodTickPercent,
            SquadBonusId.DarkBlood when effect is CurseEffect or BleedEffect => SquadBonusBook.DarkBloodTickPercent,
            SquadBonusId.FireNature when effect is BurnEffect && Has<PoisonEffect>(world, targetId) => SquadBonusBook.FireNatureTickPercent,
            SquadBonusId.FireDark when effect is BurnEffect
                => SquadBonusBook.FireDarkTickPercent * (Has<CurseEffect>(world, targetId) ? 2 : 1),
            SquadBonusId.FrostLightning when effect is ShockEffect && Frosted(world, targetId) => SquadBonusBook.FrostLightningDamagePercent,
            SquadBonusId.FrostDark when effect is CurseEffect && Frosted(world, targetId) => SquadBonusBook.FrostDarkDamagePercent,
            SquadBonusId.NatureDark when effect is PoisonEffect && Has<CurseEffect>(world, targetId) => SquadBonusBook.NatureDarkTickPercent,
            _ => 0
        };
        if (own > 0) state.Triggers++;
        percent += own;
        return percent == 0 ? damage : Math.Max(0, Percent(damage, 100 + percent));
    }

    // Тик нанёс урон: реакции бонуса на него
    public static void Ticked(World world, CombatContext context, int targetId, StatusEffectBase effect, int damage)
    {
        if (damage <= 0 || OfSource(world, effect) is not { Reacting: false } state) return;
        int sourceId = effect.MainSource();
        if (!Enemies(world, sourceId, targetId)) return;
        int team = world.Team[targetId].TeamId;

        switch (state.Id)
        {
            case SquadBonusId.Blood when effect is BleedEffect:
                HealMostWounded(world, context, state, sourceId, Percent(damage, SquadBonusBook.ByTier(SquadBonusBook.BloodBleedLifesteal, state.Tier)));
                break;

            case SquadBonusId.FireLightning when effect is ShockEffect && Find<BurnEffect>(world, targetId) is { } burn:
                React(state, () => Strike(world, context, state, sourceId, targetId,
                    Percent(burn.LivePower, SquadBonusBook.FireLightningBurnPercent), DamageType.Fire));
                break;

            case SquadBonusId.FireNature when effect is BurnEffect && Has<PoisonEffect>(world, targetId):
                if (NextInLine(world, targetId) is { } neighbour)
                    React(state, () => Strike(world, context, state, sourceId, neighbour,
                        Percent(damage, SquadBonusBook.FireNatureSplashPercent), DamageType.Fire));
                break;

            case SquadBonusId.FireDark when effect is BurnEffect && world.IsAlive(targetId):
                React(state, () => new EnergyBurnEffect(SquadBonusBook.FireDarkEnergy).ApplyEffect(world, targetId, context, sourceId));
                break;

            case SquadBonusId.FireRadiance when effect is BurnEffect:
                HealMostWounded(world, context, state, sourceId, Percent(damage, SquadBonusBook.FireRadianceHealPercent));
                break;

            case SquadBonusId.LightningNature when effect is ShockEffect:
                HealMostWounded(world, context, state, sourceId, Percent(damage, SquadBonusBook.LightningNatureShockHealPercent));
                break;

            case SquadBonusId.LightningNature when effect is PoisonEffect && context.Random.Next(100) < SquadBonusBook.LightningNatureZapChance:
                var others = AliveOf(world, team).Where(id => id != targetId).ToList();
                int zapped = others.Count > 0 ? others[context.Random.Next(others.Count)] : targetId;
                React(state, () => Strike(world, context, state, sourceId, zapped,
                    Percent(damage, SquadBonusBook.LightningNatureZapPercent), DamageType.Lightning));
                break;

            case SquadBonusId.DarkBlood when effect is CurseEffect && Has<BleedEffect>(world, targetId) && world.IsAlive(targetId):
                React(state, () => new BleedEffect(Percent(damage, SquadBonusBook.DarkBloodBleedPercent))
                    .ApplyEffect(world, targetId, context, sourceId));
                break;

            case SquadBonusId.NatureBlood when effect is PoisonEffect or BleedEffect:
                HealMostWounded(world, context, state, sourceId, Percent(damage, SquadBonusBook.NatureBloodLifesteal));
                break;
        }
    }

    // 🌿🌑 «Гниль»: яд на проклятой цели не теряет ход (каждый слой — не больше NatureDarkHolds раз)
    public static int HeldTurns(World world, int targetId, StatusEffectBase effect)
    {
        if (effect is not PoisonEffect || OfSource(world, effect) is not { } state || !state.Is(SquadBonusId.NatureDark)) return 0;
        if (!Enemies(world, effect.MainSource(), targetId) || !Has<CurseEffect>(world, targetId)) return 0;
        state.Triggers++;
        return SquadBonusBook.NatureDarkHolds;
    }

    // Тик регена вылечил юнита: 🌿💎 даёт ему энергию
    public static void RegenTicked(World world, int targetId, StatusEffectBase regen)
    {
        if (OfSource(world, regen) is not { } state || !state.Is(SquadBonusId.NatureRadiance)) return;
        if (Enemies(world, regen.MainSource(), targetId) || !world.Energy.TryGetValue(targetId, out var energy)) return;
        energy.Current = Math.Min(energy.Max, energy.Current + SquadBonusBook.NatureRadianceRegenEnergy);
        state.Triggers++;
    }

    // Конец хода юнита: ❄🌑 мороз отряда выжигает ему энергию
    public static void TurnEnded(World world, CombatContext context, int actorId)
    {
        if (world.SquadBonuses.Count == 0 || !world.IsAlive(actorId)) return;
        if (Find<FrostEffect>(world, actorId) is not { } frost || OfSource(world, frost) is not { Reacting: false } state) return;
        if (!state.Is(SquadBonusId.FrostDark) || !Enemies(world, frost.MainSource(), actorId)) return;

        int amount = frost.EnergySlowPercent * SquadBonusBook.FrostDarkEnergyPercentOfSlow / 100;
        if (amount > 0)
            React(state, () => new EnergyBurnEffect(amount).ApplyEffect(world, actorId, context, frost.MainSource()));
    }

    // ===================== Удары и заклинания =====================

    // Прибавка к урону удара или спелла (World.ResolveHit), %; может быть отрицательной (защита отряда)
    public static int HitPercent(World world, int attackerId, int targetId)
    {
        if (world.SquadBonuses.Count == 0 || !Enemies(world, attackerId, targetId)) return 0;
        int percent = 0;

        if (Of(world, attackerId) is { } attacker)
            percent += TeamPercent(world, attacker, attackerId, targetId, hit: true);

        // ❄💎 «Холодный свет»: враг под морозом бьёт отряд слабее
        if (Of(world, targetId) is { } defender && defender.Is(SquadBonusId.FrostRadiance) && Has<FrostEffect>(world, attackerId))
        {
            percent -= SquadBonusBook.FrostRadianceDamageReduction;
            defender.Triggers++;
        }
        return percent;
    }

    // Общие для ударов и тиков прибавки урона команды
    private static int TeamPercent(World world, SquadBonusState state, int sourceId, int targetId, bool hit)
    {
        int percent = state.Id switch
        {
            SquadBonusId.Blood => BloodRage(world, state),
            SquadBonusId.FireBlood when hit && HpPercent(world, sourceId) < SquadBonusBook.FireBloodThreshold
                => SquadBonusBook.FireBloodDamagePercent,
            SquadBonusId.DarkRadiance when state.Night => SquadBonusBook.DarkRadianceNightPercent,
            SquadBonusId.Frost when Has<StunEffect>(world, targetId) => SquadBonusBook.ByTier(SquadBonusBook.FrostFrozenDamagePercent, state.Tier),
            _ => 0
        };
        if (percent > 0) state.Triggers++;
        return percent;
    }

    // 🩸 постоянная прибавка и +1% за каждые N% потерянного HP отряда (павшие — потерянное HP целиком)
    private static int BloodRage(World world, SquadBonusState state)
    {
        long max = 0, current = 0;
        foreach (var entity in world.Entities.Where(e => world.Team[e.Id].TeamId == state.TeamId))
        {
            max += world.Health[entity.Id].Max;
            current += world.Health[entity.Id].Current;
        }
        int baseBonus = SquadBonusBook.ByTier(SquadBonusBook.BloodBaseDamagePercent, state.Tier);
        if (max == 0) return baseBonus;
        int missing = (int)(100 - current * 100 / max);
        return baseBonus + Math.Min(SquadBonusBook.ByTier(SquadBonusBook.BloodDamageCap, state.Tier),
            missing / SquadBonusBook.ByTier(SquadBonusBook.BloodMissingPerPercent, state.Tier));
    }

    // Урон заклинания по цели до защиты (BaseSpell.Hit)
    public static int SpellDamage(World world, int casterId, int targetId, DamageType type, int amount)
    {
        if (Of(world, casterId) is not { } state || !Enemies(world, casterId, targetId)) return amount;

        int percent = state.Id switch
        {
            SquadBonusId.Fire when type == DamageType.Fire => SquadBonusBook.ByTier(SquadBonusBook.FireDamagePercent, state.Tier),
            SquadBonusId.Frost when type == DamageType.Frost => SquadBonusBook.ByTier(SquadBonusBook.FrostDamagePercent, state.Tier),
            SquadBonusId.Dark when type == DamageType.Dark => SquadBonusBook.ByTier(SquadBonusBook.DarkDamagePercent, state.Tier),
            SquadBonusId.FireRadiance when type is DamageType.Fire or DamageType.Radiant && Has<BurnEffect>(world, targetId)
                => SquadBonusBook.FireRadianceDamagePercent,
            SquadBonusId.FrostRadiance when type == DamageType.Radiant && Frosted(world, targetId) => SquadBonusBook.FrostRadianceDamagePercent,
            SquadBonusId.LightningDark when type == DamageType.Lightning && Has<CurseEffect>(world, targetId) => SquadBonusBook.LightningDarkDamagePercent,
            SquadBonusId.Radiance when type == DamageType.Radiant => SquadBonusBook.ByTier(SquadBonusBook.RadianceDamagePercent, state.Tier),
            SquadBonusId.FrostLightning when type == DamageType.Lightning && Frosted(world, targetId) => SquadBonusBook.FrostLightningDamagePercent,
            SquadBonusId.FrostDark when type == DamageType.Dark && Frosted(world, targetId) => SquadBonusBook.FrostDarkDamagePercent,
            _ => 0
        };
        if (percent == 0) return amount;
        state.Triggers++;
        return Percent(amount, 100 + percent);
    }

    // Попадание заклинания по живой цели — после его статусов
    public static void SpellHit(World world, CombatContext context, int casterId, int targetId, DamageType type)
    {
        if (Of(world, casterId) is not { Reacting: false } state || !world.IsAlive(targetId) || !Enemies(world, casterId, targetId)) return;

        switch (state.Id)
        {
            case SquadBonusId.FireLightning when type == DamageType.Lightning && Find<BurnEffect>(world, targetId) is { } burn:
                React(state, () => Strike(world, context, state, casterId, targetId,
                    Percent(burn.LivePower, SquadBonusBook.FireLightningBurnPercent), DamageType.Fire));
                break;

            case SquadBonusId.LightningDark when type == DamageType.Lightning && Find<CurseEffect>(world, targetId) is { } curse:
                React(state, () => Strike(world, context, state, casterId, targetId,
                    Percent(curse.LivePower, SquadBonusBook.LightningDarkCursePercent), DamageType.Dark));
                break;

            case SquadBonusId.FrostLightning when type == DamageType.Lightning && Has<StunEffect>(world, targetId):
                React(state, () => new EnergyBurnEffect(SquadBonusBook.FrostLightningEnergy).ApplyEffect(world, targetId, context, casterId));
                break;

            case SquadBonusId.Prism when state.Resonance is { } element:
                Resonate(world, context, state, casterId, targetId, element);
                break;
        }
    }

    // 🌈 Попадание накладывает статус резонирующей стихии: аура 1-го уровня с долей PrismPowerPercent от её силы
    private static void Resonate(World world, CombatContext context, SquadBonusState state, int casterId, int targetId, HeroElement element)
    {
        if (AuraBook.EffectFor(element) is not { } effect || AuraBook.Find(effect) is not { } aura) return;

        int percent = Math.Max(1, AuraBook.Power(aura, AuraBook.MinLevel) * SquadBonusBook.PrismPowerPercent / 100);
        int power = AuraBook.Resolve(effect, percent, world.Attack[casterId].Damage);
        int duration = AuraBook.Duration(aura, AuraBook.MinLevel);
        int target = aura.Target == AuraTarget.Allies ? MostWounded(world, state.TeamId) ?? casterId : targetId;

        React(state, () => StatusEffectFactory.Create(effect, power, duration).ApplyEffect(world, target, context, casterId));
    }

    // Каст заклинания отрядом (SpellSystem): ⚡ каждый N-й каст — бесплатная молния
    public static void SpellCast(World world, CombatContext context, int casterId)
    {
        if (Of(world, casterId) is not { } state || !state.Is(SquadBonusId.Lightning)) return;
        state.SpellCasts++;
        if (state.SpellCasts % SquadBonusBook.ByTier(SquadBonusBook.LightningEvery, state.Tier) != 0) return;

        var enemies = world.GetAliveEnemies(casterId);
        if (enemies.Count == 0) return;
        int target = enemies[context.Random.Next(enemies.Count)].Id;
        int attack = world.Attack[casterId].Damage;

        React(state, () =>
        {
            Strike(world, context, state, casterId, target,
                Percent(attack, SquadBonusBook.ByTier(SquadBonusBook.LightningDamagePercent, state.Tier)), DamageType.Lightning);
            if (world.IsAlive(target))
                new ShockEffect(SquadBonusBook.LightningShockTurns,
                        Math.Max(1, Percent(attack, SquadBonusBook.ByTier(SquadBonusBook.LightningShockPercent, state.Tier))))
                    .ApplyEffect(world, target, context, casterId);
        });
    }

    // Базовая атака попала (DamageSystem): 🔥🩸 раненый дворф поджигает цель
    public static void BasicHit(World world, CombatContext context, int attackerId, int targetId)
    {
        if (Of(world, attackerId) is not { Reacting: false } state || !state.Is(SquadBonusId.FireBlood)) return;
        if (!world.IsAlive(targetId) || !world.IsAlive(attackerId) || HpPercent(world, attackerId) >= SquadBonusBook.FireBloodThreshold) return;

        int burn = Math.Max(1, Percent(world.Attack[attackerId].Damage, SquadBonusBook.FireBloodBurnPercent));
        React(state, () => new BurnEffect(SquadBonusBook.FireBloodBurnTurns, burn).ApplyEffect(world, targetId, context, attackerId));
    }

    // ===================== Начало раунда =====================

    public static void RoundStarted(World world, CombatContext context)
    {
        foreach (var state in world.SquadBonuses.Values)
        {
            state.Round = context.CurrentRound;
            var allies = AliveOf(world, state.TeamId);
            if (allies.Count == 0) continue;

            switch (state.Id)
            {
                case SquadBonusId.Nature:
                    int percent = SquadBonusBook.ByTier(SquadBonusBook.NatureHealPercent, state.Tier);
                    React(state, () =>
                    {
                        foreach (int ally in allies)
                            Heal(world, context, state, ally, ally, Percent(world.Health[ally].Max, percent));
                    });
                    break;

                case SquadBonusId.LightningRadiance:
                    var enemies = world.Entities.Where(e => world.IsAlive(e.Id) && world.Team[e.Id].TeamId != state.TeamId)
                        .OrderByDescending(e => world.Health[e.Id].Current).ThenBy(e => world.GetPosition(e.Id)).ToList();
                    if (enemies.Count == 0) break;
                    React(state, () =>
                    {
                        foreach (var enemy in enemies.Take(SquadBonusBook.LightningRadianceMarks))
                            new MarkEffect(turns: 1, percent: SquadBonusBook.LightningRadianceMarkPercent)
                                .ApplyEffect(world, enemy.Id, context, allies[0]);
                    });
                    break;

                case SquadBonusId.DarkRadiance:
                    state.Night = context.CurrentRound % 2 == 1;
                    if (state.Night) break;
                    React(state, () =>
                    {
                        foreach (int ally in allies)
                            Heal(world, context, state, ally, ally, Percent(world.Health[ally].Max, SquadBonusBook.DarkRadianceDayHealPercent));
                    });
                    break;

                case SquadBonusId.Prism:
                    var elements = allies.Select(id => world.Info.GetValueOrDefault(id)?.Element ?? HeroElement.Basic)
                        .Where(e => e != HeroElement.Basic).ToList();
                    state.Resonance = elements.Count > 0 ? elements[context.Random.Next(elements.Count)] : null;
                    if (state.Resonance != null) state.Triggers++;
                    break;
            }
        }
    }

    // ===================== Урон, лечение, смерть =====================

    // HP юнита упало до нуля (World.ApplyDamage): 🌿 раз за бой оставляет дворфа с 1 HP — лечение в конце хода
    public static bool TryRevive(World world, int targetId)
    {
        if (Of(world, targetId) is not { } state || !state.Is(SquadBonusId.Nature) || state.ReviveUsed) return false;
        state.ReviveUsed = true;
        world.Health[targetId].Current = 1;
        state.PendingRevives.Add(targetId);
        return true;
    }

    // Юнит потерял lost HP (World.ApplyDamage)
    public static void Damaged(World world, int targetId, int lost)
    {
        if (lost <= 0 || Of(world, targetId) is not { } state) return;
        var hp = world.Health[targetId];

        switch (state.Id)
        {
            // ⚡🩸 «Адреналин»
            case SquadBonusId.LightningBlood when hp.Current > 0 && (long)lost * 100 >= (long)hp.Max * SquadBonusBook.LightningBloodThreshold
                                                 && world.Energy.TryGetValue(targetId, out var energy):
                energy.Current = Math.Min(energy.Max, energy.Current + SquadBonusBook.LightningBloodEnergy);
                state.Triggers++;
                break;

            // 💎🩸 «Мученик»: лечение самого раненого и ответный удар светом
            case SquadBonusId.RadianceBlood:
                state.PendingHeals.Add((targetId, Percent(lost, SquadBonusBook.RadianceBloodPercent)));
                state.PendingStrikes.Add((targetId, Percent(lost, SquadBonusBook.RadianceBloodRetaliatePercent)));
                break;

            // ❄🌿 «Ледяной кокон»
            case SquadBonusId.FrostNature when hp.Current > 0 && HpPercent(world, targetId) < SquadBonusBook.FrostNatureThreshold
                                              && state.CocoonUsed.Add(targetId):
                state.PendingCocoons.Add(targetId);
                break;
        }
    }

    // Лечение ушло сверх максимума HP (World.ApplyHeal)
    public static void Overhealed(World world, int targetId, int overflow)
    {
        if (overflow <= 0 || Of(world, targetId) is not { Reacting: false } state) return;

        switch (state.Id)
        {
            // 💎 «Ореол»: избыток — в щит
            case SquadBonusId.Radiance:
                int cap = Percent(world.Health[targetId].Max, SquadBonusBook.ByTier(SquadBonusBook.RadianceShieldCap, state.Tier));
                int current = world.Shields.GetValueOrDefault(targetId);
                int shield = Math.Min(cap, current + Percent(overflow, SquadBonusBook.ByTier(SquadBonusBook.RadianceShieldPercent, state.Tier)));
                if (shield > current)
                {
                    world.Shields[targetId] = shield;
                    state.Triggers++;
                }
                break;

            // 🌿💎 «Благодатная почва»: избыток — самому раненому
            case SquadBonusId.NatureRadiance:
                state.PendingHeals.Add((targetId, overflow));
                break;
        }
    }

    // Отложенное лечение бонусов — в конце хода (HealthSystem), с логом
    public static void ResolvePending(World world, CombatContext context)
    {
        foreach (var state in world.SquadBonuses.Values)
        {
            ResolvePending(world, context, state);
            ReportTriggers(context, state);
        }
    }

    // Бонус срабатывал с прошлой отметки — одна запись SquadBonus на отряд за действие: по ней фронт пускает
    // импульс от значка бонуса. Усиления (урон, статусы) своих записей не дают, поэтому считаем по Triggers
    private static void ReportTriggers(CombatContext context, SquadBonusState state)
    {
        if (state.Triggers <= state.ReportedTriggers) return;
        state.ReportedTriggers = state.Triggers;
        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.SquadBonus,
            Phase = Phase,
            AttackerTeam = state.TeamId,
            ActionType = state.Id.ToString(),
            SkillName = state.Id.ToString()
        });
    }

    private static void ResolvePending(World world, CombatContext context, SquadBonusState state)
    {
        if (state.PendingRevives.Count == 0 && state.PendingCocoons.Count == 0 && state.PendingHeals.Count == 0
            && state.PendingStrikes.Count == 0) return;

        React(state, () =>
        {
            foreach (int id in state.PendingRevives)
                Heal(world, context, state, id, id, Percent(world.Health[id].Max, SquadBonusBook.ByTier(SquadBonusBook.NatureRevivePercent, state.Tier)));

            foreach (int id in state.PendingCocoons.Where(world.IsAlive))
            {
                Heal(world, context, state, id, id, Percent(world.Health[id].Max, SquadBonusBook.FrostNatureHealPercent));
                world.Shields[id] = world.Shields.GetValueOrDefault(id) + Percent(world.Health[id].Max, SquadBonusBook.FrostNatureShieldPercent);
            }

            foreach (var (from, amount) in state.PendingHeals)
            {
                if (MostWounded(world, state.TeamId, exclude: from) is { } ally)
                    Heal(world, context, state, from, ally, amount);
            }

            foreach (var (from, amount) in state.PendingStrikes)
            {
                var front = world.Entities.Where(e => world.IsAlive(e.Id) && world.Team[e.Id].TeamId != state.TeamId)
                    .OrderBy(e => world.GetPosition(e.Id)).FirstOrDefault();
                if (front != null)
                    Strike(world, context, state, from, front.Id, amount, DamageType.Radiant);
            }
        });

        state.PendingRevives.Clear();
        state.PendingCocoons.Clear();
        state.PendingHeals.Clear();
        state.PendingStrikes.Clear();
    }

    // Юнит погиб с этими статусами (HealthSystem)
    public static void Died(World world, CombatContext context, int deadId, IReadOnlyList<IStatusEffect> statuses)
    {
        if (world.SquadBonuses.Count == 0) return;
        var effects = statuses.OfType<StatusEffectBase>().ToList();

        // 🔥 Горение (его доля по уровню бонуса) переходит на следующего по линии
        if (effects.OfType<BurnEffect>().FirstOrDefault() is { } burn && OfSource(world, burn) is { Reacting: false } fire
            && fire.Is(SquadBonusId.Fire) && NextInLine(world, deadId) is { } next
            && burn.CopyRemaining() is { } copy)
        {
            int share = SquadBonusBook.ByTier(SquadBonusBook.FireSpreadPercent, fire.Tier);
            if (share < 100) copy.Strengthen(share - 100);
            React(fire, () => copy.ApplyEffect(world, next, context, burn.MainSource()));
        }

        // 🌑🩸 «Ритуал»: оставшийся урон проклятия и кровотечения — следующему врагу
        var ritual = effects.Where(e => e is CurseEffect or BleedEffect).ToList();
        if (ritual.Count > 0 && OfSource(world, ritual[0]) is { Reacting: false } blood && blood.Is(SquadBonusId.DarkBlood)
            && NextInLine(world, deadId) is { } heir)
        {
            int sourceId = ritual[0].MainSource();
            long remaining = ritual.Sum(e => (long)e.RemainingDamage);
            long cap = (long)world.Attack[sourceId].Damage * SquadBonusBook.DarkBloodTransferCap / 100;
            int amount = (int)Math.Min(remaining, cap);
            if (amount > 0)
                React(blood, () => Strike(world, context, blood, sourceId, heir, amount, DamageType.Dark));
        }
    }

    // ===================== Помощники =====================

    private static void React(SquadBonusState state, Action body)
    {
        state.Reacting = true;
        state.Triggers++;
        try
        {
            body();
        }
        finally
        {
            state.Reacting = false;
        }
    }

    // Откат по цели в раундах: true — можно срабатывать (и откат запущен)
    private static bool Ready(SquadBonusState state, int targetId, int cooldown)
    {
        if (state.LastProc.TryGetValue(targetId, out int last) && state.Round - last < cooldown) return false;
        state.LastProc[targetId] = state.Round;
        return true;
    }

    // Удар бонуса: проходит защиту цели, как удар (World.ResolveHit)
    private static void Strike(World world, CombatContext context, SquadBonusState state, int sourceId, int targetId, int amount, DamageType type)
    {
        if (amount <= 0 || !world.IsAlive(targetId)) return;
        int damage = world.ResolveHit(sourceId, targetId, amount);
        int hpLeft = world.ApplyDamage(targetId, damage);
        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.Damage,
            Phase = Phase,
            AttackerId = sourceId,
            AttackerTeam = state.TeamId,
            TargetId = targetId,
            TargetTeam = world.Team[targetId].TeamId,
            ActionType = state.Id.ToString(),
            SkillName = state.Id.ToString(),
            DamageType = type,
            Damage = damage,
            TargetRemainingHp = hpLeft
        });
    }

    private static void Heal(World world, CombatContext context, SquadBonusState state, int sourceId, int targetId, int amount)
    {
        if (amount <= 0 || !world.IsAlive(targetId)) return;
        int healed = world.ApplyHeal(targetId, amount);
        if (healed <= 0) return;
        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.Heal,
            Phase = Phase,
            AttackerId = sourceId,
            AttackerTeam = state.TeamId,
            TargetId = targetId,
            TargetTeam = state.TeamId,
            ActionType = state.Id.ToString(),
            SkillName = state.Id.ToString(),
            Heal = healed,
            TargetRemainingHp = world.Health[targetId].Current
        });
    }

    private static void HealMostWounded(World world, CombatContext context, SquadBonusState state, int sourceId, int amount)
    {
        if (amount > 0 && MostWounded(world, state.TeamId) is { } ally)
            React(state, () => Heal(world, context, state, sourceId, ally, amount));
    }

    // Снять статус с цели (взрыв, вмерзание) — фронт видит это как окончание статуса
    private static void Remove(World world, CombatContext context, int targetId, StatusEffectBase effect)
    {
        if (!world.StatusEffects.TryGetValue(targetId, out var component) || !component.ActiveEffects.Remove(effect)) return;
        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.StatusExpired,
            Phase = Phase,
            TargetId = targetId,
            TargetTeam = world.Team[targetId].TeamId,
            ActionType = effect.EffectName,
            TargetRemainingHp = world.Health[targetId].Current,
            ActiveEffects = { effect.ToLogData() }
        });
    }

    private static bool Enemies(World world, int a, int b) =>
        a > 0 && b > 0 && world.Team.TryGetValue(a, out var ta) && world.Team.TryGetValue(b, out var tb) && ta.TeamId != tb.TeamId;

    private static List<int> AliveOf(World world, int team) => world.Entities
        .Where(e => world.Team[e.Id].TeamId == team && world.IsAlive(e.Id))
        .OrderBy(e => world.GetPosition(e.Id))
        .Select(e => e.Id)
        .ToList();

    // Самый раненый живой союзник (меньше всего HP в процентах); null — все целы
    private static int? MostWounded(World world, int team, int exclude = -1)
    {
        var wounded = AliveOf(world, team)
            .Where(id => id != exclude && world.Health[id].Current < world.Health[id].Max)
            .OrderBy(id => (double)world.Health[id].Current / world.Health[id].Max)
            .ToList();
        return wounded.Count > 0 ? wounded[0] : null;
    }

    // Ближайший живой союзник цели по линии: следующий за ней, а если она последняя — ближайший перед ней
    private static int? NextInLine(World world, int targetId)
    {
        int team = world.Team[targetId].TeamId;
        int position = world.GetPosition(targetId);
        var others = AliveOf(world, team).Where(id => id != targetId).ToList();
        if (others.Count == 0) return null;
        return others.Where(id => world.GetPosition(id) > position).Cast<int?>().FirstOrDefault() ?? others[^1];
    }

    // Статус на цели. Истекающий в этом проходе тиков тоже считается: горение на последнем ходу уже без ходов,
    // но тикнуло в этом ходу, и шок, тикающий следом, должен его видеть («Плазма»; так же «Лесной пожар» и др.)
    private static T? Find<T>(World world, int id) where T : class, IStatusEffect =>
        world.StatusEffects.TryGetValue(id, out var component)
            ? component.ActiveEffects.OfType<T>().FirstOrDefault(e => e.RemainingTurns > 0 || e is StatusEffectBase { OnTarget: true })
            : null;

    private static bool Has<T>(World world, int id) where T : class, IStatusEffect => Find<T>(world, id) != null;

    // Под морозом или обледенел
    private static bool Frosted(World world, int id) => Has<FrostEffect>(world, id) || Has<FreezeEffect>(world, id);

    private static int HpPercent(World world, int id) =>
        (int)((long)world.Health[id].Current * 100 / Math.Max(1, world.Health[id].Max));

    private static int Percent(int amount, int percent) =>
        (int)Math.Round((long)amount * percent / 100.0, MidpointRounding.AwayFromZero);
}
