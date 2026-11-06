<script lang="ts" setup>
import { computed, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import HeroCard from '@/components/inventory/HeroCard.vue';
import HeroFilters from '@/components/inventory/HeroFilters.vue';
import { useHeroFilters } from '@/composables/UseHeroFilters';
import { useHeroes } from '@/composables/UseHeroes';
import { useProgressiveList } from '@/composables/UseProgressiveList';
import { vPauseOffscreen } from '@/directives/pauseOffscreen';

// refreshToken меняется, когда из сундука выпали новые герои — тогда перечитываем список
const props = defineProps<{ refreshToken?: number }>();

const { t } = useI18n();

// Список общий (useHeroes): при повторном открытии окна он виден сразу, а свежий приходит в фоне
const { heroes, error, refresh } = useHeroes();

// Фильтры по стихии и рангу
const { element, rank, filtered } = useHeroFilters(heroes);

// Сильные сверху, при равной мощи — по имени
const sorted = computed(() => [...filtered.value]
    .sort((a, b) => b.power.total - a.power.total || a.name.localeCompare(b.name)));

// Сотни карточек не создаются разом: порция сразу, остальные — по кадрам
const visible = useProgressiveList(sorted);

onMounted(refresh);
watch(() => props.refreshToken, refresh);
</script>

<template>
    <div class="inventory-heroes">
        <div v-if="!heroes && !error" class="inventory-hint">{{ t('common.loading') }}</div>
        <div v-else-if="!heroes" class="inventory-hint inventory-hint-error">{{ t('squad.loadError') }}</div>
        <template v-else>
            <div class="inventory-hint">{{ t('inventory.heroesCount', { count: heroes.length }) }}</div>
            <HeroFilters v-model:element="element" v-model:rank="rank" :heroes="heroes" />
            <div v-if="sorted.length" class="inventory-grid">
                <!-- selectable — чтобы карточку не тащили: здесь только просмотр.
                     v-pause-offscreen — анимации престижа и ауры стоят, пока карточка за краем списка -->
                <HeroCard v-for="hero in visible" :key="hero.id" v-pause-offscreen :hero="hero" selectable />
            </div>
            <div v-else class="inventory-hint">{{ heroes.length ? t('heroFilters.nothing') : t('inventory.noHeroes') }}</div>
        </template>
    </div>
</template>

<style scoped>
.inventory-heroes {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
}

.inventory-hint {
    margin-bottom: 10px;
    text-align: center;
    color: #cbd5e1;
    font-family: 'Arial', sans-serif;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.inventory-hint-error {
    color: #ffb4b4;
}

.inventory-grid {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-items: center;
    gap: 10px;
    padding: 4px;
}
</style>
