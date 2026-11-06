<script lang="ts" setup>
import { computed, ref, type Component } from 'vue';
import { AxiosError } from 'axios';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import MineCostView from './MineCostView.vue';
import MiningApiService from '@/services/apiServices/MiningApiService';
import { MiningBoard, MiningError, PickaxeUpgrade, PickaxeUpgradeInfo, canAfford, digEnergyNow } from '@/models/types/Mining';
import { IconCrate, IconHourglass, IconPickaxe } from '@/components/icons/ui';

// Кирка — общая на все шахты: запас кликов (сила кирки) и три её улучшения: ёмкость, восстановление, сила удара
const props = defineProps<{
    board: MiningBoard;
    // Время сервера (мс) — по нему досчитываем восстановление
    serverNow: number;
}>();
const emit = defineEmits<{ (e: 'upgraded', board: MiningBoard): void }>();

const { t, te, locale } = useI18n();

const ICONS: Record<PickaxeUpgrade, Component> = { Capacity: IconCrate, Recovery: IconHourglass, Strength: IconPickaxe };

const busy = ref<PickaxeUpgrade | null>(null);
const message = ref<{ text: string; error: boolean } | null>(null);

const format = (value: number) => Math.round(value).toLocaleString(locale.value);

const energy = computed(() => digEnergyNow(props.board.wallet, props.board.digEnergyMax, props.board.digEnergyPerHour, props.serverNow));

// Что даёт улучшение: запас кликов, кликов в час или процент добычи клика — сейчас и после
const effectText = (info: PickaxeUpgradeInfo) => {
    const next = info.next !== null ? ` → ${format(info.next)}` : '';
    return t(`mining.pickaxe.effects.${info.upgrade}`, { value: `${format(info.value)}${next}` });
};

const upgrade = async (info: PickaxeUpgradeInfo) => {
    if (busy.value) return;
    busy.value = info.upgrade;
    message.value = null;
    try {
        const board = await MiningApiService.upgradePickaxe(info.upgrade);
        emit('upgraded', board);
        message.value = { text: t('mining.pickaxe.upgraded', { name: t(`mining.pickaxe.names.${info.upgrade}`), level: info.level + 1 }), error: false };
    } catch (error) {
        const code = (error as AxiosError<{ code?: MiningError }>).response?.data?.code;
        message.value = { text: code && te(`mining.errors.${code}`) ? t(`mining.errors.${code}`) : t('mining.actionError'), error: true };
    } finally {
        busy.value = null;
    }
};
</script>

<template>
    <section class="pickaxe">
        <!-- Запас кликов на все шахты -->
        <div class="pickaxe-energy">
            <div class="pickaxe-energy-title">
                <IconPickaxe size="26px" />
                <span>{{ t('mining.pickaxe.title') }}</span>
            </div>
            <div class="pickaxe-energy-value">
                {{ t('mining.pickaxe.energy', { energy: Math.floor(energy), max: board.digEnergyMax }) }}
                <span class="pickaxe-energy-rate">{{ t('mining.pickaxe.rate', { rate: format(board.digEnergyPerHour) }) }}</span>
            </div>
            <div class="pickaxe-bar"><div class="pickaxe-bar-fill" :style="{ width: `${(energy / board.digEnergyMax) * 100}%` }"></div></div>
            <div v-if="message" class="pickaxe-message" :class="{ 'pickaxe-message-error': message.error }">{{ message.text }}</div>
        </div>

        <!-- Улучшения кирки -->
        <div class="pickaxe-upgrades">
            <div v-for="info in board.pickaxe" :key="info.upgrade" class="pickaxe-upgrade">
                <div class="pickaxe-upgrade-head">
                    <span class="pickaxe-upgrade-icon"><component :is="ICONS[info.upgrade]" /></span>
                    <span class="pickaxe-upgrade-name">{{ t(`mining.pickaxe.names.${info.upgrade}`) }}</span>
                    <span class="pickaxe-upgrade-level">{{ info.level }} / {{ info.maxLevel }}</span>
                </div>
                <div class="pickaxe-upgrade-effect">{{ effectText(info) }}</div>
                <div class="pickaxe-upgrade-action">
                    <MineCostView v-if="info.cost" :cost="info.cost" :wallet="board.wallet" />
                    <span v-else class="pickaxe-upgrade-max">{{ t('mining.upgrades.maxed') }}</span>
                    <GemButton
                        v-if="info.cost"
                        :color="'green'"
                        :disabled="!!busy || !canAfford(board.wallet, info.cost)"
                        @click="upgrade(info)"
                    >{{ t('mining.upgrades.upgrade') }}</GemButton>
                </div>
            </div>
        </div>
    </section>
</template>

<style scoped>
.pickaxe {
    width: 100%;
    box-sizing: border-box;
    display: grid;
    grid-template-columns: minmax(200px, 240px) 1fr;
    gap: 12px;
    margin-bottom: 12px;
    padding: 10px 12px;
    border-radius: 12px;
    background: rgba(15, 23, 32, 0.8);
    border: 2px solid rgba(157, 173, 175, 0.7);
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5);
    font-family: 'Arial', sans-serif;
}

.pickaxe-energy {
    display: flex;
    flex-direction: column;
    justify-content: center;
    row-gap: 6px;
}

.pickaxe-energy-title {
    display: flex;
    align-items: center;
    column-gap: 8px;
    color: #f8fafc;
    font-size: 15px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.pickaxe-energy-value {
    color: #fde68a;
    font-size: 14px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.pickaxe-energy-rate {
    margin-left: 6px;
    color: #cbd5e1;
    font-size: 12px;
    font-weight: normal;
}

.pickaxe-bar {
    height: 12px;
    overflow: hidden;
    border-radius: 6px;
    background: rgba(0, 0, 0, 0.55);
    box-shadow: inset 0 0 3px #000, 0 0 0 1px rgba(157, 173, 175, 0.45);
}

.pickaxe-bar-fill {
    height: 100%;
    background: linear-gradient(to bottom, #fde68a, #f59e0b);
    transition: width 0.3s ease;
}

.pickaxe-message {
    color: #86efac;
    font-size: 12px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.pickaxe-message-error {
    color: #ffb4b4;
}

/* Три улучшения в ряд, на узком окне — сколько влезет */
.pickaxe-upgrades {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
    gap: 10px;
}

.pickaxe-upgrade {
    display: flex;
    flex-direction: column;
    row-gap: 6px;
    padding: 8px 10px;
    border-radius: 10px;
    background: rgba(0, 0, 0, 0.3);
    box-shadow: inset 0 0 6px rgba(0, 0, 0, 0.6), 0 0 0 1px rgba(157, 173, 175, 0.3);
}

.pickaxe-upgrade-head {
    display: flex;
    align-items: center;
    column-gap: 8px;
    color: #f8fafc;
    font-size: 14px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.pickaxe-upgrade-icon {
    width: 30px;
    height: 30px;
    display: flex;
    justify-content: center;
    align-items: center;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.45);
    box-shadow: inset 0 0 6px #000, 0 0 0 1px rgba(157, 173, 175, 0.5);
    font-size: 18px;
}

.pickaxe-upgrade-name {
    flex: 1;
}

.pickaxe-upgrade-level {
    color: #fde68a;
    font-variant-numeric: tabular-nums;
}

.pickaxe-upgrade-effect {
    color: #cbd5e1;
    font-size: 12px;
}

.pickaxe-upgrade-action {
    display: flex;
    justify-content: space-between;
    align-items: center;
    column-gap: 8px;
    font-size: 13px;
}

.pickaxe-upgrade-action :deep(.gem-text) {
    font-size: 13px;
}

.pickaxe-upgrade-max {
    color: rgba(203, 213, 225, 0.6);
    font-size: 12px;
    font-weight: bold;
}
</style>
