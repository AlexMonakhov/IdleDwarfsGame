<script lang="ts" setup>
import { computed } from 'vue';
import AvatarFrame from '@/components/account/AvatarFrame.vue';
import { useAccount } from '@/composables/UseAccount';
import type { DelveDwarfInfo } from '@/models/types/Social';

// Где отряд на карте спуска: аватар игрока с рамкой, выбранной в настройках аккаунта, в «дышащем» зелёном ореоле.
// Рядом — сколько дворфов отряда ещё живы
const props = defineProps<{ dwarves: DelveDwarfInfo[] }>();

const { selectedFrame } = useAccount();
const alive = computed(() => props.dwarves.filter(d => d.hp > 0).length);
</script>

<template>
    <span class="squad-marker">
        <span class="squad-marker-glow" aria-hidden="true"></span>
        <AvatarFrame :effect="selectedFrame" :size="50" />
        <span class="squad-marker-count">{{ alive }}</span>
    </span>
</template>

<style scoped>
.squad-marker {
    position: relative;
    display: inline-grid;
    place-items: center;
}

/* Ореол под рамкой: отмечает «вы здесь», не перекрывая эффект рамки */
.squad-marker-glow {
    position: absolute;
    inset: -4px;
    border-radius: 50%;
    box-shadow: 0 0 0 2px #4ade80, 0 0 16px rgba(74, 222, 128, 0.75);
    animation: squad-marker-breathe 2.4s ease-in-out infinite;
}

@keyframes squad-marker-breathe {
    50% { box-shadow: 0 0 0 2px #4ade80, 0 0 26px rgba(74, 222, 128, 0.95); }
}

@media (prefers-reduced-motion: reduce) {
    .squad-marker-glow {
        animation: none;
    }
}

.squad-marker-count {
    position: absolute;
    right: -8px;
    bottom: -6px;
    z-index: 2;
    min-width: 20px;
    height: 20px;
    box-sizing: border-box;
    display: grid;
    place-items: center;
    padding: 0 4px;
    border-radius: 10px;
    background: #14532d;
    box-shadow: 0 0 0 1px #4ade80, 0 2px 3px rgba(0, 0, 0, 0.7);
    color: #fff;
    font-family: 'Arial', sans-serif;
    font-size: 11px;
    font-weight: bold;
}
</style>
