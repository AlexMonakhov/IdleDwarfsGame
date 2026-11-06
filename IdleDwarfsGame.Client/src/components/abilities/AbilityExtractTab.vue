<script lang="ts" setup>
import { computed, ref, toRef, watch } from 'vue';
import HeroFilters from '@/components/inventory/HeroFilters.vue';
import { useHeroFilters } from '@/composables/UseHeroFilters';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import HeroCard from '@/components/inventory/HeroCard.vue';
import AbilitiesApiService from '@/services/apiServices/AbilitiesApiService';
import { AbilityActionResult, AbilityInfo } from '@/models/types/Abilities';
import { HeroInfo, isBusy } from '@/models/types/Heroes';
import { statusLabel } from '@/i18n';
import { vDragScroll } from '@/directives/dragScroll';
import AbilityTile from './AbilityTile.vue';
import { abilityErrorText, abilityName } from './abilityText';

// Извлечение: выбранный дворф распыляется, одна его способность остаётся у игрока
const props = defineProps<{ heroes: HeroInfo[] }>();
const emit = defineEmits<{
    (e: 'extracted', result: AbilityActionResult): void;
    // Сервер отказал — данные устарели, окну стоит перечитать их
    (e: 'stale'): void;
}>();

const { t } = useI18n();

const heroId = ref<string | null>(null);
const picked = ref<AbilityInfo | null>(null);
// Первое нажатие «Извлечь» только спрашивает: дворф пропадёт насовсем
const confirming = ref(false);
const busy = ref(false);
const message = ref<{ text: string; error: boolean } | null>(null);

const hero = computed(() => props.heroes.find(h => h.id === heroId.value) ?? null);
const abilities = computed(() => hero.value?.abilities ?? []);

// Сначала дворфы, из которых есть что достать
// Фильтры по стихии и рангу
const { element: elementFilter, rank: rankFilter, filtered } = useHeroFilters(toRef(props, 'heroes'));

const sorted = computed(() => [...filtered.value].sort((a, b) =>
    Number(!a.abilities?.length) - Number(!b.abilities?.length)
    || b.level - a.level
    || a.name.localeCompare(b.name)));

const lastHero = computed(() => props.heroes.length <= 1);

watch(heroId, () => {
    picked.value = abilities.value[0] ?? null;
    confirming.value = false;
});

watch(picked, () => { confirming.value = false; });

const select = (target: HeroInfo) => {
    if (isBusy(target)) return;
    message.value = null;
    heroId.value = heroId.value === target.id ? null : target.id;
};

const extract = async () => {
    const target = hero.value;
    const ability = picked.value;
    if (!target || !ability || busy.value || lastHero.value) return;
    if (!confirming.value) {
        confirming.value = true;
        return;
    }

    busy.value = true;
    try {
        const result = await AbilitiesApiService.extract(target.id, ability.kind, ability.key);
        message.value = { text: t('abilities.extract.done', { ability: abilityName(ability) }), error: false };
        heroId.value = null;
        emit('extracted', result);
    } catch (error) {
        message.value = { text: abilityErrorText(error), error: true };
        emit('stale');
    } finally {
        busy.value = false;
        confirming.value = false;
    }
};

const buttonText = computed(() => {
    if (busy.value) return t('abilities.extract.extracting');
    if (confirming.value && hero.value) return t('abilities.extract.confirm', { name: hero.value.name });
    return t('abilities.extract.button');
});
</script>

<template>
    <div class="forge">
        <aside class="forge-panel">
            <template v-if="hero">
                <h4 class="forge-title">{{ t('abilities.extract.title', { name: hero.name }) }}</h4>
                <div v-if="abilities.length" class="forge-tiles">
                    <AbilityTile
                        v-for="ability in abilities"
                        :key="`${ability.kind}-${ability.key}`"
                        :ability="ability"
                        :selected="picked?.kind === ability.kind && picked.key === ability.key"
                        @click="picked = ability"
                    />
                </div>
                <div v-else class="forge-note">{{ t('abilities.extract.nothing') }}</div>

                <div v-for="name in hero.immunities" :key="name" class="forge-note forge-note-warn">
                    {{ t('abilities.extract.immunity', { effect: statusLabel(name) }) }}
                </div>
                <div v-if="abilities.length" class="forge-note">{{ t('abilities.extract.lost') }}</div>
                <div v-if="lastHero" class="forge-note forge-note-warn">{{ t('abilities.errors.LastHero') }}</div>

                <GemButton
                    class="forge-action"
                    :color="confirming ? 'red' : 'pink'"
                    :disabled="!picked || busy || lastHero"
                    @click="extract"
                >{{ buttonText }}</GemButton>
            </template>
            <div v-else class="forge-note">{{ t('abilities.extract.chooseHero') }}</div>

            <div v-if="message" class="forge-message" :class="{ 'forge-message-error': message.error }">{{ message.text }}</div>
        </aside>

        <section v-drag-scroll class="forge-list">
            <HeroFilters v-model:element="elementFilter" v-model:rank="rankFilter" :heroes="heroes" />
            <div class="forge-grid">
                <HeroCard
                    v-for="item in sorted"
                    :key="item.id"
                    v-tooltip="isBusy(item) ? t('abilities.busy') : null"
                    :hero="item"
                    selectable
                    :disabled="isBusy(item)"
                    :selected="item.id === heroId"
                    @click="select(item)"
                />
            </div>
            <div v-if="!heroes.length" class="forge-note">{{ t('inventory.noHeroes') }}</div>
        </section>
    </div>
</template>

<style scoped src="./forge.css"></style>
