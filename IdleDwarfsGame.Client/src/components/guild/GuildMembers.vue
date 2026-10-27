<script lang="ts" setup>
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import GuildApiService from '@/services/apiServices/GuildApiService';
import { GuildInfo, GuildJoinRequestInfo, GuildMemberInfo } from '@/models/types/Guild';
import { GuildRun } from './guildShared';

// Своя гильдия: участники, заявки на вступление (у главы) и выход
const props = defineProps<{
    guild: GuildInfo;
    busy: string | null;
    run: GuildRun;
    serverNow: number;
}>();

const { t, locale } = useI18n();

const isLeader = computed(() => props.guild.myRole === 'Leader');
const isAlone = computed(() => props.guild.members.length <= 1);

// Участник, у которого нажали «Исключить», и выход из гильдии ждут подтверждения
const confirmKickId = ref<string | null>(null);
const confirmLeave = ref(false);

// Исключённого уже нет в списке
watch(() => props.guild.members, members => {
    if (confirmKickId.value && !members.some(m => m.userId === confirmKickId.value)) confirmKickId.value = null;
});

/* ===== Время ===== */
const UNITS: [Intl.RelativeTimeFormatUnit, number][] = [
    ['year', 365 * 24 * 3600],
    ['month', 30 * 24 * 3600],
    ['week', 7 * 24 * 3600],
    ['day', 24 * 3600],
    ['hour', 3600],
    ['minute', 60],
];

const relativeTime = (iso: string) => {
    const seconds = Math.round((new Date(iso).getTime() - props.serverNow) / 1000);
    if (Math.abs(seconds) < 60) return t('friends.justNow');
    const format = new Intl.RelativeTimeFormat(locale.value, { numeric: 'auto' });
    const [unit, size] = UNITS.find(([, size]) => Math.abs(seconds) >= size) ?? UNITS[UNITS.length - 1];
    return format.format(Math.trunc(seconds / size), unit);
};

const formatDate = (iso: string) => new Date(iso).toLocaleDateString(locale.value);

const statusText = (member: GuildMemberInfo) => {
    if (member.isOnline) return t('friends.online');
    if (member.lastSeenAt) return t('friends.lastSeen', { time: relativeTime(member.lastSeenAt) });
    return t('friends.offline');
};

/* ===== Действия ===== */
const accept = (request: GuildJoinRequestInfo) => props.run(request.userId, async () => {
    await GuildApiService.accept(request.userId);
    return t('guild.accepted', { name: request.username });
});

const decline = (request: GuildJoinRequestInfo) => props.run(request.userId, async () => {
    await GuildApiService.decline(request.userId);
    return t('guild.declined', { name: request.username });
});

const kick = (member: GuildMemberInfo) => props.run(member.userId, async () => {
    await GuildApiService.kick(member.userId);
    confirmKickId.value = null;
    return t('guild.kicked', { name: member.username });
});

const leave = () => props.run('leave', async () => {
    const name = props.guild.name;
    await GuildApiService.leave();
    confirmLeave.value = false;
    return t('guild.left', { name });
});
</script>

<template>
    <div class="guild-page">
        <div class="guild-summary">
            <span class="guild-summary-emblem">🛡</span>
            <div class="guild-row-main">
                <span class="guild-summary-name">{{ guild.name }}</span>
                <span class="guild-row-status">
                    {{ t('guild.membersCount', { count: guild.members.length, max: guild.maxMembers }) }}
                    · {{ t('guild.foundedAt', { date: formatDate(guild.createdAt) }) }}
                </span>
            </div>
        </div>

        <!-- ===== Заявки (только глава) ===== -->
        <section v-if="isLeader" class="guild-section">
            <h4 class="guild-subtitle">{{ t('guild.incoming') }}</h4>
            <ul v-if="guild.incoming.length" class="guild-list">
                <li v-for="request in guild.incoming" :key="request.userId" class="guild-row">
                    <div class="guild-row-main">
                        <span class="guild-row-name">{{ request.username }}</span>
                        <span class="guild-row-status">{{ t('friends.sentAt', { time: relativeTime(request.createdAt) }) }}</span>
                    </div>
                    <div class="guild-row-actions">
                        <GemButton
                            :color="'green'"
                            :disabled="!!busy || guild.members.length >= guild.maxMembers"
                            @click="accept(request)"
                        >{{ t('friends.accept') }}</GemButton>
                        <GemButton :color="'red'" :disabled="!!busy" @click="decline(request)">{{ t('friends.decline') }}</GemButton>
                    </div>
                </li>
            </ul>
            <div v-else class="guild-hint">{{ t('guild.noIncoming') }}</div>
        </section>

        <!-- ===== Участники ===== -->
        <section class="guild-section">
            <h4 class="guild-subtitle">{{ t('guild.members') }}</h4>
            <ul class="guild-list">
                <li
                    v-for="member in guild.members"
                    :key="member.userId"
                    class="guild-row"
                    :class="{ 'guild-row-online': member.isOnline, 'guild-row-me': member.isMe }"
                >
                    <span class="presence-dot" :class="{ 'presence-dot-online': member.isOnline }"></span>
                    <div class="guild-row-main">
                        <span class="guild-row-name" :title="t('guild.joinedAt', { date: formatDate(member.joinedAt) })">
                            {{ member.username }}
                            <span v-if="member.role === 'Leader'" class="guild-role">👑 {{ t('guild.roles.Leader') }}</span>
                            <span v-if="member.isMe" class="guild-you">{{ t('guild.you') }}</span>
                        </span>
                        <span class="guild-row-status">{{ statusText(member) }}</span>
                    </div>
                    <div v-if="isLeader && !member.isMe" class="guild-row-actions">
                        <template v-if="confirmKickId === member.userId">
                            <span class="guild-confirm">{{ t('guild.confirmKick') }}</span>
                            <GemButton :color="'red'" :disabled="!!busy" @click="kick(member)">{{ t('friends.yes') }}</GemButton>
                            <GemButton :color="'blue'" :disabled="busy === member.userId" @click="confirmKickId = null">{{ t('friends.no') }}</GemButton>
                        </template>
                        <GemButton v-else :color="'red'" :disabled="!!busy" @click="confirmKickId = member.userId">
                            {{ t('guild.kick') }}
                        </GemButton>
                    </div>
                </li>
            </ul>
        </section>

        <!-- ===== Выход ===== -->
        <div class="guild-leave">
            <template v-if="confirmLeave">
                <span class="guild-confirm">
                    {{ isAlone ? t('guild.confirmDisband') : isLeader ? t('guild.confirmLeaveLeader') : t('guild.confirmLeave') }}
                </span>
                <GemButton :color="'red'" :disabled="!!busy" @click="leave">{{ t('friends.yes') }}</GemButton>
                <GemButton :color="'blue'" :disabled="busy === 'leave'" @click="confirmLeave = false">{{ t('friends.no') }}</GemButton>
            </template>
            <GemButton v-else :color="'red'" :disabled="!!busy" @click="confirmLeave = true">{{ t('guild.leave') }}</GemButton>
        </div>
    </div>
</template>

<style scoped src="./guild.css"></style>

<style scoped>
.guild-summary {
    display: flex;
    align-items: center;
    column-gap: 14px;
    margin-bottom: 14px;
    padding: 10px 14px;
    border-radius: 12px;
    background: var(--wood-background);
    box-shadow: inset 0 0 6px black, 0 0 0 2px rgb(157, 173, 175), 0 0 0 4px #333;
}

.guild-summary-emblem {
    font-size: 40px;
    line-height: 1;
    filter: drop-shadow(2px 3px 2px rgba(0, 0, 0, 0.8));
}

.guild-summary-name {
    color: #ffd166;
    font-size: 20px;
    font-weight: bold;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.guild-row-online {
    border-color: rgba(0, 200, 83, 0.7);
}

.guild-row-online .guild-row-status {
    color: var(--green-gem-color);
    font-weight: bold;
}

.guild-row-me {
    background: rgba(33, 150, 243, 0.18);
}

.guild-role {
    margin-left: 6px;
    color: #ffd166;
    font-size: 12px;
    text-transform: uppercase;
}

.guild-you {
    margin-left: 6px;
    color: #a5f3fc;
    font-size: 12px;
    font-weight: normal;
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

.guild-leave {
    display: flex;
    justify-content: flex-end;
    align-items: center;
    column-gap: 8px;
    margin-top: auto;
    padding-top: 6px;
}
</style>
