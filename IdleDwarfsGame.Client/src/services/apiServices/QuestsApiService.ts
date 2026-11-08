import { API_BASE_URL } from './apiConfig';
import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import { QuestBoard, QuestClaimResult } from '@/models/types/Quests';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Доска заданий у каждого игрока своя — он берётся из токена
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

class QuestsApiService {
    async get(): Promise<QuestBoard> {
        const response = await apiClient.get<QuestBoard>('/quests');
        return response.data;
    }

    async claim(questId: string): Promise<QuestClaimResult> {
        const response = await apiClient.post<QuestClaimResult>(`/quests/${questId}/claim`);
        return response.data;
    }

    // Раз в сутки бесплатно, дальше за радужные монеты
    async reroll(questId: string): Promise<QuestBoard> {
        const response = await apiClient.post<QuestBoard>(`/quests/${questId}/reroll`);
        return response.data;
    }

    async claimMilestone(index: number): Promise<QuestClaimResult> {
        const response = await apiClient.post<QuestClaimResult>(`/quests/milestones/${index}/claim`);
        return response.data;
    }
}

export default new QuestsApiService();
