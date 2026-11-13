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
import type { DelveDwarfInfo, DelveNodeInfo, DelveNodeType, DelveOverview, DelveRewardInfo } from '@/models/types/Social';
import { formatLeft, formatNumber, openExternalFight, socialErrorText } from '@/components/social/socialShared';
import { IconCoin, IconExperience, IconRainbowCoin, IconTrophy } from '@/components/icons/ui';
import { vDragScroll } from '@/directives/dragScroll';
import DelveSlotCard from './DelveSlotCard.vue';
import DelveRewardCard from './DelveRewardCard.vue';
import { groupRelics } from './delveText';

// «Глубинный спуск» — бесконечный забег своим отрядом (сервер: DelveRules / DelveService).
// Сверху — путь (следующий этаж целиком и узел через этаж под текущим) и выбор узла: награда, реликвия, привал.
// Снизу — отряд как в алтаре: карточки дворфов, под каждой — спелл, предмет, пассивка и аура с подсказками.
// Награду, которую надевают на дворфа, выбирают кликом, затем кликают по дворфу
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

const NODE_ICONS: Record<DelveNodeType, string> = {
    Fight: '⚔️',
    Elite: '💀',
    Camp: '🔥',
    Treasure: '💎',
    Boss: '👑',
};

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
const choosing = computed(() => active.value && run.value?.pending !== 'None');
const columns = computed(() => Array.from({ length: overview.value?.columns ?? 5 }, (_, i) => i));
const dwarves = computed(() => run.value?.dwarves ?? []);
const fallen = computed(() => dwarves.value.filter(d => d.hp <= 0));
const squadPower = computed(() => dwarves.value.reduce((sum, d) => sum + (d.hero.power?.total ?? 0), 0));
const relics = computed(() => groupRelics(run.value?.relics ?? []));

const isBossFloor = (floor: number) => !!overview.value && floor > 0 && floor % overview.value.bossEvery === 0;
const nextNode = (column: number) => run.value?.next.find(n => n.column === column) ?? null;
const isReachable = (node: DelveNodeInfo | null) => {
    const r = run.value;
    if (!node || !r || !active.value || choosing.value) return false;
    return r.floor === 0 || Math.abs(node.column - r.column) <= 1;
};

const nextRunIn = computed(() => (overview.value ? formatLeft(new Date(overview.value.nextRunAt).getTime() - now.value - clockOffset) : ''));
const relicName = (key: string) => t(`social.delve.relicNames.${key}`);
const relicHint = (key: string) => t(`social.delve.relicHints.${key}`);

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
const move = async (node: DelveNodeInfo | null) => {
    if (!node || busy.value || !isReachable(node)) return;
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
        :header-message="t('social.delve.title')"
        width="min(1180px, calc(96 * var(--vw)))"
        height="min(940px, calc(94 * var(--vh)))"
    >
        <div v-if="!overview" class="delve-hint">{{ t('common.loading') }}</div>

        <div v-else class="delve">
            <!-- ===== Верх: путь и выбор ===== -->
            <section class="delve-top">
                <div class="delve-left">
                    <div class="delve-map">
                        <template v-if="run && (active || dead) && run.next.length">
                            <div class="delve-floor delve-floor-current">
                                <span class="delve-floor-number">{{ run.floor || '—' }}</span>
                                <template v-for="column in columns" :key="column">
                                    <span v-if="column === run.column" class="delve-node delve-node-here" v-tooltip="run.floor ? t('social.delve.you') : t('social.delve.entrance')">⛏️</span>
                                    <span v-else class="delve-node-empty"></span>
                                </template>
                            </div>
                            <div class="delve-floor" :class="{ 'delve-floor-boss': isBossFloor(run.floor + 1) }">
                                <span class="delve-floor-number">{{ run.floor + 1 }}</span>
                                <template v-for="column in columns" :key="column">
                                    <button
                                        v-if="nextNode(column)"
                                        type="button"
                                        class="delve-node"
                                        :class="[`delve-node-${nextNode(column)!.type.toLowerCase()}`, { 'delve-node-reachable': isReachable(nextNode(column)) }]"
                                        :disabled="!isReachable(nextNode(column)) || busy"
                                        v-tooltip="t(`social.delve.nodes.${nextNode(column)!.type}`)"
                                        @click="move(nextNode(column))"
                                    >
                                        <span class="delve-node-icon">{{ NODE_ICONS[nextNode(column)!.type] }}</span>
                                    </button>
                                </template>
                            </div>
                            <div class="delve-floor" :class="{ 'delve-floor-boss': isBossFloor(run.floor + 2) }">
                                <span class="delve-floor-number">{{ run.floor + 2 }}</span>
                                <template v-for="column in columns" :key="column">
                                    <span
                                        v-if="run.peek && run.peek.column === column"
                                        class="delve-node delve-node-peek"
                                        :class="`delve-node-${run.peek.type.toLowerCase()}`"
                                        v-tooltip="t(`social.delve.nodes.${run.peek.type}`)"
                                    >
                                        <span class="delve-node-icon">{{ NODE_ICONS[run.peek.type] }}</span>
                                    </span>
                                    <span v-else class="delve-node delve-node-unknown" v-tooltip="t('social.delve.unknown')">?</span>
                                </template>
                            </div>
                            <p class="delve-hint delve-map-hint">{{ t('social.delve.peekHint') }}</p>
                        </template>
                        <p v-else class="delve-hint">{{ t('social.delve.intro') }}</p>
                    </div>

                    <div class="delve-actions">
                        <GemButton v-if="overview.canStart" :color="'green'" :disabled="busy" @click="start">{{ t('social.delve.start') }}</GemButton>
                        <span v-else-if="!active" class="delve-hint">{{ t('social.delve.nextRun', { time: nextRunIn }) }}</span>
                        <GemButton v-if="active" :color="'red'" :disabled="busy" @click="retreatConfirmOpen = true">{{ t('social.delve.retreat') }}</GemButton>
                        <InfoComponent inline :title="t('social.delve.title')" :text="t('social.delve.intro')" />
                    </div>

                    <p class="delve-hint"><IconTrophy /> {{ t('social.delve.best', { floor: overview.bestFloor }) }}</p>
                    <ol v-if="overview.leaderboard.length" class="delve-leaders">
                        <li v-for="leader in overview.leaderboard" :key="leader.userId" :class="{ 'delve-leader-me': leader.isMe }">
                            <span>{{ leader.place }}. {{ leader.username }}</span>
                            <span>{{ t('social.delve.floor', { floor: leader.floor }) }}</span>
                        </li>
                    </ol>
                </div>

                <div class="delve-right">
                    <template v-if="run">
                        <div class="delve-status" :class="`delve-status-${run.status.toLowerCase()}`">
                            <span>{{ t(`social.delve.status.${run.status}`) }} · {{ t('social.delve.floor', { floor: run.floor }) }}</span>
                            <span class="delve-earned">
                                <span><IconCoin /> {{ formatNumber(run.goldEarned) }}</span>
                                <span><IconExperience /> {{ formatNumber(run.experienceEarned) }}</span>
                                <span v-if="run.rainbowCoinsEarned"><IconRainbowCoin /> {{ run.rainbowCoinsEarned }}</span>
                            </span>
                        </div>

                        <!-- Пал весь отряд: раз за забег можно воскресить всех -->
                        <div v-if="dead" class="delve-choice delve-choice-dead">
                            <template v-if="!run.revived">
                                <p class="delve-hint">{{ t('social.delve.reviveHint') }}</p>
                                <div class="delve-buttons">
                                    <GemButton :color="'yellow'" :disabled="busy || !run.canRevive" @click="reviveAll">
                                        {{ t('social.delve.reviveAll', { cost: overview.reviveCost }) }} <IconRainbowCoin />
                                    </GemButton>
                                </div>
                            </template>
                            <p v-else class="delve-hint">{{ t('social.delve.reviveUsed') }}</p>
                        </div>

                        <!-- Выбор реликвии -->
                        <div v-if="active && run.pending === 'Relic'" class="delve-choice">
                            <h4>{{ t('social.delve.chooseRelic') }}</h4>
                            <div class="delve-relic-cards">
                                <button v-for="key in run.offer" :key="key" type="button" class="delve-relic-card" :disabled="busy" @click="chooseRelic(key)">
                                    <b>{{ relicName(key) }}</b>
                                    <span>{{ relicHint(key) }}</span>
                                </button>
                            </div>
                        </div>

                        <!-- Привал -->
                        <div v-if="active && run.pending === 'Camp'" class="delve-choice">
                            <h4>🔥 {{ t('social.delve.camp') }}</h4>
                            <div class="delve-buttons">
                                <GemButton :color="'green'" :disabled="busy" @click="rest">{{ t('social.delve.rest') }}</GemButton>
                                <GemButton v-for="hero in fallen" :key="hero.heroId" :color="'yellow'" :disabled="busy" @click="revive(hero.heroId)">
                                    {{ t('social.delve.revive', { name: hero.name }) }}
                                </GemButton>
                            </div>
                        </div>

                        <!-- Награда узла -->
                        <div v-if="active && run.pending === 'Reward'" class="delve-choice">
                            <h4>{{ picked ? t('social.delve.chooseTarget', { reward: rewardLabel(picked) }) : t('social.delve.chooseReward') }}</h4>
                            <div v-drag-scroll class="delve-reward-cards">
                                <DelveRewardCard
                                    v-for="(reward, index) in run.rewards"
                                    :key="index"
                                    :reward="reward"
                                    :selected="pickedReward === index"
                                    :disabled="busy"
                                    @pick="chooseReward(index)"
                                />
                            </div>
                            <div class="delve-buttons">
                                <GemButton v-if="picked" :color="'blue'" :disabled="busy" @click="pickedReward = null">{{ t('social.delve.cancel') }}</GemButton>
                                <GemButton :color="'yellow'" :disabled="busy" @click="decline">{{ t('social.delve.decline') }}</GemButton>
                            </div>
                        </div>

                        <p v-if="active && run.pending === 'None'" class="delve-hint delve-go">{{ t('social.delve.chooseNode') }}</p>

                        <div class="delve-relics">
                            <span class="delve-relics-title">{{ t('social.delve.relics') }}:</span>
                            <span v-for="relic in relics" :key="relic.key" class="delve-relic" v-tooltip="relicHint(relic.key)">
                                {{ relicName(relic.key) }}<b v-if="relic.count > 1"> ×{{ relic.count }}</b>
                            </span>
                            <span v-if="!relics.length" class="delve-hint">{{ t('social.delve.noRelics') }}</span>
                        </div>
                    </template>
                    <p v-else class="delve-hint">{{ t('social.delve.intro') }}</p>
                </div>
            </section>

            <!-- ===== Низ: отряд, как вкладка «Отряд» в алтаре ===== -->
            <section v-if="run" class="delve-squad">
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
    gap: 10px;
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

/* ===== Верх ===== */

.delve-top {
    flex: 1;
    min-height: 0;
    display: grid;
    grid-template-columns: minmax(280px, 340px) minmax(0, 1fr);
    gap: 12px;
}

@media (max-width: 820px) {
    .delve-top {
        grid-template-columns: 1fr;
    }
}

.delve-left,
.delve-right {
    min-width: 0;
    min-height: 0;
    display: flex;
    flex-direction: column;
    gap: 8px;
    overflow-y: auto;
}

.delve-map {
    display: flex;
    flex-direction: column;
    gap: 8px;
    padding: 12px;
    border-radius: 14px;
    background: radial-gradient(ellipse at top, rgba(60, 40, 90, 0.6), rgba(8, 10, 18, 0.9));
    border: 2px solid rgba(139, 92, 246, 0.4);
}

.delve-map-hint {
    font-size: 11px;
}

.delve-floor {
    display: grid;
    grid-template-columns: 26px repeat(5, 1fr);
    align-items: center;
    gap: 6px;
    padding: 3px 0;
    border-radius: 10px;
}

.delve-floor-boss {
    background: rgba(250, 204, 21, 0.1);
}

.delve-floor-number {
    color: #a78bfa;
    font-weight: bold;
    text-align: right;
}

.delve-node,
.delve-node-empty {
    justify-self: center;
    width: 42px;
    height: 42px;
}

.delve-node {
    display: grid;
    place-items: center;
    border-radius: 50%;
    border: 2px solid rgba(148, 163, 184, 0.4);
    background: rgba(15, 23, 32, 0.85);
    color: #f8fafc;
    cursor: default;
    opacity: 0.55;
    transition: transform 0.15s, box-shadow 0.15s, opacity 0.15s;
}

.delve-node-icon {
    font-size: 19px;
}

.delve-node-elite { border-color: rgba(248, 113, 113, 0.6); }
.delve-node-camp { border-color: rgba(251, 146, 60, 0.6); }
.delve-node-treasure { border-color: rgba(96, 165, 250, 0.6); }
.delve-node-boss { border-color: rgba(250, 204, 21, 0.8); }

.delve-node-here {
    opacity: 1;
    border-color: #4ade80;
    box-shadow: 0 0 14px rgba(74, 222, 128, 0.7);
    font-size: 19px;
}

.delve-node-peek {
    opacity: 0.85;
    border-style: dashed;
}

.delve-node-unknown {
    opacity: 0.35;
    border-style: dashed;
    color: #94a3b8;
    font-weight: bold;
}

.delve-node-reachable {
    opacity: 1;
    cursor: pointer;
    border-color: #fde047;
    box-shadow: 0 0 12px rgba(253, 224, 71, 0.6);
    animation: delve-pulse 1.4s ease-in-out infinite;
}

.delve-node-reachable:hover {
    transform: scale(1.1);
}

@keyframes delve-pulse {
    50% { box-shadow: 0 0 20px rgba(253, 224, 71, 0.9); }
}

.delve-actions,
.delve-buttons {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 8px;
}

.delve-leaders {
    margin: 0;
    padding: 0;
    list-style: none;
    display: flex;
    flex-direction: column;
    gap: 3px;
}

.delve-leaders li {
    display: flex;
    justify-content: space-between;
    padding: 3px 10px;
    border-radius: 8px;
    background: rgba(15, 23, 32, 0.7);
    font-size: 12px;
}

.delve-leader-me {
    border: 1px solid rgba(250, 204, 21, 0.7);
}

.delve-status {
    display: flex;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: 8px;
    padding: 8px 12px;
    border-radius: 10px;
    background: rgba(15, 23, 32, 0.8);
    border: 2px solid rgba(157, 173, 175, 0.5);
    font-weight: bold;
}

.delve-earned {
    display: flex;
    flex-wrap: wrap;
    gap: 12px;
}

.delve-status-active { border-color: rgba(74, 222, 128, 0.6); }
.delve-status-dead { border-color: rgba(248, 113, 113, 0.7); }

.delve-choice {
    display: flex;
    flex-direction: column;
    gap: 8px;
    padding: 10px 12px;
    border-radius: 12px;
    border: 2px solid rgba(250, 204, 21, 0.6);
    background: rgba(66, 45, 16, 0.55);
}

.delve-choice-dead {
    border-color: rgba(248, 113, 113, 0.7);
    background: rgba(60, 14, 14, 0.55);
}

.delve-choice h4 {
    margin: 0;
    font-size: 13px;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.delve-go {
    padding: 10px 12px;
    border-radius: 12px;
    border: 2px dashed rgba(253, 224, 71, 0.45);
    color: #fde68a;
    font-size: 13px;
}

.delve-reward-cards {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-items: flex-start;
    gap: 10px;
    /* Сверху место под подъём плитки при наведении */
    padding-top: 4px;
}

.delve-relic-cards {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
    gap: 8px;
}

.delve-relic-card {
    display: flex;
    flex-direction: column;
    gap: 4px;
    padding: 10px;
    border-radius: 10px;
    border: 2px solid rgba(167, 139, 250, 0.6);
    background: rgba(30, 20, 50, 0.9);
    color: #f8fafc;
    text-align: left;
    cursor: pointer;
}

.delve-relic-card:hover {
    border-color: #fde047;
}

.delve-relic-card span {
    color: #cbd5e1;
    font-size: 12px;
}

.delve-relics {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 6px;
}

.delve-relics-title {
    color: #a78bfa;
    font-weight: bold;
    font-size: 12px;
    text-transform: uppercase;
}

.delve-relic {
    padding: 3px 10px;
    border-radius: 10px;
    background: rgba(139, 92, 246, 0.3);
    border: 1px solid rgba(167, 139, 250, 0.6);
    font-size: 12px;
}

.delve-relic b {
    color: #fde68a;
}

/* ===== Низ: отряд как во вкладке алтаря ===== */

.delve-squad {
    flex-shrink: 0;
    padding: 6px 8px;
    border-radius: 12px;
    background: rgba(8, 12, 20, 0.35);
}

.delve-squad-heading {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    column-gap: 8px;
    row-gap: 4px;
    margin-bottom: 8px;
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
    gap: 10px;
    overflow-x: auto;
    /* Сверху место под звёзды ранга */
    padding: 10px 2px 2px;
}

/* Слот как в ряду отряда алтаря (SquadRow): пунктир, номер места, карточка дворфа */
.delve-squad-slot {
    position: relative;
    box-sizing: border-box;
    flex-shrink: 0;
    width: 148px;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 6px;
    padding: 6px;
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
    width: 120px;
    height: 6px;
    border-radius: 3px;
    background: rgba(0, 0, 0, 0.6);
    overflow: hidden;
}

.delve-hp-fill {
    height: 100%;
    background: linear-gradient(90deg, #22c55e, #86efac);
}

/* Спелл, предмет, пассивка, аура — матрица 2×2 под карточкой */
.delve-slots {
    display: grid;
    grid-template-columns: repeat(2, 48px);
    gap: 6px;
}

.delve-replaces {
    color: #fca5a5;
    font-size: 11px;
    text-align: center;
}

.delve-footer {
    flex: 1;
    min-height: 24px;
    color: #fff;
    font-size: 14px;
    font-weight: bold;
    text-align: left;
}

.delve-footer-error {
    color: #ffb4b4;
}
</style>
