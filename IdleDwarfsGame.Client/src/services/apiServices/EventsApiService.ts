import { API_BASE_URL } from './apiConfig';
import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import type {
    CommunityMinesClaimResult,
    CommunityMinesOverview,
    WorldBossAttackResult,
    WorldBossClaimResult,
    WorldBossOverview,
} from '@/models/types/WorldEvents';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// Активности ивентов недели: мировые боссы и общие шахты
class EventsApiService {
    async getWorldBoss(): Promise<WorldBossOverview> {
        const response = await apiClient.get<WorldBossOverview>('/events/world-boss');
        return response.data;
    }

    // Бой идёт на сервере целиком — ответ несёт лог для арены
    async attackWorldBoss(): Promise<WorldBossAttackResult> {
        const response = await apiClient.post<WorldBossAttackResult>('/events/world-boss/attack', undefined, { timeout: 20000 });
        return response.data;
    }

    async claimWorldBoss(): Promise<WorldBossClaimResult> {
        const response = await apiClient.post<WorldBossClaimResult>('/events/world-boss/claim');
        return response.data;
    }

    async getCommunityMines(): Promise<CommunityMinesOverview> {
        const response = await apiClient.get<CommunityMinesOverview>('/events/community-mines');
        return response.data;
    }

    async sendDwarves(depositId: string, heroIds: string[]): Promise<CommunityMinesOverview> {
        const response = await apiClient.post<CommunityMinesOverview>(`/events/community-mines/${depositId}/dwarves`, { heroIds });
        return response.data;
    }

    async recallDwarf(depositId: string, heroId: string): Promise<CommunityMinesOverview> {
        const response = await apiClient.post<CommunityMinesOverview>(`/events/community-mines/${depositId}/dwarves/${heroId}/recall`);
        return response.data;
    }

    async upgradeDeposit(depositId: string): Promise<CommunityMinesOverview> {
        const response = await apiClient.post<CommunityMinesOverview>(`/events/community-mines/${depositId}/upgrade`);
        return response.data;
    }

    async claimCommunityMines(): Promise<CommunityMinesClaimResult> {
        const response = await apiClient.post<CommunityMinesClaimResult>('/events/community-mines/claim');
        return response.data;
    }
}

export default new EventsApiService();
