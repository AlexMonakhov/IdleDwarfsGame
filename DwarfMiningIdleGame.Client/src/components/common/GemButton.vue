<script lang="ts" setup>
import { computed } from 'vue';
import BadgeComponent from './BadgeComponent.vue';


interface Props {
  width?: string | null
  height?: string | null
  color?: 'pink' | 'green' | 'blue' | 'yellow' | 'red'
  round?: boolean
  badgeCount?: number
  showBadgeDotOnly?: boolean
}


const props = withDefaults(defineProps<Props>(), {
  width: null,
  height: null,
  color: 'pink',
  round: false,
  badgeCount: 0,
  showBadgeDotOnly: false
})

const buttonClasses = computed<string[]>(() => {
    let classes =  [];
    if(props.round){
        classes.push('gem-button-round')
    }
    else{
        classes.push('gem-button-bevel')
    }
    return classes;
})
</script>

<style scoped>
.gem-button {
  position: relative;
  z-index: 1;
  background: var(--metalic-background);
  padding: 2px; /* Толщина рамки */
  border: 0.7px solid rgba(0, 0, 0, 0.1);
  box-shadow: 
    0 8px 15px rgba(0, 0, 0, 0.4), /* Внешняя тень */
    inset 0 1px 2px rgba(255, 255, 255, 0.8); /* Блик на самой рамке */
  cursor: pointer;
  display: inline-block;
  outline: none;
  transition: transform 0.1s ease, filter 0.2s ease;
  min-height: 40px;
  min-width: 40px;
}

.gem-button-bevel {
    border-radius: 8px; 
    corner-shape: bevel;
}

.gem-button-bevel .gem-inner{
    border-radius: 6px;
    corner-shape: bevel;
}

.gem-button-bevel .gem-inner-top{
    border-radius: 4px;
    corner-shape: bevel;
}

.gem-button-round {
    border-radius: 50%;
}

.gem-button-round .gem-inner{
    border-radius: 50%;

}

.gem-button-round .gem-inner-top{
    border-radius: 50%;
}

.gem-button-round {

}

.gem-button:active .gem-inner {
    transform: scale(0.99) ; 
    box-shadow: 0 0 2px rgba(255, 255, 255, 0.5), inset 0.5px 0px 1px 0.5px #555;
    z-index: 10;
}

.gem-button:hover {
  filter: brightness(1.1);
  z-index: 10;
}

/* 2. Сам кристалл (внутренний контейнер) */
.gem-inner {
    display: flex;
    align-items: center;
    justify-content: center;
    position: relative;
    overflow: hidden;
    padding: 5px;
    box-shadow:  inset 0 0 2px rgba(255, 255, 255, 0.5), 0px 0.5px 1px 0.5px #555;
    height: calc(100% - 10px);
}

/* 3. Лучи света в центре кристалла (псевдоэлемент) */
.gem-inner::after {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    height: 100%;
    width: 100%;    
    background:  
        linear-gradient( 45deg , 
        rgba(0,0,0,0.3) 0%,
        rgba(0,0,0,0.1) 25%,
        rgba(255,255,255,0.3) 30%,
        rgba(255,255,255,0.2) 40%,
        rgba(0,0,0,0.1) 45%,
        rgba(0,0,0,0.1) 65%,
        rgba(255,255,255,0.3) 70%,
        rgba(255,255,255,0.2) 75%,
        rgba(0,0,0,0.1) 80%,
        rgba(0,0,0,0.3) 100%);

    pointer-events: none; /* Чтобы блики не мешали кликать на текст */
    transition: all ease-out 0.2s;
}



.gem-inner:hover::after{
    width:110%;
}

.gem-inner:hover .gem-inner-top::before{
    width:110%;
}

.gem-inner:hover .sparkle-top{
    left:60%;
}

.gem-inner:hover .sparkle-bottom{
    left:20%;
}



/* 4. Текст на кнопке */
.gem-text {
  position: relative;
  z-index: 2;
  color: #ffffff;
  font-family: 'Arial', sans-serif;
  font-weight: bold;
  font-size: 16px;
  text-transform: uppercase;
  letter-spacing: 1px;
  text-shadow: 1px 1px 0px rgba(0,0,0,0.6);
  text-overflow: ellipsis;
  overflow: hidden;
  user-select: none;
}

/* 5. Вариации цветов для разных кристаллов */
.blue-gem {
  background-color: #0077ff;
}

.green-gem {
  background-color: #00c853;
}

.pink-gem {
  background-color: #e91e63;
}

.red-gem {
    background-color: #d40808;
}

/* Можно добавить и золотой/желтый кристалл */
.yellow-gem {
  background-color: #ffb300;
}

.gem-inner-top {
    position: relative; 
    overflow: hidden;
    padding: 5px 10px;
    box-shadow: 0 0 2px rgba(255, 255, 255, 1);
    width: calc(100% - 15px);
    height: calc(100% - 10px);
    display: flex;
    justify-content: center;
    align-items: center;
}

.gem-inner-top::before {
    position: absolute;
    width: 100%;
    height: 100%;
    content: '';
    left:0;
    top:0;
    pointer-events: none; 
    transition: all ease-out 0.2s;
}

.sparkle{
    width: 10px;
    height: 10px;
    position: absolute;
    background: radial-gradient(circle at center, rgba(255, 255, 255, 0.9) 0%, transparent 60%);
    transition: all ease-in-out 0.3s;
}

.sparkle-top{
    top: 0;
    left: 65%;
}

.sparkle-bottom{
    bottom: 0;
    left: 15%;
}

</style>

<template>
    <button class="gem-button" :class="buttonClasses" :style="{'width': width ?? '', 'height': height ?? ''}">
        <span :class="['gem-inner', `${props.color.toString()}-gem`]">
            <span class="gem-inner-top"><span class="gem-text"><slot></slot></span></span>
            <span class="sparkle sparkle-top"></span>
            <span class="sparkle sparkle-bottom"></span>    
        </span>
        <BadgeComponent v-if="props.badgeCount > 0" :count="props.badgeCount" :show-dot-only="props.showBadgeDotOnly"></BadgeComponent>
    </button>
</template>