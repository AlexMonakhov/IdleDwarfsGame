import { i18n, statusLabel, t } from '@/i18n';
import { HeroInfo } from '@/models/types/Heroes';
import { EquipmentInfo, ItemEffectInfo, ItemStats, PassiveInfo } from '@/models/types/Equipment';

// Новый предмет на сервере без перевода показывается по ключу, а не падает
export const itemName = (item: EquipmentInfo): string =>
    i18n.global.te(`equipment.items.${item.key}`) ? t(`equipment.items.${item.key}`) : item.key;

// Срабатывание с шансом меньше 100% описывается своим текстом: equipment.effects.<kind>Chance
const effectKey = (effect: ItemEffectInfo): string =>
    effect.chance != null && effect.chance < 100 ? `${effect.kind}Chance` : effect.kind;

export const effectText = (effect: ItemEffectInfo): string => t(`equipment.effects.${effectKey(effect)}`, {
    value: effect.value,
    chance: effect.chance ?? 100,
    spell: effect.spell ?? '',
    proc: effect.procSpell ?? '',
    level: effect.procLevel ?? '',
    passive: effect.passive ? t(`passives.names.${effect.passive}`) : '',
    aura: effect.aura ? statusLabel(effect.aura) : '',
    target: effect.auraTarget ? t(`equipment.auraTargets.${effect.auraTarget}`) : '',
});

// Есть ли у героя то, что эффект усиливает. Без этого предмет надеть можно, но эффект не сработает.
// В hero.spells спеллы лежат строками вида «Flame Wave 2»
export const effectActive = (effect: ItemEffectInfo, hero: HeroInfo): boolean => {
    if (effect.spell) return hero.spells.some(spell => spell.startsWith(`${effect.spell} `));
    if (effect.passive) return (hero.passives ?? []).some(p => p.passive === effect.passive);
    if (effect.kind.startsWith('Aura') && effect.kind !== 'AuraGrant') return !!hero.aura;
    return true;
};

export const passiveName = (passive: PassiveInfo): string => t(`passives.names.${passive.passive}`);

export const passiveText = (passive: PassiveInfo): string => t(`passives.descriptions.${passive.passive}`, {
    percent: passive.percent,
    turns: passive.turns,
    targets: passive.targets,
});

export const STAT_KEYS: (keyof ItemStats)[] = ['attack', 'maxHp', 'speed', 'maxEnergy', 'startEnergy', 'energyPerAttack'];

export const itemStatLines = (stats: ItemStats) => STAT_KEYS
    .filter(key => stats[key] !== 0)
    .map(key => ({ key, value: stats[key] }));
