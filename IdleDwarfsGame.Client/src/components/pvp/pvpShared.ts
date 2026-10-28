import { AxiosError } from 'axios';
import { i18n } from '@/i18n';

// Сервер отвечает { code, message } — показываем перевод по коду, иначе его сообщение
export const pvpErrorText = (error: unknown, fallback: string) => {
    const { t, te } = i18n.global;
    const data = (error as AxiosError<{ code?: string; message?: string }>).response?.data;
    if (data?.code && te(`pvp.errors.${data.code}`)) return t(`pvp.errors.${data.code}`);
    return data?.message ?? fallback;
};

// До конца сезона: «3 д 4 ч» — дальше суток, иначе таймер 4:07:09
export const formatSeasonLeft = (ms: number, formatTime: (ms: number) => string) => {
    const { t } = i18n.global;
    const hours = Math.floor(Math.max(0, ms) / 3_600_000);
    if (hours < 24) return formatTime(ms);
    return t('pvp.daysLeft', { days: Math.floor(hours / 24), hours: hours % 24 });
};

// Очки со знаком: +24 / −12
export const signed = (value: number) => (value > 0 ? `+${value}` : value < 0 ? `−${Math.abs(value)}` : '0');
