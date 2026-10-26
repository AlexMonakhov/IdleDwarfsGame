import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import { FriendsOverview, SendFriendRequestResult } from '@/models/types/Friends';

const apiClient = axios.create({
    baseURL: 'https://localhost:7232',
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Друзья есть только у залогиненного игрока — он берётся из токена
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// Ошибки приходят как { code, message }: по code фронт показывает переведённый текст (friends.errors.<code>)
class FriendsApiService {

    async getOverview(): Promise<FriendsOverview> {
        try {
            const response = await apiClient.get<FriendsOverview>('/friends');
            return response.data;
        } catch (error) {
            console.error('Ошибка при загрузке друзей:', error);
            throw error;
        }
    }

    async sendRequest(username: string): Promise<SendFriendRequestResult> {
        try {
            const response = await apiClient.post<SendFriendRequestResult>('/friends/requests', { username });
            return response.data;
        } catch (error) {
            console.error('Ошибка при отправке заявки в друзья:', error);
            throw error;
        }
    }

    async accept(fromUserId: string): Promise<void> {
        try {
            await apiClient.post(`/friends/requests/${fromUserId}/accept`);
        } catch (error) {
            console.error('Ошибка при принятии заявки в друзья:', error);
            throw error;
        }
    }

    async decline(fromUserId: string): Promise<void> {
        try {
            await apiClient.post(`/friends/requests/${fromUserId}/decline`);
        } catch (error) {
            console.error('Ошибка при отклонении заявки в друзья:', error);
            throw error;
        }
    }

    // Отозвать свою заявку
    async cancel(toUserId: string): Promise<void> {
        try {
            await apiClient.delete(`/friends/requests/${toUserId}`);
        } catch (error) {
            console.error('Ошибка при отзыве заявки в друзья:', error);
            throw error;
        }
    }

    async remove(friendId: string): Promise<void> {
        try {
            await apiClient.delete(`/friends/${friendId}`);
        } catch (error) {
            console.error('Ошибка при удалении друга:', error);
            throw error;
        }
    }
}

export default new FriendsApiService();
