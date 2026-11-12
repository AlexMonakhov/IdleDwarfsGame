using System.Linq;
using System.Text.Json.Serialization;

// Откуда урон или лечение: обычный удар, спелл (и его статусы), аура, пассивка героя, арена (без бойца-источника)
public enum CombatStatKind
{
    Attack,
    Skill,
    Aura,
    Passive,
    Environment
}

// Кто и как стоит за уроном или лечением статуса. У слоистых статусов источник — у каждого слоя свой
public readonly record struct StatOrigin(int SourceId, CombatStatKind Kind)
{
    public static readonly StatOrigin Environment = new(-1, CombatStatKind.Environment);
}

// Доля числа из лога, приписанная одному источнику (тик статуса, где слои от разных бойцов)
public readonly record struct StatShare(StatOrigin Origin, int Amount);

// Итог боя по бойцу: нанесённый урон и лечение по источникам, полученный урон и лечение, добивания
public class CombatUnitStats
{
    public int EntityId { get; set; }
    public int Team { get; set; }

    public int DamageDealt { get; set; }
    public int AttackDamage { get; set; }
    public int SkillDamage { get; set; }
    public int AuraDamage { get; set; }
    public int PassiveDamage { get; set; }

    public int HealingDone { get; set; }
    public int SkillHealing { get; set; }
    public int AuraHealing { get; set; }
    public int PassiveHealing { get; set; }

    public int DamageTaken { get; set; }
    public int HealingReceived { get; set; }
    public int Kills { get; set; }

    internal void AddDamage(CombatStatKind kind, int amount)
    {
        DamageDealt += amount;
        switch (kind)
        {
            case CombatStatKind.Attack: AttackDamage += amount; break;
            case CombatStatKind.Skill: SkillDamage += amount; break;
            case CombatStatKind.Aura: AuraDamage += amount; break;
            case CombatStatKind.Passive: PassiveDamage += amount; break;
        }
    }

    internal void AddHealing(CombatStatKind kind, int amount)
    {
        HealingDone += amount;
        switch (kind)
        {
            case CombatStatKind.Skill: SkillHealing += amount; break;
            case CombatStatKind.Aura: AuraHealing += amount; break;
            case CombatStatKind.Passive: PassiveHealing += amount; break;
        }
    }
}

// Считает статистику по логам по мере их записи (CombatContext.AddLog).
// Удары, спеллы и пассивки знают атакующего из лога; тики статусов приносят разбивку по слоям (CombatLogEntry.Shares)
public class CombatStatsTracker
{
    private readonly Dictionary<int, CombatUnitStats> _units = new();
    // Кто последним ранил бойца: ему засчитывается добивание
    private readonly Dictionary<int, int> _lastHitBy = new();

    public void Record(CombatLogEntry entry)
    {
        if (entry.EventType == CombatEventType.Death)
        {
            if (_lastHitBy.TryGetValue(entry.TargetId, out int killer) && Unit(killer, null) is { } stats
                && stats.Team != entry.TargetTeam)
                stats.Kills++;
            return;
        }

        if (entry.TargetId <= 0) return;

        if (entry.Damage > 0)
        {
            Unit(entry.TargetId, entry.TargetTeam)!.DamageTaken += entry.Damage;
            var shares = SharesOf(entry, entry.Damage, entry.EventType == CombatEventType.Attack ? CombatStatKind.Attack : null);
            foreach (var share in shares)
            {
                if (share.Origin.SourceId <= 0 || share.Amount <= 0) continue;
                var source = Unit(share.Origin.SourceId, null);
                // Плата своим HP (Blood Price) и урон по своим — не нанесённый урон
                if (source == null || source.Team == entry.TargetTeam) continue;
                source.AddDamage(share.Origin.Kind, share.Amount);
            }
            if (shares.Where(s => s.Origin.SourceId > 0).OrderByDescending(s => s.Amount).FirstOrDefault() is { Amount: > 0 } main)
                _lastHitBy[entry.TargetId] = main.Origin.SourceId;
        }

        if (entry.Heal > 0)
        {
            Unit(entry.TargetId, entry.TargetTeam)!.HealingReceived += entry.Heal;
            foreach (var share in SharesOf(entry, entry.Heal, null))
            {
                if (share.Origin.SourceId <= 0 || share.Amount <= 0) continue;
                Unit(share.Origin.SourceId, null)?.AddHealing(share.Origin.Kind, share.Amount);
            }
        }
    }

    // Бойцы боя заранее: команда нужна, чтобы отличать урон по врагам от урона по своим
    public void Register(int entityId, int team) => Unit(entityId, team);

    public List<CombatUnitStats> Build() => _units.Values.OrderBy(u => u.Team).ThenBy(u => u.EntityId).ToList();

    private static List<StatShare> SharesOf(CombatLogEntry entry, int amount, CombatStatKind? kind)
    {
        if (entry.Shares != null) return entry.Shares;
        if (entry.AttackerId <= 0) return new();
        var resolved = kind ?? (entry.Phase == "Passive" ? CombatStatKind.Passive : CombatStatKind.Skill);
        return new() { new StatShare(new StatOrigin(entry.AttackerId, resolved), amount) };
    }

    private CombatUnitStats? Unit(int entityId, int? team)
    {
        if (_units.TryGetValue(entityId, out var stats)) return stats;
        if (team is not { } knownTeam) return null;
        return _units[entityId] = new CombatUnitStats { EntityId = entityId, Team = knownTeam };
    }

    // Делит amount между источниками пропорционально их весу; остаток от округления — самому весомому
    public static List<StatShare> Split(int amount, IEnumerable<(StatOrigin Origin, int Weight)> weights)
    {
        var grouped = weights
            .Where(w => w.Weight > 0)
            .GroupBy(w => w.Origin)
            .Select(g => (Origin: g.Key, Weight: g.Sum(w => w.Weight)))
            .OrderByDescending(w => w.Weight)
            .ToList();
        int total = grouped.Sum(w => w.Weight);
        if (total <= 0 || amount <= 0) return new();

        var shares = grouped.Select(w => new StatShare(w.Origin, (int)((long)amount * w.Weight / total))).ToList();
        int rest = amount - shares.Sum(s => s.Amount);
        shares[0] = shares[0] with { Amount = shares[0].Amount + rest };
        return shares;
    }
}
