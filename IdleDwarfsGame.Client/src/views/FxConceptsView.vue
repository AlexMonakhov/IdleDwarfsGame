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
          <div class="lab-controls lab-sections">
            <button v-for="item in SECTIONS" :key="item.kind" :class="{ active: section === item.kind }" @click="section = item.kind">
              {{ item.title }} <span class="lab-hint">{{ countOf(item.kind) }}</span>
            </button>
          </div>
          <div class="lab-title">
            <span class="lab-number">{{ index + 1 }} / {{ list.length }}</span>
            <b>{{ current.name }}</b>
            <button class="lab-like" :class="{ on: liked.has(current.id) }" v-tooltip="'Нравится (клавиша L)'" @click="toggleLike(current.id)">★</button>
          </div>
          <div class="lab-spell">{{ current.group }} · задумано под {{ current.spell }}</div>
          <div class="lab-about">{{ current.about }}</div>
          <div v-if="currentConcept?.levels || currentLook?.levels" class="lab-controls">
            <span class="lab-label">Уровень</span>
            <button v-for="value in LEVELS" :key="value" v-tooltip="`Клавиша ${value}`" :class="{ active: level === value }" @click="level = value">Ур. {{ value }}</button>
            <span v-if="currentConcept?.targets" class="lab-hint">целей: {{ targetsText }}</span>
            <button v-if="currentConcept?.extras" v-tooltip="'Отложенные добавки — потом из них сделаем отдельные заклинания (клавиша E)'" :class="{ active: extras }" @click="extras = !extras">Доп. эффекты</button>
          </div>
          <div v-if="currentConcept?.artifacts?.length" class="lab-controls">
            <span class="lab-label">Артефакт</span>
            <button :class="{ active: !artifact }" @click="artifact = null">Нет</button>
            <button v-for="item in currentConcept.artifacts" :key="item.key" v-tooltip="item.about" :class="{ active: artifact === item }" @click="artifact = item">{{ item.name }}</button>
          </div>
          <div v-if="artifact" class="lab-about">{{ artifact.about }}</div>

          <template v-if="currentLook">
            <div class="lab-controls">
              <button v-tooltip="'Клавиша T или пробел: тик статуса / пульс ауры'" @click="triggerAll()">⚡ Сработать</button>
              <button v-tooltip="'Срабатывать само каждые пару секунд'" :class="{ active: autoTrigger }" @click="autoTrigger = !autoTrigger">Авто</button>
              <button v-tooltip="'Клавиша A: оставить висеть и наложить следующий'" :class="{ active: stack.includes(currentLook.id) }" @click="toggleStack(currentLook.id)">
                {{ stack.includes(currentLook.id) ? '− Из набора' : '+ В набор' }}
              </button>
            </div>
            <div v-if="section === 'status'" class="lab-controls">
              <span class="lab-label">Целей</span>
              <button v-for="value in TARGET_COUNTS" :key="value" :class="{ active: lookTargets === value }" @click="lookTargets = value">{{ value === 5 ? 'все' : value }}</button>
            </div>
            <div v-if="section === 'aura'" class="lab-controls">
              <span class="lab-label">Носители</span>
              <button :class="{ active: !allBearers }" @click="allBearers = false">Один</button>
              <button :class="{ active: allBearers }" @click="allBearers = true">Весь отряд</button>
            </div>
            <div v-if="section === 'aura'" class="lab-controls">
              <span class="lab-label">Волна</span>
              <button :class="{ active: !auraWave }" @click="auraWave = null">Без волны</button>
              <button v-for="wave in currentWaves" :key="wave.id" v-tooltip="wave.about" :class="{ active: auraWave === wave }" @click="auraWave = wave">{{ wave.name }}</button>
            </div>
          </template>
          <div v-if="isLooks && stackLooks.length" class="lab-controls lab-stack">
            <span class="lab-label">Набор</span>
            <button v-for="look in stackLooks" :key="look.id" v-tooltip="'Снять'" @click="toggleStack(look.id)">{{ look.name }} ×</button>
            <button v-tooltip="'Клавиша C'" @click="stack = []">Снять все</button>
          </div>

          <div class="lab-controls">
            <button v-tooltip="'Стрелка влево или вверх'" @click="step(-1)">◀</button>
            <button v-if="!isLooks" v-tooltip="'Пробел'" @click="replay()">↻ Ещё раз</button>
            <button v-tooltip="'Стрелка вправо или вниз'" @click="step(1)">▶</button>
            <button v-if="!isLooks" :class="{ active: loop }" @click="loop = !loop">Повтор</button>
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
              :key="item.entry.id"
              class="lab-item"
              :class="{ active: item.index === index }"
              @click="select(item.index)"
            >
              <span class="lab-item-star" :class="{ on: liked.has(item.entry.id) }">★</span>
              <span class="lab-item-name">{{ item.entry.name }}</span>
              <span v-if="isLooks" class="lab-item-add" :class="{ on: stack.includes(item.entry.id) }" v-tooltip="'В набор: висеть вместе с другими'" @click.stop="toggleStack(item.entry.id)">+</span>
              <span v-else class="lab-item-spell">{{ item.entry.spell }}</span>
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
  CONCEPTS, LOOKS, attachLook, conceptById, countFor, disposeLook, removeLook, startConcept, stepConcept, stepLook, stopConcept, triggerLook,
  type ArtifactDemo, type Concept, type ConceptRun, type ConceptUnit, type Look, type LookInstance, type SpellLevel, type StageUnits,
} from '@/services/pixiFx/concepts';
import { tintOf } from '@/services/pixiFx/effect';
import type { PixiScene } from '@/services/pixiFx/PixiScene';

// Концепты анимаций: наброски эффектов на настоящей арене с юнитами. Листаются стрелками, понравившиеся
// отмечаются звёздочкой (хранятся в браузере) — из них потом собираются эффекты.
// Заклинания — разовые сцены (концепты). Ауры и эффекты — облики: висят на юнитах, пока их не снимут, и срабатывают
// по кнопке или сами; облики можно складывать в набор и смотреть, как несколько эффектов висят разом.
// Сами концепты и облики — services/pixiFx/concepts

type Section = 'spell' | 'aura' | 'status';
type Entry = Concept | Look;

const UNIT_SCALE = 0.8; // как на арене
const SPEEDS = [1, 0.5, 0.25];
const LEVELS: SpellLevel[] = [1, 2, 3];
const TARGET_COUNTS = [1, 2, 5] as const;
// Пауза между повторами, секунды
const LOOP_GAP = 0.7;
// Авто-срабатывание обликов: раз в столько секунд; статусы одного юнита тикают друг за другом через STAGGER
const AUTO_PERIOD: Record<Section, number> = { spell: 0, aura: 3.4, status: 2.4 };
const TRIGGER_STAGGER = 0.25;
const SPRITE_WAIT_MS = 15000;
const LIKED_KEY = 'fx-concepts-liked';
const SECTION_KEY = 'fx-concepts-section';
const SECTIONS: { kind: Section; title: string }[] = [
  { kind: 'spell', title: 'Заклинания' },
  { kind: 'aura', title: 'Ауры' },
  { kind: 'status', title: 'Эффекты' },
];
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
const byPosition = (team: BattleUnit[]) => [...team].sort((a, b) => a.position - b.position);

const arena = shallowRef(ARENAS.find(item => item.title === 'red') ?? ARENAS[0]);
const bgStyle = computed(() => ({
  backgroundImage: `url('${arena.value.url}')`,
  backgroundRepeat: 'round',
}));

// ----- Разделы и список -----
const SPELL_CONCEPTS = CONCEPTS.filter(c => (c.kind ?? 'spell') === 'spell');
const AURA_WAVES = CONCEPTS.filter(c => c.kind === 'aura');
const entriesOf = (kind: Section): Entry[] => (kind === 'spell' ? SPELL_CONCEPTS : LOOKS.filter(look => look.kind === kind));
const countOf = (kind: Section) => entriesOf(kind).length;
const isLook = (entry: Entry | null | undefined): entry is Look => !!entry && 'kind' in entry && (entry.kind === 'aura' || entry.kind === 'status') && !('duration' in entry);

const readSection = (): Section => {
  try {
    const saved = localStorage.getItem(SECTION_KEY);
    return SECTIONS.some(item => item.kind === saved) ? (saved as Section) : 'spell';
  } catch {
    return 'spell';
  }
};
const loading = ref(true);
const section = ref<Section>(readSection());
const isLooks = computed(() => section.value !== 'spell');
const list = computed(() => entriesOf(section.value));
const index = ref(0);
const loop = ref(true);
const panelHidden = ref(false);
const speed = ref(1);
// Уровень заклинания или ауры; по умолчанию максимальный
const level = ref<SpellLevel>(3);
// Доп. эффекты концепта (fx.extras) и выбранный артефакт заклинания
const extras = ref(false);
const artifact = shallowRef<ArtifactDemo | null>(null);
const current = computed<Entry>(() => list.value[index.value] ?? list.value[0]);
const currentConcept = computed(() => (isLook(current.value) ? null : current.value));
const currentLook = computed(() => (isLook(current.value) ? current.value : null));
const targetsText = computed(() => currentConcept.value?.targets?.map(v => (v === 'all' ? 'все' : v)).join(' / ') ?? '');
const groups = computed(() => {
  const result: { title: string; items: { entry: Entry; index: number }[] }[] = [];
  list.value.forEach((entry, i) => {
    let group = result.find(g => g.title === entry.group);
    if (!group) {
      group = { title: entry.group, items: [] };
      result.push(group);
    }
    group.items.push({ entry, index: i });
  });
  return result;
});

// ----- Облики: набор, цели, волна ауры -----
// Набор — облики, которые висят вместе с текущим (на одном юните может быть несколько статусов)
const stack = ref<string[]>([]);
const stackLooks = computed(() => stack.value.map(id => LOOKS.find(look => look.id === id)).filter((look): look is Look => !!look));
const toggleStack = (id: string) => {
  stack.value = stack.value.includes(id) ? stack.value.filter(item => item !== id) : [...stack.value, id];
};
const shownLooks = computed(() => {
  const result = currentLook.value ? [currentLook.value] : [];
  for (const look of stackLooks.value) if (!result.includes(look)) result.push(look);
  return result;
});
const lookTargets = ref<typeof TARGET_COUNTS[number]>(1);
const allBearers = ref(false);
const autoTrigger = ref(true);
const wavesFor = (look: Look) => AURA_WAVES.filter(wave => wave.spell === look.spell);
const currentWaves = computed(() => (currentLook.value?.kind === 'aura' ? wavesFor(currentLook.value) : []));
// Волна, которую аура пускает при срабатывании (концепт из auras.ts); null — только облик вздрагивает
const auraWave = shallowRef<Concept | null>(null);

// ----- Понравившиеся -----
const ALL: Entry[] = [...CONCEPTS, ...LOOKS];
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
const likedNames = computed(() => ALL.filter(c => liked.has(c.id)).map(c => `${c.name} (${c.spell})`).join(', '));
const copied = ref(false);
const copyLiked = async () => {
  await navigator.clipboard.writeText(ALL.filter(c => liked.has(c.id)).map(c => `${c.id} — ${c.name} (${c.spell})`).join('\n'));
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

const castFor = (id: number, color?: string) => getUnitView(id)?.cast?.(color);

// Вся арена: все враги — цели, кастер — первый союзник
const baseStage = (target: PixiScene): StageUnits => {
  const allies = byPosition(teamA.value).map(unitOf);
  const enemies = byPosition(teamB.value).map(unitOf);
  return {
    caster: allies[0],
    enemies,
    bystanders: [],
    allies,
    healed: [],
    width: target.size.width,
    height: target.size.height,
    cast: color => castFor(teamA.value.find(u => u.position === 1)?.id ?? -1, color),
    shake: (power, seconds = 0.3) => (shakeState = { power, left: seconds, total: seconds }),
    flash: (css, alpha = 0.5, seconds = 0.3) => {
      flashLayer.clear().rect(0, 0, target.size.width, target.size.height).fill(tintOf(css));
      flashState = { alpha, left: seconds, total: seconds };
    },
  };
};

// Сцена по механике: цели — передние враги (сколько бьёт заклинание на этом уровне), остальные — в стороне;
// лечимые — самые первые союзники после кастера (или весь отряд)
const makeStage = (target: PixiScene, concept: Concept, lvl: SpellLevel, demo: ArtifactDemo | null): StageUnits => {
  const base = baseStage(target);
  const hit = countFor(concept.targets, lvl, base.enemies.length);
  const healRule = demo?.heals ?? concept.heals?.[lvl - 1] ?? 0;
  const healCount = healRule === 'all' ? base.allies.length : healRule;
  // Лечение одного — самый раненый, не кастер
  const healed = healCount >= base.allies.length ? base.allies : base.allies.slice(1, 1 + healCount);
  return { ...base, enemies: base.enemies.slice(0, hit), bystanders: base.enemies.slice(hit), healed };
};

// Срабатывание артефакта отдельным кастом: свой концепт, свои цели (те же, соседи главной цели, случайная…)
const followUp = (main: StageUnits, demo: ArtifactDemo): { concept: Concept; stage: StageUnits; at: number; level: SpellLevel; origin: ConceptUnit | null } | null => {
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

// ----- Проигрывание концептов -----
// Основной каст и, если выбран артефакт со срабатыванием, его отдельный каст следом; здесь же волны аур
let runs: ConceptRun[] = [];
let pending: { at: number; launch: () => ConceptRun } | null = null;
let elapsed = 0;
let gap = 0;

const stopRuns = () => {
  runs.forEach(stopConcept);
  runs = [];
  pending = null;
  flashState = null;
  shakeState = null;
  flashLayer.visible = false;
  scene?.app.stage.position.set(0, 0);
};

// ----- Облики на юнитах -----
interface LookEntry {
  key: string;
  look: Look;
  unit: BattleUnit;
  inst: LookInstance;
}
let looks: LookEntry[] = [];
let queued: { at: number; entry: LookEntry }[] = [];
let lookClock = 0;
let nextAuto = 0;

// На ком висит облик: аура — на носителе (первый союзник или весь отряд), статус — на передних врагах
// или, если это лечение, на союзниках после первого
const unitsFor = (look: Look): BattleUnit[] => {
  const allies = byPosition(teamA.value);
  if (look.kind === 'aura') return allBearers.value ? allies : allies.slice(0, 1);
  const n = lookTargets.value;
  if (look.side === 'allies') return n >= allies.length ? allies : allies.slice(1, 1 + n);
  return byPosition(teamB.value).slice(0, n);
};
const lookKey = (look: Look, unit: BattleUnit) => `${look.id}|${unit.id}|${look.levels ? level.value : 0}`;
const alive = (entry: LookEntry) => entry.inst.removedAt === null;

// Привести облики на сцене к нужным: лишние плавно гаснут, новые появляются (уже висящие не трогаем)
const syncLooks = () => {
  if (!scene || loading.value) return;
  const want = new Map<string, { look: Look; unit: BattleUnit }>();
  if (isLooks.value) for (const look of shownLooks.value) for (const unit of unitsFor(look)) want.set(lookKey(look, unit), { look, unit });
  looks.filter(entry => alive(entry) && !want.has(entry.key)).forEach(entry => removeLook(entry.inst));
  const base = baseStage(scene);
  let added = false;
  for (const [key, item] of want) {
    if (looks.some(entry => entry.key === key && alive(entry))) continue;
    looks.push({ key, ...item, inst: attachLook(item.look, unitOf(item.unit), base, { back, front }, level.value) });
    added = true;
  }
  // Новый облик сначала просто висит, срабатывание — чуть позже
  if (added) nextAuto = lookClock + 1.2;
};

const clearLooks = () => {
  looks.forEach(entry => disposeLook(entry.inst));
  looks = [];
  queued = [];
};

// Сработать: статусы одного юнита — друг за другом, как пачка тиков в начале его хода; носители аур — с разбегом
const triggerAll = () => {
  const byUnit = new Map<number, LookEntry[]>();
  for (const entry of looks.filter(alive)) byUnit.set(entry.unit.id, [...(byUnit.get(entry.unit.id) ?? []), entry]);
  [...byUnit.values()].forEach((group, u) => group.forEach((entry, i) => queued.push({ at: lookClock + i * TRIGGER_STAGGER + (entry.look.kind === 'aura' ? u * 0.12 : 0), entry })));
  nextAuto = lookClock + AUTO_PERIOD[section.value];
};

const fire = (entry: LookEntry) => {
  if (!alive(entry)) return;
  triggerLook(entry.inst);
  if (entry.look.kind !== 'aura' || !scene) return;
  const wave = entry.look === currentLook.value ? auraWave.value : wavesFor(entry.look)[0];
  if (!wave) return;
  const base = baseStage(scene);
  const stage: StageUnits = { ...base, caster: unitOf(entry.unit), healed: wave.heals ? base.allies : [], cast: color => castFor(entry.unit.id, color) };
  runs.push(startConcept(wave, stage, { back, front }, { level: level.value }));
};

const start = () => {
  stopRuns();
  if (!scene || loading.value) return;
  if (isLooks.value) {
    syncLooks();
    return;
  }
  const target = scene;
  const concept = currentConcept.value;
  if (!concept) return;
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
  const next = demo ? followUp(stage, demo) : null;
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

const tickLooks = (dt: number) => {
  lookClock += dt;
  const due = queued.filter(item => item.at <= lookClock);
  queued = queued.filter(item => item.at > lookClock);
  due.forEach(item => fire(item.entry));
  if (isLooks.value && autoTrigger.value && lookClock >= nextAuto) triggerAll();
  looks = looks.filter(entry => !stepLook(entry.inst, dt));
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
  const dt = frame * speed.value;
  tickLooks(dt);
  if (isLooks.value) {
    runs = runs.filter(run => !stepConcept(run, dt));
    return;
  }
  if (!runs.length && !pending) {
    if (loop.value && !loading.value && gap > 0) {
      gap -= frame;
      if (gap <= 0) start();
    }
    return;
  }
  elapsed += dt;
  if (pending && elapsed >= pending.at) {
    runs.push(pending.launch());
    pending = null;
  }
  runs = runs.filter(run => !stepConcept(run, dt));
  if (!runs.length && !pending) {
    stopRuns();
    gap = LOOP_GAP;
  }
};

const replay = () => (isLooks.value ? triggerAll() : start());
const select = (value: number) => {
  index.value = (value + list.value.length) % list.value.length;
  start();
};
const step = (delta: number) => select(index.value + delta);
// Сменили уровень — концепт заново, облики аур пересоздаются на новом уровне
watch(level, () => {
  if (isLooks.value) syncLooks();
  else if (currentConcept.value?.levels) replay();
});
watch([extras, artifact], () => {
  if (!isLooks.value) replay();
});
watch([stack, lookTargets, allBearers], () => syncLooks());
// Другой раздел — с первого; в заклинаниях облики не висят
watch(section, value => {
  try {
    localStorage.setItem(SECTION_KEY, value);
  } catch {
    // без сохранения раздела
  }
  if (value === 'spell') clearLooks();
  select(0);
});
// Другой концепт — артефакт сбрасывается, если у нового его нет; у ауры — её первая волна
watch(current, () => {
  if (artifact.value && !currentConcept.value?.artifacts?.includes(artifact.value)) artifact.value = null;
  auraWave.value = currentWaves.value[0] ?? null;
}, { immediate: true });

watch(loop, on => {
  if (on && !runs.length && !isLooks.value) start();
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
    KeyT: () => triggerAll(),
    KeyA: () => currentLook.value && toggleStack(currentLook.value.id),
    KeyC: () => (stack.value = []),
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
  stopRuns();
  clearLooks();
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
.lab-sections {
  margin: 0 0 8px;
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
.lab-item-add {
  color: #64748b;
  font-weight: bold;
  padding: 0 4px;
  border-radius: 4px;
}
.lab-item-add:hover {
  color: #e2e8f0;
  background: rgba(255, 255, 255, 0.12);
}
.lab-item-add.on {
  color: #0e1322;
  background: #fbbf24;
}
.lab-stack button {
  border-color: rgba(251, 191, 36, 0.5);
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
