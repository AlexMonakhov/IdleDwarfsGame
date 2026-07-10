<script lang="ts" setup>

import { HeroInformation } from '@/models/types/Heroes';
import background from "@/assets/modal-background.png";

interface Props {
    playerHeroes?: HeroInformation[],
    enemyHeroes?: HeroInformation[]
}

const props = withDefaults(defineProps<Props>(), {
  playerHeroes: () => [],
  enemyHeroes: () => []
})

const backgroundStyle = `url(${background}) center / cover no-repeat, radial-gradient( #e1e8eb, #4c6a8d)`;
</script>

<template>
    <div class="list-item-wrapper">
            <div class="list-start"><slot name="list-start"></slot></div>
            <div class="list-middle" :style="{background: backgroundStyle}"><slot name="list-middle"></slot></div>
            <div class="list-end"><slot name="list-end"></slot></div>
    </div>
</template>

<style scoped>
.list-item-wrapper {
    position: relative;
    width: 100%;
    height: 100%;
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-radius: 15px;
    /* Убираем фон отсюда, переносим в after */
    background: transparent; 
    isolation: isolate; /* Создаем локальный слой для этого компонента */
}

/* СЛОЙ 1: Внешняя рамка и свечение (Металл) */
.list-item-wrapper::before {
    content: '';
    position: absolute;
    inset: -5px; /* Растягиваем чуть шире блока */
    background: var(--metalic-background);
    border-radius: 15px;
    box-shadow: 0 4px 3px 2px #181717, 0 3px 20px 4px #65a7b8;
    z-index: 1; /* Даем положительный индекс */
}

.list-item-wrapper:hover{
    /* Усиливаем свечение при наведении */
    transform: scale(1.01);
    box-shadow: 0 6px 5px 3px #181717, 0 5px 25px 5px #65a7b8;
}

.list-start, .list-middle, .list-end {
    position: relative;
    z-index: 3; /* Самый верхний слой */
    height: 100%;
    display: flex;
    justify-content: center;
    align-items: center;
 
}

.list-start {
    border-top-left-radius: 15px;
    border-bottom-left-radius: 15px;
    box-shadow: inset 0px 0px 6px black, inset 0px 0px 20px #333;
    font-size: 3em;
    font-weight: 900;
    font-family: RunesFont, sans-serif;
    text-shadow: 1px 1px 2px #000000;
}

.list-middle{
    flex:0.99;
    box-shadow: inset 0px 0px 6px black, inset 0px 0px 20px #333;
}

.list-end {
    border-top-right-radius: 15px;
    border-bottom-right-radius: 15px;
    box-shadow: inset 0px 0px 6px black, inset 0px 0px 20px #333;

}

.list-start, .list-end {
    min-width: 100px;
    background: var(--wood-background);
}

</style>
