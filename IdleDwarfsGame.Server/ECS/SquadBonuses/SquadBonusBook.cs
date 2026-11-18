// Бонусы стихий отряда. Отряд получает ровно один бонус — по составу стихий его дворфов (Basic стихией не считается):
//   5 одной стихии        → моно-бонус III
//   4 + 1                 → моно-бонус II
//   3 + 2                 → парный бонус этих двух стихий (3 + 1 + 1 при этом не считается)
//   3 + 1 + 1             → моно-бонус I
//   5 разных стихий       → «Призма» (все пять — настоящие стихии, ни одного Basic)
//   всё остальное         → бонуса нет
// Сила по убыванию: моно III > моно II > пара > моно I > призма. Механики — SquadBonuses, правила и прогоны — docs/squad-bonuses.md
public enum SquadBonusId
{
    // Моно-бонусы (3 / 4 / 5 дворфов одной стихии)
    Fire, Frost, Lightning, Nature, Dark, Radiance, Blood,

    // Пары 3 + 2. Имя — стихии в порядке HeroElement
    FireFrost, FireLightning, FireNature, FireDark, FireRadiance, FireBlood,
    FrostLightning, FrostNature, FrostDark, FrostRadiance, FrostBlood,
    LightningNature, LightningDark, LightningRadiance, LightningBlood,
    NatureDark, NatureRadiance, NatureBlood,
    DarkRadiance, DarkBlood,
    RadianceBlood,

    // Пять разных стихий
    Prism
}

public enum SquadBonusShape { Mono, Pair, Prism }

// Tier — уровень моно-бонуса 1..3; у пары и призмы всегда 1
public record SquadBonus(SquadBonusId Id, SquadBonusShape Shape, int Tier)
{
    public override string ToString() => Shape == SquadBonusShape.Mono ? $"{Id} {Tier}" : Id.ToString();
}

public static class SquadBonusBook
{
    public const int SquadSize = 5;

    // Бонус отряда с такими стихиями; null — бонуса нет
    public static SquadBonus? Resolve(IEnumerable<HeroElement> elements)
    {
        var all = elements.ToList();
        var counts = all
            .Where(e => e != HeroElement.Basic)
            .GroupBy(e => e)
            .Select(g => (Element: g.Key, Count: g.Count()))
            .OrderByDescending(c => c.Count)
            .ThenBy(c => c.Element)
            .ToList();
        if (counts.Count == 0) return null;

        var (top, count) = counts[0];
        if (count >= 5) return Mono(top, 3);
        if (count == 4) return Mono(top, 2);
        if (count == 3)
            return counts.Count > 1 && counts[1].Count >= 2 ? Pair(top, counts[1].Element) : Mono(top, 1);

        if (counts.Count >= SquadSize && counts.All(c => c.Count == 1))
            return new SquadBonus(SquadBonusId.Prism, SquadBonusShape.Prism, 1);
        return null;
    }

    public static SquadBonus Mono(HeroElement element, int tier) =>
        new(Enum.Parse<SquadBonusId>(element.ToString()), SquadBonusShape.Mono, Math.Clamp(tier, 1, 3));

    public static SquadBonus Pair(HeroElement a, HeroElement b)
    {
        var (first, second) = a < b ? (a, b) : (b, a);
        return new SquadBonus(Enum.Parse<SquadBonusId>($"{first}{second}"), SquadBonusShape.Pair, 1);
    }

    // Стихии бонуса: у моно — одна, у пары — две, у призмы — ни одной
    public static IReadOnlyList<HeroElement> ElementsOf(SquadBonusId id)
    {
        if (id == SquadBonusId.Prism) return [];
        var name = id.ToString();
        return Enum.GetValues<HeroElement>()
            .Where(e => e != HeroElement.Basic && name.Contains(e.ToString()))
            .OrderBy(e => name.IndexOf(e.ToString(), StringComparison.Ordinal))
            .ToList();
    }

    // Все бонусы: 7 моно × 3 уровня, 21 пара и призма — для прогонов баланса и каталога
    public static IEnumerable<SquadBonus> All()
    {
        var elements = ElementSpellBook.Elements.OrderBy(e => e).ToList();
        foreach (var element in elements)
            for (int tier = 1; tier <= 3; tier++)
                yield return Mono(element, tier);
        for (int i = 0; i < elements.Count; i++)
            for (int j = i + 1; j < elements.Count; j++)
                yield return Pair(elements[i], elements[j]);
        yield return new SquadBonus(SquadBonusId.Prism, SquadBonusShape.Prism, 1);
    }

    // Прибавка бонуса к боевой мощи отряда, % — по прогонам SquadBonusSimulation (docs/squad-bonuses.md):
    // средняя прибавка урона против босса и против толпы
    public static int PowerPercent(SquadBonus bonus) => bonus.Id switch
    {
        SquadBonusId.Prism => 14,
        _ when bonus.Shape == SquadBonusShape.Pair => 29,
        _ => bonus.Tier switch { 1 => 20, 2 => 39, _ => 61 }
    };

    // ===================== Числа бонусов =====================
    // Массивы — по уровням моно-бонуса I / II / III. Все механики моно-бонуса работают с I уровня, выше — только сильнее

    public static int ByTier(int[] values, int tier) => values[Math.Clamp(tier, 1, values.Length) - 1];

    // 🔥 «Неугасимое пламя»: огненный урон отряда (заклинания и горение) сильнее; погибший горящий враг передаёт часть горения соседу
    public static readonly int[] FireDamagePercent = [50, 80, 90];
    // Сколько оставшегося горения погибшего врага переходит на следующего
    public static readonly int[] FireSpreadPercent = [60, 80, 100];

    // ❄ «Вечная мерзлота»: мороз отряда сильнее; набравший порог мороза враг вмерзает на ход (раз в FreezeCooldown раундов).
    // По обледеневшим и оглушённым — больше урона
    public static readonly int[] FrostSlowPercent = [80, 140, 200];
    public static readonly int[] FrostFreezeThreshold = [40, 30, 25];
    public const int FrostFreezeCooldown = 3;
    // Урон холодом (заклинания) сильнее
    public static readonly int[] FrostDamagePercent = [30, 60, 70];
    public static readonly int[] FrostFrozenDamagePercent = [10, 25, 40];

    // ⚡ «Перегрузка»: каждый N-й каст отряда выпускает бесплатную молнию по случайному врагу (% атаки кастера).
    // Молния ещё и оставляет шок (% атаки кастера)
    public static readonly int[] LightningEvery = [2, 2, 1];
    public static readonly int[] LightningDamagePercent = [200, 400, 275];
    public static readonly int[] LightningShockPercent = [20, 40, 60];
    public const int LightningShockTurns = 2;

    // 🌿 «Корни мира»: в начале раунда отряд лечится на % макс. HP, яд отряда сильнее; раз за бой первый
    // погибший дворф встаёт с NatureRevivePercent HP
    public static readonly int[] NatureHealPercent = [6, 10, 15];
    public static readonly int[] NaturePoisonPercent = [110, 180, 225];
    public static readonly int[] NatureRevivePercent = [30, 50, 90];

    // 🌑 «Пакт тени»: выжженная у врагов энергия уходит союзнику с меньшей энергией; тёмный урон сильнее
    public static readonly int[] DarkStealPercent = [100, 100, 100];
    public static readonly int[] DarkDamagePercent = [30, 40, 50];

    // 💎 «Ореол»: лечение сверх максимума становится щитом (до % макс. HP); урон светом сильнее
    public static readonly int[] RadianceShieldPercent = [100, 100, 100];
    public static readonly int[] RadianceShieldCap = [20, 35, 60];
    public static readonly int[] RadianceDamagePercent = [80, 120, 140];

    // 🩸 «Кровный долг»: урон отряда сильнее всегда и ещё +1% за каждые N% потерянного HP отряда (до потолка); тики кровотечения лечат самого раненого
    public static readonly int[] BloodMissingPerPercent = [3, 2, 2];
    // Постоянная прибавка урона — ярость от ран идёт сверху
    public static readonly int[] BloodBaseDamagePercent = [10, 20, 35];
    public static readonly int[] BloodDamageCap = [30, 60, 100];
    public static readonly int[] BloodBleedLifesteal = [20, 45, 70];

    // ===== Пары =====

    // 🔥❄ «Лёд и пламя»: горение и мороз на одной цели взрываются (оба снимаются): цели — % её макс. HP,
    // но не больше % атаки, остальным врагам — FireFrostSplashPercent от этого. Не чаще одного взрыва за раунд на отряд
    public const int FireFrostMaxHpPercent = 6;
    public const int FireFrostAttackCap = 190;
    public const int FireFrostSplashPercent = 60;

    // 🔥⚡ «Плазма»: урон молнией по горящей цели добавляет огненный удар силой текущего горения
    public const int FireLightningBurnPercent = 950;

    // 🔥🌿 «Лесной пожар»: горение на отравленных сильнее и перекидывается на соседа
    public const int FireNatureTickPercent = 130;
    public const int FireNatureSplashPercent = 130;

    // 🔥🌑 «Адское пламя»: горение отряда сильнее (на проклятых — вдвое больше прибавка), каждый тик выжигает энергию
    public const int FireDarkTickPercent = 225;
    public const int FireDarkEnergy = 10;

    // 🔥💎 «Очищающий огонь»: тики горения отряда лечат самого раненого дворфа; огонь и свет по горящим сильнее
    public const int FireRadianceHealPercent = 70;
    public const int FireRadianceDamagePercent = 70;

    // 🔥🩸 «Кипящая кровь»: дворфы ниже 50% HP бьют сильнее, их удары поджигают
    public const int FireBloodThreshold = 50;
    public const int FireBloodDamagePercent = 90;
    public const int FireBloodBurnPercent = 40;
    public const int FireBloodBurnTurns = 2;

    // ❄⚡ «Сверхпроводник»: молния по замороженным сильнее; по обездвиженным ещё и выжигает энергию
    public const int FrostLightningDamagePercent = 250;
    public const int FrostLightningEnergy = 50;

    // ❄🌿 «Ледяной кокон»: дворф ниже порога HP раз за бой вмерзает в кокон — лечение и щит; яд на замороженных сильнее
    public const int FrostNatureThreshold = 40;
    public const int FrostNaturePoisonPercent = 275;
    public const int FrostNatureHealPercent = 50;
    public const int FrostNatureShieldPercent = 50;

    // ❄🌑 «Полярная ночь»: враги под морозом теряют энергию в конце хода; тёмный урон по ним сильнее
    public const int FrostDarkEnergyPercentOfSlow = 225;
    public const int FrostDarkDamagePercent = 150;

    // ❄💎 «Холодный свет»: враги под морозом бьют отряд слабее; мороз отряда лечит самого раненого (раз за действие);
    // свет по замороженным сильнее
    public const int FrostRadianceDamageReduction = 45;
    public const int FrostRadianceHealPercent = 8;
    public const int FrostRadianceDamagePercent = 170;

    // ❄🩸 «Кровавый лёд»: кровотечение на замороженных накладывается в полную силу (без затухания) и тикает сильнее
    public const int FrostBloodTickPercent = 200;

    // ⚡🌿 «Гроза над лесом»: тики шока лечат; тики яда с шансом бьют молнией
    public const int LightningNatureShockHealPercent = 100;
    public const int LightningNatureZapChance = 70;
    public const int LightningNatureZapPercent = 450;

    // ⚡🌑 «Чёрная молния»: молния по проклятому сразу срабатывает тиком проклятия и бьёт сильнее
    public const int LightningDarkCursePercent = 600;
    public const int LightningDarkDamagePercent = 100;

    // ⚡💎 «Небесный суд»: в начале раунда метка на двух самых здоровых врагов
    public const int LightningRadianceMarkPercent = 140;
    public const int LightningRadianceMarks = 2;

    // ⚡🩸 «Адреналин»: дворф, потерявший разом ≥ порога макс. HP, получает энергию
    public const int LightningBloodThreshold = 4;
    public const int LightningBloodEnergy = 80;

    // 🌿🌑 «Гниль»: яд на проклятых не истекает — каждый слой держится до NatureDarkHolds лишних ходов
    public const int NatureDarkHolds = 2;
    public const int NatureDarkTickPercent = 300;

    // 🌿💎 «Благодатная почва»: лечение сверх максимума уходит самому раненому, реген отряда сильнее
    // и каждый его тик даёт дворфу энергию
    public const int NatureRadianceRegenPercent = 110;
    public const int NatureRadianceRegenEnergy = 45;

    // 🌿🩸 «Симбиоз»: яд и кровотечение отряда сильнее, часть их урона лечит самого раненого
    public const int NatureBloodLifesteal = 90;
    public const int NatureBloodTickPercent = 130;

    // 🌑💎 «Равновесие»: нечётные раунды — ночь (урон больше), чётные — день (лечение)
    public const int DarkRadianceNightPercent = 50;
    public const int DarkRadianceDayHealPercent = 15;

    // 🌑🩸 «Ритуал»: проклятие и кровотечение отряда сильнее; тик проклятия по кровоточащему добавляет стак кровотечения;
    // смерть врага переносит оставшийся урон проклятия и кровотечения на следующего (не больше % атаки)
    public const int DarkBloodBleedPercent = 100;
    public const int DarkBloodTickPercent = 110;
    public const int DarkBloodTransferCap = 500;

    // 💎🩸 «Мученик»: часть потерянного дворфом HP лечит самого раненого союзника, ещё часть светом бьёт первого врага
    public const int RadianceBloodPercent = 70;
    public const int RadianceBloodRetaliatePercent = 100;

    // 🌈 «Призма»: в начале раунда резонирует стихия случайного дворфа: попадания заклинаний накладывают её статус
    // (аура 1-го уровня, PrismPowerPercent её силы)
    public const int PrismPowerPercent = 350;
}
