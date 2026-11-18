import { API_BASE_URL, attachErrorSound } from './apiConfig';
import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import type { AchievementBoard, AchievementClaimResult, LoginCalendarInfo, LoginClaimResult } from '@/models/types/Achievements';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});
attachErrorSound(apiClient);

// Достижения и календарь у каждого игрока свои — он берётся из токена
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

class AchievementsApiService {
    async get(): Promise<AchievementBoard> {
        const response = await apiClient.get<AchievementBoard>('/achievements');
        return response.data;
    }

    // Все достигнутые ступени достижения сразу
    async claim(key: string): Promise<AchievementClaimResult> {
        const response = await apiClient.post<AchievementClaimResult>(`/achievements/${encodeURIComponent(key)}/claim`);
        return response.data;
    }

    async claimAll(): Promise<AchievementClaimResult> {
        const response = await apiClient.post<AchievementClaimResult>('/achievements/claim-all');
        return response.data;
    }

    async getCalendar(): Promise<LoginCalendarInfo> {
        const response = await apiClient.get<LoginCalendarInfo>('/login-calendar');
        return response.data;
    }

    async claimCalendar(): Promise<LoginClaimResult> {
        const response = await apiClient.post<LoginClaimResult>('/login-calendar/claim');
        return response.data;
    }
}

export default new AchievementsApiService();
