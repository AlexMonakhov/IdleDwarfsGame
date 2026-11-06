import { AxiosError } from 'axios';
import { i18n } from '@/i18n';

// Текст отказа кузницы: по коду (ForgeError) — перевод, иначе сообщение сервера
export const forgeErrorText = (error: unknown): string => {
    const { t, te } = i18n.global;
    const data = (error as AxiosError<{ code?: string; message?: string }>).response?.data;
    if (data?.code && te(`forge.errors.${data.code}`)) return t(`forge.errors.${data.code}`);
    return data?.message ?? t('forge.actionError');
};
