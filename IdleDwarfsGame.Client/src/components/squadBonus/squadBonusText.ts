import { t } from '@/i18n';
import { SPELL_COLORS } from '@/components/icons/spells/spellColors';
import { BonusElement, MONO_VALUES, SquadBonus, tierNumeral } from '@/battle/squadBonuses';

// Тексты и цвета бонусов стихий отряда: имя, описание с числами уровня, цвет стихии

// Цвет стихии — как у иконок заклинаний: светлее камней, читается на тёмном
export const bonusElementColor = (element: BonusElement): string => SPELL_COLORS[element];

// Цвет бонуса для заголовков: у моно — стихия, у пары — главная (первая по порядку), у призмы — радужный акцент
export const PRISM_COLOR = '#f9a8d4';
export const bonusColor = (bonus: SquadBonus): string =>
    bonus.elements.length ? bonusElementColor(bonus.elements[0]) : PRISM_COLOR;

export const elementLabel = (element: BonusElement): string => t(`elementNames.${element}`);

// «Неугасимое пламя III»; у пары и призмы уровня нет
export const bonusName = (bonus: SquadBonus, withTier = true): string => {
    const name = t(`squadBonuses.names.${bonus.id}`);
    return withTier && bonus.shape === 'Mono' ? `${name} ${tierNumeral(bonus.tier)}` : name;
};

const formatParam = (key: string, value: number): string => {
    if (key !== 'every') return String(value);
    return value === 1 ? t('squadBonuses.everyCast') : t('squadBonuses.everyNthCast', { n: value });
};

// Число для всех уровней: «50 / 80 / 90». Если слово меняется вместе с числом («каждый 2-й каст» / «каждый каст»),
// одинаковые уровни сливаются: «Каждый 2-й каст (I, II) / Каждый каст (III)»
const formatAllTiers = (key: string, byTier: number[]): string => {
    if (key !== 'every') return [...new Set(byTier)].length === 1 ? String(byTier[0]) : byTier.join(' / ');
    const groups: { value: number; tiers: number[] }[] = [];
    byTier.forEach((value, i) => {
        const last = groups[groups.length - 1];
        if (last?.value === value) last.tiers.push(i + 1);
        else groups.push({ value, tiers: [i + 1] });
    });
    if (groups.length === 1) return formatParam(key, groups[0].value);
    return groups.map(g => `${formatParam(key, g.value)} (${g.tiers.map(tierNumeral).join(', ')})`).join(' / ');
};

// Описание с числами уровня бонуса; allTiers — все три уровня через «/» (каталог в коллекции)
export const bonusDescription = (bonus: SquadBonus, allTiers = false): string => {
    const key = `squadBonuses.descriptions.${bonus.id}`;
    if (bonus.shape !== 'Mono') return t(key);
    const values = MONO_VALUES[bonus.elements[0]];
    const params: Record<string, string> = {};
    for (const [name, byTier] of Object.entries(values)) {
        params[name] = allTiers ? formatAllTiers(name, byTier) : formatParam(name, byTier[bonus.tier - 1]);
    }
    return t(key, params);
};
