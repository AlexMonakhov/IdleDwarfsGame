<script lang="ts" setup>
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import ConfirmModal from '@/components/common/ConfirmModal.vue';
import GemButton from '@/components/common/GemButton.vue';
import UnitPortrait from '@/components/interface/UnitPortrait.vue';
import SpellName from '@/components/icons/spells/SpellName.vue';
import AuraName from '@/components/icons/status/AuraName.vue';
import SocialApiService from '@/services/apiServices/SocialApiService';
import { useSocial } from '@/composables/UseSocial';
import { useResources } from '@/composables/UseResources';
import { i18n, statusLabel } from '@/i18n';
import type { HeroElement } from '@/battle/types';
import type { Rarity } from '@/models/types/Inventory';
import type { DelveDwarfInfo, DelveNodeInfo, DelveNodeType, DelveOverview, DelvePassiveValues, DelveRewardInfo } from '@/models/types/Social';
import { ELEMENT_COLORS, RARITY_COLORS } from '@/components/inventory/lootStyles';
import { formatLeft, formatNumber, openExternalFight, socialErrorText } from '@/components/social/socialShared';
import { IconCoin, IconExperience, IconRainbowCoin, IconTrophy } from '@/components/icons/ui';

// «Глубинный спуск» — бесконечный забег своим отрядом (сервер: DelveRules / DelveService).
// Видно только следующий этаж и узел через этаж под текущим столбцом. После каждого узла — награда на выбор:
// стихия и спелл достаются дворфу сами, остальное игрок надевает на выбранного дворфа (второй клик)
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

const REWARD_ICONS: Record<DelveRewardInfo['kind'], string> = {
    Element: '💠',
    Spell: '✨',
    Item: '🗡️',
    Passive: '📜',
    Aura: '🌀',
    LevelUp: '⭐',
};

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
const elementName = (element: HeroElement | null) => (element ? t(`elementNames.${element}`) : '');
const itemName = (key: string) => (i18n.global.te(`equipment.items.${key}`) ? t(`equipment.items.${key}`) : key);
const passiveName = (key: string) => (i18n.global.te(`passives.names.${key}`) ? t(`passives.names.${key}`) : key);
const passiveText = (key: string, values: DelvePassiveValues | null) =>
    values ? t(`passives.descriptions.${key}`, { percent: values.percent, turns: values.turns, targets: values.targets }) : '';
const auraName = (effect: string) => t('abilities.auraName', { effect: statusLabel(effect) });
const rarityColor = (rarity: string | null) => (rarity ? RARITY_COLORS[rarity as Rarity] : undefined);

/* ===== Награды ===== */

const rewardTitle = (reward: DelveRewardInfo) => {
    switch (reward.kind) {
        case 'Element': return elementName(reward.element);
        case 'Spell': return `${reward.spellName ?? ''} ${reward.level}`;
        case 'Item': return itemName(reward.key ?? '');
        case 'Passive': return `${passiveName(reward.key ?? '')} · ${t('social.delve.level', { level: reward.level })}`;
        case 'Aura': return `${auraName(reward.key ?? '')} · ${t('social.delve.level', { level: reward.level })}`;
        default: return t('social.delve.rewardKinds.LevelUp');
    }
};

const rewardText = (reward: DelveRewardInfo) => {
    switch (reward.kind) {
        case 'Element': return t('social.delve.rewardElement');
        case 'Spell': return t('social.delve.rewardSpell', { element: elementName(reward.element) });
        case 'Item': return reward.rarity ? t(`rarity.${reward.rarity}`) : '';
        case 'Passive': return passiveText(reward.key ?? '', reward.passiveValues);
        case 'Aura': return '';
        default: return t('social.delve.rewardLevelUp', { element: elementName(reward.element) });
    }
};

const picked = computed(() => (pickedReward.value != null ? run.value?.rewards[pickedReward.value] ?? null : null));

// Что награда заменит у дворфа — подсказка при выборе цели
const replaces = (dwarf: DelveDwarfInfo): string | null => {
    const reward = picked.value;
    if (!reward) return null;
    if (reward.kind === 'Item' && dwarf.itemKey) return itemName(dwarf.itemKey);
    if (reward.kind === 'Passive' && dwarf.passive) return passiveName(dwarf.passive);
    if (reward.kind === 'Aura' && dwarf.auraEffect) return auraName(dwarf.auraEffect);
    return null;
};

const canTake = (dwarf: DelveDwarfInfo) => !!picked.value && picked.value.fits.includes(dwarf.heroId);

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
    act(() => SocialApiService.chooseDelveReward(index), rewardTitle(reward));
};

const giveTo = (dwarf: DelveDwarfInfo) => {
    const index = pickedReward.value;
    const reward = picked.value;
    if (index == null || !reward || !canTake(dwarf)) return;
    act(() => SocialApiService.chooseDelveReward(index, dwarf.heroId), `${rewardTitle(reward)} → ${dwarf.name}`);
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
    if (!active.value || picked.value) return;
    dragFrom.value = dwarf.heroId;
    event.dataTransfer?.setData('text/plain', dwarf.heroId);
};
const onDrop = (dwarf: DelveDwarfInfo) => {
    if (dragFrom.value) swap(dragFrom.value, dwarf.heroId);
    dragFrom.value = null;
    swapFrom.value = null;
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
        width="min(1080px, calc(96 * var(--vw)))"
        height="min(900px, calc(92 * var(--vh)))"
    >
        <div v-if="!overview" class="delve-hint">{{ t('common.loading') }}</div>

        <div v-else class="delve">
            <!-- Путь: текущий этаж, следующий целиком, через этаж — только узел под вами -->
            <section class="delve-left">
                <div class="delve-map">
                    <h4 class="delve-subtitle">{{ t('social.delve.map') }}</h4>
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

                <p class="delve-hint"><IconTrophy /> {{ t('social.delve.best', { floor: overview.bestFloor }) }}</p>

                <div class="delve-actions">
                    <GemButton v-if="overview.canStart" :color="'green'" :disabled="busy" @click="start">{{ t('social.delve.start') }}</GemButton>
                    <span v-else-if="!active" class="delve-hint">{{ t('social.delve.nextRun', { time: nextRunIn }) }}</span>
                    <GemButton v-if="active" :color="'red'" :disabled="busy" @click="retreatConfirmOpen = true">{{ t('social.delve.retreat') }}</GemButton>
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

            <!-- Забег -->
            <section class="delve-side">
                <p v-if="run" class="delve-hint">{{ t('social.delve.intro') }}</p>

                <template v-if="run">
                    <div class="delve-status" :class="`delve-status-${run.status.toLowerCase()}`">
                        <span>{{ t(`social.delve.status.${run.status}`) }} · {{ t('social.delve.floor', { floor: run.floor }) }}</span>
                        <span class="delve-earned">
                            {{ t('social.delve.earned') }}:
                            <span><IconCoin /> {{ formatNumber(run.goldEarned) }}</span>
                            <span><IconExperience /> {{ formatNumber(run.experienceEarned) }}</span>
                            <span v-if="run.rainbowCoinsEarned"><IconRainbowCoin /> {{ run.rainbowCoinsEarned }}</span>
                        </span>
                    </div>

                    <!-- Пал весь отряд: раз за забег можно воскресить всех -->
                    <div v-if="dead" class="delve-choice delve-choice-dead">
                        <template v-if="!run.revived">
                            <p class="delve-hint">{{ t('social.delve.reviveHint') }}</p>
                            <GemButton :color="'yellow'" :disabled="busy || !run.canRevive" @click="reviveAll">
                                {{ t('social.delve.reviveAll', { cost: overview.reviveCost }) }} <IconRainbowCoin />
                            </GemButton>
                        </template>
                        <p v-else class="delve-hint">{{ t('social.delve.reviveUsed') }}</p>
                    </div>

                    <!-- Выбор реликвии -->
                    <div v-if="active && run.pending === 'Relic'" class="delve-choice">
                        <h4>{{ t('social.delve.chooseRelic') }}</h4>
                        <div class="delve-cards">
                            <button v-for="key in run.offer" :key="key" type="button" class="delve-card" :disabled="busy" @click="chooseRelic(key)">
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
                            <GemButton v-for="hero in fallen" :key="hero.heroId" :color="'yellow'" :disabled="busy" @click="revive(hero.heroId)">
                                {{ t('social.delve.revive', { name: hero.name }) }}
                            </GemButton>
                        </div>
                    </div>

                    <!-- Награда узла -->
                    <div v-if="active && run.pending === 'Reward'" class="delve-choice">
                        <h4>{{ picked ? t('social.delve.chooseTarget', { reward: rewardTitle(picked) }) : t('social.delve.chooseReward') }}</h4>
                        <div class="delve-cards">
                            <button
                                v-for="(reward, index) in run.rewards"
                                :key="index"
                                type="button"
                                class="delve-card"
                                :class="{ 'delve-card-picked': pickedReward === index }"
                                :style="{ borderColor: rarityColor(reward.rarity) ?? (reward.element ? ELEMENT_COLORS[reward.element] : undefined) }"
                                :disabled="busy"
                                @click="chooseReward(index)"
                            >
                                <span class="delve-card-kind">{{ REWARD_ICONS[reward.kind] }} {{ t(`social.delve.rewardKinds.${reward.kind}`) }}</span>
                                <b v-if="reward.kind === 'Spell' && reward.spellName"><SpellName :spell="`${reward.spellName} ${reward.level}`" /></b>
                                <b v-else-if="reward.kind === 'Aura' && reward.key"><AuraName :aura="reward.key" /> · {{ t('social.delve.level', { level: reward.level }) }}</b>
                                <b v-else :style="{ color: rarityColor(reward.rarity) }">{{ rewardTitle(reward) }}</b>
                                <span>{{ rewardText(reward) }}</span>
                            </button>
                        </div>
                        <div class="delve-camp">
                            <GemButton v-if="picked" :color="'blue'" :disabled="busy" @click="pickedReward = null">{{ t('social.delve.cancel') }}</GemButton>
                            <GemButton :color="'yellow'" :disabled="busy" @click="decline">{{ t('social.delve.decline') }}</GemButton>
                        </div>
                    </div>

                    <h4 class="delve-subtitle">{{ t('social.delve.squad') }}</h4>
                    <p v-if="active" class="delve-hint">{{ t('social.delve.squadHint') }}</p>
                    <div class="delve-dwarves">
                        <div
                            v-for="dwarf in dwarves"
                            :key="dwarf.heroId"
                            class="delve-dwarf"
                            :class="{
                                'delve-dwarf-fallen': dwarf.hp <= 0,
                                'delve-dwarf-selected': swapFrom === dwarf.heroId,
                                'delve-dwarf-target': canTake(dwarf),
                                'delve-dwarf-unfit': picked && !canTake(dwarf),
                            }"
                            :draggable="active && !picked"
                            @click="onDwarfClick(dwarf)"
                            @dragstart="onDragStart(dwarf, $event)"
                            @dragover.prevent
                            @drop.prevent="onDrop(dwarf)"
                        >
                            <span class="delve-dwarf-place">{{ dwarf.position }}</span>
                            <UnitPortrait :unit="dwarf.unit" :size="52" />
                            <div class="delve-hp"><div class="delve-hp-fill" :style="{ width: `${Math.round(dwarf.hp * 100)}%` }"></div></div>
                            <div class="delve-dwarf-info">
                                <b>{{ dwarf.hp <= 0 ? '💀 ' : '' }}{{ dwarf.name }}</b>
                                <span :style="{ color: ELEMENT_COLORS[dwarf.element] }">{{ t('social.delve.rank', { rank: dwarf.level }) }} · {{ elementName(dwarf.element) }}</span>
                            </div>
                            <ul class="delve-slots">
                                <li>
                                    <span class="delve-slot-name">{{ t('social.delve.slots.spell') }}</span>
                                    <SpellName v-if="dwarf.spellName" :spell="`${dwarf.spellName} ${dwarf.spellLevel}`" />
                                    <span v-else>{{ t('social.delve.empty') }}</span>
                                </li>
                                <li>
                                    <span class="delve-slot-name">{{ t('social.delve.slots.item') }}</span>
                                    <span>{{ dwarf.itemKey ? itemName(dwarf.itemKey) : t('social.delve.empty') }}</span>
                                </li>
                                <li v-tooltip="dwarf.passive ? passiveText(dwarf.passive, dwarf.passiveValues) : undefined">
                                    <span class="delve-slot-name">{{ t('social.delve.slots.passive') }}</span>
                                    <span>{{ dwarf.passive ? `${passiveName(dwarf.passive)} ${dwarf.passiveLevel}` : t('social.delve.empty') }}</span>
                                </li>
                                <li>
                                    <span class="delve-slot-name">{{ t('social.delve.slots.aura') }}</span>
                                    <span v-if="dwarf.auraEffect"><AuraName :aura="dwarf.auraEffect" /> {{ dwarf.auraLevel }}</span>
                                    <span v-else>{{ t('social.delve.empty') }}</span>
                                </li>
                            </ul>
                            <span v-if="canTake(dwarf) && replaces(dwarf)" class="delve-replaces">{{ t('social.delve.replaces', { old: replaces(dwarf) }) }}</span>
                        </div>
                    </div>

                    <h4 class="delve-subtitle">{{ t('social.delve.relics') }}</h4>
                    <div class="delve-relics">
                        <span v-for="key in run.relics" :key="key" class="delve-relic" v-tooltip="relicHint(key)">{{ relicName(key) }}</span>
                        <span v-if="!run.relics.length" class="delve-hint">{{ t('social.delve.noRelics') }}</span>
                    </div>
                </template>
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
    display: grid;
    grid-template-columns: minmax(260px, 0.9fr) minmax(0, 1.6fr);
    gap: 14px;
    padding: 6px;
    font-family: 'Arial', sans-serif;
    text-align: left;
    color: #f8fafc;
    font-size: 13px;
}

@media (max-width: 760px) {
    .delve {
        grid-template-columns: 1fr;
    }
}

.delve-hint {
    margin: 0 0 6px;
    color: #cbd5e1;
    font-size: 12px;
}

.delve-left,
.delve-side {
    display: flex;
    flex-direction: column;
    gap: 6px;
    min-width: 0;
}

.delve-map {
    display: flex;
    flex-direction: column;
    gap: 10px;
    padding: 12px;
    border-radius: 14px;
    background: radial-gradient(ellipse at top, rgba(60, 40, 90, 0.6), rgba(8, 10, 18, 0.9));
    border: 2px solid rgba(139, 92, 246, 0.4);
}

.delve-map-hint {
    margin: 0;
    font-size: 11px;
}

.delve-floor {
    display: grid;
    grid-template-columns: 26px repeat(5, 1fr);
    align-items: center;
    gap: 6px;
    padding: 4px 0;
    border-radius: 10px;
}

.delve-floor-boss {
    background: rgba(250, 204, 21, 0.08);
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
    gap: 10px;
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
    background: rgba(66, 45, 16, 0.6);
}

.delve-choice-dead {
    border-color: rgba(248, 113, 113, 0.7);
    background: rgba(60, 14, 14, 0.55);
}

.delve-choice h4,
.delve-subtitle {
    margin: 4px 0 2px;
    font-size: 13px;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.delve-cards {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
    gap: 8px;
}

.delve-card {
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
    transition: transform 0.15s, box-shadow 0.15s;
}

.delve-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 0 10px rgba(253, 224, 71, 0.4);
}

.delve-card-picked {
    box-shadow: 0 0 14px rgba(253, 224, 71, 0.9);
    background: rgba(66, 45, 16, 0.95);
}

.delve-card-kind {
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 1px;
    color: #fde68a !important;
}

.delve-card span {
    color: #cbd5e1;
    font-size: 12px;
}

.delve-camp {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
}

.delve-dwarves {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
    gap: 8px;
}

.delve-dwarf {
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
    padding: 8px;
    border-radius: 12px;
    border: 2px solid rgba(148, 163, 184, 0.35);
    background: rgba(15, 23, 32, 0.8);
    cursor: pointer;
    user-select: none;
    transition: border-color 0.15s, box-shadow 0.15s, opacity 0.15s;
}

.delve-dwarf-place {
    position: absolute;
    top: 4px;
    left: 8px;
    color: #a78bfa;
    font-weight: bold;
}

.delve-dwarf-fallen {
    background: rgba(60, 14, 14, 0.6);
}

.delve-dwarf-selected {
    border-color: #60a5fa;
    box-shadow: 0 0 12px rgba(96, 165, 250, 0.7);
}

.delve-dwarf-target {
    border-color: #fde047;
    box-shadow: 0 0 12px rgba(253, 224, 71, 0.6);
}

.delve-dwarf-unfit {
    opacity: 0.4;
    cursor: not-allowed;
}

.delve-dwarf-info {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 1px;
    font-size: 12px;
}

.delve-slots {
    width: 100%;
    margin: 0;
    padding: 0;
    list-style: none;
    display: flex;
    flex-direction: column;
    gap: 2px;
    font-size: 11px;
}

.delve-slots li {
    display: flex;
    justify-content: space-between;
    gap: 6px;
    min-width: 0;
}

.delve-slots li > span:last-child {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    text-align: right;
}

.delve-slot-name {
    color: #94a3b8;
    flex-shrink: 0;
}

.delve-replaces {
    color: #fca5a5;
    font-size: 11px;
}

.delve-hp {
    width: 52px;
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
