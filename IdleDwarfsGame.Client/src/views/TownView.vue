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
  </div>
</template>

<script lang="ts" setup>
import TimelineGorizontal from '@/components/timeline-gorizontal.vue';

import ModalComponent from '@/components/common/ModalComponent.vue';
import GemButton from '@/components/common/GemButton.vue';
import IconHelm from '@/components/icons/IconHelm.vue';
import { useInventoryBadge } from '@/composables/UseInventoryBadge';
import IconButton from '@/components/common/IconButton.vue';
import { ref, onMounted, provide, nextTick, computed } from 'vue';
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
const AURA_ELEMENTS: HeroElement[] = ['Fire', 'Frost', 'Lightning', 'Nature', 'Dark', 'Radiance'];
const auraSamples: HeroInfo[] = AURA_ELEMENTS.map(element => ({
  id: `aura-${element}`,
  name: element,
  unitType: 'Dwarf',
  element,
  level: AURA_MIN_RANK,
  placeInSquad: null,
  onExpedition: false,
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
</script>

<style lang="scss">
.town {
    width: 100vw;
    height: 100vh;
    position: fixed;
    background-color: darkcyan;
    overflow-y: auto;
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