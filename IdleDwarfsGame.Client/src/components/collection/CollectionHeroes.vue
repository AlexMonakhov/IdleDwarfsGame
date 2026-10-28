<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import CollectionSplit from './CollectionSplit.vue';
import CssAuraEffect from '@/components/effects/CssAuraEffect.vue';
import UnitAvatar from '@/components/interface/UnitAvatar.vue';
import { STATUS_ICONS } from '@/components/icons/status';
import { elementAura } from '@/components/effects/elementAura';
import { DAMAGE_COLORS, resolveSpellVisual } from '@/battle/FxRegistry';
import { elementColor } from '@/battle/sprites';
import { statusLabel } from '@/i18n';
import { DEFAULT_SPELL_LEVEL, HERO_KINDS, HeroKind, PREVIEW_RANK, SPELL_LEVELS, SpellSample } from './catalog';
import { CollectionPreview, spellScene } from './previewScenes';

// Все виды героев на 5-м ранге и их заклинания; выбранное заклинание на выбранном уровне крутится на сцене
const { t } = useI18n();

const selected = ref<{ kind: HeroKind; spell: SpellSample } | null>(null);
// Уровень не сбрасывается при переключении заклинаний — удобно сравнивать их на одном уровне
const level = ref<number>(DEFAULT_SPELL_LEVEL);
const current = computed(() => selected.value?.spell.levels[level.value - 1] ?? null);

const spellColor = (spell: SpellSample) =>
    (spell.levels[0].damage ? resolveSpellVisual(spell.name, spell.damageType).color : DAMAGE_COLORS.Heal);

const isSelected = (kind: HeroKind, spell?: SpellSample) =>
    selected.value?.kind === kind && (!spell || selected.value.spell === spell);

// Клик по карточке открывает первое заклинание героя, повторный — закрывает панель
const selectHero = (kind: HeroKind) => {
    selected.value = isSelected(kind) ? null : { kind, spell: kind.spells[0] };
};

const selectSpell = (kind: HeroKind, spell: SpellSample) => {
    selected.value = { kind, spell };
};

const preview = computed<CollectionPreview | null>(() => {
    if (!selected.value) return null;
    const { kind, spell } = selected.value;
    return {
        key: `${kind.element}-${spell.key}-${level.value}`,
        scene: spellScene(kind.element, spell, level.value),
        element: kind.element,
        title: spell.name,
        color: spellColor(spell),
        description: t(`collection.spellDescriptions.${spell.key}`),
    };
});

const avatar = (kind: HeroKind) => ({
    name: t(`collection.heroNames.${kind.element}`),
    unitType: 'Dwarf',
    element: kind.element,
    level: PREVIEW_RANK,
});
</script>

<template>
    <CollectionSplit :hint="selected ? t('collection.switchHint') : t('collection.heroesHint')" :preview="preview">
        <template v-if="selected" #switcher>
            <div class="preview-switchers">
                <div class="spell-switcher">
                    <button
                        v-for="spell in selected.kind.spells"
                        :key="spell.key"
                        class="spell-chip"
                        :class="{ 'spell-chip-active': spell === selected.spell }"
                        :style="{ '--spell-color': spellColor(spell) }"
                        @click="selectSpell(selected.kind, spell)"
                    >{{ spell.name }}</button>
                </div>
                <div class="spell-switcher" :title="t('collection.spellLevelHint')">
                    <button
                        v-for="value in SPELL_LEVELS"
                        :key="value"
                        class="spell-chip"
                        :class="{ 'spell-chip-active': value === level }"
                        :style="{ '--spell-color': spellColor(selected.spell) }"
                        @click="level = value"
                    >{{ t('collection.spellLevel', { level: value }) }}</button>
                </div>
            </div>
        </template>

        <template v-if="selected && current" #tags>
            <span v-if="current.damage" class="collection-chip" :style="{ '--chip-color': DAMAGE_COLORS[selected.spell.damageType] }">
                {{ t(`collection.damageTypes.${selected.spell.damageType}`) }}
            </span>
            <span v-if="current.heal" class="collection-chip" :style="{ '--chip-color': DAMAGE_COLORS.Heal }">
                {{ current.damage ? t('collection.selfHealing') : t('collection.healing') }}
            </span>
            <span v-if="current.statuses?.length" class="collection-chip">
                <span class="collection-chip-label">{{ t('collection.statuses') }}</span>
                <span v-for="name in current.statuses" :key="name" class="spell-status" :title="statusLabel(name)">
                    <component v-if="STATUS_ICONS[name]" :is="STATUS_ICONS[name]" size="14px" />
                    {{ statusLabel(name) }}
                </span>
            </span>
        </template>

        <div
            v-for="kind in HERO_KINDS"
            :key="kind.element"
            class="codex-hero"
            :class="[`codex-hero-${elementColor(kind.element)}`, { 'codex-hero-selected': isSelected(kind) }]"
            @click="selectHero(kind)"
        >
            <div class="codex-hero-portrait">
                <CssAuraEffect :effect="elementAura(kind.element)" :size="64">
                    <UnitAvatar :unit="avatar(kind)" :size="64" :show-level="false" />
                </CssAuraEffect>
            </div>
            <div class="codex-hero-info">
                <div class="codex-hero-name">{{ t(`collection.heroNames.${kind.element}`) }}</div>
                <div class="codex-hero-element">
                    {{ t(`elements.${kind.element}`) }}
                    <span class="codex-hero-rank" :title="t('heroUpgrade.rank', { rank: PREVIEW_RANK })">{{ '★'.repeat(PREVIEW_RANK) }}</span>
                </div>
                <div class="codex-hero-spells">
                    <button
                        v-for="spell in kind.spells"
                        :key="spell.key"
                        class="spell-chip"
                        :class="{ 'spell-chip-active': isSelected(kind, spell) }"
                        :style="{ '--spell-color': spellColor(spell) }"
                        @click.stop="selectSpell(kind, spell)"
                    >{{ spell.name }}</button>
                </div>
            </div>
        </div>
    </CollectionSplit>
</template>

<style scoped>
/* Карточка героя: портрет в ауре стихии и кнопки его заклинаний */
.codex-hero {
    box-sizing: border-box;
    width: 300px;
    display: flex;
    align-items: center;
    column-gap: 12px;
    padding: 12px;
    border-radius: 14px;
    border: 2px solid color-mix(in srgb, var(--card-accent) 55%, #333);
    background:
        radial-gradient(circle at 15% 50%, color-mix(in srgb, var(--card-accent) 22%, transparent), transparent 60%),
        rgba(15, 23, 32, 0.85);
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5);
    cursor: pointer;
    transition: transform 0.2s cubic-bezier(0.34, 1.4, 0.64, 1), box-shadow 0.25s ease, border-color 0.25s ease;
}

.codex-hero:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 16px 2px color-mix(in srgb, var(--card-accent) 50%, transparent);
}

.codex-hero-selected,
.codex-hero-selected:hover {
    border-color: #ffd166;
    box-shadow: 0 0 0 1px #ffd166, 0 0 18px 4px rgba(255, 209, 102, 0.55);
}

.codex-hero-red { --card-accent: var(--red-gem-color); }
.codex-hero-blue { --card-accent: var(--blue-gem-color); }
.codex-hero-yellow { --card-accent: var(--yellow-gem-color); }
.codex-hero-green { --card-accent: var(--green-gem-color); }
.codex-hero-purple { --card-accent: var(--purple-gem-color); }
.codex-hero-white { --card-accent: var(--white-gem-color); }

/* Аура стихии рисуется вокруг портрета и не должна наезжать на текст */
.codex-hero-portrait {
    flex-shrink: 0;
    padding: 6px;
}

.codex-hero-info {
    min-width: 0;
    display: flex;
    flex-direction: column;
    row-gap: 4px;
}

.codex-hero-name {
    color: #f8fafc;
    font-size: 15px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.codex-hero-element {
    color: color-mix(in srgb, var(--card-accent) 75%, white);
    font-size: 12px;
    font-weight: bold;
}

.codex-hero-rank {
    margin-left: 4px;
    color: #ffd166;
    letter-spacing: -1px;
    text-shadow: 0 0 6px rgba(255, 209, 102, 0.6);
}

/* Справа от заголовка: заклинания героя, под ними уровни */
.preview-switchers {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    row-gap: 4px;
}

.codex-hero-spells,
.spell-switcher {
    display: flex;
    flex-wrap: wrap;
    gap: 4px;
}

/* Кнопка заклинания: окрашена цветом его визуала */
.spell-chip {
    padding: 3px 8px;
    border-radius: 8px;
    border: 1px solid color-mix(in srgb, var(--spell-color) 60%, transparent);
    background: rgba(0, 0, 0, 0.55);
    color: var(--spell-color);
    font-family: 'Arial', sans-serif;
    font-size: 11px;
    font-weight: bold;
    white-space: nowrap;
    text-shadow: 0 1px 2px #000;
    cursor: pointer;
    transition: background 0.15s ease, box-shadow 0.15s ease;
}

.spell-chip:hover {
    background: color-mix(in srgb, var(--spell-color) 20%, rgba(0, 0, 0, 0.55));
}

.spell-chip-active {
    background: color-mix(in srgb, var(--spell-color) 30%, rgba(0, 0, 0, 0.5));
    box-shadow: 0 0 8px color-mix(in srgb, var(--spell-color) 70%, transparent);
}

.spell-status {
    display: inline-flex;
    align-items: center;
    column-gap: 3px;
    color: #f8fafc;
}
</style>
