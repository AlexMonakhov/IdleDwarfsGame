<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import LoginCalendarPanel from './LoginCalendarPanel.vue';
import AchievementsPanel from './AchievementsPanel.vue';
import { useInterface, type RewardsTab } from '@/composables/UseInterface';
import { useRewards } from '@/composables/UseRewards';
import type { ModalTab, TabIcon } from '@/models/types/Modal';
import { IconCalendar, IconTrophy } from '@/components/icons/ui';

// Награды: календарь входов и достижения
const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t } = useI18n();
const { rewardsTab } = useInterface();
const { calendarClaimable, achievementsClaimable } = useRewards();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

const ICONS: Record<RewardsTab, TabIcon> = {
    calendar: IconCalendar,
    achievements: IconTrophy,
};

const tabs = computed<ModalTab[]>(() => (Object.keys(ICONS) as RewardsTab[]).map(key => ({
    key,
    label: t(`achievements.tabs.${key}`),
    icon: ICONS[key],
    badgeCount: key === 'calendar' ? (calendarClaimable.value ? 1 : 0) : achievementsClaimable.value,
})));

const activeTab = computed({
    get: () => rewardsTab.value,
    set: value => { rewardsTab.value = value as RewardsTab; },
});

const header = computed(() => t(`achievements.headers.${rewardsTab.value}`));
</script>

<template>
    <ModalComponent
        v-model="open"
        v-model:active-tab="activeTab"
        :tabs="tabs"
        :header-message="header"
        width="min(1100px, calc(calc(94 * var(--vw)) - 140px))"
        height="min(780px, calc(90 * var(--vh)))"
    >
        <div class="rewards-modal">
            <LoginCalendarPanel v-if="rewardsTab === 'calendar'" />
            <AchievementsPanel v-else />
        </div>
    </ModalComponent>
</template>

<style scoped>
.rewards-modal {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
}
</style>
