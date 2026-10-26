import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import { SessionHeartbeat, SessionStarted } from '@/models/types/Friends';

const BASE_URL = 'https://localhost:7232';

const apiClient = axios.create({
    baseURL: BASE_URL,
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

// Заходы в игру: вкладка открывает сессию, продлевает её пингами и закрывает при уходе (см. usePresence)
class SessionsApiService {

    async start(): Promise<SessionStarted> {
        const response = await apiClient.post<SessionStarted>('/sessions');
        return response.data;
    }

    // 404 — сессия уже закрыта, нужно открыть новую
    async heartbeat(sessionId: string): Promise<SessionHeartbeat> {
        const response = await apiClient.post<SessionHeartbeat>(`/sessions/${sessionId}/heartbeat`);
        return response.data;
    }

    // Токен передаём явно: при выходе он уже может быть стёрт из useAuth.
    // keepalive — запрос доживает до сервера, даже если вкладку как раз закрывают (axios так не умеет)
    end(sessionId: string, token: string): void {
        fetch(`${BASE_URL}/sessions/${sessionId}/end`, {
            method: 'POST',
            keepalive: true,
            headers: { Authorization: `Bearer ${token}` }
        }).catch(() => {
            // Не дошло — сессия сама протухнет на сервере, когда перестанут приходить пинги
        });
    }
}

export default new SessionsApiService();
