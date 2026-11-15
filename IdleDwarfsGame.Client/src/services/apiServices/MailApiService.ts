import { API_BASE_URL } from './apiConfig';
import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import type { MailClaimResult, MailInfo } from '@/models/types/Mail';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Почта у каждого игрока своя — он берётся из токена
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

class MailApiService {
    async getMine(): Promise<MailInfo[]> {
        const response = await apiClient.get<MailInfo[]>('/mail/me');
        return response.data;
    }

    async markRead(mailId: string): Promise<MailInfo> {
        const response = await apiClient.post<MailInfo>(`/mail/me/${mailId}/read`);
        return response.data;
    }

    // Вложения уже на складе и на аккаунте; письмо помечено забранным
    async claim(mailId: string): Promise<MailClaimResult> {
        const response = await apiClient.post<MailClaimResult>(`/mail/me/${mailId}/claim`);
        return response.data;
    }
}

export default new MailApiService();
