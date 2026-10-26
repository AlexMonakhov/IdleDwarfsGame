import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import { ChestOpenResult, InventoryInfo } from '@/models/types/Inventory';

const apiClient = axios.create({
    baseURL: 'https://localhost:7232',
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Сундуки и гемы есть только у залогиненного игрока — он берётся из токена
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

class InventoryApiService {

    async getMyInventory(): Promise<InventoryInfo> {
        try {
            const response = await apiClient.get<InventoryInfo>('/inventory/me');
            return response.data;
        } catch (error) {
            console.error('Ошибка при загрузке инвентаря:', error);
            throw error;
        }
    }

    // Что выпало, уже лежит в инвентаре: гемы — в стопках, герои — в коллекции
    async openChest(chestId: string): Promise<ChestOpenResult> {
        try {
            const response = await apiClient.post<ChestOpenResult>(`/inventory/me/chests/${chestId}/open`);
            return response.data;
        } catch (error) {
            console.error('Ошибка при открытии сундука:', error);
            throw error;
        }
    }
}

export default new InventoryApiService();
