<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import UnitAvatar from '@/components/interface/UnitAvatar.vue';
import CssAuraEffect from '@/components/effects/CssAuraEffect.vue';
import { elementAura } from '@/components/effects/elementAura';
import { ELEMENT_COLORS } from '@/components/inventory/lootStyles';
import { useTutorial } from '@/composables/UseTutorial';
import { uiScale } from '@/composables/UseUiScale';
import { anchorRect, findAnchor, isClipped, isInAnchor } from '@/tutorial/anchors';
import type { TutorialPlacement } from '@/tutorial/types';

// Шаг обучения поверх всего: затемнение с вырезом под подсвеченный элемент, рамка вокруг него и облачко
// наставника. Живёт вне .ui-stage (телепорт в body, как модалки), поэтому считает всё в пикселях окна:
// getBoundingClientRect уже учитывает масштаб интерфейса, а само облачко масштабируется на --ui-scale
const { t } = useI18n();
const { active, currentStep, next, skip } = useTutorial();

// Якорь ещё не появился (окно открывается, список грузится) — ждём столько, потом облачко встаёт по центру
const ANCHOR_WAIT_MS = 3000;
const HOLE_PADDING = 8;
const GAP = 16;
const MARGIN = 12;

interface Rect { left: number, top: number, width: number, height: number }

const hole = ref<Rect | null>(null);
const anchorMissing = ref(false);
const viewport = ref({ width: window.innerWidth, height: window.innerHeight });
const bubbleEl = ref<HTMLElement | null>(null);
const bubbleSize = ref({ width: 0, height: 0 });
const confirmSkip = ref(false);

// Облачко крупнее остального интерфейса — его текст читают, а не узнают по виду. Растёт всё сразу (текст, портрет,
// кнопки), но не шире экрана: ширина облачка в логических пикселях — .tutorial-bubble
const BUBBLE_ZOOM = 1.5;
const BUBBLE_WIDTH = 470;
const bubbleScale = computed(() =>
  Math.min(uiScale.value * BUBBLE_ZOOM, (viewport.value.width - 2 * MARGIN) / BUBBLE_WIDTH));

const scenario = computed(() => active.value?.scenario ?? null);
const stepNumber = computed(() => (active.value?.stepIndex ?? 0) + 1);
const stepCount = computed(() => scenario.value?.steps.length ?? 0);
const isLast = computed(() => stepNumber.value === stepCount.value);
const blocking = computed(() => currentStep.value?.block ?? true);
// Без якоря облачко не показываем, пока не истечёт ожидание: иначе оно мелькнёт по центру и перепрыгнет
// Скрытый шаг облачка не показывает; шаг с задержкой — только когда она прошла; ждущий шаг (wait) без якоря молчит
const delayPassed = ref(true);
const ready = computed(() => {
  const step = currentStep.value;
  if (!step || step.hidden || !delayPassed.value) return false;
  return !step.target || !!hole.value || (anchorMissing.value && !step.wait);
});

let stepStartedAt = 0;
// Якорь за краем докручиваем: окно ещё может въезжать, поэтому повторяем, если он всё ещё обрезан (не чаще и не больше)
const SCROLL_RETRY_MS = 700;
const SCROLL_ATTEMPTS = 3;
let scrollAttempts = 0;
let lastScrollAt = 0;
let delayTimer = 0;
watch(currentStep, step => {
  stepStartedAt = performance.now();
  scrollAttempts = 0;
  lastScrollAt = 0;
  hole.value = null;
  anchorMissing.value = false;
  confirmSkip.value = false;
  window.clearTimeout(delayTimer);
  delayPassed.value = !step?.delay;
  if (step?.delay) delayTimer = window.setTimeout(() => { delayPassed.value = true; }, step.delay);
});

const sameRect = (a: Rect | null, b: Rect | null) =>
  a === b || (!!a && !!b && a.left === b.left && a.top === b.top && a.width === b.width && a.height === b.height);

// Положение якоря меряем каждый кадр, пока идёт шаг: окно въезжает с анимацией, город прокручивается,
// список догружается — подписываться на всё это по отдельности сложнее, чем один замер за кадр
let frame = 0;
const measure = () => {
  frame = requestAnimationFrame(measure);
  const step = currentStep.value;
  if (!step) return;

  if (viewport.value.width !== window.innerWidth || viewport.value.height !== window.innerHeight) {
    viewport.value = { width: window.innerWidth, height: window.innerHeight };
  }

  let next: Rect | null = null;
  // Якорь за краем экрана или списка (город прокручивается, кандидаты в шахту внизу окна) — докручиваем один раз
  const el = step.target ? findAnchor(step.target) : null;
  const now = performance.now();
  if (el && scrollAttempts < SCROLL_ATTEMPTS && now - lastScrollAt > SCROLL_RETRY_MS && isClipped(el)) {
    scrollAttempts++;
    lastScrollAt = now;
    el.scrollIntoView({ behavior: 'smooth', block: 'nearest', inline: 'center' });
  }
  const rect = step.target ? anchorRect(step.target) : null;
  if (rect) {
    const pad = HOLE_PADDING * uiScale.value;
    next = {
      left: Math.round(rect.left - pad),
      top: Math.round(rect.top - pad),
      width: Math.round(rect.width + pad * 2),
      height: Math.round(rect.height + pad * 2),
    };
  } else if (step.target && !anchorMissing.value && performance.now() - stepStartedAt > ANCHOR_WAIT_MS) {
    anchorMissing.value = true;
  }
  if (!sameRect(hole.value, next)) hole.value = next;

  const bubble = bubbleEl.value;
  if (bubble) {
    const width = bubble.offsetWidth * bubbleScale.value;
    const height = bubble.offsetHeight * bubbleScale.value;
    if (width !== bubbleSize.value.width || height !== bubbleSize.value.height) bubbleSize.value = { width, height };
  }
};

// Затемнение — один путь: весь экран минус скруглённый вырез (evenodd). Клики в вырезе проходят к элементу
const shadePath = computed(() => {
  const { width, height } = viewport.value;
  const outer = `M0 0H${width}V${height}H0Z`;
  const rect = hole.value;
  if (!rect) return outer;
  const r = Math.min(12 * uiScale.value, rect.width / 2, rect.height / 2);
  const { left: x, top: y, width: w, height: h } = rect;
  return `${outer} M${x + r} ${y}H${x + w - r}A${r} ${r} 0 0 1 ${x + w} ${y + r}V${y + h - r}`
    + `A${r} ${r} 0 0 1 ${x + w - r} ${y + h}H${x + r}A${r} ${r} 0 0 1 ${x} ${y + h - r}V${y + r}A${r} ${r} 0 0 1 ${x + r} ${y}Z`;
});

const holeStyle = computed(() => hole.value && {
  left: `${hole.value.left}px`,
  top: `${hole.value.top}px`,
  width: `${hole.value.width}px`,
  height: `${hole.value.height}px`,
});

// Облачко у выреза: с заданной стороны, а если там не помещается — с той, где помещается.
// Не поместилось нигде (вырез на весь экран) — прижимаем к краю экрана поверх выреза
const ORDER: Exclude<TutorialPlacement, 'auto'>[] = ['bottom', 'top', 'right', 'left'];
const OPPOSITE = { top: 'bottom', bottom: 'top', left: 'right', right: 'left' } as const;

const bubblePosition = computed(() => {
  const { width: vw, height: vh } = viewport.value;
  const { width: bw, height: bh } = bubbleSize.value;
  const rect = hole.value;
  const clamp = (x: number, y: number) => ({
    left: Math.max(MARGIN, Math.min(x, vw - bw - MARGIN)),
    top: Math.max(MARGIN, Math.min(y, vh - bh - MARGIN)),
  });
  if (!rect) return clamp((vw - bw) / 2, (vh - bh) / 2);

  const gap = GAP * uiScale.value;
  const cx = rect.left + rect.width / 2;
  const cy = rect.top + rect.height / 2;
  const at = (side: Exclude<TutorialPlacement, 'auto'>) => {
    switch (side) {
      case 'bottom': return { x: cx - bw / 2, y: rect.top + rect.height + gap };
      case 'top': return { x: cx - bw / 2, y: rect.top - gap - bh };
      case 'right': return { x: rect.left + rect.width + gap, y: cy - bh / 2 };
      case 'left': return { x: rect.left - gap - bw, y: cy - bh / 2 };
    }
  };
  const fits = (side: Exclude<TutorialPlacement, 'auto'>) => {
    const { x, y } = at(side);
    // По оси вдоль выреза облачко можно сдвинуть, важно только не залезть на вырез поперёк
    if (side === 'bottom' || side === 'top') return y >= MARGIN && y + bh <= vh - MARGIN;
    return x >= MARGIN && x + bw <= vw - MARGIN;
  };

  const preferred = currentStep.value?.placement;
  const order = preferred && preferred !== 'auto' ? [preferred, OPPOSITE[preferred], ...ORDER] : ORDER;
  const side = order.find(fits);
  if (side) {
    const { x, y } = at(side);
    return clamp(x, y);
  }
  return clamp((vw - bw) / 2, vh - bh - MARGIN);
});

// Наставник — Брокк, огненный дворф 4-го ранга из обучающего боя (TutorialBattle.cs): его портрет и цвет стихии
const MENTOR = { name: 'Brokk', unitType: 'Dwarf', element: 'Fire', level: 4, isBoss: false } as const;
const MENTOR_AURA = elementAura(MENTOR.element);
const MENTOR_SIZE = 72;

const bubbleStyle = computed(() => ({
  '--tooltip-accent': ELEMENT_COLORS[MENTOR.element],
  left: `${bubblePosition.value.left}px`,
  top: `${bubblePosition.value.top}px`,
  transform: `scale(${bubbleScale.value})`,
  // До первого замера размер неизвестен — не показываем, чтобы облачко не прыгнуло
  visibility: bubbleSize.value.width ? 'visible' as const : 'hidden' as const,
}));

const advanceHint = computed(() => {
  const advance = currentStep.value?.advance;
  if (currentStep.value?.passive) return t('tutorial.ui.waitHint');
  if (advance === 'click-target') return t('tutorial.ui.clickHint');
  if (advance && advance !== 'next') return t('tutorial.ui.actionHint');
  return '';
});

const onSkip = () => {
  if (scenario.value?.mandatory && !confirmSkip.value) {
    confirmSkip.value = true;
    return;
  }
  skip();
};

// Esc закрывает шаг, а не окно под ним: ловим на захвате раньше ModalComponent и помечаем обработанным.
// Обязательное обучение Esc не закрывает
const onKeydown = (event: KeyboardEvent) => {
  // Шаг не показан (скрытый, ждёт якорь или задержку) — клавиши принадлежат игре (Esc пропускает анимацию сундука)
  if (event.key !== 'Escape' || !currentStep.value || !ready.value) return;
  event.preventDefault();
  event.stopPropagation();
  if (confirmSkip.value) confirmSkip.value = false;
  else if (!scenario.value?.mandatory) skip();
};

// Блокирующий шаг: проходят только клики по облачку и по подсвеченному — если шаг ждёт клика или действия
// (на шаге «Дальше» подсвеченное только показывают: открыть сундук посреди рассказа нельзя). Затемнение и так ловит клики мимо выреза,
// но вырез — прямоугольник, и в него может попасть край соседнего здания города
const POINTER_EVENTS = ['click', 'mousedown', 'pointerdown', 'touchstart'] as const;
const onPointerCapture = (event: Event) => {
  const step = currentStep.value;
  if (!step || !blocking.value || !ready.value) return;
  const target = event.target as Node;
  if (bubbleEl.value?.contains(target)) return;
  if (step.target && !step.passive && step.advance !== 'next' && isInAnchor(step.target, target)) return;
  event.preventDefault();
  event.stopPropagation();
};

onMounted(() => {
  frame = requestAnimationFrame(measure);
  window.addEventListener('keydown', onKeydown, true);
  POINTER_EVENTS.forEach(type => window.addEventListener(type, onPointerCapture, { capture: true, passive: false }));
});

onBeforeUnmount(() => {
  cancelAnimationFrame(frame);
  window.clearTimeout(delayTimer);
  window.removeEventListener('keydown', onKeydown, true);
  POINTER_EVENTS.forEach(type => window.removeEventListener(type, onPointerCapture, { capture: true }));
});
</script>

<template>
  <Teleport to="body">
    <div v-if="currentStep && ready" class="tutorial-overlay" :class="{ 'tutorial-overlay-blocking': blocking }">
      <svg class="tutorial-shade" :width="viewport.width" :height="viewport.height">
        <path :d="shadePath" fill-rule="evenodd" />
      </svg>
      <div v-if="holeStyle" class="tutorial-highlight" :style="holeStyle"></div>

      <div :key="currentStep.id" ref="bubbleEl" class="tutorial-bubble" :data-step="currentStep.id" :style="bubbleStyle">
        <!-- Портрет как на карточке героя: огненная аура стихии вокруг (4-й ранг — с аурой) -->
        <div class="tutorial-mentor">
          <CssAuraEffect :effect="MENTOR_AURA" :size="MENTOR_SIZE">
            <UnitAvatar :unit="MENTOR" :size="MENTOR_SIZE" :show-level="false" />
          </CssAuraEffect>
        </div>
        <div class="tutorial-body">
          <template v-if="confirmSkip">
            <p class="tutorial-text">{{ t('tutorial.ui.skipConfirm') }}</p>
            <div class="tutorial-actions">
              <button type="button" class="tutorial-link" @click="confirmSkip = false">{{ t('tutorial.ui.no') }}</button>
              <GemButton :color="'red'" @click="skip">{{ t('tutorial.ui.yes') }}</GemButton>
            </div>
          </template>
          <template v-else>
            <p class="tutorial-text">{{ t(`tutorial.${currentStep.text}`) }}</p>
            <div class="tutorial-actions">
              <!-- Счётчик — у коротких знакомств; у длинного стартового обучения он только пугает -->
              <span class="tutorial-counter">{{ stepCount > 1 && !scenario?.mandatory ? `${stepNumber}/${stepCount}` : '' }}</span>
              <button type="button" class="tutorial-link" @click="onSkip">{{ t(`tutorial.${scenario?.skipLabel ?? 'ui.skip'}`) }}</button>
              <GemButton v-if="currentStep.advance === 'next'" :color="'green'" @click="next">
                {{ isLast ? t('tutorial.ui.done') : t('tutorial.ui.next') }}
              </GemButton>
              <span v-else class="tutorial-hint">{{ advanceHint }}</span>
            </div>
          </template>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
/* Выше модалок (UseModalStack начинает с 1000 и растёт по 10) и их кнопок закрытия */
.tutorial-overlay {
  position: fixed;
  inset: 0;
  z-index: 100000;
  pointer-events: none;
  font-family: 'Arial', sans-serif;
}

.tutorial-shade {
  position: absolute;
  inset: 0;
  pointer-events: none;
}

.tutorial-shade path {
  fill: rgba(0, 0, 0, 0.55);
}

/* Блокирующий шаг: клик мимо выреза не доходит до игры. В вырезе пути нет — туда клик проходит */
.tutorial-overlay-blocking .tutorial-shade path {
  pointer-events: visiblePainted;
}

.tutorial-highlight {
  position: absolute;
  box-sizing: border-box;
  border-radius: calc(12px * var(--ui-scale));
  border: 3px solid #ffd166;
  box-shadow: 0 0 12px 2px rgba(255, 209, 102, 0.8), inset 0 0 10px rgba(255, 209, 102, 0.5);
  animation: tutorial-pulse 1.4s ease-in-out infinite;
  pointer-events: none;
}

@keyframes tutorial-pulse {
  0%, 100% { box-shadow: 0 0 10px 2px rgba(255, 209, 102, 0.7), inset 0 0 8px rgba(255, 209, 102, 0.4); }
  50% { box-shadow: 0 0 22px 6px rgba(255, 209, 102, 1), inset 0 0 14px rgba(255, 209, 102, 0.7); }
}

/* Облачко — как наши подсказки (TooltipPanel): толща кристалла в цвете стихии наставника, белый текст с обводкой.
   Размер — в логических пикселях, на экран его переводит bubbleScale (--ui-scale × BUBBLE_ZOOM) от левого верхнего угла */
.tutorial-bubble {
  position: absolute;
  display: flex;
  align-items: flex-start;
  column-gap: 16px;
  box-sizing: border-box;
  width: 470px;
  padding: 16px 18px;
  border: 1px solid color-mix(in srgb, var(--tooltip-accent) 70%, white);
  border-radius: 12px;
  /* Чуть светлее подсказки: текста здесь больше, и читаться он должен с первого взгляда */
  background: linear-gradient(165deg,
    color-mix(in srgb, var(--tooltip-accent) 55%, #5a7396) 0%,
    color-mix(in srgb, var(--tooltip-accent) 45%, #415775) 45%,
    color-mix(in srgb, var(--tooltip-accent) 32%, #2a3a52) 100%);
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.7);
  color: #f8fafc;
  text-align: left;
  text-shadow: var(--text-outline);
  transform-origin: 0 0;
  pointer-events: auto;
  animation: tutorial-bubble-in 0.25s ease-out;
}

@keyframes tutorial-bubble-in {
  from { opacity: 0; }
  to { opacity: 1; }
}

.tutorial-mentor {
  flex: none;
}

.tutorial-body {
  flex: 1;
  min-width: 0;
}

.tutorial-text {
  margin: 2px 0 12px;
  font-size: 17px;
  font-weight: bold;
  line-height: 1.4;
  white-space: pre-line;
}

.tutorial-actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  column-gap: 12px;
  padding-top: 8px;
  border-top: 1px solid rgba(255, 255, 255, 0.35);
}

.tutorial-counter {
  margin-right: auto;
  font-size: 14px;
  color: rgba(248, 250, 252, 0.75);
}

.tutorial-link {
  padding: 0;
  border: none;
  background: none;
  color: rgba(248, 250, 252, 0.75);
  font: inherit;
  font-size: 14px;
  text-decoration: underline;
  text-shadow: var(--text-outline);
  cursor: pointer;
}

.tutorial-link:hover {
  color: #fff;
}

.tutorial-hint {
  font-size: 14px;
  font-style: italic;
  color: rgba(248, 250, 252, 0.8);
}
</style>
