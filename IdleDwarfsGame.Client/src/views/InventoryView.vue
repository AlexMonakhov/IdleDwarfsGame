<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';
import { AxiosError } from 'axios';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import PanelComponent from '@/components/common/PanelComponent.vue';
import HeroCard from '@/components/inventory/HeroCard.vue';
import HeroesApiService from '@/services/apiServices/HeroesApiService';
import { HeroInfo, SQUAD_SIZE } from '@/models/types/Heroes';
import { vDragScroll } from '@/directives/dragScroll';

const { t } = useI18n();

type Slots = (string | null)[];

const heroes = ref<HeroInfo[]>([]);
// slots[i] — id героя на месте i + 1
const slots = ref<Slots>(Array(SQUAD_SIZE).fill(null));
// Что сейчас лежит на сервере — для «Сбросить» и индикатора изменений
const savedSlots = ref<Slots>(Array(SQUAD_SIZE).fill(null));

const loading = ref(true);
const saving = ref(false);
const errorMessage = ref<string | null>(null);
const notice = ref<string | null>(null);

const heroById = computed(() => new Map(heroes.value.map(h => [h.id, h])));
const slotHeroes = computed(() => slots.value.map(id => (id ? heroById.value.get(id) ?? null : null)));
// Мощь отряда — сумма мощи героев в слотах, меняется сразу при перетаскивании
const squadPower = computed(() => slotHeroes.value.reduce((sum, hero) => sum + (hero?.power.total ?? 0), 0));
// Все, кто не в отряде: сильные сверху, ушедшие в экспедицию — в конце
const reserve = computed(() => heroes.value
    .filter(h => !slots.value.includes(h.id))
    .sort((a, b) => Number(a.onExpedition) - Number(b.onExpedition) || b.level - a.level || a.name.localeCompare(b.name)));

const dirty = computed(() => slots.value.some((id, i) => id !== savedSlots.value[i]));
const hasHero = computed(() => slots.value.some(id => id !== null));

const applyHeroes = (list: HeroInfo[]) => {
    heroes.value = list;
    const next: Slots = Array(SQUAD_SIZE).fill(null);
    list.forEach(h => {
        if (h.placeInSquad && h.placeInSquad <= SQUAD_SIZE) next[h.placeInSquad - 1] = h.id;
    });
    slots.value = next;
    savedSlots.value = [...next];
};

onMounted(async () => {
    try {
        applyHeroes(await HeroesApiService.getMyHeroes());
    } catch {
        errorMessage.value = t('squad.loadError');
    } finally {
        loading.value = false;
    }
});

/* ===== Drag-n-drop ===== */
// fromSlot === null — героя тащат из списка героев
const drag = ref<{ heroId: string; fromSlot: number | null } | null>(null);
const hoverSlot = ref<number | null>(null);
const hoverReserve = ref(false);

const onDragStart = (event: DragEvent, heroId: string, fromSlot: number | null) => {
    // Герой в экспедиции не может встать в отряд
    if (heroById.value.get(heroId)?.onExpedition) {
        event.preventDefault();
        return;
    }
    drag.value = { heroId, fromSlot };
    notice.value = null;
    if (event.dataTransfer) {
        // Без setData Firefox не начинает перетаскивание
        event.dataTransfer.setData('text/plain', heroId);
        event.dataTransfer.effectAllowed = 'move';
    }
};

const onDragEnd = () => {
    drag.value = null;
    hoverSlot.value = null;
    hoverReserve.value = false;
};

// Герой встаёт в слот; тот, кто там стоял, меняется с ним местами (или уходит в список, если тащили из списка)
const dropOnSlot = (index: number) => {
    const current = drag.value;
    onDragEnd();
    if (!current || current.fromSlot === index) return;

    const next = [...slots.value];
    const displaced = next[index];
    next[index] = current.heroId;
    if (current.fromSlot !== null) next[current.fromSlot] = displaced;
    slots.value = next;
};

const dropOnReserve = () => {
    const current = drag.value;
    onDragEnd();
    if (!current || current.fromSlot === null) return;

    const next = [...slots.value];
    next[current.fromSlot] = null;
    slots.value = next;
};

/* ===== Клики — то же без перетаскивания ===== */
// Герой из отряда уходит обратно в список
const removeFromSquad = (index: number) => {
    if (!slots.value[index]) return;
    const next = [...slots.value];
    next[index] = null;
    slots.value = next;
    errorMessage.value = null;
    notice.value = null;
};

// Герой из списка встаёт в первый свободный слот
const addToSquad = (hero: HeroInfo) => {
    if (hero.onExpedition) return;
    notice.value = null;
    const free = slots.value.indexOf(null);
    if (free === -1) {
        errorMessage.value = t('squad.squadFull');
        return;
    }
    const next = [...slots.value];
    next[free] = hero.id;
    slots.value = next;
    errorMessage.value = null;
};

/* ===== Сохранение ===== */
const reset = () => {
    slots.value = [...savedSlots.value];
    errorMessage.value = null;
    notice.value = null;
};

const save = async () => {
    if (!dirty.value || !hasHero.value || saving.value) return;

    saving.value = true;
    errorMessage.value = null;
    notice.value = null;
    try {
        applyHeroes(await HeroesApiService.saveSquad(slots.value));
        notice.value = t('squad.saved');
    } catch (error) {
        const message = (error as AxiosError<{ message?: string }>).response?.data?.message;
        errorMessage.value = message ?? t('squad.saveError');
    } finally {
        saving.value = false;
    }
};

const statusText = computed(() => {
    if (errorMessage.value) return errorMessage.value;
    if (!hasHero.value) return t('squad.needOneHero');
    if (dirty.value) return t('squad.unsaved');
    return notice.value ?? '';
});
</script>

<template>
    <div class="main inventory-page">
        <!-- Ширина — под 5 слотов в один ряд: 5 × 148 + зазоры, поля секции и панели -->
        <PanelComponent :header-message="t('squad.header')" width="920px">
            <div v-if="loading" class="inventory-hint">{{ t('common.loading') }}</div>

            <template v-else-if="heroes.length">
                <section class="inventory-section">
                    <div class="inventory-heading">
                        <h3 class="inventory-title">{{ t('squad.squad') }}</h3>
                        <span class="squad-power">⚔ {{ t('power.squad', { power: squadPower }) }}</span>
                    </div>
                    <div class="inventory-hint">{{ t('squad.squadHint') }}</div>
                    <div class="squad-row">
                        <div
                            v-for="(hero, index) in slotHeroes"
                            :key="index"
                            class="squad-slot"
                            :class="{ 'squad-slot-hover': hoverSlot === index, 'squad-slot-empty': !hero }"
                            @dragover.prevent="hoverSlot = index"
                            @dragleave="hoverSlot = null"
                            @drop.prevent="dropOnSlot(index)"
                        >
                            <span class="squad-slot-number">{{ index + 1 }}</span>
                            <HeroCard
                                v-if="hero"
                                :hero="hero"
                                :dragging="drag?.heroId === hero.id"
                                @dragstart="onDragStart($event, hero.id, index)"
                                @dragend="onDragEnd"
                                @click="removeFromSquad(index)"
                            />
                            <span v-else class="squad-slot-placeholder">{{ t('squad.emptySlot') }}</span>
                        </div>
                    </div>
                </section>

                <section
                    class="inventory-section reserve"
                    :class="{ 'reserve-hover': hoverReserve && drag && drag.fromSlot !== null }"
                    @dragover.prevent="hoverReserve = true"
                    @dragleave="hoverReserve = false"
                    @drop.prevent="dropOnReserve"
                >
                    <h3 class="inventory-title">{{ t('squad.heroes') }}</h3>
                    <div class="inventory-hint">{{ t('squad.heroesHint') }}</div>
                    <div v-drag-scroll class="reserve-grid">
                        <HeroCard
                            v-for="hero in reserve"
                            :key="hero.id"
                            :hero="hero"
                            :dragging="drag?.heroId === hero.id"
                            :disabled="hero.onExpedition"
                            @dragstart="onDragStart($event, hero.id, null)"
                            @dragend="onDragEnd"
                            @click="addToSquad(hero)"
                        />
                        <div v-if="!reserve.length" class="inventory-hint">{{ t('squad.noReserve') }}</div>
                    </div>
                </section>
            </template>

            <template #panelFooter>
                <div class="inventory-footer">
                    <span class="inventory-status" :class="{ 'inventory-status-error': errorMessage || !hasHero }">{{ statusText }}</span>
                    <GemButton :color="'blue'" :disabled="!dirty || saving" @click="reset">{{ t('squad.reset') }}</GemButton>
                    <GemButton :color="'green'" :disabled="!dirty || !hasHero || saving" @click="save">
                        {{ saving ? t('squad.saving') : t('squad.save') }}
                    </GemButton>
                </div>
            </template>
        </PanelComponent>
    </div>
</template>

<style scoped>
.inventory-page {
    height: 100vh;
}

.inventory-section {
    margin-bottom: 16px;
    padding: 8px;
    border-radius: 12px;
    border: 2px dashed transparent;
    transition: border-color 0.15s ease, background-color 0.15s ease;
}

.inventory-title {
    margin: 0 0 2px;
    color: #f8fafc;
    font-family: 'Arial', sans-serif;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.inventory-heading {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
}

.squad-power {
    color: #ffd166;
    font-family: 'Arial', sans-serif;
    font-size: 14px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.inventory-hint {
    margin-bottom: 10px;
    color: #cbd5e1;
    font-family: 'Arial', sans-serif;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

/* Все 5 слотов всегда в одну линию — порядок слотов это порядок строя */
.squad-row {
    display: flex;
    justify-content: center;
    flex-wrap: nowrap;
    gap: 10px;
}

.squad-slot {
    position: relative;
    box-sizing: border-box;
    width: 148px;
    min-height: 196px;
    display: flex;
    justify-content: center;
    align-items: center;
    border-radius: 12px;
    background: rgba(0, 0, 0, 0.35);
    border: 2px solid rgba(157, 173, 175, 0.5);
    box-shadow: inset 0 0 10px #000;
    transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.squad-slot-empty {
    border-style: dashed;
}

.squad-slot-hover {
    border-color: var(--yellow-gem-color);
    box-shadow: inset 0 0 10px #000, 0 0 12px 2px var(--yellow-gem-color);
}

.squad-slot-number {
    position: absolute;
    top: -10px;
    left: -10px;
    z-index: 1;
    width: 22px;
    height: 22px;
    border-radius: 50%;
    background: var(--wood-background);
    border: 1px solid rgb(157, 173, 175);
    color: #fff;
    font-family: 'Arial', sans-serif;
    font-size: 12px;
    font-weight: bold;
    line-height: 22px;
    text-align: center;
}

.squad-slot-placeholder {
    color: rgba(203, 213, 225, 0.6);
    font-family: 'Arial', sans-serif;
    font-size: 13px;
    text-transform: uppercase;
}

.reserve-hover {
    border-color: var(--yellow-gem-color);
    background-color: rgba(255, 179, 0, 0.08);
}

.reserve-grid {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-items: center;
    gap: 10px;
    max-height: 300px;
    overflow-x: hidden;
    overflow-y: auto;
    /* Сверху место под звёзды ранга — список их не обрезает */
    padding: 10px 4px 4px;
}

.inventory-footer {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: flex-end;
    column-gap: 10px;
}

.inventory-status {
    flex: 1;
    text-align: left;
    color: #fff;
    font-family: 'Arial', sans-serif;
    font-size: 14px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.inventory-status-error {
    color: #ffb4b4;
}
</style>
