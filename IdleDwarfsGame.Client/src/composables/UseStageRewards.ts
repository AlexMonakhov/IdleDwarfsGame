import { computed, ref } from 'vue';
import { RewardClaimAllResult, RewardClaimResult, RewardInfo } from '@/models/types/Inventory';
import { useResources } from './UseResources';

// Награды пройденных уровней кампании и этажей башни: по одной или все разом.
// Забранное гаснет в списке сразу, без перечитывания обзора — тот прогоняет прогноз боя и стоит дорого.
// Обзор перечитывается только после ошибки: награду могли забрать в другой вкладке
interface Options {
    // Сколько незабранных наград прислал сервер в обзоре
    unclaimed: () => number;
    claimOne: (rewardId: string) => Promise<RewardClaimResult>;
    claimAll: () => Promise<RewardClaimAllResult>;
    reload: () => Promise<void>;
}

// Что показать в плашке «Получено»: забранные подряд награды копятся в одну, пока плашку не закрыли
export interface RewardNotice {
    reward: RewardInfo;
    // Башня: пропуск до этажа — у плашки свой заголовок, и с наградами за бои она не складывается
    skippedTo?: number;
}

export type RewardState = 'pending' | 'claimed' | 'none';

const addRewards = (a: RewardInfo, b: RewardInfo): RewardInfo => {
    const items = a.items.map(item => ({ ...item }));
    for (const item of b.items) {
        const same = items.find(i => i.itemType === item.itemType && i.subType === item.subType
            && i.rarity === item.rarity && i.level === item.level);
        if (same) same.quantity += item.quantity;
        else items.push({ ...item });
    }
    return {
        gold: a.gold + b.gold,
        experience: a.experience + b.experience,
        chests: [...a.chests, ...b.chests],
        items,
    };
};

export function useStageRewards(options: Options) {
    const { set: setResources } = useResources();

    // id награды, которую забираем, или 'all'
    const busy = ref<string | null>(null);
    const error = ref(false);
    const notice = ref<RewardNotice | null>(null);
    const claimedIds = ref(new Set<string>());
    const allClaimed = ref(false);

    const stateOf = (rewardId: string | null): RewardState => {
        if (!rewardId) return 'none';
        return allClaimed.value || claimedIds.value.has(rewardId) ? 'claimed' : 'pending';
    };

    const remaining = computed(() => allClaimed.value ? 0 : Math.max(0, options.unclaimed() - claimedIds.value.size));

    const show = (reward: RewardInfo, skippedTo?: number) => {
        const previous = notice.value;
        notice.value = previous && !previous.skippedTo && !skippedTo
            ? { reward: addRewards(previous.reward, reward) }
            : { reward, skippedTo };
    };

    const run = async (key: string, action: () => Promise<void>) => {
        if (busy.value) return;
        busy.value = key;
        error.value = false;
        try {
            await action();
        } catch {
            error.value = true;
            await options.reload();
        } finally {
            busy.value = null;
        }
    };

    const claim = (rewardId: string | null) => {
        if (!rewardId || stateOf(rewardId) !== 'pending') return Promise.resolve();
        return run(rewardId, async () => {
            const result = await options.claimOne(rewardId);
            setResources(result.resources);
            claimedIds.value.add(rewardId);
            show(result.reward);
        });
    };

    const claimAll = () => {
        if (!remaining.value) return Promise.resolve();
        return run('all', async () => {
            const result = await options.claimAll();
            setResources(result.resources);
            result.rewardIds.forEach(id => claimedIds.value.add(id));
            // Забраны все до одной, даже те, которых нет в списке
            allClaimed.value = true;
            show(result.reward);
        });
    };

    // Пришёл свежий обзор — в нём уже актуальные награды
    const reset = () => {
        claimedIds.value = new Set();
        allClaimed.value = false;
    };

    const dismiss = () => {
        notice.value = null;
        error.value = false;
    };

    return { busy, error, notice, remaining, stateOf, show, claim, claimAll, reset, dismiss };
}

// Прокрутить список так, чтобы элемент с data-anchor (последний пройденный уровень) встал первым.
// gap — отступ сверху: край списка затухает (drag-scroll), карточка не должна в него заходить.
// Окно масштабируется через transform (--ui-scale): getBoundingClientRect уже с масштабом, scrollTop — без
export const scrollToAnchor = (scroller: HTMLElement | null, gap = 20) => {
    const anchor = scroller?.querySelector<HTMLElement>('[data-anchor]');
    if (!scroller || !anchor || !scroller.offsetHeight) return;
    const box = scroller.getBoundingClientRect();
    const scale = box.height / scroller.offsetHeight || 1;
    scroller.scrollTop += (anchor.getBoundingClientRect().top - box.top) / scale - gap;
};
