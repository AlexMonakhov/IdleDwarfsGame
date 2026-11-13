<script lang="ts" setup>
import { computed, defineProps } from 'vue';

const props = defineProps<{
  currentHp: number;
  maxHp: number;
  attack?: number;
  armor?: number;
  energy?: number;
  maxEnergy?: number;
  // Цены заклинаний: на полоске энергии — метка, с какой энергии юнит кастует
  spellCosts?: number[];
}>();



const hpFillStyle = computed(() => ({
    width: `${props.maxHp ? Math.max(0, props.currentHp) / props.maxHp * 100 : 0}%`
}) )

const energyFillStyle = computed(() => ({
    width: `${props.maxEnergy ? Math.min(props.energy ?? 0, props.maxEnergy) / props.maxEnergy * 100 : 0}%`
}) )

// Метки каста: первая (самое дешёвое заклинание) — главная, на ней юнит начнёт кастовать.
// Цена выше запаса энергии недостижима — такую метку не ставим
const castMarks = computed(() => {
    const max = props.maxEnergy ?? 0;
    if (!max) return [];
    return [...new Set(props.spellCosts ?? [])]
        .filter(cost => cost > 0 && cost <= max)
        .sort((a, b) => a - b)
        .map((cost, index) => ({
            cost,
            main: index === 0,
            ready: (props.energy ?? 0) >= cost,
            left: `${cost / max * 100}%`,
        }));
});

// Энергии хватает на каст — на следующем ходу юнит применит заклинание
const castReady = computed(() => castMarks.value.some(mark => mark.ready));




</script>

<style lang="scss">
/* Металлическая рамка — как у InputComponent */
.stats-frame{
    background: var(--metalic-background);
    padding: 2px;
    border-radius: 6px;
    corner-shape: bevel;
    border: 0.7px solid rgba(0, 0, 0, 0.1);
    box-shadow:
        0 3px 6px rgba(0, 0, 0, 0.5),
        inset 0 1px 2px rgba(255, 255, 255, 0.8);
}

/* Вдавленная деревянная подложка под полосками */
.stats-inner{
    display: flex;
    flex-direction: column;
    gap: 2px;
    padding: 2px;
    border-radius: 4px;
    corner-shape: bevel;
    background: var(--wood-background);
    box-shadow: inset 0 0 4px black, inset 0 0 8px #333;
}

.hp{
    background: rgba(60, 20, 30, 0.85);
    height: 16px;
    color: whitesmoke;
    position: relative;
    border-radius: 3px;
    box-shadow: inset 0 1px 3px rgba(0, 0, 0, 0.8);
    overflow: hidden;
    &-fill{
        position: absolute;
        background: linear-gradient(45deg, transparent, #ff1d00);
        height: 100%;
        left: 0;
        bottom: 0;
        transition: width 0.8s;
    }
    &-numbers{
        position: absolute;
        background: transparent;
        line-height: 16px;
        font-size: 12px;
        font-weight: 700;
        width: 100%;
        height: 100%;
        left: 0;
        bottom: 0;
        text-align: center;
        text-shadow: 0 1px 2px #000, 0 0 2px #000;
    }
}

.energy-wrap{
    position: relative;
}

/* Метка каста: черта поперёк полоски энергии, чуть выступает за её края */
.energy-mark{
    position: absolute;
    top: -2px;
    bottom: -2px;
    width: 2px;
    border-radius: 1px;
    transform: translateX(-50%);
    background: rgba(226, 232, 240, 0.55);
    box-shadow: 0 0 2px #000;
    pointer-events: none;
    transition: background 0.3s, box-shadow 0.3s;
    &-main{
        background: #ffd166;
        box-shadow: 0 0 2px #000, 0 0 3px rgba(255, 209, 102, 0.8);
    }
    &-ready{
        background: #fff;
        box-shadow: 0 0 2px #000, 0 0 6px 1px #ffd166;
    }
}

/* Энергии хватает на каст — полоска разгорается */
.energy-ready .energy-fill{
    background: linear-gradient(90deg, #60a5fa, #e0f2fe);
    animation: energy-ready-pulse 0.9s ease-in-out infinite alternate;
}

@keyframes energy-ready-pulse{
    from { filter: brightness(1); }
    to { filter: brightness(1.5); }
}

@media (prefers-reduced-motion: reduce){
    .energy-ready .energy-fill{
        animation: none;
    }
}

.energy{
    background: rgba(15, 24, 48, 0.9);
    height: 5px;
    position: relative;
    border-radius: 3px;
    box-shadow: inset 0 1px 2px rgba(0, 0, 0, 0.8);
    overflow: hidden;
    &-fill{
        position: absolute;
        background: linear-gradient(90deg, #2563eb, #7dd3fc);
        height: 100%;
        left: 0;
        bottom: 0;
        transition: width 0.5s;
    }
}


</style>

<template>
    <div>
        <div class="icons">
            <div class="attack">{{ attack }}</div>
            <div class="armor">{{ armor }}</div>
        </div>
        <div class="stats-frame">
            <div class="stats-inner">
                <div class="hp">
                    <div class="hp-fill" :style="hpFillStyle"></div>
                    <div class="hp-numbers">{{ currentHp }}</div>
                </div>
                <div v-if="maxEnergy" class="energy-wrap" :class="{ 'energy-ready': castReady }">
                    <div class="energy">
                        <div class="energy-fill" :style="energyFillStyle"></div>
                    </div>
                    <span
                        v-for="mark in castMarks"
                        :key="mark.cost"
                        class="energy-mark"
                        :class="{ 'energy-mark-main': mark.main, 'energy-mark-ready': mark.ready }"
                        :style="{ left: mark.left }"
                    ></span>
                </div>
            </div>
        </div>
    </div>
</template>
