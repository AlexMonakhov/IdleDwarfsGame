<template>
  <div class="stress">
    <div class="stress-bg" :style="bgStyle"></div>
    <CanvasLayer ref="layerRef">
      <div class="grid">
        <template v-for="unit in teamA" :key="unit.id">
          <CanvasComponent :class="['grid-item', `grid-item-left-${unit.position}`]" style="position: absolute;">
            <CharacterComponent :unit="unit" :scale="UNIT_SCALE" />
          </CanvasComponent>
        </template>
        <template v-for="unit in teamB" :key="unit.id">
          <CanvasComponent :class="['grid-item', `grid-item-right-${unit.position}`]" style="position: absolute;">
            <CharacterComponent :unit="unit" :scale="UNIT_SCALE" rotate />
          </CanvasComponent>
        </template>
      </div>

      <div class="stress-panel">
        <div class="stress-fps" :class="{ low: fps < peakFps * 0.8 }">
          FPS {{ fps }} · худший кадр {{ worstFrame.toFixed(1) }} мс · минимум FPS {{ minFps ?? '—' }}
        </div>
        <div class="stress-controls">
          <button title="Клавиша H" @click="interfaceHidden = !interfaceHidden">{{ interfaceHidden ? 'Показать интерфейс' : 'Скрыть интерфейс' }}</button>
          <button :class="{ active: manual }" @click="manual = !manual">{{ manual ? 'Ручной режим' : 'Автобой' }}</button>
          <button v-if="!manual" @click="paused = !paused">{{ paused ? 'Продолжить' : 'Пауза' }}</button>
          <button v-for="value in [1, 2, 4]" :key="value" :class="{ active: speed === value }" @click="speed = value">x{{ value }}</button>
          <button :class="{ active: aurasOn }" @click="aurasOn = !aurasOn">Ауры</button>
          <button :class="{ active: statusesOn }" @click="statusesOn = !statusesOn">Статусы</button>
          <button :class="{ active: flaresOn }" @click="flaresOn = !flaresOn">Вспышки</button>
          <button @click="clearStatuses()">Снять статусы</button>
          <button @click="minFps = null">Сбросить минимум</button>
        </div>

        <!-- Время эффектов только JS: растр на видеокарте не виден — поэтому тип можно скрыть и смотреть на FPS -->
        <div class="stress-profile">
          <div class="stress-profile-head">Эффекты: {{ effectsMs.toFixed(2) }} мс JS за кадр · клик — скрыть тип</div>
          <button
            v-for="row in profileRows"
            :key="row.type"
            class="stress-profile-row"
            :class="{ off: row.hidden }"
            @click="toggleType(row.type)"
          >
            <span>{{ row.type }}</span>
            <span>{{ row.emitters }}</span>
            <span>{{ row.ms.toFixed(2) }} мс</span>
          </button>
        </div>
      </div>

      <!-- Ручной режим: автобой стоит, заклинание и его уровень выбираются здесь — кастует дворф этой строки -->
      <div v-if="manual" class="stress-spells">
        <template v-for="team in teams" :key="team.title">
          <div class="stress-spells-team">{{ team.title }}</div>
          <div
            v-for="unit in team.units"
            :key="unit.id"
            class="stress-spells-row"
            @mouseenter="highlight(unit, true)"
            @mouseleave="highlight(unit, false)"
          >
            <span class="stress-spells-unit">{{ unit.position }} · {{ unit.element }}</span>
            <!-- Заклинание и кнопки его уровней: цифра — каст этого уровня -->
            <div
              v-for="sample in fighterOf(unit).spells"
              :key="sample.key"
              class="stress-spell"
              :style="{ '--spell-color': spellColor(sample) }"
            >
              <span>{{ sample.name }}</span>
              <button
                v-for="level in SPELL_LEVELS"
                :key="level"
                :disabled="casting.has(unit.id)"
                :title="`${sample.name}, уровень ${level}`"
                @click="castSpell(unit, sample, level)"
              >{{ level }}</button>
            </div>
            <button
              v-if="aurasOn"
              class="stress-spells-aura"
              :disabled="casting.has(unit.id)"
              title="Волна ауры"
              @click="pulseAura(unit)"
            >✦</button>
          </div>
        </template>
      </div>
    </CanvasLayer>

    <div v-if="loading" class="stress-loading">Загрузка спрайтов…</div>
  </div>
</template>

<script lang="ts" setup>
import { computed, nextTick, onBeforeUnmount, onMounted, provide, reactive, ref, watch } from 'vue';
import CanvasComponent from '@/components/canvas/CanvasComponent.vue';
import CanvasLayer from '@/components/canvas/CanvasLayer.vue';
import CharacterComponent from '@/components/character-component.vue';
import { arenaBackground, arenaBackgroundFilter } from '@/battle/arenaBackgrounds';
import { BattleUnit, FloatingText, auraEffectOf, createBattleState, resetBattleState, teamUnits } from '@/battle/BattleState';
import { BATTLE_FX_KEY, BattleFx } from '@/battle/BattleFx';
import { STATUS_VISUALS, resolveAuraVisual, resolveSpellVisual } from '@/battle/FxRegistry';
import { getUnitView } from '@/battle/UnitRegistry';
import { useInterface } from '@/composables/UseInterface';
import { EffectType } from '@/services/particleService/types/interfaces/canvas-interfaces';
import { CombatUnitInfo, HeroElement } from '@/battle/types';
import { AURA_SAMPLES, AuraSample, HERO_KINDS, PREVIEW_RANK, SPELL_LEVELS, SPELLS, SpellSample } from '@/components/collection/catalog';

// Нагрузочный тест арены: 10 дворфов, у каждого аура и одно заклинание 3-го уровня.
// Автобой: ходят по очереди, как в бою — на своём ходу носитель пускает волну ауры и кастует, и так по кругу без конца.
// Ручной режим: автобой стоит, заклинание (любое из стихии дворфа, любого уровня) выбирается в списке справа.
// Эффекты те же, что в бою (BattleFx и вьюхи юнитов), паузы — как в BattlePlayer

const UNIT_SCALE = 0.8; // как на арене
const SPELL_LEVEL = 3;
const ATTACK = 80;
const MAX_HP = 1200;
const BACKGROUND = 'arena_blue_crystals';

// Мс при скорости x1
const TURN_START = 300;
const AURA_RECOVER = 450;
const ACTION_GAP = 400;
const CAST_WINDUP = 300;
const CAST_RECOVER = 400;
const AURA_WAVE = 300;
const FLOATING_TEXT = 1100;
// Дольше спрайты не ждём
const SPRITE_WAIT_MS = 15000;

// Реестр вьюх юнитов общий с ареной и коллекцией: id не должны пересекаться с их id
const FIRST_ID = 9101;

interface Fighter {
  element: HeroElement;
  // Заклинание автобоя
  spell: SpellSample;
  // Все заклинания стихии — для ручного режима
  spells: SpellSample[];
  aura: AuraSample;
}

const spellsOf = (element: HeroElement) => HERO_KINDS.find(kind => kind.element === element)?.spells ?? [];

// Первые пять — левая команда, остальные — правая
const ROSTER: Fighter[] = ([
  { element: 'Fire', spell: SPELLS.meteor },
  { element: 'Frost', spell: SPELLS.blizzard },
  { element: 'Lightning', spell: SPELLS.chainLightning },
  { element: 'Nature', spell: SPELLS.healingRune },
  { element: 'Radiance', spell: SPELLS.radiantNova },
  { element: 'Fire', spell: SPELLS.flameWave },
  { element: 'Frost', spell: SPELLS.glacialPrison },
  { element: 'Lightning', spell: SPELLS.thunderclap },
  { element: 'Dark', spell: SPELLS.hex },
  { element: 'Radiance', spell: SPELLS.dawnlight },
] as Pick<Fighter, 'element' | 'spell'>[]).map((fighter, i) => ({
  ...fighter,
  spells: spellsOf(fighter.element),
  aura: AURA_SAMPLES[i % AURA_SAMPLES.length],
}));

const fighterOf = (unit: BattleUnit) => ROSTER[unit.id - FIRST_ID];

const units: CombatUnitInfo[] = ROSTER.map((fighter, i) => ({
  entityId: FIRST_ID + i,
  name: fighter.spell.name,
  unitType: 'Dwarf',
  element: fighter.element,
  level: PREVIEW_RANK,
  team: i < 5 ? 1 : 2,
  position: (i % 5) + 1,
  maxHp: MAX_HP,
  hp: MAX_HP,
  attack: ATTACK,
  speed: 10,
  maxEnergy: 100,
  energy: 100,
  spells: fighter.spells.map(spell => spell.name),
  immunities: [],
  auraEffect: fighter.aura.effect,
  auraTarget: fighter.aura.target,
  power: { total: 0, stats: 0, level: 0, skills: 0, passives: 0 },
}));

const layerRef = ref<InstanceType<typeof CanvasLayer> | null>(null);
const speed = ref(1);
const paused = ref(false);
// Ручной режим: автобой стоит, заклинания кастуются по кнопкам
const manual = ref(true);
// Кто сейчас кастует: его кнопки неактивны, пока каст не закончится
const casting = reactive(new Set<number>());
const loading = ref(true);
const aurasOn = ref(true);
const statusesOn = ref(true);
// Скрыть верхнюю панель игры (кнопки разделов, ресурсы, аккаунт); панель теста остаётся
const { interfaceHidden } = useInterface();
const onKey = (event: KeyboardEvent) => {
  if (event.code === 'KeyH' && !event.ctrlKey && !event.metaKey && !event.altKey) interfaceHidden.value = !interfaceHidden.value;
};

const state = createBattleState();
resetBattleState(state, { units, logs: [], winningTeam: 0, rounds: 0 });
const fx = new BattleFx(() => layerRef.value?.particleService ?? null, () => speed.value);
const flaresOn = ref(true);
watch(flaresOn, on => (fx.flares = on));
provide(BATTLE_FX_KEY, fx);

const teamA = computed(() => teamUnits(state, 1));
const teamB = computed(() => teamUnits(state, 2));
const teams = computed(() => [
  { title: 'Слева', units: teamA.value },
  { title: 'Справа', units: teamB.value },
]);
const allUnits = () => Object.values(state.units);

// Цвет рамки заклинания — цвет его визуала
const spellColor = (sample: SpellSample) => resolveSpellVisual(sample.name, sample.damageType, SPELL_LEVEL).color;

// Наведение на строку списка подсвечивает дворфа, который будет кастовать
const highlight = (unit: BattleUnit, on: boolean) => {
  if (casting.has(unit.id)) return;
  unit.focus = on ? { role: 'attacker', color: '#ffe8a3' } : null;
};

const bgStyle = {
  backgroundImage: `url('${arenaBackground(BACKGROUND)}')`,
  backgroundRepeat: 'round',
  filter: arenaBackgroundFilter(BACKGROUND),
};

const wait = (ms: number) => new Promise(resolve => setTimeout(resolve, ms / speed.value));
const nextFrame = () => new Promise<void>(resolve => requestAnimationFrame(() => resolve()));
const view = (unit: BattleUnit) => getUnitView(unit.id);
const el = (unit: BattleUnit) => view(unit)?.el?.();

let floatingTextId = 0;
const float = (unit: BattleUnit, text: string, kind: FloatingText['kind'], damageType?: string) => {
  const entry: FloatingText = { id: ++floatingTextId, text, kind, damageType };
  unit.floatingTexts.push(entry);
  setTimeout(() => {
    const index = unit.floatingTexts.findIndex(t => t.id === entry.id);
    if (index >= 0) unit.floatingTexts.splice(index, 1);
  }, FLOATING_TEXT);
};

// Юниты не умирают: здоровье, ушедшее в ноль, восстанавливается
const damage = (unit: BattleUnit, amount: number, damageType: string) => {
  unit.hp = unit.hp > amount ? unit.hp - amount : MAX_HP;
  float(unit, `-${amount}`, 'damage', damageType);
  view(unit)?.hit?.(damageType);
};

const heal = (unit: BattleUnit, amount: number) => {
  unit.hp = Math.min(MAX_HP, unit.hp + amount);
  float(unit, `+${amount}`, 'heal');
  view(unit)?.heal?.();
  fx.heal(el(unit));
};

const applyStatus = (unit: BattleUnit, name: string) => {
  if (statusesOn.value) unit.statuses[name] = { turns: 2, power: 0, stacks: 1 };
};

const clearStatuses = () => allUnits().forEach(u => (u.statuses = {}));
watch(statusesOn, on => !on && clearStatuses());

function shuffled<T>(items: T[]): T[] {
  return items
    .map(item => ({ item, order: Math.random() }))
    .sort((a, b) => a.order - b.order)
    .map(({ item }) => item);
}

// Лечащее заклинание — в союзников (не в себя), остальные — во врагов
const pickTargets = (caster: BattleUnit, support: boolean, count: number) => {
  const pool = support
    ? teamUnits(state, caster.team).filter(u => u.id !== caster.id)
    : teamUnits(state, 3 - caster.team);
  return shuffled(pool).slice(0, count);
};

// Каст как в BattlePlayer.playCast: подсветка, круг под кастером, визуал заклинания, попадания по мере доставки
const castSpell = async (caster: BattleUnit, sample = fighterOf(caster).spell, level = SPELL_LEVEL) => {
  if (casting.has(caster.id)) return;
  casting.add(caster.id);
  const spell = sample.levels[level - 1];
  const support = !spell.damage;
  const visual = resolveSpellVisual(sample.name, sample.damageType, level);
  const targets = pickTargets(caster, support, spell.targets ?? 1);

  caster.focus = { role: 'caster', color: visual.color };
  if (!support) targets.forEach(u => (u.focus = { role: 'target' }));
  view(caster)?.cast?.(visual.color);
  float(caster, sample.name, 'status');
  fx.castCircle(el(caster), visual.color, CAST_WINDUP + 700);
  await wait(CAST_WINDUP);

  const hits = fx.spell(el(caster), targets.map(el), visual);
  await Promise.all(targets.map(async (target, i) => {
    await hits[i];
    if (spell.damage) damage(target, Math.round(ATTACK * spell.damage), sample.damageType);
    if (support && spell.heal) heal(target, Math.round(ATTACK * spell.heal));
    spell.statuses?.forEach(name => applyStatus(target, name));
  }));

  // Лечение вместе с уроном (сияющая вспышка) — кастеру после попаданий
  if (!support && spell.heal) {
    await wait(visual.healDelay ?? 0);
    heal(caster, Math.round(ATTACK * spell.heal));
  }

  await wait(CAST_RECOVER);
  caster.focus = null;
  targets.forEach(u => (u.focus = null));
  casting.delete(caster.id);
};

// Срабатывание ауры как в BattlePlayer.playAura: вспышка носителя, волна, затем статус и попадание на целях
const auraTargets = (bearer: BattleUnit) => {
  const target = bearer.auraTarget;
  if (target === 'All') return allUnits();
  return teamUnits(state, target === 'Allies' ? bearer.team : 3 - bearer.team);
};

const pulseAura = async (bearer: BattleUnit) => {
  const effect = auraEffectOf(bearer);
  if (!effect) return;
  const visual = resolveAuraVisual(effect, bearer.auraTarget);
  view(bearer)?.aura?.(visual.color);
  fx.auraBurst(el(bearer), visual);
  await wait(AURA_WAVE);
  for (const target of auraTargets(bearer)) {
    if (STATUS_VISUALS[effect]) applyStatus(target, effect);
    fx.play(visual.hit, el(target));
  }
};

// Выключенные ауры — без эффекта у юнита: сфера снимается, волн нет
watch(aurasOn, on => allUnits().forEach(u => (u.auraEffect = on ? fighterOf(u).aura.effect : null)));

let active = true;

// Очерёдность ходов: команды по очереди, позиция за позицией
const turnOrder = () => teamA.value.flatMap((unit, i) => [unit, teamB.value[i]]).filter(Boolean);

const play = async () => {
  while (active) {
    for (const unit of turnOrder()) {
      while (active && (paused.value || manual.value)) await wait(100);
      if (!active) return;

      unit.focus = { role: 'attacker', color: '#ffe8a3' };
      await wait(TURN_START);
      if (aurasOn.value && auraEffectOf(unit)) {
        await pulseAura(unit);
        await wait(AURA_RECOVER);
      }
      await castSpell(unit);
      await wait(ACTION_GAP);
    }
  }
};

// Счётчик кадров: средний FPS и самый долгий кадр за полсекунды.
// FPS краснеет, когда падает больше чем на 20% от лучшего (частота экрана)
const fps = ref(0);
const peakFps = ref(0);
const worstFrame = ref(0);
const minFps = ref<number | null>(null);
let fpsFrame = 0;
let frames = 0;
let worst = 0;
let windowStart = performance.now();
let lastFrame = windowStart;

// Профиль эффектов по типам (GlobalParticleService.profile): сколько эмиттеров живо и мс JS за кадр
interface ProfileRow {
  type: EffectType;
  emitters: number;
  ms: number;
  hidden: boolean;
}
const profileRows = ref<ProfileRow[]>([]);
const effectsMs = ref(0);
const particles = () => layerRef.value?.particleService ?? null;

const refreshProfile = () => {
  const service = particles();
  if (!service) return;
  effectsMs.value = service.frameMs;
  profileRows.value = [...service.profile.entries()]
    .map(([type, stat]) => ({ type, ...stat, hidden: service.hidden.has(type) }))
    .filter(row => row.emitters > 0 || row.ms > 0.005 || row.hidden)
    .sort((a, b) => b.ms - a.ms);
};

const toggleType = (type: EffectType) => {
  const service = particles();
  if (!service) return;
  if (service.hidden.has(type)) service.hidden.delete(type);
  else service.hidden.add(type);
  refreshProfile();
};

const measure = (now: number) => {
  frames++;
  worst = Math.max(worst, now - lastFrame);
  lastFrame = now;
  if (now - windowStart >= 500) {
    fps.value = Math.round((frames * 1000) / (now - windowStart));
    worstFrame.value = worst;
    peakFps.value = Math.max(peakFps.value, fps.value);
    if (!loading.value) minFps.value = Math.min(minFps.value ?? fps.value, fps.value);
    refreshProfile();
    frames = 0;
    worst = 0;
    windowStart = now;
  }
  fpsFrame = requestAnimationFrame(measure);
};

onMounted(async () => {
  fpsFrame = requestAnimationFrame(measure);
  window.addEventListener('keydown', onKey);

  // Вьюхи юнитов регистрируются при монтировании, листы спрайтов — ещё грузятся
  await nextTick();
  const service = particles();
  if (service) service.profiling = true;
  await nextFrame();
  await Promise.race([
    Promise.all(allUnits().map(u => view(u)?.ready?.())),
    new Promise(resolve => setTimeout(resolve, SPRITE_WAIT_MS)),
  ]);
  await nextFrame();
  if (!active) return;
  loading.value = false;

  play();
});

onBeforeUnmount(() => {
  active = false;
  cancelAnimationFrame(fpsFrame);
  window.removeEventListener('keydown', onKey);
  // На других страницах панель игры нужна
  interfaceHidden.value = false;
});
</script>

<style scoped>
.stress {
  width: 100vw;
  height: 100vh;
  position: fixed;
}

.stress-bg {
  position: absolute;
  inset: 0;
  z-index: -1;
}

.grid-item {
  position: absolute;
}

.grid-item-left-1 { top: 50%; left: 35%; }
.grid-item-left-2 { top: 65%; left: 30%; }
.grid-item-left-3 { top: 45%; left: 25%; }
.grid-item-left-4 { top: 55%; left: 20%; }
.grid-item-left-5 { top: 65%; left: 15%; }
.grid-item-right-1 { top: 50%; right: 35%; }
.grid-item-right-2 { top: 65%; right: 30%; }
.grid-item-right-3 { top: 45%; right: 25%; }
.grid-item-right-4 { top: 55%; right: 20%; }
.grid-item-right-5 { top: 65%; right: 15%; }

/* Поверх канваса эффектов (z-index 9999 в CanvasLayer) */
.stress-panel {
  position: absolute;
  top: 100px;
  left: 20px;
  z-index: 10000;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.stress-fps {
  align-self: flex-start;
  padding: 6px 12px;
  border-radius: 8px;
  background: rgba(14, 19, 34, 0.8);
  color: #4ade80;
  font: 700 16px monospace;
}

.stress-fps.low {
  color: #f87171;
}

.stress-controls {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.stress-controls button {
  border: 1px solid rgba(255, 255, 255, 0.5);
  background: rgba(14, 19, 34, 0.7);
  color: white;
  border-radius: 8px;
  padding: 8px 12px;
  cursor: pointer;
}

.stress-controls button:disabled {
  opacity: 0.45;
  cursor: default;
}

.stress-controls button.active {
  border-color: #ffd166;
  color: #ffd166;
}

.stress-profile {
  align-self: flex-start;
  display: flex;
  flex-direction: column;
  min-width: 300px;
  padding: 6px;
  border-radius: 8px;
  background: rgba(14, 19, 34, 0.8);
  font: 12px monospace;
}

.stress-profile-head {
  padding: 2px 6px 6px;
  color: #cbd5e1;
  text-align: left;
}

.stress-profile-row {
  display: grid;
  grid-template-columns: 1fr 40px 80px;
  gap: 8px;
  padding: 2px 6px;
  border: none;
  border-radius: 4px;
  background: transparent;
  color: #f8fafc;
  font: inherit;
  text-align: left;
  cursor: pointer;
}

.stress-profile-row:hover {
  background: rgba(255, 255, 255, 0.08);
}

.stress-profile-row span:not(:first-child) {
  text-align: right;
}

/* Скрытый тип: не рисуется */
.stress-profile-row.off {
  color: #64748b;
  text-decoration: line-through;
}

/* Список заклинаний ручного режима — справа, поверх канваса эффектов */
.stress-spells {
  position: absolute;
  top: 100px;
  right: 20px;
  z-index: 10000;
  display: flex;
  flex-direction: column;
  gap: 6px;
  max-height: calc(100vh - 140px);
  overflow-y: auto;
  padding: 8px;
  border-radius: 8px;
  background: rgba(14, 19, 34, 0.8);
  color: #cbd5e1;
  font-size: 13px;
}

.stress-spells-row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 6px;
}

/* Заклинание: имя и кнопки уровней в одной рамке цвета заклинания */
.stress-spell {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 3px 3px 3px 10px;
  border: 1px solid var(--spell-color);
  border-radius: 8px;
  background: rgba(14, 19, 34, 0.7);
  color: white;
}

.stress-spell span {
  margin-right: 4px;
}

.stress-spells-team {
  margin-top: 4px;
  color: #ffd166;
  font-weight: 700;
}

.stress-spells-unit {
  width: 90px;
  font-family: monospace;
}

.stress-spells button {
  min-width: 28px;
  border: 1px solid rgba(255, 255, 255, 0.35);
  background: rgba(14, 19, 34, 0.7);
  color: white;
  border-radius: 6px;
  padding: 4px 8px;
  cursor: pointer;
}

.stress-spells button:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.12);
}

.stress-spells button:disabled {
  opacity: 0.45;
  cursor: default;
}

.stress-spells-aura {
  color: #ffd166 !important;
}

.stress-loading {
  position: absolute;
  inset: 0;
  z-index: 10001;
  display: flex;
  justify-content: center;
  align-items: center;
  background: radial-gradient(ellipse at center, #1a2232 0%, #0a0f18 75%);
  color: white;
  font-size: 20px;
  font-weight: 700;
}
</style>
