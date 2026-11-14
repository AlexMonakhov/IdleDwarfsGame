<template>
  <div class="town">
    <CanvasLayer>
      <h1>{{ t('town.title') }}</h1>
    <TimelineGorizontal></TimelineGorizontal>
    <CanvasComponent 
      :effects="animationObject2"
    >    
      <IconButton :badge-count="armorBadge" round inner>
        <IconHelm size="60px" primary-color="rgb(217 215 178)" secondary-color="rgb(64 255 251)"></IconHelm>
      </IconButton>
    </CanvasComponent>
    <GemButton :color="'pink'" @click="open = !open" :width="'70px'" height="44px">{{ t('town.campaign') }}</GemButton>
    <GemButton :color="'green'" @click="open = !open" :badge-count="helmBadge">{{ t('town.hello') }}</GemButton>
    <GemButton :color="'blue'" @click="open = !open" :badge-count="armorBadge" height="60px" width="60px" round>{{ t('town.hi') }}</GemButton>
    <TooltipComponent>
      <GemButton :color="'yellow'">{{ t('town.yes') }}</GemButton>
    </TooltipComponent>

    <ModalComponent
        v-model="open"
        :header-message="t('campaign.header')">
          <ul class="company-list">
            <CanvasLayer>
            <CanvasComponent 
              :effects="[{ func: 'god-rays-radiance', padding: 100, isUnder: true }, { func: 'particles', padding: 100, isUnder: true }]"
            >    
              <IconButton :badge-count="armorBadge" round inner style="margin: 50px">
                <IconHelm size="60px" primary-color="rgb(217 215 178)" secondary-color="rgb(64 255 251)"></IconHelm>
              </IconButton>
            </CanvasComponent>
            <CanvasComponent 
              :effects="[{ func: 'particles', padding: 100, isUnder: false }, { func: 'god-rays-radiance', padding: 100, isUnder: false }]"
            >    
              <IconButton :badge-count="armorBadge" round inner style="margin: 20px" >
                <IconHelm size="60px" primary-color="rgb(217 215 178)" secondary-color="rgb(64 255 251)"></IconHelm>
              </IconButton>
            </CanvasComponent>
            </CanvasLayer>
          </ul>
    </ModalComponent>

    <ModalComponent 
        v-model="open1"
        width="800px"
        height="200px">
        <div class="company-list-row"> AHAHH </div>
    </ModalComponent>

    <TooltipComponent>
    <CanvasComponent 
      style="margin: 50px 0 0 50px"
      :effects="animationObject"
    >    
      <IconButton :badge-count="armorBadge" round inner @click="animationvalue = !animationvalue">
        <IconHelm size="60px" primary-color="rgb(217 215 178)" secondary-color="rgb(64 255 251)"></IconHelm>
      </IconButton>
    </CanvasComponent>
    </TooltipComponent>

    <CardComponent :color="'grey'" :back="{ variant: 'large' }"></CardComponent>
    <CardComponent :color="'yellow'" :back="{ variant: 'crystal' }"></CardComponent>
    <TooltipComponent>
      <template #description>lorem ipsum dolor sit amet consectetur adipisicing elit some long description lorem ipsum dolor sit amet consectetur adipisicing elit some long description</template>
      <CardComponent :color="'blue'" :back="{ variant: 'cracked' }"></CardComponent>
    </TooltipComponent>
    <CardComponent :color="'red'" :back="{ variant: 'emerald' }"></CardComponent>
    <InfoComponent></InfoComponent>
    </CanvasLayer>

    <!-- Тест параметров ледяной кромки (прокрутить вниз) -->
    <div class="card-lab">
      <div v-for="sample in frameSamples" :key="sample.name" class="card-lab-item">
        <CardComponent :color="sample.color" :back="sample.back"></CardComponent>
        <div class="card-lab-label">
          <b>{{ sample.name }}</b>
          <span>{{ describeBack(sample.back) }}</span>
          <span class="card-lab-download">
            скачать:
            <a href="#" @click.prevent="download(sample, 'svg')">svg</a>
            <a href="#" @click.prevent="download(sample, 'webp')">webp</a>
          </span>
        </div>
      </div>
    </div>

    <!-- Карточки героев высшего ранга: аура каждой стихии (у реальных аккаунтов таких героев пока нет) -->
    <div class="card-lab">
      <div v-for="hero in auraSamples" :key="hero.id" class="card-lab-item">
        <HeroCard :hero="hero" selectable />
        <div class="card-lab-label">
          <b>{{ t(`elements.${hero.element}`) }}</b>
        </div>
      </div>
    </div>

    <!-- Готовые картинки из assets/card-backs (back.image), с перекраской и без -->
    <div class="card-lab">
      <div v-for="sample in imageSamples" :key="sample.name + sample.color" class="card-lab-item">
        <CardComponent :color="sample.color" :back="sample.back"></CardComponent>
        <div class="card-lab-label">
          <b>{{ sample.name }}</b>
          <span>{{ sample.back.tint ? 'tint: true' : 'без tint' }}</span>
        </div>
      </div>
    </div>
    <!-- Экран загрузки: концепты кристалла, который собирается по мере загрузки -->
    <div class="loader-lab">
      <div class="loader-lab-controls">
        <b>Экран загрузки — концепты</b>
        <input type="range" min="0" max="100" step="1" v-model.number="loaderPercent" @input="stopLoaderSim" />
        <span class="loader-lab-value">{{ loaderPercent }}%</span>
        <GemButton color="green" @click="runLoaderSim">Симуляция</GemButton>
        <GemButton color="blue" @click="loaderSeed++">Новый раскол</GemButton>
        <GemButton color="yellow" @click="runArenaPreview">Экран арены</GemButton>
      </div>

      <!-- Превью экрана загрузки арены: те же этапы, что в ArenaView (запрос → фон → спрайты) -->
      <div v-if="arenaPreview.mounted" class="arena-preview" @click="arenaPreview.active = false">
        <ArenaLoadingScreen :active="arenaPreview.active" :progress="arenaPreview.progress" :background="arenaPreview.background" />
      </div>
      <div class="card-lab">
        <div v-for="(sample, i) in loaderSamples" :key="`${loaderSeed}-${i}`" class="card-lab-item">
          <CrystalLoader
            :progress="loaderPercent / 100"
            :variant="sample.variant"
            :shape="sample.shape"
            :color="sample.color"
            :gradient-to="sample.gradientTo ?? null"
            :frame="sample.frame"
            :seed="loaderSeed * 101 + i"
          />
          <div class="card-lab-label">
            <b>{{ sample.name }}</b>
            <span>{{ sample.variant }} · {{ sample.shape }} · {{ sample.color }}{{ sample.gradientTo ? ` → ${sample.gradientTo}` : '' }}</span>
          </div>
        </div>
      </div>

      <!-- Кристаллы-полосы загрузки со счетчиком (тот же ползунок) -->
      <div class="bar-lab">
        <div v-for="(sample, i) in barSamples" :key="`${loaderSeed}-bar-${i}`" class="bar-lab-item">
          <CrystalBar
            :progress="loaderPercent / 100"
            :variant="sample.variant"
            :color="sample.color"
            :gradient-to="sample.gradientTo ?? null"
            :counter="sample.counter"
            :seed="loaderSeed * 211 + i"
          />
          <div class="card-lab-label">
            <b>{{ sample.name }}</b>
            <span>{{ sample.variant }} · счетчик: {{ sample.counter }} · {{ sample.color }}{{ sample.gradientTo ? ` → ${sample.gradientTo}` : '' }}</span>
          </div>
        </div>
      </div>

      <!-- CrystalIcon отдельно: целый камень как иконка (так он стоит в карточке камней на складе) -->
      <div class="icon-lab">
        <div v-for="gem in iconSamples" :key="gem.name" class="icon-lab-item">
          <CrystalIcon :color="gem.color" :shape="gem.shape" size="64px" />
          <span>{{ gem.name }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import TimelineGorizontal from '@/components/timeline-gorizontal.vue';

import ModalComponent from '@/components/common/ModalComponent.vue';
import GemButton from '@/components/common/GemButton.vue';
import IconHelm from '@/components/icons/IconHelm.vue';
import { useInventoryBadge } from '@/composables/UseInventoryBadge';
import IconButton from '@/components/common/IconButton.vue';
import { ref, reactive, onMounted, onUnmounted, provide, nextTick, computed } from 'vue';
import CanvasComponent from '@/components/canvas/CanvasComponent.vue';
import CanvasLayer from '@/components/canvas/CanvasLayer.vue';
import CardComponent from '@/components/card/CardComponent.vue';
import { downloadCardBack } from '@/components/card/cardBackArt';
import type { CardBackOptions } from '@/components/card/cardBackArt';
import crystalLarge from '@/assets/card-backs/crystal-large.webp';
import crystalCracked from '@/assets/card-backs/crystal-cracked.webp';
import crystalEmerald from '@/assets/card-backs/crystal-emerald.webp';
import InfoComponent from '@/components/common/InfoComponent.vue';
import TooltipComponent from '@/components/common/TooltipComponent.vue';
import HeroCard from '@/components/inventory/HeroCard.vue';
import CrystalLoader from '@/components/loading/CrystalLoader.vue';
import CrystalBar from '@/components/loading/CrystalBar.vue';
import CrystalIcon from '@/components/loading/CrystalIcon.vue';
import ArenaLoadingScreen from '@/components/loading/ArenaLoadingScreen.vue';
import { GEM_COLORS } from '@/components/inventory/lootStyles';
import { AURA_MIN_RANK } from '@/components/effects/elementAura';
import type { HeroElement } from '@/battle/types';
import type { HeroInfo } from '@/models/types/Heroes';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const open = ref(false);
const open1 = ref(false);
const animationvalue = ref(false);
const onStageClick = async (lvl: number) => {
  open1.value = !open1.value;
  await nextTick();
  // router.push("arena");
}
const { count: helmBadge } = useInventoryBadge('helm');
const { count: armorBadge } = useInventoryBadge('armor');

const animationObject = computed(() => {
  if(animationvalue.value) return ([{ func: 'particles', padding: 100, isUnder: false }, { func: 'god-rays', padding: 100, isUnder: true }])
  else return ([])
})

type CardColor = 'pink' | 'green' | 'blue' | 'yellow' | 'red' | 'grey';

// Варианты рубашки для сравнения (не указанные параметры — по умолчанию, см. components/card/README.md)
const frameSamples: { name: string; color: CardColor; back: CardBackOptions }[] = [
  { name: '1. По умолчанию', color: 'grey', back: {} },
  { name: '2. Тонкая', color: 'blue', back: { frame: { depth: 8, depthJitter: 3 } } },
  { name: '3. Толстая', color: 'green', back: { frame: { depth: 24, depthJitter: 8 } } },
  { name: '4. Мелкие куски', color: 'pink', back: { frame: { step: 10 } } },
  { name: '5. Крупные куски', color: 'yellow', back: { frame: { step: 38 } } },
  { name: '6. Много торчит наружу', color: 'grey', back: { frame: { protrudeChance: 0.7 } } },
  { name: '7. Ровный внешний край', color: 'blue', back: { frame: { protrudeChance: 0 } } },
  { name: '8. Много шипов внутрь', color: 'green', back: { frame: { spikeChance: 0.5 } } },
  { name: '9. Длинные шипы', color: 'pink', back: { frame: { spikeChance: 0.3, spikeLength: 38 } } },
  { name: '10. Аккуратная', color: 'yellow', back: { frame: { step: 30, depthJitter: 2, protrudeChance: 0, spikeChance: 0 } } },
  { name: '11. Хаос', color: 'red', back: { frame: { step: 16, depthJitter: 18, protrudeChance: 0.5, spikeChance: 0.35, spikeLength: 30 } } },
  { name: '12. Монолит', color: 'blue', back: { variant: 'cracked', frame: { step: 45, depth: 18, depthJitter: 12, protrudeChance: 0.4, spikeChance: 0.2 } } },
];

// Цвет редкости как его видит карта (переменные из App.vue; у grey переменной нет)
const resolveColor = (color: CardColor) =>
  getComputedStyle(document.documentElement).getPropertyValue(`--${color}-gem-color`).trim() || '#9aa3ad';

const download = (sample: { name: string; color: CardColor; back: CardBackOptions }, format: 'svg' | 'webp') =>
  downloadCardBack(resolveColor(sample.color), sample.back, `card-back-${sample.name.split('.')[0]}-${sample.color}`, format);

const imageSamples: { name: string; color: CardColor; back: CardBackOptions }[] = [
  { name: 'crystal-large.webp', color: 'grey', back: { image: crystalLarge } },
  { name: 'crystal-large.webp', color: 'blue', back: { image: crystalLarge, tint: true } },
  { name: 'crystal-cracked.webp', color: 'yellow', back: { image: crystalCracked, tint: true } },
  { name: 'crystal-emerald.webp', color: 'pink', back: { image: crystalEmerald, tint: true } },
];

// Герой-заглушка каждой стихии на ранге, с которого карточка получает ауру
const AURA_ELEMENTS: HeroElement[] = ['Fire', 'Frost', 'Lightning', 'Nature', 'Dark', 'Radiance', 'Blood'];
const auraSamples: HeroInfo[] = AURA_ELEMENTS.map(element => ({
  id: `aura-${element}`,
  name: element,
  unitType: 'Dwarf',
  element,
  level: AURA_MIN_RANK,
  placeInSquad: null,
  onExpedition: false,
  inMine: false,
  maxHp: 200,
  attack: 20,
  speed: 6,
  maxEnergy: 100,
  spells: [],
  immunities: [],
  power: { total: 1000, stats: 600, level: 200, skills: 200, passives: 0 },
  upgrade: { level: 20, maxLevel: 20, cost: null, next: null },
}));

const describeBack = (back: CardBackOptions) => {
  const { frame = {}, ...rest } = back;
  const entries = [...Object.entries(rest), ...Object.entries(frame)];
  return entries.length ? entries.map(([k, v]) => `${k}: ${v}`).join(', ') : 'всё по умолчанию';
};

const animationObject2 = computed(() => {
  if(animationvalue.value) return ([{ func: 'particles', padding: 100, isUnder: false }])
  else return ([{ func: 'particles', padding: 100, isUnder: true, args: { speed: '2', color: '#0ff' } }])
})

// --- Экран загрузки: демо-прогресс (рывками, как настоящая загрузка ассетов) ---
const loaderPercent = ref(0);
const loaderSeed = ref(1);
let loaderTimer: number | undefined;
const stopLoaderSim = () => { clearTimeout(loaderTimer); loaderTimer = undefined; };
const runLoaderSim = () => {
  stopLoaderSim();
  loaderPercent.value = 0;
  const tick = () => {
    loaderPercent.value = Math.min(100, loaderPercent.value + Math.ceil(Math.random() * 7));
    if (loaderPercent.value < 100) loaderTimer = window.setTimeout(tick, 80 + Math.random() * 260);
  };
  loaderTimer = window.setTimeout(tick, 400);
};
onUnmounted(stopLoaderSim);

// Превью экрана загрузки арены: каждый запуск — следующая арена. Клик по экрану — «загрузка закончилась»
const PREVIEW_ARENAS = ['lava', 'ice', 'storm', 'marsh', 'crypt', 'sanctum', 'altar'];
const arenaPreview = reactive({ mounted: false, active: false, progress: 0, background: 'lava', index: -1 });
let arenaPreviewTimers: number[] = [];
const runArenaPreview = () => {
  arenaPreviewTimers.forEach(clearTimeout);
  arenaPreview.index = (arenaPreview.index + 1) % PREVIEW_ARENAS.length;
  arenaPreview.background = PREVIEW_ARENAS[arenaPreview.index];
  arenaPreview.mounted = true;
  arenaPreview.active = true;
  arenaPreview.progress = 0.35;
  const at = (ms: number, fn: () => void) => arenaPreviewTimers.push(window.setTimeout(fn, ms));
  at(2500, () => (arenaPreview.progress = 0.55));
  at(3600, () => (arenaPreview.progress = 0.6));
  for (let i = 1; i <= 8; i++) at(3600 + i * 350, () => (arenaPreview.progress = 0.6 + 0.38 * (i / 8)));
  at(6800, () => (arenaPreview.active = false));
  at(9000, () => (arenaPreview.mounted = false));
};
onUnmounted(() => arenaPreviewTimers.forEach(clearTimeout));

type LoaderSample = {
  name: string
  variant: 'assemble' | 'heal' | 'shards'
  shape: 'brilliant' | 'crystal'
  color: 'pink' | 'green' | 'blue' | 'yellow' | 'red' | 'purple' | 'white' | 'random'
  gradientTo?: 'pink' | 'green' | 'blue' | 'yellow' | 'red' | 'purple' | 'white' | 'random'
  frame: 'round' | 'bevel'
};
const loaderSamples: LoaderSample[] = [
  { name: 'A1. Грани слетаются', variant: 'assemble', shape: 'brilliant', color: 'random', frame: 'round' },
  { name: 'A2. Кристалл, градиент', variant: 'assemble', shape: 'crystal', color: 'blue', gradientTo: 'purple', frame: 'bevel' },
  { name: 'B1. Трещины затягиваются', variant: 'heal', shape: 'brilliant', color: 'pink', frame: 'round' },
  { name: 'B2. Кристалл наливается', variant: 'heal', shape: 'crystal', color: 'green', gradientTo: 'yellow', frame: 'bevel' },
  { name: 'C1. Осколки от центра', variant: 'shards', shape: 'brilliant', color: 'random', gradientTo: 'random', frame: 'round' },
  { name: 'C2. Осколки, кристалл', variant: 'shards', shape: 'crystal', color: 'red', frame: 'bevel' },
];

type BarSample = {
  name: string
  variant: 'shards' | 'cluster'
  color: LoaderSample['color']
  gradientTo?: LoaderSample['color']
  counter: 'center' | 'end' | 'follow' | 'none'
};
const barSamples: BarSample[] = [
  { name: 'F. Осколки рваным фронтом', variant: 'shards', color: 'random', gradientTo: 'random', counter: 'end' },
  { name: 'G. Друза растет', variant: 'cluster', color: 'purple', gradientTo: 'pink', counter: 'center' },
];

const iconSamples = (['Ruby', 'Sapphire', 'Topaz', 'Emerald', 'Diamond', 'Amethyst', 'Garnet'] as const).map(name => ({
  name,
  color: GEM_COLORS[name],
  shape: (['Ruby', 'Diamond', 'Garnet'] as string[]).includes(name) ? 'brilliant' as const : 'crystal' as const,
}));
</script>

<style lang="scss">
.town {
    width: var(--stage-w);
    height: var(--stage-h);
    position: fixed;
    background-color: darkcyan;
    overflow-y: auto;
}

.arena-preview {
    position: fixed;
    inset: 0;
    z-index: 10000;
    pointer-events: auto;
}

.loader-lab {
    padding: 40px 24px 0;
}

.loader-lab-controls {
    display: flex;
    align-items: center;
    gap: 12px;
    flex-wrap: wrap;
    padding: 10px 14px;
    border-radius: 10px;
    background: rgba(0, 0, 0, 0.45);
    color: #fff;
    font-family: monospace;
}

.loader-lab-controls input[type=range] {
    width: 240px;
}

.loader-lab-value {
    min-width: 40px;
}

.bar-lab {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 28px;
    padding: 0 24px 40px;
}

.bar-lab-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
}

.bar-lab-item .card-lab-label {
    max-width: none;
}

.icon-lab {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: 20px;
    padding: 0 24px 80px;
}

.icon-lab-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
    color: #fff;
    font-family: monospace;
    font-size: 12px;
}

.card-lab {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: 32px 16px;
    padding: 40px 24px 80px;
}

.card-lab-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
}

.card-lab-label {
    display: flex;
    flex-direction: column;
    gap: 2px;
    max-width: 230px;
    padding: 6px 10px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.45);
    color: #fff;
    font-size: 12px;
    font-family: monospace;
}

.card-lab-download a {
    color: #7dd3fc;
    margin-left: 6px;
}
.company-list-row{
    border: 1px solid white;
    border-radius: 20px;
    width: 30%;
    padding: 2px;
    height: 70px;
}


</style>