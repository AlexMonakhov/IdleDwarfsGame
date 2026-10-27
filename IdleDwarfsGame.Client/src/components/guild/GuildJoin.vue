<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import InputComponent from '@/components/common/InputComponent.vue';
import GuildApiService from '@/services/apiServices/GuildApiService';
import { GuildOutgoingRequestInfo, GuildOverview, GuildSearchResult } from '@/models/types/Guild';
import { GuildRun, guildErrorText } from './guildShared';

// Игрок без гильдии: найти гильдию и подать заявку или создать свою
const props = defineProps<{
    overview: GuildOverview;
    busy: string | null;
    run: GuildRun;
}>();

const { t } = useI18n();

type Mode = 'choose' | 'search' | 'create';
const mode = ref<Mode>('choose');

const outgoing = computed(() => props.overview.outgoing);
const requestsLeft = computed(() => props.overview.maxOutgoingRequests - outgoing.value.length);

/* ===== Поиск ===== */
const query = ref('');
const results = ref<GuildSearchResult[]>([]);
const searching = ref(false);
const searchError = ref<string | null>(null);
// Поиск уже был — чтобы не показывать «ничего не найдено» до первого запроса
const searched = ref(false);

const search = async () => {
    searching.value = true;
    searchError.value = null;
    try {
        results.value = await GuildApiService.search(query.value.trim());
        searched.value = true;
    } catch (error) {
        searchError.value = guildErrorText(error, t('guild.searchError'));
    } finally {
        searching.value = false;
    }
};

const openSearch = () => {
    mode.value = 'search';
    // Сразу показываем самые большие гильдии
    if (!searched.value) search();
};

// После действия гильдия перечитывается целиком; отметку «заявка подана» обновляем в найденном сами
const markRequested = (guildId: string, requested: boolean) => {
    results.value = results.value.map(r => (r.guildId === guildId ? { ...r, requested } : r));
};

const sendRequest = async (guild: GuildSearchResult) => {
    const ok = await props.run(guild.guildId, async () => {
        await GuildApiService.sendRequest(guild.guildId);
        return t('guild.requestSent', { name: guild.name });
    });
    if (ok) markRequested(guild.guildId, true);
};

const cancelRequest = async (guildId: string, name: string) => {
    const ok = await props.run(guildId, async () => {
        await GuildApiService.cancelRequest(guildId);
        return t('guild.requestCancelled', { name });
    });
    if (ok) markRequested(guildId, false);
};

const cancelOutgoing = (request: GuildOutgoingRequestInfo) => cancelRequest(request.guildId, request.guildName);

/* ===== Создание ===== */
const name = ref('');

const create = () => {
    const value = name.value.trim();
    if (!value) return;
    props.run('create', async () => {
        await GuildApiService.create(value);
        name.value = '';
        return t('guild.created', { name: value });
    });
};
</script>

<template>
    <div class="guild-page">
        <!-- ===== Выбор ===== -->
        <template v-if="mode === 'choose'">
            <div class="guild-hint">{{ t('guild.noGuildHint') }}</div>
            <div class="guild-options">
                <button type="button" class="guild-option" @click="openSearch">
                    <span class="guild-option-icon">🔍</span>
                    <span class="guild-option-title">{{ t('guild.find') }}</span>
                    <span class="guild-option-text">{{ t('guild.findHint') }}</span>
                </button>
                <button type="button" class="guild-option" @click="mode = 'create'">
                    <span class="guild-option-icon">🛡</span>
                    <span class="guild-option-title">{{ t('guild.create') }}</span>
                    <span class="guild-option-text">{{ t('guild.createHint') }}</span>
                </button>
            </div>
        </template>

        <template v-else>
            <div class="guild-back">
                <GemButton :color="'blue'" :disabled="!!busy" @click="mode = 'choose'">{{ t('common.back') }}</GemButton>
            </div>

            <!-- ===== Поиск ===== -->
            <section v-if="mode === 'search'" class="guild-section">
                <form class="guild-form" @submit.prevent="search">
                    <InputComponent v-model="query" :placeholder="t('guild.searchPlaceholder')" :disabled="searching" />
                    <GemButton type="submit" :color="'green'" :disabled="searching">
                        {{ searching ? t('guild.searching') : t('guild.search') }}
                    </GemButton>
                </form>

                <div v-if="searchError" class="guild-hint guild-error">{{ searchError }}</div>
                <ul v-else-if="results.length" class="guild-list">
                    <li v-for="guild in results" :key="guild.guildId" class="guild-row">
                        <span class="guild-emblem">🛡</span>
                        <div class="guild-row-main">
                            <span class="guild-row-name">{{ guild.name }}</span>
                            <span class="guild-row-status">
                                {{ t('guild.leader', { name: guild.leaderName }) }}
                                · {{ t('guild.membersCount', { count: guild.memberCount, max: guild.maxMembers }) }}
                            </span>
                        </div>
                        <div class="guild-row-actions">
                            <GemButton
                                v-if="guild.requested"
                                :color="'yellow'"
                                :disabled="!!busy"
                                @click="cancelRequest(guild.guildId, guild.name)"
                            >{{ t('guild.cancelRequest') }}</GemButton>
                            <GemButton
                                v-else
                                :color="'green'"
                                :disabled="!!busy || guild.memberCount >= guild.maxMembers || requestsLeft <= 0"
                                @click="sendRequest(guild)"
                            >{{ guild.memberCount >= guild.maxMembers ? t('guild.full') : t('guild.join') }}</GemButton>
                        </div>
                    </li>
                </ul>
                <div v-else-if="searched && !searching" class="guild-hint">{{ t('guild.nothingFound') }}</div>
            </section>

            <!-- ===== Создание ===== -->
            <section v-else class="guild-section">
                <h4 class="guild-subtitle">{{ t('guild.create') }}</h4>
                <div class="guild-hint">{{ t('guild.nameHint') }}</div>
                <form class="guild-form" @submit.prevent="create">
                    <InputComponent v-model="name" :placeholder="t('guild.namePlaceholder')" :disabled="busy === 'create'" />
                    <GemButton type="submit" :color="'green'" :disabled="!!busy || !name.trim()">
                        {{ busy === 'create' ? t('guild.creating') : t('guild.createSubmit') }}
                    </GemButton>
                </form>
            </section>
        </template>

        <!-- ===== Свои заявки ===== -->
        <section v-if="mode !== 'create' && outgoing.length" class="guild-section">
            <h4 class="guild-subtitle">{{ t('guild.myRequests', { count: outgoing.length, max: overview.maxOutgoingRequests }) }}</h4>
            <ul class="guild-list">
                <li v-for="request in outgoing" :key="request.guildId" class="guild-row">
                    <span class="guild-emblem">🛡</span>
                    <div class="guild-row-main">
                        <span class="guild-row-name">{{ request.guildName }}</span>
                        <span class="guild-row-status">{{ t('guild.waitingForLeader') }}</span>
                    </div>
                    <div class="guild-row-actions">
                        <GemButton :color="'yellow'" :disabled="!!busy" @click="cancelOutgoing(request)">{{ t('guild.cancelRequest') }}</GemButton>
                    </div>
                </li>
            </ul>
        </section>
    </div>
</template>

<style scoped src="./guild.css"></style>

<style scoped>
.guild-options {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
    gap: 16px;
    margin-bottom: 16px;
}

/* Карточки выбора — как карточки экспедиций */
.guild-option {
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 8px;
    padding: 22px 16px;
    border-radius: 12px;
    border: 2px solid rgba(157, 173, 175, 0.7);
    background: rgba(15, 23, 32, 0.8);
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5);
    color: inherit;
    font: inherit;
    text-align: center;
    cursor: pointer;
    transition: transform 0.12s ease, border-color 0.15s ease, box-shadow 0.15s ease;
}

.guild-option:hover {
    transform: translateY(-3px);
    border-color: var(--yellow-gem-color);
    box-shadow: 0 6px 14px rgba(0, 0, 0, 0.6), 0 0 12px 2px var(--yellow-gem-color);
}

.guild-option-icon {
    font-size: 48px;
    line-height: 1;
    filter: drop-shadow(2px 3px 2px rgba(0, 0, 0, 0.8));
}

.guild-option-title {
    color: #f8fafc;
    font-size: 16px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.guild-option-text {
    color: #cbd5e1;
    font-size: 13px;
    line-height: 1.35;
}

.guild-back {
    margin-bottom: 12px;
}

.guild-emblem {
    flex-shrink: 0;
    font-size: 26px;
    filter: drop-shadow(1px 2px 1px rgba(0, 0, 0, 0.8));
}

.guild-error {
    color: #ffb4b4;
}
</style>
