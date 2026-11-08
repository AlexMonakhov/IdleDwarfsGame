import { API_BASE_URL } from './apiConfig';
import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import { AbilityActionResult, AbilityInfo, AbilityKind } from '@/models/types/Abilities';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Способности есть только у залогиненного игрока — он берётся из токена
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

class AbilitiesApiService {

    async getMyAbilities(): Promise<AbilityInfo[]> {
        try {
            const response = await apiClient.get<AbilityInfo[]>('/abilities/me');
            return response.data;
        } catch (error) {
            console.error('Ошибка при загрузке способностей:', error);
            throw error;
        }
    }

    // Дворф распыляется, выбранная способность остаётся у игрока
    async extract(heroId: string, kind: AbilityKind, key: string): Promise<AbilityActionResult> {
        try {
            const response = await apiClient.post<AbilityActionResult>('/abilities/extract', { heroId, kind, key });
            return response.data;
        } catch (error) {
            console.error('Ошибка при извлечении способности:', error);
            throw error;
        }
    }

    // Способность заменяет такую же по виду у дворфа. В ответе — дворф уже с ней
    async insert(abilityId: string, heroId: string): Promise<AbilityActionResult> {
        try {
            const response = await apiClient.post<AbilityActionResult>(`/abilities/${abilityId}/insert`, { heroId });
            return response.data;
        } catch (error) {
            console.error('Ошибка при вставке способности:', error);
            throw error;
        }
    }

    async combine(firstId: string, secondId: string): Promise<AbilityActionResult> {
        try {
            const response = await apiClient.post<AbilityActionResult>('/abilities/combine', { firstId, secondId });
            return response.data;
        } catch (error) {
            console.error('Ошибка при слиянии способностей:', error);
            throw error;
        }
    }

    // Пять дворфов одного ранга — один рангом выше. В ответе — новый дворф
    async fuse(heroIds: string[]): Promise<AbilityActionResult> {
        try {
            const response = await apiClient.post<AbilityActionResult>('/abilities/fuse', { heroIds });
            return response.data;
        } catch (error) {
            console.error('Ошибка при слиянии дворфов:', error);
            throw error;
        }
    }
}

export default new AbilitiesApiService();
