<script lang="ts" setup>
import BadgeComponent from './BadgeComponent.vue';

interface Props {
  width?: string | null
  height?: string | null
  round?: boolean
  inner?: boolean
  badgeCount?: number
  showBadgeDotOnly?: boolean
  hoverColor?: string
  hoverColorLight?: string
}

const props = withDefaults(defineProps<Props>(), {
  width: null,
  height: null,
  round: false,
  badgeCount: 0,
  showBadgeDotOnly: false,
  hoverColor: '#222',
  hoverColorLight: '#eee',
  inner: false
})


</script>

<template>
    <button :class="['icon-button', { 'icon-button-square': !props.round, 'icon-button-round' : props.round }]" :style="{ width: props.width, height: props.height, '--hover-color': inner ? props.hoverColorLight : props.hoverColor}">
        <span v-if="props.inner" class="inner"></span>
        <span class="slot"><slot></slot></span>
        <BadgeComponent v-if="props.badgeCount > 0" :count="props.badgeCount" :show-dot-only="props.showBadgeDotOnly"></BadgeComponent>
    </button>
</template>

<style scoped>
.icon-button {
    border: 0.7px solid rgba(0, 0, 0, 0.1);
    box-shadow: 
        0 8px 15px rgba(0, 0, 0, 0.4), /* Внешняя тень */
        inset 0 1px 2px rgba(255, 255, 255, 0.8),
        0 0 2px 2px rgba(0,0,0, 0.3); /* Блик на самой рамке */
    cursor: pointer;
    display: inline-flex;
    justify-content: center;
    align-items: center;
    outline: none;
    transition:  filter 0.1s ease;
    padding: 10px;
    --opacityLeft: 1;
    --opacityRight: 1;
    position: relative;
}

.icon-button-square {
    border-radius: 15px;
    background: var(--metalic-background);
}

.icon-button-round {
    border-radius: 50%;
    background: var(--metalic-round-background);
}

.slot {
    z-index: 100;
    line-height: 0;
}

.inner{
    position: absolute;
    top: 4px; bottom: 4px;
    left: 4px; right: 4px;
    background: var(--wood-background);
    box-shadow: inset 0px 0px 6px black, inset 0px 0px 20px #333;
    border-radius: 12px;
    z-index: 10;
}

.icon-button-round .inner{
    border-radius: 50%;
}

.icon-button:hover {
    filter: drop-shadow(0 0 3px #eee);
}

.icon-button:hover :deep(svg path) {
    fill: var(--hover-color) !important;
}


.icon-button:active {
    box-shadow: inset 0px 0px 2px 1px rgba(0,0,0, 0.8);
    transform: scale(0.99);
}

</style>
