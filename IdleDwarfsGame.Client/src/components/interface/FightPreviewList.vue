<script lang="ts" setup>

import { computed } from 'vue';
import { CombatUnitInfo } from '@/battle/types';
import { forecastVerdict } from '@/battle/power';
import { ArenaInfo, BattleForecast } from '@/models/types/Campaign';
import { STATUS_ICONS } from '@/components/icons/status';
import UnitPortrait from './UnitPortrait.vue';
import TooltipComponent from '@/components/common/TooltipComponent.vue';
import background from "@/assets/modal-background.png";
import { useI18n } from 'vue-i18n';
import { statusLabel } from '@/i18n';

const { t } = useI18n();

interface Props {
    title?: string,
    playerUnits?: CombatUnitInfo[],
    enemyUnits?: CombatUnitInfo[],
    arena?: ArenaInfo | null,
    forecast?: BattleForecast | null,
    locked?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  title: '',
  playerUnits: () => [],
  enemyUnits: () => [],
  arena: null,
  forecast: null,
  locked: false
})

const chance = computed(() => Math.round((props.forecast?.winChance ?? 0) * 100));
const verdict = computed(() => forecastVerdict(props.forecast?.winChance ?? 0));

const backgroundStyle = `url(${background}) center / cover no-repeat, radial-gradient( #e1e8eb, #4c6a8d)`;
</script>

<template>
    <div class="list-item-wrapper" :class="{ locked: props.locked }">
            <div class="list-start"><slot name="list-start"></slot></div>
            <div class="list-middle" :style="{background: backgroundStyle}">
                <slot name="list-middle">
                    <div class="preview">
                        <div v-if="props.title" class="preview-title">{{ props.title }}</div>
                        <div class="preview-body">
                            <div class="preview-team preview-team-player">
                                <UnitPortrait v-for="unit in props.playerUnits" :key="`p-${unit.position}`" :unit="unit" :size="40" />
                            </div>

                            <TooltipComponent v-if="props.arena" class="preview-arena">
                                <div class="arena-effect">
                                    <component
                                      v-if="props.arena.effectName && STATUS_ICONS[props.arena.effectName]"
                                      :is="STATUS_ICONS[props.arena.effectName]"
                                      size="34px" />
                                    <span v-else class="arena-vs">VS</span>
                                </div>
                                <template #title>{{ props.arena.name }}</template>
                                <template #description>{{ props.arena.description }}</template>
                                <template v-if="props.arena.effectName" #stats>
                                    {{ t('campaign.arenaEffect', { effect: statusLabel(props.arena.effectName), power: props.arena.effectPower }) }}
                                </template>
                            </TooltipComponent>

                            <div class="preview-team">
                                <UnitPortrait v-for="unit in props.enemyUnits" :key="`e-${unit.position}`" :unit="unit" :size="40" />
                            </div>
                        </div>
                        <div v-if="props.forecast" class="preview-forecast">
                            <span class="forecast-power">⚔ {{ props.forecast.playerPower }}</span>
                            <span class="forecast-verdict" :class="`forecast-${verdict}`" :title="t('power.chance', { chance })">
                                {{ t(`power.verdict.${verdict}`) }} · {{ chance }}%
                            </span>
                            <span class="forecast-power">⚔ {{ props.forecast.enemyPower }}</span>
                        </div>
                    </div>
                </slot>
            </div>
            <div class="list-end"><slot name="list-end"></slot></div>
    </div>
</template>

<style scoped>
.list-item-wrapper {
    position: relative;
    width: 100%;
    height: 100%;
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-radius: 15px;
    /* Убираем фон отсюда, переносим в after */
    background: transparent;
}

/* СЛОЙ 1: Внешняя рамка и свечение (Металл) */
.list-item-wrapper::before {
    content: '';
    position: absolute;
    inset: -5px; /* Растягиваем чуть шире блока */
    background: var(--metalic-background);
    border-radius: 15px;
    box-shadow: 0 4px 3px 2px #181717, 0 3px 20px 4px #65a7b8;
    z-index: 1; /* Даем положительный индекс */
}

.list-item-wrapper:hover{
    /* Усиливаем свечение при наведении */
    transform: scale(1.01);
    box-shadow: 0 6px 5px 3px #181717, 0 5px 25px 5px #65a7b8;
}

/* Заблокированный уровень: приглушён, без свечения и реакции на наведение */
.list-item-wrapper.locked {
    filter: grayscale(0.85) brightness(0.65);
}

.list-item-wrapper.locked::before {
    box-shadow: 0 4px 3px 2px #181717;
}

.list-item-wrapper.locked:hover {
    transform: none;
    box-shadow: none;
}

.list-start, .list-middle, .list-end {
    position: relative;
    z-index: 3; /* Самый верхний слой */
    height: 100%;
    display: flex;
    justify-content: center;
    align-items: center;

}

.list-start {
    border-top-left-radius: 15px;
    border-bottom-left-radius: 15px;
    box-shadow: inset 0px 0px 6px black, inset 0px 0px 20px #333;
    font-size: 3em;
    font-weight: 900;
    font-family: RunesFont, sans-serif;
    text-shadow: 1px 1px 2px #000000;
}

.list-middle{
    flex:0.99;
    /* Можно уже содержимого: на узком окне портреты переносятся, а не выталкивают строку вбок */
    min-width: 0;
    box-shadow: inset 0px 0px 6px black, inset 0px 0px 20px #333;
}

.list-end {
    border-top-right-radius: 15px;
    border-bottom-right-radius: 15px;
    box-shadow: inset 0px 0px 6px black, inset 0px 0px 20px #333;

}

.list-start, .list-end {
    min-width: 100px;
    background: var(--wood-background);
}

.preview {
    width: 100%;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 6px;
    padding: 0 12px;
    box-sizing: border-box;
}

.preview-title {
    font-weight: 800;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    text-shadow: 0 1px 3px #000, 0 0 6px #000;
}

.preview-body {
    width: 100%;
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 10px;
}

.preview-team {
    min-width: 0;
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
}

/* Как на арене: передняя линия (позиция 1) у обеих команд ближе к центру */
.preview-team-player {
    flex-direction: row-reverse;
}

.arena-effect {
    width: 46px;
    height: 46px;
    display: flex;
    justify-content: center;
    align-items: center;
    border-radius: 50%;
    background: var(--wood-background);
    box-shadow: 0 0 0 3px rgb(157, 173, 175), 0 0 0 5px #333, 0 0 14px 4px #65a7b8;
}

.preview-forecast {
    width: 100%;
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 13px;
    font-weight: 700;
    text-shadow: 0 1px 3px #000, 0 0 6px #000;
}

.forecast-power {
    color: #ffd166;
}

.forecast-verdict {
    padding: 1px 10px;
    border-radius: 10px;
    background: rgba(0, 0, 0, 0.45);
    border: 1px solid currentColor;
}

.forecast-easy { color: #4ade80; }
.forecast-likely { color: #bef264; }
.forecast-risky { color: #fbbf24; }
.forecast-deadly { color: #f87171; }

.arena-vs {
    font-weight: 900;
    font-family: RunesFont, sans-serif;
}
</style>
