<script lang="ts" setup>
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import GemButton from '@/components/common/GemButton.vue';
import UnitPortrait from '@/components/interface/UnitPortrait.vue';
import SocialApiService from '@/services/apiServices/SocialApiService';
import { useSocial } from '@/composables/UseSocial';
import { useResources } from '@/composables/UseResources';
import type { DelveNodeInfo, DelveNodeType, DelveOverview } from '@/models/types/Social';
import { formatLeft, formatNumber, openExternalFight, socialErrorText } from '@/components/social/socialShared';
import { IconCoin, IconTrophy } from '@/components/icons/ui';

// «Глубинный спуск» — забег-рогалик по карте недели (концепт, сервер: DelveRules / DelveService).
// HP героев сохраняется между боями, реликвии усиливают отряд до конца забега
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

const overview = ref<DelveOverview | null>(null);
const busy = ref(false);
const errorMessage = ref<string | null>(null);
const notice = ref<string | null>(null);
const now = ref(Date.now());
let clockOffset = 0;
let ticker = 0;

const apply = (next: DelveOverview) => {
    overview.value = next;
    clockOffset = new Date(next.serverTime).getTime() - Date.now();
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
    notice.value = null;
    load();
    ticker = window.setInterval(() => (now.value = Date.now()), 1000);
}, { immediate: true });
onBeforeUnmount(() => window.clearInterval(ticker));

const run = computed(() => overview.value?.run ?? null);
const active = computed(() => run.value?.status === 'Active');
const choosing = computed(() => active.value && run.value?.pending !== 'None');

// Этажи сверху вниз: сначала первый, внизу — босс
const floors = computed(() => {
    const map = overview.value?.map ?? [];
    const count = overview.value?.floors ?? 0;
    return Array.from({ length: count }, (_, i) => i + 1).map(floor => ({
        floor,
        nodes: Array.from({ length: overview.value?.columns ?? 3 }, (_, column) => map.find(n => n.floor === floor && n.column === column) ?? null),
    }));
});

const isReachable = (node: DelveNodeInfo) => {
    const r = run.value;
    if (!r || !active.value || choosing.value) return false;
    return node.floor === r.floor + 1 && (r.floor === 0 || Math.abs(node.column - r.column) <= 1);
};
const isCurrent = (node: DelveNodeInfo) => !!run.value && run.value.floor === node.floor && run.value.column === node.column && run.value.floor > 0;
const isPassed = (node: DelveNodeInfo) => !!run.value && node.floor < run.value.floor;

const nextRunIn = computed(() => (overview.value ? formatLeft(new Date(overview.value.nextDayAt).getTime() - now.value - clockOffset) : ''));
const relicName = (key: string) => t(`social.delve.relicNames.${key}`);
const relicHint = (key: string) => t(`social.delve.relicHints.${key}`);
const hpPercent = (unit: { hp: number; maxHp: number }) => (unit.maxHp ? Math.round(unit.hp / unit.maxHp * 100) : 0);

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

// Спуск в узел. Бой уже проведён сервером — показываем его на арене и возвращаемся сюда
const move = async (node: DelveNodeInfo) => {
    if (busy.value || !isReachable(node)) return;
    busy.value = true;
    errorMessage.value = null;
    notice.value = null;
    try {
        const result = await SocialApiService.moveDelve(node.column);
        apply(result.overview);
        if (result.combat) {
            if (result.gold) refreshResources();
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
        width="min(1000px, calc(94 * var(--vw)))"
        height="min(900px, calc(92 * var(--vh)))"
    >
        <div v-if="!overview" class="delve-hint">{{ t('common.loading') }}</div>

        <div v-else class="delve">
            <!-- Карта недели -->
            <section class="delve-map">
                <div v-for="row in floors" :key="row.floor" class="delve-floor">
                    <span class="delve-floor-number">{{ row.floor }}</span>
                    <template v-for="(node, column) in row.nodes" :key="column">
                        <button
                            v-if="node"
                            type="button"
                            class="delve-node"
                            :class="[
                                `delve-node-${node.type.toLowerCase()}`,
                                { 'delve-node-reachable': isReachable(node), 'delve-node-current': isCurrent(node), 'delve-node-passed': isPassed(node) }
                            ]"
                            :disabled="!isReachable(node) || busy"
                            v-tooltip="t(`social.delve.nodes.${node.type}`)"
                            @click="move(node)"
                        >
                            <span class="delve-node-icon">{{ NODE_ICONS[node.type] }}</span>
                        </button>
                        <span v-else class="delve-node-empty"></span>
                    </template>
                </div>
            </section>

            <!-- Забег -->
            <section class="delve-side">
                <p class="delve-hint">{{ t('social.delve.intro') }}</p>
                <p class="delve-hint"><IconTrophy /> {{ t('social.delve.best', { floor: overview.bestFloor }) }}</p>

                <template v-if="run">
                    <div class="delve-status" :class="`delve-status-${run.status.toLowerCase()}`">
                        <span>{{ t(`social.delve.status.${run.status}`) }} · {{ t('social.delve.floor', { floor: run.floor }) }}</span>
                        <span><IconCoin /> {{ t('social.delve.gold', { gold: formatNumber(run.goldEarned) }) }}</span>
                    </div>

                    <!-- Выбор реликвии -->
                    <div v-if="active && run.pending === 'Relic'" class="delve-choice">
                        <h4>{{ t('social.delve.chooseRelic') }}</h4>
                        <div class="delve-relics-offer">
                            <button v-for="key in run.offer" :key="key" type="button" class="delve-relic-card" :disabled="busy" @click="chooseRelic(key)">
                                <b>{{ relicName(key) }}</b>
                                <span>{{ relicHint(key) }}</span>
                            </button>
                        </div>
                    </div>

                    <!-- Привал -->
                    <div v-if="active && run.pending === 'Camp'" class="delve-choice">
                        <h4>🔥 {{ t('social.delve.camp') }}</h4>
                        <div class="delve-camp">
                            <GemButton :color="'green'" :disabled="busy" @click="rest">{{ t('social.delve.rest') }}</GemButton>
                            <GemButton v-for="hero in run.fallen" :key="hero.heroId" :color="'yellow'" :disabled="busy" @click="revive(hero.heroId)">
                                {{ t('social.delve.revive', { name: hero.name }) }}
                            </GemButton>
                        </div>
                    </div>

                    <h4 class="delve-subtitle">{{ t('social.delve.squad') }}</h4>
                    <div class="delve-units">
                        <div v-for="unit in run.units" :key="unit.entityId" class="delve-unit">
                            <UnitPortrait :unit="unit" :size="46" />
                            <div class="delve-hp"><div class="delve-hp-fill" :style="{ width: `${hpPercent(unit)}%` }"></div></div>
                        </div>
                        <div v-for="hero in run.fallen" :key="hero.heroId" class="delve-unit delve-unit-fallen" v-tooltip="t('social.delve.fallen')">
                            <span class="delve-fallen-name">💀 {{ hero.name }}</span>
                        </div>
                    </div>

                    <h4 class="delve-subtitle">{{ t('social.delve.relics') }}</h4>
                    <div class="delve-relics">
                        <span v-for="key in run.relics" :key="key" class="delve-relic" v-tooltip="relicHint(key)">{{ relicName(key) }}</span>
                        <span v-if="!run.relics.length" class="delve-hint">{{ t('social.delve.noRelics') }}</span>
                    </div>
                </template>

                <div class="delve-actions">
                    <GemButton v-if="overview.canStart" :color="'green'" :disabled="busy" @click="start">{{ t('social.delve.start') }}</GemButton>
                    <span v-else-if="!active" class="delve-hint">{{ t('social.delve.nextRun', { time: nextRunIn }) }}</span>
                    <GemButton v-if="active" :color="'red'" :disabled="busy" @click="retreat">{{ t('social.delve.retreat') }}</GemButton>
                </div>

                <template v-if="overview.leaderboard.length">
                    <h4 class="delve-subtitle">{{ t('social.delve.leaderboard') }}</h4>
                    <ol class="delve-leaders">
                        <li v-for="leader in overview.leaderboard" :key="leader.userId" :class="{ 'delve-leader-me': leader.isMe }">
                            <span>{{ leader.place }}. {{ leader.username }}</span>
                            <span>{{ t('social.delve.floor', { floor: leader.floor }) }}</span>
                        </li>
                    </ol>
                </template>
            </section>
        </div>

        <template #modalFooter>
            <div class="delve-footer">
                <span :class="{ 'delve-footer-error': errorMessage }">{{ footerText }}</span>
            </div>
        </template>
    </ModalComponent>
</template>

<style scoped>
.delve {
    flex: 1;
    min-width: 0;
    display: grid;
    grid-template-columns: minmax(220px, 1fr) minmax(0, 1.4fr);
    gap: 14px;
    padding: 6px;
    font-family: 'Arial', sans-serif;
    text-align: left;
    color: #f8fafc;
    font-size: 13px;
}

@media (max-width: 720px) {
    .delve {
        grid-template-columns: 1fr;
    }
}

.delve-hint {
    margin: 0 0 6px;
    color: #cbd5e1;
    font-size: 12px;
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

.delve-floor {
    display: grid;
    grid-template-columns: 22px repeat(3, 1fr);
    align-items: center;
    gap: 8px;
}

.delve-floor-number {
    color: #a78bfa;
    font-weight: bold;
    text-align: right;
}

.delve-node,
.delve-node-empty {
    justify-self: center;
    width: 46px;
    height: 46px;
}

.delve-node {
    display: grid;
    place-items: center;
    border-radius: 50%;
    border: 2px solid rgba(148, 163, 184, 0.4);
    background: rgba(15, 23, 32, 0.85);
    cursor: default;
    opacity: 0.55;
    transition: transform 0.15s, box-shadow 0.15s, opacity 0.15s;
}

.delve-node-icon {
    font-size: 20px;
}

.delve-node-elite { border-color: rgba(248, 113, 113, 0.6); }
.delve-node-camp { border-color: rgba(251, 146, 60, 0.6); }
.delve-node-treasure { border-color: rgba(96, 165, 250, 0.6); }
.delve-node-boss { border-color: rgba(250, 204, 21, 0.8); width: 56px; height: 56px; }

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

.delve-node-current {
    opacity: 1;
    border-color: #4ade80;
    box-shadow: 0 0 14px rgba(74, 222, 128, 0.7);
}

.delve-node-passed {
    opacity: 0.3;
}

@keyframes delve-pulse {
    50% { box-shadow: 0 0 20px rgba(253, 224, 71, 0.9); }
}

.delve-side {
    display: flex;
    flex-direction: column;
    gap: 6px;
    min-width: 0;
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

.delve-status-active { border-color: rgba(74, 222, 128, 0.6); }
.delve-status-dead { border-color: rgba(248, 113, 113, 0.7); }
.delve-status-completed { border-color: rgba(250, 204, 21, 0.8); }

.delve-choice {
    padding: 10px 12px;
    border-radius: 12px;
    border: 2px solid rgba(250, 204, 21, 0.6);
    background: rgba(66, 45, 16, 0.6);
}

.delve-choice h4,
.delve-subtitle {
    margin: 4px 0 6px;
    font-size: 13px;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.delve-relics-offer {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
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

.delve-camp {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
}

.delve-units {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
}

.delve-unit {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 3px;
}

.delve-unit-fallen {
    justify-content: center;
    min-width: 60px;
    min-height: 46px;
    padding: 4px 6px;
    border-radius: 8px;
    background: rgba(60, 14, 14, 0.6);
}

.delve-fallen-name {
    color: #fca5a5;
    font-size: 11px;
}

.delve-hp {
    width: 46px;
    height: 5px;
    border-radius: 3px;
    background: rgba(0, 0, 0, 0.6);
    overflow: hidden;
}

.delve-hp-fill {
    height: 100%;
    background: linear-gradient(90deg, #22c55e, #86efac);
}

.delve-relics {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
}

.delve-relic {
    padding: 3px 10px;
    border-radius: 10px;
    background: rgba(139, 92, 246, 0.3);
    border: 1px solid rgba(167, 139, 250, 0.6);
    font-size: 12px;
}

.delve-actions {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-top: 6px;
}

.delve-leaders {
    margin: 0;
    padding: 0;
    list-style: none;
    display: flex;
    flex-direction: column;
    gap: 4px;
}

.delve-leaders li {
    display: flex;
    justify-content: space-between;
    padding: 4px 10px;
    border-radius: 8px;
    background: rgba(15, 23, 32, 0.7);
}

.delve-leader-me {
    border: 1px solid rgba(250, 204, 21, 0.7);
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
