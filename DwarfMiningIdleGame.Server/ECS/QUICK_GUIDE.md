# БЫСТРАЯ ШПАРГАЛКА: СПЕЛЛЫ И ЭФФЕКТЫ

## Структура ID спеллов

```
Fireball:  ID 1-3   (уровни 1-3)
Lightning: ID 4-6   (уровни 1-3)
IceShard:  ID 7-9   (следующий диапазон)
Incinerate: ID 10-12 (если добавить)
```

## Как использовать в коде

### Добавить спелл герою

```csharp
world.Spells[entity.Id] = new SpellComponent
{
    SpellIds = new List<int> { 1, 5 }  // Fireball Lvl1, Lightning Lvl2
};
```

### Добавить статус-эффект

```csharp
var burnEffect = new BurnEffect(turns: 3, damagePerTurn: 10);
burnEffect.ApplyEffect(world, targetId, context);
```

### Создать новый спелл

1. Создайте класс:
```csharp
public class NewSpell : BaseSpell
{
    public NewSpell(int level = 1) : base(
        id: level + X,  // Выберите диапазон ID
        name: "NewSpell",
        level: level)
    {
    }

    protected override int GetDamage(World world, int casterId)
    {
        int baseDamage = world.Attack[casterId].Damage;
        return Level switch
        {
            1 => (int)(baseDamage * 1.2),
            2 => (int)(baseDamage * 0.8),
            3 => (int)(baseDamage * 0.6),
            _ => baseDamage
        };
    }
}
```

2. Зарегистрируйте в SpellSystem:
```csharp
RegisterSpell(new NewSpell(1));
RegisterSpell(new NewSpell(2));
RegisterSpell(new NewSpell(3));
```

### Создать новый эффект

```csharp
public class CustomEffect : IStatusEffect
{
    public string EffectName => "Custom";
    public int RemainingTurns { get; set; }

    public CustomEffect(int turns = 2)
    {
        RemainingTurns = turns;
    }

    public void ApplyEffect(World world, int targetId, CombatContext context)
    {
        if (!world.StatusEffects.ContainsKey(targetId))
            world.StatusEffects[targetId] = new StatusEffectComponent();
        
        world.StatusEffects[targetId].ActiveEffects.Add(this);
    }

    public void OnTurnStart(World world, int targetId, CombatContext context)
    {
        // Логика эффекта
        RemainingTurns--;
    }

    public void OnTurnEnd(World world, int targetId, CombatContext context)
    {
        // Опционально
    }
}
```

## Уровни спеллов

- **Уровень 1**: 1 цель (концентрированный урон)
- **Уровень 2**: 3 случайные цели (разделенный урон)
- **Уровень 3**: Все враги (слабый AOE урон)

## Логика выполнения хода

1. **TurnOrderSystem** - определяет активного персонажа
2. **StatusEffectSystem** - применяет эффекты в начале хода
3. **SpellSystem** - кастует спелл (если нет стана)
4. **DamageSystem** - обычная атака (если спелл не был)
5. **HealthSystem** - проверка живых персонажей

## Текущая конфигурация героев

Герои спавнятся с:
- Fireball Уровня 1 (ID 1)
- Lightning Уровня 2 (ID 5)

Измените это в `EntityFactory.CreatePlayer()`.
