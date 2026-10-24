using System;
using System.Collections.Generic;
using System.Linq;

public class SpellSystem : ICombatSystem
{
    private readonly IReadOnlyDictionary<int, ISpell> _spellRegistry;
    private readonly Random _random;

    public SpellSystem(IReadOnlyDictionary<int, ISpell> spellRegistry, Random random)
    {
        _spellRegistry = spellRegistry;
        _random = random;
    }

    public void Execute(World world, CombatContext context)
    {
        int attacker = context.CurrentActor;
        if (attacker == -1 || !world.TurnState.ContainsKey(attacker) || !world.IsAlive(attacker)) return;

        var state = world.TurnState[attacker];
        if (!state.CanAct || state.ActionCompleted) return;

        // Проверяем наличие компонентов
        if (!world.Spells.ContainsKey(attacker) || !world.Energy.ContainsKey(attacker)) return;
        if (world.GetAliveEnemies(attacker).Count == 0) return;

        var energy = world.Energy[attacker];

        // Находим все заклинания, на которые хватает энергии
        var affordableSpells = world.Spells[attacker].SpellIds
            .Select(id => _spellRegistry.GetValueOrDefault(id))
            .OfType<ISpell>()
            .Where(spell => energy.Current >= spell.EnergyCost)
            .ToList();

        // Если энергии не хватает ни на один спелл — ход уйдёт в DamageSystem (базовая атака)
        if (affordableSpells.Count == 0) return;

        // Выбираем случайный из доступных (тут можно добавить умный выбор)
        var spellToCast = affordableSpells[_random.Next(affordableSpells.Count)];

        // Сначала списываем энергию, чтобы лог каста показывал актуальное значение
        energy.Current -= spellToCast.EnergyCost;
        spellToCast.Execute(world, attacker, context);
        state.ActionCompleted = true;
    }
}
