<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, provide, ref } from 'vue';
import CanvasLayer from '@/components/canvas/CanvasLayer.vue';
import CharacterComponent from '@/components/character-component.vue';
import { BattleUnit, auraEffectOf, createBattleState, resetBattleState } from '@/battle/BattleState';
import { BattlePlayer } from '@/battle/BattlePlayer';
import { BATTLE_FX_KEY, BattleFx } from '@/battle/BattleFx';
import { resolveAuraVisual } from '@/battle/FxRegistry';
import { getUnitView } from '@/battle/UnitRegistry';
import { elementColor } from '@/battle/sprites';
import { CombatResult, HeroElement } from '@/battle/types';
import { soundService } from '@/services/soundService/SoundService';
import caveBackground from '@/assets/red_crystal_arena.png';
import crystalsBackground from '@/assets/arena_blue_crystals.png';

// Маленькая арена для превью: проигрывает сцену по кругу тем же BattlePlayer, что и настоящий бой.
// Новая сцена — новый компонент: родитель меняет key
const props = defineProps<{
    scene: CombatResult;
    // Стихия задаёт фон, как на сцене прокачки
    element: HeroElement;
    // Сцена без логов: носитель ауры сам пускает волны, как при её срабатывании в бою
    auraPulse?: boolean;
}>();

const background = computed(() => (elementColor(props.element) === 'red' ? caveBackground : crystalsBackground));

// Пауза между повторами сцены и период волн ауры, мс
const LOOP_PAUSE = 1200;
const AURA_PULSE = 2600;

// Где стоят юниты: left — % ширины сцены, raise — на сколько px выше пола (стоит дальше, рисуется позади).
// Четверо — заклинатель и три цели: между ними просвет, чтобы было видно полёт заклинания
interface Slot {
    left: number;
    raise?: number;
}
const SLOTS: Record<number, Slot[]> = {
    1: [{ left: 50 }],
    2: [{ left: 28 }, { left: 72 }],
    4: [{ left: 13 }, { left: 47, raise: 6 }, { left: 67, raise: 22 }, { left: 87 }],
};

// В толпе юниты мельче, чтобы не наезжали друг на друга
const CROWD_SCALE = 0.52;

const layerRef = ref<InstanceType<typeof CanvasLayer> | null>(null);
const state = createBattleState();
const fx = new BattleFx(() => layerRef.value?.particleService ?? null, () => 1);
const player = new BattlePlayer(state, fx);
provide(BATTLE_FX_KEY, fx);

// Слева направо: свои от задней линии к передней, затем враги
const placed = computed(() => {
    const units = Object.values(state.units)
        .sort((a, b) => a.team - b.team || (a.team === 1 ? b.position - a.position : a.position - b.position));
    const slots = SLOTS[units.length] ?? units.map((_, i) => ({ left: ((i + 1) * 100) / (units.length + 1) }));
    return units.map((unit, i) => ({ unit, slot: slots[i] }));
});

const crowded = computed(() => placed.value.length > 2);

const wait = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

let active = true;
let pulseTimer = 0;

const loop = async () => {
    while (active) {
        await player.play(props.scene);
        if (!active) return;
        await wait(LOOP_PAUSE);
    }
};

const pulse = (unit: BattleUnit) => {
    const effect = auraEffectOf(unit);
    if (!effect) return;
    const visual = resolveAuraVisual(effect, unit.auraTarget);
    const view = getUnitView(unit.id);
    view?.aura?.(visual.color);
    fx.auraBurst(view?.el?.(), visual);
    soundService.cast();
};

onMounted(() => {
    if (!props.auraPulse) {
        loop();
        return;
    }
    resetBattleState(state, props.scene);
    pulseTimer = window.setInterval(() => Object.values(state.units).forEach(pulse), AURA_PULSE);
});

onBeforeUnmount(() => {
    active = false;
    player.stop();
    window.clearInterval(pulseTimer);
});
</script>

<template>
    <div class="preview-stage" :style="{ backgroundImage: `url(${background})` }">
        <CanvasLayer ref="layerRef">
            <div
                v-for="{ unit, slot } in placed"
                :key="unit.id"
                class="preview-unit"
                :class="{ 'preview-unit-crowd': crowded }"
                :style="{ left: `${slot.left}%`, marginBottom: `${slot.raise ?? 0}px`, zIndex: 100 - (slot.raise ?? 0) }"
            >
                <CharacterComponent :unit="unit" :rotate="unit.team === 2" :scale="crowded ? CROWD_SCALE : undefined" />
            </div>
        </CanvasLayer>
    </div>
</template>

<style scoped>
.preview-stage {
    position: relative;
    /* Канвасы частиц с z-index 9999 не вылезают поверх остального окна */
    isolation: isolate;
    height: 100%;
    min-height: 320px;
    overflow: hidden;
    border-radius: 14px;
    border: 2px solid #333;
    background-color: #1b1210;
    background-size: cover;
    background-position: center 65%;
    box-shadow: 0 0 0 3px rgb(157, 173, 175), inset 0 0 40px rgba(0, 0, 0, 0.85);
}

/* Затемнение сверху и снизу, как у сцены прокачки; ::before — чтобы лечь под канвасы частиц */
.preview-stage::before {
    content: '';
    position: absolute;
    inset: 0;
    background: linear-gradient(to bottom, rgba(0, 0, 0, 0.35) 0%, rgba(0, 0, 0, 0) 35%, rgba(0, 0, 0, 0.5) 100%);
    pointer-events: none;
}

/* Под ногами в кадре пустое поле — опускаем его за край, чтобы юнит стоял на полу */
.preview-unit {
    position: absolute;
    bottom: -56px;
    transform: translateX(-50%);
}

/* Пустое поле под ногами мельче вместе со спрайтом */
.preview-unit-crowd {
    bottom: -45px;
}
</style>
