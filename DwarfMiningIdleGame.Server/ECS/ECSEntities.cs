public class Entity
{
    public int Id { get; }

    public Entity(int id)
    {
        Id = id;
    }
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

public class CombatLogEntry
{
    public int AttackerId { get; set; }
    public int TargetId { get; set; }

    public int AttackerTeam { get; set; }
    public int TargetTeam { get; set; }

    public string ActionType { get; set; } = "";
    public int Damage { get; set; }
    public int TargetRemainingHp { get; set; }

    public bool IsCritical { get; set; }
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
}