<script lang="ts" setup>
import { computed, onMounted, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import HeroCard from '@/components/inventory/HeroCard.vue';
import HeroesApiService from '@/services/apiServices/HeroesApiService';
import { HeroInfo } from '@/models/types/Heroes';

// refreshToken меняется, когда из сундука выпали новые герои — тогда перечитываем список
const props = defineProps<{ refreshToken?: number }>();

const { t } = useI18n();

const heroes = ref<HeroInfo[]>([]);
const loading = ref(true);
const errorMessage = ref<string | null>(null);

// Сильные сверху, при равной мощи — по имени
const sorted = computed(() => [...heroes.value]
    .sort((a, b) => b.power.total - a.power.total || a.name.localeCompare(b.name)));

const load = async () => {
    errorMessage.value = null;
    try {
        heroes.value = await HeroesApiService.getMyHeroes();
    } catch {
        errorMessage.value = t('squad.loadError');
    } finally {
        loading.value = false;
    }
};

onMounted(load);
watch(() => props.refreshToken, load);
</script>

<template>
    <div class="inventory-heroes">
        <div v-if="loading" class="inventory-hint">{{ t('common.loading') }}</div>
        <div v-else-if="errorMessage" class="inventory-hint inventory-hint-error">{{ errorMessage }}</div>
        <template v-else>
            <div class="inventory-hint">{{ t('inventory.heroesCount', { count: heroes.length }) }}</div>
            <div v-if="sorted.length" class="inventory-grid">
                <!-- selectable — чтобы карточку не тащили: здесь только просмотр -->
                <HeroCard v-for="hero in sorted" :key="hero.id" :hero="hero" selectable />
            </div>
            <div v-else class="inventory-hint">{{ t('inventory.noHeroes') }}</div>
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
