public class Entity
{
    public int Id { get; }

    public Entity(int id)
    {
        Id = id;
    }
}

public enum CombatEventType
{
    CombatStart,
    RoundStart,
    Attack,       // базовая атака: AttackerId -> TargetId, урон в Damage
    SkillCast,    // каст спелла: только анимация каста, урон приходит отдельными Damage с тем же ActionId
    Damage,       // урон от спелла по одной цели
    StatusApplied,
    StatusTick,
    StatusExpired,
    Heal,
    Stunned,      // персонаж пропускает ход
    Immune,       // эффект не наложился из-за иммунитета
    Death,
    CombatEnd
}

public enum DamageType
{
    Physical,
    Magical,
    Fire,
    Frost,
    Lightning,
    Poison,
    Bleed,
    True
}

public class StatusEffectLogData
{
    public string EffectId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Duration { get; set; }
    public int Stacks { get; set; }
    public int? TickDamage { get; set; }
    public DamageType? DamageType { get; set; }
    public string? Description { get; set; }
}

public class CombatLogEntry
{
    // Порядковый номер записи и номер действия. Все записи одного действия
    // (каст + попадания по целям) имеют одинаковый ActionId — фронт проигрывает их одной группой.
    public int Sequence { get; set; }
    public int ActionId { get; set; }

    public int AttackerId { get; set; } = -1;
    public int TargetId { get; set; } = -1;

    public int AttackerTeam { get; set; }
    public int TargetTeam { get; set; }

    // Существующее поле, оставляем для совместимости со старой логикой.
    public string ActionType { get; set; } = "";

    // Новые поля для структурированного лога, нужные UI/анимациям.
    public int Round { get; set; }
    public CombatEventType EventType { get; set; } = CombatEventType.Attack;
    public string Phase { get; set; } = "Action";
    public int? SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public DamageType DamageType { get; set; } = DamageType.Physical;
    public int Damage { get; set; }
    public int Heal { get; set; }
    public int TargetRemainingHp { get; set; }
    public int? TargetRemainingHpAfter { get; set; }

    // Энергия актёра после действия (для полоски энергии на фронте).
    public int? AttackerEnergy { get; set; }

    public bool IsCritical { get; set; }

    // Для CombatEnd.
    public int? WinningTeam { get; set; }

    // Для UI удобно иметь уже подготовленный текст, но не использовать его как единственный источник данных.
    public string? Text { get; set; }

    // Список эффектов, которые были применены или активны в момент этого лога.
    public List<StatusEffectLogData> AppliedEffects { get; set; } = new();
    public List<StatusEffectLogData> ActiveEffects { get; set; } = new();
}

// Снимок участника боя на старте — фронт строит по нему сцену, дальше всё восстанавливается из логов.
public class CombatUnitInfo
{
    public int EntityId { get; set; }
    public Guid? HeroId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string UnitType { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Team { get; set; }
    public int Position { get; set; }
    public int MaxHp { get; set; }
    public int Hp { get; set; }
    public int Attack { get; set; }
    public int Speed { get; set; }
    public int MaxEnergy { get; set; }
    public int Energy { get; set; }
    public List<string> Spells { get; set; } = new();
    public List<string> Immunities { get; set; } = new();
    public string? Aura { get; set; }
}

public class CombatResult
{
    public List<CombatUnitInfo> Units { get; set; } = new();
    public List<CombatLogEntry> Logs { get; set; } = new();
    public int WinningTeam { get; set; }
    public int Rounds { get; set; }
    public RewardEntity? Rewards { get; set; }
}

public interface ICombatSystem
{
    void Execute(World world, CombatContext context);
}

public class CombatContext
{
    public List<CombatLogEntry> Logs = new();
    public List<Entity> CurrentRoundOrder = new();
    public int CurrentActor = -1;
    public bool CombatEnded;
    public bool Win;
    public int WinningTeam;
    public int CurrentRound { get; set; } = 0;
    public bool IsNewRound { get; set; } = false;
    public int MaxRounds { get; set; } = 15;

    // Текущее действие, к которому привязываются логи.
    public int CurrentActionId { get; private set; }

    public int NextAction() => ++CurrentActionId;

    public CombatLogEntry AddLog(CombatLogEntry entry)
    {
        entry.Sequence = Logs.Count;
        entry.Round = CurrentRound;
        if (entry.ActionId == 0) entry.ActionId = CurrentActionId;
        Logs.Add(entry);
        return entry;
    }
}

public interface IGlobalEffect
{
    string Name { get; }
    TriggerPhase Trigger { get; } // OnRoundStart или OnTurnStart

    // Применяет эффект ко всему миру
    void Apply(World world, CombatContext context);
}

public class HealthComponent
{
    public int Current;
    public int Max;
}

public class AttackComponent
{
    public int Damage;
}

public class SpeedComponent
{
    public int Value;
}

public class TeamComponent
{
    public int TeamId; // 1 или 2
}

// Описательные данные юнита (не влияют на механику, нужны для логов и фронта)
public class UnitInfoComponent
{
    public Guid? HeroId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string UnitType { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public int Position { get; set; }
    public bool IsDead { get; set; }
}

public class SpellComponent
{
    public List<int> SpellIds { get; set; } = new(); // ID спелла персонажа
}

public class StatusEffectComponent
{
    public List<IStatusEffect> ActiveEffects { get; set; } = new(); // Активные эффекты на персонажа
}

public class ImmunityComponent
{
    // Будем хранить названия эффектов (например, "Burn", "Stun")
    public List<string> ImmuneTo { get; set; } = new();
}

public class EnergyComponent
{
    public int Current { get; set; } = 0;
    public int Max { get; set; } = 100;

    // Базовый прирост за удар. Предметы смогут менять это значение!
    public int GainPerAttack { get; set; } = 20;
}

public class TurnStateComponent
{
    public bool CanAct { get; set; } = true;         // Может ли ходить (снимется станом)
    public bool ActionCompleted { get; set; } = false; // Сделал ли уже действие (кастанул спелл)
}

public enum AuraTarget { Allies, Enemies, All }
public enum TriggerPhase { OnTurnStart, OnRoundStart }

public class AuraComponent
{
    public AuraTarget TargetType { get; set; }
    public TriggerPhase Trigger { get; set; }

    // Эффект создаётся заново для каждой цели, иначе один экземпляр делил бы RemainingTurns между всеми.
    public string EffectName { get; set; } = string.Empty;
    public int Power { get; set; }
    public int Duration { get; set; } = 1;

    public IStatusEffect CreateEffect() => StatusEffectFactory.Create(EffectName, Power, Duration);
}
