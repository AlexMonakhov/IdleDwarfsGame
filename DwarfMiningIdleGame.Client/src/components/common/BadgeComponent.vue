<script lang="ts" setup>
interface Props {
  type?: 'error' | 'success' | 'info' | 'warning'
  count?: number
  showDotOnly?: boolean 
}

const props = withDefaults(defineProps<Props>(), {
  type: 'error',
  count: 0,
  showDotOnly: false
})
</script>

<template>
  <div v-if="count > 0 || showDotOnly" class="badge-outer">
    <div v-if="count > 0 && !showDotOnly" :class="['badge-inner', `badge-${props.type}`]">
        {{ count > 99 ? '99+' : count }}
    </div>
    <span class="sparkle sparkle-top"></span>
    <span class="sparkle sparkle-bottom"></span>
  </div>
</template>

<style scoped>
.badge-outer {
  position: absolute; 
  top: 2px; 
  right: 2px; 
  z-index: 100; 
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

.badge-inner {
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
}

.badge-error{
  background: var(--error);
}

.badge-success{
  background: var(--success);
}

.badge-info{
  background: var(--info);
}

.badge-warning{
  background: var(--warning);
}

.sparkle{
    width: 5px;
    height: 5px;
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
    left: 5%;
}

</style>
