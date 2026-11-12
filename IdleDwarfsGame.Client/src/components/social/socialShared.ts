import { AxiosError } from 'axios';
import type { Router } from 'vue-router';
import { i18n } from '@/i18n';
import { useExternalFight, type ExternalFight } from '@/composables/UseExternalFight';

// Сервер отвечает { code, message } — перевод по коду из social.<section>.errors, иначе его сообщение
export const socialErrorText = (error: unknown, section: string, fallback?: string) => {
    const { t, te } = i18n.global;
    const data = (error as AxiosError<{ code?: string; message?: string } | string>).response?.data;
    if (typeof data === 'string') return data;
    if (data?.code && te(`social.${section}.errors.${data.code}`)) return t(`social.${section}.errors.${data.code}`);
    return data?.message ?? fallback ?? t('social.actionError');
};

// Открыть бой на арене (внешний режим, см. UseExternalFight)
export const openExternalFight = (router: Router, fight: ExternalFight) => {
    useExternalFight().pendingFight.value = fight;
    return router.push({ name: 'arena', query: { external: '1', attack: Date.now() } });
};

export const formatNumber = (value: number) => Math.floor(value).toLocaleString(i18n.global.locale.value);

// 1:05:09 / 4:07 / 2д 3ч
export const formatLeft = (ms: number) => {
    const total = Math.max(0, Math.ceil(ms / 1000));
    const d = Math.floor(total / 86400);
    const h = Math.floor((total % 86400) / 3600);
    const m = Math.floor((total % 3600) / 60);
    const s = total % 60;
    const pad = (n: number) => String(n).padStart(2, '0');
    if (d > 0) return i18n.global.t('social.daysHours', { d, h });
    return h > 0 ? `${h}:${pad(m)}:${pad(s)}` : `${m}:${pad(s)}`;
};

// Цвет имени героя по стихии (для текста; спрайтовые цвета — battle/sprites)
const ELEMENT_TEXT: Record<string, string> = {
    Basic: '#e2e8f0',
    Fire: '#f87171',
    Frost: '#7dd3fc',
    Lightning: '#fde047',
    Nature: '#86efac',
    Dark: '#c4b5fd',
    Radiance: '#fefce8',
    Blood: '#fb7185',
};

export const elementText = (element?: string | null) => ELEMENT_TEXT[element ?? 'Basic'] ?? ELEMENT_TEXT.Basic;
