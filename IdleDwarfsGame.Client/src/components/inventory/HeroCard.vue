<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import UnitPortrait from '@/components/interface/UnitPortrait.vue';
import GlassButton from '@/components/common/GlassButton.vue';
import CssAuraEffect from '@/components/effects/CssAuraEffect.vue';
import { AURA_MIN_RANK, elementAura } from '@/components/effects/elementAura';
import { HeroInfo } from '@/models/types/Heroes';
import { elementColor } from '@/battle/sprites';

const { t } = useI18n();

const props = defineProps<{
    hero: HeroInfo;
    // Карточку сейчас тащат — показываем её полупрозрачной на старом месте
    dragging?: boolean;
    // Карточку нельзя взять (герой в экспедиции): её видно, но не перетащить и не выбрать
    disabled?: boolean;
    // Карточку выбирают кликом, а не перетаскиванием
    selectable?: boolean;
    // Выбрана: золотая кайма и разгоревшийся кристалл, как у активной вкладки
    selected?: boolean;
}>();

const color = computed(() => elementColor(props.hero.element));

// Герой высшего ранга: портрет в ауре своей стихии (как у боссов гильдии), по кромке карточки бежит отблеск
const aura = computed(() => (props.hero.level >= AURA_MIN_RANK ? elementAura(props.hero.element) : 'none'));

// Уровень прокачки — звёздами: 5 звёзд на ступень, следующая ступень перекрашивает их по одной.
// Например, 7 = две звезды второй ступени и три первой. Значение у звезды — её ступень, 0 — пустая
const STARS_PER_TIER = 5;
const MAX_TIER = 4;
const upgradeStars = computed<number[]>(() => {
    const level = props.hero.upgrade?.level;
    if (!level) return [];
    const tier = Math.min(Math.floor((level - 1) / STARS_PER_TIER), MAX_TIER - 1);
    const filled = level - tier * STARS_PER_TIER;
    return Array.from({ length: STARS_PER_TIER }, (_, i) => (i < filled ? tier + 1 : tier));
});
</script>

<template>
    <div
        class="hero-card"
        :class="[`hero-card-${color}`, {
            'hero-card-dragging': dragging,
            'hero-card-disabled': disabled,
            'hero-card-selectable': selectable && !disabled,
            'hero-card-selected': selected,
            'hero-card-prestige': aura !== 'none',
        }]"
        :draggable="!disabled && !selectable"
    >
        <!-- Ранг — по звезде за каждый, на верхней кромке оправы. TODO: заменить символ на иконку звезды -->
        <span class="hero-card-rank" :title="t('heroUpgrade.rank', { rank: hero.level })">
            <span v-for="index in hero.level" :key="index" class="hero-card-rank-star">★</span>
        </span>
        <!-- Кристалл как у вкладок модалки, только окрашен в цвет стихии героя -->
        <GlassButton tag="div" class="hero-card-glass" :active="selected">
            <!-- upgrade нет только у ответа старого сервера: карточка без звёзд прокачки, но не падает -->
            <span v-if="hero.upgrade" class="hero-card-upgrade" :title="t('heroUpgrade.level', { level: hero.upgrade.level, max: hero.upgrade.maxLevel })">
                <span v-for="(tier, index) in upgradeStars" :key="index" class="hero-card-upgrade-star" :class="`hero-card-upgrade-star-${tier}`">★</span>
            </span>
            <CssAuraEffect :effect="aura" :size="72">
                <UnitPortrait :unit="hero" :size="72" :show-level="false" />
            </CssAuraEffect>
            <div class="hero-card-name" :title="hero.name">{{ hero.name }}</div>
            <div class="hero-card-power" :title="t('power.value', { power: hero.power.total })">⚔ {{ hero.power.total }}</div>
            <div v-if="hero.onExpedition" class="hero-card-busy">{{ t('squad.onExpedition') }}</div>
        </GlassButton>
    </div>
</template>

<style scoped>
/* Металлическая оправа, как у рамки модалки; внутри — стекло GlassButton */
.hero-card {
    position: relative;
    box-sizing: border-box;
    width: 136px;
    padding: 12px;
    /* Колонка — чтобы стекло тянулось на всю высоту, когда соседняя карточка в ряду выше */
    display: flex;
    flex-direction: column;
    border-radius: 16px;
    background: var(--metalic-background);
    box-shadow:
        inset 0 1px 2px rgba(255, 255, 255, 0.8),
        0 4px 3px 2px #181717,
        0 3px 12px 2px rgba(0, 0, 0, 0.6);
    /* Толща кристалла: цвет стихии, утопленный в тёмно-синее стекло вкладок */
    --glass-top: color-mix(in srgb, var(--card-accent) 45%, #9ab4cf);
    --glass-mid: color-mix(in srgb, var(--card-accent) 40%, #3b5272);
    --glass-bottom: color-mix(in srgb, var(--card-accent) 30%, #141d2b);
    cursor: grab;
    user-select: none;
    transition: transform 0.2s cubic-bezier(0.34, 1.4, 0.64, 1), box-shadow 0.3s ease, opacity 0.15s ease;
}

.hero-card:hover {
    transform: translateY(-3px);
    box-shadow:
        inset 0 1px 2px rgba(255, 255, 255, 0.8),
        0 4px 3px 2px #181717,
        0 3px 18px 4px color-mix(in srgb, var(--card-accent) 55%, transparent);
}

.hero-card:active {
    cursor: grabbing;
}

/* Стекло растянуто по оправе. Два класса — чтобы перебить размеры, скругление и свечение самого GlassButton */
.hero-card .hero-card-glass {
    --glass-glow: color-mix(in srgb, var(--card-accent) 45%, transparent);
    display: flex;
    flex: 1;
    width: 100%;
    min-width: 0;
    padding: 10px 6px;
    border-radius: 10px;
}

/* Свет внутри кристалла разгорается при наведении, отблеск пробегает по всей карточке */
.hero-card:hover .hero-card-glass {
    --glass-glow: color-mix(in srgb, var(--card-accent) 80%, white 10%);
}

.hero-card:hover .hero-card-glass::after {
    transform: translateX(120%);
}

/* Блик по форме карточки, а не вкладки */
.hero-card .hero-card-glass::before {
    border-radius: 8px 8px 50% 50% / 8px 8px 30% 30%;
}

/* Содержимое над бликом, иначе он засвечивает портрет; пробегающий отблеск — поверх всего */
.hero-card-glass > * {
    position: relative;
    z-index: 1;
}

.hero-card .hero-card-glass::after {
    z-index: 2;
}

.hero-card-selected,
.hero-card-selected:hover {
    box-shadow:
        inset 0 1px 2px rgba(255, 255, 255, 0.8),
        0 0 0 3px #ffd166,
        0 0 18px 4px rgba(255, 209, 102, 0.65);
}

.hero-card-selected .hero-card-glass {
    --glass-glow: color-mix(in srgb, var(--card-accent) 85%, white 15%);
}

.hero-card-dragging {
    opacity: 0.35;
}

.hero-card-selectable {
    cursor: pointer;
}

.hero-card-disabled {
    cursor: not-allowed;
    filter: grayscale(0.85) brightness(0.7);
}

.hero-card-disabled:hover {
    transform: none;
}

/* Ранг — золотые звёзды на верхней кромке оправы */
.hero-card-rank {
    position: absolute;
    top: -6px;
    left: 50%;
    transform: translateX(-50%);
    z-index: 3;
    display: flex;
    column-gap: 1px;
    line-height: 1;
}

.hero-card-rank-star {
    color: #ffd166;
    font-size: 17px;
    text-shadow: 0 0 6px rgba(255, 209, 102, 0.6);
    filter: drop-shadow(0 1px 1px rgba(0, 0, 0, 0.9)) drop-shadow(0 0 1px rgba(0, 0, 0, 0.9));
}

/* Уровень прокачки — ряд звёзд над портретом */
.hero-card-upgrade {
    display: flex;
    column-gap: 1px;
    line-height: 1;
}

.hero-card-upgrade-star {
    font-size: 12px;
    filter: drop-shadow(0 1px 1px rgba(0, 0, 0, 0.9));
}

/* Пустая — тёмное гнездо в стекле */
.hero-card-upgrade-star-0 { color: rgba(0, 0, 0, 0.45); }
.hero-card-upgrade-star-1 { color: #f1f5f9; }
.hero-card-upgrade-star-2 { color: #ff7a3d; }
.hero-card-upgrade-star-3 { color: #c084fc; }
.hero-card-upgrade-star-4 { color: #5eead4; }

/* Звёзды старших ступеней слегка светятся своим цветом */
.hero-card-upgrade-star-2,
.hero-card-upgrade-star-3,
.hero-card-upgrade-star-4 {
    text-shadow: 0 0 6px currentColor;
}

.hero-card-busy {
    padding: 1px 6px;
    border-radius: 6px;
    background: rgba(0, 0, 0, 0.6);
    color: #fde68a;
    font-family: 'Arial', sans-serif;
    font-size: 10px;
    font-weight: bold;
    text-transform: uppercase;
}

.hero-card-red { --card-accent: var(--red-gem-color); }
.hero-card-blue { --card-accent: var(--blue-gem-color); }
.hero-card-yellow { --card-accent: var(--yellow-gem-color); }
.hero-card-green { --card-accent: var(--green-gem-color); }
.hero-card-purple { --card-accent: var(--purple-gem-color); }

/* Бриллиант — почти бесцветное стекло, цвет ему даёт радужный перелив */
.hero-card-white {
    --card-accent: var(--white-gem-color);
    --glass-top: #c9d6e3;
    --glass-mid: #7d8fa3;
    --glass-bottom: #2a3340;
}

/* ===== Высший ранг: отблеск бежит по кромке оправы, вокруг карточки дышит свечение стихии ===== */
@property --prestige-angle {
    syntax: '<angle>';
    initial-value: 0deg;
    inherits: false;
}

/* Своё наложение слоёв: свечение (z-index -1) остаётся под карточкой, но над фоном окна */
.hero-card-prestige {
    isolation: isolate;
}

.hero-card-prestige::before,
.hero-card-prestige::after {
    content: '';
    position: absolute;
    border-radius: inherit;
    pointer-events: none;
}

/* Кольцо толщиной в 3px по краю оправы */
.hero-card-prestige::before {
    inset: -1px;
    z-index: 2;
    padding: 3px;
    background: conic-gradient(
        from var(--prestige-angle),
        transparent 0deg,
        transparent 220deg,
        var(--card-accent) 290deg,
        #fff 325deg,
        var(--card-accent) 345deg,
        transparent 360deg
    );
    -webkit-mask: linear-gradient(#000 0 0) content-box, linear-gradient(#000 0 0);
    -webkit-mask-composite: xor;
    mask: linear-gradient(#000 0 0) content-box, linear-gradient(#000 0 0);
    mask-composite: exclude;
    animation: hero-prestige-spin 3.2s linear infinite;
}

.hero-card-prestige::after {
    inset: 0;
    z-index: -1;
    box-shadow: 0 0 14px 2px color-mix(in srgb, var(--card-accent) 70%, transparent);
    animation: hero-prestige-pulse 2.6s ease-in-out infinite;
}

/* Бриллиант: вместо одного цвета по кромке бежит весь спектр, и он же переливается в свечении */
.hero-card-prestige.hero-card-white::before {
    background: conic-gradient(from var(--prestige-angle), #ff6b6b, #ffd166, #6bff95, #6bd5ff, #a78bfa, #ff7ad9, #ff6b6b);
    animation-duration: 4s;
}

.hero-card-prestige.hero-card-white::after {
    box-shadow: 0 0 16px 3px rgba(255, 122, 217, 0.6);
    animation: hero-prestige-pulse 2.6s ease-in-out infinite, hero-prestige-rainbow 5s linear infinite;
}

/* Тьма: кольцо крутится медленно и в обратную сторону, свечение глубокое */
.hero-card-prestige.hero-card-purple::before {
    animation-duration: 5s;
    animation-direction: reverse;
}

.hero-card-prestige.hero-card-purple::after {
    box-shadow: 0 0 16px 4px rgba(126, 34, 206, 0.75);
}

@keyframes hero-prestige-spin {
    to { --prestige-angle: 360deg; }
}

@keyframes hero-prestige-pulse {
    0%, 100% { opacity: 0.45; }
    50% { opacity: 1; }
}

@keyframes hero-prestige-rainbow {
    to { filter: hue-rotate(360deg); }
}

@media (prefers-reduced-motion: reduce) {
    .hero-card-prestige::before,
    .hero-card-prestige::after {
        animation: none !important;
    }
}

/* Без стихии — мутное дымчато-серое стекло, без синевы вкладок */
.hero-card-basic {
    --card-accent: #9ca3af;
    --glass-top: #8b9096;
    --glass-mid: #555a60;
    --glass-bottom: #26292d;
}

.hero-card-name {
    max-width: 100%;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    color: #f8fafc;
    font-family: 'Arial', sans-serif;
    font-size: 13px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.hero-card-power {
    color: #ffd166;
    font-family: 'Arial', sans-serif;
    font-size: 12px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}
</style>
