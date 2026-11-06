<script lang="ts" setup>
import { useId } from 'vue';

// Радужная монета — редкая (донатная) валюта: монета, переливающаяся всеми цветами, с гуртом и звездой.
// Свой SVG, а не SplitIcon: тот красит только двумя цветами, а здесь градиент. Размер — как у остальных иконок
withDefaults(defineProps<{ size?: string }>(), { size: '1.2em' });

// id внутри svg должны быть уникальны, иначе несколько монет на странице делят один градиент
const uid = useId();
const fillId = `rainbow-fill-${uid}`;
const shineId = `rainbow-shine-${uid}`;
const cutId = `rainbow-cut-${uid}`;
</script>

<template>
  <svg class="rainbow-coin" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" :width="size" :height="size">
    <defs>
      <linearGradient :id="fillId" x1="0" y1="0" x2="1" y2="1">
        <stop offset="0%" stop-color="#ff6b6b" />
        <stop offset="20%" stop-color="#ffd166" />
        <stop offset="40%" stop-color="#6bff95" />
        <stop offset="60%" stop-color="#6bd5ff" />
        <stop offset="80%" stop-color="#a78bfa" />
        <stop offset="100%" stop-color="#ff7ad9" />
      </linearGradient>
      <!-- Левая половина светлее — как у двухцветных иконок -->
      <linearGradient :id="shineId" x1="0" y1="0" x2="1" y2="0">
        <stop offset="50%" stop-color="#fff" stop-opacity="0.45" />
        <stop offset="50%" stop-color="#fff" stop-opacity="0" />
      </linearGradient>
      <mask :id="cutId" maskUnits="userSpaceOnUse" x="0" y="0" width="100" height="100">
        <rect x="0" y="0" width="100" height="100" fill="#fff" />
        <!-- Гурт и звезда — прорези насквозь -->
        <circle cx="50" cy="50" r="32" fill="none" stroke="#000" stroke-width="3.5" />
        <path d="M50 30 L55.3 44.6 L70.8 45.1 L58.6 54.6 L62.9 69.5 L50 60.8 L37.1 69.5 L41.4 54.6 L29.2 45.1 L44.7 44.6 Z" fill="#000" />
      </mask>
    </defs>
    <g :mask="`url(#${cutId})`">
      <circle cx="50" cy="50" r="44" :fill="`url(#${fillId})`" />
      <circle cx="50" cy="50" r="44" :fill="`url(#${shineId})`" />
    </g>
  </svg>
</template>

<style scoped>
/* В строке текста садится на базовую линию, как буква — так же, как SplitIcon */
.rainbow-coin {
  flex-shrink: 0;
  vertical-align: -0.2em;
}
</style>
