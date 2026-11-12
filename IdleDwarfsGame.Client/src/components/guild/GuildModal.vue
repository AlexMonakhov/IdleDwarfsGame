<script lang="ts" setup>
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import ShopPanel from '@/components/shop/ShopPanel.vue';
import GuildJoin from './GuildJoin.vue';
import GuildMembers from './GuildMembers.vue';
import GuildBosses from './GuildBosses.vue';
import GuildDepth from './GuildDepth.vue';
import GuildApiService from '@/services/apiServices/GuildApiService';
import { GuildOverview } from '@/models/types/Guild';
import type { ModalTab, TabIcon } from '@/models/types/Modal';
import { IconGuild, IconMinecart, IconPouch, IconSkull } from '@/components/icons/ui';
import { GuildRun, guildErrorText } from './guildShared';

const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t } = useI18n();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

// Пока окно открыто, раз в столько перечитываем гильдию — кто зашёл, кто подал заявку, приняли ли нас
const REFRESH_MS = 30_000;

const overview = ref<GuildOverview | null>(null);
const loading = ref(false);
// Что сейчас выполняется — блокируем только связанные кнопки
const busy = ref<string | null>(null);
const errorMessage = ref<string | null>(null);
const notice = ref<string | null>(null);

/* ===== Вкладки (только у тех, кто в гильдии) ===== */
type GuildTab = 'guild' | 'bosses' | 'depth' | 'shop';

const ICONS: Record<GuildTab, TabIcon> = {
    guild: IconGuild,
    bosses: IconSkull,
    // Гонка глубины — недельное соревнование гильдий, сразу под боссами
    depth: IconMinecart,
    shop: IconPouch,
};

const tab = ref<GuildTab>('guild');
// Подпись гонки глубины — в social.* (i18n/locales/social.ts)
const tabLabel = (key: GuildTab) => (key === 'depth' ? t('social.depth.tab') : t(`guild.tabs.${key}`));
const guild = computed(() => overview.value?.guild ?? null);

const tabs = computed<ModalTab[]>(() => guild.value
    ? (Object.keys(ICONS) as GuildTab[]).map(key => ({
        key,
        label: tabLabel(key),
        icon: ICONS[key],
        badgeCount: key === 'guild' ? guild.value?.incoming.length : undefined,
    }))
    : []);

const activeTab = computed({
    get: () => tab.value,
    set: value => {
        tab.value = value as GuildTab;
        errorMessage.value = null;
        notice.value = null;
    },
});

const header = computed(() => {
    if (!guild.value) return t('guild.header');
    return tab.value === 'guild' ? guild.value.name : tabLabel(tab.value);
});

/* ===== Время ===== */
// Таймеры и «был в сети» считаем по часам сервера: offset — насколько они расходятся с часами браузера
const now = ref(Date.now());
let clockOffset = 0;
let ticker = 0;
let lastLoad = 0;
const serverNow = computed(() => now.value + clockOffset);

/* ===== Загрузка ===== */
const load = async () => {
    loading.value = true;
    lastLoad = Date.now();
    try {
        const next = await GuildApiService.getOverview();
        clockOffset = new Date(next.serverTime).getTime() - Date.now();
        now.value = Date.now();
        // Вступили или вышли — вкладки начинаются с самой гильдии
        if (!!next.guild !== !!overview.value?.guild) tab.value = 'guild';
        overview.value = next;
    } catch {
        errorMessage.value = t('guild.loadError');
    } finally {
        loading.value = false;
    }
};

watch(open, isOpen => {
    window.clearInterval(ticker);
    if (!isOpen) return;

    errorMessage.value = null;
    notice.value = null;
    load();
    ticker = window.setInterval(() => {
        now.value = Date.now();
        if (!loading.value && !busy.value && Date.now() - lastLoad >= REFRESH_MS) load();
    }, 1000);
}, { immediate: true });

onBeforeUnmount(() => window.clearInterval(ticker));

/* ===== Действия ===== */
// И после успеха, и после ошибки перечитываем гильдию: заявку могли принять или отклонить с другой стороны
const run: GuildRun = async (key, action) => {
    if (busy.value) return false;
    busy.value = key;
    errorMessage.value = null;
    notice.value = null;
    let ok = false;
    try {
        notice.value = (await action()) ?? null;
        ok = true;
    } catch (error) {
        errorMessage.value = guildErrorText(error, t('guild.actionError'));
    } finally {
        busy.value = null;
    }
    await load();
    return ok;
};

const footerText = computed(() => errorMessage.value ?? notice.value ?? '');
</script>

<template>
    <ModalComponent
        v-model="open"
        v-model:active-tab="activeTab"
        :tabs="tabs"
        :header-message="header"
        :width="guild ? 'min(900px, calc(calc(94 * var(--vw)) - 140px))' : 'min(680px, calc(94 * var(--vw)))'"
        height="min(940px, calc(92 * var(--vh)))"
    >
        <div v-if="loading && !overview" class="guild-loading">{{ t('common.loading') }}</div>

        <GuildJoin
            v-else-if="overview && !guild"
            :overview="overview"
            :busy="busy"
            :run="run"
        />

        <template v-else-if="guild">
            <GuildMembers
                v-if="tab === 'guild'"
                :guild="guild"
                :busy="busy"
                :run="run"
                :server-now="serverNow"
            />
            <GuildBosses
                v-else-if="tab === 'bosses'"
                :busy="busy"
                :run="run"
                :server-now="serverNow"
                @navigate="open = false"
            />
            <GuildDepth
                v-else-if="tab === 'depth'"
                :busy="busy"
                :run="run"
                :server-now="serverNow"
            />
            <!-- Лавка гильдии: та же полка, что на рынке, но за значки почёта с боссов -->
            <ShopPanel v-else shop="Guild" :sections="['Item', 'Hero']" />
        </template>

        <template #modalFooter>
            <div class="guild-footer">
                <span class="guild-footer-status" :class="{ 'guild-footer-error': errorMessage }">{{ footerText }}</span>
            </div>
        </template>
    </ModalComponent>
</template>

<style scoped>
.guild-loading {
    margin: 6px;
    color: #cbd5e1;
    font-family: 'Arial', sans-serif;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.guild-footer {
    flex: 1;
    display: flex;
    align-items: center;
    min-height: 24px;
}

.guild-footer-status {
    flex: 1;
    text-align: left;
    color: #fff;
    font-family: 'Arial', sans-serif;
    font-size: 14px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.guild-footer-error {
    color: #ffb4b4;
}
</style>
