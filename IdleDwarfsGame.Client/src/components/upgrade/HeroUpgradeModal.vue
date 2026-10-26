<script lang="ts" setup>
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import HeroCard from '@/components/inventory/HeroCard.vue';
import HeroUpgradePanel from './HeroUpgradePanel.vue';
import HeroesApiService from '@/services/apiServices/HeroesApiService';
import { useResources } from '@/composables/UseResources';
import { HeroInfo, HeroUpgradeResult } from '@/models/types/Heroes';
import { vDragScroll } from '@/directives/dragScroll';

// Прокачка героев: справа список, по клику на героя слева выезжает его панель
const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t } = useI18n();
const { refresh: refreshResources } = useResources();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

const heroes = ref<HeroInfo[]>([]);
const loading = ref(false);
const errorMessage = ref<string | null>(null);
const selectedId = ref<string | null>(null);

const load = async () => {
    loading.value = true;
    errorMessage.value = null;
    try {
        heroes.value = await HeroesApiService.getMyHeroes();
        if (selectedId.value && !heroes.value.some(h => h.id === selectedId.value)) selectedId.value = null;
    } catch {
        errorMessage.value = t('heroUpgrade.loadError');
    } finally {
        loading.value = false;
    }
};

watch(open, isOpen => {
    if (!isOpen) return;
    selectedId.value = null;
    load();
}, { immediate: true });

// Порядок не зависит от прокачки, иначе выбранная карточка прыгала бы по списку после каждого клика
const sorted = computed(() => [...heroes.value]
    .sort((a, b) => b.level - a.level || a.name.localeCompare(b.name)));

const selected = computed(() => heroes.value.find(h => h.id === selectedId.value) ?? null);

const select = (hero: HeroInfo) => {
    selectedId.value = selectedId.value === hero.id ? null : hero.id;
};

const onUpgraded = (result: HeroUpgradeResult) => {
    heroes.value = heroes.value.map(h => (h.id === result.hero.id ? result.hero : h));
};

// Сервер отказал — значит, у нас устаревшие уровень или ресурсы
const onStale = () => {
    load();
    refreshResources();
};
</script>

<template>
    <ModalComponent
        v-model="open"
        :header-message="t('heroUpgrade.header')"
        width="min(1280px, 94vw)"
        height="min(780px, 90vh)"
    >
        <div class="hero-upgrade" :class="{ 'hero-upgrade-selected': selected?.upgrade }">
            <Transition name="upgrade-panel">
                <HeroUpgradePanel
                    v-if="selected?.upgrade"
                    :key="selected.id"
                    :hero="selected"
                    @upgraded="onUpgraded"
                    @stale="onStale"
                />
            </Transition>

            <section v-drag-scroll class="hero-upgrade-list">
                <div v-if="loading && !heroes.length" class="hero-upgrade-hint">{{ t('common.loading') }}</div>
                <div v-else-if="errorMessage" class="hero-upgrade-hint hero-upgrade-hint-error">{{ errorMessage }}</div>
                <template v-else>
                    <div class="hero-upgrade-hint">{{ selected ? t('heroUpgrade.switchHero') : t('heroUpgrade.chooseHero') }}</div>
                    <div class="hero-upgrade-grid">
                        <HeroCard
                            v-for="hero in sorted"
                            :key="hero.id"
                            :hero="hero"
                            selectable
                            :selected="hero.id === selectedId"
                            @click="select(hero)"
                        />
                    </div>
                    <div v-if="!heroes.length" class="hero-upgrade-hint">{{ t('collection.noHeroes') }}</div>
                </template>
            </section>
        </div>
    </ModalComponent>
</template>

<style scoped>
.hero-upgrade {
    flex: 1;
    min-width: 0;
    display: flex;
    column-gap: 14px;
    font-family: 'Arial', sans-serif;
}

.hero-upgrade-list {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    overflow-y: auto;
}

/* С выбранным героем список занимает всё место справа от панели, но не уже двух карточек в ряд (2 × 136 + зазор и поля) */
.hero-upgrade-selected .hero-upgrade-list {
    min-width: 290px;
}

.hero-upgrade-hint {
    margin-bottom: 10px;
    text-align: center;
    color: #cbd5e1;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.hero-upgrade-hint-error {
    color: #ffb4b4;
}

.hero-upgrade-grid {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-items: center;
    align-content: flex-start;
    gap: 10px;
    /* Сверху место под звёзды ранга и кайму выбранной карточки — список их не обрезает */
    padding: 10px 4px 4px;
}

/* Панель выезжает слева */
.upgrade-panel-enter-active {
    transition: transform 0.3s cubic-bezier(0.34, 1.3, 0.64, 1), opacity 0.25s ease;
}

.upgrade-panel-leave-active {
    display: none;
}

.upgrade-panel-enter-from {
    opacity: 0;
    transform: translateX(-40px);
}
</style>
