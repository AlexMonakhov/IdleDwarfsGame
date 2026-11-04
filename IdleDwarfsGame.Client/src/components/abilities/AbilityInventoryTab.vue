<script lang="ts" setup>
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import HeroCard from '@/components/inventory/HeroCard.vue';
import AbilitiesApiService from '@/services/apiServices/AbilitiesApiService';
import { ABILITY_KINDS, AbilityActionResult, AbilityInfo, AbilityKind, canCombine, canInsert } from '@/models/types/Abilities';
import { HeroInfo } from '@/models/types/Heroes';
import { vDragScroll } from '@/directives/dragScroll';
import AbilityTile from './AbilityTile.vue';
import { abilityErrorText, abilityName, sameAbility } from './abilityText';

// Извлечённые способности: выбранную вставляют дворфу или сливают с такой же в одну уровнем выше
const props = defineProps<{
    heroes: HeroInfo[];
    abilities: AbilityInfo[];
}>();
const emit = defineEmits<{
    (e: 'changed', result: AbilityActionResult): void;
    // Сервер отказал — данные устарели, окну стоит перечитать их
    (e: 'stale'): void;
}>();

const { t } = useI18n();

type Mode = 'combine' | 'insert';

const selectedId = ref<string | null>(null);
const mode = ref<Mode>('combine');
const partnerId = ref<string | null>(null);
const heroId = ref<string | null>(null);
const filter = ref<AbilityKind | null>(null);
const busy = ref(false);
const message = ref<{ text: string; error: boolean } | null>(null);

const selected = computed(() => props.abilities.find(a => a.id === selectedId.value) ?? null);
const partner = computed(() => props.abilities.find(a => a.id === partnerId.value) ?? null);
const hero = computed(() => props.heroes.find(h => h.id === heroId.value) ?? null);

const visible = computed(() => props.abilities.filter(a => !filter.value || a.kind === filter.value));
const candidates = computed(() => {
    const current = selected.value;
    return current ? props.abilities.filter(a => canCombine(current, a)) : [];
});
const isCandidate = (ability: AbilityInfo) => candidates.value.some(c => c.id === ability.id);

// Что потеряет дворф: вставленная способность заменяет его способности того же вида
const replaced = computed(() => {
    if (!hero.value || !selected.value) return [];
    const kind = selected.value.kind;
    return (hero.value.abilities ?? []).filter(a => a.kind === kind);
});

// Сначала те, кому выбранную способность можно вставить
const fits = (target: HeroInfo) => !selected.value || canInsert(selected.value, target);
const sortedHeroes = computed(() => [...props.heroes]
    .sort((a, b) => Number(fits(b)) - Number(fits(a)) || b.level - a.level || a.name.localeCompare(b.name)));

const insertBlocked = (target: HeroInfo) =>
    fits(target) ? null : t('abilities.inventory.wrongElement', { element: t(`elements.${selected.value?.element}`) });

// Выбранная способность исчезла (её использовали) — сбрасываем выбор
watch(() => props.abilities, () => {
    if (selectedId.value && !selected.value) selectedId.value = null;
    if (partnerId.value && !partner.value) partnerId.value = null;
});

watch(selectedId, () => {
    partnerId.value = null;
    heroId.value = null;
});

const combineText = computed(() => {
    if (busy.value) return t('abilities.inventory.combining');
    if (!selected.value || !partner.value) return t('abilities.inventory.combineTitle');
    return t('abilities.inventory.combine', { level: selected.value.level + partner.value.level });
});

const pickAbility = (ability: AbilityInfo) => {
    message.value = null;
    if (selected.value && mode.value === 'combine' && isCandidate(ability)) {
        partnerId.value = partnerId.value === ability.id ? null : ability.id;
        return;
    }
    selectedId.value = selectedId.value === ability.id ? null : ability.id;
};

const pickHero = (target: HeroInfo) => {
    message.value = null;
    if (!fits(target)) return;
    heroId.value = heroId.value === target.id ? null : target.id;
};

const run = async (request: () => Promise<AbilityActionResult>, success: (result: AbilityActionResult) => string) => {
    if (busy.value) return;
    busy.value = true;
    try {
        const result = await request();
        message.value = { text: success(result), error: false };
        emit('changed', result);
        return result;
    } catch (error) {
        message.value = { text: abilityErrorText(error), error: true };
        emit('stale');
    } finally {
        busy.value = false;
    }
};

const combine = async () => {
    const first = selected.value;
    const second = partner.value;
    const firstId = first?.id;
    const secondId = second?.id;
    if (!first || !second || !firstId || !secondId) return;
    const level = first.level + second.level;
    const before = new Set(props.abilities.map(a => a.id));

    const result = await run(
        () => AbilitiesApiService.combine(firstId, secondId),
        () => t('abilities.inventory.combined', { ability: abilityName(first), level }),
    );
    // Сразу выбираем получившуюся: её можно тут же слить ещё раз или вставить
    const combined = result?.abilities.find(a => !before.has(a.id) && sameAbility(a, first));
    if (combined) selectedId.value = combined.id;
};

const insert = async () => {
    const ability = selected.value;
    const target = hero.value;
    const abilityId = ability?.id;
    if (!ability || !abilityId || !target) return;

    const result = await run(
        () => AbilitiesApiService.insert(abilityId, target.id),
        () => t('abilities.inventory.inserted', { name: target.name, ability: abilityName(ability) }),
    );
    if (result) selectedId.value = null;
};
</script>

<template>
    <div class="forge">
        <aside class="forge-panel">
            <template v-if="selected">
                <h4 class="forge-title">{{ t('abilities.inventory.selected', { ability: abilityName(selected) }) }}</h4>
                <div class="forge-tiles">
                    <AbilityTile :ability="selected" selected />
                </div>

                <div class="forge-filters">
                    <button
                        class="forge-filter"
                        :class="{ 'forge-filter-active': mode === 'combine' }"
                        @click="mode = 'combine'"
                    >{{ t('abilities.inventory.combineTitle') }}</button>
                    <button
                        class="forge-filter"
                        :class="{ 'forge-filter-active': mode === 'insert' }"
                        @click="mode = 'insert'"
                    >{{ t('abilities.inventory.insertTitle') }}</button>
                </div>

                <template v-if="mode === 'combine'">
                    <div class="forge-note">{{ t('abilities.inventory.combineHint', { max: selected.maxLevel }) }}</div>
                    <div v-if="selected.level >= selected.maxLevel" class="forge-note forge-note-warn">{{ t('abilities.inventory.maxed') }}</div>
                    <div v-else-if="!candidates.length" class="forge-note forge-note-warn">{{ t('abilities.inventory.combineNone') }}</div>
                    <GemButton
                        v-if="candidates.length"
                        class="forge-action"
                        color="yellow"
                        :disabled="!partner || busy"
                        @click="combine"
                    >{{ combineText }}</GemButton>
                </template>

                <template v-else>
                    <div class="forge-note">{{ t('abilities.inventory.insertHint') }}</div>
                    <div v-if="selected.kind === 'Spell' && selected.element" class="forge-note forge-note-warn">
                        {{ t('abilities.inventory.spellElement', { element: t(`elements.${selected.element}`) }) }}
                    </div>
                    <div v-if="replaced.length" class="forge-note forge-note-warn">
                        {{ t('abilities.inventory.replaces', { ability: replaced.map(abilityName).join(', ') }) }}
                    </div>
                    <GemButton
                        class="forge-action"
                        color="green"
                        :disabled="!hero || busy"
                        @click="insert"
                    >{{ busy ? t('abilities.inventory.inserting') : t('abilities.inventory.insert', { name: hero?.name ?? '…' }) }}</GemButton>
                </template>
            </template>
            <div v-else class="forge-note">{{ abilities.length ? t('abilities.inventory.hint') : t('abilities.inventory.empty') }}</div>

            <div v-if="message" class="forge-message" :class="{ 'forge-message-error': message.error }">{{ message.text }}</div>
        </aside>

        <section v-drag-scroll class="forge-list">
            <div v-if="selected && mode === 'insert'" class="forge-grid">
                <HeroCard
                    v-for="item in sortedHeroes"
                    :key="item.id"
                    v-tooltip="insertBlocked(item)"
                    :hero="item"
                    selectable
                    :disabled="!fits(item)"
                    :selected="item.id === heroId"
                    @click="pickHero(item)"
                />
            </div>
            <template v-else>
                <div class="forge-filters">
                    <button
                        class="forge-filter"
                        :class="{ 'forge-filter-active': !filter }"
                        @click="filter = null"
                    >{{ t('abilities.all') }}</button>
                    <button
                        v-for="kind in ABILITY_KINDS"
                        :key="kind"
                        class="forge-filter"
                        :class="{ 'forge-filter-active': filter === kind }"
                        @click="filter = kind"
                    >{{ t(`abilities.kinds.${kind}`) }}</button>
                </div>
                <div class="forge-grid">
                    <AbilityTile
                        v-for="ability in visible"
                        :key="ability.id ?? ability.key"
                        :ability="ability"
                        :selected="ability.id === selectedId || ability.id === partnerId"
                        :highlighted="mode === 'combine' && isCandidate(ability)"
                        @click="pickAbility(ability)"
                    />
                </div>
            </template>
        </section>
    </div>
</template>

<style scoped src="./forge.css"></style>
