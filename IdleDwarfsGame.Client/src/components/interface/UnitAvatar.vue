<script lang="ts" setup>
import { computed } from 'vue';
import { PortraitUnit } from '@/battle/types';
import { elementColor, getPortraitUrl } from '@/battle/sprites';

// Только лицо юнита в кружке — без подсказки (её добавляет UnitPortrait)
interface Props {
    unit: Pick<PortraitUnit, 'name' | 'unitType' | 'element' | 'level' | 'isBoss'>;
    size?: number;
    showLevel?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
    size: 48,
    showLevel: true,
});

// Иконка — уже вырезанная с портрета голова (tools/portraits.js), растягивается на весь кружок
const color = computed(() => elementColor(props.unit.element));
const sprite = computed(() => getPortraitUrl(color.value, props.unit.level));

const portraitStyle = computed(() => ({
    width: `${props.size}px`,
    height: `${props.size}px`,
    backgroundImage: sprite.value ? `url(${sprite.value})` : 'none',
}));
</script>

<template>
    <div class="portrait" :class="[`portrait-${color}`, { 'portrait-boss': unit.isBoss }]" :style="portraitStyle">
        <span v-if="!sprite" class="portrait-letter">{{ unit.name[0] }}</span>
        <span v-if="showLevel" class="portrait-level">{{ unit.level }}</span>
    </div>
</template>

<style scoped>
.portrait {
    position: relative;
    flex-shrink: 0;
    box-sizing: border-box;
    border-radius: 50%;
    background-color: #1b2533;
    background-repeat: no-repeat;
    background-size: cover;
    background-position: center;
    border: 2px solid rgb(157, 173, 175);
    box-shadow: 0 0 0 2px #333, inset 0 0 8px black, 0 3px 6px rgba(0, 0, 0, 0.5);
    display: flex;
    justify-content: center;
    align-items: center;
}

.portrait-red {
    background-color: #3a1b1b;
}

.portrait-yellow {
    background-color: #3a321b;
}

.portrait-green {
    background-color: #1b3a24;
}

.portrait-basic {
    background-color: #2b2b2b;
}

.portrait-purple {
    background-color: #2a1b3a;
}

.portrait-white {
    background-color: #2b3440;
}

.portrait-boss {
    border-color: #ffd166;
    box-shadow: 0 0 0 2px #333, inset 0 0 8px black, 0 0 12px 3px #ff7b00;
}

.portrait-letter {
    font-weight: 900;
    font-size: 1.2em;
    color: #e2e8f0;
    text-shadow: 0 1px 3px #000;
}

.portrait-level {
    position: absolute;
    right: -4px;
    bottom: -4px;
    min-width: 16px;
    height: 16px;
    padding: 0 3px;
    box-sizing: border-box;
    border-radius: 8px;
    background: var(--wood-background);
    border: 1px solid rgb(157, 173, 175);
    font-size: 10px;
    font-weight: 700;
    line-height: 14px;
    text-align: center;
    color: #fff;
}
</style>
