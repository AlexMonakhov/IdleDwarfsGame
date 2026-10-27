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
        var templates = HeroSeedData.Heroes.ToDictionary(h => h.Id);
        foreach (var definition in GuildBosses.All)
        {
            var damages = new System.Collections.Generic.List<int>();
            for (int seed = 0; seed < 20; seed++)
            {
                var scenario = new GuildBossScenario(factory, templates[definition.TemplateHeroId], definition, definition.MaxHp);
                var world = new World();
                factory.CreateSquad(world, StarterSquad.Create(Guid.NewGuid(), new Random(seed)), teamId: 1);
                await scenario.PopulateEnemiesAsync(world, definition.Level);
                var ctx = new CombatEngine(new Random(seed)).Run(world);
                damages.Add(definition.MaxHp - scenario.BossHpLeft);
            }
            output.WriteLine($"{definition.Tier}: avg {damages.Average():F0}, min {damages.Min()}, max {damages.Max()}, hits to kill ~{definition.MaxHp / damages.Average():F0}");
        }
    }
}
