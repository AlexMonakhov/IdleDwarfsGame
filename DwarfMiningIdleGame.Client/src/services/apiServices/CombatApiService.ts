import axios from 'axios';

export interface CombatStartRequest {
    zoneId: number;
    partyId?: number;
}

export interface CombatStartResponse {

    logs: object[];
    winningTeam: number;
}


const apiClient = axios.create({
    baseURL: 'https://localhost:7232', 
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

export const CombatApiService = {
    async start(): Promise<CombatStartResponse> {
        try {
            const response = await apiClient.post<CombatStartResponse>('/combat/start');
            return response.data;
        } catch (error) {
            console.error('Ошибка при инициализации боя:', error);
            throw error; 
        }
    }
};