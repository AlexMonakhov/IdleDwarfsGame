import { API_BASE_URL } from './apiConfig';
import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import { FixedShopBuyResult, FixedShopKind, FixedShopOverview } from '@/models/types/FixedShop';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Лимиты покупок и валюта у каждого игрока свои — он берётся из токена
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

class FixedShopsApiService {
    // Лавка ивента приходит вместе с ивентом недели (event)
    async get(shop: FixedShopKind): Promise<FixedShopOverview> {
        const response = await apiClient.get<FixedShopOverview>(`/fixed-shops/${shop}`);
        return response.data;
    }

    async buy(shop: FixedShopKind, offerKey: string): Promise<FixedShopBuyResult> {
        const response = await apiClient.post<FixedShopBuyResult>(`/fixed-shops/${shop}/${offerKey}/buy`);
        return response.data;
    }
}

export default new FixedShopsApiService();
