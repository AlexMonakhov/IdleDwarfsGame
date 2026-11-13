import { API_BASE_URL } from './apiConfig';
import axios from 'axios';
import { useAuth } from '@/composables/UseAuth';
import type {
    ArenaBetOverview,
    ArenaBetReplay,
    DelveDeclineResult,
    DelveMoveResult,
    DelveOverview,
    FriendHelpResult,
    FriendMinesOverview,
    GuildDepthOverview,
    MercenaryFightResult,
    MercenaryOverview
} from '@/models/types/Social';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    timeout: 5000,
    headers: { 'Content-Type': 'application/json' }
});

apiClient.interceptors.request.use(config => {
    const token = useAuth().token.value;
    if (token) config.headers.Authorization = `Bearer ${token}`;
    return config;
});

// Бой с длинным логом — таймаут больше обычного
const FIGHT_TIMEOUT = 15000;

// Гонка глубины гильдий, помощь друзьям, ставки на «Бой дня», глубинный спуск.
// Ошибки приходят как { code, message } — переводит вызывающий (socialErrorText)
class SocialApiService {
    /* ===== Гонка глубины ===== */
    async getDepth() { return (await apiClient.get<GuildDepthOverview>('/guilds/depth')).data; }
    async placeDepthDwarf(heroId: string) { return (await apiClient.post<GuildDepthOverview>('/guilds/depth/place', { heroId })).data; }
    async recallDepthDwarf() { return (await apiClient.post<GuildDepthOverview>('/guilds/depth/recall')).data; }
    async collectDepth() { return (await apiClient.post<GuildDepthOverview>('/guilds/depth/collect')).data; }
    async upgradeDepthPick() { return (await apiClient.post<GuildDepthOverview>('/guilds/depth/upgrade-pick')).data; }
    async upgradeDepthShaft() { return (await apiClient.post<GuildDepthOverview>('/guilds/depth/upgrade-shaft')).data; }
    async claimDepth() { return (await apiClient.post<GuildDepthOverview>('/guilds/depth/claim')).data; }

    /* ===== Друзья ===== */
    async getFriendMines(friendId: string) { return (await apiClient.get<FriendMinesOverview>(`/friends/${friendId}/mines`)).data; }
    async helpFriendMine(friendId: string, mineId: string) {
        return (await apiClient.post<FriendHelpResult>(`/friends/${friendId}/mines/${mineId}/help`)).data;
    }
    async getMercenaries() { return (await apiClient.get<MercenaryOverview>('/friends/mercenaries', { timeout: 10000 })).data; }
    async fightWithMercenary(stageId: number, lenderUserId: string, heroId: string, slot: number) {
        return (await apiClient.post<MercenaryFightResult>(`/campaign/${stageId}/fight-with-mercenary`,
            { lenderUserId, heroId, slot }, { timeout: FIGHT_TIMEOUT })).data;
    }

    /* ===== «Бой дня» ===== */
    async getBets() { return (await apiClient.get<ArenaBetOverview>('/pvp/bets', { timeout: FIGHT_TIMEOUT })).data; }
    async placeBet(side: 1 | 2, amount: number) { return (await apiClient.post<ArenaBetOverview>('/pvp/bets', { side, amount })).data; }
    async claimBets() { return (await apiClient.post<ArenaBetOverview>('/pvp/bets/claim', null, { timeout: FIGHT_TIMEOUT })).data; }
    async replayBet(windowStart: string) {
        return (await apiClient.get<ArenaBetReplay>(`/pvp/bets/${encodeURIComponent(windowStart)}/replay`, { timeout: FIGHT_TIMEOUT })).data;
    }

    /* ===== Глубинный спуск ===== */
    async getDelve() { return (await apiClient.get<DelveOverview>('/delve')).data; }
    async startDelve() { return (await apiClient.post<DelveOverview>('/delve/start')).data; }
    async moveDelve(column: number) { return (await apiClient.post<DelveMoveResult>('/delve/move', { column }, { timeout: FIGHT_TIMEOUT })).data; }
    async chooseRelic(key: string) { return (await apiClient.post<DelveOverview>('/delve/relic', { key })).data; }
    async camp(action: 'rest' | 'revive', heroId?: string) { return (await apiClient.post<DelveOverview>('/delve/camp', { action, heroId })).data; }
    async chooseDelveReward(index: number, heroId?: string) { return (await apiClient.post<DelveOverview>('/delve/reward', { index, heroId })).data; }
    async declineDelveReward() { return (await apiClient.post<DelveDeclineResult>('/delve/reward/decline')).data; }
    async orderDelve(heroIds: string[]) { return (await apiClient.post<DelveOverview>('/delve/order', { heroIds })).data; }
    async reviveDelve() { return (await apiClient.post<DelveOverview>('/delve/revive')).data; }
    async retreatDelve() { return (await apiClient.post<DelveOverview>('/delve/retreat')).data; }
}

export default new SocialApiService();
