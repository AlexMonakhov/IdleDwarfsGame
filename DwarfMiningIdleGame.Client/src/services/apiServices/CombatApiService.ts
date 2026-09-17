import axios from 'axios';

export interface CombatStartRequest {
    zoneId: number;
    partyId?: number;
}

export interface CombatStartResponse {

    logs: object[];
    winningTeam: number;
    fightId: string;
}


const apiClient = axios.create({
    baseURL: 'https://localhost:7232', 
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

class CombatApiService {

    async start(): Promise<CombatStartResponse> {
        try {
            const response = await apiClient.post<CombatStartResponse>('/combat/start');
            return response.data;
        } catch (error) {
            console.error('Ошибка при инициализации боя:', error);
            throw error; 
        }
    }

    async getCombatEntities(): Promise<any> {
        try {
            const response = await apiClient.get('/combat/entities');
            return response.data;
        } catch (error) {
            console.error('Ошибка при получении сущностей боя:', error);
            throw error; 
        }
    }

    async getRewards(fightId: string): Promise<any> {
        try {
            const response = await apiClient.get(`/combat/rewards/${fightId}`);
            return response.data;
        } catch (error) {
            console.error('Ошибка при получении наград:', error);
            throw error; 
        }
    }
}

export default new CombatApiService();

