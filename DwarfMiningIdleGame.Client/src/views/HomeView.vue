<template>
  <div class="home">
    <div v-for="color in colors" :key="color">
      <div class="box" :style="{backgroundColor: color}">{{color}}</div>
    </div>
    <BlackHoleEffect></BlackHoleEffect>
    <BlackHoleEffectChat></BlackHoleEffectChat>
  </div>
</template>

<script lang="ts" setup>
import {battleManager} from '../ecs/service/BattleManager';
import { onMounted, ref } from 'vue';
import { heroes, buildHeroes, updateHeroHp  } from '../models/composables/heroes';
import CharacterDwarf from '@/components/character-dwarf.vue';
import { ICharacterConfig } from '@/models/hero';
import generatePalette from '@/models/composables/colorUtils';
import BlackHoleEffect from '@/components/BlackHoleEffect.vue';
import BlackHoleEffectChat from '@/components/BlackHoleEffectChat.vue';

const colors = ref<ICharacterConfig | null>(null)
onMounted(() => {
  colors.value = generatePalette('#0ff');
})

const skipFight = () => battleManager.skipFight();


</script>
