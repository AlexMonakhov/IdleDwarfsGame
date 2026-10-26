<script lang="ts" setup>
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { AxiosError } from 'axios';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import GemButton from '@/components/common/GemButton.vue';
import InputComponent from '@/components/common/InputComponent.vue';
import BadgeComponent from '@/components/common/BadgeComponent.vue';
import FriendsApiService from '@/services/apiServices/FriendsApiService';
import { usePresence } from '@/composables/UsePresence';
import { FriendInfo, FriendRequestInfo, FriendsOverview } from '@/models/types/Friends';

const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t, te, locale } = useI18n();
const { setIncomingFriendRequests } = usePresence();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

// Пока окно открыто, раз в столько перечитываем список — кто зашёл, кто вышел
const REFRESH_MS = 30_000;

const overview = ref<FriendsOverview | null>(null);
const loading = ref(false);
// Что сейчас выполняется: 'add' или id игрока в строке — блокируем только её кнопки
const busy = ref<string | null>(null);
const errorMessage = ref<string | null>(null);
const notice = ref<string | null>(null);

const tab = ref<'friends' | 'requests'>('friends');
const username = ref('');
// Друг, у которого нажали «Удалить» и ждут подтверждения
const confirmRemoveId = ref<string | null>(null);

/* ===== Время ===== */
// «Был в сети 5 минут назад» считаем по часам сервера: offset — насколько они расходятся с часами браузера
const now = ref(Date.now());
let clockOffset = 0;
let ticker = 0;
const serverNow = computed(() => now.value + clockOffset);

const UNITS: [Intl.RelativeTimeFormatUnit, number][] = [
    ['year', 365 * 24 * 3600],
    ['month', 30 * 24 * 3600],
    ['week', 7 * 24 * 3600],
    ['day', 24 * 3600],
    ['hour', 3600],
    ['minute', 60],
];

const relativeTime = (iso: string) => {
    const seconds = Math.round((new Date(iso).getTime() - serverNow.value) / 1000);
    if (Math.abs(seconds) < 60) return t('friends.justNow');
    const format = new Intl.RelativeTimeFormat(locale.value, { numeric: 'auto' });
    const [unit, size] = UNITS.find(([, size]) => Math.abs(seconds) >= size) ?? UNITS[UNITS.length - 1];
    return format.format(Math.trunc(seconds / size), unit);
};

const formatDate = (iso: string) => new Date(iso).toLocaleDateString(locale.value);

const statusText = (friend: FriendInfo) => {
    if (friend.isOnline) return t('friends.online');
    if (friend.lastSeenAt) return t('friends.lastSeen', { time: relativeTime(friend.lastSeenAt) });
    return t('friends.offline');
};

/* ===== Загрузка ===== */
// Сервер отвечает { code, message } — показываем перевод по коду, иначе его сообщение
const errorText = (error: unknown, fallback: string) => {
    const data = (error as AxiosError<{ code?: string; message?: string }>).response?.data;
    if (data?.code && te(`friends.errors.${data.code}`)) return t(`friends.errors.${data.code}`);
    return data?.message ?? fallback;
};

const load = async () => {
    loading.value = true;
    try {
        const next = await FriendsApiService.getOverview();
        clockOffset = new Date(next.serverTime).getTime() - Date.now();
        now.value = Date.now();
        overview.value = next;
        setIncomingFriendRequests(next.incoming.length);
        // Друга, которого собирались удалить, уже нет в списке
        if (confirmRemoveId.value && !next.friends.some(f => f.userId === confirmRemoveId.value)) {
            confirmRemoveId.value = null;
        }
    } catch {
        errorMessage.value = t('friends.loadError');
    } finally {
        loading.value = false;
    }
};

watch(open, isOpen => {
    window.clearInterval(ticker);
    if (!isOpen) return;

    errorMessage.value = null;
    notice.value = null;
    confirmRemoveId.value = null;
    load().then(() => {
        // Есть только входящие заявки — сразу показываем их
        if (overview.value && !overview.value.friends.length && overview.value.incoming.length) tab.value = 'requests';
    });
    ticker = window.setInterval(() => {
        now.value = Date.now();
        if (!loading.value && !busy.value) load();
    }, REFRESH_MS);
}, { immediate: true });

onBeforeUnmount(() => window.clearInterval(ticker));

/* ===== Списки ===== */
const friends = computed(() => overview.value?.friends ?? []);
const incoming = computed(() => overview.value?.incoming ?? []);
const outgoing = computed(() => overview.value?.outgoing ?? []);
const maxFriends = computed(() => overview.value?.maxFriends ?? 0);

/* ===== Действия ===== */
// Выполняет действие и показывает его итог в футере. И после успеха, и после ошибки перечитываем список:
// заявку могли принять или отозвать с другой стороны
const run = async (key: string, action: () => Promise<string>) => {
    if (busy.value) return;
    busy.value = key;
    errorMessage.value = null;
    notice.value = null;
    try {
        notice.value = await action();
    } catch (error) {
        errorMessage.value = errorText(error, t('friends.actionError'));
    } finally {
        busy.value = null;
    }
    await load();
};

const addFriend = () => {
    const name = username.value.trim();
    if (!name) {
        errorMessage.value = t('friends.enterName');
        notice.value = null;
        return;
    }
    run('add', async () => {
        const result = await FriendsApiService.sendRequest(name);
        username.value = '';
        if (result.outcome === 'Accepted') {
            tab.value = 'friends';
            return t('friends.becameFriends', { name });
        }
        return t('friends.requestSent', { name });
    });
};

const accept = (request: FriendRequestInfo) => run(request.userId, async () => {
    await FriendsApiService.accept(request.userId);
    return t('friends.accepted', { name: request.username });
});

const decline = (request: FriendRequestInfo) => run(request.userId, async () => {
    await FriendsApiService.decline(request.userId);
    return t('friends.declined', { name: request.username });
});

const cancel = (request: FriendRequestInfo) => run(request.userId, async () => {
    await FriendsApiService.cancel(request.userId);
    return t('friends.cancelled', { name: request.username });
});

const remove = (friend: FriendInfo) => run(friend.userId, async () => {
    await FriendsApiService.remove(friend.userId);
    confirmRemoveId.value = null;
    return t('friends.removed', { name: friend.username });
});

const footerText = computed(() => errorMessage.value ?? notice.value ?? '');
</script>

<template>
    <ModalComponent
        v-model="open"
        :header-message="t('friends.header')"
        width="min(640px, 94vw)"
        height="min(700px, 88vh)"
    >
        <div class="friends">
            <form class="friends-add" @submit.prevent="addFriend">
                <InputComponent
                    v-model="username"
                    :placeholder="t('friends.addPlaceholder')"
                    :disabled="busy === 'add'"
                />
                <GemButton type="submit" :color="'green'" :disabled="!!busy">
                    {{ busy === 'add' ? t('friends.adding') : t('friends.add') }}
                </GemButton>
            </form>

            <div class="friends-tabs">
                <button
                    type="button"
                    class="friends-tab"
                    :class="{ 'friends-tab-active': tab === 'friends' }"
                    @click="tab = 'friends'"
                >{{ t('friends.tabFriends', { count: friends.length, max: maxFriends }) }}</button>
                <button
                    type="button"
                    class="friends-tab"
                    :class="{ 'friends-tab-active': tab === 'requests' }"
                    @click="tab = 'requests'"
                >
                    {{ t('friends.tabRequests') }}
                    <BadgeComponent :count="incoming.length" />
                </button>
            </div>

            <div v-if="loading && !overview" class="friends-hint">{{ t('common.loading') }}</div>

            <!-- ===== Друзья ===== -->
            <template v-else-if="tab === 'friends'">
                <ul v-if="friends.length" class="friends-list">
                    <li
                        v-for="friend in friends"
                        :key="friend.userId"
                        class="friend-row"
                        :class="{ 'friend-row-online': friend.isOnline }"
                    >
                        <span class="presence-dot" :class="{ 'presence-dot-online': friend.isOnline }"></span>
                        <div class="friend-main">
                            <span class="friend-name" :title="t('friends.friendsSince', { date: formatDate(friend.friendsSince) })">
                                {{ friend.username }}
                            </span>
                            <span class="friend-status">{{ statusText(friend) }}</span>
                        </div>
                        <div class="friend-actions">
                            <template v-if="confirmRemoveId === friend.userId">
                                <span class="friend-confirm">{{ t('friends.confirmRemove') }}</span>
                                <GemButton :color="'red'" :disabled="!!busy" @click="remove(friend)">{{ t('friends.yes') }}</GemButton>
                                <GemButton :color="'blue'" :disabled="busy === friend.userId" @click="confirmRemoveId = null">{{ t('friends.no') }}</GemButton>
                            </template>
                            <GemButton v-else :color="'red'" :disabled="!!busy" @click="confirmRemoveId = friend.userId">
                                {{ t('friends.remove') }}
                            </GemButton>
                        </div>
                    </li>
                </ul>
                <div v-else class="friends-hint">{{ t('friends.noFriends') }}</div>
            </template>

            <!-- ===== Заявки ===== -->
            <template v-else>
                <section class="friends-section">
                    <h4 class="friends-subtitle">{{ t('friends.incoming') }}</h4>
                    <ul v-if="incoming.length" class="friends-list">
                        <li v-for="request in incoming" :key="request.userId" class="friend-row">
                            <div class="friend-main">
                                <span class="friend-name">{{ request.username }}</span>
                                <span class="friend-status">{{ t('friends.sentAt', { time: relativeTime(request.createdAt) }) }}</span>
                            </div>
                            <div class="friend-actions">
                                <GemButton :color="'green'" :disabled="!!busy" @click="accept(request)">{{ t('friends.accept') }}</GemButton>
                                <GemButton :color="'red'" :disabled="!!busy" @click="decline(request)">{{ t('friends.decline') }}</GemButton>
                            </div>
                        </li>
                    </ul>
                    <div v-else class="friends-hint">{{ t('friends.noIncoming') }}</div>
                </section>

                <section class="friends-section">
                    <h4 class="friends-subtitle">{{ t('friends.outgoing') }}</h4>
                    <ul v-if="outgoing.length" class="friends-list">
                        <li v-for="request in outgoing" :key="request.userId" class="friend-row">
                            <div class="friend-main">
                                <span class="friend-name">{{ request.username }}</span>
                                <span class="friend-status">{{ t('friends.sentAt', { time: relativeTime(request.createdAt) }) }}</span>
                            </div>
                            <div class="friend-actions">
                                <GemButton :color="'yellow'" :disabled="!!busy" @click="cancel(request)">{{ t('friends.cancel') }}</GemButton>
                            </div>
                        </li>
                    </ul>
                    <div v-else class="friends-hint">{{ t('friends.noOutgoing') }}</div>
                </section>
            </template>
        </div>

        <template #modalFooter>
            <div class="friends-footer">
                <span class="friends-status" :class="{ 'friends-status-error': errorMessage }">{{ footerText }}</span>
            </div>
        </template>
    </ModalComponent>
</template>

<style scoped>
.friends {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    padding: 6px;
    font-family: 'Arial', sans-serif;
    text-align: left;
}

.friends-hint {
    margin: 6px 0 10px;
    color: #cbd5e1;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

/* ===== Добавление ===== */
.friends-add {
    display: flex;
    align-items: flex-start;
    column-gap: 10px;
}

/* Высота кнопки — как у поля (38px + металлическая рамка) */
.friends-add > .gem-button {
    flex-shrink: 0;
    height: 46px;
}

/* ===== Вкладки ===== */
.friends-tabs {
    display: flex;
    column-gap: 8px;
    margin: 2px 0 12px;
}

.friends-tab {
    position: relative;
    flex: 1;
    height: 38px;
    border: none;
    border-radius: 10px;
    background: var(--wood-background);
    box-shadow: inset 0 0 6px black, 0 0 0 2px rgb(157, 173, 175), 0 0 0 4px #333;
    color: #cbd5e1;
    font: bold 14px 'Arial', sans-serif;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
    cursor: pointer;
    transition: color 0.15s ease, box-shadow 0.15s ease;
}

.friends-tab:hover {
    color: #fff;
}

.friends-tab-active {
    color: #fff;
    box-shadow: inset 0 0 6px black, 0 0 0 2px var(--yellow-gem-color), 0 0 0 4px #333, 0 0 12px 2px var(--yellow-gem-color);
}

/* ===== Списки ===== */
.friends-section {
    margin-bottom: 14px;
}

.friends-subtitle {
    margin: 0 0 6px;
    color: #f8fafc;
    font-size: 14px;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.friends-list {
    display: flex;
    flex-direction: column;
    row-gap: 8px;
    margin: 0;
    padding: 0;
    list-style: none;
}

.friend-row {
    display: flex;
    align-items: center;
    column-gap: 12px;
    min-height: 56px;
    padding: 6px 12px;
    box-sizing: border-box;
    border-radius: 10px;
    border: 2px solid rgba(157, 173, 175, 0.5);
    background: rgba(15, 23, 32, 0.7);
    box-shadow: 0 3px 8px rgba(0, 0, 0, 0.45);
}

.friend-row-online {
    border-color: rgba(0, 200, 83, 0.7);
}

.presence-dot {
    flex-shrink: 0;
    width: 12px;
    height: 12px;
    border-radius: 50%;
    background: #64748b;
    box-shadow: inset 0 0 2px #000;
}

.presence-dot-online {
    background: var(--green-gem-color);
    box-shadow: 0 0 8px 1px var(--green-gem-color);
}

.friend-main {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    row-gap: 2px;
}

.friend-name {
    overflow: hidden;
    color: #f8fafc;
    font-size: 16px;
    font-weight: bold;
    text-overflow: ellipsis;
    white-space: nowrap;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.friend-status {
    color: #cbd5e1;
    font-size: 12px;
}

.friend-row-online .friend-status {
    color: var(--green-gem-color);
    font-weight: bold;
}

.friend-actions {
    flex-shrink: 0;
    display: flex;
    align-items: center;
    column-gap: 8px;
}

.friend-actions :deep(.gem-text) {
    font-size: 13px;
}

.friend-confirm {
    color: #ffd166;
    font-size: 13px;
    font-weight: bold;
}

/* ===== Футер ===== */
.friends-footer {
    flex: 1;
    display: flex;
    align-items: center;
    min-height: 24px;
}

.friends-status {
    flex: 1;
    text-align: left;
    color: #fff;
    font-family: 'Arial', sans-serif;
    font-size: 14px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.friends-status-error {
    color: #ffb4b4;
}
</style>
