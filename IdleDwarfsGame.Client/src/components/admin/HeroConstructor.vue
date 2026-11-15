<script lang="ts" setup>
import { computed, reactive, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { AxiosError } from 'axios';
import HeroCard from '@/components/inventory/HeroCard.vue';
import AdminApiService from '@/services/apiServices/AdminApiService';
import { statusLabel } from '@/i18n';
import type { HeroElement } from '@/battle/types';
import type { HeroInfo, HeroProfession } from '@/models/types/Heroes';
import type { AdminMailCatalog, MailAttachmentRequest, MailHeroSpec } from '@/models/types/Mail';

// Конструктор дворфа для письма. Незаданное выпадает случайно из seed, но превью и письмо используют один seed —
// игрок получит ровно того дворфа, что видно здесь. «Свои способности» без выбранных — дворф без них (пустой).
// Иммунитет — только отмеченный, случайного у дворфа из конструктора не бывает
const props = defineProps<{ catalog: AdminMailCatalog }>();
const emit = defineEmits<{ (e: 'add', request: MailAttachmentRequest, preview: HeroInfo): void }>();

const { t, te } = useI18n();

const ELEMENTS: HeroElement[] = ['Fire', 'Frost', 'Lightning', 'Nature', 'Dark', 'Radiance', 'Blood'];

const form = reactive({
    level: 4,
    element: '' as HeroElement | '',
    profession: null as HeroProfession | null,
    name: '',
    customAbilities: false,
    spellFamily: null as number | null,
    spellLevel: 1,
    passive: null as string | null,
    passiveLevel: 1,
    aura: null as string | null,
    auraLevel: 1,
    immunities: [] as string[],
    amount: 1,
});

const seed = ref<number | null>(null);
const preview = ref<HeroInfo | null>(null);
const error = ref<string | null>(null);

// Заклинания стихии дворфа (семейства — первые уровни из каталога)
const spellOptions = computed(() => props.catalog.abilities
    .filter(a => a.kind === 'Spell' && a.level === 1 && a.element === form.element)
    .map(a => ({ family: Number(a.key), name: a.spellName ?? a.key })));
const passiveOptions = computed(() => [...new Set(props.catalog.abilities.filter(a => a.kind === 'Passive').map(a => a.key))]);
const auraOptions = computed(() => [...new Set(props.catalog.abilities.filter(a => a.kind === 'Aura').map(a => a.key))]);

const passiveName = (key: string) => (te(`passives.names.${key}`) ? t(`passives.names.${key}`) : key);

// На 1-м ранге стихии нет, а заклинание — только своей стихии
watch(() => form.level, level => {
    if (level === 1) form.element = '';
});
watch(() => form.element, () => {
    if (!spellOptions.value.some(s => s.family === form.spellFamily)) form.spellFamily = null;
});

const spec = computed<MailHeroSpec>(() => ({
    name: form.name.trim() || null,
    profession: form.profession,
    customAbilities: form.customAbilities,
    spellFamily: form.customAbilities ? form.spellFamily : null,
    spellLevel: form.spellLevel,
    passive: form.customAbilities ? form.passive : null,
    passiveLevel: form.passiveLevel,
    aura: form.customAbilities ? form.aura : null,
    auraLevel: form.auraLevel,
    immunities: [...form.immunities],
}));

const element = computed(() => (form.level === 1 ? '' : form.element));

// Превью перечитываем при любом изменении; seed сохраняется — меняется только то, что поправили
let timer = 0;
const loadPreview = () => {
    window.clearTimeout(timer);
    timer = window.setTimeout(async () => {
        try {
            const result = await AdminApiService.previewHero({ level: form.level, element: element.value, seed: seed.value, heroSpec: spec.value });
            seed.value = result.seed;
            preview.value = result.hero;
            error.value = null;
        } catch (e) {
            error.value = (e as AxiosError<{ message?: string }>).response?.data?.message ?? String(e);
        }
    }, 200);
};
watch([spec, element, () => form.level], loadPreview, { deep: true, immediate: true });

const reroll = () => {
    seed.value = null;
    loadPreview();
};

const toggleImmunity = (name: string) => {
    form.immunities = form.immunities.includes(name) ? form.immunities.filter(i => i !== name) : [...form.immunities, name];
};

// Пресет: дворф 4-го ранга совсем без способностей и иммунитетов
const emptyRankFour = () => {
    Object.assign(form, {
        level: 4, customAbilities: true, spellFamily: null, passive: null, aura: null, immunities: [],
    });
};

const add = () => {
    if (!preview.value || seed.value === null || error.value) return;
    emit('add', {
        kind: 'Hero',
        key: element.value,
        level: form.level,
        rarity: 'Common',
        abilityKind: null,
        amount: Math.max(1, Math.floor(form.amount) || 1),
        seed: seed.value,
        heroSpec: spec.value,
    }, preview.value);
    // Следующий дворф — с новым seed, чтобы не прислать двух одинаковых случайно
    reroll();
};
</script>

<template>
    <section class="constructor">
        <h3>{{ t('admin.constructor.title') }}</h3>
        <div class="constructor-body">
            <div class="constructor-form">
                <label>
                    {{ t('admin.constructor.rank') }}
                    <select v-model.number="form.level" class="admin-input">
                        <option v-for="rank in 4" :key="rank" :value="rank">{{ rank }}</option>
                    </select>
                </label>
                <label>
                    {{ t('admin.constructor.element') }}
                    <select v-model="form.element" class="admin-input" :disabled="form.level === 1">
                        <option value="">{{ t('admin.constructor.random') }}</option>
                        <option v-for="e in ELEMENTS" :key="e" :value="e">{{ t(`elementNames.${e}`) }}</option>
                    </select>
                </label>
                <label>
                    {{ t('admin.constructor.profession') }}
                    <select v-model="form.profession" class="admin-input">
                        <option :value="null">{{ t('admin.constructor.random') }}</option>
                        <option v-for="p in catalog.professions" :key="p" :value="p">{{ t(`professions.${p}`) }}</option>
                    </select>
                </label>
                <label>
                    {{ t('admin.constructor.name') }}
                    <input v-model="form.name" class="admin-input" maxlength="32" :placeholder="t('admin.constructor.random')" />
                </label>

                <div class="constructor-mode">
                    <label><input v-model="form.customAbilities" type="radio" :value="false" /> {{ t('admin.constructor.randomAbilities') }}</label>
                    <label><input v-model="form.customAbilities" type="radio" :value="true" /> {{ t('admin.constructor.customAbilities') }}</label>
                </div>

                <template v-if="form.customAbilities">
                    <label class="constructor-ability">
                        {{ t('abilities.kinds.Spell') }}
                        <select v-model="form.spellFamily" class="admin-input" :disabled="!element">
                            <option :value="null">{{ t('admin.constructor.none') }}</option>
                            <option v-for="s in spellOptions" :key="s.family" :value="s.family">{{ s.name }}</option>
                        </select>
                        <select v-model.number="form.spellLevel" class="admin-input constructor-level" :disabled="form.spellFamily === null">
                            <option v-for="l in 3" :key="l" :value="l">{{ t('admin.level', { level: l }) }}</option>
                        </select>
                    </label>
                    <span v-if="!element" class="admin-hint">{{ t('admin.constructor.spellNeedsElement') }}</span>
                    <label class="constructor-ability">
                        {{ t('abilities.kinds.Passive') }}
                        <select v-model="form.passive" class="admin-input">
                            <option :value="null">{{ t('admin.constructor.none') }}</option>
                            <option v-for="p in passiveOptions" :key="p" :value="p">{{ passiveName(p) }}</option>
                        </select>
                        <select v-model.number="form.passiveLevel" class="admin-input constructor-level" :disabled="form.passive === null">
                            <option v-for="l in 3" :key="l" :value="l">{{ t('admin.level', { level: l }) }}</option>
                        </select>
                    </label>
                    <label class="constructor-ability">
                        {{ t('abilities.kinds.Aura') }}
                        <select v-model="form.aura" class="admin-input">
                            <option :value="null">{{ t('admin.constructor.none') }}</option>
                            <option v-for="a in auraOptions" :key="a" :value="a">{{ statusLabel(a) }}</option>
                        </select>
                        <select v-model.number="form.auraLevel" class="admin-input constructor-level" :disabled="form.aura === null">
                            <option v-for="l in 3" :key="l" :value="l">{{ t('admin.level', { level: l }) }}</option>
                        </select>
                    </label>
                </template>

                <div class="constructor-immunities">
                    <span>{{ t('admin.constructor.immunities') }}</span>
                    <button
                        v-for="name in catalog.immunities"
                        :key="name"
                        type="button"
                        class="constructor-chip"
                        :class="{ 'constructor-chip-on': form.immunities.includes(name) }"
                        @click="toggleImmunity(name)"
                    >{{ statusLabel(name) }}</button>
                </div>
                <span class="admin-hint">{{ t('admin.constructor.immunityHint') }}</span>

                <div class="constructor-actions">
                    <button type="button" class="constructor-button" @click="emptyRankFour">{{ t('admin.constructor.empty') }}</button>
                    <button type="button" class="constructor-button" @click="reroll">{{ t('admin.constructor.reroll') }}</button>
                </div>
            </div>

            <div class="constructor-preview">
                <HeroCard v-if="preview" :hero="preview" selectable />
                <span v-if="error" class="admin-status-error">{{ error }}</span>
                <div class="constructor-add">
                    <input v-model.number="form.amount" class="admin-input admin-amount" type="number" min="1" max="20" :title="t('admin.amount')" />
                    <button type="button" class="admin-add" :disabled="!preview || !!error" @click="add">{{ t('admin.add') }}</button>
                </div>
                <span v-if="form.amount > 1" class="admin-hint">{{ t('admin.constructor.amountHint') }}</span>
            </div>
        </div>
    </section>
</template>

<style scoped>
.constructor {
    margin: 10px 0 18px;
    padding: 12px;
    border-radius: 12px;
    border: 1px solid rgba(255, 209, 102, 0.35);
    background: rgba(255, 209, 102, 0.05);
}

.constructor h3 {
    margin: 0 0 10px;
    font-size: 15px;
}

.constructor-body {
    display: flex;
    flex-wrap: wrap;
    gap: 20px;
    align-items: flex-start;
}

.constructor-form {
    flex: 1;
    min-width: 280px;
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 8px 12px;
}

.constructor-form > label {
    display: flex;
    flex-direction: column;
    gap: 3px;
    font-size: 13px;
}

.constructor-mode,
.constructor-immunities,
.constructor-actions,
.constructor-ability,
.constructor-form > .admin-hint {
    grid-column: 1 / -1;
}

.constructor-mode {
    display: flex;
    gap: 16px;
    font-size: 13px;
}

.constructor-form > .constructor-ability {
    flex-direction: row;
    align-items: center;
    gap: 8px;
}

.constructor-ability .admin-input {
    flex: 1;
}

.constructor-ability .constructor-level {
    flex: 0 0 90px;
}

.constructor-immunities {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 6px;
    font-size: 13px;
}

.constructor-chip,
.constructor-button {
    padding: 4px 10px;
    border-radius: 999px;
    border: 1px solid rgba(255, 255, 255, 0.25);
    background: rgba(0, 0, 0, 0.3);
    color: #e2e8f0;
    font: 12px 'Arial', sans-serif;
    cursor: pointer;
}

.constructor-chip-on {
    border-color: #4ade80;
    background: rgba(74, 222, 128, 0.2);
    color: #bbf7d0;
}

.constructor-button {
    border-radius: 8px;
    padding: 6px 12px;
}

.constructor-button:hover,
.constructor-chip:hover {
    background: rgba(255, 255, 255, 0.1);
}

.constructor-actions {
    display: flex;
    gap: 8px;
}

.constructor-preview {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
    min-width: 150px;
}

.constructor-add {
    display: flex;
    gap: 4px;
}

/* Поля и кнопки — как в админке (AdminView), стили scoped там не видны */
.admin-input {
    box-sizing: border-box;
    width: 100%;
    padding: 6px 8px;
    border-radius: 8px;
    border: 1px solid rgba(255, 255, 255, 0.18);
    background: rgba(0, 0, 0, 0.35);
    color: #f1f5f9;
    font: 13px 'Arial', sans-serif;
}

.admin-amount {
    width: 74px;
}

.admin-add {
    padding: 5px 10px;
    border: 0;
    border-radius: 8px;
    background: #2f855a;
    color: #fff;
    font: 700 13px 'Arial', sans-serif;
    cursor: pointer;
}

.admin-add:disabled {
    opacity: 0.5;
    cursor: default;
}

.admin-hint {
    font-size: 12px;
    color: #94a3b8;
}

.admin-status-error {
    color: #f87171;
    font-size: 13px;
    max-width: 220px;
    text-align: center;
}
</style>
