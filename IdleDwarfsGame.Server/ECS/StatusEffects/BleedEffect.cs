// Кровотечение (правила — docs/effect-rules.md): у наложения нет своей длительности, каждое добавляет ровно один стак,
// и кровотечение висит столько ходов, сколько на нём стаков.
// Первое наложение бьёт в полную силу, каждое следующее добавляет тем меньше урона, чем больше стаков уже висит:
// (MaxStacks - стаки) × 10% своего урона, с округлением вниз. На 5 стаках — 50%, на 7 — 30%, на 9 — 10%.
// На MaxStacks стаках наложение ничего не даёт — ни стака, ни урона.
// Каждый тик уходит самый новый (самый слабый) стак: сильные первые стаки держатся, а урон упирается в потолок —
// сумму прибавок 10 стаков, 100% + 90% + … + 10% = 5,5 урона одного наложения.
// Пример: 15, 10, 5 → стаки [15, 9, 4]: тик 28, затем [15, 9] — тик 24, затем [15] — тик 15
public class BleedEffect : StatusEffectBase
{
    public const int MaxStacks = 10;

    public override string EffectName => "Bleed";
    protected override DamageType? EffectDamageType => DamageType.Bleed;

    // Прибавка каждого стака к урону за ход и чья она, в порядке наложения
    private readonly List<(int Power, StatOrigin Origin)> _stacks = new();

    public BleedEffect(int damagePerTurn = 5) : base(1, damagePerTurn)
    {
        _stacks.Add((damagePerTurn, StatOrigin.Environment));
    }

    protected override void Absorb(StatusEffectBase added)
    {
        if (Stacks >= MaxStacks) return;
        // (10 - стаки) × урон × 10 / 100
        int extra = (MaxStacks - Stacks) * added.Power / MaxStacks;
        _stacks.Add((extra, added.Origin));
        Sync();
    }

    protected override void SetOrigin(StatOrigin origin)
    {
        base.SetOrigin(origin);
        for (int i = 0; i < _stacks.Count; i++)
            _stacks[i] = (_stacks[i].Power, origin);
    }

    // «Мастер стихии» усиливает урон, но ходов не добавляет: ходы кровотечения — это его стаки,
    // а наложение не может дать больше одного стака
    protected override void Empower(int percent, int turns)
    {
        for (int i = 0; i < _stacks.Count; i++)
        {
            var (power, origin) = _stacks[i];
            _stacks[i] = ((int)Math.Round(power * (100 + percent) / 100.0, MidpointRounding.AwayFromZero), origin);
        }
        Sync();
    }

    // Потолок периодического источника (арена) режет стаки, а не ходы: ходы и так равны стакам
    protected override void LimitTurns(int limit)
    {
    }

    protected override List<(StatOrigin Origin, int Weight)> OriginWeights() =>
        _stacks.Select(s => (s.Origin, s.Power)).ToList();

    public override void OnTurnStart(World world, int targetId, CombatContext context)
    {
        int damage = Power;
        // Источники — до снятия стака: этот тик ещё его
        var weights = OriginWeights();
        _stacks.RemoveAt(_stacks.Count - 1);
        Sync();
        if (!world.IsAlive(targetId)) return;
        DealTickDamage(world, targetId, context, damage, DamageType.Bleed, weights);
    }

    // Каждый тик снимает самый новый стак: стак i (с нуля, от старого) продержится i + 1 тиков
    public override int RemainingDamage => _stacks.Select((s, i) => s.Power * (i + 1)).Sum();

    public override bool TrimStacks(int keep)
    {
        if (_stacks.Count <= keep) return false;
        _stacks.RemoveRange(keep, _stacks.Count - keep);
        Sync();
        return true;
    }

    private void Sync()
    {
        Power = _stacks.Sum(s => s.Power);
        Stacks = _stacks.Count;
        RemainingTurns = _stacks.Count;
    }
}
