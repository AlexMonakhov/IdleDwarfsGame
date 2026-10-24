<script lang="ts" setup>
import { useRoute, useRouter } from 'vue-router';
import GemButton from '@/components/common/GemButton.vue';
import PanelComponent from '@/components/common/PanelComponent.vue';

const router = useRouter();
const route = useRoute();
</script>

<template>
  <div class="main not-found-page">
    <PanelComponent :header-message="'404'">
      <div class="not-found-code">404</div>
      <div class="not-found-title">Здесь ещё никто не копал</div>
      <div class="not-found-path">{{ route.fullPath }}</div>

      <template #panelFooter>
        <div class="not-found-actions">
          <GemButton :color="'blue'" @click="router.back()">Назад</GemButton>
          <GemButton :color="'green'" @click="router.push('/')">На главную</GemButton>
        </div>
      </template>
    </PanelComponent>
  </div>
</template>

<style scoped>
.not-found-page {
  height: 100vh;
}

.not-found-code {
  font-size: 96px;
  font-weight: 900;
  line-height: 1;
  letter-spacing: 0.1em;
  background: var(--rune-text-background);
  background-clip: text;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  filter: drop-shadow(-4px 2px 1px #333);
  user-select: none;
}

.not-found-title {
  margin-top: 12px;
  font-family: 'Arial', sans-serif;
  font-weight: bold;
  font-size: 18px;
  text-transform: uppercase;
  letter-spacing: 1px;
  text-shadow: 1px 1px 0px rgba(0, 0, 0, 0.6);
}

.not-found-path {
  margin-top: 8px;
  font-family: monospace;
  font-size: 13px;
  opacity: 0.8;
  word-break: break-all;
  text-shadow: 1px 1px 0px rgba(0, 0, 0, 0.6);
}

.not-found-actions {
  display: flex;
  justify-content: center;
  column-gap: 10px;
}
</style>
