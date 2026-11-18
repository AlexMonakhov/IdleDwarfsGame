// Бонус стихий отряда в бою (World.SquadBonuses, по команде) и то, что ему нужно помнить между ходами
public class SquadBonusState
{
    public SquadBonusState(int teamId, SquadBonus bonus, IReadOnlyList<HeroElement> elements)
    {
        TeamId = teamId;
        Bonus = bonus;
        Elements = elements;
    }

    public int TeamId { get; }
    public SquadBonus Bonus { get; }
    // Стихии дворфов отряда (без Basic) — для призмы
    public IReadOnlyList<HeroElement> Elements { get; }

    public SquadBonusId Id => Bonus.Id;
    public int Tier => Bonus.Tier;
    public bool Is(SquadBonusId id) => Bonus.Id == id;

    // Сколько раз бонус сработал за бой — для тестов и прогонов баланса
    public int Triggers { get; set; }

    // ⚡ «Перегрузка»: касты отряда
    public int SpellCasts { get; set; }
    // 🌿 III: воскрешение уже потрачено
    public bool ReviveUsed { get; set; }
    // ❄🌿: кто уже побывал в коконе
    public HashSet<int> CocoonUsed { get; } = new();
    // Раунд последнего срабатывания по цели (взрыв «Льда и пламени», вмерзание)
    public Dictionary<int, int> LastProc { get; } = new();
    // ❄💎: действие, в котором уже было лечение
    public int LastHealAction { get; set; } = -1;
    // 🌑💎: сейчас ночь (нечётный раунд)
    public bool Night { get; set; }
    // 🌈: стихия, которая резонирует в этом раунде
    public HeroElement? Resonance { get; set; }
    // Текущий раунд (ставит SquadBonusSystem) — для откатов по раундам
    public int Round { get; set; }

    // Отложенное до конца хода (HealthSystem): лечение, коконы, воскрешения — им нужен лог, а урон наносится без контекста
    public List<(int Target, int Amount)> PendingHeals { get; } = new();
    public List<int> PendingCocoons { get; } = new();
    public List<int> PendingRevives { get; } = new();
    // Отложенные удары бонуса: от кого и сколько (ответ «Мученика»)
    public List<(int From, int Amount)> PendingStrikes { get; } = new();

    // Бонус сейчас сам действует (взрыв, перенос) — его наложения не усиливаются и не вызывают новых реакций
    public bool Reacting { get; set; }
}
