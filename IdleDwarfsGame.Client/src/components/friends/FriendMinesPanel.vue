<script lang="ts" setup>
import { onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import SocialApiService from '@/services/apiServices/SocialApiService';
import { useResources } from '@/composables/UseResources';
import type { FriendMineInfo, FriendMinesOverview } from '@/models/types/Social';
import { formatNumber, socialErrorText } from '@/components/social/socialShared';
import { IconPickaxe } from '@/components/icons/ui';

// Шахты друга: «постучать киркой» — склад его шахты сразу пополняется (сервер: FriendHelpRules)
const props = defineProps<{ friendId: string }>();
const emit = defineEmits<{
    (e: 'back'): void;
    (e: 'message', text: string, isError: boolean): void;
}>();

const { t } = useI18n();
const { set: setResources } = useResources();

const overview = ref<FriendMinesOverview | null>(null);
const busy = ref(false);
const loadError = ref(false);

onMounted(async () => {
    try {
        overview.value = await SocialApiService.getFriendMines(props.friendId);
    } catch (error) {
        loadError.value = true;
        emit('message', socialErrorText(error, 'help'), true);
    }
});

const canHelp = (mine: FriendMineInfo) => !!overview.value && !overview.value.helpedToday && mine.helpAmount >= 1
    && overview.value.helpsGiven < overview.value.maxHelpsGiven
    && overview.value.friendHelpsReceived < overview.value.maxHelpsReceived;

const help = async (mine: FriendMineInfo) => {
    if (busy.value) return;
    busy.value = true;
    try {
        const result = await SocialApiService.helpFriendMine(props.friendId, mine.mineId);
        overview.value = result.mines;
        setResources(result.resources);
        emit('message', t('social.help.done', { amount: formatNumber(result.added), gold: formatNumber(result.gold) }), false);
    } catch (error) {
        emit('message', socialErrorText(error, 'help'), true);
    } finally {
        busy.value = false;
    }
};
</script>

<template>
    <div class="friend-mines">
        <div class="friend-mines-head">
            <GemButton :color="'blue'" @click="emit('back')">← {{ t('social.help.back') }}</GemButton>
            <h4 v-if="overview">{{ t('social.help.title', { name: overview.friendName }) }}</h4>
        </div>

        <div v-if="!overview" class="friend-mines-hint">{{ loadError ? t('social.loadError') : t('common.loading') }}</div>
        <template v-else>
            <p class="friend-mines-hint">{{ t('social.help.hint', { hours: overview.helpHours }) }}</p>
            <p class="friend-mines-hint">
                {{ t('social.help.limits', { given: overview.helpsGiven, max: overview.maxHelpsGiven, received: overview.friendHelpsReceived, maxReceived: overview.maxHelpsReceived }) }}
            </p>
            <p v-if="overview.helpedToday" class="friend-mines-hint friend-mines-done">{{ t('social.help.helped') }}</p>

            <ul v-if="overview.mines.length" class="friend-mines-list">
                <li v-for="mine in overview.mines" :key="mine.mineId" class="friend-mine">
                    <div class="friend-mine-main">
                        <span class="friend-mine-title">{{ t('social.help.mine', { plot: mine.plot, type: t(`social.help.types.${mine.type}`), level: mine.level }) }}</span>
                        <div class="friend-mine-bar"><div class="friend-mine-fill" :style="{ width: `${mine.capacity ? Math.min(100, mine.stored / mine.capacity * 100) : 0}%` }"></div></div>
                        <span class="friend-mine-muted">{{ t('social.help.stored', { stored: formatNumber(mine.stored), capacity: formatNumber(mine.capacity), dwarves: mine.dwarves }) }}</span>
                    </div>
                    <GemButton v-if="!mine.dwarves" :color="'blue'" disabled>{{ t('social.help.noDwarves') }}</GemButton>
                    <GemButton v-else-if="mine.helpAmount < 1" :color="'blue'" disabled>{{ t('social.help.full') }}</GemButton>
                    <GemButton v-else :color="'green'" :disabled="busy || !canHelp(mine)" @click="help(mine)">
                        <IconPickaxe /> {{ t('social.help.help', { amount: formatNumber(mine.helpAmount) }) }}
                    </GemButton>
                </li>
            </ul>
            <div v-else class="friend-mines-hint">{{ t('social.help.noMines') }}</div>
        </template>
    </div>
</template>

<style scoped>
.friend-mines {
    display: flex;
    flex-direction: column;
    row-gap: 8px;
    font-family: 'Arial', sans-serif;
    text-align: left;
    color: #f8fafc;
}

.friend-mines-head {
    display: flex;
    align-items: center;
    gap: 12px;
}

.friend-mines-head h4 {
    margin: 0;
    font-size: 15px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.friend-mines-hint {
    margin: 0;
    color: #cbd5e1;
    font-size: 13px;
}

.friend-mines-done {
    color: #86efac;
}

.friend-mines-list {
    display: flex;
    flex-direction: column;
    row-gap: 8px;
    margin: 0;
    padding: 0;
    list-style: none;
}

.friend-mine {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 8px 12px;
    border-radius: 10px;
    border: 2px solid rgba(157, 173, 175, 0.5);
    background: rgba(15, 23, 32, 0.7);
}

.friend-mine-main {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    row-gap: 3px;
}

.friend-mine-title {
    font-weight: bold;
}

.friend-mine-muted {
    color: #cbd5e1;
    font-size: 12px;
}

.friend-mine-bar {
    height: 7px;
    border-radius: 4px;
    background: rgba(0, 0, 0, 0.5);
    overflow: hidden;
}

.friend-mine-fill {
    height: 100%;
    background: linear-gradient(90deg, #f59e0b, #fde047);
}
</style>
