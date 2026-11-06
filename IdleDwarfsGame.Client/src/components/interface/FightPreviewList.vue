<script lang="ts" setup>

import { computed } from 'vue';
import { CombatUnitInfo, HeroElement } from '@/battle/types';
import { forecastVerdict } from '@/battle/power';
import { arenaPreview } from '@/battle/arenaBackgrounds';
import { ArenaInfo, BattleForecast } from '@/models/types/Campaign';
import { STATUS_ICONS } from '@/components/icons/status';
import { ELEMENT_COLORS } from '@/components/inventory/lootStyles';
import UnitPortrait from './UnitPortrait.vue';
import CssAuraEffect from '@/components/effects/CssAuraEffect.vue';
import { elementAura } from '@/components/effects/elementAura';
import { useI18n } from 'vue-i18n';
import { statusLabel } from '@/i18n';
import StatIcon from '@/components/icons/stats/StatIcon.vue';
import { IconCrown } from '@/components/icons/ui';

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
  portraitSize: 44
})

const chance = computed(() => Math.round((props.forecast?.winChance ?? 0) * 100));
const verdict = computed(() => forecastVerdict(props.forecast?.winChance ?? 0));

const element = computed(() => props.element ?? props.arena?.element ?? 'Basic');
const accent = computed(() => ELEMENT_COLORS[element.value] ?? ELEMENT_COLORS.Basic);

// Значок арены в панели справа. Аура стихии (снег, угли, искры…) — только у открытых боёв, у заблокированных она бы рябила
const ARENA_ICON_SIZE = 40;
const aura = computed(() => (props.locked ? 'none' : elementAura(element.value)));
const backgroundImage = computed(() => `url(${arenaPreview(props.arena?.background)})`);

// Свечение карточки (glowCard.css): текущий бой горит ярко, закрытый погашен, остальные — спокойно
const glowState = computed(() => {
    if (props.locked) return 'glow-card-done';
    return props.current ? 'glow-card-ready' : 'glow-card-waiting';
});
</script>

<template>
    <div class="stage-card glow-card" :class="[glowState, { locked: props.locked, current: props.current, boss: props.boss }]" :style="{ '--accent': accent }">
        <div class="stage-card-arena" aria-hidden="true">
            <div class="stage-card-arena-image" :style="{ backgroundImage }"></div>
        </div>

        <div class="list-start"><slot name="list-start"></slot></div>
        <div class="list-middle">
            <slot name="list-middle">
                <div class="preview">
                    <!-- Слева — бой: название, отряды и прогноз -->
                    <div class="preview-main">
                        <div v-if="props.title" class="preview-title">
                            <span v-if="props.boss" class="preview-boss"><IconCrown /> {{ t('unit.boss') }}</span>
                            <span class="preview-name">{{ props.title }}</span>
                        </div>
                        <!-- Одна сетка на отряды и прогноз: мощь стоит ровно под своим отрядом, вердикт — под «VS» -->
                        <div class="preview-grid">
                            <div class="preview-team preview-team-player">
                                <UnitPortrait v-for="unit in props.playerUnits" :key="`p-${unit.position}`" :unit="unit" :size="props.portraitSize" />
                            </div>
                            <span class="preview-vs">VS</span>
                            <div class="preview-team preview-team-enemy">
                                <UnitPortrait v-for="unit in props.enemyUnits" :key="`e-${unit.position}`" :unit="unit" :size="props.portraitSize" />
                            </div>

                            <template v-if="props.forecast">
                                <span class="forecast-power forecast-power-player" v-tooltip="t('power.squad', { power: props.forecast.playerPower })"><StatIcon stat="power" /> {{ props.forecast.playerPower }}</span>
                                <span class="forecast-verdict" :class="`forecast-${verdict}`" v-tooltip="t('power.chance', { chance })">
                                    {{ t(`power.verdict.${verdict}`) }} · {{ chance }}%
                                </span>
                                <span class="forecast-power forecast-power-enemy"><StatIcon stat="power" /> {{ props.forecast.enemyPower }}</span>
                            </template>
                        </div>
                        <slot name="preview-footer"></slot>
                    </div>

                    <!-- Справа — арена: значок эффекта, название, описание и что делает эффект -->
                    <aside v-if="props.arena" class="preview-arena-info">
                        <div class="arena-info-head">
                            <!-- Аура стихии арены — та же, что у героев 4-го ранга и выше -->
                            <CssAuraEffect :effect="aura" :size="ARENA_ICON_SIZE">
                                <div class="arena-effect">
                                    <component
                                      v-if="props.arena.effectName && STATUS_ICONS[props.arena.effectName]"
                                      :is="STATUS_ICONS[props.arena.effectName]"
                                      size="24px" />
                                    <span v-else class="arena-vs">VS</span>
                                </div>
                            </CssAuraEffect>
                            <span class="arena-info-name">{{ props.arena.name }}</span>
                        </div>
                        <div v-if="props.arena.description" class="arena-info-description">{{ props.arena.description }}</div>
                        <div v-if="props.arena.effectName" class="arena-info-effect">
                            {{ t(props.arena.effectName === 'Frost' ? 'campaign.arenaEffectFrost' : 'campaign.arenaEffect', { effect: statusLabel(props.arena.effectName), power: props.arena.effectPower }) }}
                        </div>
                    </aside>
                </div>
            </slot>
        </div>
        <div class="list-end"><slot name="list-end"></slot></div>
    </div>
</template>

<style scoped src="@/components/common/glowCard.css"></style>

<style scoped>
/* Карточка — светящаяся, как у боссов гильдии (glowCard.css): рамка и ореол цвета стихии.
   Слои внутри изолированной карточки: арена (-2) → ореол (::after, -1) → содержимое → блик по рамке (::before) */
.stage-card {
    --accent: #cbd5e1;
    --glow-color: var(--accent);
    width: 100%;
    min-height: 150px;
    display: flex;
    align-items: stretch;
    border-radius: 14px;
}

.stage-card-arena {
    position: absolute;
    inset: 0;
    z-index: -2;
    overflow: hidden;
    /* Внутри рамки в 2px */
    border-radius: 12px;
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
    transform: scale(1.08);
}

/* Заблокированный: арена и отряды приглушены, без реакции на наведение */
.stage-card.locked .stage-card-arena-image {
    filter: grayscale(0.9) brightness(0.55);
}

.stage-card.locked .list-middle,
.stage-card.locked .list-start {
    filter: grayscale(0.75) brightness(0.75);
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
    border-radius: 12px 0 0 12px;
    border-right: 2px solid color-mix(in srgb, var(--accent) 55%, transparent);
    font-size: 2.6em;
    font-weight: 900;
    font-family: RunesFont, sans-serif;
    text-shadow: 1px 1px 2px #000;
}

.list-end {
    width: 132px;
    border-radius: 0 12px 12px 0;
    border-left: 2px solid color-mix(in srgb, var(--accent) 55%, transparent);
}

.list-middle {
    flex: 1;
    /* Можно уже содержимого: на узком окне портреты переносятся, а не выталкивают строку вбок */
    min-width: 0;
}

/* Две части: слева бой (по ширине содержимого), справа — панель арены во всю оставшуюся высоту */
.preview {
    width: 100%;
    align-self: stretch;
    display: flex;
    align-items: stretch;
    gap: 14px;
    padding: 10px 12px;
    box-sizing: border-box;
}

.preview-main {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    justify-content: center;
    gap: 8px;
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

/* Три колонки: свой отряд, «VS», враги; во второй строке — мощь под каждым отрядом и вердикт под «VS».
   Сетка по ширине содержимого и прижата влево */
.preview-grid {
    max-width: 100%;
    display: grid;
    grid-template-columns: minmax(0, auto) auto minmax(0, auto);
    align-items: center;
    gap: 6px 10px;
}

.preview-vs {
    font-size: 15px;
    font-weight: 900;
    font-family: RunesFont, sans-serif;
    color: #e2e8f0;
    text-shadow: 0 0 8px var(--accent), 0 1px 2px #000;
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

/* ===== Панель арены справа: стекло с рамкой цвета стихии ===== */
.preview-arena-info {
    flex: 0 0 250px;
    min-width: 0;
    display: flex;
    flex-direction: column;
    justify-content: center;
    gap: 6px;
    padding: 8px 12px;
    box-sizing: border-box;
    border-radius: 10px;
    background: rgba(8, 11, 18, 0.62);
    border: 1px solid color-mix(in srgb, var(--accent) 45%, transparent);
    text-align: left;
    font-size: 12px;
    line-height: 1.3;
    text-shadow: 0 1px 2px #000;
}

.arena-info-head {
    display: flex;
    align-items: center;
    gap: 12px;
}

.arena-info-name {
    min-width: 0;
    font-size: 13px;
    font-weight: 800;
    letter-spacing: 0.06em;
    text-transform: uppercase;
}

.arena-info-description {
    color: #cbd5e1;
}

.arena-info-effect {
    color: #ffd166;
    font-weight: 700;
}

.arena-effect {
    width: 40px;
    height: 40px;
    display: flex;
    justify-content: center;
    align-items: center;
    border-radius: 50%;
    background: var(--wood-background);
    box-shadow: 0 0 0 2px rgb(157, 173, 175), 0 0 0 4px #333, 0 0 12px 3px var(--accent);
}

.arena-vs {
    font-size: 13px;
    font-weight: 900;
    font-family: RunesFont, sans-serif;
}

/* Прогноз — вторая строка сетки: мощь под отрядами, вердикт под «VS» */
.forecast-power,
.forecast-verdict {
    font-size: 13px;
    font-weight: 700;
    white-space: nowrap;
    text-shadow: 0 1px 3px #000, 0 0 6px #000;
}

.forecast-power {
    padding: 1px 8px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.45);
    color: #ffd166;
}

/* Мощь — у края своего отряда со стороны «VS», как и передняя линия */
.forecast-power-player {
    justify-self: end;
}

.forecast-power-enemy {
    justify-self: start;
}

.forecast-verdict {
    justify-self: center;
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
    .stage-card:not(.locked):hover .stage-card-arena-image {
        transform: none;
    }
}
</style>
