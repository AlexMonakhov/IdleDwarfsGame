import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import { PvpFightResult, PvpOverview } from '@/models/types/Pvp';

const apiClient = axios.create({
    baseURL: 'https://localhost:7232',
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// PvP есть только у залогиненного игрока — он берётся из токена
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// Ошибки приходят как { code, message }: по code фронт показывает переведённый текст (pvp.errors.<code>)
class PvpApiService {

    // Сервер прогоняет прогноз боя с каждым соперником — таймаут больше обычного
    async getOverview(): Promise<PvpOverview> {
        try {
            const response = await apiClient.get<PvpOverview>('/pvp/me', { timeout: 15000 });
            return response.data;
        } catch (error) {
            console.error('Ошибка при загрузке PvP:', error);
            throw error;
        }
    }

    // Бой с длинным логом: таймаут больше обычного
    async fight(opponentUserId: string): Promise<PvpFightResult> {
        try {
            const response = await apiClient.post<PvpFightResult>(`/pvp/opponents/${opponentUserId}/fight`, null, { timeout: 15000 });
            return response.data;
        } catch (error) {
            console.error('Ошибка при бое PvP:', error);
            throw error;
        }
    }
}

export default new PvpApiService();
