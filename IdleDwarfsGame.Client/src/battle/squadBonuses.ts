import type { HeroElement } from './types';

// Бонусы стихий отряда — клиентское зеркало SquadBonusBook.Resolve (сервер, docs/squad-bonuses.md).
// Отряд получает ровно один бонус по стихиям своих дворфов (Basic и монстры не считаются):
//   5 одной стихии → моно III, 4 + 1 → моно II, 3 + 2 → пара этих стихий, 3 + 1 + 1 → моно I,
//   5 разных стихий → «Призма», остальное — без бонуса.
// Числа для описаний — копия SquadBonusBook: поменяли там — поменять и здесь

// Стихии в порядке HeroElement: по нему же собраны имена пар (FireFrost, не FrostFire)
export const BONUS_ELEMENTS = ['Fire', 'Frost', 'Lightning', 'Nature', 'Dark', 'Radiance', 'Blood'] as const;
export type BonusElement = typeof BONUS_ELEMENTS[number];

export type SquadBonusShape = 'Mono' | 'Pair' | 'Prism';

export interface SquadBonus {
    // Id бонуса как на сервере: Fire, FireFrost, Prism
    id: string;
    shape: SquadBonusShape;
    // Уровень моно-бонуса 1..3; у пары и призмы — 1
    tier: number;
    // У моно — одна стихия, у пары — две по порядку HeroElement, у призмы — ни одной
    elements: BonusElement[];
}

export const SQUAD_SIZE = 5;
// Сколько дворфов одной стихии нужно на уровень моно-бонуса: I — 3, II — 4, III — 5
export const MONO_COUNTS = [3, 4, 5];
// Пара 3 + 2
export const PAIR_MAIN = 3;
export const PAIR_SECOND = 2;

const isBonusElement = (element: HeroElement | null | undefined): element is BonusElement =>
    !!element && (BONUS_ELEMENTS as readonly string[]).includes(element);

const order = (element: BonusElement) => BONUS_ELEMENTS.indexOf(element);

export const monoBonus = (element: BonusElement, tier: number): SquadBonus =>
    ({ id: element, shape: 'Mono', tier: Math.min(3, Math.max(1, tier)), elements: [element] });

export const pairBonus = (a: BonusElement, b: BonusElement): SquadBonus => {
    const elements = order(a) < order(b) ? [a, b] : [b, a];
    return { id: elements.join(''), shape: 'Pair', tier: 1, elements };
};

export const PRISM: SquadBonus = { id: 'Prism', shape: 'Prism', tier: 1, elements: [] };

// Сколько дворфов каждой стихии; Basic и пустые места не считаются
export const elementCounts = (elements: (HeroElement | null | undefined)[]): Map<BonusElement, number> => {
    const counts = new Map<BonusElement, number>();
    for (const element of elements) {
        if (isBonusElement(element)) counts.set(element, (counts.get(element) ?? 0) + 1);
    }
    return counts;
};

// Стихии по убыванию числа дворфов, при равенстве — по порядку HeroElement (как ThenBy на сервере)
const sortedCounts = (counts: Map<BonusElement, number>) =>
    [...counts.entries()].sort((a, b) => b[1] - a[1] || order(a[0]) - order(b[0]));

// Бонус отряда с такими стихиями; null — бонуса нет
export const resolveSquadBonus = (elements: (HeroElement | null | undefined)[]): SquadBonus | null => {
    const counts = sortedCounts(elementCounts(elements));
    if (!counts.length) return null;
    const [top, count] = counts[0];
    if (count >= 5) return monoBonus(top, 3);
    if (count === 4) return monoBonus(top, 2);
    if (count === 3) return counts.length > 1 && counts[1][1] >= 2 ? pairBonus(top, counts[1][0]) : monoBonus(top, 1);
    if (counts.length >= SQUAD_SIZE && counts.every(([, c]) => c === 1)) return PRISM;
    return null;
};

// Бонус отряда на арене: стихия засчитывается только у дворфов
export const unitsSquadBonus = (units: { unitType?: string | null; element: HeroElement }[]): SquadBonus | null =>
    resolveSquadBonus(units.filter(u => (u.unitType ?? 'Dwarf') === 'Dwarf').map(u => u.element));

// Прибавка к боевой мощи, % (SquadBonusBook.PowerPercent) — заодно порядок силы бонусов
export const bonusPower = (bonus: SquadBonus): number => {
    if (bonus.shape === 'Prism') return 14;
    if (bonus.shape === 'Pair') return 29;
    return [20, 40, 61][bonus.tier - 1];
};

/* ===== Мощь отряда с бонусом ===== */

// То, что нужно герою для мощи отряда: подходит и HeroInfo, и герой забега спуска
export interface SquadPowerHero {
    power: { total: number };
    element: HeroElement;
    level: number;
    unitType?: string | null;
}

export interface SquadPower {
    // Итог — как CombatPower.OfSquad на сервере: мощь героев плюс прибавка бонуса
    total: number;
    // Сумма мощи героев без бонуса
    base: number;
    bonus: SquadBonus | null;
    bonusPower: number;
}

// Бонус отряда героев (инвентарь, отряд): стихия героя 1-го ранга не считается (HeroLevels.NormalizeElement), монстр — тоже
export const heroesSquadBonus = (heroes: (Pick<SquadPowerHero, 'element' | 'level' | 'unitType'> | null | undefined)[]): SquadBonus | null =>
    resolveSquadBonus(heroes.map(hero =>
        (hero && hero.level > 1 && (hero.unitType ?? 'Dwarf') === 'Dwarf' ? hero.element : 'Basic')));

// Мощь героя с сервера (power.total) посчитана для героя в одиночку — без бонуса отряда.
// Сервер прибавляет бонус каждому дворфу отряда отдельно с округлением (CombatPower.Of), здесь — так же.
// Стихия героя 1-го ранга не считается (HeroLevels.NormalizeElement), монстр — тоже
export const squadPowerOf = (heroes: (SquadPowerHero | null | undefined)[]): SquadPower => {
    const squad = heroes.filter((hero): hero is SquadPowerHero => !!hero);
    const bonus = heroesSquadBonus(squad);
    const percent = bonus ? bonusPower(bonus) : 0;
    const base = squad.reduce((sum, hero) => sum + hero.power.total, 0);
    const bonusPart = squad.reduce((sum, hero) => sum + Math.round((hero.power.total * percent) / 100), 0);
    return { total: base + bonusPart, base, bonus, bonusPower: bonusPart };
};

// Все бонусы для каталога: 7 моно, 21 пара и призма
export const ALL_MONO = BONUS_ELEMENTS.map(element => monoBonus(element, 1));
export const ALL_PAIRS: SquadBonus[] = BONUS_ELEMENTS.flatMap((a, i) => BONUS_ELEMENTS.slice(i + 1).map(b => pairBonus(a, b)));

/* ===== Подсказки при сборке отряда ===== */

// Чего не хватает до бонуса: «ещё 1 огонь → Неугасимое пламя III»
export interface SquadBonusHint {
    bonus: SquadBonus;
    // Сколько дворфов какой стихии добавить (на свободное место или вместо того, кто бонусу не нужен)
    need: { element: BonusElement; count: number }[];
    // Для призмы: сколько ещё разных стихий нужно (любых из недостающих)
    needDistinct?: number;
}

// Подсказываем только близкие бонусы
const MAX_HINT_NEED = 2;
const MAX_HINTS = 4;

const hintSize = (hint: SquadBonusHint) => hint.need.reduce((sum, n) => sum + n.count, 0) + (hint.needDistinct ?? 0);

// Бонусы сильнее текущего, до которых не хватает 1–2 дворфов. Ближние — первыми, при равенстве — сильные.
// Считаем по стихиям, которые уже есть в отряде: чтобы подсказка вела от состава, а не перечисляла всё подряд
export const squadBonusHints = (elements: (HeroElement | null | undefined)[]): SquadBonusHint[] => {
    const counts = elementCounts(elements);
    const current = resolveSquadBonus(elements);
    const currentPower = current ? bonusPower(current) : 0;
    const hints: SquadBonusHint[] = [];
    const has = (element: BonusElement) => counts.get(element) ?? 0;

    // Моно: следующий уровень по каждой стихии
    for (const [element, count] of counts) {
        const target = MONO_COUNTS.find(n => n > count);
        if (!target) continue;
        hints.push({ bonus: monoBonus(element, MONO_COUNTS.indexOf(target) + 1), need: [{ element, count: target - count }] });
    }

    // Пары: обе стихии уже в отряде, главная — та, где не хватает меньше
    const present = [...counts.keys()];
    for (let i = 0; i < present.length; i++) {
        for (let j = i + 1; j < present.length; j++) {
            const options = [[present[i], present[j]], [present[j], present[i]]].map(([main, second]) => [
                { element: main, count: Math.max(0, PAIR_MAIN - has(main)) },
                { element: second, count: Math.max(0, PAIR_SECOND - has(second)) },
            ].filter(n => n.count > 0));
            const need = options.reduce((best, n) => (n.reduce((s, x) => s + x.count, 0) < best.reduce((s, x) => s + x.count, 0) ? n : best));
            if (need.length) hints.push({ bonus: pairBonus(present[i], present[j]), need });
        }
    }

    // Призма: все стихии пока разные, их уже хотя бы три
    if (present.length >= 3 && present.every(e => has(e) === 1) && present.length < SQUAD_SIZE) {
        hints.push({ bonus: PRISM, need: [], needDistinct: SQUAD_SIZE - present.length });
    }

    return hints
        .filter(hint => hintSize(hint) > 0 && hintSize(hint) <= MAX_HINT_NEED && bonusPower(hint.bonus) > currentPower)
        .sort((a, b) => hintSize(a) - hintSize(b) || bonusPower(b.bonus) - bonusPower(a.bonus))
        .slice(0, MAX_HINTS);
};

/* ===== Числа для описаний (SquadBonusBook) ===== */

// Параметры описаний моно-бонусов по уровням I / II / III: ключ — плейсхолдер в тексте squadBonuses.descriptions
export const MONO_VALUES: Record<BonusElement, Record<string, number[]>> = {
    Fire: { damage: [50, 80, 90], spread: [60, 80, 100] },
    Frost: { slow: [80, 140, 200], threshold: [40, 30, 25], cold: [30, 60, 70], frozen: [10, 25, 40] },
    Lightning: { every: [2, 2, 1], damage: [200, 400, 275], shock: [20, 40, 60] },
    Nature: { heal: [6, 10, 15], poison: [110, 180, 225], revive: [30, 50, 90] },
    Dark: { damage: [30, 40, 50] },
    Radiance: { cap: [20, 35, 60], damage: [80, 120, 140] },
    Blood: { base: [10, 20, 35], per: [3, 2, 2], cap: [30, 60, 100], heal: [20, 45, 70] },
};

// Римская цифра уровня моно-бонуса
export const tierNumeral = (tier: number) => ['I', 'II', 'III'][tier - 1] ?? '';
