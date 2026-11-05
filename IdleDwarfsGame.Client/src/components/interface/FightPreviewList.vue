<script lang="ts" setup>

import { computed } from 'vue';
import { CombatUnitInfo, HeroElement } from '@/battle/types';
import { forecastVerdict } from '@/battle/power';
import { arenaPreview } from '@/battle/arenaBackgrounds';
import { ArenaInfo, BattleForecast } from '@/models/types/Campaign';
import { STATUS_ICONS } from '@/components/icons/status';
import { ELEMENT_COLORS } from '@/components/inventory/lootStyles';
import UnitPortrait from './UnitPortrait.vue';
import TooltipComponent from '@/components/common/TooltipComponent.vue';
import CssAuraEffect from '@/components/effects/CssAuraEffect.vue';
import { elementAura } from '@/components/effects/elementAura';
import { useI18n } from 'vue-i18n';
import { statusLabel } from '@/i18n';

const { t } = useI18n();

// Карточка боя кампании или этажа башни: фон — арена боя, рамка и свечение — её стихия.
// Слева номер (слот list-start), справа кнопка боя (list-end), под прогнозом — что-то своё для режима (preview-footer)
interface Props {
    title?: string,
    playerUnits?: CombatUnitInfo[],
    enemyUnits?: CombatUnitInfo[],
    arena?: ArenaInfo | null,
    forecast?: BattleForecast | null,
    locked?: boolean,
    // Бой, который можно начать прямо сейчас: карточка горит
    current?: boolean,
    boss?: boolean,
    // Стихия рамки; по умолчанию — стихия арены
    element?: HeroElement | null,
    portraitSize?: number
}

const props = withDefaults(defineProps<Props>(), {
  title: '',
  playerUnits: () => [],
  enemyUnits: () => [],
  arena: null,
  forecast: null,
  locked: false,
  current: false,
  boss: false,
  element: null,
  portraitSize: 56
})

const chance = computed(() => Math.round((props.forecast?.winChance ?? 0) * 100));
const verdict = computed(() => forecastVerdict(props.forecast?.winChance ?? 0));

const element = computed(() => props.element ?? props.arena?.element ?? 'Basic');
const accent = computed(() => ELEMENT_COLORS[element.value] ?? ELEMENT_COLORS.Basic);

// Значок арены между отрядами. Аура стихии (снег, угли, искры…) — только у открытых боёв, у заблокированных она бы рябила
const ARENA_ICON_SIZE = 50;
const aura = computed(() => (props.locked ? 'none' : elementAura(element.value)));
const backgroundImage = computed(() => `url(${arenaPreview(props.arena?.background)})`);
</script>

<template>
    <div class="stage-card" :class="{ locked: props.locked, current: props.current, boss: props.boss }" :style="{ '--accent': accent }">
        <div class="stage-card-arena" aria-hidden="true">
            <div class="stage-card-arena-image" :style="{ backgroundImage }"></div>
        </div>

        <div class="list-start"><slot name="list-start"></slot></div>
        <div class="list-middle">
            <slot name="list-middle">
                <div class="preview">
                    <div v-if="props.title" class="preview-title">
                        <span v-if="props.boss" class="preview-boss">👑 {{ t('unit.boss') }}</span>
                        <span class="preview-name">{{ props.title }}</span>
                    </div>
                    <div class="preview-body">
                        <div class="preview-team preview-team-player">
                            <UnitPortrait v-for="unit in props.playerUnits" :key="`p-${unit.position}`" :unit="unit" :size="props.portraitSize" />
                        </div>

                        <TooltipComponent v-if="props.arena" class="preview-arena">
                            <!-- Аура стихии арены — та же, что у героев 4-го ранга и выше -->
                            <CssAuraEffect :effect="aura" :size="ARENA_ICON_SIZE">
                                <div class="arena-effect">
                                    <component
                                      v-if="props.arena.effectName && STATUS_ICONS[props.arena.effectName]"
                                      :is="STATUS_ICONS[props.arena.effectName]"
                                      size="30px" />
                                    <span v-else class="arena-vs">VS</span>
                                </div>
                            </CssAuraEffect>
                            <template #title>{{ props.arena.name }}</template>
                            <template #description>{{ props.arena.description }}</template>
                            <template v-if="props.arena.effectName" #stats>
                                {{ t(props.arena.effectName === 'Frost' ? 'campaign.arenaEffectFrost' : 'campaign.arenaEffect', { effect: statusLabel(props.arena.effectName), power: props.arena.effectPower }) }}
                            </template>
                        </TooltipComponent>

                        <div class="preview-team preview-team-enemy">
                            <UnitPortrait v-for="unit in props.enemyUnits" :key="`e-${unit.position}`" :unit="unit" :size="props.portraitSize" />
                        </div>
                    </div>
                    <div v-if="props.forecast" class="preview-forecast">
                        <span class="forecast-power" v-tooltip="t('power.squad', { power: props.forecast.playerPower })">⚔ {{ props.forecast.playerPower }}</span>
                        <span class="forecast-verdict" :class="`forecast-${verdict}`" v-tooltip="t('power.chance', { chance })">
                            {{ t(`power.verdict.${verdict}`) }} · {{ chance }}%
                        </span>
                        <span class="forecast-power">⚔ {{ props.forecast.enemyPower }}</span>
                    </div>
                    <slot name="preview-footer"></slot>
                </div>
            </slot>
        </div>
        <div class="list-end"><slot name="list-end"></slot></div>
    </div>
</template>

<style scoped>
/* Слои внутри изолированной карточки: металлическая рамка (::before, -2) → арена (-1) → содержимое.
   Ореол текущего боя — ::after поверх рамки, но без кликов */
.stage-card {
    --accent: #cbd5e1;
    position: relative;
    isolation: isolate;
    width: 100%;
    min-height: 172px;
    display: flex;
    align-items: stretch;
    border-radius: 14px;
    transition: transform 0.15s ease;
}

.stage-card::before {
    content: '';
    position: absolute;
    inset: -4px;
    z-index: -2;
    border-radius: 17px;
    background: var(--metalic-background);
    box-shadow: 0 4px 3px 2px #181717, 0 2px 12px 2px color-mix(in srgb, var(--accent) 45%, transparent);
}

.stage-card:not(.locked):hover {
    transform: translateY(-2px);
}

/* Текущий бой: рамка в цвете стихии и пульсирующий ореол */
.stage-card.current::after {
    content: '';
    position: absolute;
    inset: -4px;
    border-radius: 17px;
    pointer-events: none;
    box-shadow:
        0 0 0 2px var(--accent),
        0 0 16px 3px var(--accent),
        0 0 32px 8px color-mix(in srgb, var(--accent) 40%, transparent);
    animation: stage-current-pulse 2.2s ease-in-out infinite;
}

@keyframes stage-current-pulse {
    0%, 100% { opacity: 0.55; }
    50% { opacity: 1; }
}

.stage-card-arena {
    position: absolute;
    inset: 0;
    z-index: -1;
    overflow: hidden;
    border-radius: 14px;
    box-shadow: inset 0 0 0 2px #333;
}

.stage-card-arena-image {
    position: absolute;
    inset: 0;
    background-size: cover;
    background-position: center 62%;
    filter: saturate(0.85);
    transition: transform 0.5s ease, filter 0.3s ease;
}

/* Арену видно за портретами, по краям и снизу темнее — под номер, кнопку и прогноз; сверху отсвет стихии */
.stage-card-arena::after {
    content: '';
    position: absolute;
    inset: 0;
    background:
        radial-gradient(ellipse 60% 55% at 50% 0%, color-mix(in srgb, var(--accent) 28%, transparent), transparent),
        linear-gradient(to bottom, rgba(10, 14, 22, 0.15) 0%, rgba(10, 14, 22, 0.35) 55%, rgba(10, 14, 22, 0.8) 100%);
}

.stage-card:not(.locked):hover .stage-card-arena-image {
    transform: scale(1.05);
}

/* Заблокированный: арена и отряды приглушены, без реакции на наведение */
.stage-card.locked .stage-card-arena-image {
    filter: grayscale(0.9) brightness(0.55);
}

.stage-card.locked .list-middle,
.stage-card.locked .list-start {
    filter: grayscale(0.75) brightness(0.75);
}

.stage-card.locked::before {
    box-shadow: 0 4px 3px 2px #181717;
}

.list-start, .list-middle, .list-end {
    display: flex;
    justify-content: center;
    align-items: center;
}

/* Номер и кнопка — на стеклянных панелях поверх арены, отделены от неё линией цвета стихии */
.list-start, .list-end {
    flex-shrink: 0;
    background: linear-gradient(to bottom, rgba(14, 19, 30, 0.78), rgba(8, 11, 18, 0.9));
    box-shadow: inset 0 0 12px rgba(0, 0, 0, 0.8);
}

.list-start {
    width: 84px;
    border-radius: 14px 0 0 14px;
    border-right: 2px solid color-mix(in srgb, var(--accent) 55%, transparent);
    font-size: 2.6em;
    font-weight: 900;
    font-family: RunesFont, sans-serif;
    text-shadow: 1px 1px 2px #000;
}

.list-end {
    width: 132px;
    border-radius: 0 14px 14px 0;
    border-left: 2px solid color-mix(in srgb, var(--accent) 55%, transparent);
}

.list-middle {
    flex: 1;
    /* Можно уже содержимого: на узком окне портреты переносятся, а не выталкивают строку вбок */
    min-width: 0;
}

.preview {
    width: 100%;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
    padding: 10px 12px;
    box-sizing: border-box;
}

.preview-title {
    display: flex;
    align-items: center;
    gap: 8px;
    max-width: 100%;
    padding: 2px 14px;
    border-radius: 10px;
    background: rgba(8, 11, 18, 0.6);
    border: 1px solid color-mix(in srgb, var(--accent) 45%, transparent);
}

.preview-name {
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
    font-weight: 800;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    text-shadow: 0 1px 3px #000, 0 0 6px #000;
}

.preview-boss {
    flex-shrink: 0;
    color: #ffd166;
    font-size: 12px;
    font-weight: 800;
    text-transform: uppercase;
    text-shadow: 0 0 8px #ff7b00, 0 1px 2px #000;
}

.stage-card.boss .preview-title {
    border-color: #ffd166;
    box-shadow: 0 0 10px rgba(255, 123, 0, 0.45);
}

/* Три колонки: отряды — равные половины, значок арены — посередине. Так он всегда ровно по центру карточки,
   сколько бы ни было дворфов и врагов */
.preview-body {
    width: 100%;
    display: grid;
    grid-template-columns: minmax(0, 1fr) auto minmax(0, 1fr);
    align-items: center;
    gap: 12px;
}

.preview-team {
    min-width: 0;
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
}

/* Как на арене: передняя линия (позиция 1) у обеих команд ближе к центру */
.preview-team-player {
    flex-direction: row-reverse;
    justify-content: flex-start;
}

.preview-team-enemy {
    justify-content: flex-start;
}

.preview-arena {
    grid-column: 2;
}

.arena-effect {
    width: 50px;
    height: 50px;
    display: flex;
    justify-content: center;
    align-items: center;
    border-radius: 50%;
    background: var(--wood-background);
    box-shadow: 0 0 0 3px rgb(157, 173, 175), 0 0 0 5px #333, 0 0 14px 4px var(--accent);
    cursor: help;
}

.arena-vs {
    font-weight: 900;
    font-family: RunesFont, sans-serif;
}

/* Вердикт — под значком арены, мощь отрядов — по краям */
.preview-forecast {
    width: 100%;
    display: grid;
    grid-template-columns: 1fr auto 1fr;
    align-items: center;
    gap: 12px;
    font-size: 13px;
    font-weight: 700;
    text-shadow: 0 1px 3px #000, 0 0 6px #000;
}

.forecast-power {
    justify-self: start;
    padding: 1px 8px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.45);
    color: #ffd166;
}

.forecast-power:last-child {
    justify-self: end;
}

.forecast-verdict {
    padding: 1px 10px;
    border-radius: 10px;
    background: rgba(0, 0, 0, 0.55);
    border: 1px solid currentColor;
}

.forecast-easy { color: #4ade80; }
.forecast-likely { color: #bef264; }
.forecast-risky { color: #fbbf24; }
.forecast-deadly { color: #f87171; }

@media (prefers-reduced-motion: reduce) {
    .stage-card.current::after {
        animation: none;
        opacity: 0.8;
    }

    .stage-card:not(.locked):hover,
    .stage-card:not(.locked):hover .stage-card-arena-image {
        transform: none;
    }
}
</style>
