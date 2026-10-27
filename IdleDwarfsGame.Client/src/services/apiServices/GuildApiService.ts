import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import {
    GuildBossAttackResult,
    GuildBossesOverview,
    GuildBossTier,
    GuildOverview,
    GuildSearchResult
} from '@/models/types/Guild';
import { RewardClaimResult } from '@/models/types/Inventory';

const apiClient = axios.create({
    baseURL: 'https://localhost:7232',
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Гильдия есть только у залогиненного игрока — он берётся из токена
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// Ошибки приходят как { code, message }: по code фронт показывает переведённый текст (guild.errors.<code>)
class GuildApiService {

    async getOverview(): Promise<GuildOverview> {
        try {
            const response = await apiClient.get<GuildOverview>('/guilds/me');
            return response.data;
        } catch (error) {
            console.error('Ошибка при загрузке гильдии:', error);
            throw error;
        }
    }

    // Пустой запрос — самые большие гильдии
    async search(query: string): Promise<GuildSearchResult[]> {
        try {
            const response = await apiClient.get<GuildSearchResult[]>('/guilds/search', { params: { query } });
            return response.data;
        } catch (error) {
            console.error('Ошибка при поиске гильдий:', error);
            throw error;
        }
    }

    async create(name: string): Promise<GuildOverview> {
        try {
            const response = await apiClient.post<GuildOverview>('/guilds', { name });
            return response.data;
        } catch (error) {
            console.error('Ошибка при создании гильдии:', error);
            throw error;
        }
    }

    async leave(): Promise<void> {
        try {
            await apiClient.post('/guilds/leave');
        } catch (error) {
            console.error('Ошибка при выходе из гильдии:', error);
            throw error;
        }
    }

    async sendRequest(guildId: string): Promise<void> {
        try {
            await apiClient.post(`/guilds/${guildId}/requests`);
        } catch (error) {
            console.error('Ошибка при отправке заявки в гильдию:', error);
            throw error;
        }
    }

    // Отозвать свою заявку
    async cancelRequest(guildId: string): Promise<void> {
        try {
            await apiClient.delete(`/guilds/${guildId}/requests`);
        } catch (error) {
            console.error('Ошибка при отзыве заявки в гильдию:', error);
            throw error;
        }
    }

    async accept(userId: string): Promise<void> {
        try {
            await apiClient.post(`/guilds/requests/${userId}/accept`);
        } catch (error) {
            console.error('Ошибка при принятии заявки в гильдию:', error);
            throw error;
        }
    }

    async decline(userId: string): Promise<void> {
        try {
            await apiClient.post(`/guilds/requests/${userId}/decline`);
        } catch (error) {
            console.error('Ошибка при отклонении заявки в гильдию:', error);
            throw error;
        }
    }

    async kick(userId: string): Promise<void> {
        try {
            await apiClient.delete(`/guilds/members/${userId}`);
        } catch (error) {
            console.error('Ошибка при исключении из гильдии:', error);
            throw error;
        }
    }

    async getBosses(): Promise<GuildBossesOverview> {
        try {
            const response = await apiClient.get<GuildBossesOverview>('/guilds/bosses');
            return response.data;
        } catch (error) {
            console.error('Ошибка при загрузке боссов гильдии:', error);
            throw error;
        }
    }

    // Бой с длинным логом: таймаут больше обычного
    async attack(tier: GuildBossTier): Promise<GuildBossAttackResult> {
        try {
            const response = await apiClient.post<GuildBossAttackResult>(`/guilds/bosses/${tier}/attack`, null, { timeout: 15000 });
            return response.data;
        } catch (error) {
            console.error('Ошибка при атаке босса гильдии:', error);
            throw error;
        }
    }

    // Награда за убитого босса — сразу на аккаунт
    async claimReward(tier: GuildBossTier): Promise<RewardClaimResult> {
        try {
            const response = await apiClient.post<RewardClaimResult>(`/guilds/bosses/${tier}/claim`);
            return response.data;
        } catch (error) {
            console.error('Ошибка при получении награды за босса:', error);
            throw error;
        }
    }
}

export default new GuildApiService();
