import { API_BASE_URL, attachErrorSound } from './apiConfig';
import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import type { TutorialStatus } from '@/tutorial/types';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});
attachErrorSound(apiClient);

// Прогресс обучения у каждого игрока свой — он берётся из токена
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// Статусы с сервера — с большой буквы (enum TutorialStatus), на фронте — как в types.ts
type ServerStatus = 'Completed' | 'Skipped';

// Стартовое обучение: идёт ли и с какого шага продолжать
export interface OnboardingState {
    active: boolean;
    step: number;
}

interface ServerProgress {
    onboarding: OnboardingState;
    scenarios: Record<string, ServerStatus>;
    done: string[];
    campaignCleared: number;
}

export interface TutorialProgress {
    onboarding: OnboardingState;
    scenarios: Record<string, TutorialStatus>;
    // Активности, которые игрок уже пробовал
    done: string[];
    campaignCleared: number;
}

const fromServer = (progress: ServerProgress): TutorialProgress => ({
    onboarding: progress.onboarding ?? { active: false, step: 0 },
    scenarios: Object.fromEntries(Object.entries(progress.scenarios)
        .map(([key, status]) => [key, status === 'Skipped' ? 'skipped' : 'completed'])),
    done: progress.done,
    campaignCleared: progress.campaignCleared,
});

class TutorialApiService {
    async get(): Promise<TutorialProgress> {
        const response = await apiClient.get<ServerProgress>('/tutorial');
        return fromServer(response.data);
    }

    async setStatus(key: string, status: TutorialStatus): Promise<TutorialProgress> {
        const action = status === 'skipped' ? 'skip' : 'complete';
        const response = await apiClient.post<ServerProgress>(`/tutorial/${encodeURIComponent(key)}/${action}`);
        return fromServer(response.data);
    }

    // Стартовое обучение: контрольная точка, конец и пропуск (сервер выдаёт всё, что обучение выдало бы по дороге)
    async onboardingStep(step: number): Promise<TutorialProgress> {
        const response = await apiClient.post<ServerProgress>(`/tutorial/onboarding/step/${step}`);
        return fromServer(response.data);
    }

    async completeOnboarding(): Promise<TutorialProgress> {
        const response = await apiClient.post<ServerProgress>('/tutorial/onboarding/complete');
        return fromServer(response.data);
    }

    // activities — окна, которые сразу помечаются знакомыми: после пропуска их знакомства не запускаются
    async skipOnboarding(activities: string[]): Promise<TutorialProgress> {
        const response = await apiClient.post<ServerProgress>('/tutorial/onboarding/skip', { activities });
        return fromServer(response.data);
    }

    async reset(): Promise<TutorialProgress> {
        const response = await apiClient.post<ServerProgress>('/tutorial/reset');
        return fromServer(response.data);
    }
}

export default new TutorialApiService();
