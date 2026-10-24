import axios from 'axios';
import { CombatResult } from '@/battle/types';

export type { CombatLogEntry, CombatResult, CombatUnitInfo } from '@/battle/types';

export interface CombatStartRequest {
    playerId?: string;
    floor?: number;
}

const apiClient = axios.create({
    baseURL: 'https://localhost:7232',
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

class CombatApiService {

    async start(request: CombatStartRequest = {}): Promise<CombatResult> {
        try {
            const response = await apiClient.post<CombatResult>('/combat/start', null, { params: request });
            return response.data;
        } catch (error) {
            console.error('Ошибка при инициализации боя:', error);
            throw error;
        }
    }

    async startBoss(playerId?: string, difficulty = 1): Promise<CombatResult> {
        try {
            const response = await apiClient.post<CombatResult>('/combat/boss', null, { params: { playerId, difficulty } });
            return response.data;
        } catch (error) {
            console.error('Ошибка при инициализации боя с боссом:', error);
            throw error;
        }
    }
}

export default new CombatApiService();
