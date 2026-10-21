<script lang="ts" setup >
import { ref, watch, defineProps, computed } from 'vue';
import CharacterSkeleton from './character-skeleton.vue';
import CharacterStats from './character-stats.vue';
import { heroes } from '@/models/composables/heroes';
import { HeroModel } from '@/models/hero';

import { Entity } from '@/ecs/entities/Storage';


let model = ref<HeroModel | null>(null);
let damageTaken = ref(0);
const aura = ref(false);

const props = defineProps<{
  entity: Entity;
  sprite?: string;
  frameWidth?: number;
  frameHeight?: number;
  frameCount?: number;
  scale?: number;
  rotate?: boolean;
  animationType?: 'idle' | 'attack' | 'victory';
}>();

const getSkeletonStyle = computed(() => {
  return {
    transform: props.rotate ? 'rotateY(180deg)' : 'none',
    filter: model.value?.currentHp === 0 ? 'grayscale(100%)' : 'none',
  };
});


watch(
  () => heroes.value,
  (newHeroes) => {
    const found = newHeroes.find(hero => hero.id === props.entity) ?? null;
    console.log(found);
    model.value = {
      name: found?.name ?? 'Unknown',
      id: props.entity,
      currentHp: found?.currentHp ?? 100,
      maxHp: found?.maxHp ?? 100,
      lvl: found?.lvl ?? 4,
      color: Math.random() < 0.5 ? 'red' : 'blue'
    };
  },
  { immediate: true, deep: true }
);

watch(
  () => model.value?.currentHp,
  (newHp, oldHp) => {
    damageTaken.value = (oldHp ?? 0) - (newHp ?? 0);
  },
);
</script>

<template>
    <div class="character-wrapper" >
        <div v-if="aura" class="back-area"></div>
        <div v-if="aura" class="front-area"></div>
        <CharacterStats class="character-stats" :currentHp="model?.currentHp!" :maxHp="model?.maxHp!"></CharacterStats>
        <CharacterSkeleton 
              :style="getSkeletonStyle"
              :key="model?.id"
              :sprite-src="props.sprite"
              :animation-type="props.animationType"
              :model="model"
              :frame-width="props.frameWidth ?? 409.6"
              :frame-height="props.frameHeight ?? 409.6"
              :frame-count="props.frameCount ?? 24"
              :fps="12"
              :columns="5"
              :scale="props.scale ?? 0.65"
              :padding="1.1"></CharacterSkeleton>
       
    </div>
</template>

<style>
  .character-wrapper{
    width: min-content;
    position: relative;
  }
  .character-stats{
    transform: translate(50%);
    top: 15%;
    width: 50%;
    height: 20px;
    z-index: 1;
    position: absolute;
  }
  .back-area{
    position: absolute;
    height: 60%;
    width: 50%;
    transform: translate(50%, 40%);
    border-radius: 50%;
    box-shadow: inset 0 0 40px 2px red, inset 0 0 50px 5px yellow, 0 0 40px 2px red, inset 0 0 50px 5px yellow, inset 0 0 50px 5px yellow, inset 0 0 30px 5px red;
    z-index: -1;
  }
  .front-area{
    position: absolute;
    height: 60%;
    width: 50%;
    transform: translate(50%, 40%);
    border-radius: 50%;
    box-shadow: inset 0 0 40px 2px red, inset 0 0 50px 5px yellow, 0 0 40px 2px red, inset 0 0 50px 5px yellow, inset 0 0 50px 5px yellow, inset 0 0 30px 5px red;
    z-index: 1;
  }
</style>