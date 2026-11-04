<template>
  <div class="stress">
    <div class="stress-bg" :style="bgStyle"></div>
    <CanvasLayer ref="layerRef">
      <div class="grid">
        <template v-for="unit in teamA" :key="unit.id">
          <CanvasComponent :class="['grid-item', `grid-item-left-${unit.position}`]" :style="slotStyle(unit, 'marginLeft')">
            <CharacterComponent :unit="unit" :scale="slotScale(unit)" :pose="POSES[slots[unit.id].pose]" :animations="SPRITE_ANIMATIONS" />
          </CanvasComponent>
        </template>
        <template v-for="unit in teamB" :key="unit.id">
          <CanvasComponent :class="['grid-item', `grid-item-right-${unit.position}`]" :style="slotStyle(unit, 'marginRight')">
            <CharacterComponent :unit="unit" :scale="slotScale(unit)" :pose="POSES[slots[unit.id].pose]" :animations="SPRITE_ANIMATIONS" rotate />
          </CanvasComponent>
        </template>
      </div>

      <div class="stress-panel">
        <div class="stress-fps" :class="{ low: fps < peakFps * 0.8 }">
          FPS {{ fps }} · худший кадр {{ worstFrame.toFixed(1) }} мс · минимум FPS {{ minFps ?? '—' }}
        </div>
        <div class="stress-controls">
          <button v-tooltip="'Клавиша H'" @click="interfaceHidden = !interfaceHidden">{{ interfaceHidden ? 'Показать интерфейс' : 'Скрыть интерфейс' }}</button>
          <button :class="{ active: manual }" @click="manual = !manual">{{ manual ? 'Ручной режим' : 'Автобой' }}</button>
          <button v-if="!manual" @click="paused = !paused">{{ paused ? 'Продолжить' : 'Пауза' }}</button>
          <button v-for="value in [1, 2, 4]" :key="value" :class="{ active: speed === value }" @click="speed = value">x{{ value }}</button>
          <button :class="{ active: aurasOn }" @click="aurasOn = !aurasOn">Ауры</button>
          <button :class="{ active: statusesOn }" @click="statusesOn = !statusesOn">Статусы</button>
          <button :class="{ active: flaresOn }" @click="flaresOn = !flaresOn">Вспышки</button>
          <button @click="clearStatuses()">Снять статусы</button>
          <button @click="minFps = null">Сбросить минимум</button>
        </div>

        <div class="stress-controls">
          <span class="stress-controls-label">Арена</span>
          <button v-for="item in ARENAS" :key="item.title" :class="{ active: arena === item }" @click="arena = item">{{ item.title }}</button>
        </div>

        <!-- Проверка спрайтов: юнит, цвет, уровень и анимация сразу у всех на арене -->
        <div class="stress-controls">
          <span class="stress-controls-label">Юнит</span>
          <button v-for="item in SPRITE_UNITS" :key="item.title" :class="{ active: spriteUnit === item }" @click="spriteUnit = item">{{ item.title }}</button>
        </div>
        <div class="stress-controls">
          <span class="stress-controls-label">Цвет</span>
          <button v-for="item in SPRITE_ELEMENTS" :key="item.title" :class="{ active: spriteElement === item }" @click="spriteElement = item">{{ item.title }}</button>
        </div>
        <div class="stress-controls">
          <span class="stress-controls-label">Уровень</span>
          <button v-for="value in SPRITE_LEVELS" :key="value" :class="{ active: spriteLevel === value }" @click="spriteLevel = value">{{ value }}</button>
          <span class="stress-controls-label">Анимация</span>
          <button v-for="(item, index) in POSES" :key="item.title" :class="{ active: allOnPose(index) }" @click="setAllPoses(index)">{{ item.title }}</button>
        </div>
        <div class="stress-controls">
          <span class="stress-controls-label">Масштаб всех, %</span>
          <input type="range" :min="SCALE_MIN" :max="SCALE_MAX" :value="allScale" @input="setAllScales">
          <input class="stress-scale-input" type="number" :min="SCALE_MIN" :max="SCALE_MAX" :value="allScale" @input="setAllScales">
          <button :class="{ active: slotsOpen }" @click="slotsOpen = !slotsOpen">Слоты</button>
          <button @click="resetSlots()">Сбросить слоты</button>
        </div>

        <!-- Своя анимация и масштаб у каждого слота: наведение на строку подсвечивает юнита -->
        <div v-if="slotsOpen" class="stress-slots">
          <div
            v-for="unit in [...teamA, ...teamB]"
            :key="unit.id"
            class="stress-slot"
            @mouseenter="highlight(unit, true)"
            @mouseleave="highlight(unit, false)"
          >
            <span class="stress-slot-name">{{ unit.team === 1 ? 'Л' : 'П' }}{{ unit.position }}</span>
            <select v-model.number="slots[unit.id].pose">
              <option v-for="(item, index) in POSES" :key="item.title" :value="index">{{ item.title }}</option>
            </select>
            <input type="range" :min="SCALE_MIN" :max="SCALE_MAX" :value="slots[unit.id].scale" @input="setSlotScale(unit, $event)">
            <input class="stress-scale-input" type="number" :min="SCALE_MIN" :max="SCALE_MAX" :value="slots[unit.id].scale" @input="setSlotScale(unit, $event)">
          </div>
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
            <span class="stress-spells-unit">{{ unit.position }} · {{ fighterOf(unit).element }}</span>
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
                v-tooltip="`${sample.name}, уровень ${level}`"
                @click="castSpell(unit, sample, level)"
              >{{ level }}</button>
            </div>
            <button
              v-if="aurasOn"
              class="stress-spells-aura"
              :disabled="casting.has(unit.id)"
              v-tooltip="'Волна ауры'"
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
import { computed, nextTick, onBeforeUnmount, onMounted, provide, reactive, ref, shallowRef, watch } from 'vue';
import CanvasComponent from '@/components/canvas/CanvasComponent.vue';
import CanvasLayer from '@/components/canvas/CanvasLayer.vue';
import CharacterComponent from '@/components/character-component.vue';
import { BattleUnit, TURN_COLOR, addFloatingText, auraEffectOf, createBattleState, resetBattleState, teamUnits } from '@/battle/BattleState';
import { BATTLE_FX_KEY, BattleFx } from '@/battle/BattleFx';
import { STATUS_VISUALS, resolveAuraVisual, resolveSpellVisual } from '@/battle/FxRegistry';
import { SpriteAnimation, SpritePose } from '@/battle/sprites';
import { getUnitView } from '@/battle/UnitRegistry';
import { useInterface } from '@/composables/UseInterface';
import { EffectType } from '@/services/particleService/types/interfaces/canvas-interfaces';
import { CombatUnitInfo, HeroElement } from '@/battle/types';
import { AURA_SAMPLES, AuraSample, HERO_KINDS, PREVIEW_LOOK_RANK, SPELL_LEVELS, SPELLS, SpellSample } from '@/components/collection/catalog';

// Нагрузочный тест арены: 10 дворфов, у каждого аура и одно заклинание 3-го уровня.
// Автобой: ходят по очереди, как в бою — на своём ходу носитель пускает волну ауры и кастует, и так по кругу без конца.
// Ручной режим: автобой стоит, заклинание (любое из стихии дворфа, любого уровня) выбирается в списке справа.
// Эффекты те же, что в бою (BattleFx и вьюхи юнитов), паузы — как в BattlePlayer

const UNIT_SCALE = 0.8; // как на арене
const SPELL_LEVEL = 3;
const ATTACK = 80;
const MAX_HP = 1200;

// Фоны для просмотра: все картинки assets/arenas как есть, без фильтров игровых арен. Новый файл появится здесь сам
const arenaImages = require.context('@/assets/arenas', false, /\.(png|jpe?g|webp)$/);
const ARENAS = arenaImages.keys().map(path => ({
  title: path.replace(/^\.\/(arena_)?|\.\w+$/g, ''),
  url: arenaImages(path) as string,
}));

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

// Масштаб слота, % от обычного размера на арене
const SCALE_MIN = 25;
const SCALE_MAX = 300;
const SCALE_DEFAULT = 100;
// Рамка юнита при обычном масштабе (кадр 409.6 и поле 1.1, см. character-component) и доля её высоты, где стоят ноги (как у тени)
const UNIT_BOX = 409.6 * 1.1 * UNIT_SCALE;
const FEET_Y = 0.8;

// Проверка спрайтов по рангам: листа нужного ранга или варианта нет — рисуется ближайший (см. battle/sprites.ts)
const SPRITE_LEVELS = [1, 2, 3, 4, 5];
// В отличие от боя, здесь грузится и ходьба — чтобы её проверить
const SPRITE_ANIMATIONS: SpriteAnimation[] = ['idle', 'walk', 'attack', 'victory', 'death'];
// Мобы нарисованы в одном уровне, поэтому кнопки уровней на них не влияют.
// Вперемешку — юниты идут по кругу, чтобы сравнить размеры рядом
const UNIT_TYPES = ['Dwarf', 'Goblin', 'Skeleton', 'Golem', 'Draconid'];
const SPRITE_UNITS: { title: string; type: string | null }[] = [
  { title: 'Дворф', type: 'Dwarf' },
  { title: 'Гоблин', type: 'Goblin' },
  { title: 'Скелет', type: 'Skeleton' },
  { title: 'Голем', type: 'Golem' },
  { title: 'Дракондир', type: 'Draconid' },
  { title: 'Вперемешку', type: null },
];
// Как в отряде — у каждого стихия его заклинаний
const SPRITE_ELEMENTS: { title: string; element: HeroElement | null }[] = [
  { title: 'Как в отряде', element: null },
  { title: 'Без стихии', element: 'Basic' },
  { title: 'Огонь', element: 'Fire' },
  { title: 'Лёд', element: 'Frost' },
  { title: 'Молния', element: 'Lightning' },
  { title: 'Природа', element: 'Nature' },
  { title: 'Тьма', element: 'Dark' },
  { title: 'Кровь', element: 'Blood' },
  { title: 'Сияние', element: 'Radiance' },
];
const POSES: (SpritePose & { title: string })[] = [
  { title: 'Idle', animation: 'idle', variant: 1 },
  { title: 'Ходьба', animation: 'walk', variant: 1 },
  { title: 'Победа', animation: 'victory', variant: 1 },
  { title: 'Удар 1', animation: 'attack', variant: 1 },
  { title: 'Удар 2', animation: 'attack', variant: 2 },
  { title: 'Удар 3', animation: 'attack', variant: 3 },
];

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
  { element: 'Blood', spell: SPELLS.bloodPrice },
  { element: 'Blood', spell: SPELLS.hemorrhage },
  { element: 'Frost', spell: SPELLS.glacialPrison },
  { element: 'Dark', spell: SPELLS.soulHarvest },
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
  level: PREVIEW_LOOK_RANK,
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
const arena = shallowRef(ARENAS.find(item => item.title === 'red') ?? ARENAS[0]);
const spriteLevel = ref(PREVIEW_LOOK_RANK);
const spriteUnit = shallowRef(SPRITE_UNITS[0]);
const spriteElement = shallowRef(SPRITE_ELEMENTS[0]);
// Настройки слотов: pose — номер в POSES, scale — в процентах
const slots = reactive<Record<number, { pose: number; scale: number }>>(
  Object.fromEntries(units.map(unit => [unit.entityId, { pose: 0, scale: SCALE_DEFAULT }])),
);
const slotsOpen = ref(true);
const allScale = ref(SCALE_DEFAULT);
const allSlots = () => Object.values(slots);

const allOnPose = (index: number) => allSlots().every(slot => slot.pose === index);
const setAllPoses = (index: number) => allSlots().forEach(slot => (slot.pose = index));

// Значение ещё набирается или вне диапазона — null, масштаб остаётся прежним
const readScale = (event: Event) => {
  const value = Number((event.target as HTMLInputElement).value);
  return value >= SCALE_MIN && value <= SCALE_MAX ? value : null;
};
const setSlotScale = (unit: BattleUnit, event: Event) => {
  const value = readScale(event);
  if (value !== null) slots[unit.id].scale = value;
};
const setAllScales = (event: Event) => {
  const value = readScale(event);
  if (value === null) return;
  allScale.value = value;
  allSlots().forEach(slot => (slot.scale = value));
};
const resetSlots = () => {
  allScale.value = SCALE_DEFAULT;
  allSlots().forEach(slot => {
    slot.pose = 0;
    slot.scale = SCALE_DEFAULT;
  });
};

const slotScale = (unit: BattleUnit) => (UNIT_SCALE * slots[unit.id].scale) / 100;
// Рамка растёт от своего угла: сдвиг оставляет ноги на месте, чтобы размеры сравнивались в той же точке арены
const slotStyle = (unit: BattleUnit, side: 'marginLeft' | 'marginRight') => {
  const extra = (UNIT_BOX * (slots[unit.id].scale - SCALE_DEFAULT)) / SCALE_DEFAULT;
  return { position: 'absolute' as const, [side]: `${-extra / 2}px`, marginTop: `${-extra * FEET_Y}px` };
};
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
  unit.focus = on ? { role: 'attacker', color: TURN_COLOR } : null;
};

const bgStyle = computed(() => ({
  backgroundImage: `url('${arena.value.url}')`,
  backgroundRepeat: 'round',
}));

const wait = (ms: number) => new Promise(resolve => setTimeout(resolve, ms / speed.value));
const nextFrame = () => new Promise<void>(resolve => requestAnimationFrame(() => resolve()));
const view = (unit: BattleUnit) => getUnitView(unit.id);
const el = (unit: BattleUnit) => view(unit)?.el?.();

// Юниты не умирают: здоровье, ушедшее в ноль, восстанавливается
const damage = (unit: BattleUnit, amount: number, damageType: string) => {
  unit.hp = unit.hp > amount ? unit.hp - amount : MAX_HP;
  addFloatingText(unit, FLOATING_TEXT, `-${amount}`, 'damage', { damageType, amount });
  view(unit)?.hit?.(damageType);
};

const heal = (unit: BattleUnit, amount: number) => {
  unit.hp = Math.min(MAX_HP, unit.hp + amount);
  addFloatingText(unit, FLOATING_TEXT, `+${amount}`, 'heal', { amount });
  view(unit)?.heal?.();
  fx.heal(el(unit));
};

const applyStatus = (unit: BattleUnit, name: string) => {
  if (statusesOn.value) unit.statuses[name] = { turns: 2, power: 0, stacks: 1 };
};

// Заклинание сжигает статус на цели (Жатва душ — проклятие)
const consumeStatus = (unit: BattleUnit, name: string) => {
  delete unit.statuses[name];
};

const clearStatuses = () => allUnits().forEach(u => (u.statuses = {}));
watch(statusesOn, on => !on && clearStatuses());

function shuffled<T>(items: T[]): T[] {
  return items
    .map(item => ({ item, order: Math.random() }))
    .sort((a, b) => a.order - b.order)
    .map(({ item }) => item);
}

// Лечащее заклинание — в союзников (не в себя), остальные — во врагов.
// Заклинание, сжигающее статус, сначала берёт тех, на ком он висит
const pickTargets = (caster: BattleUnit, support: boolean, count: number, prefer?: string) => {
  const pool = support
    ? teamUnits(state, caster.team).filter(u => u.id !== caster.id)
    : teamUnits(state, 3 - caster.team);
  const ordered = shuffled(pool);
  if (prefer) ordered.sort((a, b) => Number(!!b.statuses[prefer]) - Number(!!a.statuses[prefer]));
  return ordered.slice(0, count);
};

// Каст как в BattlePlayer.playCast: подсветка, круг под кастером, визуал заклинания, попадания по мере доставки
const castSpell = async (caster: BattleUnit, sample = fighterOf(caster).spell, level = SPELL_LEVEL) => {
  if (casting.has(caster.id)) return;
  casting.add(caster.id);
  const spell = sample.levels[level - 1];
  const support = !spell.damage;
  const visual = resolveSpellVisual(sample.name, sample.damageType, level);
  const targets = pickTargets(caster, support, spell.targets ?? 1, spell.consumes);

  caster.focus = { role: 'caster', color: visual.color };
  if (!support) targets.forEach(u => (u.focus = { role: 'target' }));
  view(caster)?.cast?.(visual.color);
  addFloatingText(caster, FLOATING_TEXT, sample.name, 'cast', { color: visual.color });
  fx.castCircle(el(caster), visual.color, CAST_WINDUP + 700);
  await wait(CAST_WINDUP);

  // Плата здоровьем — до доставки, как в BattlePlayer.playCast
  if (spell.selfDamage) {
    damage(caster, Math.round(MAX_HP * spell.selfDamage), sample.damageType);
    fx.play(visual.sacrifice, el(caster));
    await wait(visual.sacrificeDelay ?? 0);
  }

  const hits = fx.spell(el(caster), targets.map(el), visual);
  await Promise.all(targets.map(async (target, i) => {
    await hits[i];
    if (spell.damage) damage(target, Math.round(ATTACK * spell.damage), sample.damageType);
    if (spell.consumes) consumeStatus(target, spell.consumes);
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
      while (active && (paused.value || manual.value || loading.value)) await wait(100);
      if (!active) return;

      unit.focus = { role: 'attacker', color: TURN_COLOR };
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

// Вьюхи юнитов регистрируются при монтировании, листы спрайтов — ещё грузятся
const waitForSprites = async () => {
  loading.value = true;
  await nextTick();
  await nextFrame();
  await Promise.race([
    Promise.all(allUnits().map(u => view(u)?.ready?.())),
    new Promise(resolve => setTimeout(resolve, SPRITE_WAIT_MS)),
  ]);
  await nextFrame();
  loading.value = false;
};

// Смена уровня, юнита или цвета пересоздаёт скелеты юнитов: им нужны другие листы
watch([spriteLevel, spriteUnit, spriteElement], () => {
  allUnits().forEach(u => {
    u.level = spriteLevel.value;
    u.unitType = spriteUnit.value.type ?? UNIT_TYPES[(u.id - FIRST_ID) % UNIT_TYPES.length];
    u.element = spriteElement.value.element ?? fighterOf(u).element;
  });
  waitForSprites();
});

onMounted(async () => {
  fpsFrame = requestAnimationFrame(measure);
  window.addEventListener('keydown', onKey);

  await nextTick();
  const service = particles();
  if (service) service.profiling = true;
  await waitForSprites();
  if (!active) return;

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
  width: var(--stage-w);
  height: var(--stage-h);
  position: fixed;
}

.stress-bg {
  position: absolute;
  inset: 0;
  z-index: -1;
}

.grid-item {
  position: absolute;
  /* Насколько каждый отряд отодвинут от центра: расстановка слотов внутри отряда не меняется,
     растёт только промежуток между командами, чтобы удары передних слотов не пересекались */
  --squad-shift: 5%;
}

.grid-item-left-1 { top: 50%; left: calc(35% - var(--squad-shift)); }
.grid-item-left-2 { top: 65%; left: calc(30% - var(--squad-shift)); }
.grid-item-left-3 { top: 45%; left: calc(25% - var(--squad-shift)); }
.grid-item-left-4 { top: 55%; left: calc(20% - var(--squad-shift)); }
.grid-item-left-5 { top: 65%; left: calc(15% - var(--squad-shift)); }
.grid-item-right-1 { top: 50%; right: calc(35% - var(--squad-shift)); }
.grid-item-right-2 { top: 65%; right: calc(30% - var(--squad-shift)); }
.grid-item-right-3 { top: 45%; right: calc(25% - var(--squad-shift)); }
.grid-item-right-4 { top: 55%; right: calc(20% - var(--squad-shift)); }
.grid-item-right-5 { top: 65%; right: calc(15% - var(--squad-shift)); }

/* Поверх канваса эффектов (z-index 9999 в CanvasLayer) */
.stress-panel {
  position: absolute;
  top: 100px;
  left: 20px;
  z-index: 10000;
  display: flex;
  flex-direction: column;
  gap: 8px;
  max-height: calc(var(--stage-h) - 120px);
  overflow-y: auto;
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

.stress-controls-label {
  align-self: center;
  padding: 8px 12px;
  border-radius: 8px;
  background: rgba(14, 19, 34, 0.7);
  color: #cbd5e1;
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

.stress-scale-input {
  width: 64px;
  padding: 6px 8px;
  border: 1px solid rgba(255, 255, 255, 0.5);
  border-radius: 8px;
  background: rgba(14, 19, 34, 0.7);
  color: white;
}

/* Слева первая команда, справа вторая: по пять строк в столбце */
.stress-slots {
  align-self: flex-start;
  display: grid;
  grid-template-rows: repeat(5, auto);
  grid-auto-flow: column;
  gap: 4px 16px;
  padding: 8px;
  border-radius: 8px;
  background: rgba(14, 19, 34, 0.8);
  color: #cbd5e1;
  font-size: 13px;
}

.stress-slot {
  display: flex;
  align-items: center;
  gap: 6px;
}

.stress-slot input[type='range'] {
  width: 110px;
}

.stress-slot-name {
  width: 24px;
  font-family: monospace;
}

.stress-slot select {
  padding: 4px 6px;
  border: 1px solid rgba(255, 255, 255, 0.35);
  border-radius: 6px;
  background: #0e1322;
  color: white;
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
  max-height: calc(var(--stage-h) - 140px);
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
