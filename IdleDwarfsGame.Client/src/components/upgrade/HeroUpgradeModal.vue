<script lang="ts" setup>
import { computed, ref, watch } from 'vue';
import { AxiosError } from 'axios';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import HeroCard from '@/components/inventory/HeroCard.vue';
import HeroUpgradePanel from './HeroUpgradePanel.vue';
import EquipmentList from '@/components/equipment/EquipmentList.vue';
import { itemName } from '@/components/equipment/equipmentText';
import HeroesApiService from '@/services/apiServices/HeroesApiService';
import EquipmentApiService from '@/services/apiServices/EquipmentApiService';
import { useResources } from '@/composables/UseResources';
import { useItemDrag } from '@/composables/UseItemDrag';
import { HeroInfo, HeroUpgradeResult } from '@/models/types/Heroes';
import { EquipmentChangeResult, EquipmentError, EquipmentInfo, EquipmentSlot, EquipmentType, canWear } from '@/models/types/Equipment';
import type { ModalTab } from '@/models/types/Modal';
import { vDragScroll } from '@/directives/dragScroll';

// Прокачка и снаряжение героев: справа список героев или предметов, по клику на героя слева выезжает его панель
const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t, te } = useI18n();
const { refresh: refreshResources } = useResources();
const { dragged, fromSlot, end: endDrag } = useItemDrag();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

const heroes = ref<HeroInfo[]>([]);
const loading = ref(false);
const errorMessage = ref<string | null>(null);
const selectedId = ref<string | null>(null);

type UpgradeTab = 'heroes' | 'items';
const TAB_ICONS: Record<UpgradeTab, string> = { heroes: '⚔', items: '🛡' };

const tab = ref<UpgradeTab>('heroes');
const tabs = computed<ModalTab[]>(() => (Object.keys(TAB_ICONS) as UpgradeTab[]).map(key => ({
    key,
    label: t(`heroUpgrade.tabs.${key}`),
    icon: TAB_ICONS[key],
})));
const activeTab = computed({
    get: () => tab.value,
    set: value => { tab.value = value as UpgradeTab; },
});

const items = ref<EquipmentInfo[]>([]);
const itemFilter = ref<EquipmentType | null>(null);
const changing = ref(false);
const itemMessage = ref<{ text: string; error: boolean } | null>(null);

const load = async () => {
    loading.value = true;
    errorMessage.value = null;
    try {
        [heroes.value, items.value] = await Promise.all([
            HeroesApiService.getMyHeroes(),
            EquipmentApiService.getMyEquipment(),
        ]);
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
    tab.value = 'heroes';
    itemFilter.value = null;
    itemMessage.value = null;
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

/* ===== Снаряжение ===== */
const applyChange = (result: EquipmentChangeResult) => {
    items.value = result.items;
    heroes.value = heroes.value.map(h => result.heroes.find(changed => changed.id === h.id) ?? h);
};

const changeErrorText = (error: unknown) => {
    const code = (error as AxiosError<{ code?: EquipmentError }>).response?.data?.code;
    return code && te(`equipment.errors.${code}`) ? t(`equipment.errors.${code}`) : t('equipment.changeError');
};

const change = async (request: () => Promise<EquipmentChangeResult>, successText: string) => {
    if (changing.value) return;
    changing.value = true;
    try {
        applyChange(await request());
        itemMessage.value = { text: successText, error: false };
    } catch (error) {
        itemMessage.value = { text: changeErrorText(error), error: true };
        // Отказ обычно значит, что данные устарели: например, предмет сняли в другой вкладке
        load();
    } finally {
        changing.value = false;
    }
};

const equip = (item: EquipmentInfo, slot?: EquipmentSlot) => {
    const hero = selected.value;
    if (!hero) return;
    if (!canWear(item, hero.element)) {
        itemMessage.value = { text: t('equipment.errors.ElementRestricted'), error: true };
        return;
    }
    change(
        () => EquipmentApiService.equip(item.id, hero.id, slot),
        t('equipment.equipped', { item: itemName(item), name: hero.name }),
    );
};

const unequip = (item: EquipmentInfo) => {
    change(() => EquipmentApiService.unequip(item.id), t('equipment.removed', { item: itemName(item) }));
};

// Клик по предмету в списке: надетый на этого героя снимается, остальные надеваются на него
const onPickItem = (item: EquipmentInfo) => {
    if (!selected.value) {
        itemMessage.value = { text: t('equipment.hintChooseHero'), error: true };
        return;
    }
    if (item.heroId === selected.value.id) unequip(item);
    else equip(item);
};

// Клик по пустому слоту — показываем то, что в него подходит
const onPickType = (type: EquipmentType) => {
    itemFilter.value = type;
    tab.value = 'items';
};

// Предмет бросили из слота обратно в список — снимаем
const onDropOnList = () => {
    const item = fromSlot.value ? dragged.value : null;
    endDrag();
    if (item) unequip(item);
};

// На вкладке предметов всегда есть выбранный герой, если он вообще есть: слоты живут на его панели
watch([tab, sorted], () => {
    if (tab.value === 'items' && !selectedId.value) selectedId.value = sorted.value[0]?.id ?? null;
});
</script>

<template>
    <ModalComponent
        v-model="open"
        v-model:active-tab="activeTab"
        :tabs="tabs"
        :header-message="t('heroUpgrade.header')"
        width="min(1280px, calc(94vw - 140px))"
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
                    @equip="equip"
                    @unequip="unequip"
                    @pick-type="onPickType"
                />
            </Transition>

            <section v-drag-scroll class="hero-upgrade-list" @dragover.prevent @drop.prevent="onDropOnList">
                <div v-if="loading && !heroes.length" class="hero-upgrade-hint">{{ t('common.loading') }}</div>
                <div v-else-if="errorMessage" class="hero-upgrade-hint hero-upgrade-hint-error">{{ errorMessage }}</div>
                <template v-else-if="tab === 'items'">
                    <div class="hero-upgrade-hint" :class="{ 'hero-upgrade-hint-error': itemMessage?.error }">
                        {{ itemMessage?.text ?? (selected ? t('equipment.hintEquip') : t('equipment.hintChooseHero')) }}
                    </div>
                    <EquipmentList
                        v-model:filter="itemFilter"
                        :items="items"
                        :heroes="heroes"
                        :hero="selected"
                        draggable
                        @pick="onPickItem"
                    />
                </template>
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
                    <div v-if="!heroes.length" class="hero-upgrade-hint">{{ t('inventory.noHeroes') }}</div>
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
