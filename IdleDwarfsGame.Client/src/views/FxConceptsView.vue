<template>
  <div class="lab">
    <div class="lab-bg" :style="bgStyle"></div>
    <CanvasLayer ref="layerRef">
      <div class="grid">
        <template v-for="unit in teamA" :key="unit.id">
          <CanvasComponent :class="['grid-item', `grid-item-left-${unit.position}`]" style="position: absolute">
            <CharacterComponent :unit="unit" :scale="UNIT_SCALE" />
          </CanvasComponent>
        </template>
        <template v-for="unit in teamB" :key="unit.id">
          <CanvasComponent :class="['grid-item', `grid-item-right-${unit.position}`]" style="position: absolute">
            <CharacterComponent :unit="unit" :scale="UNIT_SCALE" rotate />
          </CanvasComponent>
        </template>
      </div>

      <div class="lab-panel" :class="{ compact: panelHidden }">
        <div class="lab-head">
          <div class="lab-title">
            <span class="lab-number">{{ index + 1 }} / {{ CONCEPTS.length }}</span>
            <b>{{ current.name }}</b>
            <button class="lab-like" :class="{ on: liked.has(current.id) }" v-tooltip="'Нравится (клавиша L)'" @click="toggleLike(current.id)">★</button>
          </div>
          <div class="lab-spell">{{ current.group }} · задумано под {{ current.spell }}</div>
          <div class="lab-about">{{ current.about }}</div>
          <div v-if="current.levels" class="lab-controls">
            <span class="lab-label">Уровень</span>
            <button v-for="value in LEVELS" :key="value" v-tooltip="`Клавиша ${value}`" :class="{ active: level === value }" @click="level = value">Ур. {{ value }}</button>
            <span v-if="current.targets" class="lab-hint">целей: {{ targetsText }}</span>
            <button v-if="current.extras" v-tooltip="'Отложенные добавки — потом из них сделаем отдельные заклинания (клавиша E)'" :class="{ active: extras }" @click="extras = !extras">Доп. эффекты</button>
          </div>
          <div v-if="current.artifacts?.length" class="lab-controls">
            <span class="lab-label">Артефакт</span>
            <button :class="{ active: !artifact }" @click="artifact = null">Нет</button>
            <button v-for="item in current.artifacts" :key="item.key" v-tooltip="item.about" :class="{ active: artifact === item }" @click="artifact = item">{{ item.name }}</button>
          </div>
          <div v-if="artifact" class="lab-about">{{ artifact.about }}</div>
          <div class="lab-controls">
            <button v-tooltip="'Стрелка влево или вверх'" @click="step(-1)">◀</button>
            <button v-tooltip="'Пробел'" @click="replay()">↻ Ещё раз</button>
            <button v-tooltip="'Стрелка вправо или вниз'" @click="step(1)">▶</button>
            <button :class="{ active: loop }" @click="loop = !loop">Повтор</button>
            <button v-for="value in SPEEDS" :key="value" :class="{ active: speed === value }" @click="speed = value">x{{ value }}</button>
            <button v-tooltip="'Клавиша H'" @click="interfaceHidden = !interfaceHidden">{{ interfaceHidden ? 'Показать меню' : 'Скрыть меню' }}</button>
            <button v-tooltip="'Клавиша P'" @click="panelHidden = !panelHidden">{{ panelHidden ? 'Показать список' : 'Скрыть список' }}</button>
          </div>
          <div v-if="!panelHidden" class="lab-controls">
            <span class="lab-label">Арена</span>
            <button v-for="item in ARENAS" :key="item.title" :class="{ active: arena === item }" @click="arena = item">{{ item.title }}</button>
          </div>
        </div>

        <div v-if="!panelHidden" class="lab-list">
          <template v-for="group in groups" :key="group.title">
            <div class="lab-group">{{ group.title }}</div>
            <button
              v-for="item in group.items"
              :key="item.concept.id"
              class="lab-item"
              :class="{ active: item.index === index }"
              @click="select(item.index)"
            >
              <span class="lab-item-star" :class="{ on: liked.has(item.concept.id) }">★</span>
              <span class="lab-item-name">{{ item.concept.name }}</span>
              <span class="lab-item-spell">{{ item.concept.spell }}</span>
            </button>
          </template>
        </div>

        <div v-if="liked.size" class="lab-liked">
          <b>Понравились ({{ liked.size }}):</b> {{ likedNames }}
          <button @click="copyLiked()">{{ copied ? 'Скопировано' : 'Скопировать список' }}</button>
        </div>
      </div>
    </CanvasLayer>
    <div v-if="loading" class="lab-loading">Загрузка спрайтов…</div>
  </div>
</template>

<script lang="ts" setup>
import { computed, nextTick, onBeforeUnmount, onMounted, provide, reactive, ref, shallowRef, watch } from 'vue';
import { Container, Graphics } from 'pixi.js';
import CanvasComponent from '@/components/canvas/CanvasComponent.vue';
import CanvasLayer from '@/components/canvas/CanvasLayer.vue';
import CharacterComponent from '@/components/character-component.vue';
import { BattleUnit, createBattleState, resetBattleState, teamUnits } from '@/battle/BattleState';
import { BATTLE_FX_KEY, BattleFx } from '@/battle/BattleFx';
import { getUnitView } from '@/battle/UnitRegistry';
import { useInterface } from '@/composables/UseInterface';
import { CombatUnitInfo, HeroElement } from '@/battle/types';
import { PREVIEW_LOOK_RANK } from '@/components/collection/catalog';
import {
  CONCEPTS, conceptById, countFor, startConcept, stepConcept, stopConcept,
  type ArtifactDemo, type Concept, type ConceptRun, type ConceptUnit, type SpellLevel, type StageUnits,
} from '@/services/pixiFx/concepts';
import { tintOf } from '@/services/pixiFx/effect';
import type { PixiScene } from '@/services/pixiFx/PixiScene';

// Концепты анимаций: наброски эффектов на настоящей арене с юнитами. Листаются стрелками, понравившиеся
// отмечаются звёздочкой (хранятся в браузере) — из них потом собираются эффекты заклинаний.
// Сами концепты — services/pixiFx/concepts

const UNIT_SCALE = 0.8; // как на арене
const SPEEDS = [1, 0.5, 0.25];
const LEVELS: SpellLevel[] = [1, 2, 3];
// Пауза между повторами, секунды
const LOOP_GAP = 0.7;
const SPRITE_WAIT_MS = 15000;
const LIKED_KEY = 'fx-concepts-liked';
// Реестр вьюх юнитов общий с ареной и нагрузочным тестом: id не должны пересекаться
const FIRST_ID = 9301;

const arenaImages = require.context('@/assets/arenas', false, /\.(png|jpe?g|webp)$/);
const ARENAS = arenaImages.keys().map(path => ({
  title: path.replace(/^\.\/(arena_)?|\.\w+$/g, ''),
  url: arenaImages(path) as string,
}));

const ELEMENTS: HeroElement[] = ['Fire', 'Frost', 'Lightning', 'Nature', 'Radiance', 'Dark', 'Blood', 'Frost', 'Dark', 'Basic'];
const units: CombatUnitInfo[] = ELEMENTS.map((element, i) => ({
  entityId: FIRST_ID + i,
  name: i < 5 ? 'Союзник' : 'Враг',
  unitType: 'Dwarf',
  element,
  level: PREVIEW_LOOK_RANK,
  team: i < 5 ? 1 : 2,
  position: (i % 5) + 1,
  maxHp: 1200,
  hp: 1200,
  attack: 80,
  speed: 10,
  maxEnergy: 100,
  energy: 0,
  spells: [],
  immunities: [],
  power: { total: 0, stats: 0, level: 0, skills: 0, passives: 0 },
}));

const layerRef = ref<InstanceType<typeof CanvasLayer> | null>(null);
const state = createBattleState();
resetBattleState(state, { units, logs: [], winningTeam: 0, rounds: 0 });
const battleFx = new BattleFx(() => layerRef.value?.particleService ?? null, () => 1);
battleFx.flares = false;
provide(BATTLE_FX_KEY, battleFx);

const teamA = computed(() => teamUnits(state, 1));
const teamB = computed(() => teamUnits(state, 2));

const arena = shallowRef(ARENAS.find(item => item.title === 'red') ?? ARENAS[0]);
const bgStyle = computed(() => ({
  backgroundImage: `url('${arena.value.url}')`,
  backgroundRepeat: 'round',
}));

const loading = ref(true);
const index = ref(0);
const loop = ref(true);
const panelHidden = ref(false);
const speed = ref(1);
// Уровень заклинания для концептов с levels: true; по умолчанию максимальный
const level = ref<SpellLevel>(3);
// Доп. эффекты концепта (fx.extras) и выбранный артефакт заклинания
const extras = ref(false);
const artifact = shallowRef<ArtifactDemo | null>(null);
const current = computed(() => CONCEPTS[index.value]);
const targetsText = computed(() => current.value.targets?.map(v => (v === 'all' ? 'все' : v)).join(' / ') ?? '');
const groups = computed(() => {
  const result: { title: string; items: { concept: typeof CONCEPTS[number]; index: number }[] }[] = [];
  CONCEPTS.forEach((concept, i) => {
    let group = result.find(g => g.title === concept.group);
    if (!group) {
      group = { title: concept.group, items: [] };
      result.push(group);
    }
    group.items.push({ concept, index: i });
  });
  return result;
});

// ----- Понравившиеся -----
const readLiked = (): string[] => {
  try {
    return JSON.parse(localStorage.getItem(LIKED_KEY) ?? '[]');
  } catch {
    return [];
  }
};
const liked = reactive(new Set<string>(readLiked()));
const toggleLike = (id: string) => {
  if (liked.has(id)) liked.delete(id);
  else liked.add(id);
  localStorage.setItem(LIKED_KEY, JSON.stringify([...liked]));
};
const likedNames = computed(() => CONCEPTS.filter(c => liked.has(c.id)).map(c => `${c.name} (${c.spell})`).join(', '));
const copied = ref(false);
const copyLiked = async () => {
  await navigator.clipboard.writeText(CONCEPTS.filter(c => liked.has(c.id)).map(c => `${c.id} — ${c.name} (${c.spell})`).join('\n'));
  copied.value = true;
  setTimeout(() => (copied.value = false), 1500);
};

// ----- Сцена -----
let scene: PixiScene | null = null;
const back = new Container();
const front = new Container();
const flashLayer = new Graphics();
flashLayer.blendMode = 'add';
let flashState: { alpha: number; left: number; total: number } | null = null;
let shakeState: { power: number; left: number; total: number } | null = null;

const unitOf = (unit: BattleUnit): ConceptUnit => {
  const view = getUnitView(unit.id);
  const el = view?.el?.() ?? null;
  const point = (anchor: 'center' | 'feet' | 'head') => battleFx.point(el, anchor) ?? { x: 0, y: 0 };
  return {
    center: point('center'),
    feet: point('feet'),
    head: point('head'),
    hit: (crit, flash) => view?.hit?.('Magical', { crit, flash }),
    heal: () => view?.heal?.(),
    proc: true,
  };
};

// Сцена по механике: цели — передние враги (сколько бьёт заклинание на этом уровне), остальные — в стороне;
// лечимые — самые первые союзники после кастера (или весь отряд)
const makeStage = (target: PixiScene, concept: Concept, lvl: SpellLevel, demo: ArtifactDemo | null): StageUnits => {
  const allies = [...teamA.value].sort((a, b) => a.position - b.position).map(unitOf);
  const enemies = [...teamB.value].sort((a, b) => a.position - b.position).map(unitOf);
  const hit = countFor(concept.targets, lvl, enemies.length);
  const healRule = demo?.heals ?? concept.heals?.[lvl - 1] ?? 0;
  const healCount = healRule === 'all' ? allies.length : healRule;
  // Лечение одного — самый раненый, не кастер
  const healed = healCount >= allies.length ? allies : allies.slice(1, 1 + healCount);
  return {
    caster: allies[0],
    enemies: enemies.slice(0, hit),
    bystanders: enemies.slice(hit),
    allies,
    healed,
    width: target.size.width,
    height: target.size.height,
    cast: color => getUnitView(teamA.value.find(u => u.position === 1)?.id ?? -1)?.cast?.(color),
    shake: (power, seconds = 0.3) => (shakeState = { power, left: seconds, total: seconds }),
    flash: (css, alpha = 0.5, seconds = 0.3) => {
      flashLayer.clear().rect(0, 0, target.size.width, target.size.height).fill(tintOf(css));
      flashState = { alpha, left: seconds, total: seconds };
    },
  };
};

// Срабатывание артефакта отдельным кастом: свой концепт, свои цели (те же, соседи главной цели, случайная…)
const followUp = (target: PixiScene, main: StageUnits, demo: ArtifactDemo): { concept: Concept; stage: StageUnits; at: number; level: SpellLevel; origin: ConceptUnit | null } | null => {
  const then = demo.then;
  if (!then) return null;
  const ids = Array.isArray(then.concept) ? then.concept : [then.concept];
  const concept = conceptById(ids[Math.floor(Math.random() * ids.length)]);
  if (!concept) return null;
  const all = [...main.enemies, ...main.bystanders].sort((a, b) => a.feet.x - b.feet.x);
  const first = main.enemies[0];
  const near = all.filter(e => e !== first).sort((a, b) => Math.abs(a.feet.x - first.feet.x) - Math.abs(b.feet.x - first.feet.x));
  const pick = {
    same: main.enemies,
    neighbours: near.slice(0, 2),
    one: [near[0] ?? first],
    all,
    allies: [],
  }[then.targets];
  const healed = then.targets === 'allies' ? main.allies.slice(1, 1 + main.enemies.length) : [];
  const stage: StageUnits = { ...main, enemies: pick, bystanders: all.filter(e => !pick.includes(e)), healed };
  const origin = then.origin === 'target' ? first : then.origin === 'healed' ? (main.healed[0] ?? null) : null;
  return { concept, stage, at: then.after ?? 0.8, level: then.level, origin };
};

// ----- Проигрывание -----
// Основной каст и, если выбран артефакт со срабатыванием, его отдельный каст следом
let runs: ConceptRun[] = [];
let pending: { at: number; launch: () => ConceptRun } | null = null;
let elapsed = 0;
let gap = 0;

const stop = () => {
  runs.forEach(stopConcept);
  runs = [];
  pending = null;
  flashState = null;
  shakeState = null;
  flashLayer.visible = false;
  scene?.app.stage.position.set(0, 0);
};

const start = () => {
  stop();
  if (!scene || loading.value) return;
  const target = scene;
  const concept = current.value;
  const lvl = concept.levels ? level.value : 3;
  const demo = artifact.value && concept.artifacts?.includes(artifact.value) ? artifact.value : null;
  const stage = makeStage(target, concept, lvl, demo);
  const main = startConcept(concept, stage, { back, front }, {
    level: lvl,
    extras: extras.value,
    artifacts: demo && !demo.then ? [demo.kind] : [],
  });
  runs = [main];
  elapsed = 0;
  const next = demo ? followUp(target, stage, demo) : null;
  if (demo && next) {
    pending = {
      at: main.duration * next.at,
      launch: () => startConcept(next.concept, next.stage, { back, front }, {
        level: next.level,
        trigger: { kind: demo.kind, spell: concept.spell },
        origin: next.origin?.center ?? null,
      }),
    };
  }
};

const tick = (frame: number) => {
  if (!scene) return;
  if (flashState) {
    flashState.left -= frame;
    flashLayer.visible = flashState.left > 0;
    flashLayer.alpha = flashState.alpha * Math.max(flashState.left / flashState.total, 0);
    if (flashState.left <= 0) flashState = null;
  }
  if (shakeState) {
    shakeState.left -= frame;
    const power = shakeState.power * Math.max(shakeState.left / shakeState.total, 0);
    scene.app.stage.position.set((Math.random() - 0.5) * 2 * power, (Math.random() - 0.5) * 2 * power);
    if (shakeState.left <= 0) {
      shakeState = null;
      scene.app.stage.position.set(0, 0);
    }
  }
  if (!runs.length && !pending) {
    if (loop.value && !loading.value && gap > 0) {
      gap -= frame;
      if (gap <= 0) start();
    }
    return;
  }
  const dt = frame * speed.value;
  elapsed += dt;
  if (pending && elapsed >= pending.at) {
    runs.push(pending.launch());
    pending = null;
  }
  runs = runs.filter(run => !stepConcept(run, dt));
  if (!runs.length && !pending) {
    stop();
    gap = LOOP_GAP;
  }
};

const replay = () => start();
const select = (value: number) => {
  index.value = (value + CONCEPTS.length) % CONCEPTS.length;
  start();
};
const step = (delta: number) => select(index.value + delta);
// Сменили уровень — показать концепт заново на нём
watch(level, () => {
  if (current.value.levels) replay();
});
watch([extras, artifact], () => replay());
// Другой концепт — артефакт сбрасывается, если у нового его нет
watch(index, () => {
  if (artifact.value && !current.value.artifacts?.includes(artifact.value)) artifact.value = null;
});

watch(loop, on => {
  if (on && !runs.length) start();
});

const { interfaceHidden } = useInterface();
const onKey = (event: KeyboardEvent) => {
  if (event.ctrlKey || event.metaKey || event.altKey) return;
  const actions: Record<string, () => void> = {
    ArrowRight: () => step(1),
    ArrowDown: () => step(1),
    ArrowLeft: () => step(-1),
    ArrowUp: () => step(-1),
    Space: () => replay(),
    KeyL: () => toggleLike(current.value.id),
    KeyH: () => (interfaceHidden.value = !interfaceHidden.value),
    KeyP: () => (panelHidden.value = !panelHidden.value),
    Digit1: () => (level.value = 1),
    Digit2: () => (level.value = 2),
    Digit3: () => (level.value = 3),
    KeyE: () => (extras.value = !extras.value),
  };
  const action = actions[event.code];
  if (!action) return;
  event.preventDefault();
  action();
};

let active = true;
const wait = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

onMounted(async () => {
  window.addEventListener('keydown', onKey);
  await nextTick();
  // Сцена создаётся лениво — ждём её и спрайты юнитов
  const started = performance.now();
  while (active && !layerRef.value?.particleService.pixiScene && performance.now() - started < SPRITE_WAIT_MS) await wait(50);
  scene = layerRef.value?.particleService.pixiScene ?? null;
  if (!active || !scene) return;
  scene.background.addChild(back);
  scene.foreground.addChild(front, flashLayer);
  flashLayer.visible = false;
  scene.addTicker(tick);
  await Promise.race([
    Promise.all([...teamA.value, ...teamB.value].map(unit => getUnitView(unit.id)?.ready?.())),
    wait(SPRITE_WAIT_MS),
  ]);
  await wait(300);
  if (!active) return;
  loading.value = false;
  start();
});

onBeforeUnmount(() => {
  active = false;
  window.removeEventListener('keydown', onKey);
  stop();
  scene?.removeTicker(tick);
  // Сцену удаляет слой; свои контейнеры убираем сами, если она ещё жива
  for (const node of [back, front, flashLayer]) {
    if (!node.destroyed) node.destroy({ children: true });
  }
  interfaceHidden.value = false;
});
</script>

<style scoped>
.lab {
  width: var(--stage-w);
  height: var(--stage-h);
  position: fixed;
}

.lab-bg {
  position: absolute;
  inset: 0;
  z-index: -1;
}

/* Расстановка как на арене */
.grid-item {
  position: absolute;
  --squad-shift: 5%;
}
.grid { isolation: isolate; --unit-overlay-z: 100; }
.grid-item-left-1 { top: 50%; left: calc(35% - var(--squad-shift)); --depth: 50; }
.grid-item-left-2 { top: 65%; left: calc(30% - var(--squad-shift)); --depth: 66; }
.grid-item-left-3 { top: 45%; left: calc(25% - var(--squad-shift)); --depth: 45; }
.grid-item-left-4 { top: 55%; left: calc(20% - var(--squad-shift)); --depth: 55; }
.grid-item-left-5 { top: 65%; left: calc(15% - var(--squad-shift)); --depth: 65; }
.grid-item-right-1 { top: 50%; right: calc(35% - var(--squad-shift)); --depth: 50; }
.grid-item-right-2 { top: 65%; right: calc(30% - var(--squad-shift)); --depth: 66; }
.grid-item-right-3 { top: 45%; right: calc(25% - var(--squad-shift)); --depth: 45; }
.grid-item-right-4 { top: 55%; right: calc(20% - var(--squad-shift)); --depth: 55; }
.grid-item-right-5 { top: 65%; right: calc(15% - var(--squad-shift)); --depth: 65; }

.lab-panel {
  position: absolute;
  top: 84px;
  left: 16px;
  right: 16px;
  z-index: 10000;
  display: flex;
  align-items: flex-start;
  gap: 8px;
  color: #e2e8f0;
  font-size: 13px;
  pointer-events: none;
}
.lab-panel > * {
  pointer-events: auto;
}

.lab-head,
.lab-list,
.lab-liked {
  background: rgba(14, 19, 34, 0.85);
  border: 1px solid rgba(148, 163, 184, 0.25);
  border-radius: 8px;
  padding: 10px 12px;
}
.lab-head {
  width: 400px;
  flex: none;
}
.lab-liked {
  width: 240px;
  flex: none;
}

.lab-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 17px;
}
.lab-number {
  color: #94a3b8;
  font-size: 12px;
}
.lab-spell {
  color: #fbbf24;
  margin-top: 2px;
}
.lab-about {
  color: #cbd5e1;
  margin: 4px 0 8px;
  min-height: 34px;
}

.lab-controls {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 4px;
  margin-top: 4px;
}
.lab-label {
  color: #94a3b8;
  margin-right: 4px;
}

.lab-hint {
  color: #94a3b8;
  font-size: 12px;
  margin: 0 6px;
}

.lab button {
  background: rgba(255, 255, 255, 0.08);
  border: 1px solid rgba(148, 163, 184, 0.3);
  border-radius: 5px;
  color: inherit;
  padding: 4px 8px;
  cursor: pointer;
  font-size: 12px;
}
.lab button:hover {
  background: rgba(255, 255, 255, 0.16);
}
.lab button.active {
  background: rgba(251, 191, 36, 0.25);
  border-color: #fbbf24;
}

.lab-like {
  margin-left: auto;
  font-size: 18px !important;
  line-height: 1;
  color: #64748b !important;
}
.lab-like.on,
.lab-item-star.on {
  color: #fbbf24 !important;
}

.lab-list {
  flex: 1;
  min-width: 0;
  columns: 230px;
  column-gap: 12px;
  max-height: calc(var(--stage-h) * 0.36);
  overflow-y: auto;
}
.lab-group {
  color: #94a3b8;
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  margin: 6px 0 2px;
  break-after: avoid;
}
.lab-group:first-child {
  margin-top: 0;
}
.lab .lab-item {
  display: flex;
  width: 100%;
  align-items: center;
  gap: 8px;
  text-align: left;
  break-inside: avoid;
  border-color: transparent;
  background: transparent;
  padding: 4px 6px;
  font-size: 13px;
}
.lab-item-star {
  color: transparent;
}
.lab-item-name {
  flex: 1;
}
.lab-item-spell {
  color: #94a3b8;
  font-size: 11px;
}

.lab-liked {
  display: flex;
  flex-direction: column;
  gap: 6px;
  color: #cbd5e1;
}
.lab-liked button {
  align-self: flex-start;
}

.lab-loading {
  position: absolute;
  inset: 0;
  z-index: 10001;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #e2e8f0;
  font-size: 18px;
  background: radial-gradient(ellipse at center, #1a2232 0%, #0a0f18 75%);
}
</style>
