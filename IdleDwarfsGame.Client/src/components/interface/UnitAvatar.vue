<script lang="ts" setup>
import { computed } from 'vue';
import { PortraitUnit } from '@/battle/types';
import { elementColor, getSpriteUrl, SPRITE_COLUMNS } from '@/battle/sprites';

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

// Персонаж занимает центр кадра (~25-75% по обеим осям) — приближаем эту область в 2 раза
const ZOOM = 2;
const CROP_LEFT = 0.25;
const CROP_TOP = 0.2;

const color = computed(() => elementColor(props.unit.element));
const sprite = computed(() => getSpriteUrl('idle', color.value, props.unit.level, 1, props.unit.unitType));

const portraitStyle = computed(() => {
    const frame = props.size * ZOOM;
    return {
        width: `${props.size}px`,
        height: `${props.size}px`,
        backgroundImage: sprite.value ? `url(${sprite.value})` : 'none',
        backgroundSize: `${frame * SPRITE_COLUMNS}px auto`,
        backgroundPosition: `${-frame * CROP_LEFT}px ${-frame * CROP_TOP}px`,
    };
});
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
