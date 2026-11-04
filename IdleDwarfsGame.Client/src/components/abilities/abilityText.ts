import { AxiosError } from 'axios';
import { i18n, statusLabel, t } from '@/i18n';
import { AbilityError, AbilityInfo } from '@/models/types/Abilities';

// Отказ сервера — переведённый текст по коду, иначе общий
export const abilityErrorText = (error: unknown): string => {
    const code = (error as AxiosError<{ code?: AbilityError }>).response?.data?.code;
    return code && i18n.global.te(`abilities.errors.${code}`) ? t(`abilities.errors.${code}`) : t('abilities.actionError');
};

// Название способности: заклинание приходит с сервера по-английски, как и в hero.spells
export const abilityName = (ability: AbilityInfo): string => {
    switch (ability.kind) {
        case 'Spell':
            return ability.spellName ?? ability.key;
        case 'Passive':
            return i18n.global.te(`passives.names.${ability.key}`) ? t(`passives.names.${ability.key}`) : ability.key;
        default:
            return t('abilities.auraName', { effect: statusLabel(ability.key) });
    }
};

// Что способность делает на своём уровне
export const abilityText = (ability: AbilityInfo): string => {
    switch (ability.kind) {
        case 'Spell':
            return ability.element ? t('abilities.spellText', { element: t(`elements.${ability.element}`) }) : '';
        case 'Passive':
            return t(`passives.descriptions.${ability.key}`, {
                percent: ability.percent,
                turns: ability.turns,
                targets: ability.targets,
            });
        default:
            return t('abilities.auraText', {
                effect: statusLabel(ability.key),
                power: ability.power,
                turns: ability.duration,
                target: ability.auraTarget ? t(`equipment.auraTargets.${ability.auraTarget}`) : '',
            });
    }
};

// Одна и та же способность — для поиска пары к слиянию и сравнения с тем, что уже есть у дворфа
export const sameAbility = (a: AbilityInfo, b: AbilityInfo) => a.kind === b.kind && a.key === b.key;
