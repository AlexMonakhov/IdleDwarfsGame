import { AxiosError } from 'axios';
import { i18n } from '@/i18n';

// Выполняет действие гильдии: блокирует кнопки по key, показывает итог в футере окна и перечитывает гильдию.
// Возвращает true, если действие прошло
export type GuildRun = (key: string, action: () => Promise<string | void>) => Promise<boolean>;

// Сервер отвечает { code, message } — показываем перевод по коду, иначе его сообщение
export const guildErrorText = (error: unknown, fallback: string) => {
    const { t, te } = i18n.global;
    const data = (error as AxiosError<{ code?: string; message?: string }>).response?.data;
    if (data?.code && te(`guild.errors.${data.code}`)) return t(`guild.errors.${data.code}`);
    return data?.message ?? fallback;
};

// 1:05:09 / 4:07
export const formatTime = (ms: number) => {
    const total = Math.max(0, Math.ceil(ms / 1000));
    const h = Math.floor(total / 3600);
    const m = Math.floor((total % 3600) / 60);
    const s = total % 60;
    const pad = (n: number) => String(n).padStart(2, '0');
    return h > 0 ? `${h}:${pad(m)}:${pad(s)}` : `${m}:${pad(s)}`;
};
