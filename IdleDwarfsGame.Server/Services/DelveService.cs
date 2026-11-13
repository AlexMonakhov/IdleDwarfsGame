using Microsoft.EntityFrameworkCore;

// Причина отказа — фронт показывает по коду переведённый текст (delve.errors.<code>)
public enum DelveError
{
    AlreadyRanThisPeriod,
    NoActiveRun,
    // Сначала нужно выбрать реликвию, награду или решить, что делать на привале
    ChoicePending,
    NothingPending,
    InvalidMove,
    InvalidRelic,
    InvalidCampAction,
    NoFallen,
    InvalidReward,
    InvalidTarget,
    InvalidOrder,
    CannotRevive,
    NotEnoughRainbowCoins,
    Changed
}

public class DelveException(DelveError code, string message) : InvalidOperationException(message)
{
    public DelveError Code { get; } = code;
}

// «Глубинный спуск» (правила — DelveRules). Сервис работает с GameDbContext напрямую.
// Бой собирается вручную: отряд забега с сохранённым HP и реликвиями против монстров уровня кампании по глубине
public class DelveService(
    GameDbContext db,
    ICampaignRepository campaign,
    IPlayerRepository players,
    IEntityFactory entityFactory,
    ICombatEngine engine,
    IEnumerable<ICombatScenario> scenarios,
    TimeProvider clock)
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<DelveOverview> GetAsync(Guid userId) => await DescribeAsync(userId, Now);

    public async Task<DelveOverview> StartAsync(Guid userId)
    {
        var now = Now;
        var period = DelveRules.PeriodOf(now);
        var playerId = await PlayerIdAsync(userId);
        if (await db.DelveRuns.AnyAsync(r => r.UserId == userId && r.PeriodStart == period))
            throw new DelveException(DelveError.AlreadyRanThisPeriod, "You have already delved in this period");

        // Незаконченный прошлый забег закрывается: глубина засчитана, активный забег всегда один
        await db.DelveRuns
            .Where(r => r.UserId == userId && r.Status == DelveRunStatus.Active)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Status, DelveRunStatus.Retreated)
                .SetProperty(r => r.Pending, DelvePending.None)
                .SetProperty(r => r.EndedAt, now));

        int seed = Random.Shared.Next();
        var squad = DelveRules.StartSquad(playerId, new Random(seed));
        var run = new DelveRun
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlayerId = playerId,
            PeriodStart = period,
            Seed = seed,
            Column = DelveRules.StartColumn,
            Status = DelveRunStatus.Active,
            StartedAt = now
        };
        run.SetSquad(squad);
        run.SetHp(squad.ToDictionary(h => h.Id, _ => 1.0));
        db.DelveRuns.Add(run);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            throw new DelveException(DelveError.AlreadyRanThisPeriod, "You have already delved in this period");
        }
        return await DescribeAsync(userId, now);
    }

    // Спуститься в узел следующего этажа. Бой — сразу проводится, привал и сокровищница — ждут решения
    public async Task<DelveMoveResult> MoveAsync(Guid userId, int column)
    {
        var now = Now;
        var run = await RequireActiveAsync(userId);
        if (run.Pending != DelvePending.None)
            throw new DelveException(DelveError.ChoicePending, "Make your choice first");
        if (!DelveRules.CanMove(run.Floor, run.Column, column))
            throw new DelveException(DelveError.InvalidMove, "You can't go there");

        var node = DelveRules.NodeAt(run.Seed, run.Floor + 1, column);
        int fromColumn = run.Column;
        run.Floor = node.Floor;
        run.Column = node.Column;
        run.Version++;
        var result = new DelveMoveResult { Node = node.Type };

        switch (node.Type)
        {
            case DelveNodeType.Camp:
                run.Pending = DelvePending.Camp;
                break;
            case DelveNodeType.Treasure:
                if (!OfferRelics(run))
                    OfferRewards(run);
                break;
            default:
                await FightAsync(run, node, fromColumn, now, result);
                break;
        }

        await SaveAsync();
        if (result.Gold > 0 || result.Experience > 0 || result.RainbowCoins > 0)
            await PayAsync(run.PlayerId, result.Gold, result.Experience, result.RainbowCoins);
        result.Overview = await DescribeAsync(userId, now);
        return result;
    }

    public async Task<DelveOverview> ChooseRelicAsync(Guid userId, string key)
    {
        var run = await RequireActiveAsync(userId);
        if (run.Pending != DelvePending.Relic)
            throw new DelveException(DelveError.NothingPending, "There is nothing to choose");
        if (!run.Offer().Contains(key))
            throw new DelveException(DelveError.InvalidRelic, "This relic is not offered");

        var relics = run.Relics();
        relics.Add(key);
        run.SetRelics(relics);
        // Феникс действует сразу: павшие встают
        if (key == "phoenix")
            run.SetHp(run.Hp().ToDictionary(x => x.Key, x => x.Value <= 0 ? DelveRules.PhoenixHp : x.Value));
        run.SetOffer([]);
        run.Pending = DelvePending.None;
        OfferRewards(run);
        run.Version++;
        await SaveAsync();
        return await DescribeAsync(userId, Now);
    }

    // Привал: rest — лечение живых, revive — воскресить одного павшего (heroId)
    public async Task<DelveOverview> CampAsync(Guid userId, string action, Guid? heroId)
    {
        var run = await RequireActiveAsync(userId);
        if (run.Pending != DelvePending.Camp)
            throw new DelveException(DelveError.NothingPending, "You are not at a camp");

        var hp = run.Hp();
        if (action == "rest")
        {
            double heal = DelveRules.CampHeal + (run.Relics().Contains("ale") ? DelveRules.AleHeal : 0);
            hp = hp.ToDictionary(x => x.Key, x => x.Value > 0 ? Math.Min(1, x.Value + heal) : 0);
        }
        else if (action == "revive")
        {
            var fallen = heroId is { } id && hp.TryGetValue(id, out var value) && value <= 0 ? id
                : throw new DelveException(DelveError.NoFallen, "Choose a fallen hero");
            hp[fallen] = DelveRules.CampRevive;
        }
        else
        {
            throw new DelveException(DelveError.InvalidCampAction, "Unknown camp action");
        }

        run.SetHp(hp);
        run.Pending = DelvePending.None;
        OfferRewards(run);
        run.Version++;
        await SaveAsync();
        return await DescribeAsync(userId, Now);
    }

    // Взять награду index из предложенных; heroId — дворф для наград, которые надевают на выбранного
    public async Task<DelveOverview> ChooseRewardAsync(Guid userId, int index, Guid? heroId)
    {
        var run = await RequireActiveAsync(userId);
        if (run.Pending != DelvePending.Reward)
            throw new DelveException(DelveError.NothingPending, "There is nothing to choose");
        var rewards = run.Rewards();
        if (index < 0 || index >= rewards.Count)
            throw new DelveException(DelveError.InvalidReward, "This reward is not offered");
        var reward = rewards[index];

        var squad = run.Squad();
        Hero? target = null;
        if (reward.NeedsTarget)
        {
            target = squad.FirstOrDefault(h => h.Id == heroId)
                ?? throw new DelveException(DelveError.InvalidTarget, "Choose a dwarf for this reward");
            if (DelveRules.CannotApply(reward, target) is { } reason)
                throw new DelveException(DelveError.InvalidTarget, reason);
        }
        if (DelveRules.Apply(reward, squad, target) == null)
            throw new DelveException(DelveError.InvalidTarget, "Nobody can take this reward");

        run.SetSquad(squad);
        ClearRewards(run);
        await SaveAsync();
        return await DescribeAsync(userId, Now);
    }

    // Отказаться от награды узла: золото и опыт на аккаунт по уровню кампании и глубине
    public async Task<DelveDeclineResult> DeclineRewardAsync(Guid userId)
    {
        var run = await RequireActiveAsync(userId);
        if (run.Pending != DelvePending.Reward)
            throw new DelveException(DelveError.NothingPending, "There is nothing to choose");

        var progress = await players.GetProgressAsync(run.PlayerId);
        double stages = DelveRules.DeclineStagesFor(run.Floor);
        var result = new DelveDeclineResult
        {
            Gold = DailyQuests.GoldFor(stages, progress),
            Experience = DailyQuests.ExperienceFor(stages, progress)
        };
        run.GoldEarned += result.Gold;
        run.ExperienceEarned += result.Experience;
        ClearRewards(run);
        await SaveAsync();
        await PayAsync(run.PlayerId, result.Gold, result.Experience, 0);
        result.Overview = await DescribeAsync(userId, Now);
        return result;
    }

    // Новый порядок мест отряда: все дворфы забега, каждый по разу
    public async Task<DelveOverview> ReorderAsync(Guid userId, IReadOnlyList<Guid> heroIds)
    {
        var run = await RequireActiveAsync(userId);
        var squad = run.Squad();
        if (heroIds.Count != squad.Count || heroIds.Distinct().Count() != squad.Count || !squad.All(h => heroIds.Contains(h.Id)))
            throw new DelveException(DelveError.InvalidOrder, "List every dwarf of the squad once");

        foreach (var hero in squad)
            hero.PlaceInSquad = heroIds.ToList().IndexOf(hero.Id) + 1;
        run.SetSquad(squad.OrderBy(h => h.PlaceInSquad));
        run.Version++;
        await SaveAsync();
        return await DescribeAsync(userId, Now);
    }

    // Воскресить весь павший отряд за радужные монеты: раз за забег, только забег этого периода
    public async Task<DelveOverview> ReviveAsync(Guid userId)
    {
        var now = Now;
        var period = DelveRules.PeriodOf(now);
        var run = await db.DelveRuns.FirstOrDefaultAsync(r => r.UserId == userId && r.PeriodStart == period);
        if (run is not { Status: DelveRunStatus.Dead, Revived: false })
            throw new DelveException(DelveError.CannotRevive, "There is nothing to revive");

        int paid = await db.Players
            .Where(p => p.Id == run.PlayerId && p.RainbowCoins >= DelveRules.ReviveCost)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.RainbowCoins, p => p.RainbowCoins - DelveRules.ReviveCost));
        if (paid == 0)
            throw new DelveException(DelveError.NotEnoughRainbowCoins, $"Reviving costs {DelveRules.ReviveCost} rainbow coins");

        run.SetHp(run.Hp().ToDictionary(x => x.Key, _ => 1.0));
        run.Status = DelveRunStatus.Active;
        run.Pending = DelvePending.None;
        run.Revived = true;
        run.EndedAt = null;
        run.Version++;
        try
        {
            await SaveAsync();
        }
        catch (DelveException)
        {
            // Забег успел измениться — монеты возвращаются
            await PayAsync(run.PlayerId, 0, 0, DelveRules.ReviveCost);
            throw;
        }
        return await DescribeAsync(userId, now);
    }

    // Выйти из спуска: забег окончен, глубина засчитана
    public async Task<DelveOverview> RetreatAsync(Guid userId)
    {
        var now = Now;
        var run = await RequireActiveAsync(userId);
        run.Status = DelveRunStatus.Retreated;
        run.Pending = DelvePending.None;
        run.EndedAt = now;
        run.Version++;
        await SaveAsync();
        return await DescribeAsync(userId, now);
    }

    /* ===== Бой ===== */

    private async Task FightAsync(DelveRun run, DelveNode node, int fromColumn, DateTime now, DelveMoveResult result)
    {
        int maxStage = (await campaign.GetStageIdsAsync()).DefaultIfEmpty(1).Max();
        int stageId = DelveRules.StageFor(node, maxStage);
        var stage = await campaign.GetStageAsync(stageId)
            ?? throw new InvalidOperationException($"Campaign stage {stageId} not found");

        var world = BuildSquadWorld(run, node.Type is DelveNodeType.Elite or DelveNodeType.Boss, aliveOnly: true);
        var scenario = scenarios.OfType<CampaignScenario>().First();
        int before = world.Entities.Count;
        scenario.SpawnEnemies(world, stage);
        double hpFactor = node.Type switch { DelveNodeType.Elite => DelveRules.EliteHp, DelveNodeType.Boss => DelveRules.BossHp, _ => 1 };
        double factor = DelveRules.EnemyFactor(node, maxStage);
        foreach (var enemy in world.Entities.Skip(before))
        {
            var health = world.Health[enemy.Id];
            health.Max = Math.Max(1, (int)Math.Round(health.Max * hpFactor * factor));
            health.Current = health.Max;
            var attack = world.Attack[enemy.Id];
            attack.Damage = Math.Max(1, (int)Math.Round(attack.Damage * factor));
            attack.Base = attack.Damage;
            if (node.Type == DelveNodeType.Boss)
                world.Info[enemy.Id].IsBoss = true;
        }
        PreparedFight.AddArena(world, stage.Arena);

        var combat = PreparedFight.Run(engine, world, ((ICombatScenario)scenario).MaxRounds);
        result.Combat = combat;
        result.Background = stage.Arena?.Background ?? "lava";
        result.Won = combat.WinningTeam == 1;

        // HP после боя — доля от максимума с реликвиями; павшие — 0
        var hp = run.Hp();
        foreach (var entity in world.Entities)
        {
            var info = world.Info[entity.Id];
            if (world.Team[entity.Id].TeamId != 1 || info.HeroId is not { } heroId)
                continue;
            var health = world.Health[entity.Id];
            hp[heroId] = health.Max > 0 ? Math.Clamp((double)health.Current / health.Max, 0, 1) : 0;
        }

        if (result.Won == true)
        {
            if (run.Relics().Contains("bloodthirst"))
                hp = hp.ToDictionary(x => x.Key, x => x.Value > 0 ? Math.Min(1, x.Value + DelveRules.BloodthirstHeal) : 0);

            var progress = await players.GetProgressAsync(run.PlayerId);
            double stages = DelveRules.FightStagesFor(node);
            result.Gold = DailyQuests.GoldFor(stages, progress);
            result.Experience = DailyQuests.ExperienceFor(stages, progress);
            result.RainbowCoins = node.Type == DelveNodeType.Boss ? DelveRules.BossRainbowCoins : 0;
            run.GoldEarned += result.Gold;
            run.ExperienceEarned += result.Experience;
            run.RainbowCoinsEarned += result.RainbowCoins;
            run.SetHp(hp);

            // После элиты — сначала реликвия, награда узла — после неё
            if (node.Type != DelveNodeType.Elite || !OfferRelics(run))
                OfferRewards(run);
        }
        else
        {
            // Поражение — весь отряд пал (или кончились раунды): забег окончен, этаж не пройден.
            // Отряд стоит там, откуда шёл: после воскрешения путь выбирается заново
            run.SetHp(hp.ToDictionary(x => x.Key, _ => 0.0));
            run.Status = DelveRunStatus.Dead;
            run.EndedAt = now;
            run.Floor--;
            run.Column = fromColumn;
        }
    }

    // false — все реликвии уже взяты
    private static bool OfferRelics(DelveRun run)
    {
        var offer = DelveRules.RelicOffer(run.Seed, run.Floor, run.Relics());
        if (offer.Count == 0)
            return false;
        run.SetOffer(offer);
        run.Pending = DelvePending.Relic;
        return true;
    }

    private static void OfferRewards(DelveRun run)
    {
        var offer = DelveRules.RewardOffer(run.Seed, run.Floor, run.Squad());
        if (offer.Count == 0)
            return;
        run.SetRewards(offer);
        run.Pending = DelvePending.Reward;
    }

    private static void ClearRewards(DelveRun run)
    {
        run.SetRewards([]);
        run.Pending = DelvePending.None;
        run.Version++;
    }

    // Отряд забега с его HP и реликвиями. strongFoe — бой с элитой или боссом (истребитель гигантов).
    // aliveOnly — только живые (для боя); иначе все, павшие — с нулём HP (для показа)
    private World BuildSquadWorld(DelveRun run, bool strongFoe, bool aliveOnly)
    {
        var world = new World();
        var hp = run.Hp();
        var heroes = run.Squad().Where(h => !aliveOnly || hp.GetValueOrDefault(h.Id, 1) > 0).ToList();
        entityFactory.CreateSquad(world, heroes, teamId: 1);

        var relics = run.Relics();
        double attack = 1 + (relics.Contains("rage") ? DelveRules.RageAttack : 0) + (strongFoe && relics.Contains("giantslayer") ? DelveRules.GiantslayerAttack : 0);
        double maxHp = 1 + (relics.Contains("stoneskin") ? DelveRules.StoneskinHp : 0);
        foreach (var entity in world.Entities)
        {
            var info = world.Info[entity.Id];
            var health = world.Health[entity.Id];
            health.Max = (int)Math.Round(health.Max * maxHp);
            double fraction = hp.GetValueOrDefault(info.HeroId ?? Guid.Empty, 1);
            health.Current = fraction <= 0 ? 0 : Math.Max(1, (int)Math.Round(health.Max * fraction));

            var atk = world.Attack[entity.Id];
            atk.Damage = (int)Math.Round(atk.Damage * attack);
            atk.Base = atk.Damage;

            if (relics.Contains("swiftboots"))
                world.Speed[entity.Id].Value += DelveRules.SwiftSpeed;
            if (relics.Contains("spark"))
            {
                var energy = world.Energy[entity.Id];
                energy.Current = Math.Min(energy.Max, energy.Current + (int)Math.Round(energy.Max * DelveRules.SparkEnergy));
            }
        }
        return world;
    }

    /* ===== Ответ ===== */

    private async Task<DelveOverview> DescribeAsync(Guid userId, DateTime now)
    {
        var period = DelveRules.PeriodOf(now);
        var run = await db.DelveRuns.AsNoTracking()
            .Where(r => r.UserId == userId && r.PeriodStart == period)
            .FirstOrDefaultAsync();

        var best = await db.DelveRuns.AsNoTracking()
            .Where(r => r.PeriodStart == period && r.Floor > 0)
            .OrderByDescending(r => r.Floor)
            .Take(DelveRules.LeaderboardSize)
            .Select(r => new { r.UserId, r.Floor })
            .ToListAsync();
        var ids = best.Select(b => b.UserId).ToList();
        var names = await db.Users.AsNoTracking().Where(u => ids.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Username);

        long coins = await db.Players.Where(p => p.Id == (run != null ? run.PlayerId : Guid.Empty)).Select(p => p.RainbowCoins).FirstOrDefaultAsync();

        return new DelveOverview
        {
            ServerTime = now,
            PeriodStart = period,
            NextRunAt = period.AddDays(DelveRules.PeriodDays),
            Columns = DelveRules.Columns,
            BossEvery = DelveRules.BossEvery,
            ReviveCost = DelveRules.ReviveCost,
            Run = run == null ? null : DescribeRun(run, coins),
            CanStart = run == null,
            BestFloor = run?.Floor ?? 0,
            Leaderboard = best.Select((b, i) => new DelveLeaderInfo
            {
                Place = i + 1,
                UserId = b.UserId,
                Username = names.GetValueOrDefault(b.UserId, string.Empty),
                Floor = b.Floor,
                IsMe = b.UserId == userId
            }).ToList(),
            Relics = DelveRules.Relics.Select(r => new DelveRelicInfo { Key = r.Key, Rare = r.Rare }).ToList()
        };
    }

    private DelveRunInfo DescribeRun(DelveRun run, long rainbowCoins)
    {
        var hp = run.Hp();
        var squad = run.Squad();
        var world = BuildSquadWorld(run, false, aliveOnly: false);
        var units = CombatSnapshot.Units(world).Where(u => u.HeroId != null).ToDictionary(u => u.HeroId!.Value);

        var dwarves = squad.Select((h, i) =>
        {
            int? spellId = h.SpellIds.Count > 0 ? h.SpellIds[0] : null;
            var spell = spellId is { } id ? SpellRegistry.All.GetValueOrDefault(id) : null;
            return new DelveDwarfInfo
            {
                HeroId = h.Id,
                Name = h.Name,
                Element = h.Element,
                Level = h.Level,
                Position = h.PlaceInSquad ?? i + 1,
                Hp = Math.Clamp(hp.GetValueOrDefault(h.Id, 1), 0, 1),
                Unit = units.GetValueOrDefault(h.Id) ?? new CombatUnitInfo(),
                SpellId = spellId,
                SpellName = spell?.Name,
                SpellLevel = spell?.Level ?? 0,
                ItemKey = h.Equipment.FirstOrDefault()?.ItemKey,
                Passive = h.Passives.Count > 0 ? h.Passives[0] : null,
                PassiveLevel = h.Passives.Count > 0 ? h.PassiveLevel : 0,
                PassiveValues = h.Passives.Count > 0 ? PassiveBook.Values(h.Passives[0], h.PassiveLevel) : null,
                AuraEffect = h.AuraEffect,
                AuraLevel = h.AuraEffect != null ? h.AuraLevel : 0
            };
        }).ToList();

        var rewards = run.Pending == DelvePending.Reward
            ? run.Rewards().Select(r =>
            {
                var spell = r.SpellId is { } id ? SpellRegistry.All.GetValueOrDefault(id) : null;
                var item = r.Kind == DelveRewardKind.Item ? EquipmentCatalog.Find(r.Key ?? string.Empty) : null;
                return new DelveRewardInfo
                {
                    Kind = r.Kind,
                    Key = r.Key,
                    Level = r.Level,
                    SpellId = r.SpellId,
                    SpellName = spell?.Name,
                    Element = r.Element,
                    Rarity = item?.Rarity,
                    PassiveValues = r.Kind == DelveRewardKind.Passive && Enum.TryParse<HeroPassive>(r.Key, out var passive)
                        ? PassiveBook.Values(passive, r.Level)
                        : null,
                    NeedsTarget = r.NeedsTarget,
                    Fits = r.NeedsTarget ? squad.Where(h => DelveRules.CannotApply(r, h) == null).Select(h => h.Id).ToList() : []
                };
            }).ToList()
            : [];

        // Карта видна, пока по ней можно идти: у павшего отряда — на случай воскрешения
        bool onMap = run.Status == DelveRunStatus.Active || run.Status == DelveRunStatus.Dead && !run.Revived;
        return new DelveRunInfo
        {
            Id = run.Id,
            Status = run.Status,
            Floor = run.Floor,
            Column = run.Column,
            Pending = run.Pending,
            Offer = run.Offer(),
            Rewards = rewards,
            Relics = run.Relics(),
            Dwarves = dwarves,
            Next = onMap ? DelveRules.FloorOf(run.Seed, run.Floor + 1).Select(ToInfo).ToList() : [],
            Peek = onMap ? ToInfo(DelveRules.NodeAt(run.Seed, run.Floor + 2, run.Column)) : null,
            GoldEarned = run.GoldEarned,
            ExperienceEarned = run.ExperienceEarned,
            RainbowCoinsEarned = run.RainbowCoinsEarned,
            Revived = run.Revived,
            CanRevive = run.Status == DelveRunStatus.Dead && !run.Revived && rainbowCoins >= DelveRules.ReviveCost
        };
    }

    private static DelveNodeInfo ToInfo(DelveNode node) => new() { Floor = node.Floor, Column = node.Column, Type = node.Type };

    /* ===== Вспомогательное ===== */

    private async Task<DelveRun> RequireActiveAsync(Guid userId) =>
        await db.DelveRuns.FirstOrDefaultAsync(r => r.UserId == userId && r.Status == DelveRunStatus.Active)
            ?? throw new DelveException(DelveError.NoActiveRun, "You have no active delve");

    private async Task PayAsync(Guid playerId, long gold, long experience, int rainbowCoins) =>
        await db.Players.Where(p => p.Id == playerId).ExecuteUpdateAsync(s => s
            .SetProperty(p => p.Gold, p => p.Gold + gold)
            .SetProperty(p => p.Experience, p => p.Experience + experience)
            .SetProperty(p => p.RainbowCoins, p => p.RainbowCoins + rainbowCoins));

    private async Task SaveAsync()
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            db.ChangeTracker.Clear();
            throw new DelveException(DelveError.Changed, "The delve has just changed, try again");
        }
    }

    private async Task<Guid> PlayerIdAsync(Guid userId) =>
        await db.Users.Where(u => u.Id == userId).Select(u => u.PlayerId).FirstOrDefaultAsync();
}
