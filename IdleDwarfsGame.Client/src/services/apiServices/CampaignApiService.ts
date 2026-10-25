import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import { CampaignFightResult, CampaignOverview } from '@/models/types/Campaign';

const apiClient = axios.create({
    baseURL: 'https://localhost:7232',
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Прогресс кампании привязан к игроку из токена; без токена сервер берёт демо-игрока
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

class CampaignApiService {

    async getCampaign(): Promise<CampaignOverview> {
        try {
            const response = await apiClient.get<CampaignOverview>('/campaign');
            return response.data;
        } catch (error) {
            console.error('Ошибка при загрузке кампании:', error);
            throw error;
        }
    }

    async fight(stageId: number): Promise<CampaignFightResult> {
        try {
            const response = await apiClient.post<CampaignFightResult>(`/campaign/${stageId}/fight`);
            return response.data;
        } catch (error) {
            console.error('Ошибка при запуске боя кампании:', error);
            throw error;
        }
    }
}

export default new CampaignApiService();
