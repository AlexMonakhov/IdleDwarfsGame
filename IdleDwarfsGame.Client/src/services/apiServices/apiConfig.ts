import type { AxiosError, AxiosInstance } from 'axios';
import { soundService } from '@/services/soundService/SoundService';

// Адрес сервера API. Локально фронт (vue-cli, :8080) и сервер (:7232) на разных портах — адрес берётся из .env.development.
// В продакшен-сборке переменной нет: фронт раздаёт сам сервер, и запросы идут на тот же домен относительными путями
export const API_BASE_URL: string = process.env.VUE_APP_API_URL ?? '';

// Звук ошибки, когда действие игрока не удалось (сервер отказал или недоступен). Только запросы-действия:
// чтение (GET) и фоновые запросы (сессия вкладки, обучение, отметка письма прочитанным) молчат
const SILENT_ERROR_URLS = [/^\/sessions/, /^\/tutorial/, /\/read$/];

export function attachErrorSound(client: AxiosInstance) {
    client.interceptors.response.use(undefined, (error: AxiosError) => {
        const method = error.config?.method?.toLowerCase();
        const url = error.config?.url ?? '';
        if (method && method !== 'get' && !SILENT_ERROR_URLS.some(pattern => pattern.test(url))) {
            soundService.play('ui_error');
        }
        return Promise.reject(error);
    });
}
