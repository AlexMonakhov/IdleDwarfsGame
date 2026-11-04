import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import {
    MineType,
    MineUpgrade,
    MiningActionResult,
    MiningBoard,
    MiningCollectResult,
    MiningDigResult,
} from '@/models/types/Mining';

const apiClient = axios.create({
    baseURL: 'https://localhost:7232',
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Шахты есть только у залогиненного игрока — он берётся из токена
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

class MiningApiService {

    async getBoard(): Promise<MiningBoard> {
        try {
            const response = await apiClient.get<MiningBoard>('/mining');
            return response.data;
        } catch (error) {
            console.error('Ошибка при загрузке шахт:', error);
            throw error;
        }
    }

    async build(plot: number, type: MineType): Promise<MiningActionResult> {
        try {
            const response = await apiClient.post<MiningActionResult>(`/mining/plots/${plot}/build`, { type });
            return response.data;
        } catch (error) {
            console.error('Ошибка при постройке шахты:', error);
            throw error;
        }
    }

    // clicks — клики с прошлой пачки; сервер засчитает не больше своего лимита
    async dig(mineId: string, clicks: number): Promise<MiningDigResult> {
        try {
            const response = await apiClient.post<MiningDigResult>(`/mining/${mineId}/dig`, { clicks });
            return response.data;
        } catch (error) {
            console.error('Ошибка при добыче в шахте:', error);
            throw error;
        }
    }

    async upgrade(mineId: string, upgrade: MineUpgrade): Promise<MiningActionResult> {
        try {
            const response = await apiClient.post<MiningActionResult>(`/mining/${mineId}/upgrade`, { upgrade });
            return response.data;
        } catch (error) {
            console.error('Ошибка при улучшении шахты:', error);
            throw error;
        }
    }

    async assignDwarf(mineId: string, heroId: string): Promise<MiningActionResult> {
        try {
            const response = await apiClient.put<MiningActionResult>(`/mining/${mineId}/dwarf`, { heroId });
            return response.data;
        } catch (error) {
            console.error('Ошибка при назначении дворфа в шахту:', error);
            throw error;
        }
    }

    async removeDwarf(mineId: string): Promise<MiningActionResult> {
        try {
            const response = await apiClient.delete<MiningActionResult>(`/mining/${mineId}/dwarf`);
            return response.data;
        } catch (error) {
            console.error('Ошибка при отзыве дворфа из шахты:', error);
            throw error;
        }
    }

    async collect(mineId: string): Promise<MiningCollectResult> {
        try {
            const response = await apiClient.post<MiningCollectResult>(`/mining/${mineId}/collect`);
            return response.data;
        } catch (error) {
            console.error('Ошибка при сборе добычи шахты:', error);
            throw error;
        }
    }
}

export default new MiningApiService();
