<script lang="ts" setup>
import { computed, onMounted, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { AxiosError } from 'axios';
import GemButton from '@/components/common/GemButton.vue';
import FilterRow from '@/components/common/FilterRow.vue';
import type { FilterOption } from '@/components/common/filterOptions';
import HeroFilters from '@/components/inventory/HeroFilters.vue';
import ItemFilters from '@/components/equipment/ItemFilters.vue';
import GemFilters from '@/components/inventory/GemFilters.vue';
import MailAttachmentCard from '@/components/mail/MailAttachmentCard.vue';
import HeroConstructor from '@/components/admin/HeroConstructor.vue';
import type { HeroInfo } from '@/models/types/Heroes';
import { CURRENCY_ORDER } from '@/components/mail/mailCurrencies';
import { ELEMENT_COLORS, ELEMENT_ICONS } from '@/components/inventory/lootStyles';
import { useHeroFilters } from '@/composables/UseHeroFilters';
import AdminApiService from '@/services/apiServices/AdminApiService';
import type { AbilityInfo, AbilityKind } from '@/models/types/Abilities';
import type { HeroElement } from '@/battle/types';
import type { EquipmentType } from '@/models/types/Equipment';
import type { Rarity } from '@/models/types/Inventory';
import type {
    AdminMailCatalog,
    AdminPlayerInfo,
    MailAttachmentInfo,
    MailAttachmentKind,
    MailAttachmentRequest,
    MailInfo,
} from '@/models/types/Mail';

// Админ-панель: найти игрока по нику, написать письмо и вложить любой ресурс игры.
// Ресурс выбирается списком, ниже — каталог карточек с фильтрами как на складе; у карточки — количество и «Добавить».
// Вложения письма всегда идут в одном порядке: дворфы, предметы, камни, способности, валюты.
// TODO: права доступа — пока панель открыта всем (AdminController на сервере тоже)
const { t, locale } = useI18n();

const errorText = (error: unknown, fallback: string) =>
    (error as AxiosError<{ message?: string }>).response?.data?.message ?? fallback;

// ---------- Игрок ----------
const query = ref('');
const players = ref<AdminPlayerInfo[]>([]);
const searched = ref(false);
const player = ref<AdminPlayerInfo | null>(null);
const history = ref<MailInfo[]>([]);
let searchTimer = 0;

watch(query, value => {
    window.clearTimeout(searchTimer);
    searchTimer = window.setTimeout(async () => {
        if (!value.trim()) {
            players.value = [];
            searched.value = false;
            return;
        }
        try {
            players.value = await AdminApiService.findPlayers(value);
        } catch {
            players.value = [];
        }
        searched.value = true;
    }, 250);
});

const selectPlayer = async (selected: AdminPlayerInfo) => {
    player.value = selected;
    await loadHistory();
};

const loadHistory = async () => {
    if (!player.value) return;
    try {
        history.value = await AdminApiService.getPlayerMail(player.value.id);
    } catch {
        history.value = [];
    }
};

// ---------- Каталог ----------
const catalog = ref<AdminMailCatalog | null>(null);
const catalogError = ref<string | null>(null);
const KINDS: MailAttachmentKind[] = ['Hero', 'Equipment', 'Gem', 'Ability', 'Currency'];
const kind = ref<MailAttachmentKind>('Hero');

onMounted(async () => {
    try {
        catalog.value = await AdminApiService.getCatalog();
    } catch (error) {
        catalogError.value = errorText(error, t('inventory.loadError'));
    }
});

// Фильтры — те же, что на складе
const heroes = computed(() => catalog.value?.heroes ?? []);
const { element: heroElement, rank: heroRank, filtered: filteredHeroes } = useHeroFilters(heroes);

const itemType = ref<EquipmentType | null>(null);
const itemRarity = ref<Rarity | null>(null);
const filteredItems = computed(() => (catalog.value?.equipment ?? [])
    .filter(i => (!itemType.value || i.type === itemType.value) && (!itemRarity.value || i.rarity === itemRarity.value)));

const gemKind = ref<string | null>(null);
const gemRarity = ref<Rarity | null>(null);
const gemOptions = computed(() => (catalog.value?.gems ?? []).map(g => ({ gem: g.subType, rarity: g.rarity })));
const filteredGems = computed(() => (catalog.value?.gems ?? [])
    .filter(g => (!gemKind.value || g.subType === gemKind.value) && (!gemRarity.value || g.rarity === gemRarity.value)));

// Способности: вид, стихия (у заклинаний) и уровень
const abilityKind = ref<AbilityKind | null>(null);
const abilityElement = ref<HeroElement | null>(null);
const abilityLevel = ref<number | null>(null);
const abilities = computed(() => catalog.value?.abilities ?? []);
const abilityMatches = (a: AbilityInfo, skip?: 'kind' | 'element' | 'level') =>
    (skip === 'kind' || !abilityKind.value || a.kind === abilityKind.value)
    && (skip === 'element' || !abilityElement.value || a.element === abilityElement.value)
    && (skip === 'level' || !abilityLevel.value || a.level === abilityLevel.value);
const filteredAbilities = computed(() => abilities.value.filter(a => abilityMatches(a)));
const abilityKindOptions = computed<FilterOption<string>[]>(() => (['Spell', 'Passive', 'Aura'] as AbilityKind[]).map(k => ({
    value: k,
    label: t(`abilities.kinds.${k}`),
    count: abilities.value.filter(a => a.kind === k && abilityMatches(a, 'kind')).length,
})));
const abilityElementOptions = computed<FilterOption<string>[]>(() => [...new Set(abilities.value.map(a => a.element).filter(Boolean) as HeroElement[])]
    .map(element => ({
        value: element,
        image: ELEMENT_ICONS[element],
        label: ELEMENT_ICONS[element] ? undefined : t(`elementNames.${element}`),
        color: ELEMENT_COLORS[element],
        tooltip: t(`elementNames.${element}`),
        count: abilities.value.filter(a => a.element === element && abilityMatches(a, 'element')).length,
    })));
const abilityLevelOptions = computed<FilterOption<number>[]>(() => [1, 2, 3].map(level => ({
    value: level,
    label: t('admin.level', { level }),
    count: abilities.value.filter(a => a.level === level && abilityMatches(a, 'level')).length,
})));

// ---------- Карточки каталога → вложения ----------
interface CatalogEntry {
    id: string;
    request: MailAttachmentRequest;
    preview: MailAttachmentInfo;
}

const entry = (id: string, request: Omit<MailAttachmentRequest, 'amount'>, preview: Omit<MailAttachmentInfo, 'id' | 'index' | 'amount' | 'kind'>): CatalogEntry => ({
    id,
    request: { ...request, amount: 1 },
    preview: { id, index: 0, kind: request.kind, amount: 1, ...preview },
});

const entries = computed<CatalogEntry[]>(() => {
    if (!catalog.value) return [];
    switch (kind.value) {
        case 'Hero':
            return filteredHeroes.value.map(hero => entry(
                `hero-${hero.level}-${hero.element}`,
                { kind: 'Hero', key: hero.level === 1 ? '' : hero.element, level: hero.level, rarity: 'Common', abilityKind: null },
                { hero },
            ));
        case 'Equipment':
            return filteredItems.value.map(item => entry(
                `item-${item.key}`,
                { kind: 'Equipment', key: item.key, level: 1, rarity: item.rarity, abilityKind: null },
                { equipment: item },
            ));
        case 'Gem':
            return filteredGems.value.map(gem => entry(
                `gem-${gem.subType}-${gem.rarity}`,
                { kind: 'Gem', key: gem.subType, level: 1, rarity: gem.rarity, abilityKind: null },
                { gem },
            ));
        case 'Ability':
            return filteredAbilities.value.map(ability => entry(
                `ability-${ability.kind}-${ability.key}-${ability.level}`,
                { kind: 'Ability', key: ability.key, level: ability.level, rarity: 'Common', abilityKind: ability.kind },
                { ability },
            ));
        case 'Currency':
            return [...catalog.value.currencies]
                .sort((a, b) => CURRENCY_ORDER.indexOf(a) - CURRENCY_ORDER.indexOf(b))
                .map(currency => entry(
                    `currency-${currency}`,
                    { kind: 'Currency', key: currency, level: 1, rarity: 'Common', abilityKind: null },
                    { currency },
                ));
        default:
            return [];
    }
});

// Количество у каждой карточки каталога (по умолчанию 1)
const amounts = ref<Record<string, number>>({});
const amountOf = (id: string) => Math.max(1, Math.floor(Number(amounts.value[id] ?? 1)) || 1);

// ---------- Письмо ----------
const sender = ref('');
const title = ref('');
const body = ref('');
const draft = ref<CatalogEntry[]>([]);
const sending = ref(false);
const status = ref<{ ok: boolean; text: string } | null>(null);

// Одинаковое (тот же вид, ключ, уровень, ступень) — одним вложением, количества складываются
const add = (item: CatalogEntry) => {
    const amount = amountOf(item.id);
    const existing = draft.value.find(d => d.id === item.id);
    if (existing) {
        existing.request.amount += amount;
        existing.preview.amount += amount;
        if (existing.preview.gem) existing.preview.gem = { ...existing.preview.gem, quantity: existing.preview.amount };
        return;
    }
    const copy: CatalogEntry = {
        id: item.id,
        request: { ...item.request, amount },
        preview: { ...item.preview, amount, gem: item.preview.gem ? { ...item.preview.gem, quantity: amount } : item.preview.gem },
    };
    draft.value.push(copy);
};

// Дворф из конструктора: у каждого свой seed, поэтому с другими не складывается
const addConstructed = (request: MailAttachmentRequest, hero: HeroInfo) => {
    const id = `ctor-${request.seed}-${draft.value.length}`;
    draft.value.push({ id, request, preview: { id, index: 0, kind: 'Hero', amount: request.amount, hero } });
};

const remove = (id: string) => {
    draft.value = draft.value.filter(d => d.id !== id);
};

// Порядок как в письме у игрока
const ABILITY_ORDER: AbilityKind[] = ['Spell', 'Passive', 'Aura'];
const orderedDraft = computed(() => draft.value
    .map((item, index) => ({ item, index }))
    .sort((a, b) => KINDS.indexOf(a.item.request.kind) - KINDS.indexOf(b.item.request.kind)
        || ABILITY_ORDER.indexOf(a.item.request.abilityKind ?? 'Spell') - ABILITY_ORDER.indexOf(b.item.request.abilityKind ?? 'Spell')
        || a.index - b.index)
    .map(x => x.item));

const canSend = computed(() => !!player.value && title.value.trim().length > 0 && !sending.value);

const send = async () => {
    if (!player.value || !canSend.value) return;
    sending.value = true;
    status.value = null;
    try {
        await AdminApiService.send({
            playerId: player.value.id,
            sender: sender.value.trim() || null,
            title: title.value,
            body: body.value,
            attachments: orderedDraft.value.map(d => d.request),
        });
        status.value = { ok: true, text: t('admin.sent', { name: player.value.name }) };
        draft.value = [];
        title.value = '';
        body.value = '';
        await loadHistory();
    } catch (error) {
        status.value = { ok: false, text: errorText(error, t('admin.sendError')) };
    } finally {
        sending.value = false;
    }
};

const dateText = (iso: string) => new Date(iso).toLocaleString(locale.value, { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' });
</script>

<template>
    <div class="admin">
        <header class="admin-header">
            <h1>{{ t('admin.title') }}</h1>
            <span class="admin-warning">{{ t('admin.openWarning') }}</span>
        </header>

        <div class="admin-layout">
            <!-- ===== Игрок ===== -->
            <aside class="admin-panel admin-side">
                <h2>{{ t('admin.playerSearch') }}</h2>
                <input v-model="query" class="admin-input" :placeholder="t('admin.searchPlaceholder')" />
                <div v-if="searched && !players.length" class="admin-hint">{{ t('admin.noPlayers') }}</div>
                <button
                    v-for="found in players"
                    :key="found.id"
                    type="button"
                    class="admin-player"
                    :class="{ 'admin-player-active': player?.id === found.id }"
                    @click="selectPlayer(found)"
                >
                    <b>{{ found.name }}</b>
                    <span>{{ t('admin.rating', { rating: found.rating, power: found.squadPower }) }}</span>
                </button>

                <template v-if="player">
                    <h2>{{ t('admin.history') }}</h2>
                    <div v-if="!history.length" class="admin-hint">{{ t('admin.noHistory') }}</div>
                    <div v-for="mail in history" :key="mail.id" class="admin-history">
                        <b>{{ mail.title }}</b>
                        <span>{{ dateText(mail.createdAt) }} · {{ mail.attachments.length }} 📎</span>
                        <span class="admin-history-flags">
                            <span :class="{ on: mail.isRead }">{{ mail.isRead ? '👁' : '✉' }}</span>
                            <span v-if="mail.attachments.length" :class="{ on: mail.isClaimed }">{{ mail.isClaimed ? '✔' : '⏳' }}</span>
                        </span>
                    </div>
                </template>
            </aside>

            <!-- ===== Письмо ===== -->
            <main class="admin-panel admin-main">
                <h2>
                    {{ t('admin.compose') }}
                    <span v-if="player" class="admin-to">→ {{ player.name }}</span>
                    <span v-else class="admin-hint">{{ t('admin.selectPlayer') }}</span>
                </h2>
                <div class="admin-fields">
                    <input v-model="sender" class="admin-input" :placeholder="t('admin.sender')" maxlength="64" />
                    <input v-model="title" class="admin-input" :placeholder="t('admin.titleField')" maxlength="120" />
                    <textarea v-model="body" class="admin-input admin-textarea" :placeholder="t('admin.body')" maxlength="4000"></textarea>
                </div>

                <!-- Вложения письма — в порядке, в котором их увидит игрок -->
                <h3>{{ t('admin.attachments') }}</h3>
                <div v-if="!draft.length" class="admin-hint">{{ t('admin.noAttachments') }}</div>
                <div class="admin-grid admin-draft">
                    <div v-for="item in orderedDraft" :key="item.id" class="admin-draft-item">
                        <MailAttachmentCard :attachment="item.preview" />
                        <button type="button" class="admin-remove" :title="t('admin.remove')" @click="remove(item.id)">✕</button>
                    </div>
                </div>

                <div class="admin-send">
                    <GemButton color="green" width="200px" height="46px" :disabled="!canSend" @click="send">
                        {{ sending ? t('admin.sending') : t('admin.send') }}
                    </GemButton>
                    <span v-if="status" class="admin-status" :class="{ 'admin-status-error': !status.ok }">{{ status.text }}</span>
                </div>

                <!-- Каталог ресурсов -->
                <div class="admin-resource">
                    <label>
                        {{ t('admin.resource') }}
                        <select v-model="kind" class="admin-input admin-select">
                            <option v-for="k in KINDS" :key="k" :value="k">{{ t(`admin.kinds.${k}`) }}</option>
                        </select>
                    </label>
                </div>
                <div v-if="catalogError" class="admin-hint admin-status-error">{{ catalogError }}</div>

                <HeroConstructor v-if="kind === 'Hero' && catalog" :catalog="catalog" @add="addConstructed" />
                <h3 v-if="kind === 'Hero'">{{ t('admin.constructor.quickPick') }}</h3>

                <div class="admin-filters">
                    <HeroFilters v-if="kind === 'Hero'" v-model:element="heroElement" v-model:rank="heroRank" :heroes="heroes" />
                    <ItemFilters v-if="kind === 'Equipment'" v-model:type="itemType" v-model:rarity="itemRarity" :items="catalog?.equipment ?? []" />
                    <GemFilters v-if="kind === 'Gem'" v-model:gem="gemKind" v-model:rarity="gemRarity" :gems="gemOptions" />
                    <template v-if="kind === 'Ability'">
                        <FilterRow :options="abilityKindOptions" :model-value="abilityKind" :all-label="t('admin.allKinds')" @update:model-value="abilityKind = $event as AbilityKind | null" />
                        <FilterRow :options="abilityElementOptions" :model-value="abilityElement" :all-label="t('admin.allElements')" @update:model-value="abilityElement = $event as HeroElement | null" />
                        <FilterRow :options="abilityLevelOptions" :model-value="abilityLevel" :all-label="t('admin.allLevels')" @update:model-value="abilityLevel = $event as number | null" />
                    </template>
                </div>

                <div class="admin-grid">
                    <div v-for="item in entries" :key="item.id" class="admin-entry">
                        <MailAttachmentCard :attachment="item.preview" />
                        <div class="admin-entry-actions">
                            <input
                                v-model.number="amounts[item.id]"
                                class="admin-input admin-amount"
                                type="number"
                                min="1"
                                :placeholder="t('admin.amount')"
                                :title="t('admin.amount')"
                            />
                            <button type="button" class="admin-add" @click="add(item)">{{ t('admin.add') }}</button>
                        </div>
                    </div>
                </div>
            </main>
        </div>
    </div>
</template>

<style scoped>
.admin {
    position: fixed;
    width: var(--stage-w);
    height: var(--stage-h);
    overflow-y: auto;
    box-sizing: border-box;
    padding: 90px 20px 40px;
    background: radial-gradient(ellipse at 50% 0%, #1d2740 0%, #0b0f1a 70%);
    color: #e2e8f0;
    font-family: 'Arial', sans-serif;
}

.admin-header {
    display: flex;
    align-items: baseline;
    gap: 16px;
    margin-bottom: 14px;
}

.admin-header h1 {
    margin: 0;
    font-size: 26px;
}

.admin-warning {
    font-size: 13px;
    color: #fbbf24;
}

.admin-layout {
    display: grid;
    grid-template-columns: 300px 1fr;
    gap: 16px;
    align-items: start;
}

@media (max-width: 900px) {
    .admin-layout {
        grid-template-columns: 1fr;
    }
}

.admin-panel {
    padding: 14px;
    border-radius: 14px;
    border: 1px solid rgba(255, 255, 255, 0.1);
    background: rgba(14, 19, 34, 0.75);
}

.admin-side {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.admin-panel h2 {
    margin: 4px 0 8px;
    font-size: 17px;
    display: flex;
    align-items: baseline;
    gap: 10px;
}

.admin-panel h3 {
    margin: 16px 0 6px;
    font-size: 15px;
}

.admin-to {
    color: #ffd166;
}

.admin-hint {
    font-size: 13px;
    font-weight: 400;
    color: #94a3b8;
}

.admin-input {
    box-sizing: border-box;
    width: 100%;
    padding: 8px 10px;
    border-radius: 8px;
    border: 1px solid rgba(255, 255, 255, 0.18);
    background: rgba(0, 0, 0, 0.35);
    color: #f1f5f9;
    font: 14px 'Arial', sans-serif;
}

.admin-input:focus {
    outline: none;
    border-color: #ffd166;
}

.admin-textarea {
    min-height: 110px;
    resize: vertical;
}

.admin-fields {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.admin-player {
    display: flex;
    flex-direction: column;
    gap: 2px;
    padding: 8px 10px;
    border-radius: 8px;
    border: 1px solid rgba(255, 255, 255, 0.1);
    background: rgba(255, 255, 255, 0.04);
    color: inherit;
    text-align: left;
    cursor: pointer;
}

.admin-player span {
    font-size: 12px;
    color: #94a3b8;
}

.admin-player:hover {
    background: rgba(255, 255, 255, 0.09);
}

.admin-player-active {
    border-color: #ffd166;
    background: rgba(255, 209, 102, 0.12);
}

.admin-history {
    display: grid;
    grid-template-columns: 1fr auto;
    gap: 2px 8px;
    padding: 6px 8px;
    border-radius: 8px;
    background: rgba(255, 255, 255, 0.04);
    font-size: 13px;
}

.admin-history span {
    font-size: 12px;
    color: #94a3b8;
}

.admin-history-flags {
    grid-row: 1 / 3;
    grid-column: 2;
    display: flex;
    gap: 6px;
    align-items: center;
}

.admin-history-flags .on {
    color: #4ade80;
}

.admin-resource {
    margin-top: 22px;
    padding-top: 14px;
    border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.admin-resource label {
    display: flex;
    align-items: center;
    gap: 10px;
    font-weight: 700;
}

.admin-select {
    width: auto;
    min-width: 200px;
}

.admin-filters {
    display: flex;
    flex-direction: column;
    gap: 6px;
    margin: 12px 0;
}

.admin-grid {
    display: flex;
    flex-wrap: wrap;
    gap: 14px;
    align-items: flex-start;
}

.admin-entry,
.admin-draft-item {
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 6px;
}

.admin-entry-actions {
    display: flex;
    gap: 4px;
}

.admin-amount {
    width: 74px;
    padding: 5px 6px;
}

.admin-add,
.admin-remove {
    border: 0;
    border-radius: 8px;
    cursor: pointer;
    font: 700 13px 'Arial', sans-serif;
}

.admin-add {
    padding: 5px 10px;
    background: #2f855a;
    color: #fff;
}

.admin-add:hover {
    background: #38a169;
}

.admin-remove {
    position: absolute;
    top: -8px;
    left: -8px;
    z-index: 5;
    width: 24px;
    height: 24px;
    background: #b91c1c;
    color: #fff;
}

.admin-send {
    display: flex;
    align-items: center;
    gap: 14px;
    margin-top: 14px;
}

.admin-status {
    color: #4ade80;
    font-size: 14px;
}

.admin-status-error {
    color: #f87171;
}
</style>
