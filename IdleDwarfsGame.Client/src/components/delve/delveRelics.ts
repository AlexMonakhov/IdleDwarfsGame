import type { Component } from 'vue';
import { IconAxe, IconCoin, IconFang, IconFlame, IconFlask, IconPouch, IconRune, IconShield, IconSpyglass } from '@/components/icons/ui';
import { IconAttack, IconSpeed, IconStartEnergy } from '@/components/icons/stats';

// Реликвии спуска: значок и цвет по ключу. Боевые — цвета своего стата, добычи — золото, феникс — пламя
export const RELIC_ICONS: Record<string, Component> = {
    rage: IconAttack,
    stoneskin: IconShield,
    swiftboots: IconSpeed,
    bloodthirst: IconFang,
    ale: IconFlask,
    spark: IconStartEnergy,
    giantslayer: IconAxe,
    phoenix: IconFlame,
    luck: IconCoin,
    wisdom: IconRune,
    greed: IconPouch,
    insight: IconSpyglass,
};

// Цвета значка: светлая и тёмная половины SplitIcon. Без них значки берут бледные цвета по умолчанию (беж и бирюза).
// Тёмная — она же акцент реликвии: оправа, стекло, свечение
const RELIC_COLORS: Record<string, { light: string; dark: string }> = {
    rage: { light: '#fecaca', dark: '#ef4444' },
    stoneskin: { light: '#f1f5f9', dark: '#64748b' },
    swiftboots: { light: '#ccfbf1', dark: '#14b8a6' },
    bloodthirst: { light: '#fecdd3', dark: '#e11d48' },
    ale: { light: '#fde68a', dark: '#d97706' },
    spark: { light: '#dbeafe', dark: '#3b82f6' },
    giantslayer: { light: '#fed7aa', dark: '#ea580c' },
    phoenix: { light: '#fef08a', dark: '#f97316' },
    luck: { light: '#fef9c3', dark: '#eab308' },
    wisdom: { light: '#ede9fe', dark: '#8b5cf6' },
    greed: { light: '#fde68a', dark: '#ca8a04' },
    insight: { light: '#e0f2fe', dark: '#0ea5e9' },
};

const FALLBACK = { light: '#ede9fe', dark: '#8b5cf6' };

export const relicColors = (key: string) => RELIC_COLORS[key] ?? FALLBACK;
export const relicAccent = (key: string) => relicColors(key).dark;
