<script lang="ts" setup>
import { useRoute, useRouter } from 'vue-router';
import GemButton from '@/components/common/GemButton.vue';
import PanelComponent from '@/components/common/PanelComponent.vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
</script>

<template>
  <div class="main not-found-page">
    <PanelComponent :header-message="'404'">
      <div class="not-found-code">404</div>
      <div class="not-found-title">{{ t('notFound.title') }}</div>
      <div class="not-found-path">{{ route.fullPath }}</div>

      <template #panelFooter>
        <GemButton :color="'blue'" @click="router.back()">{{ t('common.back') }}</GemButton>
        <GemButton :color="'green'" @click="router.push('/')">{{ t('common.toHome') }}</GemButton>
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
</style>
