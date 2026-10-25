import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import { ExpeditionBoard, ExpeditionClaimResult, ExpeditionInfo, PlayerResources } from '@/models/types/Expeditions';

const apiClient = axios.create({
    baseURL: 'https://localhost:7232',
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Экспедиции и ресурсы есть только у залогиненного игрока — он берётся из токена
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

class ExpeditionsApiService {

    async getBoard(): Promise<ExpeditionBoard> {
        try {
            const response = await apiClient.get<ExpeditionBoard>('/expeditions');
            return response.data;
        } catch (error) {
            console.error('Ошибка при загрузке экспедиций:', error);
            throw error;
        }
    }

    // heroIds[i] — герой в ячейке i + 1
    async start(expeditionId: string, heroIds: string[]): Promise<ExpeditionInfo> {
        try {
            const response = await apiClient.post<ExpeditionInfo>(`/expeditions/${expeditionId}/start`, { heroIds });
            return response.data;
        } catch (error) {
            console.error('Ошибка при отправке героев в экспедицию:', error);
            throw error;
        }
    }

    async claim(expeditionId: string): Promise<ExpeditionClaimResult> {
        try {
            const response = await apiClient.post<ExpeditionClaimResult>(`/expeditions/${expeditionId}/claim`);
            return response.data;
        } catch (error) {
            console.error('Ошибка при получении награды экспедиции:', error);
            throw error;
        }
    }

    async getResources(): Promise<PlayerResources> {
        try {
            const response = await apiClient.get<PlayerResources>('/player/me/resources');
            return response.data;
        } catch (error) {
            console.error('Ошибка при загрузке ресурсов:', error);
            throw error;
        }
    }
}

export default new ExpeditionsApiService();
