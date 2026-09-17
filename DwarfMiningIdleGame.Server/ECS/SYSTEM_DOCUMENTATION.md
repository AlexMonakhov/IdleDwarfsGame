/**
 * СИСТЕМА СПЕЛЛОВ И СТАТУС-ЭФФЕКТОВ
 * 
 * Архитектура:
 * 1. Спеллы (Spells/) - реализуют интерфейс ISpell
 * 2. Статус-эффекты (StatusEffects/) - реализуют интерфейс IStatusEffect
 * 3. Системы (Systems) - реализуют ICombatSystem
 * 4. World - хранит компоненты для каждой сущности
 * 
 * ========================================
 * КАК ДОБАВИТЬ НОВЫЙ СПЕЛЛ
 * ========================================
 * 
 * 1. Создайте класс, наследующий BaseSpell:
 * 
 *    public class IceShard : BaseSpell
 *    {
 *        public IceShard(int level = 1) : base(
 *            id: level + 6,  // Выберите уникальный ID диапазон
 *            name: "IceShard",
 *            level: level)
 *        {
 *        }
 *
 *        protected override int GetDamage(World world, int casterId)
 *        {
 *            int baseDamage = world.Attack[casterId].Damage;
 *            
 *            return Level switch
 *            {
 *                1 => (int)(baseDamage * 1.1),
 *                2 => (int)(baseDamage * 0.75),
 *                3 => (int)(baseDamage * 0.5),
 *                _ => baseDamage
 *            };
 *        }
 *    }
 * 
 * 2. (Опционально) Переопределите GetTargets() для особой логики выбора целей
 * 
 * 3. Зарегистрируйте спелл в SpellSystem.RegisterDefaultSpells():
 * 
 *    RegisterSpell(new IceShard(1)); // ID 7
 *    RegisterSpell(new IceShard(2)); // ID 8
 *    RegisterSpell(new IceShard(3)); // ID 9
 * 
 * 4. Добавьте спелл героям в EntityFactory:
 * 
 *    world.Spells[entity.Id] = new SpellComponent
 *    {
 *        SpellIds = new List<int> { 1, 5, 7 }  // IDs спеллов
 *    };
 * 
 * ========================================
 * КАК ДОБАВИТЬ НОВЫЙ СТАТУС-ЭФФЕКТ
 * ========================================
 * 
 * 1. Создайте класс, реализующий IStatusEffect:
 * 
 *    public class SlowEffect : IStatusEffect
 *    {
 *        public string EffectName => "Slow";
 *        public int RemainingTurns { get; set; }
 *        private int _speedReduction;
 *
 *        public SlowEffect(int turns = 2, int speedReduction = 3)
 *        {
 *            RemainingTurns = turns;
 *            _speedReduction = speedReduction;
 *        }
 *
 *        public void ApplyEffect(World world, int targetId, CombatContext context)
 *        {
 *            if (!world.StatusEffects.ContainsKey(targetId))
 *            {
 *                world.StatusEffects[targetId] = new StatusEffectComponent();
 *            }
 *            world.StatusEffects[targetId].ActiveEffects.Add(this);
 *            
 *            // Уменьшаем скорость
 *            world.Speed[targetId].Value -= _speedReduction;
 *        }
 *
 *        public void OnTurnStart(World world, int targetId, CombatContext context)
 *        {
 *            context.Logs.Add(new CombatLogEntry
 *            {
 *                AttackerId = -1,
 *                TargetId = targetId,
 *                ActionType = "Slow",
 *                Damage = 0,
 *                TargetRemainingHp = world.Health[targetId].Current
 *            });
 *            
 *            RemainingTurns--;
 *        }
 *
 *        public void OnTurnEnd(World world, int targetId, CombatContext context)
 *        {
 *            // Выполнится в конце хода (если нужно)
 *        }
 *    }
 * 
 * 2. Применяйте эффект из спеллов:
 * 
 *    protected override void ApplyDamage(World world, int casterId, List<int> targets, CombatContext context)
 *    {
 *        base.ApplyDamage(world, casterId, targets, context);
 *        
 *        // Добавляем эффект к каждой цели
 *        foreach (var targetId in targets)
 *        {
 *            new SlowEffect(2, 2).ApplyEffect(world, targetId, context);
 *        }
 *    }
 * 
 * ========================================
 * ТИПЫ СТАТУС-ЭФФЕКТОВ
 * ========================================
 * 
 * BurnEffect - наносит урон в начале хода
 * FrostEffect - замораживает противника (визуально)
 * StunEffect - оглушает противника (запрещает действие)
 * 
 * ========================================
 * КАК ПРИМЕНЯТЬ ЭФФЕКТ К ЦЕЛИ
 * ========================================
 * 
 * Вариант 1: В методе ApplyDamage спелла
 * 
 *    var burnEffect = new BurnEffect(3, 10);  // 3 хода, 10 урона в ход
 *    burnEffect.ApplyEffect(world, targetId, context);
 * 
 * Вариант 2: Вероятностное применение эффекта
 * 
 *    if (new Random().Next(100) < 30)  // 30% шанс
 *    {
 *        new StunEffect(1).ApplyEffect(world, targetId, context);
 *    }
 * 
 * ========================================
 * ПОРЯДОК ВЫПОЛНЕНИЯ СИСТЕМ
 * ========================================
 * 
 * 1. TurnOrderSystem     - определяет, кто ходит
 * 2. StatusEffectSystem  - применяет эффекты в начале хода
 * 3. SpellSystem         - кастует спелл (если нет стана)
 * 4. DamageSystem        - наносит обычную атаку (если спелл не был)
 * 5. HealthSystem        - проверяет, жив ли кто-то
 * 
 * ========================================
 * ПРИМЕРЫ КОМПОНЕНТОВ
 * ========================================
 * 
 * SpellComponent - хранит IDs доступных спеллов
 * StatusEffectComponent - хранит активные эффекты
 * HealthComponent - текущее и максимальное здоровье
 * AttackComponent - урон базовой атаки
 * SpeedComponent - скорость (для очередности ходов)
 * TeamComponent - команда (1 или 2)
 */
