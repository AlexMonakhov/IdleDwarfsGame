import i18n, { chestLabel } from '@/i18n';
import type { GrantedReward } from '@/models/types/Achievements';

// Что получено — одной строкой для сообщения под заголовком окна
export const grantedText = (granted: GrantedReward): string => {
    const { t, locale } = i18n.global;
    const format = (value: number) => value.toLocaleString(locale.value);
    const parts: string[] = [];
    if (granted.rainbowCoins) parts.push(t('achievements.reward.rainbow', { amount: format(granted.rainbowCoins) }));
    if (granted.elderSeals) parts.push(t('quests.reward.seals', { amount: format(granted.elderSeals) }));
    if (granted.reward.gold) parts.push(t('quests.reward.gold', { amount: format(granted.reward.gold) }));
    if (granted.reward.experience) parts.push(t('quests.reward.experience', { amount: format(granted.reward.experience) }));
    parts.push(...granted.reward.chests.map(chest => chestLabel(chest)));
    parts.push(...granted.frames.map(frame => t('achievements.frameReward', { frame: t(`account.frames.${frame}`) })));
    return parts.join(', ');
};
