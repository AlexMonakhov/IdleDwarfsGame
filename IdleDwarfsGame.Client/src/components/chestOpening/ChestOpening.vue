<script lang="ts" setup>
import { soundService } from '@/services/soundService/SoundService';
import type { SoundKey } from '@/services/soundService/soundCatalog';
import { computed, nextTick, onMounted, onUnmounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import CardComponent from '@/components/card/CardComponent.vue';
import GemButton from '@/components/common/GemButton.vue';
import { CHEST_ICONS, RARITY_COLORS } from '@/components/inventory/lootStyles';
import { RARITY_ORDER } from '@/models/types/Forge';
import type { ChestType } from '@/models/types/Inventory';
import ChestRewardFace from './ChestRewardFace.vue';
import CardBackdrop from './CardBackdrop.vue';
import { CHEST_CARD_COUNT, RARITY_CARD_COLORS, rarityRank, randomVariant, type ChestOpeningVariant, type ChestRewardCard, type BackdropStyle } from './chestRewards';

// Окно открытия сундука. Этапы: сундук ждёт клика → вспышка → карты (жилы) вылетают → игрок разбивает каждую
// за 3 удара (CardComponent) → «Забрать». Пробел бьёт сразу по всем неразбитым картам, «Пропустить» раскрывает всё сразу
const props = withDefaults(defineProps<{
    rewards: ChestRewardCard[];
    chestType: ChestType;
    chestLevel: number;
    // Без варианта — случайный из четырёх (CHEST_OPENING_VARIANTS)
    variant?: ChestOpeningVariant;
    // Фон окна карт и чаша под иконкой. По умолчанию — выбранный вид: осколки, тонкая рамка, стекло, без чаши
    backdrop?: BackdropStyle;
    bowl?: boolean;
    // Рамка окна карты и выпуклое стекло (CardComponent)
    frame?: 'default' | 'thin' | 'none';
    glass?: boolean;
}>(), { variant: undefined, backdrop: 'shards', bowl: false, frame: 'thin', glass: true });

// Вариант выбирается один раз на открытие
const activeVariant: ChestOpeningVariant = props.variant ?? randomVariant();

const emit = defineEmits<{ (e: 'close'): void }>();

const { t } = useI18n();

type Stage = 'chest' | 'opening' | 'dealing' | 'reveal' | 'done';
const stage = ref<Stage>('chest');
const skipped = ref(false);
const revealed = ref<boolean[]>(Array(CHEST_CARD_COUNT).fill(false));
// Сколько ударов уже получила каждая карта — для тряски слота
const hits = ref<number[]>(Array(CHEST_CARD_COUNT).fill(0));

const cards = ref<(InstanceType<typeof CardComponent> | null)[]>([]);
const slotEls = ref<(HTMLElement | null)[]>([]);
const chestEl = ref<HTMLElement | null>(null);
// Колбэки :ref для v-for (в шаблоне без приведения типов — его не разбирает eslint)
const setSlotEl = (i: number, el: unknown) => (slotEls.value[i] = el as HTMLElement | null);
const setCard = (i: number, el: unknown) => (cards.value[i] = el as InstanceType<typeof CardComponent> | null);
const rowEl = ref<HTMLElement | null>(null);

// Смещения слотов до сундука и до центра ряда (px) — откуда карта вылетает и где собирается стопка
const offsets = ref<{ fromX: number; fromY: number; pileX: number; pileY: number }[]>([]);

// ---------- Размер карт под экран ----------
const CARD_W = 220; // CardComponent 200×300 + margin 10
const CARD_H = 320;
const viewport = ref({ w: window.innerWidth, h: window.innerHeight });
const onResize = () => (viewport.value = { w: window.innerWidth, h: window.innerHeight });
const columns = computed(() => (viewport.value.w < 760 ? 3 : 5));
const scale = computed(() => {
    const rows = Math.ceil(CHEST_CARD_COUNT / columns.value);
    const byWidth = (viewport.value.w - 32 - (columns.value - 1) * 8) / (columns.value * CARD_W);
    const byHeight = (viewport.value.h - 220) / (rows * CARD_H);
    return Math.max(0.35, Math.min(1, byWidth, byHeight));
});

// ---------- Раскладка и тайминги вариантов ----------
// Жила: карты вырастают в случайном порядке
const veinOrder = ref<number[]>([]);

const DEAL_TIMING: Record<ChestOpeningVariant, { duration: number; stagger: number }> = {
    burst: { duration: 750, stagger: 50 },
    deal: { duration: 520, stagger: 220 },
    fan: { duration: 1300, stagger: 0 },
    vein: { duration: 800, stagger: 190 },
};

const dealTotal = computed(() => {
    const timing = DEAL_TIMING[activeVariant];
    return timing.duration + timing.stagger * (CHEST_CARD_COUNT - 1);
});

const finalTransform = (i: number) => {
    if (activeVariant !== 'fan' || columns.value !== 5) return 'translate(0px, 0px) rotate(0deg) scale(1)';
    const d = i - (CHEST_CARD_COUNT - 1) / 2;
    return `translate(0px, ${d * d * 12}px) rotate(${d * 6}deg) scale(1)`;
};

const slotStyle = (i: number) => {
    const timing = DEAL_TIMING[activeVariant];
    const order = activeVariant === 'vein' ? veinOrder.value.indexOf(i) : i;
    const offset = offsets.value[i] ?? { fromX: 0, fromY: 0, pileX: 0, pileY: 0 };
    const reward = props.rewards[i];
    return {
        '--from-x': `${offset.fromX}px`,
        '--from-y': `${offset.fromY}px`,
        '--pile-x': `${offset.pileX}px`,
        '--pile-y': `${offset.pileY}px`,
        '--spin': `${(i % 2 ? 1 : -1) * (360 + i * 60)}deg`,
        '--final': finalTransform(i),
        '--delay': `${order * timing.stagger}ms`,
        '--duration': `${timing.duration}ms`,
        '--slot-color': reward ? RARITY_COLORS[reward.rarity] : '#fff',
        '--card-scale': scale.value,
        // В веере средняя карта сверху стопки
        zIndex: activeVariant === 'fan' ? 10 - Math.abs(i - 2) : undefined,
    };
};

// Мифические и выше подсвечиваются ещё до раскрытия, легендарные и выше празднуются после
const teases = (i: number) => rarityRank(props.rewards[i]?.rarity ?? 'Common') >= RARITY_ORDER.indexOf('Mythic');
const celebrates = (i: number) => rarityRank(props.rewards[i]?.rarity ?? 'Common') >= RARITY_ORDER.indexOf('Legendary');

// Искры празднования: разлетаются по кругу на разное расстояние
const SPARKS = Array.from({ length: 16 }, (_, id) => {
    const angle = (id / 16) * Math.PI * 2 + Math.random() * 0.3;
    const distance = 120 + Math.random() * 90;
    return {
        id,
        style: {
            '--spark-x': `${Math.cos(angle) * distance}px`,
            '--spark-y': `${Math.sin(angle) * distance}px`,
            '--spark-size': `${4 + Math.random() * 5}px`,
            animationDelay: `${Math.random() * 120}ms`,
        },
    };
});

// Звук открытия — по уровню сундука (1–5 → обычный…легендарный)
const CHEST_OPEN_SOUNDS: SoundKey[] = ['chest_open_common', 'chest_open_uncommon', 'chest_open_rare', 'chest_open_mythic', 'chest_open_legendary'];

const revealSound = (i: number): SoundKey => {
    const rank = rarityRank(props.rewards[i]?.rarity ?? 'Common');
    if (rank >= RARITY_ORDER.indexOf('Legendary')) return 'card_reveal_legendary';
    if (rank >= RARITY_ORDER.indexOf('Mythic')) return 'card_reveal_rare';
    return 'card_reveal';
};

const chestColor = computed(() => RARITY_COLORS[RARITY_ORDER[Math.min(Math.max(props.chestLevel, 1), 5) - 1]]);

// ---------- Ход открытия ----------
const timers: number[] = [];
const later = (ms: number, action: () => void) => timers.push(window.setTimeout(action, ms));
const clearTimers = () => timers.splice(0).forEach(clearTimeout);

const center = (el: Element) => {
    const rect = el.getBoundingClientRect();
    return { x: rect.left + rect.width / 2, y: rect.top + rect.height / 2 };
};

const measure = () => {
    const chest = chestEl.value ? center(chestEl.value) : null;
    const row = rowEl.value ? center(rowEl.value) : null;
    offsets.value = slotEls.value.map(el => {
        if (!el || !chest || !row) return { fromX: 0, fromY: 0, pileX: 0, pileY: 0 };
        const slot = center(el);
        return { fromX: chest.x - slot.x, fromY: chest.y - slot.y, pileX: row.x - slot.x, pileY: row.y - slot.y };
    });
};

const openChest = async () => {
    if (stage.value !== 'chest') return;
    veinOrder.value = [...Array(CHEST_CARD_COUNT).keys()].sort(() => Math.random() - 0.5);
    stage.value = 'opening';
    soundService.play(CHEST_OPEN_SOUNDS[Math.min(Math.max(props.chestLevel, 1), 5) - 1]);
    // Слоты уже в разметке (невидимые) — меряем, откуда им лететь
    await nextTick();
    measure();
    later(activeVariant === 'vein' ? 650 : 480, () => {
        stage.value = 'dealing';
        // Каждая карта вылетает со своей задержкой — у каждой свой звук
        const { stagger } = DEAL_TIMING[activeVariant];
        for (let order = 0; order < CHEST_CARD_COUNT; order++) later(order * stagger, () => soundService.play('card_deal'));
        later(dealTotal.value + 60, () => (stage.value = 'reveal'));
    });
};

// Первые два удара — по руде, третий раскалывает карту (раскрытие звучит поверх, в onRevealed)
const onHit = (i: number, phase: number) => {
    hits.value[i]++;
    soundService.play(phase < 3 ? 'card_hit' : 'ui_click_ore_card_crack');
};

const onRevealed = (i: number) => {
    revealed.value[i] = true;
    soundService.play(revealSound(i));
    if (revealed.value.every(Boolean)) later(skipped.value ? 0 : 700, () => (stage.value = 'done'));
};

const hitAll = () => cards.value.forEach((card, i) => !revealed.value[i] && card?.hit());

const skip = async () => {
    if (stage.value === 'done') return;
    clearTimers();
    soundService.play('chest_skip');
    skipped.value = true;
    stage.value = 'reveal';
    await nextTick();
    cards.value.forEach(card => card?.reveal());
};

// Пробел и кнопка с ним (для сенсорных экранов): открыть сундук → бить по всем картам → забрать
const pressSpace = () => {
    if (stage.value === 'chest') openChest();
    else if (stage.value === 'reveal') hitAll();
    else if (stage.value === 'done') emit('close');
};

// Слушаем в фазе перехвата: раньше модалки, под которой открыт сундук. preventDefault + stopPropagation —
// Esc пропускает анимацию, а не закрывает заодно и модалку (ModalComponent игнорирует defaultPrevented)
const onKeydown = (event: KeyboardEvent) => {
    if (event.code !== 'Space' && event.code !== 'Enter' && event.code !== 'Escape') return;
    event.preventDefault();
    event.stopPropagation();
    if (event.repeat) return;
    if (event.code === 'Escape') {
        if (stage.value === 'done') emit('close');
        else skip();
        return;
    }
    if (event.code === 'Enter') {
        if (stage.value === 'done') emit('close');
        return;
    }
    pressSpace();
};

onMounted(() => {
    // Кнопка, открывшая окно, держит фокус — иначе пробел/Enter нажимали бы её снова
    (document.activeElement as HTMLElement | null)?.blur();
    soundService.play('chest_appear');
    window.addEventListener('keydown', onKeydown, true);
    window.addEventListener('resize', onResize);
});

onUnmounted(() => {
    clearTimers();
    window.removeEventListener('keydown', onKeydown, true);
    window.removeEventListener('resize', onResize);
});

const slotsVisible = computed(() => stage.value !== 'chest');
const hint = computed(() => {
    if (stage.value === 'chest') return t('chests.reveal.tapChest');
    if (stage.value === 'reveal') return t('chests.reveal.tapCards');
    return '';
});
</script>

<template>
    <Teleport to="body">
        <div class="chest-opening" :class="[`variant-${activeVariant}`, `stage-${stage}`, { skipped }]" :style="{ '--chest-color': chestColor, '--card-scale': scale }">
            <div class="chest-opening-backdrop"></div>
            <div class="chest-opening-flash"></div>

            <button v-if="stage !== 'done'" class="chest-opening-skip" type="button" data-sound="none" @click="skip">
                {{ t('chests.reveal.skip') }} <kbd>Esc</kbd>
            </button>

            <!-- Сундук и карты звучат сами (открытие, удары, раскрытие) — без общего звука клика -->
            <div class="chest-opening-stage" data-sound="none">
                <!-- Сундук: виден, пока не открыт -->
                <div
                    v-if="stage === 'chest' || stage === 'opening'"
                    ref="chestEl"
                    class="chest-opening-chest"
                    @click="openChest"
                >
                    <component :is="CHEST_ICONS[chestType]" size="170px" />
                </div>

                <!-- Трещина в породе (вариант «Жила») -->
                <div v-if="activeVariant === 'vein' && stage !== 'chest'" class="vein-seam"></div>

                <div
                    v-if="slotsVisible"
                    ref="rowEl"
                    class="chest-opening-row"
                    :class="{ 'row-wrap': columns === 3 }"
                    :style="{ '--card-scale': scale }"
                >
                    <div
                        v-for="(reward, i) in rewards"
                        :key="i"
                        :ref="el => setSlotEl(i, el)"
                        class="chest-slot"
                        :class="{
                            'slot-tease': teases(i) && !revealed[i] && (stage === 'reveal'),
                            'slot-celebrate': celebrates(i) && revealed[i],
                            'slot-revealed': revealed[i],
                            [`slot-hit-${Math.min(hits[i], 2)}`]: hits[i] > 0 && !revealed[i],
                        }"
                        :style="slotStyle(i)"
                    >
                        <!-- Искры при раскрытии легендарной и выше -->
                        <div v-if="celebrates(i) && revealed[i]" class="chest-slot-sparks">
                            <span v-for="spark in SPARKS" :key="spark.id" :style="spark.style"></span>
                        </div>
                        <div class="chest-slot-size">
                            <div class="chest-slot-scale">
                                <CardComponent
                                    :ref="el => setCard(i, el)"
                                    :color="RARITY_CARD_COLORS[reward.rarity]"
                                    :back="{ variant: 'large', seed: 11 + i * 7 }"
                                    hide-info
                                    :frame="frame"
                                    :glass="glass"
                                    :locked="stage !== 'reveal'"
                                    @hit="onHit(i, $event)"
                                    @revealed="onRevealed(i)"
                                >
                                    <!-- Фон окна карты: глубже иконки, при наклоне сдвигается сильнее неё -->
                                    <template #backdrop>
                                        <CardBackdrop :type="backdrop" />
                                    </template>
                                    <ChestRewardFace :reward="reward" :bowl="bowl" />
                                </CardComponent>
                                <!-- Блик, пробегающий по лицевой стороне легендарной карты -->
                                <div v-if="celebrates(i) && revealed[i]" class="chest-slot-shine"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="chest-opening-footer">
                <div v-if="hint" class="chest-opening-hint">{{ hint }}</div>
                <!-- То же, что пробел: кнопка для сенсорных экранов, оформлена как «Пропустить» -->
                <button v-if="stage === 'chest' || stage === 'reveal'" class="chest-opening-button" type="button" data-sound="none" @click="pressSpace">
                    {{ stage === 'chest' ? t('chests.reveal.openChest') : t('chests.reveal.hitAll') }} <kbd>{{ t('chests.reveal.space') }}</kbd>
                </button>
                <GemButton v-if="stage === 'done'" color="green" width="180px" height="48px" data-sound="ui_click_primary" @click="emit('close')">
                    {{ t('chests.reveal.collect') }}
                </GemButton>
            </div>
        </div>
    </Teleport>
</template>

<style scoped>
.chest-opening {
    position: fixed;
    inset: 0;
    z-index: 2000;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    overflow: hidden;
    color: #fff;
    user-select: none;
}

.chest-opening-backdrop {
    position: absolute;
    inset: 0;
    background:
        radial-gradient(ellipse at 50% 55%, color-mix(in srgb, var(--chest-color) 22%, transparent) 0%, transparent 55%),
        radial-gradient(ellipse at 50% 50%, rgba(20, 18, 28, 0.88) 0%, rgba(5, 5, 10, 0.97) 100%);
    animation: backdrop-in 0.35s ease-out both;
}

@keyframes backdrop-in {
    from { opacity: 0; }
}

.chest-opening-stage {
    position: relative;
    flex: 1;
    width: 100%;
    display: grid;
    place-items: center;
}

.chest-opening-footer {
    position: relative;
    min-height: 110px;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: flex-start;
    gap: 10px;
}

.chest-opening-hint {
    font-size: 15px;
    opacity: 0.85;
    display: flex;
    align-items: center;
    gap: 10px;
    animation: hint-in 0.4s ease-out both;
}

@keyframes hint-in {
    from { opacity: 0; transform: translateY(6px); }
}

kbd {
    display: inline-block;
    padding: 2px 8px;
    border-radius: 5px;
    border: 1px solid rgba(255, 255, 255, 0.4);
    border-bottom-width: 3px;
    font-family: inherit;
    font-size: 12px;
    background: rgba(255, 255, 255, 0.08);
}

.chest-opening-skip {
    position: absolute;
    top: 16px;
    right: 16px;
}

.chest-opening-skip,
.chest-opening-button {
    z-index: 30;
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 8px 14px;
    border-radius: 10px;
    border: 1px solid rgba(255, 255, 255, 0.25);
    background: rgba(0, 0, 0, 0.4);
    color: #fff;
    font-size: 14px;
    cursor: pointer;
    transition: background 0.2s;
}

.chest-opening-button {
    position: relative;
    padding: 10px 18px;
    font-size: 15px;
    animation: hint-in 0.4s ease-out both;
}

.chest-opening-skip:hover,
.chest-opening-button:hover {
    background: rgba(255, 255, 255, 0.12);
}

/* ---------- Сундук ---------- */
.chest-opening-chest {
    grid-area: 1 / 1;
    cursor: pointer;
    filter: drop-shadow(0 0 24px var(--chest-color));
    animation: chest-in 0.5s cubic-bezier(0.2, 1.4, 0.4, 1) both, chest-idle 2.2s ease-in-out 0.6s infinite;
    transition: transform 0.2s;
    z-index: 5;
}

.chest-opening-chest:hover {
    transform: scale(1.06);
}

@keyframes chest-in {
    from { transform: scale(0.3) translateY(60px); opacity: 0; }
}

@keyframes chest-idle {
    0%, 70%, 100% { rotate: 0deg; }
    76% { rotate: -6deg; }
    82% { rotate: 6deg; }
    88% { rotate: -3deg; }
    94% { rotate: 2deg; }
}

.stage-opening .chest-opening-chest {
    pointer-events: none;
    animation: chest-burst 0.5s ease-in forwards;
}

@keyframes chest-burst {
    0% { transform: scale(1); }
    15% { transform: scale(0.9) rotate(-8deg); }
    30% { transform: scale(1.05) rotate(8deg); }
    45% { transform: scale(0.95) rotate(-6deg); }
    70% { transform: scale(1.25); opacity: 1; filter: drop-shadow(0 0 60px var(--chest-color)) brightness(2); }
    100% { transform: scale(1.6); opacity: 0; filter: drop-shadow(0 0 80px var(--chest-color)) brightness(4); }
}

/* Жила: сундук уходит в породу */
.variant-vein.stage-opening .chest-opening-chest {
    animation: chest-sink 0.65s ease-in forwards;
}

@keyframes chest-sink {
    0% { transform: translateY(0); }
    20% { transform: translateY(-14px) rotate(-4deg); }
    40% { transform: translateY(0) rotate(4deg); }
    100% { transform: translateY(160px) scale(0.7); opacity: 0; filter: brightness(0.4); }
}

/* Вспышка в момент открытия */
.chest-opening-flash {
    position: absolute;
    inset: 0;
    pointer-events: none;
    opacity: 0;
    background: radial-gradient(circle at 50% 45%, #fff 0%, color-mix(in srgb, var(--chest-color) 70%, transparent) 25%, transparent 60%);
    z-index: 20;
}

.stage-dealing:not(.variant-vein) .chest-opening-flash {
    animation: flash 0.6s ease-out;
}

@keyframes flash {
    0% { opacity: 0; transform: scale(0.6); }
    20% { opacity: 1; }
    100% { opacity: 0; transform: scale(1.6); }
}

/* ---------- Ряд карт ---------- */
.chest-opening-row {
    grid-area: 1 / 1;
    display: flex;
    justify-content: center;
    gap: 8px;
    z-index: 10;
}

.chest-opening-row.row-wrap {
    flex-wrap: wrap;
    max-width: calc(3 * 220px * var(--card-scale) + 16px);
}

.chest-slot {
    position: relative;
    opacity: 0;
    transform: var(--final);
}

/* Пока сундук открывается, слоты уже в разметке (для замера), но невидимы */
.stage-dealing .chest-slot {
    animation-duration: var(--duration);
    animation-delay: var(--delay);
    animation-fill-mode: both;
}

.stage-reveal .chest-slot,
.stage-done .chest-slot {
    opacity: 1;
}

.chest-slot-size {
    width: calc(220px * var(--card-scale));
    height: calc(320px * var(--card-scale));
}

.chest-slot-scale {
    width: 220px;
    height: 320px;
    transform: scale(var(--card-scale));
    transform-origin: top left;
}

/* Взрыв */
.variant-burst.stage-dealing .chest-slot {
    animation-name: deal-burst;
    animation-timing-function: cubic-bezier(0.18, 1.25, 0.4, 1);
}

@keyframes deal-burst {
    0% { transform: translate(var(--from-x), var(--from-y)) scale(0.15) rotate(var(--spin)); opacity: 0; }
    12% { opacity: 1; }
    100% { transform: var(--final); opacity: 1; }
}

/* Раздача */
.variant-deal.stage-dealing .chest-slot {
    animation-name: deal-deal;
    animation-timing-function: ease-out;
}

@keyframes deal-deal {
    0% { transform: translate(var(--from-x), var(--from-y)) scale(0.3) rotate(-25deg); opacity: 0; }
    10% { opacity: 1; }
    70% { transform: translate(0px, -22px) rotate(3deg) scale(1.06); opacity: 1; }
    100% { transform: var(--final); opacity: 1; }
}

/* Веер */
.variant-fan.stage-dealing .chest-slot {
    animation-name: deal-fan;
    animation-timing-function: cubic-bezier(0.45, 0, 0.25, 1);
}

@keyframes deal-fan {
    0% { transform: translate(var(--from-x), var(--from-y)) scale(0.2); opacity: 0; }
    30% { transform: translate(var(--pile-x), calc(var(--pile-y) - 50px)) scale(0.85); opacity: 1; }
    52% { transform: translate(var(--pile-x), var(--pile-y)) scale(1); opacity: 1; }
    85% { transform: var(--final) translateY(-10px); opacity: 1; }
    100% { transform: var(--final); opacity: 1; }
}

/* Жила: карта вырастает из трещины снизу, видно только то, что выше трещины */
.variant-vein.stage-dealing .chest-slot {
    animation-name: deal-vein;
    animation-timing-function: cubic-bezier(0.3, 0.9, 0.4, 1);
}

@keyframes deal-vein {
    0% { transform: translateY(100%); clip-path: inset(-40% -40% 100% -40%); opacity: 1; }
    65% { transform: translateY(-8%); clip-path: inset(-40% -40% -8% -40%); }
    75% { transform: translateY(0) rotate(-2deg); }
    85% { transform: translateY(-3%) rotate(2deg); }
    100% { transform: var(--final); clip-path: inset(-40% -40% -40% -40%); opacity: 1; }
}

.vein-seam {
    grid-area: 1 / 1;
    align-self: center;
    width: min(1200px, 92vw);
    height: 6px;
    /* В центрированной ячейке отступ сдвигает на половину: полоса встаёт под нижний край ряда */
    margin-top: calc(320px * var(--card-scale));
    border-radius: 3px;
    background: linear-gradient(90deg, transparent, var(--chest-color) 15%, #fff 50%, var(--chest-color) 85%, transparent);
    box-shadow: 0 0 18px 4px var(--chest-color), 0 0 60px 12px color-mix(in srgb, var(--chest-color) 50%, transparent);
    clip-path: polygon(0 40%, 8% 0, 17% 70%, 26% 20%, 35% 90%, 44% 10%, 53% 80%, 62% 15%, 71% 85%, 80% 25%, 89% 75%, 100% 40%, 100% 60%, 0 60%);
    animation: seam-open 0.6s ease-out 0.3s both;
    z-index: 11;
}

.stage-reveal .vein-seam,
.stage-done .vein-seam {
    opacity: 0.35;
    transition: opacity 0.8s;
}

@keyframes seam-open {
    from { transform: scaleX(0); opacity: 0; }
}

/* ---------- Подсказки редкости и празднование ---------- */
/* Мифическая и выше ещё в рубашке: свечение по контуру самой жилы (drop-shadow повторяет её силуэт) */
.slot-tease .chest-slot-scale {
    animation: tease 1.3s ease-in-out infinite;
}

@keyframes tease {
    0%, 100% { filter: drop-shadow(0 0 4px var(--slot-color)); }
    50% { filter: drop-shadow(0 0 10px var(--slot-color)) drop-shadow(0 0 26px var(--slot-color)); }
}

/* Легендарная и выше раскрыта: вспышка свечения по карте, дальше спокойное дыхание */
.slot-celebrate .chest-slot-scale {
    animation: celebrate-flare 0.9s ease-out, celebrate-glow 2s ease-in-out 0.9s infinite;
}

@keyframes celebrate-flare {
    0% { filter: drop-shadow(0 0 0 var(--slot-color)) brightness(1.8); }
    35% { filter: drop-shadow(0 0 18px var(--slot-color)) drop-shadow(0 0 40px var(--slot-color)) brightness(1.3); }
    100% { filter: drop-shadow(0 0 8px var(--slot-color)) brightness(1); }
}

@keyframes celebrate-glow {
    0%, 100% { filter: drop-shadow(0 0 8px var(--slot-color)); }
    50% { filter: drop-shadow(0 0 14px var(--slot-color)) drop-shadow(0 0 4px #fff); }
}

/* Блик по лицевой стороне: только в границах карты (200×300 со скруглением, как у CardComponent) */
.chest-slot-shine {
    position: absolute;
    top: 10px;
    left: 10px;
    width: 200px;
    height: 300px;
    border-radius: 16px;
    overflow: hidden;
    pointer-events: none;
    z-index: 20;
}

.chest-slot-shine::before {
    content: '';
    position: absolute;
    top: -20%;
    bottom: -20%;
    width: 45%;
    left: -60%;
    background: linear-gradient(100deg, transparent, rgba(255, 255, 255, 0.55), transparent);
    transform: skewX(-15deg);
    animation: shine-sweep 0.9s ease-in-out 0.15s both;
}

@keyframes shine-sweep {
    to { left: 130%; }
}

/* Искры разлетаются из центра карты */
.chest-slot-sparks {
    position: absolute;
    left: 50%;
    top: 50%;
    width: 0;
    height: 0;
    pointer-events: none;
    z-index: 25;
}

.chest-slot-sparks span {
    position: absolute;
    width: var(--spark-size);
    height: var(--spark-size);
    margin: calc(var(--spark-size) / -2);
    border-radius: 50%;
    background: #fff;
    box-shadow: 0 0 6px 2px var(--slot-color), 0 0 14px 4px var(--slot-color);
    animation: spark-fly 0.9s cubic-bezier(0.15, 0.8, 0.3, 1) both;
}

@keyframes spark-fly {
    0% { transform: translate(0, 0) scale(1.2); opacity: 1; }
    70% { opacity: 1; }
    100% { transform: translate(var(--spark-x), var(--spark-y)) scale(0.2); opacity: 0; }
}

/* Тряска слота от ударов (сама карта трескается внутри CardComponent) */
.slot-hit-1 .chest-slot-size {
    animation: slot-shake 0.18s ease-out;
}

.slot-hit-2 .chest-slot-size {
    animation: slot-shake-hard 0.22s ease-out;
}

@keyframes slot-shake {
    30% { transform: translate(-3px, 1px); }
    60% { transform: translate(3px, -1px); }
}

@keyframes slot-shake-hard {
    25% { transform: translate(-5px, 2px) rotate(-1deg); }
    50% { transform: translate(5px, -2px) rotate(1deg); }
    75% { transform: translate(-3px, 1px); }
}

.slot-revealed .chest-slot-size {
    animation: slot-pop 0.35s cubic-bezier(0.2, 1.6, 0.4, 1);
}

@keyframes slot-pop {
    0% { transform: scale(1); }
    40% { transform: scale(1.08); }
    100% { transform: scale(1); }
}

/* Пропуск: всё без анимаций */
.skipped .chest-slot,
.skipped .chest-slot-size {
    animation: none !important;
}
</style>
