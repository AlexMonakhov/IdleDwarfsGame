<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, ref, watch } from "vue";
import { useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import FightPreviewList from "@/components/interface/FightPreviewList.vue";
import IconButton from "@/components/common/IconButton.vue";
import IconHelm from "@/components/icons/IconHelm.vue";
import CanvasComponent from "@/components/canvas/CanvasComponent.vue";
import CanvasLayer from "@/components/canvas/CanvasLayer.vue";
import TowerRewardLine from "./TowerRewardLine.vue";
import TowerApiService from "@/services/apiServices/TowerApiService";
import { TowerFloorInfo, TowerOverview } from "@/models/types/Tower";
import { RewardInfo } from "@/models/types/Inventory";
import { ELEMENT_COLORS, ELEMENT_ICONS } from "@/components/inventory/lootStyles";
import { useResources } from "@/composables/UseResources";
import { useSquadModal } from "@/composables/UseSquadModal";

const { t } = useI18n();
const { set: setResources } = useResources();

const props = withDefaults(defineProps<{ height?: string }>(), {
    height: '30%'
});

const router = useRouter();
const tower = ref<TowerOverview | null>(null);
const loading = ref(false);
// Флаг, а не текст: сообщение переводится в шаблоне и меняется вместе с языком
const error = ref(false);

const load = async () => {
    loading.value = true;
    error.value = false;
    try {
        tower.value = await TowerApiService.getTower();
    } catch {
        error.value = true;
    } finally {
        loading.value = false;
    }
};

// Бой проигрывается на арене, она сама запросит его у сервера по номеру этажа
const startFloor = (floor: TowerFloorInfo) => {
    if (floor.status !== 'Available' || tower.value?.pendingRewardId) return;
    router.push({ name: 'arena', query: { tower: floor.floor } });
};

// Отряд меняется в общем окне отряда поверх башни; когда его закрыли — перечитываем башню с новым отрядом
const { squadOpen } = useSquadModal();
watch(squadOpen, (isOpen, wasOpen) => {
    if (!isOpen && wasOpen) load();
});

/* ===== Награды ===== */
const busy = ref(false);
const actionError = ref<'claim' | 'skip' | null>(null);
const claimed = ref<RewardInfo | null>(null);
const skippedTo = ref<number | null>(null);

// Награда за пройденный этаж уже выдана на сервере, но на аккаунт попадает только по кнопке
const claimReward = async () => {
    const rewardId = tower.value?.pendingRewardId;
    if (!rewardId || busy.value) return;
    busy.value = true;
    actionError.value = null;
    try {
        const result = await TowerApiService.claimReward(rewardId);
        setResources(result.resources);
        claimed.value = result.reward;
        skippedTo.value = null;
    } catch {
        actionError.value = 'claim';
    } finally {
        busy.value = false;
    }
    // И после ошибки (награду уже забрали в другой вкладке) показываем актуальное состояние
    await load();
};

// Этажи до рекорда прошлых недель засчитываются разом, их награда сразу на аккаунте
const skip = async () => {
    if (!tower.value?.skipToFloor || busy.value) return;
    busy.value = true;
    actionError.value = null;
    try {
        const result = await TowerApiService.skip();
        setResources(result.resources);
        claimed.value = result.reward;
        skippedTo.value = result.skippedTo;
    } catch {
        actionError.value = 'skip';
    } finally {
        busy.value = false;
    }
    await load();
};

/* ===== Неделя ===== */
const now = ref(Date.now());
let clockId = 0;

const timeLeft = computed(() => {
    if (!tower.value) return '';
    const ms = Math.max(0, Date.parse(tower.value.weekEndsAt) - now.value);
    const minutes = Math.floor(ms / 60_000);
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);
    return days > 0
        ? t('tower.timeLeftDays', { days, hours: hours % 24 })
        : t('tower.timeLeftHours', { hours, minutes: minutes % 60 });
});

// Неделя кончилась, пока окно открыто, — башня начинается заново
watch(timeLeft, () => {
    if (tower.value && Date.parse(tower.value.weekEndsAt) <= now.value && !loading.value) load();
});

// Блок, в котором сейчас бой: его стихия подсвечена в круге недели
const currentBlock = computed(() => tower.value ? Math.floor((tower.value.currentFloor - 1) / tower.value.floorsPerBlock) : 0);
const blockRange = (block: number) => {
    const size = tower.value?.floorsPerBlock ?? 5;
    return `${block * size + 1}–${(block + 1) * size}`;
};

const floorTitle = (floor: TowerFloorInfo) => {
    const element = t(`elementNames.${floor.element}`);
    const boss = floor.enemies.find(unit => unit.isBoss);
    return boss
        ? t('tower.bossTitle', { element, name: boss.name })
        : t('tower.floorTitle', { element });
};

onMounted(() => {
    load();
    clockId = window.setInterval(() => { now.value = Date.now(); }, 30_000);
});
onBeforeUnmount(() => clearInterval(clockId));
</script>

<template>
  <div class="tower">
    <div v-if="tower" class="tower-header">
        <div class="tower-rotation" v-tooltip="t('tower.rotationHint', { count: tower.floorsPerBlock })">
            <span
                v-for="(element, block) in tower.rotation"
                :key="block"
                class="tower-rotation-block"
                :class="{ current: block === currentBlock }"
                :style="{ '--element-color': ELEMENT_COLORS[element] }"
            >
                <img v-if="ELEMENT_ICONS[element]" :src="ELEMENT_ICONS[element]" class="tower-element-icon" alt="" />
                <span>{{ blockRange(block) }}</span>
            </span>
            <span class="tower-rotation-more">…</span>
        </div>

        <div class="tower-status">
            <span>{{ t('tower.weekEnds', { time: timeLeft }) }}</span>
            <span>{{ t('tower.cleared', { floor: tower.clearedFloor }) }}</span>
            <span v-if="tower.bestFloor">{{ t('tower.best', { floor: tower.bestFloor }) }}</span>
            <button class="tower-button" v-tooltip="t('tower.squadHint')" @click="squadOpen = true">⚔ {{ t('tower.squad') }}</button>
        </div>

        <!-- Пропуск до рекорда прошлых недель: что он даст — видно сразу -->
        <div v-if="tower.skipToFloor && tower.skipReward" class="tower-skip">
            <span class="tower-skip-hint">{{ t('tower.skipHint') }}</span>
            <TowerRewardLine :reward="tower.skipReward" />
            <button class="tower-button tower-button-gold" :disabled="busy || !!tower.pendingRewardId" @click="skip">
                ⏩ {{ busy ? t('tower.skipping') : t('tower.skip', { floor: tower.skipToFloor }) }}
            </button>
        </div>
    </div>

    <div v-if="claimed" class="tower-reward">
        <span class="tower-reward-title">
            {{ skippedTo ? t('tower.skipped', { floor: skippedTo }) : t('rewards.claimedTitle') }}
        </span>
        <TowerRewardLine :reward="claimed" />
        <span v-if="claimed.chests.length" class="tower-reward-hint">{{ t('rewards.chestsHint') }}</span>
        <button class="tower-reward-close" v-tooltip="t('common.close')" @click="claimed = null">✕</button>
    </div>
    <div v-if="actionError" class="tower-message error">{{ actionError === 'skip' ? t('tower.skipError') : t('rewards.claimError') }}</div>

    <!-- Пока список перезагружается после награды, не прячем его за «Загрузка…» -->
    <div v-if="loading && !tower" class="tower-message">{{ t('common.loading') }}</div>
    <div v-else-if="error" class="tower-message error">{{ t('tower.loadError') }}</div>
    <ul v-else-if="tower" class="list-wrapper">
        <li v-for="floor in tower.floors" :key="floor.floor" class="list" :style="{ height: props.height }">
            <FightPreviewList
              :title="floorTitle(floor)"
              :player-units="tower.squad"
              :enemy-units="floor.enemies"
              :arena="floor.arena"
              :forecast="floor.forecast"
              :locked="floor.status === 'Locked'"
            >
                <template #list-start>
                    <span class="floor-number" :class="{ 'floor-boss': floor.type === 'Boss' }" :style="{ '--element-color': ELEMENT_COLORS[floor.element] }">
                        {{ floor.floor }}
                    </span>
                </template>
                <template #preview-footer>
                    <TowerRewardLine :reward="floor.reward" />
                </template>
                <template #list-end>
                    <!-- Пока награда за пройденный этаж не забрана, вместо боя — получение награды -->
                    <CanvasLayer v-if="floor.status === 'Available' && tower.pendingRewardId" :page="false">
                    <CanvasComponent
                    :effects="[{ func: 'god-rays-radiance', padding: 40, isUnder: true },{ func: 'particles', padding: 100, isUnder: false }]"
                    >
                        <IconButton inner class="floor-claim" v-tooltip="t('rewards.claim')" :disabled="busy" @click="claimReward">
                            <span class="floor-claim-icon">🎁</span>
                            <span class="floor-claim-label">{{ t('rewards.claim') }}</span>
                        </IconButton>
                    </CanvasComponent>
                    </CanvasLayer>
                    <CanvasLayer v-else-if="floor.status === 'Available'" :page="false">
                    <CanvasComponent
                    :effects="[{ func: 'god-rays-radiance', padding: 40, isUnder: true },{ func: 'particles', padding: 100, isUnder: false }]"
                    >
                        <IconButton inner v-tooltip="t('tower.fight')" @click="startFloor(floor)"><IconHelm size="60px" primary-color="rgb(217 215 178)" secondary-color="rgb(64 255 251)"></IconHelm></IconButton>
                    </CanvasComponent>
                    </CanvasLayer>
                    <span v-else class="floor-lock" v-tooltip="t('tower.locked')">🔒</span>
                </template>
            </FightPreviewList>
        </li>
    </ul>
  </div>
</template>

<style scoped>
.tower {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
}

.tower-header {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 10px;
    margin-bottom: 20px;
    text-shadow: 0 1px 3px #000;
}

.tower-rotation {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-items: center;
    gap: 6px;
}

.tower-rotation-block {
    display: flex;
    align-items: center;
    gap: 4px;
    padding: 2px 8px;
    border: 1px solid color-mix(in srgb, var(--element-color) 55%, transparent);
    border-radius: 10px;
    background: rgba(14, 19, 34, 0.6);
    font-size: 12px;
    color: #cbd5e1;
    opacity: 0.75;
}

.tower-rotation-block.current {
    border-color: var(--element-color);
    box-shadow: 0 0 10px var(--element-color);
    color: #f8fafc;
    font-weight: 700;
    opacity: 1;
}

.tower-rotation-more {
    color: #cbd5e1;
}

.tower-element-icon {
    width: 16px;
    height: 16px;
}

.tower-status {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-items: center;
    gap: 6px 18px;
    font-size: 14px;
}

.tower-skip {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-items: center;
    gap: 8px 14px;
    padding: 10px 16px;
    border: 1px solid rgba(255, 209, 102, 0.6);
    border-radius: 12px;
    background: rgba(14, 19, 34, 0.75);
}

.tower-skip-hint {
    flex-basis: 100%;
    text-align: center;
    font-size: 13px;
    color: #cbd5e1;
}

.tower-button {
    border: 1px solid #94a3b8;
    border-radius: 8px;
    padding: 5px 14px;
    background: rgba(14, 19, 34, 0.7);
    color: #f8fafc;
    cursor: pointer;
}

.tower-button-gold {
    border-color: #ffd166;
    color: #ffd166;
    font-weight: 700;
}

.tower-button:disabled {
    opacity: 0.5;
    cursor: default;
}

/* Высота списка — ровно остаток окна: от неё считаются проценты высоты этажей (props.height),
   а сами этажи выходят за список и прокручиваются в теле модалки */
.list-wrapper {
    flex: 1 0 0;
    min-height: 0;
    width: 100%;
    margin: 0;
    padding: 0;
}

.list {
    list-style: none;
    margin-bottom: 25px;
}

.list:last-child {
    margin-bottom: 0;
}

.floor-number {
    color: var(--element-color);
    text-shadow: 0 0 10px var(--element-color), 1px 1px 2px #000000;
}

.floor-boss {
    color: #ffd166;
    text-shadow: 0 0 10px #ff7b00, 0 0 18px var(--element-color), 1px 1px 2px #000000;
}

.floor-lock {
    font-size: 36px;
    filter: drop-shadow(0 2px 3px #000);
}

.tower-message {
    width: 100%;
    margin: auto 0;
    text-align: center;
    font-size: 1.2em;
    text-shadow: 0 1px 3px #000;
}

.tower-message.error {
    color: #fca5a5;
}

.tower-reward {
    position: relative;
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    justify-content: center;
    gap: 6px 16px;
    margin: 0 0 20px;
    padding: 12px 40px;
    border: 1px solid rgba(255, 209, 102, 0.6);
    border-radius: 12px;
    background: rgba(14, 19, 34, 0.75);
    color: #f8fafc;
    text-shadow: 0 1px 3px #000;
}

.tower-reward-title {
    color: #ffd166;
    font-weight: 700;
}

.tower-reward-hint {
    flex-basis: 100%;
    text-align: center;
    font-size: 0.85em;
    color: #cbd5e1;
}

.tower-reward-close {
    position: absolute;
    top: 6px;
    right: 8px;
    border: none;
    background: none;
    color: #cbd5e1;
    cursor: pointer;
}

.floor-claim {
    display: flex;
    flex-direction: column;
    align-items: center;
}

.floor-claim-icon {
    font-size: 40px;
    line-height: 1;
    filter: drop-shadow(0 2px 3px #000);
}

.floor-claim-label {
    margin-top: 2px;
    font-size: 11px;
    font-weight: 700;
    color: #ffd166;
    text-shadow: 0 1px 2px #000;
}
</style>
