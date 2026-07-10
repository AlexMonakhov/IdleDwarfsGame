<template>
  <div class="arena" :style="bgStyle">
      {{  fightresult }}
      <div class="grid">
        <template v-for="(hero, n) in teamA" :key="hero">
          <CharacterComponent  :class="['grid-item',`grid-item-left-${n + 1}`]"  :entity="hero" :animation-type="animationType"/> 
        </template>
        <template v-for="(hero, n) in teamB" :key="hero">
          <CharacterComponent  :class="['grid-item', 'grid-item-right',`grid-item-right-${n + 1}`]"  :entity="hero" rotate :animation-type="animationType"/> 
        </template>
      </div>
      <div><button @click="changeAnim('idle')">Toggle Idle</button>
      <button @click="changeAnim('victory')">Toggle victory</button>
    <button @click="changeAnim('attack')">Toggle attack</button></div>
      
</div>

  </template>
  
  <script lang="ts" setup>
  import { battleManager } from '../ecs/service/BattleManager';
  import { computed, onMounted, ref } from 'vue';
  import { heroes, buildHeroes, updateHeroHp  } from '../models/composables/heroes';
  import CharacterDwarf from '@/components/character-dwarf.vue';
  import { Entity } from '@/ecs/entities/Storage';
  import arenaBgBlueCrystals from '@/assets/arena_red_crystals.png';
  import mButton from '@/components/m-button.vue';
  import { logService } from '@/ecs/service/LogService';
  import CharacterSkeleton from '@/components/character-skeleton.vue';
  import CharacterComponent from '@/components/character-component.vue';
  import image from '@/assets/sprite-list/dwarf-idle/dwarf_idle_lvl4_red_1.png';
  import { CombatApiService } from '@/services/apiServices/CombatApiService';

  const bgImage = ref('arena_blue_crystals')
  const teamA = ref<Entity[]>([]);
  const teamB = ref<Entity[]>([]);
  const animationType = ref<'idle' | 'attack' | 'victory'>('idle');
  const fightresult = ref<any>(null);

  const bgMap = {
  arena_blue_crystals: arenaBgBlueCrystals,
  };  

  onMounted(() => {
    battleManager.createEntities();  
    buildHeroes();
    [teamA.value, teamB.value] = battleManager.getTeams();
    battleManager.StartBattle();
  })

  const round = computed(() => {
    return battleManager.state.round
  })

  const winner = computed(() => {
    return battleManager.state.winner
  })
  
  const onSkipClick = () => battleManager.skipFight();
  const onLogClick = () => {console.log(logService.getLog())}

  const bgStyle = computed(() => ({
    backgroundImage: `url('${bgMap[bgImage.value]}')`,
    backgroundRepeat: 'round',
    
    }));

  const  changeAnim = async (animType: 'idle' | 'attack' | 'victory') => {
    fightresult.value = await CombatApiService.start();
    animationType.value = animType;
  };  


  </script>

  <style lang="scss">
  .arena{
    width: 100vw;
    height: 100%;
    position: fixed;
  }

  .fight-info{
    font-size: 3em;
    color: whitesmoke;
    font-weight: 400;
    text-shadow: -2px 1px 1px #888;
  }

.container {
  display: grid;
  grid-template-columns: 200px 200px auto 200px 200px;
  background-color: dodgerblue;
  padding: 5px;
  gap: 5px;  
}
.container div {
  background-color: #f1f1f1;
  color: #000;
  padding: 10px;
  font-size: 30px;
  text-align: center;
}

// .grid {
//     display: grid;
//     grid-template-columns: repeat(12, 1fr);
//     grid-template-rows: repeat(5, 1fr);
//     position: absolute;
//     width: 100%;
//     height: 100%;
//     background-color: rgba(0, 0, 0, 0.5);
//     gap: 5px;
// }

// .grid-item-left-1 {
// 	grid-column: 3 ;
//   grid-row: 3 ;
// }

// .grid-item-left-2 {
// 	grid-column: 3 ;
//   grid-row: 4 ;
// }

// .grid-item-left-3 {
// 	grid-column: 3 ;
//     grid-row: 5;
// }

// .grid-item-left-4 {
// 	grid-column: 4;
//   grid-row: 4;
// }

// .grid-item-left-5 {
// 	grid-column: 4;
//     grid-row: 5;
// }

// .grid-item-right-1 {
// 	grid-column: 10;
//     grid-row: 4;
// }

// .grid-item-right-2 {
// 	grid-column: 10 ;
//     grid-row: 5;
// }

// .grid-item-right-3 {
// 	grid-column: 11;
//   grid-row: 3;
// }

// .grid-item-right-4 {
// 	grid-column: 11;
//     grid-row: 4;
// }

// .grid-item-right-5 {
// 	grid-column: 11;
//     grid-row: 5;
// }

.grid-item{
  position: absolute;
}


.grid-item-left-1 {
  top:50%;
  left: 35%;
}

.grid-item-left-2 {
  top:65%;
  left: 30%;
}

.grid-item-left-3 {
  top: 45%;
  left: 25%;
}

.grid-item-left-4 {
  top:55%;
  left: 20%;
}

.grid-item-left-5 {
  top:65%;
  left: 15%;
}

.grid-item-right-1 {
  top:50%;
  right: 35%;
}

.grid-item-right-2 {
  top:65%;
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
  