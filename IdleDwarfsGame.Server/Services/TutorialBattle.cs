// Обучающий бой: первая попытка 1-го уровня кампании. Бой «запечён» — всегда одинаковый (постоянный сид движка),
// поэтому фронт может остановить его на нужных событиях и объяснить механики (docs/tutorial-plan.md).
// Новичок приходит в него без своих дворфов (их даёт стартовый сундук — награда этого боя), и за него бьётся
// наставник — огненный дворф 4-го ранга с Огненным шаром 3-го уровня: заклинание стоит столько же энергии, сколько
// даёт удар, так что на своём 2-м ходу он кастует его по всем врагам и поджигает их. Если отряд у игрока уже есть
// (аккаунт до обучения), наставник встаёт на последнее место.
// Наставник — копия только для этого боя, в БД его нет. Что бой выигран и каст случился, проверяют TutorialBattleTests
public static class TutorialBattle
{
    public const int StageId = 1;
    public const int Seed = 20261116;

    // Место наставника: один — впереди, с отрядом — последнее, подальше от врагов (бьют первую позицию)
    public const int SoloSlot = 1;
    public const int MentorSlot = 5;

    public static readonly Guid MentorId = Guid.Parse("7e7e7e7e-0000-4000-8000-000000000001");

    // Обучающий бой — только пока 1-й уровень не пройден. Бой с наёмником друга идёт как обычный
    public static bool AppliesTo(int stageId, bool alreadyCleared) => stageId == StageId && !alreadyCleared;

    public static Hero Mentor(Guid playerId, int slot = MentorSlot) => new()
    {
        Id = MentorId,
        PlayerId = playerId,
        Name = "Brokk",
        UnitType = "Dwarf",
        Profession = HeroProfession.Axeman,
        Element = HeroElement.Fire,
        Level = 4,
        PlaceInSquad = slot,
        // Статы не от ранга, а под бой: с честными ×9.75 наставник убивал бы гоблинов обычным ударом
        // раньше, чем дойдёт до заклинания. Один против трёх гоблинов он выигрывает за несколько раундов
        MaxHp = 2400,
        Attack = 300,
        // Быстрее всех на поле: в каждом раунде ходит первым, поэтому его 2-й ход — начало 2-го раунда
        Speed = 12,
        MaxEnergy = 100,
        StartEnergy = 0,
        EnergyPerAttack = 20,
        SpellIds = [SpellRegistry.IdOf(FireballFirstLevelId, 3)],
        Passives = [],
        Immunities = []
    };

    private const int FireballFirstLevelId = 1;

    // Отряд на бой: наставник один или на своём месте в отряде игрока (дворф с этого места в бою не участвует)
    public static List<Hero> Squad(Guid playerId, IReadOnlyCollection<Hero> squad) => squad.Count == 0
        ? [Mentor(playerId, SoloSlot)]
        : squad
            .Select((hero, index) => (Hero: hero, Place: hero.PlaceInSquad ?? index + 1))
            .Where(x => x.Place != MentorSlot)
            .Select(x => x.Hero)
            .Append(Mentor(playerId))
            .ToList();

    public static ICombatEngine Engine() => new CombatEngine(new Random(Seed));
}
