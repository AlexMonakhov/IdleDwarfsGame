<script lang="ts" setup>
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import ConfirmModal from '@/components/common/ConfirmModal.vue';
import GemButton from '@/components/common/GemButton.vue';
import InfoComponent from '@/components/common/InfoComponent.vue';
import HeroCard from '@/components/inventory/HeroCard.vue';
import StatIcon from '@/components/icons/stats/StatIcon.vue';
import SocialApiService from '@/services/apiServices/SocialApiService';
import { useSocial } from '@/composables/UseSocial';
import { useResources } from '@/composables/UseResources';
import type { ModalTab, TabIcon } from '@/models/types/Modal';
import type { DelveDwarfInfo, DelveNodeInfo, DelveOverview, DelveRewardInfo } from '@/models/types/Social';
import { formatLeft, formatNumber, openExternalFight, socialErrorText } from '@/components/social/socialShared';
import { IconCoin, IconExperience, IconPickaxe, IconRainbowCoin, IconTrophy } from '@/components/icons/ui';
import DelveMap from './DelveMap.vue';
import DelveSlotCard from './DelveSlotCard.vue';
import DelveRewardCard from './DelveRewardCard.vue';
import DelveRelicCard from './DelveRelicCard.vue';
import DelveRelicSlot from './DelveRelicSlot.vue';
import { groupRelics } from './delveText';
import { regionOf } from './delveMap';
import { arenaPreview } from '@/battle/arenaBackgrounds';

// «Глубинный спуск» — бесконечный забег своим отрядом (сервер: DelveRules / DelveService).
// Вкладка «Забег»: слева путь (или выбор узла — награда, реликвия, привал: идти дальше без него нельзя),
// справа состояние забега, снизу отряд как в алтаре. Вкладка «Рейтинг» — лучшие спуски периода
const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t } = useI18n();
const router = useRouter();
const { delveOpen } = useSocial();
const { refresh: refreshResources } = useResources();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

type DelveTab = 'run' | 'rating';
const TAB_ICONS: Record<DelveTab, TabIcon> = { run: IconPickaxe, rating: IconTrophy };
const tab = ref<DelveTab>('run');
const tabs = computed<ModalTab[]>(() => (Object.keys(TAB_ICONS) as DelveTab[]).map(key => ({
    key,
    label: t(`social.delve.tabs.${key}`),
    icon: TAB_ICONS[key],
})));
const activeTab = computed({
    get: () => tab.value,
    set: value => { tab.value = value as DelveTab; },
});

const SLOT_KINDS = ['spell', 'item', 'passive', 'aura'] as const;

const overview = ref<DelveOverview | null>(null);
const busy = ref(false);
const errorMessage = ref<string | null>(null);
const notice = ref<string | null>(null);
const retreatConfirmOpen = ref(false);
const now = ref(Date.now());
// Награда, для которой выбирается дворф (индекс в run.rewards)
const pickedReward = ref<number | null>(null);
// Дворф, выбранный кликом для перестановки
const swapFrom = ref<string | null>(null);
const dragFrom = ref<string | null>(null);
const hoverSlot = ref<string | null>(null);
let clockOffset = 0;
let ticker = 0;

const apply = (next: DelveOverview) => {
    overview.value = next;
    clockOffset = new Date(next.serverTime).getTime() - Date.now();
    if (next.run?.pending !== 'Reward') pickedReward.value = null;
};

const load = async () => {
    try {
        apply(await SocialApiService.getDelve());
    } catch (error) {
        errorMessage.value = socialErrorText(error, 'delve', t('social.loadError'));
    }
};

watch(open, isOpen => {
    window.clearInterval(ticker);
    if (!isOpen) return;
    errorMessage.value = null;
    pickedReward.value = null;
    swapFrom.value = null;
    load();
    ticker = window.setInterval(() => (now.value = Date.now()), 1000);
}, { immediate: true });
onBeforeUnmount(() => window.clearInterval(ticker));

const run = computed(() => overview.value?.run ?? null);
const active = computed(() => run.value?.status === 'Active');
const dead = computed(() => run.value?.status === 'Dead');
const pending = computed(() => (active.value ? run.value?.pending ?? 'None' : 'None'));
const canMove = computed(() => active.value && pending.value === 'None');
const onMap = computed(() => !!run.value && (active.value || dead.value) && run.value.next.length > 0);
const dwarves = computed(() => run.value?.dwarves ?? []);
const fallen = computed(() => dwarves.value.filter(d => d.hp <= 0));
const squadPower = computed(() => dwarves.value.reduce((sum, d) => sum + (d.hero.power?.total ?? 0), 0));
const relics = computed(() => groupRelics(run.value?.relics ?? []));

const nextRunIn = computed(() => (overview.value ? formatLeft(new Date(overview.value.nextRunAt).getTime() - now.value - clockOffset) : ''));
const relicName = (key: string) => t(`social.delve.relicNames.${key}`);
const relicHint = (key: string) => t(`social.delve.relicHints.${key}`);
// Редкость и предел копий реликвии — из каталога обзора; сколько уже есть — из забега
const relicInfo = (key: string) => overview.value?.relics.find(r => r.key === key) ?? null;
const relicCount = (key: string) => relics.value.find(r => r.key === key)?.count ?? 0;

// Фон окон выбора — арена текущего региона, как у карты
const region = computed(() => regionOf(run.value?.floor ?? 0, overview.value?.bossEvery ?? 10));
const choiceStyle = computed(() => ({ '--arena-color': region.value.color, '--arena-image': `url(${arenaPreview(region.value.arena)})` }));

/* ===== Награды ===== */

const picked = computed<DelveRewardInfo | null>(() =>
    (pickedReward.value != null ? run.value?.rewards[pickedReward.value] ?? null : null));

const rewardLabel = (reward: DelveRewardInfo) => {
    if (reward.item) return t(`equipment.items.${reward.item.key}`);
    if (reward.kind === 'Spell') return `${reward.spellName ?? ''} ${reward.level}`;
    if (reward.kind === 'Element' && reward.element) return t(`elementNames.${reward.element}`);
    return t(`social.delve.rewardKinds.${reward.kind}`);
};

const canTake = (dwarf: DelveDwarfInfo) => !!picked.value && picked.value.fits.includes(dwarf.heroId);

// Что награда заменит у дворфа — подсказка при выборе цели
const replaces = (dwarf: DelveDwarfInfo): string | null => {
    const reward = picked.value;
    if (!reward || !canTake(dwarf)) return null;
    if (reward.kind === 'Item' && dwarf.itemKey) return t(`equipment.items.${dwarf.itemKey}`);
    if (reward.kind === 'Spell' && dwarf.spellName) return `${dwarf.spellName} ${dwarf.spellLevel}`;
    if (reward.kind === 'Passive' && dwarf.passive) return t(`passives.names.${dwarf.passive}`);
    if (reward.kind === 'Aura' && dwarf.auraEffect) return t('social.delve.slots.aura');
    return null;
};

/* ===== Действия ===== */

const act = async (action: () => Promise<DelveOverview>, done?: string) => {
    if (busy.value) return;
    busy.value = true;
    errorMessage.value = null;
    notice.value = null;
    try {
        apply(await action());
        if (done) notice.value = done;
    } catch (error) {
        errorMessage.value = socialErrorText(error, 'delve');
    } finally {
        busy.value = false;
    }
};

const start = () => act(() => SocialApiService.startDelve());
const retreat = () => act(() => SocialApiService.retreatDelve());
const chooseRelic = (key: string) => act(() => SocialApiService.chooseRelic(key), relicName(key));
const rest = () => act(() => SocialApiService.camp('rest'));
const revive = (heroId: string) => act(() => SocialApiService.camp('revive', heroId));
const reviveAll = async () => {
    await act(() => SocialApiService.reviveDelve());
    refreshResources();
};

const chooseReward = (index: number) => {
    const reward = run.value?.rewards[index];
    if (!reward || busy.value) return;
    if (reward.needsTarget) {
        pickedReward.value = pickedReward.value === index ? null : index;
        swapFrom.value = null;
        return;
    }
    act(() => SocialApiService.chooseDelveReward(index), rewardLabel(reward));
};

const giveTo = (dwarf: DelveDwarfInfo) => {
    const index = pickedReward.value;
    const reward = picked.value;
    if (index == null || !reward || !canTake(dwarf)) return;
    act(() => SocialApiService.chooseDelveReward(index, dwarf.heroId), `${rewardLabel(reward)} → ${dwarf.name}`);
};

const decline = async () => {
    if (busy.value) return;
    busy.value = true;
    errorMessage.value = null;
    try {
        const result = await SocialApiService.declineDelveReward();
        apply(result.overview);
        notice.value = t('social.delve.declined', { gold: formatNumber(result.gold), experience: formatNumber(result.experience) });
        refreshResources();
    } catch (error) {
        errorMessage.value = socialErrorText(error, 'delve');
    } finally {
        busy.value = false;
    }
};

/* ===== Порядок отряда: перетаскивание или два клика ===== */

const swap = (fromId: string, toId: string) => {
    if (fromId === toId || !active.value) return;
    const ids = dwarves.value.map(d => d.heroId);
    const from = ids.indexOf(fromId);
    const to = ids.indexOf(toId);
    if (from < 0 || to < 0) return;
    [ids[from], ids[to]] = [ids[to], ids[from]];
    act(() => SocialApiService.orderDelve(ids));
};

const onDwarfClick = (dwarf: DelveDwarfInfo) => {
    if (picked.value) {
        giveTo(dwarf);
        return;
    }
    if (!active.value) return;
    if (swapFrom.value == null) {
        swapFrom.value = dwarf.heroId;
        return;
    }
    const from = swapFrom.value;
    swapFrom.value = null;
    swap(from, dwarf.heroId);
};

const onDragStart = (dwarf: DelveDwarfInfo, event: DragEvent) => {
    if (!active.value || picked.value) {
        event.preventDefault();
        return;
    }
    dragFrom.value = dwarf.heroId;
    if (event.dataTransfer) {
        event.dataTransfer.setData('text/plain', dwarf.heroId);
        event.dataTransfer.effectAllowed = 'move';
    }
};
const onDragEnd = () => {
    dragFrom.value = null;
    hoverSlot.value = null;
};
const onDrop = (dwarf: DelveDwarfInfo) => {
    const from = dragFrom.value;
    onDragEnd();
    swapFrom.value = null;
    if (from) swap(from, dwarf.heroId);
};

// Спуск в узел. Бой уже проведён сервером — показываем его на арене и возвращаемся сюда
const move = async (node: DelveNodeInfo) => {
    if (busy.value || !canMove.value) return;
    busy.value = true;
    errorMessage.value = null;
    notice.value = null;
    try {
        const result = await SocialApiService.moveDelve(node.column);
        apply(result.overview);
        if (result.gold || result.experience || result.rainbowCoins) {
            refreshResources();
            notice.value = t('social.delve.fightReward', { gold: formatNumber(result.gold), experience: formatNumber(result.experience) })
                + (result.rainbowCoins ? t('social.delve.fightRainbow', { coins: result.rainbowCoins }) : '');
        }
        if (result.combat) {
            const combat = result.combat;
            const background = result.background ?? 'lava';
            open.value = false;
            openExternalFight(router, {
                title: t('social.delve.fightTitle', { floor: node.floor, node: t(`social.delve.nodes.${node.type}`) }),
                load: () => Promise.resolve({ combat, background }),
                backLabel: t('social.delve.back'),
                back: () => (delveOpen.value = true),
            });
        }
    } catch (error) {
        errorMessage.value = socialErrorText(error, 'delve');
    } finally {
        busy.value = false;
    }
};

const footerText = computed(() => errorMessage.value ?? notice.value ?? '');
</script>

<template>
    <ModalComponent
        v-model="open"
        v-model:active-tab="activeTab"
        :tabs="tabs"
        :header-message="t('social.delve.title')"
        width="min(1180px, calc(96 * var(--vw)))"
        height="min(960px, calc(94 * var(--vh)))"
    >
        <div v-if="!overview" class="delve-hint">{{ t('common.loading') }}</div>

        <!-- ===== Рейтинг ===== -->
        <div v-else-if="tab === 'rating'" class="delve-rating">
            <article class="delve-rating-card">
                <div class="delve-rating-heading">
                    <span class="delve-rating-title">{{ t('social.delve.leaderboard') }}</span>
                    <span class="delve-rating-left">⏳ {{ t('social.delve.periodLeft', { time: nextRunIn }) }}</span>
                </div>
                <div class="delve-rating-me">
                    <span class="delve-rating-floor">{{ overview.bestFloor }}</span>
                    <span class="delve-rating-label">{{ t('social.delve.myDepth') }}</span>
                </div>
                <ol v-if="overview.leaderboard.length" class="delve-rating-top">
                    <li v-for="leader in overview.leaderboard" :key="leader.userId" :class="{ 'delve-rating-top-me': leader.isMe }">
                        <span class="delve-rating-place">{{ leader.place }}</span>
                        <span class="delve-rating-name">{{ leader.username }}</span>
                        <span class="delve-rating-value">{{ t('social.delve.floor', { floor: leader.floor }) }}</span>
                    </li>
                </ol>
                <span v-else class="delve-hint">{{ t('social.delve.noScores') }}</span>
            </article>
        </div>

        <!-- ===== Забег ===== -->
        <div v-else class="delve">
            <section class="delve-top">
                <!-- Слева: выбор узла (идти дальше без него нельзя) или путь -->
                <div class="delve-stage">
                    <!-- Без inline значок сам встаёт в правый верхний угол: он absolute, а .delve-stage — relative -->
                    <InfoComponent :title="t('social.delve.title')" :text="t('social.delve.intro')" />
                    <div v-if="pending === 'Reward' && run" class="delve-choice" :style="choiceStyle">
                        <span class="delve-choice-light" aria-hidden="true"><span class="delve-choice-rays"></span></span>
                        <h4>{{ picked ? t('social.delve.chooseTarget', { reward: rewardLabel(picked) }) : t('social.delve.chooseReward') }}</h4>
                        <div class="delve-reward-cards">
                            <DelveRewardCard
                                v-for="(reward, index) in run.rewards"
                                :key="index"
                                :reward="reward"
                                :selected="pickedReward === index"
                                :disabled="busy"
                                @pick="chooseReward(index)"
                            />
                        </div>
                        <div class="delve-buttons delve-buttons-bottom">
                            <GemButton v-if="picked" :color="'blue'" :disabled="busy" @click="pickedReward = null">{{ t('social.delve.cancel') }}</GemButton>
                            <GemButton :color="'yellow'" :disabled="busy" @click="decline">{{ t('social.delve.decline') }}</GemButton>
                        </div>
                    </div>

                    <div v-else-if="pending === 'Relic' && run" class="delve-choice" :style="choiceStyle">
                        <span class="delve-choice-light" aria-hidden="true"><span class="delve-choice-rays"></span></span>
                        <h4>{{ t('social.delve.chooseRelic') }}</h4>
                        <div class="delve-reward-cards">
                            <DelveRelicCard
                                v-for="key in run.offer"
                                :key="key"
                                :relic-key="key"
                                :rare="relicInfo(key)?.rare"
                                :max="relicInfo(key)?.max"
                                :owned="relicCount(key)"
                                :disabled="busy"
                                @pick="chooseRelic(key)"
                            />
                        </div>
                    </div>

                    <div v-else-if="pending === 'Camp'" class="delve-choice delve-choice-camp" :style="choiceStyle">
                        <h4>🔥 {{ t('social.delve.camp') }}</h4>
                        <p class="delve-hint">{{ t('social.delve.campHint') }}</p>
                        <div class="delve-buttons">
                            <GemButton :color="'green'" :disabled="busy" @click="rest">{{ t('social.delve.rest') }}</GemButton>
                            <GemButton v-for="hero in fallen" :key="hero.heroId" :color="'yellow'" :disabled="busy" @click="revive(hero.heroId)">
                                {{ t('social.delve.revive', { name: hero.name }) }}
                            </GemButton>
                        </div>
                    </div>

                    <DelveMap
                        v-else-if="run && onMap"
                        :run="run"
                        :columns="overview.columns"
                        :boss-every="overview.bossEvery"
                        :can-move="canMove"
                        :busy="busy"
                        @move="move"
                    />

                    <div v-else class="delve-choice delve-choice-intro">
                        <h4>{{ t('social.delve.title') }}</h4>
                        <p class="delve-hint">{{ t('social.delve.intro') }}</p>
                        <div class="delve-buttons">
                            <GemButton v-if="overview.canStart" :color="'green'" :disabled="busy" @click="start">{{ t('social.delve.start') }}</GemButton>
                            <span v-else class="delve-hint">{{ t('social.delve.nextRun', { time: nextRunIn }) }}</span>
                        </div>
                    </div>
                </div>
            </section>

            <!-- ===== Отряд, как вкладка «Отряд» в алтаре ===== -->
            <div v-if="run" class="delve-bottom">
                <section class="delve-squad">
                    <div class="delve-squad-heading">
                        <h3 class="delve-squad-title">{{ t('social.delve.squad') }}</h3>
                        <InfoComponent inline :title="t('social.delve.squad')" :text="t('social.delve.squadHint')" />
                        <span v-if="picked" class="delve-squad-picking">{{ t('social.delve.chooseTarget', { reward: rewardLabel(picked) }) }}</span>
                        <span class="delve-squad-power"><StatIcon stat="power" /> {{ t('power.squad', { power: squadPower }) }}</span>
                    </div>
                    <div class="delve-squad-row">
                        <div
                            v-for="dwarf in dwarves"
                            :key="dwarf.heroId"
                            class="delve-squad-slot"
                            :class="{
                                'delve-squad-slot-hover': hoverSlot === dwarf.heroId && dragFrom && dragFrom !== dwarf.heroId,
                                'delve-squad-slot-fallen': dwarf.hp <= 0,
                                'delve-squad-slot-target': canTake(dwarf),
                                'delve-squad-slot-unfit': picked && !canTake(dwarf),
                            }"
                            @dragover.prevent="hoverSlot = dwarf.heroId"
                            @dragleave="hoverSlot = null"
                            @drop.prevent="onDrop(dwarf)"
                        >
                            <span class="delve-squad-number">{{ dwarf.position }}</span>
                            <HeroCard
                                :hero="dwarf.hero"
                                :selectable="!!picked || !active"
                                :selected="swapFrom === dwarf.heroId || canTake(dwarf)"
                                :dragging="dragFrom === dwarf.heroId"
                                :disabled="!!picked && !canTake(dwarf)"
                                @dragstart="onDragStart(dwarf, $event)"
                                @dragend="onDragEnd"
                                @click="onDwarfClick(dwarf)"
                            />
                            <div class="delve-hp" v-tooltip="`${Math.round(dwarf.hp * 100)}%`">
                                <div class="delve-hp-fill" :style="{ width: `${Math.round(dwarf.hp * 100)}%` }"></div>
                            </div>
                            <div class="delve-slots">
                                <DelveSlotCard v-for="kind in SLOT_KINDS" :key="kind" :kind="kind" :hero="dwarf.hero" />
                            </div>
                            <span v-if="replaces(dwarf)" class="delve-replaces">{{ t('social.delve.replaces', { old: replaces(dwarf) }) }}</span>
                        </div>
                    </div>
                </section>

                <!-- Справа от отряда: состояние забега, добыча, реликвии -->
                    <aside class="delve-side">
                        <div class="delve-status" :class="`delve-status-${run.status.toLowerCase()}`">
                            <span class="delve-status-title">{{ t(`social.delve.status.${run.status}`) }}</span>
                            <span class="delve-status-floor">{{ t('social.delve.floor', { floor: run.floor }) }}</span>
                        </div>

                        <div class="delve-earned">
                            <span class="delve-side-label">{{ t('social.delve.earned') }}</span>
                            <span><IconCoin /> {{ formatNumber(run.goldEarned) }}</span>
                            <span><IconExperience /> {{ formatNumber(run.experienceEarned) }}</span>
                            <span><IconRainbowCoin /> {{ run.rainbowCoinsEarned }}</span>
                        </div>

                        <!-- Пал весь отряд: раз за забег можно воскресить всех -->
                        <div v-if="dead" class="delve-dead">
                            <template v-if="!run.revived">
                                <p class="delve-hint">{{ t('social.delve.reviveHint') }}</p>
                                <GemButton :color="'yellow'" :disabled="busy || !run.canRevive" @click="reviveAll">
                                    {{ t('social.delve.reviveAll', { cost: overview.reviveCost }) }} <IconRainbowCoin />
                                </GemButton>
                            </template>
                            <p v-else class="delve-hint">{{ t('social.delve.reviveUsed') }}</p>
                        </div>

                        <div class="delve-relics">
                            <span class="delve-side-label">{{ t('social.delve.relics') }}</span>
                            <div class="delve-relic-list">
                                <DelveRelicSlot v-for="relic in relics" :key="relic.key" :relic-key="relic.key" :count="relic.count" :rare="relicInfo(relic.key)?.rare" />
                                <span v-if="!relics.length" class="delve-hint">{{ t('social.delve.noRelics') }}</span>
                            </div>
                        </div>

                        <div class="delve-actions">
                            <GemButton v-if="active" :color="'red'" :disabled="busy" @click="retreatConfirmOpen = true">{{ t('social.delve.retreat') }}</GemButton>
                            <span v-else class="delve-hint">{{ t('social.delve.nextRun', { time: nextRunIn }) }}</span>
                        </div>
                    </aside>
            </div>
        </div>

        <template #modalFooter>
            <div class="delve-footer">
                <span :class="{ 'delve-footer-error': errorMessage }">{{ footerText }}</span>
            </div>
        </template>
    </ModalComponent>

    <ConfirmModal
        v-model="retreatConfirmOpen"
        :header="t('social.delve.retreat')"
        :message="t('social.delve.retreatConfirm')"
        @confirm="retreat"
    />
</template>

<style scoped>
.delve {
    flex: 1;
    min-width: 0;
    min-height: 0;
    display: flex;
    flex-direction: column;
    gap: 12px;
    padding: 6px;
    font-family: 'Arial', sans-serif;
    text-align: left;
    color: #f8fafc;
    font-size: 13px;
}

.delve-hint {
    margin: 0;
    color: #cbd5e1;
    font-size: 12px;
}

/* ===== Верх: путь или выбор (~60%) и состояние забега ===== */

.delve-top {
    flex: 1;
    min-height: 300px;
    display: flex;
}

.delve-top > .delve-stage {
    flex: 1;
}

/* Низ: отряд слева, справа узкая панель забега — статус, добыча, реликвии по 4 в ряд */
.delve-bottom {
    flex-shrink: 0;
    display: grid;
    grid-template-columns: minmax(0, 1fr) 236px;
    gap: 12px;
}

@media (max-width: 820px) {
    .delve-bottom {
        grid-template-columns: 1fr;
    }
}

/* relative — точка отсчёта для значка «i» в правом верхнем углу */
.delve-stage {
    position: relative;
    min-width: 0;
    min-height: 0;
    display: flex;
    flex-direction: column;
}

.delve-stage > * {
    flex: 1;
}

/* Окно выбора — «сокровищница» на фоне арены региона: та же оправа, что у карты, золотой свет из центра,
   медленные лучи за карточками и каменный уступ внизу, на котором они стоят */
.delve-choice {
    --arena-color: #facc15;
    position: relative;
    isolation: isolate;
    flex: 1;
    min-height: 0;
    display: flex;
    flex-direction: column;
    gap: 10px;
    padding: 12px 16px;
    border-radius: 14px;
    border: 3px solid transparent;
    background:
        /* Свет из центра и затемнение краёв */
        radial-gradient(ellipse 60% 55% at 50% 52%, rgba(253, 224, 71, 0.22), transparent 70%) padding-box,
        radial-gradient(ellipse 85% 80% at 50% 50%, transparent 45%, rgba(8, 10, 16, 0.85) 100%) padding-box,
        /* Каменный уступ: светлая кромка и тёмная толща */
        linear-gradient(to bottom, transparent 76%, rgba(214, 186, 140, 0.35) 76.4%, rgba(60, 44, 30, 0.85) 77.5%, rgba(20, 14, 10, 0.95) 100%) padding-box,
        /* Арена региона, приглушённая и тонированная его цветом */
        linear-gradient(color-mix(in srgb, var(--arena-color) 14%, rgba(13, 17, 24, 0.55)), rgba(13, 17, 24, 0.7)) padding-box,
        var(--arena-image, none) center / cover padding-box,
        linear-gradient(#0d1118, #0d1118) padding-box,
        var(--metalic-background) border-box;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.6);
    overflow: hidden auto;
}

/* Слой света обрезает лучи по окну: иначе они торчат за край и окно выбора начинает прокручиваться */
.delve-choice-light {
    position: absolute;
    inset: 0;
    z-index: -1;
    overflow: hidden;
    pointer-events: none;
}

/* Лучи света за карточками: одна большая вращающаяся плашка, ведёт видеокарта (transform) */
.delve-choice-rays {
    position: absolute;
    top: 52%;
    left: 50%;
    width: 130%;
    aspect-ratio: 1;
    background: repeating-conic-gradient(rgba(253, 230, 138, 0.13) 0deg 7deg, transparent 7deg 22deg);
    -webkit-mask: radial-gradient(closest-side, #000 15%, transparent 70%);
    mask: radial-gradient(closest-side, #000 15%, transparent 70%);
    transform: translate(-50%, -50%);
    animation: delve-rays 80s linear infinite;
    pointer-events: none;
}

@keyframes delve-rays {
    to { transform: translate(-50%, -50%) rotate(360deg); }
}

@media (prefers-reduced-motion: reduce) {
    .delve-choice-rays { animation: none; }
}

.delve-choice h4 {
    margin: 0;
    color: #fde68a;
    font-size: 14px;
    text-align: center;
    text-transform: uppercase;
    letter-spacing: 2px;
    text-shadow: 1px 1px 0 #000, 0 0 10px rgba(250, 204, 21, 0.5);
}

.delve-choice-camp {
    justify-content: center;
    align-items: center;
    text-align: center;
}

.delve-choice-intro {
    justify-content: center;
    border-color: rgba(139, 92, 246, 0.5);
    background: radial-gradient(ellipse at top, rgba(60, 40, 90, 0.6), rgba(8, 10, 18, 0.9));
}

/* Карточки одной ширины, в ряду тянутся до самой высокой */
.delve-reward-cards {
    flex: 1;
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-content: center;
    align-items: stretch;
    gap: 12px;
    padding-top: 4px;
}

/* Кнопки выбора награды прижаты к низу: смена заголовка и выбор не двигают их */
.delve-buttons-bottom {
    margin-top: auto;
    padding-top: 6px;
}

.delve-buttons,
.delve-actions {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 8px;
}

.delve-buttons {
    justify-content: center;
}

.delve-side {
    min-width: 0;
    min-height: 0;
    display: flex;
    flex-direction: column;
    gap: 8px;
    padding: 10px;
    border-radius: 14px;
    border: 2px solid rgba(157, 173, 175, 0.35);
    background: rgba(15, 23, 32, 0.75);
    overflow-y: auto;
}

.delve-side-label {
    color: #a78bfa;
    font-size: 11px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.delve-status {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 8px;
    padding: 4px 10px;
    border-radius: 10px;
    border: 2px solid rgba(157, 173, 175, 0.5);
    background: rgba(0, 0, 0, 0.35);
}

.delve-status-title {
    color: #e2e8f0;
    font-size: 12px;
    text-transform: uppercase;
}

.delve-status-floor {
    color: #ffd166;
    font-size: 20px;
    font-weight: 900;
    text-shadow: 0 2px 6px #000;
}

.delve-status-active { border-color: rgba(74, 222, 128, 0.6); }
.delve-status-dead { border-color: rgba(248, 113, 113, 0.7); }

.delve-earned {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 6px 12px;
    font-weight: bold;
}

.delve-earned .delve-side-label {
    width: 100%;
}

.delve-dead {
    display: flex;
    flex-direction: column;
    gap: 8px;
    padding: 10px;
    border-radius: 10px;
    border: 2px solid rgba(248, 113, 113, 0.7);
    background: rgba(60, 14, 14, 0.55);
}

.delve-relics {
    display: flex;
    flex-direction: column;
    gap: 6px;
}

.delve-relic-list {
    display: grid;
    grid-template-columns: repeat(4, 44px);
    gap: 10px 8px;
    padding-top: 6px;
}

.delve-relic-list > .delve-hint {
    grid-column: 1 / -1;
}

.delve-actions {
    margin-top: auto;
    justify-content: center;
}

/* ===== Низ: отряд как во вкладке алтаря ===== */

.delve-squad {
    min-width: 0;
    padding: 8px 10px 10px;
    border-radius: 12px;
    background: rgba(8, 12, 20, 0.35);
}

.delve-squad-heading {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    column-gap: 8px;
    row-gap: 4px;
    margin-bottom: 6px;
}

.delve-squad-title {
    margin: 0;
    color: #f8fafc;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.delve-squad-picking {
    color: #fde68a;
    font-size: 12px;
    font-weight: bold;
}

.delve-squad-power {
    margin-left: auto;
    color: #ffd166;
    font-size: 14px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.delve-squad-row {
    display: flex;
    justify-content: center;
    gap: 12px;
    overflow-x: auto;
    /* Сверху место под звёзды ранга, снизу — под подъём мини-карточек */
    padding: 12px 4px 6px;
}

/* Слот как в ряду отряда алтаря (SquadRow): пунктир, номер места, карточка дворфа */
.delve-squad-slot {
    position: relative;
    box-sizing: border-box;
    flex-shrink: 0;
    width: 156px;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
    padding: 8px 6px 10px;
    border-radius: 16px;
    border: 2px dashed rgba(255, 255, 255, 0.18);
    background: rgba(0, 0, 0, 0.18);
    transition: border-color 0.15s ease, background-color 0.15s ease, opacity 0.15s ease;
}

.delve-squad-slot-hover {
    border-color: var(--yellow-gem-color);
    background-color: rgba(255, 179, 0, 0.1);
}

.delve-squad-slot-target {
    border-color: #ffd166;
    border-style: solid;
    background-color: rgba(255, 209, 102, 0.08);
}

.delve-squad-slot-unfit {
    opacity: 0.5;
}

.delve-squad-slot-fallen :deep(.hero-card) {
    filter: grayscale(0.9) brightness(0.6);
}

.delve-squad-number {
    position: absolute;
    top: 4px;
    left: 8px;
    z-index: 4;
    color: rgba(255, 255, 255, 0.55);
    font-weight: bold;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.delve-hp {
    width: 124px;
    height: 6px;
    border-radius: 3px;
    background: rgba(0, 0, 0, 0.6);
    overflow: hidden;
}

.delve-hp-fill {
    height: 100%;
    background: linear-gradient(90deg, #22c55e, #86efac);
}

/* Спелл, предмет, пассивка, аура — квадратная матрица 2×2 под карточкой; зазор — под значок уровня в углу */
.delve-slots {
    display: grid;
    grid-template-columns: repeat(2, 52px);
    grid-auto-rows: 52px;
    gap: 12px;
    padding-top: 4px;
}

.delve-replaces {
    color: #fca5a5;
    font-size: 11px;
    text-align: center;
}

/* ===== Рейтинг — как сезоны PvP ===== */

.delve-rating {
    flex: 1;
    display: flex;
    justify-content: center;
    align-items: flex-start;
    padding: 12px;
    font-family: 'Arial', sans-serif;
    color: #f8fafc;
}

.delve-rating-card {
    width: min(480px, 100%);
    display: flex;
    flex-direction: column;
    row-gap: 10px;
    padding: 14px;
    border-radius: 12px;
    border: 2px solid rgba(255, 209, 102, 0.55);
    background: rgba(15, 23, 32, 0.8);
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5);
}

.delve-rating-heading {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
    gap: 8px;
}

.delve-rating-title {
    font-size: 14px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.delve-rating-left {
    color: #fde68a;
    font-size: 12px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
}

.delve-rating-me {
    display: flex;
    flex-direction: column;
    align-items: center;
}

.delve-rating-floor {
    color: #ffd166;
    font-size: 36px;
    font-weight: 900;
    text-shadow: 0 2px 6px #000;
}

.delve-rating-label {
    color: #e2e8f0;
    font-size: 11px;
    text-transform: uppercase;
}

.delve-rating-top {
    margin: 0;
    padding: 0;
    list-style: none;
    display: flex;
    flex-direction: column;
    row-gap: 2px;
    font-size: 13px;
}

.delve-rating-top li {
    display: flex;
    column-gap: 8px;
    padding: 3px 6px;
    border-radius: 6px;
    color: #cbd5e1;
}

.delve-rating-top li.delve-rating-top-me {
    background: rgba(255, 209, 102, 0.18);
    color: #fff;
    font-weight: bold;
}

.delve-rating-place {
    width: 22px;
    color: #ffd166;
    text-align: right;
}

.delve-rating-name {
    flex: 1;
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.delve-rating-value {
    color: #ffd166;
    font-variant-numeric: tabular-nums;
}

.delve-footer {
    flex: 1;
    min-height: 24px;
    color: #fff;
    font-family: 'Arial', sans-serif;
    font-size: 14px;
    font-weight: bold;
    text-align: left;
}

.delve-footer-error {
    color: #ffb4b4;
}
</style>
