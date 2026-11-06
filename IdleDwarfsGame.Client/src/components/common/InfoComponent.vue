<script lang="ts" setup>
import TooltipComponent from './TooltipComponent.vue';

// Значок «i» с подсказкой при наведении: title и text — её заголовок и текст (или слоты title / description).
// inline — стоит в строке (рядом с заголовком), иначе — в правом верхнем углу родителя
withDefaults(defineProps<{
  title?: string;
  text?: string;
  inline?: boolean;
}>(), {
  inline: false,
});
</script>

<template>
  <TooltipComponent class="info-tooltip" :class="{ 'info-tooltip-inline': inline }">
  <template v-if="title || $slots.title" #title><slot name="title">{{ title }}</slot></template>
  <template v-if="text || $slots.description" #description><slot name="description">{{ text }}</slot></template>
  <div class="info-outer">
    
    <div  :class="['info-inner', 'info-info']">
        i
    </div>
    
    <span class="sparkle sparkle-top"></span>
    <span class="sparkle sparkle-bottom"></span>
  </div>
  </TooltipComponent>
</template>

<style scoped>
.info-outer { 
  min-width: 20px; 
  height: 20px;
  border-radius: 10px; 
  background: var(--metalic-round-background);
  box-shadow: 0 2px 4px rgba(0,0,0,0.5);
  
  /* Центрирование текста внутри баджа */
  display: flex;
  justify-content: center;
  align-items: center;
}

.info-inner {
  width: 80%;
  height: 80%;
  color: white;
  font-family: Arial, sans-serif;
  font-size: 12px;
  font-weight: bold;
  line-height: 1; 
  border-radius: 50%;
  display: flex;
  justify-content: center;
  align-items: center;
  text-shadow: 1px 1px 1px #333, 0 0 1px black;
  user-select: none;
  pointer-events: none;
}

.info-error{
  background: var(--error);
}

.info-success{
  background: var(--success);
}

.info-info{
  background: var(--blue-gem-color, #2196f3);
}

.info-warning{
  background: var(--warning);
}

.sparkle{
    width: 5px;
    height: 5px;
    position: absolute;
    background: radial-gradient(circle at center, rgba(255, 255, 255, 0.9) 0%, transparent 60%);
    transition: all ease-in-out 0.3s;
    pointer-events: none;
}

.sparkle-top{
    top: 0;
    left: 65%;
}

.sparkle-bottom{
    bottom: 0;
    left: 5%;
}

.info-tooltip{
  position:absolute;
  top: 0;
  right: 0;
  z-index: 1000;
}

/* В строке рядом с заголовком: обычный поток, по центру строки */
.info-tooltip-inline {
  position: relative;
  top: auto;
  right: auto;
  z-index: auto;
  vertical-align: middle;
}

.info-outer {
  position: relative;
}

</style>
