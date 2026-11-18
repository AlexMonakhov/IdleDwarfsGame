using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

// Прогоны бонусов стихий отряда (SquadBonusBook) на настоящем CombatEngine.
// Отряды — «пустые» дворфы 4-го ранга 1-го уровня: заклинание своей стихии 3-го уровня (семейство случайно),
// аура стихии 1-го уровня, без пассивок, иммунитетов и предметов.
// Два боя по Rounds раундов: один босс и пять мобов, у обоих огромное HP (никто не умирает) и средняя атака.
// Каждый бой идёт дважды на одном сиде — с бонусом и без: разница — чистый вклад бонуса.
// Отчёт: dotnet test --filter SquadBonusSimulation.Report --logger "console;verbosity=detailed"
// (SQUAD_BONUS_REPORT=путь — ещё и записать markdown в файл, SQUAD_BONUS_SEEDS — сидов на вариант, по умолчанию 30;
// итоги — docs/squad-bonuses.md)
public class SquadBonusSimulation(ITestOutputHelper output)
{
    private static readonly EntityFactory Factory = new();
    public const int Rounds = 15;
    public const int Rank = 4;

    private static readonly HeroElement[] Elements = ElementSpellBook.Elements.OrderBy(e => e).ToArray();

    public enum Opponent { Boss, Mobs }

    // ===== Состав отряда под бонус =====

    // Стихии пяти слотов (спереди назад). variant меняет добивку у моно и ориентацию у пары, набор стихий у призмы
    public static HeroElement[] Composition(SquadBonus bonus, int variant)
    {
        var elements = SquadBonusBook.ElementsOf(bonus.Id);
        switch (bonus.Shape)
        {
            case SquadBonusShape.Mono:
            {
                var main = elements[0];
                var others = Elements.Where(e => e != main).ToArray();
                var first = others[variant % others.Length];
                var second = others[(variant + 3) % others.Length];
                return bonus.Tier switch
                {
                    3 => [main, main, main, main, main],
                    2 => [main, main, main, main, first],
                    _ => [main, main, main, first, second]
                };
            }
            case SquadBonusShape.Pair:
            {
                var (three, two) = variant % 2 == 0 ? (elements[0], elements[1]) : (elements[1], elements[0]);
                return [three, three, three, two, two];
            }
            default:
                return Enumerable.Range(0, 5).Select(i => Elements[(variant * 2 + i) % Elements.Length]).ToArray();
        }
    }

    public static int Variants(SquadBonus bonus) => bonus.Shape switch
    {
        SquadBonusShape.Mono => bonus.Tier == 3 ? 1 : 2,
        SquadBonusShape.Pair => 2,
        _ => 3
    };

    // Пустые дворфы под состав: профессии как у эталонного отряда, заклинание стихии 3-го уровня, аура 1-го уровня
    public static List<Hero> Squad(HeroElement[] composition, int seed)
    {
        var random = new Random(seed);
        var playerId = Guid.NewGuid();
        return composition.Select((element, index) =>
        {
            var profession = ProfessionBook.SquadOrder[index];
            var stats = ProfessionBook.Of(profession);
            var hero = HeroGenerator.Create(playerId, Rank, element.ToString(), random);
            hero.Profession = profession;
            hero.MaxHp = HeroGenerator.ScaleToRank(stats.MaxHp, Rank);
            hero.Attack = HeroGenerator.ScaleToRank(stats.Attack, Rank);
            hero.Speed = stats.Speed;
            hero.UpgradeLevel = 1;
            hero.PlaceInSquad = index + 1;
            hero.Passives = [];
            hero.Immunities = [];
            return hero;
        }).ToList();
    }

    // ===== Противники =====

    public const int EnemyHp = 50_000_000;
    public const int BossAttack = 1500;
    public const int MobAttack = 330;

    public static List<Hero> Enemies(Opponent opponent) => opponent switch
    {
        Opponent.Boss => [Enemy("Boss", BossAttack, [14], ["Stun"])], // Meteor 2, иммунитет к оглушению, как у боссов
        _ => Enumerable.Range(0, 5).Select(i => Enemy($"Mob {i + 1}", MobAttack, i % 2 == 0 ? [1] : [], [])).ToList() // Fireball 1 у каждого второго
    };

    private static Hero Enemy(string name, int attack, List<int> spells, List<string> immunities) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        UnitType = "Golem",
        Level = 4,
        MaxHp = EnemyHp,
        Attack = attack,
        Speed = 5,
        MaxEnergy = 100,
        StartEnergy = 0,
        EnergyPerAttack = 20,
        SpellIds = spells,
        Immunities = immunities
    };

    // ===== Бой =====

    public record Fight(long Damage, long BonusDamage, long Healing, int Deaths, int Triggers);

    public static Fight Run(List<Hero> squad, Opponent opponent, int seed, bool bonus)
    {
        var world = new World();
        Factory.CreateSquad(world, squad, teamId: 1);
        if (!bonus) world.SquadBonuses.Clear();
        foreach (var (enemy, index) in Enemies(opponent).Select((e, i) => (e, i)))
            Factory.CreateUnit(world, enemy, teamId: 2, position: index + 1);

        var context = new CombatEngine(new Random(seed)).Run(world, Rounds);
        var team = world.Entities.Where(e => world.Team[e.Id].TeamId == 1).Select(e => e.Id).ToList();
        return new Fight(
            Damage: context.Logs.Where(l => l.TargetTeam == 2 && l.Damage > 0).Sum(l => (long)l.Damage),
            BonusDamage: context.Logs.Where(l => l.TargetTeam == 2 && l.Damage > 0 && l.Phase == SquadBonuses.Phase).Sum(l => (long)l.Damage),
            Healing: context.Logs.Where(l => l.TargetTeam == 1 && l.Heal > 0).Sum(l => (long)l.Heal),
            Deaths: team.Count(id => !world.IsAlive(id)),
            Triggers: world.SquadBonuses.GetValueOrDefault(1)?.Triggers ?? 0);
    }

    public record Result(SquadBonus Bonus, Opponent Opponent, double With, double Without, double BonusDamage,
        double DeathsWith, double DeathsWithout, double HealWith, double HealWithout, double Triggers)
    {
        public double Delta => Without > 0 ? (With - Without) / Without * 100 : 0;
    }

    public static Result Measure(SquadBonus bonus, Opponent opponent, int seeds)
    {
        var withBonus = new List<Fight>();
        var without = new List<Fight>();
        for (int variant = 0; variant < Variants(bonus); variant++)
        {
            var composition = Composition(bonus, variant);
            for (int seed = 0; seed < seeds; seed++)
            {
                var squad = Squad(composition, seed * 31 + variant);
                withBonus.Add(Run(squad, opponent, seed, bonus: true));
                without.Add(Run(squad, opponent, seed, bonus: false));
            }
        }
        return new Result(bonus, opponent,
            withBonus.Average(f => f.Damage), without.Average(f => f.Damage), withBonus.Average(f => f.BonusDamage),
            withBonus.Average(f => f.Deaths), without.Average(f => f.Deaths),
            withBonus.Average(f => f.Healing), without.Average(f => f.Healing), withBonus.Average(f => f.Triggers));
    }

    // ===== Проверки =====

    [Fact]
    public void EveryComposition_ResolvesToItsBonus()
    {
        foreach (var bonus in SquadBonusBook.All())
            for (int variant = 0; variant < Variants(bonus); variant++)
                Assert.Equal(bonus, SquadBonusBook.Resolve(Composition(bonus, variant)));
    }

    // Каждый бонус хоть раз срабатывает в своих боях — механика не мёртвая
    [Fact]
    public void EveryBonus_Triggers()
    {
        var silent = SquadBonusBook.All()
            .Where(bonus => Enum.GetValues<Opponent>().All(opponent => Measure(bonus, opponent, seeds: 3).Triggers == 0))
            .ToList();
        Assert.Empty(silent);
    }

    [Fact]
    public void Report()
    {
        int seeds = int.TryParse(Environment.GetEnvironmentVariable("SQUAD_BONUS_SEEDS"), out int n) ? n : 30;
        var results = SquadBonusBook.All()
            .Select(bonus => (Bonus: bonus, Boss: Measure(bonus, Opponent.Boss, seeds), Mobs: Measure(bonus, Opponent.Mobs, seeds)))
            .ToList();

        var report = new StringBuilder();
        report.AppendLine($"Пустые дворфы {Rank}-го ранга, {Rounds} раундов, {seeds} сидов на вариант состава. Δ — прибавка урона от бонуса на тех же сидах.");
        report.AppendLine();
        report.AppendLine("| Бонус | Состав | Босс: урон | Босс: Δ | Мобы: урон | Мобы: Δ | Средняя Δ | Урон бонуса | Смерти с / без | Лечение с / без | Срабатываний |");
        report.AppendLine("|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|");
        foreach (var (bonus, boss, mobs) in results)
        {
            double average = (boss.Delta + mobs.Delta) / 2;
            double bonusShare = (boss.BonusDamage + mobs.BonusDamage) / Math.Max(1, boss.With + mobs.With) * 100;
            report.AppendLine($"| {Label(bonus)} | {Shape(bonus)} | {boss.With / 1000:N0}k | {boss.Delta:+0.0;-0.0}% | {mobs.With / 1000:N0}k | {mobs.Delta:+0.0;-0.0}% " +
                              $"| **{average:+0.0;-0.0}%** | {bonusShare:0.0}% " +
                              $"| {(boss.DeathsWith + mobs.DeathsWith) / 2:0.0} / {(boss.DeathsWithout + mobs.DeathsWithout) / 2:0.0} " +
                              $"| {(boss.HealWith + mobs.HealWith) / 2000:N0}k / {(boss.HealWithout + mobs.HealWithout) / 2000:N0}k " +
                              $"| {(boss.Triggers + mobs.Triggers) / 2:N0} |");
        }

        report.AppendLine();
        report.AppendLine("Средняя Δ по группам:");
        report.AppendLine();
        report.AppendLine("| Группа | Средняя Δ | Мин | Макс |");
        report.AppendLine("|---|---:|---:|---:|");
        foreach (var group in results.GroupBy(r => Shape(r.Bonus)).OrderBy(g => g.Key))
        {
            var deltas = group.Select(r => (r.Boss.Delta + r.Mobs.Delta) / 2).ToList();
            report.AppendLine($"| {group.Key} | {deltas.Average():+0.0;-0.0}% | {deltas.Min():+0.0;-0.0}% | {deltas.Max():+0.0;-0.0}% |");
        }

        output.WriteLine(report.ToString());
        if (Environment.GetEnvironmentVariable("SQUAD_BONUS_REPORT") is { Length: > 0 } path)
            File.WriteAllText(path, report.ToString());
    }

    private static string Label(SquadBonus bonus) => bonus.Shape == SquadBonusShape.Mono ? $"{bonus.Id} {new string('I', bonus.Tier)}" : bonus.Id.ToString();

    private static string Shape(SquadBonus bonus) => bonus.Shape switch
    {
        SquadBonusShape.Mono => bonus.Tier switch { 3 => "5 — моно III", 2 => "4+1 — моно II", _ => "3+1+1 — моно I" },
        SquadBonusShape.Pair => "3+2 — пара",
        _ => "1×5 — призма"
    };
}
