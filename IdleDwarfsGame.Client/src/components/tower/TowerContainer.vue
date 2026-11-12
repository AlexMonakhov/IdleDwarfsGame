<script lang="ts" setup>
import { computed, nextTick, onBeforeUnmount, ref, watch } from "vue";
import { useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import ModalComponent from "@/components/common/ModalComponent.vue";
import GemButton from "@/components/common/GemButton.vue";
import FightPreviewList from "@/components/interface/FightPreviewList.vue";
import ClearedStageRow from "@/components/interface/ClearedStageRow.vue";
import StageRewardsFooter from "@/components/interface/StageRewardsFooter.vue";
import TowerRewardLine from "./TowerRewardLine.vue";
import TowerApiService from "@/services/apiServices/TowerApiService";
import { ClearedFloorInfo, TowerFloorInfo, TowerOverview } from "@/models/types/Tower";
import { ELEMENT_COLORS, ELEMENT_ICONS } from "@/components/inventory/lootStyles";
import { useResources } from "@/composables/UseResources";
import { useSquadModal } from "@/composables/UseSquadModal";
import { scrollToAnchor, useStageRewards } from "@/composables/UseStageRewards";
import { vDragScroll } from "@/directives/dragScroll";
import { IconBanner, IconLock, IconTrophy } from '@/components/icons/ui';
import { IconPower } from '@/components/icons/stats';

// Окно бесконечной башни. Шапка (круг стихий, неделя, отряд, пропуск) стоит на месте, прокручивается список:
// пройденные этажи с незабранной наградой (и прошлых недель) и последний пройденный, дальше текущий и заблокированные.
// При открытии список прокручен к последнему пройденному этажу
const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

const { t } = useI18n();
const { set: setResources } = useResources();
const router = useRouter();

const tower = ref<TowerOverview | null>(null);
const loading = ref(false);
// Флаг, а не текст: сообщение переводится в шаблоне и меняется вместе с языком
const error = ref(false);
const scroller = ref<HTMLElement | null>(null);

const load = async (scroll = false) => {
    loading.value = true;
    error.value = false;
    try {
        tower.value = await TowerApiService.getTower();
        rewards.reset();
    } catch {
        error.value = true;
    } finally {
        loading.value = false;
    }
    if (scroll) {
        await nextTick();
        scrollToAnchor(scroller.value);
    }
};

const rewards = useStageRewards({
    unclaimed: () => tower.value?.unclaimedRewards ?? 0,
    claimOne: id => TowerApiService.claimReward(id),
    claimAll: () => TowerApiService.claimAll(),
    reload: () => load(),
});

// Бой проигрывается на арене, она сама запросит его у сервера по номеру этажа
const startFloor = (floor: TowerFloorInfo) => {
    if (floor.status !== 'Available') return;
    // attack — разовая метка: перезагрузка арены покажет итог этого боя, а не начнёт новый
    router.push({ name: 'arena', query: { tower: floor.floor, attack: Date.now() } });
};

// Отряд меняется в общем окне отряда поверх башни; когда его закрыли — перечитываем башню с новым отрядом
const { squadOpen } = useSquadModal();
watch(squadOpen, (isOpen, wasOpen) => {
    if (!isOpen && wasOpen && props.modelValue) load();
});

/* ===== Пропуск ===== */
const skipping = ref(false);
const skipError = ref(false);

// Этажи до рекорда прошлых недель засчитываются разом, их награда сразу на аккаунте
const skip = async () => {
    if (!tower.value?.skipToFloor || skipping.value) return;
    skipping.value = true;
    skipError.value = false;
    try {
        const result = await TowerApiService.skip();
        setResources(result.resources);
        rewards.show(result.reward, result.skippedTo);
    } catch {
        skipError.value = true;
    } finally {
        skipping.value = false;
    }
    await load(true);
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
    if (tower.value && Date.parse(tower.value.weekEndsAt) <= now.value && !loading.value) load(true);
});

// Открыли окно — свежий обзор, часы недели и прокрутка к текущему этажу; закрыли — всё останавливаем
watch(() => props.modelValue, isOpen => {
    clearInterval(clockId);
    if (isOpen) {
        now.value = Date.now();
        clockId = window.setInterval(() => { now.value = Date.now(); }, 30_000);
        load(true);
    } else {
        tower.value = null;
        skipError.value = false;
        rewards.dismiss();
    }
}, { immediate: true });

onBeforeUnmount(() => clearInterval(clockId));

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

const clearedTitle = (floor: ClearedFloorInfo) => t('tower.clearedTitle', { floor: floor.floor, element: t(`elementNames.${floor.element}`) });
const clearedKey = (floor: ClearedFloorInfo) => floor.rewardId ?? `floor-${floor.floor}`;
const lastClearedIndex = computed(() => (tower.value?.clearedFloors.length ?? 0) - 1);
</script>

<template>
  <ModalComponent
    v-model="open"
    :header-message="t('tower.header')"
    width="min(980px, calc(94 * var(--vw)))"
    height="min(780px, calc(88 * var(--vh)))"
  >
    <div class="stage-modal">
        <div v-if="tower" class="stage-modal-header">
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

            <div class="stage-modal-status">
                <span>⏳ {{ t('tower.weekEnds', { time: timeLeft }) }}</span>
                <span>{{ t('tower.cleared', { floor: tower.clearedFloor }) }}</span>
                <span v-if="tower.bestFloor"><IconTrophy /> {{ t('tower.best', { floor: tower.bestFloor }) }}</span>
                <button class="stage-button" v-tooltip="t('tower.squadHint')" @click="squadOpen = true"><IconBanner /> {{ t('tower.squad') }}</button>
            </div>

            <!-- Пропуск до рекорда прошлых недель: что он даст — видно сразу -->
            <div v-if="tower.skipToFloor && tower.skipReward" class="tower-skip">
                <span class="tower-skip-hint">{{ t('tower.skipHint') }}</span>
                <TowerRewardLine :reward="tower.skipReward" />
                <GemButton :color="'green'" :disabled="skipping" @click="skip">
                    ⏩ {{ skipping ? t('tower.skipping') : t('tower.skip', { floor: tower.skipToFloor }) }}
                </GemButton>
            </div>
            <div v-if="skipError" class="stage-message error">{{ t('tower.skipError') }}</div>
        </div>

        <!-- Пока список перезагружается после награды, не прячем его за «Загрузка…» -->
        <div v-if="loading && !tower" class="stage-message">{{ t('common.loading') }}</div>
        <div v-else-if="error && !tower" class="stage-message error">{{ t('tower.loadError') }}</div>
        <div v-else-if="tower" ref="scroller" v-drag-scroll class="stage-scroll">
            <ul class="stage-list">
                <li
                  v-for="(floor, index) in tower.clearedFloors"
                  :key="clearedKey(floor)"
                  :data-anchor="index === lastClearedIndex ? '' : undefined"
                >
                    <ClearedStageRow
                      :number="floor.floor"
                      :title="clearedTitle(floor)"
                      :background="floor.background"
                      :element="floor.element"
                      :boss="floor.type === 'Boss'"
                      :tag="floor.previousWeek ? t('tower.previousWeek') : null"
                      :reward-state="rewards.stateOf(floor.rewardId)"
                      :claiming="rewards.busy.value === floor.rewardId"
                      :disabled="!!rewards.busy.value"
                      @claim="rewards.claim(floor.rewardId)"
                    >
                        <TowerRewardLine
                          v-if="floor.reward && rewards.stateOf(floor.rewardId) === 'pending'"
                          class="cleared-reward"
                          :reward="floor.reward"
                        />
                    </ClearedStageRow>
                </li>

                <li v-for="floor in tower.floors" :key="floor.floor">
                    <FightPreviewList
                      :title="floorTitle(floor)"
                      :player-units="tower.squad"
                      :enemy-units="floor.enemies"
                      :arena="floor.arena"
                      :forecast="floor.forecast"
                      :locked="floor.status === 'Locked'"
                      :current="floor.status === 'Available'"
                      :boss="floor.type === 'Boss'"
                      :element="floor.element"
                    >
                        <template #list-start>
                            <span class="floor-number" :class="{ 'stage-number-boss': floor.type === 'Boss' }" :style="{ '--element-color': ELEMENT_COLORS[floor.element] }">
                                {{ floor.floor }}
                            </span>
                        </template>
                        <template #preview-footer>
                            <TowerRewardLine :reward="floor.reward" />
                        </template>
                        <template #list-end>
                            <GemButton
                              v-if="floor.status === 'Available'"
                              :color="'red'"
                              class="stage-fight stage-fight-ready"
                              @click="startFloor(floor)"
                            ><IconPower /> {{ t('tower.fight') }}</GemButton>
                            <span v-else class="stage-lock" v-tooltip="t('tower.locked')"><IconLock /></span>
                        </template>
                    </FightPreviewList>
                </li>
            </ul>
        </div>
    </div>

    <template #modalFooter>
        <StageRewardsFooter
          :remaining="rewards.remaining.value"
          :busy="rewards.busy.value === 'all'"
          :notice="rewards.notice.value"
          :error="rewards.error.value"
          @claim-all="rewards.claimAll()"
          @dismiss="rewards.dismiss()"
        />
    </template>
  </ModalComponent>
</template>

<style scoped src="@/components/interface/stageModal.css"></style>

<style scoped>
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

.tower-skip {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-items: center;
    gap: 8px 14px;
    padding: 8px 16px;
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

.floor-number {
    color: var(--element-color);
    text-shadow: 0 0 10px var(--element-color), 1px 1px 2px #000000;
}

.floor-number.stage-number-boss {
    color: #ffd166;
    text-shadow: 0 0 10px #ff7b00, 0 0 18px var(--element-color), 1px 1px 2px #000000;
}

/* Награда этажа в строке пройденного — слева, под названием */
.stage-list .cleared-reward {
    justify-content: flex-start;
}
</style>
