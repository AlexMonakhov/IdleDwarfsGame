using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

public class CombatPowerTests
{
    private readonly ITestOutputHelper _output;

    public CombatPowerTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static readonly EntityFactory Factory = new();

    private static Hero MakeHero(int hp = 100, int attack = 10, int speed = 5) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test",
        MaxHp = hp,
        Attack = attack,
        Speed = speed
    };

    private static CombatPowerBreakdown PowerOf(Hero hero, int level = 1)
    {
        var world = new World();
        return CombatPower.Of(world, Factory.CreateUnit(world, hero, teamId: 1, position: 1, level));
    }

    [Fact]
    public void Breakdown_SumsToTotal()
    {
        var heroes = HeroSeedData.Heroes.Concat(CampaignSeedData.Heroes);
        foreach (var hero in heroes)
        {
            foreach (int level in new[] { 1, 3 })
            {
                var power = PowerOf(hero, level);
                Assert.True(power.Total > 0, hero.Name);
                Assert.Equal(power.Total, power.Stats + power.Level + power.Skills + power.Passives);
            }
        }
    }

    [Fact]
    public void Stats_MorePowerForMoreHpAttackAndSpeed()
    {
        int basePower = PowerOf(MakeHero()).Total;

        Assert.True(PowerOf(MakeHero(hp: 120)).Total > basePower);
        Assert.True(PowerOf(MakeHero(attack: 12)).Total > basePower);
        Assert.True(PowerOf(MakeHero(speed: 8)).Total > basePower);
    }

    [Fact]
    public void Level_OnlyScaledUnitsGetLevelPower()
    {
        // Статы, которые масштабируются уровнем 3 (×1.44) без округления
        var hero = MakeHero(hp: 100, attack: 25);

        var unscaled = PowerOf(hero);
        var scaled = PowerOf(hero, level: 3);

        Assert.Equal(0, unscaled.Level);
        Assert.True(scaled.Level > 0);
        Assert.Equal(unscaled.Stats, scaled.Stats);
    }

    [Fact]
    public void SkillsAndPassives_AddPower()
    {
        var plain = PowerOf(MakeHero());

        var caster = MakeHero();
        caster.SpellIds = [3]; // Fireball 3
        var withSkill = PowerOf(caster);

        var guardian = MakeHero();
        guardian.Immunities = ["Stun"];
        guardian.AuraEffect = "Regen";
        guardian.AuraTarget = AuraTarget.Allies;
        guardian.AuraPower = 4;
        var withPassives = PowerOf(guardian);

        Assert.Equal(0, plain.Skills);
        Assert.Equal(0, plain.Passives);
        Assert.True(withSkill.Skills > 0);
        Assert.True(withPassives.Passives > 0);
        Assert.Equal(plain.Total, withSkill.Total - withSkill.Skills);
        Assert.Equal(plain.Total, withPassives.Total - withPassives.Passives);
    }

    [Fact]
    public void Skills_StrongerSpellLevelGivesMorePower()
    {
        int SkillPower(int spellId)
        {
            var hero = MakeHero();
            hero.SpellIds = [spellId];
            return PowerOf(hero).Skills;
        }

        Assert.True(SkillPower(3) > SkillPower(1)); // Fireball 3 > Fireball 1
        Assert.True(SkillPower(6) > SkillPower(4)); // Lightning 3 > Lightning 1
        Assert.True(SkillPower(9) > SkillPower(7)); // Healing Rune 3 > Healing Rune 1
    }

    [Fact]
    public void Forecast_SquadPowerIsSumOfHeroes()
    {
        var squad = DemoSquad();
        var forecast = CombatPower.Forecast(() => TowerWorld(squad, floor: 2));

        int expected = squad.Sum(hero => CombatPower.OfHero(Factory, hero).Total);
        Assert.Equal(expected, forecast.PlayerPower);
        Assert.InRange(forecast.WinChance, 0, 1);
    }

    [Fact]
    public void Forecast_IsDeterministic()
    {
        var squad = DemoSquad();

        var first = CombatPower.Forecast(() => TowerWorld(squad, floor: 3));
        var second = CombatPower.Forecast(() => TowerWorld(squad, floor: 3));

        Assert.Equal(first.WinChance, second.WinChance);
    }

    // Сама мощь — грубый, но честный ориентир: явный перевес по мощи означает победу, явное отставание — поражение.
    // Шанс в прогнозе — доля побед в настоящих боях, поэтому проверяем мощь против него
    [Fact]
    public void Power_RoughlyPredictsOutcome()
    {
        foreach (var (name, build) in Matchups())
        {
            var forecast = CombatPower.Forecast(build);
            double ratio = (double)forecast.PlayerPower / forecast.EnemyPower;
            _output.WriteLine($"{name,-20} power {forecast.PlayerPower,5} vs {forecast.EnemyPower,5}  ratio {ratio,5:0.00}  win chance {forecast.WinChance,4:0.00}");

            if (ratio >= 1.2) Assert.True(forecast.WinChance >= 0.9, $"{name}: ratio {ratio:0.00}, chance {forecast.WinChance}");
            if (ratio <= 0.85) Assert.True(forecast.WinChance <= 0.1, $"{name}: ratio {ratio:0.00}, chance {forecast.WinChance}");
        }
    }

    // Отряды разной силы против всех уровней кампании и этажей башни
    private static IEnumerable<(string Name, Func<World> Build)> Matchups()
    {
        var repository = new CampaignTests.SeedCampaignRepository();
        var squads = new (string Name, List<Hero> Heroes)[]
        {
            ("demo", DemoSquad()),
            ("demo-3", DemoSquad().Take(3).ToList()),
            ("starter", StarterSquad.Create(Guid.NewGuid(), new Random(1)))
        };

        foreach (var (squadName, heroes) in squads)
        {
            foreach (var stageId in Enumerable.Range(1, 10))
            {
                var stage = repository.Stage(stageId);
                yield return ($"{squadName} / stage {stageId}", () =>
                {
                    var world = new World();
                    Factory.CreateSquad(world, heroes, teamId: 1);
                    new CampaignScenario(repository, Factory).SpawnEnemies(world, stage);
                    world.AddGlobalEffect(ArenaStatusEffect.FromArena(stage.Arena)!);
                    return world;
                });
            }

            foreach (var floor in Enumerable.Range(1, 8))
            {
                yield return ($"{squadName} / floor {floor}", () => TowerWorld(heroes, floor));
            }
        }
    }

    private static World TowerWorld(List<Hero> squad, int floor)
    {
        var world = new World();
        Factory.CreateSquad(world, squad, teamId: 1);
        Factory.CreateSquad(world, TowerEnemies(), teamId: 2, level: floor);
        world.AddGlobalEffect(new LavaArenaEffect());
        return world;
    }

    private static List<Hero> DemoSquad() => HeroSeedData.Heroes
        .Where(h => h.PlayerId == HeroSeedData.DemoPlayerId && h.PlaceInSquad != null)
        .OrderBy(h => h.PlaceInSquad)
        .ToList();

    private static List<Hero> TowerEnemies() => HeroSeedData.Heroes
        .Where(h => h.PlayerId == HeroSeedData.EnemyPlayerId)
        .OrderBy(h => h.PlaceInSquad)
        .ToList();
}
