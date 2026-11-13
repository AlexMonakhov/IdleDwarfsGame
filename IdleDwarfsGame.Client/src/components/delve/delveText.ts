import { i18n, t } from '@/i18n';
import type { SpellDetail } from '@/models/types/Heroes';

// Описание заклинания из коллекции: collection.spellDescriptions.<имя в camelCase> («Flame Wave» → flameWave)
export const spellDescription = (spellName: string | null | undefined): string => {
    if (!spellName) return '';
    const words = spellName.split(/\s+/).filter(Boolean);
    const key = words.map((word, i) => (i === 0 ? word.toLowerCase() : word[0].toUpperCase() + word.slice(1).toLowerCase())).join('');
    return i18n.global.te(`collection.spellDescriptions.${key}`) ? t(`collection.spellDescriptions.${key}`) : '';
};

// Урон и лечение каста, как в панели героя алтаря
export const spellPower = (detail: SpellDetail | null | undefined): string => {
    if (!detail) return '';
    const parts: string[] = [];
    if (detail.damage > 0) {
        parts.push(detail.targets > 1
            ? t('heroUpgrade.spellDamageTargets', { damage: Math.round(detail.damage), targets: detail.targets })
            : t('heroUpgrade.spellDamage', { damage: Math.round(detail.damage) }));
    }
    if (detail.healing > 0) parts.push(t('heroUpgrade.spellHealing', { healing: Math.round(detail.healing) }));
    parts.push(t('heroUpgrade.spellCost', { cost: detail.energyCost }));
    return parts.join(' · ');
};

// Реликвии складываются: ключ → число копий, в порядке первого получения
export const groupRelics = (relics: string[]): { key: string; count: number }[] => {
    const counts = new Map<string, number>();
    relics.forEach(key => counts.set(key, (counts.get(key) ?? 0) + 1));
    return [...counts].map(([key, count]) => ({ key, count }));
};
