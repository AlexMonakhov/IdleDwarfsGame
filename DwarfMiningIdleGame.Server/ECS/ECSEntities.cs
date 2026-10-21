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
    RoundStart,
    SkillCast,
    Damage,
    StatusApplied,
    StatusTick,
    StatusExpired,
    Heal,
    Death
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
    public int AttackerId { get; set; }
    public int TargetId { get; set; }

    public int AttackerTeam { get; set; }
    public int TargetTeam { get; set; }

    // Существующее поле, оставляем для совместимости со старой логикой.
    public string ActionType { get; set; } = "";

    // Новые поля для структурированного лога, нужные UI/анимациям.
    public int Round { get; set; }
    public CombatEventType EventType { get; set; } = CombatEventType.Damage;
    public string Phase { get; set; } = "Action";
    public int? SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public DamageType DamageType { get; set; } = DamageType.Physical;
    public int Damage { get; set; }
    public int TargetRemainingHp { get; set; }
    public int? TargetRemainingHpAfter { get; set; }

    public bool IsCritical { get; set; }

    // Для UI удобно иметь уже подготовленный текст, но не использовать его как единственный источник данных.
    public string? Text { get; set; }

    // Список эффектов, которые были применены или активны в момент этого лога.
    public List<StatusEffectLogData> AppliedEffects { get; set; } = new();
    public List<StatusEffectLogData> ActiveEffects { get; set; } = new();
}

public class CombatResult
{
    public List<CombatLogEntry> Logs { get; set; } = new();
    public int WinningTeam { get; set; }
}

public interface ICombatSystem
{
    void Execute(World world, CombatContext context);
}

public class CombatContext
{
    public List<CombatLogEntry> Logs = new();
    public List<Entity> CurrentRoundOrder = new();
    public int CurrentActor;
    public bool CombatEnded;
    public bool Win;
    public int CurrentRound { get; set; } = 0;
    public bool IsNewRound { get; set; } = false;
    public int MaxRounds { get; set; } = 15;
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
    public IStatusEffect EffectToApply { get; set; } // Бафф, который навешивается (например, +10 к Атаке)
}


