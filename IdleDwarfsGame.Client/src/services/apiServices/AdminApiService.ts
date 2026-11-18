import { API_BASE_URL, attachErrorSound } from './apiConfig';
import axios from 'axios';
import type { AdminHeroPreview, AdminMailCatalog, AdminPlayerInfo, HeroPreviewRequest, MailInfo, SendMailRequest } from '@/models/types/Mail';

// Админка пока без прав доступа (AdminController на сервере открыт всем)
const apiClient = axios.create({
    baseURL: API_BASE_URL,
    timeout: 10000,
    headers: {
        'Content-Type': 'application/json'
    }
});
attachErrorSound(apiClient);

class AdminApiService {
    async findPlayers(query: string): Promise<AdminPlayerInfo[]> {
        const response = await apiClient.get<AdminPlayerInfo[]>('/admin/players', { params: { query } });
        return response.data;
    }

    async getCatalog(): Promise<AdminMailCatalog> {
        const response = await apiClient.get<AdminMailCatalog>('/admin/mail/catalog');
        return response.data;
    }

    // Дворф из конструктора: seed из ответа вложить в письмо — игрок получит ровно его
    async previewHero(request: HeroPreviewRequest): Promise<AdminHeroPreview> {
        const response = await apiClient.post<AdminHeroPreview>('/admin/mail/hero-preview', request);
        return response.data;
    }

    async getPlayerMail(playerId: string): Promise<MailInfo[]> {
        const response = await apiClient.get<MailInfo[]>(`/admin/players/${playerId}/mail`);
        return response.data;
    }

    async send(request: SendMailRequest): Promise<MailInfo> {
        const response = await apiClient.post<MailInfo>('/admin/mail', request);
        return response.data;
    }
}

export default new AdminApiService();
