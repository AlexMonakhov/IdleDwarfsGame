// Что произошло за один каст: по кому попали, кого убили, какие статусы наложили, сколько лечения ушло в избыток.
// Каст собирает это сам (BaseSpell.Hit, HealTarget), а эффекты предметов (SpellBoundEffect.AfterCast) читают после каста.
// Пока каст идёт, он открыт (Open) и лежит в CombatContext.CurrentCast; вызванные предметом спеллы в него не пишут
public class CastScope
{
    public World World { get; }
    public int CasterId { get; }
    public CombatContext Context { get; }
    public BaseSpell Spell { get; }
    public SpellModifier? Modifier { get; }

    // Каст ещё идёт — попадания и лечение пишутся в него
    public bool Open { get; set; } = true;

    // Враги, по которым каст ударил (без лечимых союзников)
    public List<int> Targets { get; } = new();
    // Кого добили попадания этого каста
    public HashSet<int> Killed { get; } = new();
    // Статусы на юнитах до каста и статусы, которые каст наложил (без иммунитетов)
    public Dictionary<int, HashSet<string>> StatusesBefore { get; } = new();
    public Dictionary<int, HashSet<string>> StatusesApplied { get; } = new();

    // Лечение сверх максимума HP
    public int Overheal { get; set; }
    // Сколько HP кастер заплатил за каст (Blood Price)
    public int HpPaid { get; set; }
    // Оставшийся урон снятых кастом проклятий (Soul Harvest)
    public int CurseConsumed { get; set; }
    // Самый низкий процент HP среди союзников до каста
    public int LowestAllyHpPercent { get; }

    public CastScope(World world, int casterId, CombatContext context, BaseSpell spell, SpellModifier? modifier)
    {
        World = world;
        CasterId = casterId;
        Context = context;
        Spell = spell;
        Modifier = modifier;

        foreach (var entity in world.Entities.Where(e => world.IsAlive(e.Id)))
        {
            if (world.StatusEffects.TryGetValue(entity.Id, out var component) && component.ActiveEffects.Count > 0)
                StatusesBefore[entity.Id] = component.ActiveEffects.Select(e => e.EffectName).ToHashSet();
        }

        var allies = world.GetAliveAllies(casterId);
        LowestAllyHpPercent = allies.Count == 0 ? 100 : allies.Min(a => world.Health[a.Id].Current * 100 / Math.Max(1, world.Health[a.Id].Max));
    }

    public bool HadStatus(int unitId, string status) =>
        StatusesBefore.TryGetValue(unitId, out var names) && names.Contains(status);

    public bool Applied(int unitId, string status) =>
        StatusesApplied.TryGetValue(unitId, out var names) && names.Contains(status);

    public void RecordApplied(int unitId, string status)
    {
        if (!StatusesApplied.TryGetValue(unitId, out var names))
            StatusesApplied[unitId] = names = new HashSet<string>();
        names.Add(status);
    }
}
