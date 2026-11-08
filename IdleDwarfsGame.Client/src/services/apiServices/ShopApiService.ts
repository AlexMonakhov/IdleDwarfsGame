import { API_BASE_URL } from './apiConfig';
import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import { ShopBuyResult, ShopKind, ShopOverview } from '@/models/types/Shop';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Полка у каждого игрока своя — он берётся из токена
apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

class ShopApiService {
    async get(shop: ShopKind): Promise<ShopOverview> {
        const response = await apiClient.get<ShopOverview>(`/shop/${shop}`);
        return response.data;
    }

    // Раз в сутки бесплатно, дальше за радужные монеты; закреплённые товары остаются
    async reroll(shop: ShopKind): Promise<ShopOverview> {
        const response = await apiClient.post<ShopOverview>(`/shop/${shop}/reroll`);
        return response.data;
    }

    async setLocked(offerId: string, locked: boolean): Promise<ShopOverview> {
        const response = await apiClient.post<ShopOverview>(`/shop/offers/${offerId}/lock`, { locked });
        return response.data;
    }

    async buy(offerId: string): Promise<ShopBuyResult> {
        const response = await apiClient.post<ShopBuyResult>(`/shop/offers/${offerId}/buy`);
        return response.data;
    }
}

export default new ShopApiService();
