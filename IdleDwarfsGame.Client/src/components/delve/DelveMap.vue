<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { arenaPreview } from '@/battle/arenaBackgrounds';
import { IconCrown } from '@/components/icons/ui';
import type { DelveNodeInfo, DelveRunInfo } from '@/models/types/Social';
import { useDelveMap } from './delveMap';
import DelveNodeSign from './DelveNodeSign.vue';
import DelveSquadMarker from './DelveSquadMarker.vue';

// Путь спуска «подъёмником»: пять рельсовых путей уходят вниз, этажи — ряды. Сверху отряд (аватар игрока),
// ниже весь следующий этаж (по нему ходят), ещё ниже через этаж открыт только узел под отрядом, остальные — «?».
// Узлы — таблички на цепях, переходы — золотые стрелки. Слева шкала глубины региона до босса
const props = defineProps<{
    run: DelveRunInfo;
    columns: number;
    bossEvery: number;
    // Можно ли сейчас идти (забег идёт и ничего не ждёт решения)
    canMove: boolean;
    busy: boolean;
}>();
const emit = defineEmits<{ (e: 'move', node: DelveNodeInfo): void }>();
const { t } = useI18n();
const { region, columnList, nextNode, isBossFloor, nextBoss, bossProgress, inReach, isReachable } = useDelveMap(props);

const ROW = { here: 13, next: 52, peek: 88 };
const x = (column: number) => ((column + 0.5) / props.columns) * 100;

const hereLinks = computed(() => props.run.next.filter(inReach).map(n => ({ key: `h${n.column}`, x2: x(n.column) })));
// Этаж через один: открыт только узел под отрядом, в остальных столбцах — неизвестные узлы
const hiddenColumns = computed(() => columnList.value.filter(c => c !== props.run.peek?.column));
// Возможные пути на этаж через один: из каждого узла следующего этажа в соседние столбцы
const peekLinks = computed(() => props.run.next.flatMap(n => columnList.value
    .filter(c => Math.abs(n.column - c) <= 1)
    .map(c => ({ key: `p${n.column}-${c}`, x1: x(n.column), x2: x(c), known: c === props.run.peek?.column }))));
</script>

<template>
    <div class="delve-map" :style="{ '--arena-color': region.color }">
        <div class="delve-map-bg" :style="{ backgroundImage: `url(${arenaPreview(region.arena)})` }" aria-hidden="true"></div>

        <div class="delve-map-heading">
            <span class="delve-map-title">{{ t('social.delve.map') }}</span>
            <span class="delve-map-boss"><IconCrown size="16px" /> {{ t('social.delve.nextBoss', { floor: nextBoss }) }}</span>
        </div>

        <div class="delve-map-body">
            <!-- Шкала глубины региона: сверху вниз, последнее деление — босс -->
            <div class="delve-map-gauge" v-tooltip="t('social.delve.nextBoss', { floor: nextBoss })">
                <span
                    v-for="step in bossEvery"
                    :key="step"
                    class="delve-map-gauge-step"
                    :class="{
                        'delve-map-gauge-done': step <= bossProgress,
                        'delve-map-gauge-here': step === bossProgress,
                        'delve-map-gauge-boss': step === bossEvery,
                    }"
                ><IconCrown v-if="step === bossEvery" size="12px" /></span>
            </div>

            <div class="delve-map-field">
                <svg class="delve-map-svg" viewBox="0 0 100 100" preserveAspectRatio="none" aria-hidden="true">
                    <g v-for="column in columnList" :key="column" :class="{ 'delve-map-track-own': column === run.column }">
                        <line :x1="x(column)" y1="0" :x2="x(column)" y2="100" class="delve-map-ties" vector-effect="non-scaling-stroke" />
                        <line :x1="x(column)" y1="0" :x2="x(column)" y2="100" class="delve-map-rail" vector-effect="non-scaling-stroke" />
                    </g>
                    <line
                        v-for="link in hereLinks"
                        :key="link.key"
                        :x1="x(run.column)" :y1="ROW.here" :x2="link.x2" :y2="ROW.next"
                        :class="canMove ? 'delve-map-switch-live' : 'delve-map-switch'"
                        vector-effect="non-scaling-stroke"
                    />
                    <line
                        v-for="link in peekLinks"
                        :key="link.key"
                        :x1="link.x1" :y1="ROW.next" :x2="link.x2" :y2="ROW.peek"
                        :class="link.known ? 'delve-map-switch-peek' : 'delve-map-switch-unknown'"
                        vector-effect="non-scaling-stroke"
                    />
                </svg>

                <!-- Номера этажей у левого края ряда -->
                <span class="delve-map-floor" :style="{ top: `${ROW.here}%` }">{{ run.floor || '—' }}</span>
                <span class="delve-map-floor" :class="{ 'delve-map-floor-boss': isBossFloor(run.floor + 1) }" :style="{ top: `${ROW.next}%` }">{{ run.floor + 1 }}</span>
                <span class="delve-map-floor" :class="{ 'delve-map-floor-boss': isBossFloor(run.floor + 2) }" :style="{ top: `${ROW.peek}%` }">{{ run.floor + 2 }}</span>

                <span class="delve-map-slot delve-map-slot-here" :style="{ left: `${x(run.column)}%`, top: `${ROW.here}%` }" v-tooltip="run.floor ? t('social.delve.you') : t('social.delve.entrance')">
                    <DelveSquadMarker :dwarves="run.dwarves" />
                </span>

                <template v-for="column in columnList" :key="column">
                    <button
                        v-if="nextNode(column)"
                        type="button"
                        class="delve-map-slot"
                        :class="{ 'delve-map-slot-reachable': isReachable(nextNode(column)) }"
                        :style="{ left: `${x(column)}%`, top: `${ROW.next}%` }"
                        :disabled="!isReachable(nextNode(column)) || busy"
                        v-tooltip="t(`social.delve.nodes.${nextNode(column)!.type}`)"
                        @click="emit('move', nextNode(column)!)"
                    >
                        <DelveNodeSign
                            :type="nextNode(column)!.type"
                            :reachable="isReachable(nextNode(column))"
                            :far="!inReach(nextNode(column))"
                        />
                    </button>
                </template>

                <span
                    v-if="run.peek"
                    class="delve-map-slot delve-map-slot-peek"
                    :style="{ left: `${x(run.peek.column)}%`, top: `${ROW.peek}%` }"
                    v-tooltip="t(`social.delve.nodes.${run.peek.type}`)"
                >
                    <DelveNodeSign :type="run.peek.type" small />
                </span>
                <span
                    v-for="column in hiddenColumns"
                    :key="`u${column}`"
                    class="delve-map-slot delve-map-slot-peek"
                    :style="{ left: `${x(column)}%`, top: `${ROW.peek}%` }"
                    v-tooltip="t('social.delve.nodes.Unknown')"
                >
                    <DelveNodeSign small />
                </span>
            </div>
        </div>

        <p class="delve-map-hint">{{ canMove ? t('social.delve.chooseNode') : t('social.delve.peekHint') }}</p>
    </div>
</template>

<style scoped>
.delve-map {
    --arena-color: #9ca3af;
    position: relative;
    isolation: isolate;
    height: 100%;
    box-sizing: border-box;
    display: flex;
    flex-direction: column;
    gap: 6px;
    padding: 12px 16px;
    border-radius: 14px;
    overflow: hidden;
    border: 3px solid transparent;
    background:
        linear-gradient(#0d1118, #0d1118) padding-box,
        var(--metalic-background) border-box;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.6);
    font-family: 'Arial', sans-serif;
}

.delve-map-bg {
    position: absolute;
    inset: 0;
    z-index: -1;
    background-size: cover;
    background-position: center;
    opacity: 0.4;
}

/* Внизу темнее — туда уходит глубина */
.delve-map-bg::after {
    content: '';
    position: absolute;
    inset: 0;
    background: linear-gradient(to bottom, color-mix(in srgb, var(--arena-color) 18%, transparent), transparent 40%, rgba(13, 17, 24, 0.92));
}

.delve-map-heading {
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.delve-map-title {
    color: #fff;
    font-size: 13px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 2px;
    text-shadow: 1px 1px 0 #000;
}

.delve-map-boss {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    color: #fde68a;
    font-size: 12px;
    font-weight: bold;
    text-shadow: 1px 1px 0 #000;
}

.delve-map-body {
    flex: 1;
    min-height: 0;
    display: flex;
    gap: 12px;
}

.delve-map-gauge {
    width: 14px;
    display: flex;
    flex-direction: column;
    gap: 3px;
    padding: 3px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.55);
    box-shadow: inset 0 0 0 1px rgba(250, 204, 21, 0.35);
}

.delve-map-gauge-step {
    flex: 1;
    display: grid;
    place-items: center;
    border-radius: 3px;
    background: rgba(148, 163, 184, 0.15);
}

.delve-map-gauge-done {
    background: color-mix(in srgb, var(--arena-color) 75%, transparent);
}

.delve-map-gauge-here {
    background: #4ade80;
    box-shadow: 0 0 6px #4ade80;
}

.delve-map-gauge-boss {
    flex: 1.6;
    background: rgba(250, 204, 21, 0.3);
    --split-icon-primary: #fef08a;
    --split-icon-secondary: #eab308;
}

.delve-map-field {
    position: relative;
    flex: 1;
    min-width: 0;
    margin-left: 26px;
}

.delve-map-svg {
    position: absolute;
    inset: 0;
    width: 100%;
    height: 100%;
    overflow: visible;
}

.delve-map-ties {
    stroke: rgba(120, 85, 50, 0.55);
    stroke-width: 10;
    stroke-dasharray: 3 9;
}

.delve-map-rail {
    stroke: rgba(148, 163, 184, 0.35);
    stroke-width: 2;
}

.delve-map-track-own .delve-map-ties {
    stroke: rgba(160, 110, 60, 0.85);
}

.delve-map-track-own .delve-map-rail {
    stroke: #cbd5e1;
}

.delve-map-switch {
    stroke: rgba(203, 213, 225, 0.4);
    stroke-width: 2.5;
}

.delve-map-switch-live {
    stroke: #fde047;
    stroke-width: 3;
    stroke-dasharray: 8 6;
    filter: drop-shadow(0 0 4px rgba(253, 224, 71, 0.9));
    animation: delve-map-flow 0.9s linear infinite;
}

.delve-map-switch-peek {
    stroke: rgba(203, 213, 225, 0.25);
    stroke-width: 2;
    stroke-dasharray: 3 5;
}

.delve-map-switch-unknown {
    stroke: rgba(203, 213, 225, 0.22);
    stroke-width: 1.5;
    stroke-dasharray: 3 5;
}

@keyframes delve-map-flow {
    to { stroke-dashoffset: -14; }
}

.delve-map-floor {
    position: absolute;
    left: -28px;
    transform: translateY(-50%);
    min-width: 22px;
    padding: 1px 4px;
    border-radius: 6px;
    background: rgba(0, 0, 0, 0.6);
    color: #e9d5ff;
    font-size: 12px;
    font-weight: 900;
    text-align: center;
}

.delve-map-floor-boss {
    color: #1c1917;
    background: #facc15;
}

/* Место узла на карте: центр в точке пути, сам вид — DelveNodeSign / DelveSquadMarker */
.delve-map-slot {
    position: absolute;
    z-index: 1;
    padding: 0;
    border: none;
    background: none;
    transform: translate(-50%, -50%);
    cursor: default;
    transition: transform 0.15s ease;
}

.delve-map-slot-here {
    z-index: 2;
}

.delve-map-slot-reachable {
    cursor: pointer;
}

.delve-map-slot-reachable:hover {
    transform: translate(-50%, -50%) scale(1.08);
}

.delve-map-slot-peek {
    opacity: 0.8;
}

.delve-map-hint {
    margin: 0;
    color: #cbd5e1;
    font-size: 12px;
    text-align: center;
    text-shadow: 1px 1px 0 #000;
}
</style>
