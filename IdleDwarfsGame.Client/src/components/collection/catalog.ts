import type { AuraTarget, DamageType, HeroElement } from '@/battle/types';

// Справочник окна «Коллекция»: все виды героев, их заклинания, ауры и статусы.
// Книга заклинаний повторяет серверную ElementSpellBook на 5-м ранге; каждое заклинание можно посмотреть на любом из трёх уровней

// Что заклинание делает на одном уровне. Урон и лечение — доли атаки заклинателя
// (числа на сцене для наглядности, не точный расчёт сервера).
// Без урона заклинание лечит союзника, с уроном — самого заклинателя
export interface SpellLevelSample {
    damage?: number;
    heal?: number;
    // Статусы, которые заклинание накладывает на цель
    statuses?: string[];
    // Сколько целей поражает. Если больше одной — на сцене три врага (или три союзника у лечения)
    targets?: number;
    // На сколько из поражённых ложатся статусы (у оглушения с шансом — не на всех); по умолчанию на всех
    statusTargets?: number;
}

export interface SpellSample {
    // Ключ описания collection.spellDescriptions.<key>
    key: string;
    // Имя заклинания на сервере (SkillName): по нему и уровню FxRegistry выбирает визуал
    name: string;
    damageType: DamageType;
    // Уровни 1, 2, 3 по порядку
    levels: [SpellLevelSample, SpellLevelSample, SpellLevelSample];
}

export const SPELL_LEVELS = [1, 2, 3] as const;

// Уровень, который открыт у героя на PREVIEW_RANK — с него начинается просмотр
export const DEFAULT_SPELL_LEVEL = 3;

// Числа — по формулам серверных спеллов (ECS/Spells)
export const SPELLS: Record<string, SpellSample> = {
    fireball: {
        key: 'fireball', name: 'Fireball', damageType: 'Fire',
        levels: [
            { damage: 1.2, statuses: ['Burn'] },
            { damage: 0.8, statuses: ['Burn'], targets: 3 },
            { damage: 0.6, statuses: ['Burn'], targets: 3 },
        ],
    },
    flameWave: {
        key: 'flameWave', name: 'Flame Wave', damageType: 'Fire',
        levels: [
            { damage: 0.7, targets: 2 },
            { damage: 0.75, targets: 3 },
            { damage: 0.7, statuses: ['Burn'], targets: 3 },
        ],
    },
    meteor: {
        key: 'meteor', name: 'Meteor', damageType: 'Fire',
        levels: [
            { damage: 2 },
            { damage: 2.4, statuses: ['Burn'] },
            { damage: 2.8, statuses: ['Burn'] },
        ],
    },
    frostBolt: {
        key: 'frostBolt', name: 'Frost Bolt', damageType: 'Frost',
        levels: [
            { damage: 1.1, statuses: ['Frost'] },
            { damage: 1.25, statuses: ['Frost'] },
            { damage: 1.4, statuses: ['Frost'] },
        ],
    },
    blizzard: {
        key: 'blizzard', name: 'Blizzard', damageType: 'Frost',
        levels: [
            { damage: 0.45, statuses: ['Frost'], targets: 3 },
            { damage: 0.55, statuses: ['Frost'], targets: 3 },
            { damage: 0.65, statuses: ['Frost'], targets: 3 },
        ],
    },
    glacialPrison: {
        key: 'glacialPrison', name: 'Glacial Prison', damageType: 'Frost',
        levels: [
            { damage: 0.8, statuses: ['Freeze'] },
            { damage: 1, statuses: ['Freeze'] },
            { damage: 1, statuses: ['Freeze'], targets: 2 },
        ],
    },
    lightning: {
        key: 'lightning', name: 'Lightning', damageType: 'Lightning',
        levels: [
            { damage: 1.3 },
            { damage: 0.9, targets: 2 },
            { damage: 0.9, statuses: ['Stun'], targets: 2 },
        ],
    },
    chainLightning: {
        key: 'chainLightning', name: 'Chain Lightning', damageType: 'Lightning',
        levels: [
            { damage: 0.7, targets: 2 },
            { damage: 0.7, targets: 3 },
            { damage: 0.7, statuses: ['Shock'], targets: 4 },
        ],
    },
    thunderclap: {
        key: 'thunderclap', name: 'Thunderclap', damageType: 'Lightning',
        levels: [
            { damage: 0.4, statuses: ['Stun'], targets: 3, statusTargets: 1 },
            { damage: 0.5, statuses: ['Stun'], targets: 3, statusTargets: 1 },
            { damage: 0.6, statuses: ['Stun'], targets: 3, statusTargets: 1 },
        ],
    },
    healingRune: {
        key: 'healingRune', name: 'Healing Rune', damageType: 'Magical',
        levels: [
            { heal: 2, statuses: ['Regen'] },
            { heal: 2.5, statuses: ['Regen'] },
            { heal: 3, statuses: ['Regen'] },
        ],
    },
    venomThorns: {
        key: 'venomThorns', name: 'Venom Thorns', damageType: 'Poison',
        levels: [
            { damage: 0.8, statuses: ['Poison'] },
            { damage: 0.8, statuses: ['Poison'], targets: 2 },
            { damage: 0.8, statuses: ['Poison'], targets: 3 },
        ],
    },
    entanglingRoots: {
        key: 'entanglingRoots', name: 'Entangling Roots', damageType: 'Poison',
        levels: [
            { damage: 0.6, statuses: ['Stun'] },
            { damage: 0.7, statuses: ['Stun'], targets: 2 },
            { damage: 0.8, statuses: ['Stun', 'Poison'], targets: 2 },
        ],
    },
    shadowBolt: {
        key: 'shadowBolt', name: 'Shadow Bolt', damageType: 'Dark',
        levels: [
            { damage: 1.2 },
            { damage: 1.4, statuses: ['Curse'] },
            { damage: 1.6, statuses: ['Curse'] },
        ],
    },
    lifeDrain: {
        key: 'lifeDrain', name: 'Life Drain', damageType: 'Dark',
        levels: [
            { damage: 0.9, heal: 0.45 },
            { damage: 1.1, heal: 0.66 },
            { damage: 1.3, heal: 0.91 },
        ],
    },
    hex: {
        key: 'hex', name: 'Hex', damageType: 'Dark',
        levels: [
            { damage: 0.3, statuses: ['Curse'], targets: 3 },
            { damage: 0.3, statuses: ['Curse'], targets: 3 },
            { damage: 0.3, statuses: ['Curse'], targets: 3 },
        ],
    },
    prismRay: {
        key: 'prismRay', name: 'Prism Ray', damageType: 'Radiant',
        levels: [
            { damage: 1.3 },
            { damage: 1.5 },
            { damage: 1.5, targets: 2 },
        ],
    },
    radiantNova: {
        key: 'radiantNova', name: 'Radiant Nova', damageType: 'Radiant',
        levels: [
            { damage: 0.5, targets: 3 },
            { damage: 0.6, heal: 0.2, targets: 3 },
            { damage: 0.7, heal: 0.3, targets: 3 },
        ],
    },
    dawnlight: {
        key: 'dawnlight', name: 'Dawnlight', damageType: 'Radiant',
        levels: [
            { heal: 0.6, statuses: ['Regen'], targets: 3 },
            { heal: 0.8, statuses: ['Regen'], targets: 3 },
            { heal: 1, statuses: ['Regen'], targets: 3 },
        ],
    },
};

export interface HeroKind {
    element: HeroElement;
    spells: SpellSample[];
}

// Ранг, на котором показываем героев: на нём открыта вся книга стихии
export const PREVIEW_RANK = 5;

export const HERO_KINDS: HeroKind[] = [
    { element: 'Fire', spells: [SPELLS.fireball, SPELLS.flameWave, SPELLS.meteor] },
    { element: 'Frost', spells: [SPELLS.frostBolt, SPELLS.blizzard, SPELLS.glacialPrison] },
    { element: 'Lightning', spells: [SPELLS.lightning, SPELLS.chainLightning, SPELLS.thunderclap] },
    { element: 'Nature', spells: [SPELLS.healingRune, SPELLS.venomThorns, SPELLS.entanglingRoots] },
    { element: 'Dark', spells: [SPELLS.shadowBolt, SPELLS.lifeDrain, SPELLS.hex] },
    { element: 'Radiance', spells: [SPELLS.prismRay, SPELLS.radiantNova, SPELLS.dawnlight] },
];

export interface AuraSample {
    // Ключ названия collection.auras.<key>
    key: string;
    // Эффект ауры (Hero.AuraEffect): по нему FxRegistry выбирает цвет сферы.
    // У «усиления» эффект неизвестен клиенту — для союзников это золотая аура по умолчанию
    effect: string;
    target: AuraTarget;
    // Стихия носителя на сцене
    element: HeroElement;
}

// По одной ауре на каждый визуал из AURA_VISUALS
export const AURA_SAMPLES: AuraSample[] = [
    { key: 'fire', effect: 'Burn', target: 'Enemies', element: 'Fire' },
    { key: 'frost', effect: 'Frost', target: 'Enemies', element: 'Frost' },
    { key: 'heal', effect: 'Regen', target: 'Allies', element: 'Nature' },
    { key: 'lightning', effect: 'Shock', target: 'Enemies', element: 'Lightning' },
    { key: 'arcane', effect: 'Arcane', target: 'Enemies', element: 'Dark' },
    { key: 'poison', effect: 'Poison', target: 'Enemies', element: 'Nature' },
    { key: 'blood', effect: 'Bleed', target: 'Enemies', element: 'Fire' },
    { key: 'shadow', effect: 'Curse', target: 'Enemies', element: 'Dark' },
    { key: 'buff', effect: 'Might', target: 'Allies', element: 'Radiance' },
];

// Статусы с тиком урона и его тип (StatusEffectBase на сервере)
export const STATUS_DAMAGE: Record<string, DamageType> = {
    Burn: 'Fire',
    Poison: 'Poison',
    Curse: 'Dark',
    Shock: 'Lightning',
    Bleed: 'Bleed',
};

// Статусы, с которыми юнит пропускает ход (событие Stunned)
export const SKIP_TURN_STATUSES = new Set(['Stun', 'Freeze']);

// Статусы с ходовым лечением (событие Heal)
export const HEAL_STATUSES = new Set(['Regen']);

// Порядок вкладки «Эффекты» — как в STATUS_VISUALS
export const EFFECT_SAMPLES = ['Burn', 'Frost', 'Freeze', 'Stun', 'Shock', 'Poison', 'Bleed', 'Curse', 'Regen'];
