<script lang="ts" setup>
import { computed, onBeforeUnmount, ref } from 'vue';
import { AxiosError } from 'axios';
import { useI18n } from 'vue-i18n';
import CharacterSkeleton from '@/components/character-skeleton.vue';
import GemButton from '@/components/common/GemButton.vue';
import { STATUS_ICONS } from '@/components/icons/status';
import HeroesApiService from '@/services/apiServices/HeroesApiService';
import { useResources } from '@/composables/UseResources';
import { HeroInfo, HeroUpgradeError, HeroUpgradeResult } from '@/models/types/Heroes';
import { HeroModel } from '@/models/hero';
import { elementColor } from '@/battle/sprites';
import { statusLabel } from '@/i18n';
import { vDragScroll } from '@/directives/dragScroll';
import caveBackground from '@/assets/red_crystal_arena.png';
import crystalsBackground from '@/assets/arena_blue_crystals.png';

// Выбранный герой в окне прокачки: сцена со спрайтом, характеристики и кнопка повышения уровня
const props = defineProps<{ hero: HeroInfo }>();
const emit = defineEmits<{
    (e: 'upgraded', result: HeroUpgradeResult): void;
    // Данные устарели (героя прокачали в другой вкладке, ресурсы потратили) — окну стоит перечитать их
    (e: 'stale'): void;
}>();

const { t, te, locale } = useI18n();
const { resources, set: setResources } = useResources();

// Спрайт 409.6px, персонаж занимает его центр
const FRAME = 409.6;
const SCALE = 1.6;
// Победа: 24 кадра на 16 fps — потом снова idle
const VICTORY_MS = 1500;

const color = computed(() => elementColor(props.hero.element));
const background = computed(() => (color.value === 'red' ? caveBackground : crystalsBackground));

const model = computed<HeroModel>(() => ({
    id: 0,
    name: props.hero.name,
    currentHp: props.hero.maxHp,
    maxHp: props.hero.maxHp,
    lvl: props.hero.level,
    color: color.value,
    unitType: props.hero.unitType,
}));

const upgrade = computed(() => props.hero.upgrade);
const isMax = computed(() => upgrade.value.cost === null);

/* ===== Цена ===== */
const lacksGold = computed(() => !!upgrade.value.cost && (resources.value?.gold ?? 0) < upgrade.value.cost.gold);
const lacksExperience = computed(() => !!upgrade.value.cost && (resources.value?.experience ?? 0) < upgrade.value.cost.experience);
const affordable = computed(() => !isMax.value && !lacksGold.value && !lacksExperience.value);

const format = (value: number) => value.toLocaleString(locale.value);

/* ===== Характеристики ===== */
interface StatRow {
    key: string;
    value: number;
    next?: number;
}

const stats = computed<StatRow[]>(() => {
    const { hero } = props;
    const next = upgrade.value.next;
    return [
        { key: 'power', value: hero.power.total, next: next?.power },
        { key: 'hp', value: hero.maxHp, next: next?.maxHp },
        { key: 'attack', value: hero.attack, next: next?.attack },
        { key: 'speed', value: hero.speed },
        { key: 'energy', value: hero.maxEnergy },
    ];
});

/* ===== Прокачка ===== */
type Skeleton = InstanceType<typeof CharacterSkeleton>;
const skeleton = ref<Skeleton | null>(null);

const busy = ref(false);
const message = ref<{ text: string; error: boolean } | null>(null);
// Номер вспышки «уровень повышен»: новый ключ перезапускает CSS-анимацию даже при частых кликах
const levelUpFlash = ref(0);

let victoryTimer = 0;

const celebrate = () => {
    levelUpFlash.value++;
    skeleton.value?.playAnimation('victory');
    window.clearTimeout(victoryTimer);
    victoryTimer = window.setTimeout(() => skeleton.value?.playAnimation('idle'), VICTORY_MS);
};

onBeforeUnmount(() => window.clearTimeout(victoryTimer));

const errorText = (error: unknown) => {
    const code = (error as AxiosError<{ code?: HeroUpgradeError }>).response?.data?.code;
    return code && te(`heroUpgrade.errors.${code}`) ? t(`heroUpgrade.errors.${code}`) : t('heroUpgrade.upgradeError');
};

const upgradeHero = async () => {
    if (!affordable.value || busy.value) return;
    busy.value = true;
    message.value = null;
    try {
        const result = await HeroesApiService.upgrade(props.hero.id);
        setResources(result.resources);
        emit('upgraded', result);
        message.value = { text: t('heroUpgrade.upgraded', { name: result.hero.name, level: result.hero.upgrade.level }), error: false };
        celebrate();
    } catch (error) {
        message.value = { text: errorText(error), error: true };
        emit('stale');
    } finally {
        busy.value = false;
    }
};

const buttonText = computed(() => {
    if (isMax.value) return t('heroUpgrade.maxLevel');
    if (busy.value) return t('heroUpgrade.upgrading');
    return t('heroUpgrade.upgrade');
});

const hint = computed(() => {
    if (message.value) return message.value.text;
    if (isMax.value) return t('heroUpgrade.maxHint');
    if (!affordable.value) return t('heroUpgrade.notEnough');
    return '';
});
</script>

<template>
    <div class="upgrade-panel">
        <!-- ===== Сцена ===== -->
        <div class="upgrade-stage" :style="{ backgroundImage: `url(${background})` }">
            <div class="upgrade-stage-title">
                <div class="upgrade-stage-name">{{ hero.name }}</div>
                <div class="upgrade-stage-tags">
                    <span class="upgrade-tag">{{ t('heroUpgrade.rank', { rank: hero.level }) }}</span>
                    <span class="upgrade-tag" :class="`upgrade-tag-${color}`">{{ t(`elements.${hero.element}`) }}</span>
                    <span v-if="hero.onExpedition" class="upgrade-tag upgrade-tag-busy">{{ t('squad.onExpedition') }}</span>
                </div>
            </div>

            <div class="upgrade-stage-unit">
                <div v-if="levelUpFlash" :key="levelUpFlash" class="upgrade-glow"></div>
                <!-- Ключ по рангу: другой ранг — другие спрайты, их скелет грузит при монтировании -->
                <CharacterSkeleton
                    :key="`${hero.id}-${hero.level}-${hero.element}`"
                    ref="skeleton"
                    :model="model"
                    :frame-width="FRAME"
                    :frame-height="FRAME"
                    :frame-count="24"
                    :columns="5"
                    :fps="16"
                    :scale="SCALE"
                />
                <div v-if="levelUpFlash" :key="`text-${levelUpFlash}`" class="upgrade-float">{{ t('heroUpgrade.levelUp') }}</div>
            </div>
        </div>

        <!-- ===== Характеристики ===== -->
        <!-- Без затухания краёв: маска срезала бы тень самой панели -->
        <aside v-drag-scroll="{ fade: false }" class="upgrade-stats">
            <h4 class="upgrade-subtitle">{{ t('heroUpgrade.stats') }}</h4>
            <div v-for="row in stats" :key="row.key" class="stat-row" :class="`stat-row-${row.key}`">
                <span class="stat-label">{{ t(`heroUpgrade.statNames.${row.key}`) }}</span>
                <span class="stat-value">{{ format(row.value) }}</span>
                <span
                    v-if="row.next !== undefined"
                    class="stat-delta"
                    :class="{ 'stat-delta-zero': row.next === row.value }"
                    :title="t('heroUpgrade.nextLevel')"
                >+{{ format(row.next - row.value) }}</span>
            </div>

            <div class="stat-group">
                <span class="stat-label">{{ t('heroUpgrade.statNames.spells') }}</span>
                <span class="stat-text">{{ hero.spells.length ? hero.spells.join(', ') : t('heroUpgrade.none') }}</span>
            </div>
            <div class="stat-group">
                <span class="stat-label">{{ t('heroUpgrade.statNames.immunities') }}</span>
                <span v-if="hero.immunities.length" class="stat-text stat-icons">
                    <span v-for="name in hero.immunities" :key="name" :title="statusLabel(name)">
                        <component v-if="STATUS_ICONS[name]" :is="STATUS_ICONS[name]" size="16px" />
                        <template v-else>{{ statusLabel(name) }}</template>
                    </span>
                </span>
                <span v-else class="stat-text">{{ t('heroUpgrade.none') }}</span>
            </div>
            <div class="stat-group">
                <span class="stat-label">{{ t('heroUpgrade.statNames.aura') }}</span>
                <span class="stat-text">{{ hero.aura ?? t('heroUpgrade.none') }}</span>
            </div>
        </aside>

        <!-- ===== Прокачка ===== -->
        <div class="upgrade-controls">
            <div class="upgrade-level">
                <span class="upgrade-level-text">{{ t('heroUpgrade.level', { level: upgrade.level, max: upgrade.maxLevel }) }}</span>
                <div class="upgrade-level-bar">
                    <span
                        v-for="step in upgrade.maxLevel"
                        :key="step"
                        class="upgrade-level-step"
                        :class="{ 'upgrade-level-step-done': step <= upgrade.level }"
                    ></span>
                </div>
            </div>

            <div class="upgrade-action">
                <div v-if="upgrade.cost" class="upgrade-cost">
                    <span class="upgrade-cost-label">{{ t('heroUpgrade.cost') }}</span>
                    <span class="upgrade-cost-gold" :class="{ 'upgrade-cost-lacking': lacksGold }" :title="t('resources.gold')">
                        🪙 {{ format(upgrade.cost.gold) }}
                    </span>
                    <span class="upgrade-cost-experience" :class="{ 'upgrade-cost-lacking': lacksExperience }" :title="t('resources.experience')">
                        ✦ {{ format(upgrade.cost.experience) }}
                    </span>
                </div>
                <span class="upgrade-hint" :class="{ 'upgrade-hint-error': message?.error }">{{ hint }}</span>
                <GemButton :color="'green'" :disabled="!affordable || busy" @click="upgradeHero">{{ buttonText }}</GemButton>
            </div>
        </div>
    </div>
</template>

<style scoped>
.upgrade-panel {
    /* Ширина панели — по её колонкам, остальное место окна отдаём списку героев */
    flex: 0 1 auto;
    min-width: 0;
    display: grid;
    /* Сцена ужимается первой, если окно узкое */
    grid-template-columns: minmax(0, 350px) 250px;
    grid-template-rows: minmax(0, 1fr) auto;
    gap: 12px;
    font-family: 'Arial', sans-serif;
}

/* ===== Сцена ===== */
.upgrade-stage {
    position: relative;
    min-height: 300px;
    overflow: hidden;
    border-radius: 14px;
    border: 2px solid #333;
    background-color: #1b1210;
    background-size: cover;
    background-position: center 65%;
    box-shadow: 0 0 0 3px rgb(157, 173, 175), inset 0 0 40px rgba(0, 0, 0, 0.85);
}

/* Затемнение снизу, чтобы герой «стоял» на полу */
.upgrade-stage::after {
    content: '';
    position: absolute;
    inset: 0;
    background: linear-gradient(to bottom, rgba(0, 0, 0, 0.35) 0%, rgba(0, 0, 0, 0) 35%, rgba(0, 0, 0, 0.5) 100%);
    pointer-events: none;
}

.upgrade-stage-title {
    position: absolute;
    top: 12px;
    left: 16px;
    z-index: 2;
    display: flex;
    flex-direction: column;
    row-gap: 6px;
}

.upgrade-stage-name {
    color: #f8fafc;
    font-size: 22px;
    font-weight: bold;
    letter-spacing: 1px;
    text-transform: uppercase;
    text-shadow: 0 2px 4px #000;
}

.upgrade-stage-tags {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
}

.upgrade-tag {
    padding: 2px 8px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.65);
    color: #e2e8f0;
    font-size: 12px;
    font-weight: bold;
}

.upgrade-tag-red { color: var(--red-gem-color); }
.upgrade-tag-blue { color: var(--blue-gem-color); }
.upgrade-tag-yellow { color: var(--yellow-gem-color); }
.upgrade-tag-green { color: var(--green-gem-color); }
.upgrade-tag-purple { color: #c084fc; }
.upgrade-tag-white { color: var(--white-gem-color); }
.upgrade-tag-busy { color: #fde68a; }

.upgrade-stage-unit {
    position: absolute;
    left: 50%;
    /* Под ногами в кадре пустое поле — опускаем его за край, чтобы герой стоял на полу */
    bottom: -80px;
    z-index: 1;
    transform: translateX(-50%);
}

.upgrade-stage-unit canvas {
    position: relative;
    z-index: 1;
    display: block;
}

/* Вспышка под героем при повышении уровня */
.upgrade-glow {
    position: absolute;
    left: 50%;
    top: 55%;
    width: 520px;
    height: 520px;
    border-radius: 50%;
    background: radial-gradient(circle, rgba(255, 230, 140, 0.85) 0%, rgba(255, 200, 80, 0.35) 40%, transparent 70%);
    transform: translate(-50%, -50%) scale(0.3);
    opacity: 0;
    pointer-events: none;
    animation: upgrade-glow 1.2s ease-out;
}

@keyframes upgrade-glow {
    0% { opacity: 0; transform: translate(-50%, -50%) scale(0.3); }
    25% { opacity: 1; }
    100% { opacity: 0; transform: translate(-50%, -50%) scale(1.5); }
}

.upgrade-float {
    position: absolute;
    left: 50%;
    top: 22%;
    z-index: 2;
    color: #ffd166;
    font-size: 26px;
    font-weight: 900;
    letter-spacing: 2px;
    text-transform: uppercase;
    white-space: nowrap;
    text-shadow: 0 0 8px rgba(255, 180, 60, 0.9), 0 2px 3px #000;
    opacity: 0;
    pointer-events: none;
    animation: upgrade-float 1.4s ease-out;
}

@keyframes upgrade-float {
    0% { opacity: 0; transform: translate(-50%, 20px) scale(0.7); }
    20% { opacity: 1; transform: translate(-50%, 0) scale(1.1); }
    70% { opacity: 1; transform: translate(-50%, -24px) scale(1); }
    100% { opacity: 0; transform: translate(-50%, -44px) scale(1); }
}

/* ===== Характеристики ===== */
.upgrade-stats {
    display: flex;
    flex-direction: column;
    row-gap: 6px;
    padding: 12px 14px;
    overflow-y: auto;
    border-radius: 12px;
    background: rgba(15, 23, 32, 0.8);
    border: 2px solid rgba(157, 173, 175, 0.7);
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5);
}

.upgrade-subtitle {
    margin: 0 0 4px;
    color: #f8fafc;
    font-size: 14px;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.stat-row {
    display: flex;
    align-items: baseline;
    column-gap: 8px;
    padding: 4px 0;
    border-bottom: 1px solid rgba(157, 173, 175, 0.2);
}

.stat-label {
    flex: 1;
    color: #cbd5e1;
    font-size: 13px;
}

.stat-value {
    color: #f8fafc;
    font-size: 15px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
}

.stat-row-power .stat-value {
    color: #ffd166;
}

.stat-delta {
    min-width: 36px;
    color: #86efac;
    font-size: 12px;
    font-weight: bold;
    text-align: right;
}

.stat-delta-zero {
    color: rgba(203, 213, 225, 0.5);
}

.stat-group {
    display: flex;
    flex-direction: column;
    row-gap: 2px;
    padding-top: 6px;
}

.stat-text {
    color: #f8fafc;
    font-size: 13px;
}

.stat-icons {
    display: flex;
    gap: 6px;
}

/* ===== Прокачка ===== */
.upgrade-controls {
    grid-column: 1 / -1;
    display: flex;
    flex-direction: column;
    row-gap: 10px;
    padding: 12px 16px;
    border-radius: 12px;
    background: rgba(15, 23, 32, 0.8);
    border: 2px solid rgba(157, 173, 175, 0.7);
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5);
}

.upgrade-level {
    display: flex;
    align-items: center;
    column-gap: 14px;
}

.upgrade-level-text {
    color: #f8fafc;
    font-size: 15px;
    font-weight: bold;
    white-space: nowrap;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.upgrade-level-bar {
    flex: 1;
    display: flex;
    column-gap: 3px;
}

.upgrade-level-step {
    flex: 1;
    height: 12px;
    border-radius: 3px;
    background: rgba(0, 0, 0, 0.55);
    box-shadow: inset 0 0 3px #000;
    transition: background 0.3s ease, box-shadow 0.3s ease;
}

.upgrade-level-step-done {
    background: linear-gradient(to bottom, #fde68a, #f59e0b);
    box-shadow: 0 0 6px rgba(245, 158, 11, 0.6);
}

.upgrade-action {
    display: flex;
    align-items: center;
    column-gap: 14px;
}

.upgrade-cost {
    display: flex;
    align-items: baseline;
    column-gap: 12px;
    font-size: 15px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.upgrade-cost-label {
    color: #cbd5e1;
    font-size: 13px;
    font-weight: normal;
}

.upgrade-cost-gold {
    color: #ffd166;
}

.upgrade-cost-experience {
    color: #a5f3fc;
}

.upgrade-cost-lacking {
    color: #ff8a8a;
}

.upgrade-hint {
    flex: 1;
    text-align: right;
    color: #cbd5e1;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.upgrade-hint-error {
    color: #ffb4b4;
}
</style>
