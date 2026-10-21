<script lang="ts" setup>

import { onMounted, onBeforeUnmount, ref, getCurrentInstance, reactive, defineProps, watch, defineExpose, computed } from 'vue';
import { registerComponent, unregisterComponent } from '../ecs/entities/ComponentRegistry';
import type { Entity } from '../ecs/entities/Storage';
import { heroes } from '@/models/composables/heroes';
import { DwarfCharacterConfig, HeroModel } from '@/models/hero';
import characterStats from './character-stats.vue';
import { prop } from 'vue-class-component';

const props = defineProps<{
  entity: Entity;
  rotate?: boolean;
  characterConfig?:  DwarfCharacterConfig;
}>();

const width = ref('150');
const height = ref('130');
let model = ref<HeroModel | null>(null);
const leftHand = ref<SVGGElement | null>(null);
const dwarf = ref<SVGGElement | null>(null);
let showSlash = ref(false)
let damageTaken = ref(0);

const hit = () => {
  if (dwarf.value) {

    setTimeout(() => {
      showSlash.value = true;
      dwarf.value?.classList.add('hit-animation');
    }, 300)
    
    // Удалить класс после завершения анимации
    setTimeout(() => {
      dwarf.value?.classList.remove('hit-animation')
      showSlash.value = false;
    } , 1000);
  }
}

const attack = () => {
  if (leftHand.value) {
    leftHand.value.classList.add('attack-animation');
    //Удалить класс после завершения анимации
    setTimeout(() => leftHand.value?.classList.remove('attack-animation'), 500);
  }
}



onMounted(() => {
  registerComponent(props.entity, getCurrentInstance()!);
});

onBeforeUnmount(() => {
  unregisterComponent(props.entity);
});

watch(
  () => heroes.value,
  (newHeroes) => {
    const found = newHeroes.find(hero => hero.id === props.entity) ?? null;
    model.value = found;
  },
  { immediate: true, deep: true }
);

watch(
  () => model.value?.currentHp,
  (newHp, oldHp) => {
    damageTaken.value = (oldHp ?? 0) - (newHp ?? 0);
  },
);

watch(
  () => model.value?.currentHp,
  (value) => {
    if(value! <= 0) {     
      setTimeout(() => {
      dwarf.value?.classList.add('dead-state');
      }, 300)
    }
  }
);

defineExpose({ hit, attack });
</script>

<style lang="scss">
.hit-animation {
  animation: shake 0.5s ease;
}

@keyframes shake {
  0% { transform: rotate(0); }
  25% { transform: rotate(-10deg); }
  50% { transform: rotate(10deg); }
  75% { transform: rotate(-10deg); }
  100% { transform: rotate(0); }
}

@keyframes axe-swing {
  0% { transform: rotate(0deg);  }
  30% { transform: rotate(-55deg); }
  60% { transform: rotate(7deg); }
  100% { transform: rotate(0deg); }
}

.attack-animation {
  animation: axe-swing 0.8s cubic-bezier(1,-0.42,.42,-0.39);
  transform-box: fill-box;
  transform-origin:  0 80%;
}

.character-wrapper {
  position: relative;
  width: 110px;
}

.character-wrapper::before {
  content: "";
  position: absolute;
  bottom: 0px;
  left: 50%;
  transform: translateX(-50%);
  width: 130px;
  height: 14px;
  background: rgba(0, 0, 0, 0.4);
  border-radius: 50%;
  filter: blur(3px);
  z-index: -1;
}

.dwarf.dead-state:hover{
  filter: grayscale(0.8) drop-shadow(1px 1px 3px #615555);
}

.dwarf:hover{
  filter: drop-shadow(1px 1px 3px #fff);
}

.character-stats{
  position: relative;
}

.dead-state{
  filter: grayscale(0.8);
}

.rotate {
  transform: rotateY(180deg);
}

.slash-hit {
  position: absolute;
  top: 60%;
  left: 50%;
  width: 160px;
  height: 8px;
  background: white;
  transform: translate(-50%, -50%) rotate(-25deg);
  box-shadow: 0 0 10px white;
  opacity: 0;
  animation: slash-fade 0.4s ease-out forwards;
  border-radius: 4px;
  z-index: 5;
}

.starburst {
  position: absolute;
  z-index: 6;
  width: 50px; /* adjust to control the size */
  aspect-ratio: 1;
  background: rgb(255, 255, 249);
  clip-path: polygon(100% 50%,73.99% 57.04%,92.06% 77.03%,66.37% 68.89%,70.77% 95.48%,53.56% 74.75%,42.88% 99.49%,39.61% 72.74%,17.26% 87.79%,28.97% 63.52%,2.03% 64.09%,25% 50%,2.03% 35.91%,28.97% 36.48%,17.26% 12.21%,39.61% 27.26%,42.88% 0.51%,53.56% 25.25%,70.77% 4.52%,66.37% 31.11%,92.06% 22.97%,73.99% 42.96%,100% 50%,73.99% 57.04%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%);
  &-secondary{
    width: 30px;
    height: 30px;
    position: absolute;
    z-index: 7;
    left: 20%;
    top: 20%;
    text-align: center;
    background: rgb(255, 188, 19);
    clip-path: polygon(100% 50%,73.99% 57.04%,92.06% 77.03%,66.37% 68.89%,70.77% 95.48%,53.56% 74.75%,42.88% 99.49%,39.61% 72.74%,17.26% 87.79%,28.97% 63.52%,2.03% 64.09%,25% 50%,2.03% 35.91%,28.97% 36.48%,17.26% 12.21%,39.61% 27.26%,42.88% 0.51%,53.56% 25.25%,70.77% 4.52%,66.37% 31.11%,92.06% 22.97%,73.99% 42.96%,100% 50%,73.99% 57.04%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%,50% 50%); 
  }
}

.damage-number{
  width: 30px;
  height: 30px;
  position: absolute;
  z-index: 8;
  left: 20%;
  top: 20%;
  line-height: 2em;
  font-weight: 800;
  color: #ffffff;
  text-shadow: 1px 1px 2px #333;
  
}

.damage-box{
  position: absolute;
  z-index: 10;
  left: 20%;
  top: 45%;
  width: 50px;
  height: 50px;
  opacity: 0;
  animation: damage-fade 1s ease-in forwards;
}


@keyframes slash-fade {
  0% {
    opacity: 1;
    transform: translate(-50%, -50%) scale(1.2) rotate(-25deg);
  }
  100% {
    opacity: 0;
    transform: translate(-50%, -50%) scale(0.8) rotate(-25deg);
  }
}


@keyframes damage-fade {
  0% {
    opacity: 1;
  }
  100% {
    opacity: 0;
    transform: translateY(-50px);
  }
}

</style>

<template>


<div class="character-wrapper"  :class="{'rotate': rotate}" >
  <character-stats class="character-stats" :currentHp="model?.currentHp!" :maxHp="model?.maxHp!"  :class="{'rotate': rotate}"></character-stats>
  <div v-if="showSlash" class="damage-box">
    <div class="starburst-secondary" ></div>
    <div class="starburst"></div>
    <div class="damage-number" :class="{'rotate': rotate}">{{ damageTaken }}</div>   
  </div>
  <div v-if="showSlash" class="slash-hit" ></div>
  

    <svg  ref="dwarf" class="dwarf"  :width="width" :height="height" viewBox="0 0 1250 1024">
    <g id="dwarf" :fill="model?.config?.mainColor ?? '#fff'" :stroke="model?.config?.stroke ?? '#000'">
    <g id="leftHand" ref="leftHand" style="display:inline;stroke-width:5.4;stroke-dasharray:none;stroke-opacity:1">
      
      <g
      :style="{filter: `drop-shadow(4px 4px 25px ${model?.config?.crystalDarkColor}`}"
      id="sword"
      transform="translate(70,-40)">
        <path
          :fill="model?.config?.crystalLightColor"
          :stroke="model?.config?.stroke"
          style="fill-opacity:1;stroke-width:4;stroke-linecap:round;stroke-linejoin:round"
          d="m 821.53672,160.90608 -73.91215,283.87494 -15.04406,77.83668 -14.38997,-5.8868 5.23271,-22.23906 c 0,0 7.19499,-3.92453 -3.92453,-18.9686 -11.11953,-15.04406 -11.77362,-15.04406 -11.77362,-15.04406 l 88.30212,-323.77442 z"
          id="path18" />
        <path
          :fill="model?.config?.crystalMediumColor"
          :stroke="model?.config?.stroke"
          style="fill-opacity:1;stroke-width:4;stroke-linecap:round;stroke-linejoin:round"
          d="m 814.99582,549.43538 12.4277,-28.77995 c 0,0 -3.27045,-5.23271 11.11953,-14.38997 14.38997,-9.15726 16.35224,-10.46544 16.35224,-10.46544 l 97.45937,-327.69896 -49.71082,12.42771 -96.80528,321.81215 -5.23272,24.85541 z"
          id="path11" />
        <path
          :fill="model?.config?.crystalDarkColor"
          :stroke="model?.config?.stroke"
          style="fill-opacity:1;stroke-width:4;stroke-linecap:round;stroke-linejoin:round"
          d="m 953.66284,167.44697 c 0,0 -9.15725,-213.233252 -38.59129,-146.516098 -29.43404,66.717153 -29.43404,66.717153 -29.43404,66.717153 0,0 17.00633,88.302115 17.66042,89.610295 0.65409,1.30818 50.36491,-9.81135 50.36491,-9.81135 z"
          id="path12" />
        <path
          :fill="model?.config?.crystalLightColor"
          :stroke="model?.config?.stroke"
          style="fill-opacity:1;stroke-width:4;stroke-linecap:round;stroke-linejoin:round"
          d="m 883.67524,88.956205 c 0,0 -68.02533,69.333515 -63.4467,71.949875 4.57862,2.61636 -24.20132,-23.54723 -24.20132,-23.54723 0,0 5.8868,-25.5095 48.40264,-63.446709 42.51583,-37.937205 79.79895,-67.3712436 79.79895,-67.3712436 z"
          id="path17" />
        <path
          :fill="model?.config?.crystalMediumColor"
          :stroke="model?.config?.stroke"
          style="fill-opacity:1;stroke-width:4;stroke-linecap:round;stroke-linejoin:round"
          d="M 820.88263,158.94381 886.94569,89.610295 799.95175,406.84382 741.08368,456.55464 Z"
          id="path41" />
        <path
          :fill="model?.config?.crystalDarkColor"
          :stroke="model?.config?.stroke"
          style="fill-opacity:1;stroke-width:4;stroke-linecap:round;stroke-linejoin:round"
          d="m 887.59978,93.534833 -86.33985,313.308987 13.73589,70.64169 86.99393,-298.26492 z"
          id="path42" />
        <path
          :fill="model?.config?.accentColor"
          :stroke="model?.config?.stroke"
          style="fill-opacity:1;stroke-width:4;stroke-linecap:round;stroke-linejoin:round"
          d="m 710.99555,601.10847 -50.36491,195.57284 c 0,0 4.57863,11.11952 20.27678,15.69815 15.69815,4.57863 30.74222,7.84908 34.66676,3.92454 3.92454,-3.92454 8.50316,-25.5095 8.50316,-25.5095 l 56.90581,-180.52877 z"
          id="path43" />
        <path
          :fill="model?.config?.thirdColor"
          :stroke="model?.config?.stroke"
          style="fill-opacity:1;stroke-width:4;stroke-linecap:round;stroke-linejoin:round"
          d="m 659.32246,797.3354 c 0,0 -7.84908,-7.19499 -8.50317,1.30817 -0.65409,8.50317 -1.96227,16.35225 0,17.66043 1.96227,1.30818 16.35225,9.15725 26.16359,11.11952 9.81135,1.96227 38.5913,7.84908 43.16993,4.57863 4.57862,-3.27045 3.92453,-13.73588 3.92453,-13.73588 l -4.57862,-6.5409 z"
          id="path44" />
        <path
          :fill="model?.config?.accentColor"
          :stroke="model?.config?.stroke"
          style="fill-opacity:1;stroke-width:4;stroke-linecap:round;stroke-linejoin:round"
          d="m 649.51111,819.57445 -18.31451,18.9686 c 0,0 -13.73588,9.15726 5.23272,44.4781 18.9686,35.32085 18.9686,34.66676 18.9686,34.66676 0,0 20.93087,-7.19499 37.93721,-21.58496 17.00633,-14.38998 28.77994,-31.39631 28.77994,-31.39631 l -9.15725,-28.77995 c 0,0 -32.0504,-3.27045 -35.97494,-5.23272 -3.92454,-1.96227 -27.47177,-11.11952 -27.47177,-11.11952 z"
          id="path45" />
        <path
          :fill="model?.config?.secondaryCrystalLightColor"
          :stroke="model?.config?.secondaryCrystalStroke"
          style="fill-opacity:1;stroke-width:4;stroke-linecap:round;stroke-linejoin:round"
          d="m 644.00747,859.47392 27.08861,-21.58496 -8.50317,28.77995 z"
          id="path46" />
        <path
          :fill="model?.config?.secondaryCrystalMediumColor"
          :stroke="model?.config?.secondaryCrystalStroke"
          style="fill-opacity:1;stroke-width:4;stroke-linecap:round;stroke-linejoin:round"
          d="m 644.2784,858.81983 13.08179,45.13219 6.5409,-36.62902 z"
          id="path47" />
        <path
          :fill="model?.config?.accentColor"
          :stroke="model?.config?.stroke"
          style="fill-opacity:1;stroke-width:4;stroke-linecap:round;stroke-linejoin:round;filter:url(#filter97)"
          d="m 711.34237,594.32701 c 0,0 3.01313,-4.08325 -2.31256,-12.4878 -7.17927,-11.3297 -7.86269,-11.64214 -17.57543,-18.03795 -9.15743,-6.03014 -14.73545,-6.69299 -20.12605,-6.50802 -4.62511,0.1587 -13.17476,3.33054 -18.26239,5.18059 -5.08762,1.85004 -14.33785,10.57766 -14.33785,10.57766 0,0 -1.38753,1.85005 -2.31255,-5.08762 -0.92502,-6.93767 -2.31256,-28.21319 -2.31256,-28.21319 0,0 2.77507,-22.20054 5.55014,-27.75068 2.77506,-5.55014 10.63776,-23.58808 16.6504,-29.60072 6.01265,-6.01265 19.88799,-15.26288 23.58808,-14.80036 3.70009,0.46251 1.85004,30.98825 1.85004,30.98825 0,0 -0.92502,4.1626 3.23758,8.32521 4.16261,4.1626 18.03794,10.63776 23.12557,9.25022 5.08762,-1.38753 18.03794,-5.55013 21.73803,-10.17525 3.70009,-4.62511 10.63776,-11.10027 10.63776,-14.80036 0,-3.70009 0.46251,-8.78771 -0.46251,-15.72538 -0.92503,-6.93767 -2.31256,-18.50045 -2.31256,-18.50045 l 64.28907,-69.83921 22.66305,97.12737 -16.6504,11.10027 c 0,0 -4.62512,-0.46251 -5.55014,11.56279 -0.92502,12.02529 2.77507,19.42547 2.77507,19.42547 l 11.56278,15.26287 c 0,0 16.65041,8.78772 24.05059,8.78772 7.40018,0 16.18789,6.01264 28.21319,-12.0253 12.02529,-18.03794 14.33785,-15.72538 14.33785,-15.72538 0,0 6.47516,9.71274 8.3252,16.1879 1.85005,6.47515 5.08763,34.22583 3.70009,39.77597 -1.38753,5.55013 -6.01265,24.97561 -10.17525,33.30081 -4.1626,8.3252 -19.88798,22.66305 -25.43812,26.36314 -5.55013,3.70009 -18.03794,4.62512 -18.03794,4.62512 l 1.69134,-17.84637 c 0,0 0.89215,-1.16308 -1.85005,-12.60003 -1.90786,-7.95713 -21.30839,-14.06691 -25.00848,-13.6044 -3.70009,0.46251 -17.38385,1.49976 -22.47147,5.19985 -5.08763,3.70009 -6.01265,4.62511 -10.63776,10.63776 -4.62511,6.01264 -13.6305,17.82727 -18.20913,17.82727 -4.57863,0 -29.43404,-7.19499 -29.43404,-7.19499 l -25.5095,-7.19498 z"
          id="path97" />
        <path
          :fill="model?.config?.secondaryCrystalDarkColor"
          :stroke="model?.config?.secondaryCrystalStroke"
          style="fill-opacity:1;stroke-width:4;stroke-linecap:round;stroke-linejoin:round"
          d="m 671.09607,838.54305 20.27679,32.70449 -26.81768,-4.57863 z"
          id="path48" />
        <path
          :fill="model?.config?.secondaryCrystalDarkColor"
          :stroke="model?.config?.secondaryCrystalStroke"
          style="fill-opacity:1;stroke-width:4;stroke-linecap:round;stroke-linejoin:round"
          d="m 657.36019,903.95202 34.01267,-32.70448 -28.77995,-5.23272 z"
          id="path49" />
        <path
          :fill="model?.config?.secondaryCrystalLightColor"
          :stroke="model?.config?.secondaryCrystalStroke"
          style="fill-opacity:1;stroke-width:4;stroke-linecap:round;stroke-linejoin:round"
          d="m 752.85729,456.55464 37.93721,-28.77995 10.46543,43.16992 -26.36879,-6.51367 z"
          id="path50" />
        <path
          :fill="model?.config?.secondaryCrystalMediumColor"
          :stroke="model?.config?.secondaryCrystalStroke"
          style="fill-opacity:1;stroke-width:4;stroke-linecap:round;stroke-linejoin:round"
          d="m 774.50799,463.50592 -5.63512,40.73153 33.04115,-31.98466 z"
          id="path51" />
        <path
          :fill="model?.config?.secondaryCrystalDarkColor"
          :stroke="model?.config?.secondaryCrystalStroke"
          style="fill-opacity:1;stroke-width:4;stroke-linecap:round;stroke-linejoin:round;stroke-opacity:1"
          d="m 752.2032,457.20873 15.55306,47.68281 5.82671,-42.31064 z"
          id="path52" />
        <path
          :fill="model?.config?.secondaryCrystalMediumColor"
          :stroke="model?.config?.secondaryCrystalStroke"
          style="fill-opacity:1;stroke-width:2.90938;stroke-linecap:round;stroke-linejoin:round"
          id="path55-5"
          d="m -726.42298,-556.6782 a 31.17104,30.239841 0 0 1 -29.71466,31.5559 31.17104,30.239841 0 0 1 -32.56216,-28.79025 31.17104,30.239841 0 0 1 29.63893,-31.62287 31.17104,30.239841 0 0 1 32.63109,28.71671"
          transform="scale(-1)" />
        <path
          :fill="model?.config?.secondaryCrystalDarkColor"
          :stroke="model?.config?.secondaryCrystalStroke"
          style="fill-opacity:0;stroke-width:4.19502;stroke-linecap:round;stroke-linejoin:round"
          id="path55"
          d="m -714.92077,-557.04788 a 42.955708,45.621933 0 0 1 -40.94873,47.60744 42.955708,45.621933 0 0 1 -44.87276,-43.43498 42.955708,45.621933 0 0 1 40.84436,-47.70846 42.955708,45.621933 0 0 1 44.96776,43.32402"
          transform="scale(-1)" />
      </g>
      <path :fill="model?.config?.mainColor" :stroke="model?.config?.stroke" style="stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 655.3785,715.04246 c 0,0 -32.8383,1.38753 -33.30081,-6.01265 -0.46251,-7.40018 -10.63776,-61.05149 -10.63776,-61.05149 0,0 14.80036,2.77506 14.33785,-6.01265 -0.46251,-8.78771 -18.50045,-64.75158 -16.18789,-67.98916 2.31255,-3.23758 5.55013,-7.86269 20.813,-5.55013 15.26287,2.31255 49.95122,10.17525 49.95122,10.17525 0,0 -16.18789,6.01264 -22.20054,24.05058 -6.01265,18.03794 -12.02529,41.62602 -12.02529,61.97652 0,20.08065 9.25022,50.41373 9.25022,50.41373 z" id="path9"/>
      <path :fill="model?.config?.mainColor" :stroke="model?.config?.stroke" style="stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 612.82746,563.80127 -65.21409,-87.87715 -19.88799,37.92593 -4.62511,102.21499 c 0,0 -3.23758,24.5131 19.88799,40.701 23.12556,16.18789 82.78952,68.45167 79.08943,53.65131 -3.7001,-14.80037 -12.0253,-63.36405 -12.0253,-63.36405 0,0 1.38753,-2.77507 6.93767,-2.31256 5.55014,0.46251 8.32521,-7.86269 8.32521,-7.86269 0,0 -20.81302,-65.2141 -16.1879,-66.13912 4.62511,-0.92502 3.70009,-6.93766 3.70009,-6.93766 z" id="path10"/>
      <path :fill="model?.config?.thirdColor" :stroke="model?.config?.stroke" style="display:inline;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 718.19054,578.54238 c 0,0 -18.96861,-4.57863 -26.16359,-1.96227 -7.19499,2.61636 -30.59712,13.16114 -36.15689,33.76497 -5.55976,20.60383 -12.53993,49.67232 -5.34494,79.76045 7.19498,30.08813 12.99825,28.75054 22.7248,33.34097 12.03567,5.68021 23.28593,1.92541 34.4287,0.85529 10.21719,-0.98123 14.7635,-4.14898 12.80123,-6.4383 -1.96227,-2.28931 -16.0252,-13.40884 -19.29565,-28.4529 -3.27044,-15.04407 -7.52203,-26.16359 -3.92453,-48.40264 3.59749,-22.23905 16.35224,-62.79262 20.93087,-62.46557 z" id="path3"/>
      <g id="leftFist" :fill="model?.config?.skinColor" :stroke="model?.config?.stroke">
        <path style="display:inline;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 721.0551,573.05149 c 0,0 -19.88798,41.16351 -18.50045,76.31436 1.38754,35.15086 3.23758,44.40109 10.17525,53.65131 6.93767,9.25023 12.95032,18.50046 21.27552,21.73804 8.3252,3.23757 60.12647,21.73803 76.31436,18.03794 16.1879,-3.70009 22.20055,-2.77507 26.36315,-17.11292 4.1626,-14.33785 10.63776,-29.13821 5.08762,-32.8383 -5.55013,-3.70009 -2.77507,-1.38754 2.31256,-6.47516 5.08762,-5.08763 6.93767,-19.42548 7.40018,-23.12557 0.46251,-3.70009 3.70009,-14.80036 0,-18.03794 -3.70009,-3.23758 2.31256,-6.93767 2.31256,-6.93767 0,0 3.23757,-1.38753 8.3252,-11.10027 5.08762,-9.71273 11.10027,-16.6504 9.71274,-24.5131 -1.38754,-7.86269 -2.31256,-13.41282 -7.40018,-18.03794 -5.08763,-4.62511 -49.95122,-15.26287 -59.20145,-16.18789 -9.25023,-0.92502 -16.65041,-15.26287 -18.03794,-17.57543 -1.38753,-2.31256 -26.82565,-6.93767 -34.68834,-4.1626 -7.8627,2.77506 -22.66306,6.01264 -25.90064,12.95031 -3.23758,6.93767 -5.55014,13.41283 -5.55014,13.41283 z" id="path4"/>
        <path style="display:inline;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 851.48329,640.11563 -64.28907,-12.4878 18.50045,-17.11292 -9.25022,-41.62602 68.45167,17.11291 c 2.15839,-5.56187 4.77928,15.26288 6.93767,21.73804 z" id="path6"/>
        <path style="display:inline;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 716.42999,580.91419 c 0,0 0.78955,35.8843 7.72722,51.60968 6.93767,15.72538 8.46068,25.16719 14.47332,24.24217 6.01265,-0.92503 -5.64874,-3.06925 -5.64874,-3.06925 l -9.57727,-21.94886 c 0,0 -9.96043,-38.52391 -5.33532,-50.08669 4.62511,-11.56278 10.15201,-25.39163 15.23964,-30.01674 5.08762,-4.62511 29.51738,-7.88594 35.53002,-6.96092 6.01265,0.92502 12.75874,5.09725 22.93399,12.95994 10.17525,7.86269 7.106,-0.83603 12.65614,18.12693 5.55013,18.96296 2.65322,33.81943 -5.20948,41.21961 -7.86269,7.40018 -12.4878,10.63776 -18.03794,10.17525 -5.55013,-0.46251 -17.19227,-1.99913 -18.5798,-3.38666 -1.38754,-1.38753 -12.12391,-34.68436 -12.12391,-34.68436 l 3.87806,12.17039 c 0,0 1.85004,17.11292 11.10027,26.82566 9.25022,9.71273 -5.55014,25.43812 -5.55014,25.43812 0,0 -10.99767,-3.56462 -7.29758,31.58624 3.70009,35.15085 -28.51699,32.82069 -28.51699,32.82069 0,0 -6.34928,-58.41423 -2.91054,-38.04943 3.38666,20.05632 1.72421,37.93157 1.72421,37.93157 L 705.32973,699.31707 696.542,640.11563 Z" id="path5"/>
        <path style="display:inline;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 840.84553,691.45438 c 0,0 -47.17615,-2.77506 -69.37669,-14.33785 -22.20054,-11.56278 -18.03794,-13.87534 -18.03794,-13.87534 0,0 0.92502,-42.08852 19.42547,-36.53839 18.50045,5.55014 76.31436,16.65041 76.31436,16.65041 l 2.31256,25.43812 z" id="path7"/>
        <path style="display:inline;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 815.86992,740.48058 c 0,0 -41.16351,-1.38753 -52.72629,-12.48781 -11.56278,-11.10027 -20.81301,-6.93767 -22.20054,-21.27552 -1.38753,-14.33785 9.71274,-35.61336 9.71274,-35.61336 0,-8.92175 13.87534,14.79911 51.33875,18.96296 37.46328,4.16383 40.70099,6.93767 40.70099,6.93767 l -12.02529,38.38844 z" id="path8"/>
      </g>
      
    </g>
    <g id="body-full" style="display:inline;stroke-width:5.4;stroke-dasharray:none;stroke-opacity:1">
      <path :fill="model?.config?.secondColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 442.81876,924.88289 -3.27045,28.77995 c 0,0 44.4781,14.38998 51.67309,9.81135 7.19498,-4.57863 128.85568,-68.02533 130.81794,-75.87441 1.96227,-7.84908 0,-23.54723 0,-23.54723 l -39.89947,-13.0818 c 0,0 -39.24539,86.33985 -92.88075,78.49077 -53.63536,-7.84908 -46.44036,-4.57863 -46.44036,-4.57863 z" id="path113"/>
      <path :fill="model?.config?.mainColor" :stroke="model?.config?.stroke" style="display:inline;fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 211.92508,657.36019 c 0,0 -3.92454,13.73589 1.30818,18.31451 5.23271,4.57863 37.9372,20.93087 39.89947,16.35225 1.96227,-4.57863 22.23905,-11.77362 30.08813,-9.15726 7.84908,2.61636 35.97493,18.9686 35.97493,18.9686 l 68.02534,6.5409 196.22692,-11.77362 c 0,0 14.38997,-198.84328 -33.35858,-229.58549 -47.74855,-30.74222 -107.9248,-8.50317 -107.9248,-8.50317 0,0 -84.37758,17.00633 -105.30845,-18.9686 -20.93087,-35.97494 -31.39631,-41.20766 -30.74222,-68.67943 0.65409,-27.47177 -5.88681,-28.12586 -5.88681,-28.12586 0,0 -10.46543,0 -10.46543,9.81135 0,9.81135 9.15725,27.47177 -1.96227,41.86174 -11.11953,14.38998 -24.85541,28.12586 -37.93721,25.5095 -13.08179,-2.61635 -32.70449,-14.38997 -32.70449,-14.38997 l -17.66042,-18.9686 -7.19499,11.77361 z" id="body"/>     
      <g id="legs">
        <path :fill="model?.config?.mainColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 172.67969,931.42379 c -1.96227,15.69816 2.61636,16.35225 2.61636,16.35225 l 26.16359,-10.46544 c 0,0 34.01267,-7.84908 56.90581,0 22.89314,7.84908 47.74855,34.01267 47.74855,34.01267 l 26.81768,-10.46544 v -11.11952 c 0,0 -43.82401,-36.62903 -62.13853,-38.5913 -18.31451,-1.96227 -50.36491,-2.61636 -57.55989,2.61636 -7.19499,5.23272 -40.55357,17.66042 -40.55357,17.66042 z" id="path108"/>
        <path :fill="model?.config?.mainColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 289.76176,853.58711 14.38997,3.92454 c 0,0 43.16992,17.00633 47.74855,28.12586 4.57863,11.11953 19.62269,-18.9686 19.62269,-18.9686 l 2.61636,-84.37758 c 0,0 -20.27678,31.39631 -35.32085,43.16992 -15.04406,11.77362 -49.05672,28.12586 -49.05672,28.12586 z" id="path106"/>
        <path :fill="model?.config?.accentColor" :stroke="model?.config?.stroke" style="display:inline;fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 213.23326,678.29106 c 0,0 11.77361,24.85541 175.95014,30.74222 164.17652,5.88681 196.22692,-10.46544 196.22692,-10.46544 l 2.61636,49.71082 -23.54723,7.84908 c 0,0 22.89314,71.94987 17.00633,90.91848 -5.88681,18.9686 -60.17626,100.72982 -86.33985,96.15119 -26.16359,-4.57863 -123.62296,-59.52217 -122.96887,-79.14486 0.65409,-19.62269 5.88681,-104.00027 5.88681,-104.00027 0,0 -162.86834,-3.92454 -162.21426,-27.47177 0.65409,-23.54723 -2.61635,-54.28945 -2.61635,-54.28945 z" id="path98"/>
        <path :fill="model?.config?.mainColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 214.60524,741.86811 c 0,0 -37.92593,102.67751 -19.42547,107.30262 18.50045,4.62511 30.52574,-34.22583 30.52574,-34.22583 0,0 34.22584,15.72538 65.6766,-0.92503 31.45077,-16.6504 46.71364,-55.2701 46.71364,-55.2701 0,0 16.1879,7.16893 -38.38843,-0.23125 -54.57634,-7.40018 -85.10208,-16.65041 -85.10208,-16.65041 z" id="path103"/>
        <path :fill="model?.config?.secondColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 184.45331,878.44252 c 0,0 -13.73589,44.47811 -8.50317,51.67309 5.23272,7.19499 21.58496,-18.31451 54.94354,-19.62269 33.35858,-1.30818 41.20765,-7.84908 71.94987,15.69815 30.74222,23.54724 34.66676,20.93088 34.66676,20.93088 0,0 27.47177,-47.74856 24.85541,-54.28945 -2.61636,-6.5409 -37.28312,-25.5095 -37.28312,-25.5095 l -38.59129,-15.69816 c 0,0 -34.66676,14.38998 -52.32718,9.81135 -17.66043,-4.57863 -45.78628,-11.11953 -45.78628,-11.11953 z" id="path107"/>
        <path :fill="model?.config?.secondColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 170.06333,945.81377 c -13.73588,26.81768 -13.08179,31.3963 -13.08179,31.3963 0,0 138.66702,22.23906 142.59156,0 3.92454,-22.23905 -26.81768,-32.70448 -26.81768,-32.70448 0,0 -28.12586,-15.04407 -65.40897,-7.19499 -37.28312,7.84908 -37.28312,8.50317 -37.28312,8.50317 z" id="path109"/>
        <path :fill="model?.config?.secondColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 156.32745,980.48052 c 0,0 -7.84908,5.88681 -4.57863,17.00634 3.27045,11.11954 10.46544,18.31454 22.23905,20.27674 11.77362,1.9623 16.35224,5.2328 56.90581,3.9246 40.55356,-1.3082 68.02533,-5.2327 80.45304,-9.8114 12.4277,-4.5786 22.23905,-9.8113 22.89314,-15.04403 0.65409,-5.23272 -1.30818,-32.70449 0,-34.01267 1.30818,-1.30818 -32.70449,9.81135 -32.70449,9.81135 0,0 17.00633,5.8868 -17.00633,9.15725 -34.01267,3.27045 -128.20159,-1.30818 -128.20159,-1.30818 z" id="path110"/>
        <path :fill="model?.config?.mainColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 588.02668,752.2032 c 37.28311,74.56623 36.62902,73.91214 36.62902,73.91214 l -41.86174,-13.08179 -15.69815,-56.90581 z" id="path111"/>
      </g>
      <g id="rightHand" >
        <g id="handCrystals"  transform="translate(-170.06333,-168.75515)">
          <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 186.39205,628.55285 50.41373,66.60162 37.0009,-56.42638 z" id="path54"/>
          <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 25.900632,256.69377 12.487805,115.16531 64.289073,4.1626 z" id="path13" transform="translate(170.66667,180.84192)"/>
          <path :fill="model?.config?.crystalLightColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="M 127.19061,320.98284 26.363144,256.69377 102.215,374.63415 Z" id="path16" transform="translate(170.66667,180.84192)"/>
          <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 135.97832,425.51039 18.50045,-24.5131 -26.82565,-79.08943 -24.97561,53.1888 z" id="path15" transform="translate(170.66667,180.84192)"/>
          <path :fill="model?.config?.crystalDarkColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="M 38.850949,371.85908 110.5402,443.54833 135.97832,425.9729 102.67751,376.02168 Z" id="path14" transform="translate(170.66667,180.84192)"/>
        </g>
        <path :fill="model?.config?.thirdColor" :stroke="model?.config?.stroke" style="display:inline;fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 47.748551,554.99515 c 0,0 16.408355,-34.48879 27.200836,-33.18061 10.792481,1.30818 6.56715,5.91321 93.474733,36.62902 7.27372,2.57075 37.1546,8.74743 40.16078,-4.6115 2.35505,-10.46543 19.98261,-38.4286 15.7214,-55.80846 -3.83192,-15.62891 -40.4181,-32.68125 -40.4181,-32.68125 l 2.20033,-15.65565 -3.67684,13.67014 -22.48675,-2.87766 -33.12848,-12.78762 c 0,0 43.22039,-69.02008 45.5097,-66.40372 2.28932,2.61636 13.45533,63.49318 13.45533,63.49318 l -14.00681,-63.59577 c 0,0 75.81829,89.10527 72.54784,92.70277 -3.27045,3.59749 -14.38998,15.69815 -14.38998,15.69815 l 15.69816,-12.4277 c 0,0 10.79248,19.94973 7.52203,42.18879 -3.27045,22.23905 -11.44657,39.24538 -11.44657,39.24538 0,0 -12.10066,20.60383 -13.0818,16.67929 -0.98113,-3.92454 -8.17612,-19.62269 -8.17612,-19.62269 l 5.88681,20.27678 c 0,0 -10.46544,8.50316 -16.0252,10.46543 -5.55976,1.96227 -21.58496,6.5409 -28.4529,5.23272 -6.86794,-1.30818 -44.80515,-11.11952 -54.28945,-15.04406 -9.4843,-3.92454 -41.861744,-18.9686 -61.157391,-20.27678 -19.295648,-1.30818 -18.641558,-1.30818 -18.641558,-1.30818 z" id="path90"/>
        <path :fill="model?.config?.thirdColor" :stroke="model?.config?.stroke" style="display:inline;fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 54.210094,768.50896 c 0,0 -6.461543,14.43646 -1.228825,24.9019 5.232718,10.46543 8.503167,9.81134 8.503167,9.81134 0,0 16.352243,-17.66042 41.207654,-26.16359 24.85541,-8.50316 42.51583,-13.08179 46.44037,-13.73588 3.92454,-0.65409 38.5913,0.65409 41.20765,-0.65409 2.61636,-1.30818 0.3038,-25.93914 -6.89118,-23.97687 -7.19499,1.96227 -13.19214,-2.79127 -39.23176,1.49976 -14.34078,2.36319 -44.04102,5.83737 -45.178686,7.22785 z" id="path93"/>
        <path :fill="model?.config?.thirdColor" :stroke="model?.config?.stroke" style="display:inline;fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 49.056731,628.58024 c -6.540898,17.00634 3.158224,31.93818 3.158224,31.93818 0,0 9.354642,-8.6071 43.169923,-6.57377 36.308052,2.18321 58.246852,15.84325 66.366862,19.30528 7.53908,3.21435 28.15873,14.08617 29.46691,20.62707 1.30818,6.54089 17.43598,-16.24003 14.16553,-23.43501 -3.27045,-7.19499 -24.20132,-21.58497 -53.63536,-32.70449 -29.43404,-11.11953 -42.51583,-12.42771 -60.176256,-13.0818 -17.660423,-0.65409 -42.515833,3.92454 -42.515833,3.92454 z" id="path94"/>
        <path :fill="model?.config?.skinColor" :stroke="model?.config?.stroke" style="opacity:1;mix-blend-mode:normal;fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 56.905808,623.34752 c 0,0 17.006333,-68.02533 24.20132,-66.06306 7.194987,1.96227 90.264382,35.97493 102.038002,32.70448 11.77361,-3.27044 26.16359,-13.73587 49.05673,-19.62268 22.89314,-5.88681 28.12586,1.30817 28.12586,1.30817 L 216.50371,650.1652 204.076,664.55518 c 0,0 -36.62903,-24.20132 -60.17626,-32.70449 -23.54723,-8.50317 -38.47907,-6.49442 -43.0577,-7.1485 -4.578624,-0.65409 -43.936232,-1.35467 -43.936232,-1.35467 z" id="path96"/>
        <path :fill="model?.config?.skinColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 62.439024,808.00723 c 0,0 -4.625112,7.16892 -2.0813,24.97561 2.543812,17.80668 0.693767,17.11292 5.550135,24.74435 4.856369,7.63144 8.556459,16.41915 18.269196,20.11924 9.712737,3.70009 18.037945,6.47516 20.581755,4.39386 2.54381,-2.0813 4.85637,-3.93135 3.23758,-7.16893 -1.61879,-3.23757 -13.181575,-25.20686 -9.250229,-21.27551 3.931349,3.93134 8.787719,19.65673 19.425479,24.74435 10.63775,5.08762 21.96928,10.17525 29.60072,5.78139 7.63143,-4.39386 13.41283,-8.78771 10.63776,-10.4065 -2.77507,-1.61879 -16.41915,-15.26288 -16.88166,-21.27552 -0.46252,-6.01265 2.31255,-0.92503 2.31255,-0.92503 0,0 8.78772,19.42548 18.50045,22.20055 9.71274,2.77506 17.34418,8.78771 26.36315,1.15627 9.01897,-7.63143 11.33152,-12.95031 12.25655,-18.96296 0.92502,-6.01264 -1.61879,-24.74435 -1.61879,-24.74435 0,0 -3.00633,-3.70009 -6.70642,-3.93135 -3.70009,-0.23125 -4.1626,22.20054 -4.85637,12.95032 -0.69376,-9.25023 0.69377,-23.12557 2.08131,-21.50678 1.38753,1.61879 10.63776,15.49413 13.87533,14.33785 3.23758,-1.15628 10.17525,-3.70009 12.48781,-8.55646 2.31256,-4.85636 8.3252,-15.72538 7.40018,-17.57542 -0.92502,-1.85005 -2.31256,-14.33786 -7.86269,-21.27552 -5.55014,-6.93767 -15.03162,-19.42548 -28.44445,-19.65673 -13.41282,-0.23126 -42.55103,-1.15629 -69.37669,6.70641 -26.825652,7.86269 -59.663957,32.14453 -55.501356,35.15086 z" id="path95"/>
        <path :fill="model?.config?.mainColor" :stroke="model?.config?.stroke" style="display:inline;fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 74.949387,521.88426 c 0,0 33.840183,-80.20773 57.083763,-70.65132 20.86514,8.57854 21.916,12.50307 31.4003,13.15716 9.4843,0.65409 16.44123,0.34066 22.98213,2.30293 6.54089,1.96227 43.02482,25.3644 41.06255,37.13802 -1.96227,11.77361 3.74259,11.26462 -11.30147,42.00684 -15.04406,30.74222 -64.10079,5.88681 -64.10079,5.88681 z" id="path91"/>
        <path :fill="model?.config?.mainColor" :stroke="model?.config?.stroke" style="display:inline;fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 52.327179,663.247 c 0,0 -18.314512,47.09446 -13.735884,65.40897 4.578628,18.31452 5.232718,39.24539 13.081795,37.93721 7.849076,-1.30818 18.968602,-13.73589 55.59763,-22.89314 36.62902,-9.15726 75.22032,-5.88681 77.83668,-8.50317 2.61636,-2.61636 9.81134,-29.43404 5.23271,-35.97494 -4.57862,-6.54089 -6.54089,-17.00633 -34.01266,-26.81768 -27.47177,-9.81134 -45.78628,-18.31451 -66.717155,-17.00633 -20.930872,1.30818 -37.283116,7.84908 -37.283116,7.84908 z" id="path92"/>
      </g>
      <g id="wrist">
        <ellipse :fill="model?.config?.thirdColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" id="path99" cx="282.1319" cy="722.21136" rx="39.313461" ry="37.232159"/>
        <path :fill="model?.config?.mainColor" :stroke="model?.config?.stroke" style="display:inline;fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 407.93496,762.21861 c 0,3.70009 -4.62511,85.10208 -4.62511,85.10208 0,0 70.30171,61.97651 84.17705,58.27642 13.87534,-3.70009 45.32611,-11.10027 49.95122,-18.50045 4.62511,-7.40018 26.82566,-31.45077 24.97561,-43.47606 -1.85004,-12.0253 -15.72538,-83.25204 -21.27552,-86.0271 -5.55013,-2.77507 -123.95302,9.25022 -133.20325,4.62511 z" id="path102"/>
        <ellipse :fill="model?.config?.crystalLightColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" id="path100" cx="282.1319" cy="722.44263" rx="21.27552" ry="19.425474"/>
        <path :fill="model?.config?.thirdColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 200.7299,851.9458 c 0,0 26.82566,12.02529 67.52665,3.70009 40.70099,-8.3252 54.57633,-16.65041 69.37669,-31.45077 14.80036,-14.80036 37.0009,-41.85727 38.85095,-55.7326 1.85004,-13.87534 -35.15086,-8.09396 -35.15086,-8.09396 0,0 -20.11924,29.83198 -21.96928,34.45709 -1.85005,4.62511 -36.30714,26.13189 -36.30714,26.13189 0,0 -14.33785,3.23758 -26.5944,2.31256 -12.25655,-0.92502 -29.36947,-3.70009 -29.36947,-3.70009 0,0 -2.54381,6.2439 -8.3252,15.49413 -5.78139,9.25022 -7.6848,11.43265 -13.64408,14.56911 z" id="path104"/>
        <path :fill="model?.config?.thirdColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 625.30979,826.76943 c 0,0 18.31452,25.5095 14.38998,31.39631 -3.92454,5.88681 -16.35225,4.57863 -18.96861,3.92454 -2.61635,-0.65409 -37.9372,-13.0818 -37.9372,-13.0818 l -1.96227,-37.28311 z" id="path112"/>
      </g>
      <path :fill="model?.config?.mainColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 490.56731,966.74464 45.78628,7.19499 c 0,0 64.75488,-56.90581 78.49077,-52.98127 13.73588,3.92453 0.65409,-24.85541 0.65409,-24.85541 z" id="path114"/>
      <path :fill="model?.config?.secondColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 436.93195,955.62511 c 0,0 -11.11953,38.5913 -3.27045,41.20766 7.84908,2.61636 47.74855,11.11953 52.32718,9.15723 4.57863,-1.9622 9.81134,-8.50314 9.81134,-8.50314 l 9.81135,13.08184 c 0,0 1.96227,4.5786 42.51583,5.8868 40.55357,1.3081 100.72982,0 110.54117,-7.195 9.81135,-7.195 11.77362,-30.74225 9.15726,-37.28314 -2.61636,-6.5409 -36.62903,3.27044 -36.62903,3.27044 0,0 -9.81135,12.42771 -80.45304,1.96227 -70.64169,-10.46543 -113.81161,-21.58496 -113.81161,-21.58496 z" id="path115"/>
      <path :fill="model?.config?.secondColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 622.69343,920.30427 c 33.35858,22.89314 32.0504,18.9686 33.35858,22.23905 1.30818,3.27045 15.69815,28.12586 9.15726,30.74222 -6.5409,2.61636 -25.5095,9.15725 -51.67309,9.15725 -26.16359,0 -77.18259,-9.15725 -77.18259,-9.15725 0,0 58.86807,-45.78629 86.33984,-52.98127 z" id="path116"/>
    </g>
    <g id="beard" transform="translate(-170.06333,-168.75515)" style="filter: drop-shadow(2px 4px 6px black);">
      <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="display:inline;opacity:1;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 689.92602,587.32787 c 0,0 4.1626,-50.87624 8.32521,-48.56369 4.1626,2.31256 8.78771,27.75068 8.78771,27.75068 0,0 5.08762,17.57543 15.26287,22.66305 10.17525,5.08763 32.37579,7.8627 32.37579,7.8627 l -20.35049,19.88798 z" id="path19"/>
      <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="display:inline;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 577.6766,514.77506 68.91419,41.39477 -59.43271,8.09395 z" id="path2" transform="translate(170.06333,168.75515)"/>
      <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="display:inline;fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 456.35782,661.32968 c 21.73803,2.31256 43.47606,4.62511 65.21409,6.93767 13.71796,-6.41604 35.04046,-15.41498 48.30538,-22.45502 9.31309,-10.36457 25.82116,-25.63481 21.07132,-27.4962 l -18.96297,-18.96297 c -38.54261,20.65884 -77.08521,41.31768 -115.62782,61.97652 z" id="path58"/>
      <path :fill="model?.config?.crystalLightColor" :stroke="model?.config?.stroke" style="display:inline;fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 456.35782,660.40465 c 19.2713,-21.42969 38.54261,-42.85938 57.81391,-64.28907 18.03794,-1.38753 36.07588,-2.77507 54.11382,-4.1626 6.7024,7.56401 -1.10333,10.06985 -7.47104,13.07704 -34.8189,18.45821 -69.63779,36.91642 -104.45669,55.37463 z" id="path57"/>
      <path :fill="model?.config?.crystalLightColor" :stroke="model?.config?.stroke" style="display:inline;fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 667.26297,591.49047 c -1.52204,7.30348 -2.65072,14.40223 6.36445,16.22132 31.02516,16.79343 62.05032,33.58687 93.07548,50.38031 -15.06628,-15.37277 -29.24093,-31.8212 -44.86566,-46.52023 -11.52358,-6.91072 -22.58568,-15.60074 -34.39826,-21.39722 -6.72534,0.4386 -13.45067,0.87721 -20.17601,1.31582 z" id="path60"/>
      <path :fill="model?.config?.crystalLightColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 595.57372,622.47873 c -9.49616,10.72474 -23.35297,19.63462 -27.93169,33.5795 -5.44787,17.86444 -12.12662,35.59983 -16.81825,53.54357 l 2.10992,61.67009 45.8776,-44.72812 21.27552,-100.36495 c -8.17103,-1.23336 -16.34207,-2.46673 -24.5131,-3.70009 z" id="path62"/>
      <path :fill="model?.config?.crystalLightColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 623.3244,624.32877 c 6.93767,-0.92502 13.87533,-1.85005 20.813,-2.77507 20.19633,25.59229 40.39265,51.18459 60.58898,76.77688 l 4.09288,73.85671 c -14.96302,-18.90945 -37.68308,-32.093 -52.0918,-51.30992 -11.13435,-32.18286 -22.26871,-64.36573 -33.40306,-96.5486 z" id="path64"/>
      <path :fill="model?.config?.crystalDarkColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 551.10291,708.31425 c -17.73913,17.84673 -43.35513,35.6759 -60.46595,53.94091 -10.03885,25.0971 -20.07769,50.19421 -30.11654,75.29132 21.4085,-17.63976 45.06047,-32.75287 66.09027,-50.74402 7.23157,-11.23039 17.67703,-25.21133 23.72342,-36.76504 -1.05191,-14.12575 1.82071,-27.59742 0.7688,-41.72317 z" id="path67"/>
      <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 459.5954,837.08397 c 2.00422,-31.2966 4.00843,-62.5932 6.01265,-93.8898 12.95031,-22.97139 25.90063,-45.94279 38.85094,-68.91418 18.50045,-5.70431 46.81226,-19.91178 65.31271,-25.61609 -4.77928,17.88377 -16.75356,43.61662 -21.53284,61.50039 -17.74962,16.9214 -39.11113,33.79695 -56.23846,51.15587 -10.80167,25.2546 -21.60333,50.50921 -32.405,75.76381 z" id="path68"/>
      <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 680.91386,665.22135 c 7.63777,11.08305 20.96128,20.91624 27.5301,32.36088 0.30251,19.05825 0.60503,38.11649 0.90754,57.17473 26.36314,18.80879 52.72629,37.61758 79.08943,56.42637 -6.16682,-28.6757 -12.33363,-57.3514 -18.50045,-86.0271 -28.36736,-20.19632 -60.65926,-39.73856 -89.02662,-59.93488 z" id="path69"/>
      <path :fill="model?.config?.crystalLightColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 683.09095,663.99252 c 20.81301,3.08341 45.22351,8.12908 66.03652,11.21249 7.09184,14.80036 14.18368,29.60073 21.27552,44.40109 z" id="path70"/>
      <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 513.95292,668.69699 c -18.19211,-1.69588 -49.13897,-4.69993 -69.62039,-1.81718 -6.93496,8.7075 -16.51788,15.92808 -18.16433,27.60528 -5.35384,16.69888 -10.70767,33.39775 -16.06151,50.09662 8.08628,-9.97759 13.31051,-22.86831 25.26587,-28.90777 21.17876,-14.56883 57.40159,-32.40812 78.58036,-46.97695 z" id="path71"/>
      <path :fill="model?.config?.crystalDarkColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 408.71916,745.50673 c 17.57543,-2.00422 45.61629,-7.27888 63.19172,-9.2831 12.79615,-21.8922 15.12685,-40.51395 27.923,-62.40615 -24.19782,16.25518 -49.03201,31.81808 -72.83397,48.5039 -6.09358,7.72845 -12.18717,15.4569 -18.28075,23.18535 z" id="path72"/>
      <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 547.01003,761.23212 c -8.19379,10.31572 -14.54618,22.42078 -23.90326,31.6061 -4.45834,4.33032 -12.13069,7.45954 -14.72484,12.48652 5.78419,28.00767 11.56839,56.01533 17.35258,84.023 9.71274,-31.45077 19.42547,-62.90153 29.13821,-94.3523 -1.5417,-12.79615 -3.08341,-25.59229 -4.62511,-38.38844 -1.07919,1.54171 -2.15839,3.08341 -3.23758,4.62512 z" id="path73"/>
      <path :fill="model?.config?.crystalDarkColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 553.02268,786.20772 c 8.01686,18.03794 16.03373,36.07589 24.05059,54.11383 -17.11292,16.80458 -34.22584,33.60915 -51.33876,50.41373 8.98857,-30.44642 18.83181,-60.76638 27.28817,-91.29155 0,-4.41201 0,-8.82401 0,-13.23601 z" id="path74"/>
      <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 710.73903,769.55732 c -4.47094,23.27973 -14.50165,46.23243 -18.97259,69.51216 16.80458,17.57543 39.16891,35.4779 55.97349,53.05333 -1.69587,-34.68835 -1.10244,-74.93645 -2.79831,-109.6248 l -35.61337,-26.36314 c 1.23336,2.6209 0.17742,10.80155 1.41078,13.42245 z" id="path75"/>
      <path :fill="model?.config?.crystalDarkColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 747.93151,784.56287 c 1.23336,16.49624 1.81264,40.18746 3.046,56.6837 14.02951,11.87112 28.05902,23.74225 42.08853,35.61337 l -8.48391,-66.57838 c -11.56278,-6.93767 -25.08784,-18.78102 -36.65062,-25.71869 z" id="path76"/>
      <path :fill="model?.config?.crystalDarkColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 748.22569,834.96698 c 0.92502,17.88377 2.17709,48.52229 3.10211,66.40606 7.86269,7.40018 17.68766,14.80036 25.55035,22.20054 l -4.07363,-61.64947 c -6.01265,-4.62511 -18.56618,-22.33202 -24.57883,-26.95713 z" id="path77"/>
      <path :fill="model?.config?.crystalDarkColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 633.49964,788.98279 c -10.17525,16.80458 -20.35049,33.60915 -30.52574,50.41373 8.47937,13.25866 17.28579,38.94502 25.76516,52.20368 l 31.58624,-45.72852 c 11.71695,12.79615 23.43391,39.00113 35.15086,51.79728 l 18.50045,-31.44678 c -6.47516,-6.47516 -14.91258,-17.52895 -21.38774,-24.00411 -4.28932,5.23856 -4.049,12.05281 -8.07683,1.89901 -16.13201,-20.34037 -34.88039,-34.79392 -51.0124,-55.13429 z" id="path78"/>
      <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 577.59189,842.56836 -23.40613,25.12071 c 9.74561,14.72554 16.87486,33.37563 23.35002,49.40935 12.17946,-14.49202 30.24574,-32.90858 42.4252,-47.4006 -4.62511,-9.4044 -9.25023,-18.80879 -13.87534,-28.21319 -3.70009,6.16682 -13.28699,16.25817 -16.98708,22.42499 -4.1626,-4.93345 -7.34407,-16.40781 -11.50667,-21.34126 z" id="path79"/>
      <path :fill="model?.config?.crystalDarkColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 619.62431,875.4724 c -7.82529,7.65695 -16.95875,22.50889 -25.43813,33.76333 21.73803,19.88799 43.47607,39.77597 65.2141,59.66396 l 32.81505,-52.77841 c -9.86691,6.16682 -24.96652,13.96885 -34.83343,20.13567 -14.64619,-16.95875 -15.88354,-20.50865 -30.52973,-37.4674 1.48627,-5.99915 -0.60296,-12.93816 -4.35468,-18.72006 -0.95772,-1.53236 -1.91545,-3.06473 -2.87318,-4.59709 z" id="path80"/>
      <path :fill="model?.config?.crystalDarkColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 713.59345,861.8255 c -4.69029,5.56158 -10.68636,19.75511 -13.78459,25.83581 0.41898,6.56154 -2.3542,11.68047 -9.43888,16.04169 -5.7839,6.52226 -12.22188,14.02566 -18.00577,20.54792 22.35471,-4.1626 52.5585,-6.68999 74.91321,-10.85259 -0.56047,-6.40754 4.70677,-15.30909 -2.92594,-18.69002 -9.81662,-7.7995 -20.94141,-25.08331 -30.75803,-32.88281 z" id="path81"/>
      <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 428.57427,745.88026 c -10.02108,24.35893 -16.7717,52.96944 -26.79278,77.32837 l 61.005,-52.64694 c 0.15417,-8.17103 2.92471,-22.55591 3.07888,-30.72694 z" id="path82"/>
      <path :fill="model?.config?.crystalDarkColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 400.85646,823.67114 58.88403,1.67772 c 1.69588,-14.80036 3.39175,-40.72025 -0.14509,-54.86652 -19.57965,17.7296 -39.15929,35.4592 -58.73894,53.1888 z" id="path83"/>
      <path :fill="model?.config?.crystalDarkColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 633.03713,782.50763 c -14.49202,23.27974 -29.63813,56.69786 -44.13015,79.9776 -10.9461,-27.59651 -36.28217,-68.60185 -32.18421,-92.92791 l 42.55104,-43.01355 z" id="path65"/>
      <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 633.92928,786.18448 54.1467,61.07474 c 5.39596,-23.27974 14.38942,-46.88651 19.78538,-70.16625 l -52.16117,-56.09934 z" id="path66"/>
      <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 619.62431,626.17882 c -6.62933,32.99247 -13.25866,65.98494 -19.88799,98.97741 11.25444,18.65462 22.50888,37.30925 33.76332,55.96387 6.93767,-19.88799 13.87534,-39.77597 20.81301,-59.66396 -10.32942,-32.37579 -20.65883,-64.75158 -30.98825,-97.12737 -1.23336,0.61668 -2.46673,1.23337 -3.70009,1.85005 z" id="path63"/>
      <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="display:inline;fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 664.02539,605.36581 -14.33785,15.72538 c 10.9461,12.33364 19.27584,29.57294 30.22194,41.90658 l 87.71844,-2.59312 C 733.09374,642.05837 698.55957,623.71209 664.02539,605.36581 Z" id="path61"/>
      <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 317.88761,393.10793 28.77996,32.0504 -24.85541,21.58496 -49.05674,7.19499 50.36492,-4.57863 -38.5913,47.74855 -26.81768,17.66042 -15.04407,-57.55989 z" id="path97" transform="translate(170.06333,168.75515)"/>
      <g id="beard-crystal" transform="translate(170.06333,168.75515)">
        <path :fill="model?.config?.secondaryCrystalLightColor" :stroke="model?.config?.secondaryCrystalStroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 441.51058,839.85123 32.70448,-53.9624 14.71703,11.44656 -20.60383,51.01901 z" id="path87"/>
        <path :fill="model?.config?.secondaryCrystalMediumColor" :stroke="model?.config?.secondaryCrystalStroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 440.85649,842.46759 45.78628,90.91847 v -51.67309 l 26.81768,-37.28311 -24.52836,-48.72969 -18.64157,52.65423 z" id="path88"/>
        <path :fill="model?.config?.secondaryCrystalDarkColor" :stroke="model?.config?.secondaryCrystalStroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 488.93209,794.39199 11.44656,-9.4843 36.62903,54.94354 -50.36491,98.76755 0.65409,-58.21399 27.47177,-35.97493 z" id="path89"/>
        <path :fill="model?.config?.secondaryCrystalDarkColor" :stroke="model?.config?.secondaryCrystalStroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 490.89436,678.94515 33.03152,48.40264 -29.43404,6.5409 z" id="path86"/>
        <path :fill="model?.config?.secondaryCrystalMediumColor" :stroke="model?.config?.secondaryCrystalStroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 453.93828,728.00188 37.28312,44.47811 33.35857,-43.16993 -30.74221,5.88681 z" id="path85"/>
        <path :fill="model?.config?.secondaryCrystalLightColor" :stroke="model?.config?.secondaryCrystalStroke" style="fill-rule:nonzero;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1;paint-order:normal" d="m 454.59237,727.34779 35.6479,-47.4215 4.25157,53.30831 z" id="path84"/>
      </g>
    </g>
    <g id="face">
      <path :style="{fill: model?.config?.faceColor}" style="opacity:1;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 309.88256,340.40831 c -6.01028,6.41096 -3.70009,37.92593 -1.38753,44.8636 2.31255,6.93767 29.60073,38.38843 37.00091,39.77596 7.40018,1.38754 49.95121,-1.85004 51.80126,-3.23757 1.85004,-1.38754 0.10626,-13.04643 10.17525,-18.50045 11.10027,-6.01265 15.72538,-15.72539 17.11291,-11.56279 1.38754,4.1626 -9.25022,8.32521 -14.33785,11.10027 -5.08762,2.77507 -12.95031,15.72539 -9.71273,21.27552 3.23758,5.55014 5.55014,19.42548 19.42547,25.43813 13.87534,6.01264 42.55104,12.02529 57.3514,3.70008 16.36959,-9.20789 18.03795,-15.26286 19.88799,-20.813 1.85005,-5.55014 3.23758,-16.65041 -3.23758,-24.5131 -6.47516,-7.86269 -23.58808,-18.50045 -27.28817,-25.43812 -3.70009,-6.93767 -0.87403,-6.95355 -2.77506,-3.23758 0.46251,6.93767 29.13821,26.36314 29.13821,26.36314 0,0 7.86269,12.48781 7.86269,15.72539 0,3.23758 18.03794,1.85004 19.88799,-0.92503 1.85004,-2.77506 -3.23758,-41.1635 6.93767,-43.47606 10.17525,-2.31255 16.18789,-4.1626 17.57542,-15.26287 1.38754,-11.10027 5.11169,-14.12135 -0.92502,-18.50045 -33.58652,-24.36403 -49.21132,-18.92661 -72.38302,-20.11924 -5.26647,-0.27106 -155.17254,9.94399 -162.11021,17.34417 z" id="path20"/>
      <ellipse style="display:inline;stroke-width:5.4;stroke-dasharray:none;stroke-opacity:1;opacity:1;stroke-linecap:round;stroke-linejoin:round" id="path37" cx="395.90967" cy="381.10931" rx="18.500452" ry="16.650406"/>
      <circle style="display:inline;stroke-width:5.4;stroke-dasharray:none;stroke-opacity:1;opacity:1;stroke-linecap:round;stroke-linejoin:round" id="path38" cx="495.8121" cy="379.25925" r="15.725384"/>
      <ellipse style="display:inline;stroke-width:5.4;stroke-dasharray:none;stroke-opacity:1;opacity:1;fill:#000000;fill-opacity:0.963928;stroke-linecap:round;stroke-linejoin:round" id="path39" cx="398.22223" cy="382.49683" rx="10.63776" ry="9.7127371"/>
      <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="display:inline;stroke-width:5.4;stroke-dasharray:none;stroke-opacity:1;opacity:1;stroke-linecap:round;stroke-linejoin:round" d="m 461.58627,364.4589 51.33875,-37.46342 43.93857,30.06324 -99.90243,23.12557 z" id="path22"/>
      <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="display:inline;stroke-width:5.4;stroke-dasharray:none;stroke-opacity:1;opacity:1;stroke-linecap:round;stroke-linejoin:round" d="m 359.37127,323.29539 -39.31345,33.76333 100.82746,21.73803 13.41282,-16.1879 -72.15176,-46.25113 z" id="path21"/>
      <ellipse style="display:inline;stroke-width:5.4;stroke-dasharray:none;stroke-opacity:1;opacity:1;fill:#000000;fill-opacity:0.963928;stroke-linecap:round;stroke-linejoin:round" id="path40" cx="499.9747" cy="381.10931" rx="9.7127371" ry="9.250226"/>
    </g>
    <g id="helm" style="stroke-width:5.4;stroke-dasharray:none;stroke-opacity:1">
      <path :fill="model?.config?.thirdColor"  style="opacity:1;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 217.81189,344.7053 c 0,0 -54.28945,-17.66043 -49.05674,-5.88681 5.23272,11.77361 10.46544,33.35857 26.81768,47.74855 16.35225,14.38997 18.9686,25.5095 40.55357,29.43404 21.58496,3.92454 20.93087,5.88681 30.08813,-0.65409 9.15725,-6.5409 25.5095,-22.89314 25.5095,-28.77995 0,-5.88681 -2.61636,-107.92481 -2.61636,-107.92481 l -132.12613,-140.62929 4.57863,158.9438 30.08813,27.47177 -5.88681,-112.50343 69.33351,82.4153 -1.30818,48.40265 c 0,0 3.92454,29.43404 -7.19499,34.01267 -11.11953,4.57862 -11.77361,21.58495 -11.77361,21.58495 l -45.13219,-45.13219 c 0,0 -5.88681,-6.5409 4.57863,-7.19499 10.46543,-0.65409 23.54723,-1.30817 23.54723,-1.30817 z" id="path23"/>
      <path :fill="model?.config?.mainColor" :stroke="model?.config?.stroke" style="opacity:1;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 187.72376,217.15779 4.57862,108.5789 24.85542,17.00634 10.46543,-13.73589 23.54723,34.66675 3.27045,-69.9876 z" id="path24"/>
      <path :fill="model?.config?.crystalLightColor" :stroke="model?.config?.stroke" style="opacity:1;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 226.96914,325.73669 -18.31451,22.89314 h -21.58496 l 45.78628,51.019 -9.81135,-39.24538 z" id="path25"/>
      <path :fill="model?.config?.crystalDarkColor" :stroke="model?.config?.stroke" style="opacity:1;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 229.5855,325.0826 25.5095,39.24539 -21.58496,35.32084 -9.15726,-41.20765 z" id="path26"/>
      <path :fill="model?.config?.thirdColor" :stroke="model?.config?.stroke" style="display:inline;opacity:1;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 290.41584,298.91901 1.96228,54.28946 c 0,0 38.71032,-21.1882 67.49027,-25.11274 28.77995,-3.92453 84.25855,-4.32131 84.91264,-8.89993 0.33352,-2.33465 -20.27679,-40.55357 -20.27679,-40.55357 0,0 -13.08179,-5.23272 -41.86174,-0.65409 -28.77995,4.57863 -92.22666,12.4277 -92.22666,20.93087 z" id="path31"/>
      <path :fill="model?.config?.thirdColor" :stroke="model?.config?.stroke" style="display:inline;opacity:1;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 484.02641,320.50398 c 0,0 11.11952,-36.62903 16.35224,-34.01267 5.23272,2.61636 49.05673,15.04406 49.71082,34.66675 0.65409,19.62269 -1.96227,21.58496 -1.96227,21.58496 0,0 -34.66675,-20.93086 -64.10079,-22.23904 z" id="path32"/>
      <path :fill="model?.config?.mainColor" :stroke="model?.config?.stroke" style="opacity:1;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 517.38499,253.78682 c 0,0 12.4277,32.0504 13.08179,37.28312 0.65409,5.23271 15.69815,-22.89315 15.69815,-22.89315 l 3.27045,45.78629 c 0,0 -36.62902,-28.12586 -48.40264,-31.39631 -11.77361,-3.27045 16.35225,-28.77995 16.35225,-28.77995 z" id="path33"/>
      <path :fill="model?.config?.mainColor" :stroke="model?.config?.stroke" style="opacity:1;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 291.72403,298.26492 -1.30818,-18.31451 -33.35858,-37.93721 c 0,0 37.9372,-23.54723 61.48443,-32.05039 23.54724,-8.50317 94.18893,-11.77362 95.49711,-7.19499 1.30818,4.57863 -13.73589,35.32085 -13.73589,35.32085 l 24.20132,39.89947 c 0,0 -26.16359,-2.61636 -48.40264,0.65409 -22.23905,3.27045 -84.37757,19.62269 -84.37757,19.62269 z" id="path34"/>
      <path :fill="model?.config?.mainColor" :stroke="model?.config?.stroke" style="opacity:1;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 256.40318,167.44697 c 0,0 -48.40264,15.04407 -36.62903,31.39631 11.77362,16.35225 34.01267,40.55357 34.01267,40.55357 l 52.32718,-25.5095 c 0,0 -16.35224,-28.77995 -9.81134,-33.35858 6.54089,-4.57863 56.25171,-14.38998 60.17625,-22.89314 3.92454,-8.50317 12.4277,-37.28312 12.4277,-37.28312 L 307.42218,44.478102 217.81188,146.5161 Z" id="path35"/>
      <path :fill="model?.config?.secondColor" :stroke="model?.config?.stroke" style="opacity:1;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 293.68629,181.18286 c 0,0 5.88681,31.39631 12.42771,32.70449 6.5409,1.30818 85.03167,-15.04407 98.76755,-14.38998 13.73589,0.65409 -7.19499,-39.89947 -7.19499,-39.89947 0,0 -4.57862,-6.5409 -38.59129,1.30818 -34.01267,7.84907 -65.40898,20.27678 -65.40898,20.27678 z" id="path36"/>
      <path :fill="model?.config?.mainColor" :stroke="model?.config?.stroke" style="display:inline;opacity:1;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 463.09554,355.82482 -64.75489,-117.08206 85.03167,-150.440645 28.77995,162.214255 -38.5913,-54.28945 -34.66675,57.5599 42.51583,71.94987 z" id="path27"/>
      <path :fill="model?.config?.crystalLightColor" :stroke="model?.config?.stroke" style="opacity:1;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 472.90688,190.34012 -35.97494,62.79261 44.47811,24.20132 z" id="path28"/>
      <path :fill="model?.config?.crystalDarkColor" :stroke="model?.config?.stroke" style="opacity:1;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 474.21506,191.64829 39.89948,58.21399 -32.0504,27.47177 z" id="path29"/>
      <path :fill="model?.config?.crystalMediumColor" :stroke="model?.config?.stroke" style="display:inline;opacity:1;;stroke-width:5.4;stroke-linecap:round;stroke-linejoin:round;stroke-dasharray:none;stroke-opacity:1" d="m 440.85648,255.095 41.86175,69.33352 31.39631,-72.60397 -32.0504,27.47177 z" id="path30"/>
    </g>
  </g>
</svg>
</div>
</template>