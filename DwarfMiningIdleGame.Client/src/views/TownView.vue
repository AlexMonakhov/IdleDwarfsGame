<template>
  <div class="town">
    <CanvasLayer>
      <h1>{{ aboutMessage }}</h1>
    <TimelineGorizontal></TimelineGorizontal>
    <CanvasComponent 
      :effects="[{ func: 'god-rays-radiance', padding: 100, isUnder: true }, { func: 'particles', padding: 100, isUnder: true }]"
    >    
      <IconButton :badge-count="armorBadge" round inner>
        <IconHelm size="60px" primary-color="rgb(217 215 178)" secondary-color="rgb(64 255 251)"></IconHelm>
      </IconButton>
    </CanvasComponent>
    <GemButton :color="'pink'" @click="open = !open" :width="'70px'" height="44px">compaign</GemButton>
    <GemButton :color="'green'" @click="open = !open" :badge-count="helmBadge">hello</GemButton>
    <GemButton :color="'blue'" @click="open = !open" :badge-count="armorBadge" height="60px" width="60px" round>hi</GemButton>
    <GemButton :color="'yellow'">yes?</GemButton>

    <ModalComponent 
        v-model="open"
        :header-message="'campaign'">
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
              :effects="[{ func: 'particles', padding: 100, isUnder: false }, { func: 'god-rays-radiance', padding: 100, isUnder: true }]"
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

    <CanvasComponent 
      style="margin: 50px 0 0 50px"
      :effects="[{ func: 'god-rays-radiance', padding: 100, isUnder: true }, { func: 'particles', padding: 100, isUnder: false }]"
    >    
      <IconButton :badge-count="armorBadge" round inner>
        <IconHelm size="60px" primary-color="rgb(217 215 178)" secondary-color="rgb(64 255 251)"></IconHelm>
      </IconButton>
    </CanvasComponent>

    <CardComponent :color="'grey'"></CardComponent>
    <CardComponent :color="'yellow'"></CardComponent>
    <CardComponent :color="'green'"></CardComponent>
    </CanvasLayer>
  </div>
</template>

<script lang="ts" setup>
import TimelineGorizontal from '@/components/timeline-gorizontal.vue';

import ModalComponent from '@/components/common/ModalComponent.vue';
import GemButton from '@/components/common/GemButton.vue';
import IconHelm from '@/components/icons/IconHelm.vue';
import { useInventoryBadge } from '@/composables/UseInventoryBadge';
import IconButton from '@/components/common/IconButton.vue';
import { ref, onMounted, provide, nextTick } from 'vue';
import CanvasComponent from '@/components/canvas/CanvasComponent.vue';
import CanvasLayer from '@/components/canvas/CanvasLayer.vue';
import CardComponent from '@/components/card/CardComponent.vue';

const aboutMessage = ref("about message");
const open = ref(false);
const open1 = ref(false);
const onStageClick = async (lvl: number) => {
  open1.value = !open1.value;
  await nextTick();
  // router.push("arena");
}
const { count: helmBadge } = useInventoryBadge('helm');
const { count: armorBadge } = useInventoryBadge('armor');
</script>

<style lang="scss">
.town {
    width: 100vw;
    height: 100vh;
    position: fixed;
    background-color: darkcyan;
}
.company-list-row{
    border: 1px solid white;
    border-radius: 20px;
    width: 30%;
    padding: 2px;
    height: 70px;
}


</style>