<template>
  <div class="arena" :style="bgStyle">
    <CanvasLayer>
      <div class="grid">
        <template v-for="(hero, n) in teamA" :key="hero">
          <CanvasComponent
            ref="teamARefs"
            :class="['grid-item', `grid-item-left-${n + 1}`]"
            :animationKey="animationType"
            :effects="[{ func: 'god-rays-radiance', padding: 100, isUnder: true }, { func: 'particles', padding: 100, isUnder: true }].concat([{ func: 'particles', padding: 20, isUnder: false, from: teamARefs[n], to: teamBRefs[n], args: { color: `#${n}${n}${n}${n}${n}${n}`, duration: 650, speed: 2 } }])"
            style="position: absolute;"
          >
            <CharacterComponent :entity="hero" :animation-type="animationType" />
          </CanvasComponent>
        </template>

        <template v-for="(hero, n) in teamB" :key="hero">
          <CanvasComponent
            ref="teamBRefs"
            :class="['grid-item', 'grid-item-right', `grid-item-right-${n + 1}`]"
            :animationKey="animationType"
            :effects="[{ func: 'god-rays-radiance', padding: 100, isUnder: true }]"
            style="position: absolute;"
          >
            <CharacterComponent :entity="hero" rotate :animation-type="animationType" />
          </CanvasComponent>
        </template>
      </div>

      <div>
        <button @click="changeAnim('idle')">Toggle Idle</button>
        <button @click="changeAnim('victory')">Toggle victory</button>
        <button @click="changeAnim('attack')">Toggle attack</button>
      </div>
    </CanvasLayer>
  </div>
</template>

<script lang="ts" setup>
import { battleManager } from '../ecs/service/BattleManager';
import { computed, onMounted, ref } from 'vue';
import { buildHeroes } from '../models/composables/heroes';
import { Entity } from '@/ecs/entities/Storage';
import arenaBgBlueCrystals from '@/assets/arena_red_crystals.png';
import CharacterComponent from '@/components/character-component.vue';
import CombatApiService from '@/services/apiServices/CombatApiService';
import CanvasComponent from '@/components/canvas/CanvasComponent.vue';
import CanvasLayer from '@/components/canvas/CanvasLayer.vue';

const bgImage = ref('arena_blue_crystals');
const teamA = ref<Entity[]>([]);
const teamB = ref<Entity[]>([]);
const animationType = ref<'idle' | 'attack' | 'victory'>('idle');
const fightresult = ref<any>(null);
const teamARefs = ref<any[]>([]);
const teamBRefs = ref<any[]>([]);

const bgMap = {
  arena_blue_crystals: arenaBgBlueCrystals,
};

onMounted(() => {
  battleManager.createEntities();
  buildHeroes();
  [teamA.value, teamB.value] = battleManager.getTeams();
  battleManager.StartBattle();
});

const round = computed(() => battleManager.state.round);
const winner = computed(() => battleManager.state.winner);

const bgStyle = computed(() => ({
  backgroundImage: `url('${bgMap[bgImage.value]}')`,
  backgroundRepeat: 'round',
}));

const changeAnim = async (animType: 'idle' | 'attack' | 'victory') => {
  fightresult.value = await CombatApiService.start();
  animationType.value = animType;
};
</script>

<style lang="scss">
.arena {
  width: 100vw;
  height: 100vh;
  position: fixed;
}

.grid-item {
  position: absolute;
}

.grid-item-left-1 {
  top: 50%;
  left: 35%;
}

.grid-item-left-2 {
  top: 65%;
  left: 30%;
}

.grid-item-left-3 {
  top: 45%;
  left: 25%;
}

.grid-item-left-4 {
  top: 55%;
  left: 20%;
}

.grid-item-left-5 {
  top: 65%;
  left: 15%;
}

.grid-item-right-1 {
  top: 50%;
  right: 35%;
}

.grid-item-right-2 {
  top: 65%;
  right: 30%;
}

.grid-item-right-3 {
  top: 45%;
  right: 25%;
}

.grid-item-right-4 {
  top: 55%;
  right: 20%;
}

.grid-item-right-5 {
  top: 65%;
  right: 15%;
}
</style>
