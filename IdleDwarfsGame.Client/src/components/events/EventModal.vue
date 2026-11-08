<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import FixedShopPanel from '@/components/fixedShop/FixedShopPanel.vue';
import WorldBossPanel from './WorldBossPanel.vue';
import CommunityMinesPanel from './CommunityMinesPanel.vue';
import { EVENT_CURRENCY_ICONS, formatCountdown } from './eventShared';
import { IconCrate, IconMinecart, IconSkull } from '@/components/icons/ui';
import type { ModalTab, TabIcon } from '@/models/types/Modal';
import type { FixedShopOverview, WorldEventInfo } from '@/models/types/FixedShop';

// Ивент недели для всех аккаунтов: шапка (что идёт, сколько осталось, сколько валюты), активность ивента —
// мировые боссы или общие шахты — и лавка ивента с фиксированными наградами.
// Какой ивент идёт, становится известно из первого ответа (лавки или активности)
const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t, locale } = useI18n();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

type EventTab = 'event' | 'shop';

const tab = ref<EventTab>('event');
const event = ref<WorldEventInfo | null>(null);

const activityIcon = computed<TabIcon>(() => (event.value?.kind === 'WorldBosses' ? IconSkull : IconMinecart));
const tabs = computed<ModalTab[]>(() => [
    { key: 'event', label: t('events.tabs.event'), icon: activityIcon.value },
    { key: 'shop', label: t('events.tabs.shop'), icon: IconCrate },
]);
const shopPanel = ref<InstanceType<typeof FixedShopPanel> | null>(null);
const activeTab = computed({
    get: () => tab.value,
    set: value => {
        tab.value = value as EventTab;
        if (value === 'shop') shopPanel.value?.reload();
    },
});

const currencyIcon = computed(() => EVENT_CURRENCY_ICONS[event.value?.kind ?? 'CommunityMines']);
const kindKey = computed(() => `events.kinds.${event.value?.kind ?? 'CommunityMines'}`);
const header = computed(() => (event.value ? t(`${kindKey.value}.name`) : t('events.header')));

const onLoaded = (overview: FixedShopOverview) => {
    if (overview.event) {
        clockOffset = new Date(overview.serverTime).getTime() - Date.now();
        event.value = overview.event;
    }
};

// Активность прислала свежий ивент (валюта после удара или награды) — шапка показывает его
const onEvent = (next: WorldEventInfo) => {
    event.value = next;
};

/* ===== Время ===== */
const now = ref(Date.now());
let clockOffset = 0;
let ticker = 0;

const until = (iso: string | undefined) => (iso ? new Date(iso).getTime() - (now.value + clockOffset) : 0);
const timerText = computed(() => {
    if (!event.value) return '';
    return event.value.active
        ? t('events.endsIn', { time: formatCountdown(until(event.value.endsAt), t('fixedShop.day')) })
        : t('events.shopOnly', { time: formatCountdown(until(event.value.shopClosesAt), t('fixedShop.day')) });
});

onMounted(() => {
    ticker = window.setInterval(() => { now.value = Date.now(); }, 1000);
});

onBeforeUnmount(() => window.clearInterval(ticker));

const format = (value: number) => value.toLocaleString(locale.value);
</script>

<template>
    <ModalComponent
        v-model="open"
        v-model:active-tab="activeTab"
        :tabs="tabs"
        :header-message="header"
        width="min(1280px, calc(calc(94 * var(--vw)) - 140px))"
        height="min(820px, calc(90 * var(--vh)))"
    >
        <div class="event-modal" :class="`event-${event?.kind ?? 'CommunityMines'}`">
            <!-- ===== Шапка ивента ===== -->
            <div v-if="event" class="event-banner">
                <div class="event-banner-text">
                    <h3 class="event-title">{{ t(`${kindKey}.name`) }}</h3>
                    <p class="event-tagline">{{ t(`${kindKey}.tagline`) }}</p>
                </div>
                <div class="event-banner-side">
                    <span class="event-timer" :class="{ 'event-timer-shop': !event.active }">{{ timerText }}</span>
                    <span class="event-earned" v-tooltip="t(`${kindKey}.currency`)">
                        <component :is="currencyIcon" /> {{ t('events.balance', { amount: format(event.currency) }) }}
                    </span>
                    <span class="event-earned event-earned-total" v-tooltip="t('events.earnedHint')">
                        {{ t('events.earned', { amount: format(event.earned) }) }}
                    </span>
                    <span class="event-next">{{ t('events.next', { event: t(`events.kinds.${event.nextKind}.name`) }) }}</span>
                </div>
            </div>

            <!-- ===== Активность ивента ===== -->
            <template v-if="tab === 'event'">
                <p v-if="event" class="event-about-text">{{ t(`${kindKey}.about`) }} {{ t('events.burnWarning') }}</p>
                <WorldBossPanel v-if="event?.kind === 'WorldBosses'" @event="onEvent" @navigate="open = false" />
                <CommunityMinesPanel v-else-if="event?.kind === 'CommunityMines'" @event="onEvent" />
                <span v-else class="event-loading">{{ t('common.loading') }}</span>
            </template>

            <!-- Лавка грузится сразу: из её ответа берётся ивент для шапки. При переходе на вкладку — перечитывается
                 (валюта могла прийти за удары и шахты) -->
            <FixedShopPanel
                ref="shopPanel"
                v-show="tab === 'shop'"
                shop="Event"
                :currency-icon="currencyIcon"
                :currency-label="t(`${kindKey}.currency`)"
                @loaded="onLoaded"
            />
        </div>
    </ModalComponent>
</template>

<style scoped>
.event-modal {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    row-gap: 12px;
    font-family: 'Arial', sans-serif;
    color: #f8fafc;
    --event-accent: #fbbf24;
    --event-glow: rgba(217, 119, 6, 0.45);
}

.event-WorldBosses {
    --event-accent: #fca5a5;
    --event-glow: rgba(185, 28, 28, 0.45);
}

.event-banner {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 10px 20px;
    margin: 0 6px;
    padding: 14px 18px;
    border-radius: 14px;
    background:
        radial-gradient(ellipse 60% 120% at 0% 50%, var(--event-glow), transparent 70%),
        rgba(0, 0, 0, 0.45);
    box-shadow: inset 0 0 14px rgba(0, 0, 0, 0.8), 0 0 0 1px color-mix(in srgb, var(--event-accent) 50%, transparent);
}

.event-banner-text {
    flex: 1;
    min-width: 240px;
}

.event-title {
    margin: 0 0 4px;
    color: var(--event-accent);
    font-size: 22px;
    letter-spacing: 1px;
    text-shadow: 0 0 10px var(--event-glow), 1px 1px 0 #000;
}

.event-tagline {
    margin: 0;
    color: #e2e8f0;
    font-size: 13px;
    font-style: italic;
}

.event-banner-side {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    row-gap: 6px;
}

.event-timer {
    padding: 3px 10px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.55);
    color: #fde68a;
    font-size: 13px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
}

.event-timer-shop {
    color: #fca5a5;
}

.event-earned {
    display: inline-flex;
    align-items: center;
    column-gap: 6px;
    font-size: 13px;
    font-weight: bold;
}

.event-next {
    color: #94a3b8;
    font-size: 12px;
}

.event-earned-total {
    color: #cbd5e1;
    font-size: 12px;
    font-weight: normal;
}

.event-about-text {
    margin: 0 6px;
    color: #e2e8f0;
    font-size: 13px;
    line-height: 1.5;
}

.event-loading {
    margin: 0 6px;
    color: #cbd5e1;
    font-size: 13px;
}
</style>
