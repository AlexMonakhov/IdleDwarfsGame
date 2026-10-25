<script lang="ts" setup >
import { ref, defineProps, computed, onMounted, onBeforeUnmount, inject, watch } from 'vue';
import CharacterSkeleton from './character-skeleton.vue';
import CharacterStats from './character-stats.vue';
import { HeroModel } from '@/models/hero';
import { BattleUnit, auraEffectOf } from '@/battle/BattleState';
import { elementColor } from '@/battle/sprites';
import { BATTLE_FX_KEY } from '@/battle/BattleFx';
import { DAMAGE_COLORS, combineStatusVisuals, resolveAuraVisual } from '@/battle/FxRegistry';
import { registerUnitView, unregisterUnitView, UnitView } from '@/battle/UnitRegistry';
import { STATUS_ICONS } from './icons/status';
import DialogueBubble from './DialogueBubble.vue';
import { statusLabel } from '@/i18n';

const props = defineProps<{
  unit: BattleUnit;
  sprite?: string;
  frameWidth?: number;
  frameHeight?: number;
  frameCount?: number;
  scale?: number;
  rotate?: boolean;
}>();

const rootRef = ref<HTMLElement | null>(null);
const spriteRef = ref<HTMLElement | null>(null);
const skeletonRef = ref<InstanceType<typeof CharacterSkeleton> | null>(null);
const fxClass = ref('');
const hasStatuses = computed(() => Object.keys(props.unit.statuses ?? {}).length > 0);

// Скелету нужна модель старого формата (цвет/уровень для выбора спрайта)
const model = computed<HeroModel>(() => ({
  id: props.unit.id,
  name: props.unit.name,
  currentHp: props.unit.hp,
  maxHp: props.unit.maxHp,
  lvl: props.unit.level,
  color: elementColor(props.unit.element),
  unitType: props.unit.unitType,
}));

const getSkeletonStyle = computed(() => {
  return {
    transform: props.rotate ? 'rotateY(180deg)' : 'none',
  };
});

const fx = inject(BATTLE_FX_KEY, null);

// Визуал активных статусов: окраска, замедление анимации, CSS-классы
const statusNames = computed(() => (props.unit.alive ? Object.keys(props.unit.statuses ?? {}) : []));
const statusVisual = computed(() => combineStatusVisuals(statusNames.value));

// Пока юнит — цель удара, его силуэт пульсирует красным поверх окраски статусов
const TARGET_TINT = { color: '#ff1a1a', alpha: 0.35, pulse: 2.5 };
const spriteTint = computed(() => (props.unit.focus?.role === 'target' ? TARGET_TINT : statusVisual.value.tint));

const wrapperClasses = computed(() => [
  { dead: !props.unit.alive },
  props.unit.focus ? `focus-${props.unit.focus.role}` : '',
  ...statusVisual.value.classes,
]);

const wrapperStyle = computed(() => ({
  // Направление «лицом к врагу»: левая команда смотрит вправо
  '--face': props.rotate ? -1 : 1,
  '--focus-color': props.unit.focus?.color ?? '#ffe8a3',
}));

// Постоянные эффекты статусов (пламя, снежинки, звёзды стана) — пока статус висит на юните
const auras = new Map<string, string[]>();
const syncAuras = () => {
  if (!fx || !spriteRef.value) return;
  const active = new Set(statusNames.value);
  for (const [name, ids] of auras) {
    if (!active.has(name)) {
      fx.stopAura(ids);
      auras.delete(name);
    }
  }
  for (const name of active) {
    if (!auras.has(name)) auras.set(name, fx.startAura(spriteRef.value, name));
  }
};
const clearAuras = () => {
  auras.forEach(ids => fx?.stopAura(ids));
  auras.clear();
};

watch(() => statusNames.value.join('|'), syncAuras, { flush: 'post' });

// Аура героя: пульсирующая сфера цвета её эффекта, пока носитель жив
const auraEffect = computed(() => auraEffectOf(props.unit));
const heroAuraKey = computed(() =>
  props.unit.alive && auraEffect.value ? `${props.unit.id}|${auraEffect.value}|${props.unit.auraTarget}` : '');
let heroAuraIds: string[] = [];
const syncHeroAura = () => {
  if (!fx) return;
  fx.stopAura(heroAuraIds);
  heroAuraIds = [];
  if (heroAuraKey.value && spriteRef.value) {
    heroAuraIds = fx.startHeroAura(spriteRef.value, resolveAuraVisual(auraEffect.value, props.unit.auraTarget));
  }
};

watch(heroAuraKey, syncHeroAura, { flush: 'post' });

const floatingStyle = (kind: string, damageType?: string) => ({
  color: kind === 'heal'
    ? DAMAGE_COLORS.Heal
    : kind === 'damage'
      ? DAMAGE_COLORS[damageType ?? 'Physical'] ?? '#fff'
      : kind === 'immune' ? '#e5e7eb' : '#fde68a',
});

// Перезапуск CSS-анимации: снять класс, дождаться кадра, повесить снова
let fxTimer = 0;
const flash = (cls: string, duration: number) => {
  fxClass.value = '';
  window.clearTimeout(fxTimer);
  requestAnimationFrame(() => {
    fxClass.value = cls;
    fxTimer = window.setTimeout(() => (fxClass.value = ''), duration);
  });
};

const view: UnitView = {
  el: () => spriteRef.value,
  attack: () => {
    skeletonRef.value?.playAnimation('attack');
    skeletonRef.value?.flash('#fff6d5', 0.35, 300);
    flash('fx-lunge', 650);
  },
  cast: (color?: string) => {
    skeletonRef.value?.playAnimation('attack');
    skeletonRef.value?.flash(color ?? DAMAGE_COLORS.Magical, 0.4, 600);
  },
  hit: (_damageType, options) => {
    flash(options?.crit ? 'fx-hit fx-crit' : 'fx-hit', 450);
    skeletonRef.value?.flash(options?.flash ?? '#ff1414', options?.crit ? 0.95 : 0.85, 450);
  },
  heal: () => {
    flash('fx-heal', 700);
    skeletonRef.value?.flash(DAMAGE_COLORS.Heal, 0.45, 600);
  },
  aura: (color: string) => {
    flash('fx-aura', 600);
    skeletonRef.value?.flash(color, 0.5, 550);
  },
  victory: () => skeletonRef.value?.playAnimation('victory'),
};

// id юнита может смениться при новом бое, а компонент переиспользоваться
watch(() => props.unit.id, (id, oldId) => {
  unregisterUnitView(oldId, view);
  registerUnitView(id, view);
});

onMounted(() => {
  registerUnitView(props.unit.id, view);
  syncAuras();
  syncHeroAura();
});
onBeforeUnmount(() => {
  unregisterUnitView(props.unit.id, view);
  clearAuras();
  fx?.stopAura(heroAuraIds);
});

defineExpose(view);
</script>

<template>
    <div ref="rootRef" class="character-wrapper" :class="wrapperClasses" :style="wrapperStyle">
        <!-- Реплика висит над панелью эффектов; key перезапускает анимацию на каждой новой фразе -->
        <Transition name="speech">
          <div v-if="unit.speech" :key="unit.speech.id" class="character-speech">
            <DialogueBubble :text="unit.speech.text" />
          </div>
        </Transition>
        <div class="character-header">
          <!-- Панель эффектов висит над именем и не сдвигает шапку, когда появляется -->
          <div v-if="hasStatuses" class="status-panel">
            <div class="status-panel-inner">
              <span v-for="(stacks, name) in unit.statuses" :key="name" class="status-icon" :title="`${statusLabel(String(name))} x${stacks}`">
                <component v-if="STATUS_ICONS[name]" :is="STATUS_ICONS[name]" size="16px" />
                <template v-else>{{ name[0] }}</template>
                <sub v-if="stacks > 1">{{ stacks }}</sub>
              </span>
            </div>
          </div>
          <div class="character-name">{{ unit.name }}</div>
          <CharacterStats
            :currentHp="unit.hp"
            :maxHp="unit.maxHp"
            :energy="unit.energy"
            :maxEnergy="unit.maxEnergy"></CharacterStats>
        </div>

        <div class="floating-layer">
            <div
              v-for="(text, index) in unit.floatingTexts"
              :key="text.id"
              class="floating-text"
              :class="[`floating-${text.kind}`, { 'floating-crit': text.crit }]"
              :style="[floatingStyle(text.kind, text.damageType), { left: `${50 + ((index % 3) - 1) * 18}%` }]"
            >{{ text.text }}</div>
        </div>

        <div ref="spriteRef" class="sprite-box" :class="fxClass">
          <div class="unit-shadow"></div>
          <div class="unit-body">
            <CharacterSkeleton
                  ref="skeletonRef"
                  :style="getSkeletonStyle"
                  :key="unit.id"
                  :sprite-src="props.sprite"
                  :model="model"
                  :frame-width="props.frameWidth ?? 409.6"
                  :frame-height="props.frameHeight ?? 409.6"
                  :frame-count="props.frameCount ?? 24"
                  :fps="12"
                  :columns="5"
                  :scale="props.scale ?? 0.65"
                  :padding="1.1"
                  :tint="spriteTint"
                  :speed="statusVisual.animSpeed"
                  :paused="!unit.alive"></CharacterSkeleton>
          </div>
        </div>
    </div>
</template>

<style>
  .character-wrapper{
    width: min-content;
    position: relative;
  }
  .character-header{
    position: absolute;
    top: 2%;
    left: 25%;
    width: 50%;
    z-index: 2;
    text-align: center;
  }
  /* Над панелью статусов (она выше шапки примерно на 30px) */
  .character-speech{
    position: absolute;
    top: -34px;
    left: 50%;
    transform: translate(-50%, -100%);
    z-index: 4;
    pointer-events: none;
  }
  .speech-leave-active{
    transition: opacity 0.25s ease;
  }
  .speech-leave-to{
    opacity: 0;
  }
  .character-name{
    color: #f8fafc;
    font-size: 12px;
    font-weight: 700;
    white-space: nowrap;
    text-shadow: 0 1px 3px #000;
    margin-bottom: 4px;
  }
  /* Рамка как у полосок HP/энергии в character-stats */
  .status-panel{
    position: absolute;
    bottom: calc(100% + 4px);
    left: 50%;
    transform: translateX(-50%);
    padding: 2px;
    border-radius: 6px;
    corner-shape: bevel;
    background: var(--metalic-background);
    border: 0.7px solid rgba(0, 0, 0, 0.1);
    box-shadow:
      0 3px 6px rgba(0, 0, 0, 0.5),
      inset 0 1px 2px rgba(255, 255, 255, 0.8);
  }
  .status-panel-inner{
    display: flex;
    gap: 5px;
    padding: 2px 5px;
    border-radius: 4px;
    corner-shape: bevel;
    background: var(--wood-background);
    box-shadow: inset 0 0 4px black, inset 0 0 8px #333;
  }
  .status-icon{
    position: relative;
    display: inline-flex;
    align-items: center;
    font-size: 13px;
    line-height: 16px;
    color: #4ade80;
    filter: drop-shadow(0 1px 2px rgba(0, 0, 0, 0.8));
  }
  .status-icon sub{
    position: absolute;
    right: -4px;
    bottom: -4px;
    font-size: 9px;
    font-weight: 700;
    line-height: 1;
    color: #fff;
    text-shadow: 0 0 2px #000, 0 0 2px #000;
  }
  .floating-layer{
    position: absolute;
    inset: 15% 0 auto 0;
    height: 0;
    z-index: 3;
    pointer-events: none;
  }
  .floating-text{
    position: absolute;
    transform: translateX(-50%);
    font-weight: 800;
    font-size: 22px;
    white-space: nowrap;
    text-shadow: 0 2px 4px #000, 0 0 2px #000;
    animation: float-up 1.1s ease-out forwards;
  }
  .floating-status, .floating-immune{
    font-size: 15px;
  }
  @keyframes float-up {
    0% { opacity: 0; margin-top: 30px; }
    15% { opacity: 1; }
    100% { opacity: 0; margin-top: -40px; }
  }
  .floating-crit{
    font-size: 30px;
    animation: float-up-crit 1.1s ease-out forwards;
  }
  @keyframes float-up-crit {
    0% { opacity: 0; margin-top: 30px; transform: translateX(-50%) scale(0.6); }
    15% { opacity: 1; transform: translateX(-50%) scale(1.35); }
    30% { transform: translateX(-50%) scale(1); }
    100% { opacity: 0; margin-top: -45px; transform: translateX(-50%) scale(1); }
  }

  /* Спрайт: тень под ногами (z 0) и тело персонажа (z 1). Доли совпадают с UNIT_ANCHORS в FxRegistry. */
  .sprite-box{
    position: relative;
    isolation: isolate;
  }
  .unit-body{
    position: relative;
    z-index: 1;
    transform-origin: 50% 72%;
    transition: transform 0.2s ease-out, filter 0.2s ease-out;
  }
  .unit-body canvas{
    display: block;
  }
  .unit-shadow{
    position: absolute;
    z-index: 0;
    top: 72%;
    left: calc(50% + 3% * var(--face, 1));
    width: 100px;
    height: 24px;
    transform: translate(-50%, -50%);
    border-radius: 50%;
    background: radial-gradient(ellipse at center, rgba(0, 0, 0, 0.6) 0%, rgba(0, 0, 0, 0.35) 45%, rgba(0, 0, 0, 0) 72%);
    pointer-events: none;
    transition: box-shadow 0.15s ease-out, opacity 0.3s;
  }
  .dead .unit-shadow{
    opacity: 0.5;
  }

  /* Чей ход (атакующий/кастер): свечение и +10% размера с начала хода */
  .focus-attacker .unit-body,
  .focus-caster .unit-body{
    transform: scale(1.1);
  }
  /* Рывок в сторону врага в момент удара */
  .fx-lunge .unit-body{
    transform: translateX(calc(18px * var(--face, 1))) scale(1.1);
  }
  .focus-attacker .unit-body,
  .focus-caster .unit-body{
    filter: drop-shadow(0 0 4px var(--focus-color)) drop-shadow(0 0 12px var(--focus-color));
  }
  .focus-attacker .unit-shadow,
  .focus-caster .unit-shadow{
    box-shadow: 0 0 0 2px var(--focus-color), 0 0 16px 4px var(--focus-color);
  }

  /* Цель: яркий красный контур, пульсирующее кольцо под ногами (+ красная окраска силуэта, см. TARGET_TINT) */
  .focus-target .unit-body{
    filter:
      drop-shadow(0 0 2px #ff3030)
      drop-shadow(0 0 6px rgba(255, 20, 20, 1))
      drop-shadow(0 0 16px rgba(255, 0, 0, 0.85));
  }
  .focus-target .unit-shadow{
    animation: target-ring 0.4s ease-in-out infinite alternate;
  }
  @keyframes target-ring {
    from { box-shadow: 0 0 0 3px rgba(255, 50, 50, 1), 0 0 14px 4px rgba(255, 20, 20, 0.75); }
    to { box-shadow: 0 0 0 4px rgba(255, 110, 110, 1), 0 0 28px 10px rgba(255, 20, 20, 0.95); }
  }

  .fx-hit .unit-body{
    animation: unit-shake 0.45s ease;
  }
  .fx-crit .unit-body{
    animation: unit-shake-crit 0.5s ease;
  }
  .dead canvas{
    filter: grayscale(100%) brightness(0.6);
  }
  .fx-heal canvas{
    filter: drop-shadow(0 0 18px #4ade80) brightness(1.15);
  }
  @keyframes unit-shake {
    0%, 100% { transform: translateX(0); }
    20% { transform: translateX(calc(-10px * var(--face, 1))); }
    40% { transform: translateX(calc(8px * var(--face, 1))); }
    60% { transform: translateX(calc(-6px * var(--face, 1))); }
    80% { transform: translateX(calc(4px * var(--face, 1))); }
  }
  @keyframes unit-shake-crit {
    0%, 100% { transform: translateX(0) scale(1); }
    15% { transform: translateX(calc(-16px * var(--face, 1))) scale(0.95); }
    35% { transform: translateX(calc(12px * var(--face, 1))) scale(1.02); }
    55% { transform: translateX(calc(-8px * var(--face, 1))); }
    75% { transform: translateX(calc(5px * var(--face, 1))); }
  }

  /* Статусы (см. STATUS_VISUALS.className) */
  .status-stunned canvas{
    filter: saturate(0.6) brightness(0.9);
  }
  .status-frozen canvas{
    filter: saturate(0.4) brightness(1.15) drop-shadow(0 0 6px #bae6fd);
  }

  /* Срабатывание ауры: носитель на миг «вдыхает» силу (сама сфера рисуется на канвасе, см. AuraSphereStrategy) */
  .fx-aura .unit-body{
    animation: aura-swell 0.6s ease-out;
  }
  @keyframes aura-swell {
    0%, 100% { transform: scale(1); }
    35% { transform: scale(1.08); }
  }
</style>
