import axios, { AxiosError } from 'axios';

export interface AuthUser {
    id: string;
    username: string;
    email: string;
    playerId: string;
}

export interface AuthResponse {
    token: string;
    expiresAt: string;
    user: AuthUser;
}

export interface RegisterRequest {
    username: string;
    email: string;
    password: string;
}

export interface LoginRequest {
    // Имя пользователя или почта
    login: string;
    password: string;
}

// Ошибка, которую удобно показывать в форме: общее сообщение + ошибки по полям
export class AuthApiError extends Error {
    constructor(message: string, public fieldErrors: Record<string, string> = {}) {
        super(message);
    }
}

const apiClient = axios.create({
    baseURL: 'https://localhost:7232',
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Ключи ModelState приходят как "Username" — приводим к полям формы ("username")
const toFieldName = (key: string): string => key.charAt(0).toLowerCase() + key.slice(1);

const toAuthError = (error: unknown): AuthApiError => {
    const axiosError = error as AxiosError<{ message?: string; title?: string; errors?: Record<string, string[]> }>;
    const data = axiosError.response?.data;

    if (data?.errors) {
        const fieldErrors: Record<string, string> = {};
        Object.entries(data.errors).forEach(([key, messages]) => {
            fieldErrors[toFieldName(key)] = messages[0];
        });
        return new AuthApiError('Проверьте введённые данные', fieldErrors);
    }

    if (data?.message) {
        return new AuthApiError(data.message);
    }

    if (!axiosError.response) {
        return new AuthApiError('Сервер недоступен, попробуйте позже');
    }

    return new AuthApiError('Что-то пошло не так');
};

class AuthApiService {

    async register(request: RegisterRequest): Promise<AuthResponse> {
        try {
            const response = await apiClient.post<AuthResponse>('/auth/register', request);
            return response.data;
        } catch (error) {
            console.error('Ошибка при регистрации:', error);
            throw toAuthError(error);
        }
    }

    async login(request: LoginRequest): Promise<AuthResponse> {
        try {
            const response = await apiClient.post<AuthResponse>('/auth/login', request);
            return response.data;
        } catch (error) {
            console.error('Ошибка при входе:', error);
            throw toAuthError(error);
        }
    }

    async me(token: string): Promise<AuthUser> {
        const response = await apiClient.get<AuthUser>('/auth/me', {
            headers: { Authorization: `Bearer ${token}` }
        });
        return response.data;
    }
}

export default new AuthApiService();
