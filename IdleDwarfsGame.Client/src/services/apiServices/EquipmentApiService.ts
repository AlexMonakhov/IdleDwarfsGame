import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import { EquipmentChangeResult, EquipmentInfo, EquipmentSlot } from '@/models/types/Equipment';

const apiClient = axios.create({
    baseURL: 'https://localhost:7232',
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Снаряжение есть только у залогиненного игрока — он берётся из токена
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

class EquipmentApiService {

    async getMyEquipment(): Promise<EquipmentInfo[]> {
        try {
            const response = await apiClient.get<EquipmentInfo[]>('/equipment/me');
            return response.data;
        } catch (error) {
            console.error('Ошибка при загрузке снаряжения:', error);
            throw error;
        }
    }

    // slot не указан — сервер выберет подходящий (для колец — свободный)
    async equip(itemId: string, heroId: string, slot?: EquipmentSlot): Promise<EquipmentChangeResult> {
        try {
            const response = await apiClient.post<EquipmentChangeResult>(`/equipment/me/${itemId}/equip`, { heroId, slot: slot ?? null });
            return response.data;
        } catch (error) {
            console.error('Ошибка при надевании предмета:', error);
            throw error;
        }
    }

    async unequip(itemId: string): Promise<EquipmentChangeResult> {
        try {
            const response = await apiClient.post<EquipmentChangeResult>(`/equipment/me/${itemId}/unequip`);
            return response.data;
        } catch (error) {
            console.error('Ошибка при снятии предмета:', error);
            throw error;
        }
    }
}

export default new EquipmentApiService();
