using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

public class TmpGuildBalance(ITestOutputHelper output)
{
    [Fact]
    public async Task Measure()
    {
        var factory = new EntityFactory();
        var templates = GuildBossSeedData.Heroes.ToDictionary(h => h.Id);
        foreach (var definition in GuildBosses.All)
        {
            foreach (var variant in definition.Variants)
            {
                var arena = GuildBosses.ArenaOf(variant);
                var damages = new System.Collections.Generic.List<int>();
                for (int seed = 0; seed < 20; seed++)
                {
                    var scenario = new GuildBossScenario(factory, templates[variant.TemplateHeroId], definition, definition.MaxHp);
                    var world = new World();
                    factory.CreateSquad(world, StarterSquad.Create(Guid.NewGuid(), new Random(seed)), teamId: 1);
                    await scenario.PopulateEnemiesAsync(world, definition.Level);
                    world.AddGlobalEffect(ArenaStatusEffect.FromArena(arena)!);
                    new CombatEngine(new Random(seed)).Run(world, GuildBosses.FightRounds);
                    damages.Add(definition.MaxHp - scenario.BossHpLeft);
                }
                output.WriteLine($"{definition.Tier} / {templates[variant.TemplateHeroId].Name} @ {arena.Name}: avg {damages.Average():F0}, min {damages.Min()}, max {damages.Max()}, hits to kill ~{definition.MaxHp / damages.Average():F0}");
            }
        }
    }
}
