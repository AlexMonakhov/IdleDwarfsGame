<script lang="ts" setup>
import { computed, nextTick, onBeforeUnmount, ref, watch } from 'vue';
import { AxiosError } from 'axios';
import { useI18n } from 'vue-i18n';
import CharacterSkeleton from '@/components/character-skeleton.vue';
import GemButton from '@/components/common/GemButton.vue';
import { STATUS_ICONS } from '@/components/icons/status';
import StatIcon from '@/components/icons/stats/StatIcon.vue';
import SpellName from '@/components/icons/spells/SpellName.vue';
import AuraName from '@/components/icons/status/AuraName.vue';
import EquipmentSlots from '@/components/equipment/EquipmentSlots.vue';
import { effectText, passiveName, passiveText } from '@/components/equipment/equipmentText';
import HeroesApiService from '@/services/apiServices/HeroesApiService';
import { useResources } from '@/composables/UseResources';
import { HERO_NAME_MAX, HeroInfo, HeroRenameError, HeroUpgradeError, HeroUpgradeResult, SpellDetail } from '@/models/types/Heroes';
import { EquipmentInfo, EquipmentSlot, EquipmentType } from '@/models/types/Equipment';
import { HeroModel } from '@/models/hero';
import { elementColor, spriteColor } from '@/battle/sprites';
import { statusLabel } from '@/i18n';
import { vDragScroll } from '@/directives/dragScroll';
import { elementPreview } from '@/battle/arenaBackgrounds';
import { PASSIVE_ICONS } from '@/components/icons/passives';
import { PROFESSION_ICONS } from '@/components/icons/professions';
import { IconCoin, IconExperience } from '@/components/icons/ui';

// Выбранный герой в окне прокачки: сцена со спрайтом, характеристики и кнопка повышения уровня
const props = defineProps<{ hero: HeroInfo }>();
const emit = defineEmits<{
    (e: 'upgraded', result: HeroUpgradeResult): void;
    (e: 'renamed', hero: HeroInfo): void;
    // Данные устарели (героя прокачали в другой вкладке, ресурсы потратили) — окну стоит перечитать их
    (e: 'stale'): void;
    (e: 'equip', item: EquipmentInfo, slot: EquipmentSlot): void;
    (e: 'unequip', item: EquipmentInfo): void;
    (e: 'pick-type', type: EquipmentType): void;
}>();

const { t, te, locale } = useI18n();
const { resources, set: setResources } = useResources();

// Спрайт 409.6px: ноги дворфа по центру кадра, а голова и торс правее (меч на плече)
const FRAME = 409.6;
const SCALE = 1.6;
// Сдвиг кадра влево, в долях его ширины: по центру сцены встают голова и торс, а не только ноги
const BODY_OFFSET = 0.04;
const unitShift = `${-FRAME * SCALE * BODY_OFFSET}px`;
// Победа: 24 кадра на 16 fps — потом снова idle
const VICTORY_MS = 1500;

const color = computed(() => elementColor(props.hero.element));
// Фон — пещера стихии героя, как у арены этой стихии
const background = computed(() => elementPreview(props.hero.element));

const model = computed<HeroModel>(() => ({
    id: 0,
    name: props.hero.name,
    currentHp: props.hero.maxHp,
    maxHp: props.hero.maxHp,
    lvl: props.hero.level,
    color: spriteColor(props.hero),
    unitType: props.hero.unitType,
}));

const upgrade = computed(() => props.hero.upgrade);
const isMax = computed(() => upgrade.value.cost === null);

/* ===== Цена ===== */
const lacksGold = computed(() => !!upgrade.value.cost && (resources.value?.gold ?? 0) < upgrade.value.cost.gold);
const lacksExperience = computed(() => !!upgrade.value.cost && (resources.value?.experience ?? 0) < upgrade.value.cost.experience);
const affordable = computed(() => !isMax.value && !lacksGold.value && !lacksExperience.value);

const format = (value: number) => value.toLocaleString(locale.value);

/* ===== Характеристики ===== */
interface StatRow {
    key: string;
    value: number;
    next?: number;
}

const stats = computed<StatRow[]>(() => {
    const { hero } = props;
    const next = upgrade.value.next;
    return [
        { key: 'power', value: hero.power.total, next: next?.power },
        { key: 'hp', value: hero.maxHp, next: next?.maxHp },
        { key: 'attack', value: hero.attack, next: next?.attack },
        { key: 'speed', value: hero.speed },
        { key: 'energy', value: hero.maxEnergy },
        // Старый сервер энергию за удар не присылал
        ...(hero.energyPerAttack != null ? [{ key: 'energyPerAttack', value: hero.energyPerAttack }] : []),
    ];
});

/* ===== Заклинания: цена и сила каста ===== */
interface SpellRow {
    spell: string;
    detail: SpellDetail | null;
}

const spells = computed<SpellRow[]>(() => props.hero.spells.map(spell => ({
    spell,
    detail: props.hero.spellDetails?.find(d => d.spell === spell) ?? null,
})));

const spellEffect = (detail: SpellDetail) => {
    const parts: string[] = [];
    if (detail.damage > 0) {
        parts.push(detail.targets > 1
            ? t('heroUpgrade.spellDamageTargets', { damage: format(detail.damage), targets: detail.targets })
            : t('heroUpgrade.spellDamage', { damage: format(detail.damage) }));
    }
    if (detail.healing > 0) parts.push(t('heroUpgrade.spellHealing', { healing: format(detail.healing) }));
    return parts.join(' · ');
};

/* ===== Имя ===== */
const editingName = ref(false);
const nameDraft = ref('');
const renaming = ref(false);
const nameError = ref<string | null>(null);
const nameInput = ref<HTMLInputElement | null>(null);

const startRename = async () => {
    nameDraft.value = props.hero.name;
    nameError.value = null;
    editingName.value = true;
    await nextTick();
    nameInput.value?.focus();
    nameInput.value?.select();
};

const cancelRename = () => {
    editingName.value = false;
    nameError.value = null;
};

// Другой герой — правка имени прошлого не переносится
watch(() => props.hero.id, cancelRename);

// Пока правится имя, Esc отменяет правку, а не закрывает окно алтаря (ModalComponent смотрит на defaultPrevented).
// Слушаем на захвате — раньше окна и независимо от того, где сейчас фокус
const onRenameEscape = (event: KeyboardEvent) => {
    if (event.key !== 'Escape' || !editingName.value) return;
    event.preventDefault();
    cancelRename();
};
window.addEventListener('keydown', onRenameEscape, true);
onBeforeUnmount(() => window.removeEventListener('keydown', onRenameEscape, true));

const renameErrorText = (error: unknown) => {
    const code = (error as AxiosError<{ code?: HeroRenameError }>).response?.data?.code;
    return code && te(`heroUpgrade.renameErrors.${code}`)
        ? t(`heroUpgrade.renameErrors.${code}`, { max: HERO_NAME_MAX })
        : t('heroUpgrade.renameErrors.failed');
};

const saveName = async () => {
    if (renaming.value) return;
    const name = nameDraft.value.trim().replace(/\s+/g, ' ');
    if (name === props.hero.name) {
        cancelRename();
        return;
    }
    if (!name || name.length > HERO_NAME_MAX) {
        nameError.value = t('heroUpgrade.renameErrors.InvalidName', { max: HERO_NAME_MAX });
        return;
    }
    renaming.value = true;
    nameError.value = null;
    try {
        emit('renamed', await HeroesApiService.rename(props.hero.id, name));
        editingName.value = false;
    } catch (error) {
        nameError.value = renameErrorText(error);
    } finally {
        renaming.value = false;
    }
};

/* ===== Прокачка ===== */
type Skeleton = InstanceType<typeof CharacterSkeleton>;
const skeleton = ref<Skeleton | null>(null);

const busy = ref(false);
const message = ref<{ text: string; error: boolean } | null>(null);
// Номер вспышки «уровень повышен»: новый ключ перезапускает CSS-анимацию даже при частых кликах
const levelUpFlash = ref(0);

let victoryTimer = 0;

const celebrate = () => {
    levelUpFlash.value++;
    skeleton.value?.playAnimation('victory');
    window.clearTimeout(victoryTimer);
    victoryTimer = window.setTimeout(() => skeleton.value?.playAnimation('idle'), VICTORY_MS);
};

onBeforeUnmount(() => window.clearTimeout(victoryTimer));

const errorText = (error: unknown) => {
    const code = (error as AxiosError<{ code?: HeroUpgradeError }>).response?.data?.code;
    return code && te(`heroUpgrade.errors.${code}`) ? t(`heroUpgrade.errors.${code}`) : t('heroUpgrade.upgradeError');
};

const upgradeHero = async () => {
    if (!affordable.value || busy.value) return;
    busy.value = true;
    message.value = null;
    try {
        const result = await HeroesApiService.upgrade(props.hero.id);
        setResources(result.resources);
        emit('upgraded', result);
        message.value = { text: t('heroUpgrade.upgraded', { name: result.hero.name, level: result.hero.upgrade.level }), error: false };
        celebrate();
    } catch (error) {
        message.value = { text: errorText(error), error: true };
        emit('stale');
    } finally {
        busy.value = false;
    }
};

const buttonText = computed(() => {
    if (isMax.value) return t('heroUpgrade.maxLevel');
    if (busy.value) return t('heroUpgrade.upgrading');
    return t('heroUpgrade.upgrade');
});

const hint = computed(() => {
    if (message.value) return message.value.text;
    if (isMax.value) return t('heroUpgrade.maxHint');
    if (!affordable.value) return t('heroUpgrade.notEnough');
    return '';
});
</script>

<template>
    <div class="upgrade-panel">
        <!-- ===== Сцена ===== -->
        <div class="upgrade-stage" :style="{ backgroundImage: `url(${background})` }">
            <div class="upgrade-stage-title">
                <form v-if="editingName" class="upgrade-rename" @submit.prevent="saveName">
                    <input
                        ref="nameInput"
                        v-model="nameDraft"
                        class="upgrade-rename-input"
                        :maxlength="HERO_NAME_MAX"
                        :disabled="renaming"
                    />
                    <button type="submit" class="upgrade-name-button" :disabled="renaming" v-tooltip="t('heroUpgrade.renameSave')">✔</button>
                    <button type="button" class="upgrade-name-button" :disabled="renaming" v-tooltip="t('heroUpgrade.renameCancel')" @click="cancelRename">✕</button>
                </form>
                <div v-else class="upgrade-stage-name-row">
                    <div class="upgrade-stage-name">{{ hero.name }}</div>
                    <button type="button" class="upgrade-name-button" v-tooltip="t('heroUpgrade.rename')" @click="startRename">
                        <svg viewBox="0 0 24 24" width="12" height="12" aria-hidden="true">
                            <path fill="currentColor" d="M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zm17.71-10.21a1 1 0 0 0 0-1.41l-2.34-2.34a1 1 0 0 0-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z" />
                        </svg>
                    </button>
                </div>
                <div v-if="nameError" class="upgrade-rename-error">{{ nameError }}</div>
                <div class="upgrade-stage-tags">
                    <span class="upgrade-tag">{{ t('heroUpgrade.rank', { rank: hero.level }) }}</span>
                    <span v-if="hero.profession" class="upgrade-tag"><component :is="PROFESSION_ICONS[hero.profession]" v-if="PROFESSION_ICONS[hero.profession]" /> {{ t(`professions.${hero.profession}`) }}</span>
                    <span class="upgrade-tag" :class="`upgrade-tag-${color}`">{{ t(`elements.${hero.element}`) }}</span>
                    <span v-if="hero.onExpedition" class="upgrade-tag upgrade-tag-busy">{{ t('squad.onExpedition') }}</span>
                </div>
            </div>

            <div class="upgrade-stage-unit" :style="{ '--unit-shift': unitShift }">
                <div v-if="levelUpFlash" :key="levelUpFlash" class="upgrade-glow"></div>
                <!-- Ключ по рангу: другой ранг — другие спрайты, их скелет грузит при монтировании -->
                <CharacterSkeleton
                    :key="`${hero.id}-${hero.level}-${hero.element}`"
                    ref="skeleton"
                    :model="model"
                    :frame-width="FRAME"
                    :frame-height="FRAME"
                    :frame-count="24"
                    :columns="5"
                    :fps="16"
                    :scale="SCALE"
                />
                <div v-if="levelUpFlash" :key="`text-${levelUpFlash}`" class="upgrade-float">{{ t('heroUpgrade.levelUp') }}</div>
            </div>

            <EquipmentSlots
                :hero="hero"
                @equip="(item, slot) => emit('equip', item, slot)"
                @unequip="item => emit('unequip', item)"
                @pick-type="type => emit('pick-type', type)"
            />
        </div>

        <!-- ===== Характеристики ===== -->
        <!-- Без затухания краёв: маска срезала бы тень самой панели -->
        <aside v-drag-scroll="{ fade: false }" class="upgrade-stats">
            <h4 class="upgrade-subtitle">{{ t('heroUpgrade.stats') }}</h4>
            <div v-for="row in stats" :key="row.key" class="stat-row" :class="`stat-row-${row.key}`">
                <StatIcon :stat="row.key" class="stat-row-icon" size="18px" />
                <span class="stat-label">{{ t(`heroUpgrade.statNames.${row.key}`) }}</span>
                <span class="stat-value">{{ format(row.value) }}</span>
                <span
                    v-if="row.next !== undefined"
                    class="stat-delta"
                    :class="{ 'stat-delta-zero': row.next === row.value }"
                    v-tooltip="t('heroUpgrade.nextLevel')"
                >+{{ format(row.next - row.value) }}</span>
            </div>

            <div class="stat-group">
                <span class="stat-label">{{ t('heroUpgrade.statNames.spells') }}</span>
                <div v-for="row in spells" :key="row.spell" class="stat-spell">
                    <span class="stat-text stat-spell-head">
                        <SpellName :spell="row.spell" />
                        <span v-if="row.detail" class="stat-spell-cost" v-tooltip="t('heroUpgrade.statNames.energy')">
                            <StatIcon stat="energy" size="13px" />{{ t('heroUpgrade.spellCost', { cost: row.detail.energyCost }) }}
                        </span>
                    </span>
                    <span v-if="row.detail && spellEffect(row.detail)" class="stat-passive-text">{{ spellEffect(row.detail) }}</span>
                    <!-- Срабатывания с предметов: один шаблон на все, новый артефакт с SpellProcEffect показывается сам -->
                    <i18n-t
                        v-for="(proc, index) in row.detail?.procs ?? []"
                        :key="`${proc.spell}-${index}`"
                        :keypath="proc.chance < 100 ? 'heroUpgrade.spellProcChance' : 'heroUpgrade.spellProc'"
                        tag="span"
                        class="stat-passive-text stat-spell-proc"
                    >
                        <template #spell><SpellName :spell="proc.spell" /></template>
                        <template #damage>{{ format(proc.damage) }}</template>
                        <template #chance>{{ proc.chance }}</template>
                    </i18n-t>
                    <!-- Остальные эффекты предметов (повторы, взрывы, метки…) — тем же текстом, что в описании предмета -->
                    <span
                        v-for="(effect, index) in row.detail?.effects ?? []"
                        :key="`${effect.kind}-${index}`"
                        class="stat-passive-text stat-spell-proc"
                    >{{ t('heroUpgrade.spellItemEffect', { effect: effectText(effect) }) }}</span>
                </div>
                <span v-if="!hero.spells.length" class="stat-text">{{ t('heroUpgrade.none') }}</span>
            </div>
            <div class="stat-group">
                <span class="stat-label">{{ t('heroUpgrade.statNames.immunities') }}</span>
                <span v-if="hero.immunities.length" class="stat-text stat-icons">
                    <span v-for="name in hero.immunities" :key="name" v-tooltip="statusLabel(name)">
                        <component v-if="STATUS_ICONS[name]" :is="STATUS_ICONS[name]" size="16px" />
                        <template v-else>{{ statusLabel(name) }}</template>
                    </span>
                </span>
                <span v-else class="stat-text">{{ t('heroUpgrade.none') }}</span>
            </div>
            <div class="stat-group">
                <span class="stat-label">{{ t('heroUpgrade.statNames.aura') }}</span>
                <span v-if="hero.aura" class="stat-text"><AuraName :aura="hero.aura" size="16px" /></span>
                <span v-else class="stat-text">{{ t('heroUpgrade.none') }}</span>
            </div>
            <div class="stat-group">
                <span class="stat-label">{{ t('heroUpgrade.statNames.passives') }}</span>
                <div v-for="passive in hero.passives ?? []" :key="passive.passive" class="stat-passive">
                    <span class="stat-text stat-passive-name"><component :is="PASSIVE_ICONS[passive.passive]" v-if="PASSIVE_ICONS[passive.passive]" /> {{ passiveName(passive) }} · {{ t('passives.level', { level: passive.level }) }}</span>
                    <span class="stat-passive-text">{{ passiveText(passive) }}</span>
                </div>
                <span v-if="!hero.passives?.length" class="stat-text">{{ t('heroUpgrade.none') }}</span>
            </div>
        </aside>

        <!-- ===== Прокачка ===== -->
        <div class="upgrade-controls">
            <div class="upgrade-level">
                <span class="upgrade-level-text">{{ t('heroUpgrade.level', { level: upgrade.level, max: upgrade.maxLevel }) }}</span>
                <!-- До 20 уровней — по ячейке на уровень, дальше ячейки слишком мелкие: сплошная полоска -->
                <div v-if="upgrade.maxLevel > 20" class="upgrade-level-bar upgrade-level-bar-solid">
                    <span class="upgrade-level-fill" :style="{ width: `${(upgrade.level / upgrade.maxLevel) * 100}%` }"></span>
                </div>
                <div v-else class="upgrade-level-bar">
                    <span
                        v-for="step in upgrade.maxLevel"
                        :key="step"
                        class="upgrade-level-step"
                        :class="{ 'upgrade-level-step-done': step <= upgrade.level }"
                    ></span>
                </div>
            </div>

            <div class="upgrade-action">
                <div v-if="upgrade.cost" class="upgrade-cost">
                    <span class="upgrade-cost-label">{{ t('heroUpgrade.cost') }}</span>
                    <span class="upgrade-cost-gold" :class="{ 'upgrade-cost-lacking': lacksGold }" v-tooltip="t('resources.gold')">
                        <IconCoin /> {{ format(upgrade.cost.gold) }}
                    </span>
                    <span class="upgrade-cost-experience" :class="{ 'upgrade-cost-lacking': lacksExperience }" v-tooltip="t('resources.experience')">
                        <IconExperience /> {{ format(upgrade.cost.experience) }}
                    </span>
                </div>
                <span class="upgrade-hint" :class="{ 'upgrade-hint-error': message?.error }">{{ hint }}</span>
                <GemButton :color="'green'" :disabled="!affordable || busy" @click="upgradeHero">{{ buttonText }}</GemButton>
            </div>
        </div>
    </div>
</template>

<style scoped>
.upgrade-panel {
    /* Ширина панели — по её колонкам, остальное место окна отдаём списку героев */
    flex: 0 1 auto;
    min-width: 0;
    display: grid;
    /* Сцена ужимается первой, если окно узкое */
    grid-template-columns: minmax(0, 350px) 250px;
    grid-template-rows: minmax(0, 1fr) auto;
    gap: 12px;
    font-family: 'Arial', sans-serif;
}

/* ===== Сцена ===== */
.upgrade-stage {
    position: relative;
    min-height: 300px;
    overflow: hidden;
    border-radius: 14px;
    border: 2px solid #333;
    background-color: #1b1210;
    background-size: cover;
    background-position: center 65%;
    box-shadow: 0 0 0 3px rgb(157, 173, 175), inset 0 0 40px rgba(0, 0, 0, 0.85);
}

/* Затемнение снизу, чтобы герой «стоял» на полу */
.upgrade-stage::after {
    content: '';
    position: absolute;
    inset: 0;
    background: linear-gradient(to bottom, rgba(0, 0, 0, 0.35) 0%, rgba(0, 0, 0, 0) 35%, rgba(0, 0, 0, 0.5) 100%);
    pointer-events: none;
}

.upgrade-stage-title {
    position: absolute;
    top: 12px;
    left: 16px;
    z-index: 2;
    display: flex;
    flex-direction: column;
    row-gap: 6px;
}

.upgrade-stage-name {
    color: #f8fafc;
    font-size: 22px;
    font-weight: bold;
    letter-spacing: 1px;
    text-transform: uppercase;
    text-shadow: 0 2px 4px #000;
}

.upgrade-stage-name-row {
    display: flex;
    align-items: center;
    column-gap: 6px;
}

/* Маленькая кнопка у имени: карандаш, а в режиме правки — сохранить и отменить */
.upgrade-name-button {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 20px;
    height: 20px;
    padding: 0;
    border: 1px solid rgba(255, 255, 255, 0.25);
    border-radius: 6px;
    background: rgba(0, 0, 0, 0.55);
    color: #e2e8f0;
    font-size: 11px;
    line-height: 1;
    cursor: pointer;
    transition: background 0.15s ease, color 0.15s ease;
}

.upgrade-name-button:hover:not(:disabled) {
    background: rgba(0, 0, 0, 0.8);
    color: #ffd166;
}

.upgrade-name-button:disabled {
    opacity: 0.5;
    cursor: default;
}

.upgrade-rename {
    display: flex;
    align-items: center;
    column-gap: 4px;
}

.upgrade-rename-input {
    width: 180px;
    padding: 2px 6px;
    border: 1px solid rgba(255, 209, 102, 0.6);
    border-radius: 6px;
    background: rgba(0, 0, 0, 0.7);
    color: #f8fafc;
    font-family: inherit;
    font-size: 16px;
    font-weight: bold;
    outline: none;
}

.upgrade-rename-error {
    padding: 2px 6px;
    border-radius: 6px;
    background: rgba(0, 0, 0, 0.65);
    color: #ffb4b4;
    font-size: 12px;
}

.upgrade-stage-tags {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
}

.upgrade-tag {
    padding: 2px 8px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.65);
    color: #e2e8f0;
    font-size: 12px;
    font-weight: bold;
}

.upgrade-tag-red { color: var(--red-gem-color); }
.upgrade-tag-blue { color: var(--blue-gem-color); }
.upgrade-tag-yellow { color: var(--yellow-gem-color); }
.upgrade-tag-green { color: var(--green-gem-color); }
.upgrade-tag-purple { color: #c084fc; }
.upgrade-tag-violet { color: var(--pink-gem-color); }
.upgrade-tag-white { color: var(--white-gem-color); }
.upgrade-tag-busy { color: #fde68a; }

.upgrade-stage-unit {
    position: absolute;
    left: 50%;
    /* Под ногами в кадре пустое поле — опускаем его за край, чтобы герой стоял на полу */
    bottom: -80px;
    z-index: 1;
    transform: translateX(calc(-50% + var(--unit-shift, 0px)));
}

.upgrade-stage-unit canvas {
    position: relative;
    z-index: 1;
    display: block;
}

/* Вспышка под героем при повышении уровня */
.upgrade-glow {
    position: absolute;
    left: 50%;
    top: 55%;
    width: 520px;
    height: 520px;
    border-radius: 50%;
    background: radial-gradient(circle, rgba(255, 230, 140, 0.85) 0%, rgba(255, 200, 80, 0.35) 40%, transparent 70%);
    transform: translate(-50%, -50%) scale(0.3);
    opacity: 0;
    pointer-events: none;
    animation: upgrade-glow 1.2s ease-out;
}

@keyframes upgrade-glow {
    0% { opacity: 0; transform: translate(-50%, -50%) scale(0.3); }
    25% { opacity: 1; }
    100% { opacity: 0; transform: translate(-50%, -50%) scale(1.5); }
}

.upgrade-float {
    position: absolute;
    left: 50%;
    top: 22%;
    z-index: 2;
    color: #ffd166;
    font-size: 26px;
    font-weight: 900;
    letter-spacing: 2px;
    text-transform: uppercase;
    white-space: nowrap;
    text-shadow: 0 0 8px rgba(255, 180, 60, 0.9), 0 2px 3px #000;
    opacity: 0;
    pointer-events: none;
    animation: upgrade-float 1.4s ease-out;
}

@keyframes upgrade-float {
    0% { opacity: 0; transform: translate(-50%, 20px) scale(0.7); }
    20% { opacity: 1; transform: translate(-50%, 0) scale(1.1); }
    70% { opacity: 1; transform: translate(-50%, -24px) scale(1); }
    100% { opacity: 0; transform: translate(-50%, -44px) scale(1); }
}

/* ===== Характеристики ===== */
.upgrade-stats {
    display: flex;
    flex-direction: column;
    row-gap: 6px;
    padding: 12px 14px;
    overflow-y: auto;
    border-radius: 12px;
    background: rgba(15, 23, 32, 0.8);
    border: 2px solid rgba(157, 173, 175, 0.7);
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5);
}

.upgrade-subtitle {
    margin: 0 0 4px;
    color: #f8fafc;
    font-size: 14px;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.stat-row {
    display: flex;
    align-items: baseline;
    column-gap: 8px;
    padding: 4px 0;
    border-bottom: 1px solid rgba(157, 173, 175, 0.2);
}

/* Строка выровнена по базовой линии текста: значок сажаем на неё сами */
.stat-row-icon {
    align-self: center;
}

.stat-label {
    flex: 1;
    color: #cbd5e1;
    font-size: 13px;
}

.stat-value {
    color: #f8fafc;
    font-size: 15px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
}

.stat-row-power .stat-value {
    color: #ffd166;
}

.stat-delta {
    min-width: 36px;
    color: #86efac;
    font-size: 12px;
    font-weight: bold;
    text-align: right;
}

.stat-delta-zero {
    color: rgba(203, 213, 225, 0.5);
}

.stat-group {
    display: flex;
    flex-direction: column;
    row-gap: 2px;
    padding-top: 6px;
}

.stat-text {
    color: #f8fafc;
    font-size: 13px;
}

.stat-spell {
    display: flex;
    flex-direction: column;
}

.stat-spell-head {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    column-gap: 8px;
}

.stat-spell-cost {
    display: inline-flex;
    align-items: center;
    column-gap: 2px;
    color: #7dd3fc;
    font-size: 12px;
    font-weight: bold;
}

.stat-icons {
    display: flex;
    gap: 6px;
}

.stat-passive {
    display: flex;
    flex-direction: column;
}

.stat-passive-name {
    font-weight: bold;
}

.stat-spell-proc :deep(.spell-name) {
    vertical-align: middle;
}

.stat-passive-text {
    color: #cbd5e1;
    font-size: 12px;
}

/* ===== Прокачка ===== */
.upgrade-controls {
    grid-column: 1 / -1;
    display: flex;
    flex-direction: column;
    row-gap: 10px;
    padding: 12px 16px;
    border-radius: 12px;
    background: rgba(15, 23, 32, 0.8);
    border: 2px solid rgba(157, 173, 175, 0.7);
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5);
}

.upgrade-level {
    display: flex;
    align-items: center;
    column-gap: 14px;
}

.upgrade-level-text {
    color: #f8fafc;
    font-size: 15px;
    font-weight: bold;
    white-space: nowrap;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.upgrade-level-bar {
    flex: 1;
    display: flex;
    column-gap: 3px;
}

.upgrade-level-step {
    flex: 1;
    height: 12px;
    border-radius: 3px;
    background: rgba(0, 0, 0, 0.55);
    box-shadow: inset 0 0 3px #000;
    transition: background 0.3s ease, box-shadow 0.3s ease;
}

.upgrade-level-bar-solid {
    height: 12px;
    border-radius: 3px;
    overflow: hidden;
    background: rgba(0, 0, 0, 0.55);
    box-shadow: inset 0 0 3px #000;
}

.upgrade-level-fill {
    display: block;
    height: 100%;
    background: linear-gradient(to bottom, #fde68a, #f59e0b);
    box-shadow: 0 0 6px rgba(245, 158, 11, 0.6);
    transition: width 0.3s ease;
}

.upgrade-level-step-done {
    background: linear-gradient(to bottom, #fde68a, #f59e0b);
    box-shadow: 0 0 6px rgba(245, 158, 11, 0.6);
}

.upgrade-action {
    display: flex;
    align-items: center;
    column-gap: 14px;
}

.upgrade-cost {
    display: flex;
    align-items: baseline;
    column-gap: 12px;
    font-size: 15px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.upgrade-cost-label {
    color: #cbd5e1;
    font-size: 13px;
    font-weight: normal;
}

.upgrade-cost-gold {
    color: #ffd166;
}

.upgrade-cost-experience {
    color: #a5f3fc;
}

.upgrade-cost-lacking {
    color: #ff8a8a;
}

.upgrade-hint {
    flex: 1;
    text-align: right;
    color: #cbd5e1;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.upgrade-hint-error {
    color: #ffb4b4;
}
</style>
