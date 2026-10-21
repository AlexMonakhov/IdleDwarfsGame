using System;
using System.Collections.Generic;
using System.Linq;

public class SpellSystem : ICombatSystem
{
    private readonly Dictionary<int, ISpell> _spellRegistry;
    private static Random _random = new();

    public SpellSystem(Dictionary<int, ISpell> spellRegistry)
    {
        _spellRegistry = spellRegistry;
    }

    public void Execute(World world, CombatContext context)
    {
        int attacker = context.CurrentActor;
        if (attacker == -1 || !world.TurnState.ContainsKey(attacker)) return;

        var state = world.TurnState[attacker];
        if (!state.CanAct || state.ActionCompleted) return;   

        // Проверяем наличие компонентов
        if (!world.Spells.ContainsKey(attacker) || !world.Energy.ContainsKey(attacker)) return;

        var energy = world.Energy[attacker];
        var availableSpellIds = world.Spells[attacker].SpellIds;

        // Находим все заклинания, на которые ХВАТАЕТ энергии
        var affordableSpells = availableSpellIds
            .Select(id => _spellRegistry.GetValueOrDefault(id))
            .Where(spell => spell != null && energy.Current >= spell.EnergyCost)
            .ToList();

        // Если хватает маны хотя бы на один спелл
        if (affordableSpells.Any())
        {
            // Выбираем случайный из доступных (или можно выбирать самый дорогой)
            var spellToCast = affordableSpells[_random.Next(affordableSpells.Count)];

            // Кастуем!
            spellToCast.Execute(world, attacker, context);

            // Списываем энергию
            energy.Current -= spellToCast.EnergyCost;
            state.ActionCompleted = true;
        }
        // Если энергии мало, метод завершается, и ход переходит в DamageSystem
    }
}
