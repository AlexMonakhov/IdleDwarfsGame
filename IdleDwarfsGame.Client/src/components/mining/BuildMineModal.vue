<script lang="ts" setup>
import { computed, ref, watch } from 'vue';
import { AxiosError } from 'axios';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import GemButton from '@/components/common/GemButton.vue';
import MineCostView from './MineCostView.vue';
import { mineColor, mineGlyph } from './miningArt';
import MiningApiService from '@/services/apiServices/MiningApiService';
import { MINE_TYPES, MineCost, MineType, MiningActionResult, MiningError, MiningWallet, canAfford } from '@/models/types/Mining';

// Постройка шахты на свободном месте: выбор, что она будет добывать
const props = defineProps<{
    modelValue: boolean;
    plot: number;
    cost: MineCost;
    wallet: MiningWallet;
}>();
const emit = defineEmits<{
    (e: 'update:modelValue', value: boolean): void;
    (e: 'built', result: MiningActionResult): void;
}>();

const { t, te } = useI18n();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

const type = ref<MineType>('Gold');
const busy = ref(false);
const errorMessage = ref<string | null>(null);

watch(open, isOpen => {
    if (!isOpen) return;
    type.value = 'Gold';
    errorMessage.value = null;
});

const isFree = computed(() => !props.cost.gold && !props.cost.experience && !props.cost.crystals);
const affordable = computed(() => isFree.value || canAfford(props.wallet, props.cost));

const build = async () => {
    if (busy.value || !affordable.value) return;
    busy.value = true;
    errorMessage.value = null;
    try {
        const result = await MiningApiService.build(props.plot, type.value);
        emit('built', result);
    } catch (error) {
        const code = (error as AxiosError<{ code?: MiningError }>).response?.data?.code;
        errorMessage.value = code && te(`mining.errors.${code}`) ? t(`mining.errors.${code}`) : t('mining.actionError');
    } finally {
        busy.value = false;
    }
};
</script>

<template>
    <ModalComponent
        v-model="open"
        :header-message="t('mining.build.header')"
        width="min(760px, 94vw)"
        height="min(560px, 90vh)"
    >
        <div class="build-mine">
            <div class="build-mine-hint">{{ t('mining.build.hint', { plot }) }}</div>
            <div class="build-mine-types">
                <button
                    v-for="mineType in MINE_TYPES"
                    :key="mineType"
                    type="button"
                    class="build-mine-type"
                    :class="{ 'build-mine-type-selected': mineType === type }"
                    :style="{ '--mine-color': mineColor(mineType) }"
                    @click="type = mineType"
                >
                    <span class="build-mine-glyph">{{ mineGlyph(mineType) }}</span>
                    <span class="build-mine-name">{{ t(`mining.types.${mineType}`) }}</span>
                </button>
            </div>
        </div>

        <template #modalFooter>
            <div class="build-mine-footer">
                <span v-if="errorMessage" class="build-mine-error">{{ errorMessage }}</span>
                <span v-else class="build-mine-cost">
                    {{ t('mining.build.cost') }}
                    <template v-if="isFree">{{ t('mining.build.free') }}</template>
                    <MineCostView v-else :cost="cost" :wallet="wallet" />
                </span>
                <GemButton :color="'green'" :disabled="busy || !affordable" @click="build">
                    {{ busy ? t('mining.build.building') : t('mining.build.build') }}
                </GemButton>
            </div>
        </template>
    </ModalComponent>
</template>

<style scoped>
.build-mine {
    flex: 1;
    display: flex;
    flex-direction: column;
    font-family: 'Arial', sans-serif;
}

.build-mine-hint {
    margin-bottom: 12px;
    color: #cbd5e1;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.build-mine-types {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
    gap: 12px;
    padding: 4px;
}

.build-mine-type {
    --mine-color: #cbd5e1;
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 6px;
    padding: 14px 8px 12px;
    border-radius: 12px;
    border: 2px solid rgba(157, 173, 175, 0.6);
    background: rgba(15, 23, 32, 0.8);
    color: inherit;
    font: inherit;
    cursor: pointer;
    transition: transform 0.12s ease, border-color 0.15s ease, box-shadow 0.15s ease;
}

.build-mine-type:hover {
    transform: translateY(-2px);
    border-color: var(--mine-color);
}

.build-mine-type-selected {
    border-color: var(--mine-color);
    box-shadow: 0 0 14px 2px var(--mine-color);
}

.build-mine-glyph {
    color: var(--mine-color);
    font-size: 36px;
    line-height: 1;
    filter: drop-shadow(0 0 6px var(--mine-color));
}

.build-mine-name {
    color: #f8fafc;
    font-size: 13px;
    font-weight: bold;
}

.build-mine-footer {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: flex-end;
    column-gap: 12px;
    font-family: 'Arial', sans-serif;
}

.build-mine-cost,
.build-mine-error {
    flex: 1;
    display: flex;
    align-items: baseline;
    column-gap: 8px;
    color: #fff;
    font-size: 14px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.build-mine-error {
    color: #ffb4b4;
}
</style>
