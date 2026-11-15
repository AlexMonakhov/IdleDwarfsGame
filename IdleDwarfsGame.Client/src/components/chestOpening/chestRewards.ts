// Карты открытия сундука: всегда 5 штук — основные награды сундука + доп. награды до пяти.
// Пока сервер отдаёт только основные (ChestOpenResult), доп. награды на тестовой странице — мок (mockChestRewards)
import type { ChestBonusKind, ChestOpenResult, ChestType, Rarity } from '@/models/types/Inventory';
import { RARITY_ORDER } from '@/models/types/Forge';
import type { HeroElement } from '@/battle/types';

export const CHEST_CARD_COUNT = 5;

// Фон окна карты награды (CardBackdrop)
export type BackdropStyle =
    | 'classic' | 'tunnel' | 'rings' | 'vortex' | 'corridor' | 'sunburst' | 'veins' | 'facets' | 'stars' | 'grid'
    | 'hexprism' | 'geode' | 'shards' | 'quartz';

export const BACKDROP_STYLES: { id: BackdropStyle; name: string }[] = [
    { id: 'classic', name: 'Классика (порода)' },
    { id: 'tunnel', name: 'Тоннель-шахматка' },
    { id: 'rings', name: 'Кольца' },
    { id: 'vortex', name: 'Вихрь' },
    { id: 'corridor', name: 'Коридор' },
    { id: 'sunburst', name: 'Сияние' },
    { id: 'veins', name: 'Жилы' },
    { id: 'facets', name: 'Грани кристалла' },
    { id: 'stars', name: 'Звёзды' },
    { id: 'grid', name: 'Сетка' },
    { id: 'hexprism', name: 'Шестигранник' },
    { id: 'geode', name: 'Жеода' },
    { id: 'shards', name: 'Осколки' },
    { id: 'quartz', name: 'Кварцевая мозаика' },
];

export type ChestRewardKind =
    | 'Hero'
    | 'Gem'
    | 'Equipment'
    | 'Gold'
    | 'Experience'
    | 'RainbowCoins'
    | 'EventCurrency'
    | 'ItemShards'
    | 'HeroShards';

export interface ChestRewardCard {
    kind: ChestRewardKind;
    rarity: Rarity;
    // Подпись для наград без своего перевода (доп. награды); герой, камень и предмет подписываются по своим полям
    title: string;
    amount?: number;
    // Цвет значка доп. награды
    accent?: string;
    // Герой: стихия и ранг — портрет как на карточке героя
    element?: HeroElement;
    rank?: number;
    // Гем: камень (Ruby, Sapphire…)
    gem?: string;
    // Снаряжение: ключ предмета — картинка и название
    itemKey?: string;
    // Доп. награда (не из таблицы сундука)
    bonus: boolean;
}

// Цвета CardComponent по редкости (рубашка, ядро и волна раскрытия)
export type CardColor = 'purple' | 'green' | 'blue' | 'yellow' | 'red' | 'grey';

export const RARITY_CARD_COLORS: Record<Rarity, CardColor> = {
    Common: 'grey',
    Uncommon: 'green',
    Rare: 'blue',
    Mythic: 'purple',
    Legendary: 'yellow',
    Artifact: 'red',
};

export const rarityRank = (rarity: Rarity) => RARITY_ORDER.indexOf(rarity);

// Варианты анимации появления карт: при каждом открытии выбирается случайный
export type ChestOpeningVariant = 'burst' | 'deal' | 'fan' | 'vein';
export const randomVariant = (): ChestOpeningVariant => CHEST_OPENING_VARIANTS[Math.floor(Math.random() * CHEST_OPENING_VARIANTS.length)].id;

export const CHEST_OPENING_VARIANTS: { id: ChestOpeningVariant; name: string; description: string }[] = [
    { id: 'burst', name: 'Взрыв', description: 'Сундук вспыхивает, все карты разлетаются разом с вращением' },
    { id: 'deal', name: 'Раздача', description: 'Карты по одной вылетают из сундука, как при раздаче' },
    { id: 'fan', name: 'Веер', description: 'Карты поднимаются стопкой и раскрываются веером' },
    { id: 'vein', name: 'Жила', description: 'Сундук уходит в породу, карты по очереди вырастают из светящейся трещины' },
];

// ---------- Ответ сервера → карты ----------

const BONUS_KINDS: Record<ChestBonusKind, ChestRewardKind> = {
    Gold: 'Gold',
    Experience: 'Experience',
    Gems: 'Gem',
    RainbowCoins: 'RainbowCoins',
    EventCurrency: 'EventCurrency',
};

const BONUS_TITLES: Partial<Record<ChestBonusKind, string>> = {
    Gold: 'chests.reveal.kinds.Gold',
    Experience: 'chests.reveal.kinds.Experience',
    RainbowCoins: 'chests.reveal.kinds.RainbowCoins',
    EventCurrency: 'chests.reveal.kinds.EventCurrency',
};

// Ранг дворфа = редкость карты: 1 — обычный … 5 — артефакт
const heroRarity = (rank: number): Rarity => RARITY_ORDER[Math.min(Math.max(rank, 1), RARITY_ORDER.length) - 1];

// Пять карт открытия: герои, камни и снаряжение сундука + доп. награды, в случайном порядке.
// translate — i18n t(): подписи доп. наград
export const cardsFromResult = (result: ChestOpenResult, translate: (key: string) => string): ChestRewardCard[] => {
    const cards: ChestRewardCard[] = [
        ...result.heroes.map(hero => ({
            kind: 'Hero' as const, rarity: heroRarity(hero.level), title: hero.name, element: hero.element, rank: hero.level, bonus: false,
        })),
        ...result.items.map(item => ({
            kind: 'Gem' as const, rarity: item.rarity, title: item.subType, gem: item.subType, amount: item.quantity, bonus: false,
        })),
        ...result.equipment.map(item => ({
            kind: 'Equipment' as const, rarity: item.rarity, title: item.key, itemKey: item.key, bonus: false,
        })),
        ...(result.bonuses ?? []).map(bonus => ({
            kind: BONUS_KINDS[bonus.kind],
            rarity: bonus.rarity,
            title: BONUS_TITLES[bonus.kind] ? translate(BONUS_TITLES[bonus.kind] as string) : bonus.subType,
            gem: bonus.kind === 'Gems' ? bonus.subType : undefined,
            amount: bonus.amount,
            bonus: true,
        })),
    ];
    // Порядок перемешан: лучшая карта не всегда первая
    return cards.sort(() => Math.random() - 0.5);
};

// ---------- Мок для тестовой страницы ----------

const pick = <T>(list: readonly T[]) => list[Math.floor(Math.random() * list.length)];
const between = (min: number, max: number) => min + Math.floor(Math.random() * (max - min + 1));

// Редкость по весам: weights[i] — вес RARITY_ORDER[i]
const rollRarity = (weights: number[]): Rarity => {
    const total = weights.reduce((a, b) => a + b, 0);
    let roll = Math.random() * total;
    for (let i = 0; i < weights.length; i++) {
        roll -= weights[i];
        if (roll < 0) return RARITY_ORDER[i];
    }
    return RARITY_ORDER[0];
};

const HERO_ELEMENTS: HeroElement[] = ['Fire', 'Frost', 'Lightning', 'Nature', 'Dark', 'Radiance', 'Blood'];
const ELEMENT_ACCENTS: Record<string, string> = {
    Fire: '#ef4444', Frost: '#3b82f6', Lightning: '#facc15', Nature: '#22c55e', Dark: '#a855f7', Radiance: '#e2e8f0', Blood: '#e91e63',
};
const ELEMENT_GEMS = ['Ruby', 'Sapphire', 'Topaz', 'Emerald'];
// Ключи предметов по редкости — как в EquipmentCatalog на сервере. Артефакты — по оружию на каждое заклинание стихии
const ITEMS_BY_RARITY: Record<Rarity, string[]> = {
    Common: ['BoneNecklace', 'CopperRing', 'IronHelm', 'RustyAxe'],
    Uncommon: ['DwarvenHammer', 'MinerHelm', 'MoonstonePendant', 'RingOfTheUnderdog', 'SpitefulRing', 'TwinBand'],
    Rare: ['EmberCrown', 'EmberMace', 'FrostCrown', 'RingOfLingeringWords', 'RingOfTheStoneSkin', 'RunicAmulet', 'StormSpear'],
    Mythic: ['BlackIceSword', 'GroveCirclet', 'RingOfCleaving', 'StormAmulet', 'ThornStaff'],
    Legendary: ['DawnBlade', 'HeartOfTheMountain', 'RingOfWildfire', 'WarlordHelm'],
    Artifact: [
        'ArmageddonBlade', 'PhoenixStaff', 'VolcanoHeart',
        'GlacialRelic', 'WinterShard', 'GlacierShackles',
        'ThunderHammer', 'StormCoil', 'ThunderHorn',
        'LifeTreeSeed', 'WyvernSting', 'ElderGroveStaff',
        'AbyssalSigil', 'CurseDoll', 'ReaperScythe',
        'PrismLens', 'SunDisc', 'MorningStar',
        'BloodMoonFang', 'ThousandCutsDagger', 'SacrificialAltar',
    ],
};

// Сколько основных наград даёт сундук: остальное до пяти — доп. награды
const MAIN_SLOTS: Record<ChestType, number> = { Hero: 2, Equipment: 1, Gem: 2, Infinity: 3 };

const mainReward = (type: ChestType, level: number, index: number): ChestRewardCard => {
    // Выше уровень — тяжелее хвост редкостей
    const shift = level - 1;
    const weights = [Math.max(5, 60 - shift * 14), 28, 10 + shift * 4, 2 + shift * 3, shift * 1.5, shift * 0.6];
    const kind: ChestType = type === 'Infinity' ? (['Hero', 'Gem', 'Equipment'] as ChestType[])[index % 3] : type;
    // Артефакт бывает только у снаряжения: дворфов 5-го ранга ещё нет
    const rarity = rollRarity(kind === 'Equipment' ? weights : weights.slice(0, 5));
    if (kind === 'Hero') {
        // Ранг дворфа = редкость: 1 — обычный … 4 — легендарный
        return { kind: 'Hero', rarity, title: '', element: pick(HERO_ELEMENTS), rank: rarityRank(rarity) + 1, bonus: false };
    }
    if (kind === 'Gem') {
        const gemRarity = rarity === 'Legendary' ? 'Mythic' : rarity;
        const gem = gemRarity === 'Rare' || gemRarity === 'Mythic' ? 'Diamond' : pick(ELEMENT_GEMS);
        return { kind: 'Gem', rarity: gemRarity, title: '', gem, amount: between(2, 6) * level, bonus: false };
    }
    return { kind: 'Equipment', rarity, title: '', itemKey: pick(ITEMS_BY_RARITY[rarity]), bonus: false };
};

// Доп. награды: золото/опыт часто, осколки иногда, радужные и ивентовые монеты — редко и только с 4-го уровня
const bonusReward = (level: number): ChestRewardCard => {
    const roll = Math.random();
    const highLevel = level >= 4;
    if (highLevel && roll < 0.03) return { kind: 'RainbowCoins', rarity: 'Legendary', title: 'Радужные монеты', amount: between(1, 3), bonus: true };
    if (highLevel && roll < 0.08) return { kind: 'EventCurrency', rarity: 'Mythic', title: 'Монеты ивента', amount: between(10, 30), bonus: true };
    if (roll < 0.16) return { kind: 'ItemShards', rarity: pick(['Rare', 'Mythic'] as Rarity[]), title: 'Осколки предмета', amount: between(3, 10), bonus: true };
    if (roll < 0.26) {
        const element = pick(HERO_ELEMENTS);
        return { kind: 'HeroShards', rarity: 'Rare', title: 'Осколки героя', element, accent: ELEMENT_ACCENTS[element], amount: between(2, 8), bonus: true };
    }
    if (roll < 0.63) return { kind: 'Gold', rarity: 'Common', title: 'Золото', amount: between(8, 20) * 100 * level, bonus: true };
    return { kind: 'Experience', rarity: 'Uncommon', title: 'Опыт', amount: between(5, 12) * 100 * level, bonus: true };
};

export const mockChestRewards = (type: ChestType, level: number): ChestRewardCard[] => {
    const main = Array.from({ length: MAIN_SLOTS[type] }, (_, i) => mainReward(type, level, i));
    const bonus = Array.from({ length: CHEST_CARD_COUNT - main.length }, () => bonusReward(level));
    // Порядок перемешан: лучшая карта не всегда последняя
    return [...main, ...bonus].sort(() => Math.random() - 0.5);
};

// Тест: гарантированный артефакт
export const mockArtifactReward = (): ChestRewardCard =>
    ({ kind: 'Equipment', rarity: 'Artifact', title: '', itemKey: pick(ITEMS_BY_RARITY.Artifact), bonus: false });
