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

    <CardComponent :color="'grey'"></CardComponent>
    <CardComponent :color="'yellow'"></CardComponent>
    <TooltipComponent>
      <template #description>lorem ipsum dolor sit amet consectetur adipisicing elit some long description lorem ipsum dolor sit amet consectetur adipisicing elit some long description</template>
      <CardComponent :color="'blue'"></CardComponent>
    </TooltipComponent>
    <CardComponent :color="'green'"></CardComponent>
    <InfoComponent></InfoComponent>
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
import { ref, onMounted, provide, nextTick, computed } from 'vue';
import CanvasComponent from '@/components/canvas/CanvasComponent.vue';
import CanvasLayer from '@/components/canvas/CanvasLayer.vue';
import CardComponent from '@/components/card/CardComponent.vue';
import InfoComponent from '@/components/common/InfoComponent.vue';
import TooltipComponent from '@/components/common/TooltipComponent.vue';
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
}
.company-list-row{
    border: 1px solid white;
    border-radius: 20px;
    width: 30%;
    padding: 2px;
    height: 70px;
}


</style>