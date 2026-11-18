import { t } from '@/i18n';
import { MONO_COUNTS, SquadBonus } from '@/battle/squadBonuses';
import { elementLabel } from './squadBonusText';

// Какой состав даёт бонус: «4 × Огонь», «3 + 2: Огонь и Лёд», у призмы — «5 разных стихий»
export const bonusComposition = (bonus: SquadBonus): string => {
    const part = (count: number, index: number) =>
        t('squadBonuses.composition', { count, element: elementLabel(bonus.elements[index]) });
    if (bonus.shape === 'Mono') return part(MONO_COUNTS[bonus.tier - 1], 0);
    if (bonus.shape === 'Pair') {
        return t('squadBonuses.pairComposition', { a: elementLabel(bonus.elements[0]), b: elementLabel(bonus.elements[1]) });
    }
    return t('squadBonuses.rules.prism');
};
