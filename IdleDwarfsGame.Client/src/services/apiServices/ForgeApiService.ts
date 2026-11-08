import { API_BASE_URL } from './apiConfig';
import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import type { Rarity } from '@/models/types/Inventory';
import { ForgeGemResult, ForgeItemResult, ForgeOverview } from '@/models/types/Forge';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Предметы и камни — у залогиненного игрока, он берётся из токена
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

class ForgeApiService {
    async get(): Promise<ForgeOverview> {
        const response = await apiClient.get<ForgeOverview>('/forge');
        return response.data;
    }

    // Ровно itemFusionSize снятых предметов одной редкости
    async fuseItems(equipmentIds: string[]): Promise<ForgeItemResult> {
        const response = await apiClient.post<ForgeItemResult>('/forge/items', { equipmentIds });
        return response.data;
    }

    // times раз по gemFusionSize камней этой ступени — в times камней ступенью выше
    async fuseGems(gem: string, rarity: Rarity, times: number): Promise<ForgeGemResult> {
        const response = await apiClient.post<ForgeGemResult>('/forge/gems', { gem, rarity, times });
        return response.data;
    }
}

export default new ForgeApiService();
