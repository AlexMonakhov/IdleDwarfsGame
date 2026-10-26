import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import { HeroInfo, HeroUpgradeResult } from '@/models/types/Heroes';

const apiClient = axios.create({
    baseURL: 'https://localhost:7232',
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Инвентарь есть только у залогиненного игрока — он берётся из токена
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

class HeroesApiService {

    async getMyHeroes(): Promise<HeroInfo[]> {
        try {
            const response = await apiClient.get<HeroInfo[]>('/heroes/me');
            return response.data;
        } catch (error) {
            console.error('Ошибка при загрузке героев:', error);
            throw error;
        }
    }

    // slots[i] — герой на месте i + 1, null — пустой слот. Возвращает всех героев с новыми местами
    async saveSquad(slots: (string | null)[]): Promise<HeroInfo[]> {
        try {
            const response = await apiClient.put<HeroInfo[]>('/heroes/me/squad', { slots });
            return response.data;
        } catch (error) {
            console.error('Ошибка при сохранении отряда:', error);
            throw error;
        }
    }

    // +1 уровень прокачки героя. Возвращает обновлённого героя и остаток ресурсов
    async upgrade(heroId: string): Promise<HeroUpgradeResult> {
        try {
            const response = await apiClient.post<HeroUpgradeResult>(`/heroes/me/${heroId}/upgrade`);
            return response.data;
        } catch (error) {
            console.error('Ошибка при прокачке героя:', error);
            throw error;
        }
    }
}

export default new HeroesApiService();
