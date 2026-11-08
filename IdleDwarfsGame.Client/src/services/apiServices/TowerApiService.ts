import { API_BASE_URL } from './apiConfig';
import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import { TowerFightResult, TowerOverview, TowerSkipResult } from '@/models/types/Tower';
import { RewardClaimAllResult, RewardClaimResult } from '@/models/types/Inventory';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    // Обзор башни прогоняет прогноз боя для пяти этажей
    timeout: 10000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Прогресс башни привязан к игроку из токена; без токена сервер берёт демо-игрока
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

class TowerApiService {

    async getTower(): Promise<TowerOverview> {
        try {
            const response = await apiClient.get<TowerOverview>('/tower');
            return response.data;
        } catch (error) {
            console.error('Ошибка при загрузке башни:', error);
            throw error;
        }
    }

    async fight(floor: number): Promise<TowerFightResult> {
        try {
            const response = await apiClient.post<TowerFightResult>(`/tower/${floor}/fight`);
            return response.data;
        } catch (error) {
            console.error('Ошибка при запуске боя башни:', error);
            throw error;
        }
    }

    // Пропуск до лучшего этажа прошлых недель: награда за все пропущенные этажи сразу на аккаунте
    async skip(): Promise<TowerSkipResult> {
        try {
            const response = await apiClient.post<TowerSkipResult>('/tower/skip');
            return response.data;
        } catch (error) {
            console.error('Ошибка при пропуске этажей башни:', error);
            throw error;
        }
    }

    async claimReward(rewardId: string): Promise<RewardClaimResult> {
        try {
            const response = await apiClient.post<RewardClaimResult>(`/tower/rewards/${rewardId}/claim`);
            return response.data;
        } catch (error) {
            console.error('Ошибка при получении награды башни:', error);
            throw error;
        }
    }

    // Все незабранные награды башни (и прошлых недель) разом
    async claimAll(): Promise<RewardClaimAllResult> {
        try {
            const response = await apiClient.post<RewardClaimAllResult>('/tower/rewards/claim-all');
            return response.data;
        } catch (error) {
            console.error('Ошибка при получении наград башни:', error);
            throw error;
        }
    }
}

export default new TowerApiService();
