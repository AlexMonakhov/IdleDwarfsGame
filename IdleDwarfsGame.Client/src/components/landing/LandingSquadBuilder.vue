<script lang="ts" setup>
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import SquadBonusFigure from '@/components/squadBonus/SquadBonusFigure.vue';
import SquadBonusIcon from '@/components/squadBonus/SquadBonusIcon.vue';
import SquadBonusHints from '@/components/squadBonus/SquadBonusHints.vue';
import { BONUS_ELEMENTS, BonusElement, SQUAD_SIZE, SquadBonus, monoBonus, resolveSquadBonus } from '@/battle/squadBonuses';
import { bonusColor, bonusDescription, bonusName, elementLabel } from '@/components/squadBonus/squadBonusText';
import { ELEMENT_ICONS } from '@/components/inventory/lootStyles';
import { ELEMENT_TINT } from './landingElements';

// Мини-конструктор отряда на лендинге: пять мест, клик по стихии ставит дворфа на свободное место
// (отряд полон — первый уходит), клик по месту освобождает его. Бонус считается так же, как в игре
// (resolveSquadBonus), подсказки «+1 стихия → бонус» — игровые (SquadBonusHints)
const emit = defineEmits<{ (e: 'bonus', bonus: SquadBonus | null): void }>();

const { t } = useI18n();

// Для начала — Плазма: трое Огня и двое Молнии
const slots = ref<(BonusElement | null)[]>(['Fire', 'Fire', 'Fire', 'Lightning', 'Lightning']);

const bonus = computed(() => resolveSquadBonus(slots.value));
const pulse = ref(0);
// Ключ с уровнем: у моно-бонуса id один на все уровни, а рост II → III — тоже срабатывание
watch(() => (bonus.value ? `${bonus.value.id}:${bonus.value.tier}` : null), () => {
    pulse.value++;
    emit('bonus', bonus.value);
}, { immediate: true });

const add = (element: BonusElement) => {
    const next = [...slots.value];
    const free = next.indexOf(null);
    if (free >= 0) next[free] = element;
    else {
        next.shift();
        next.push(element);
    }
    slots.value = next;
};

const removeAt = (index: number) => {
    const next = [...slots.value];
    next[index] = null;
    slots.value = next;
};

const clear = () => { slots.value = Array(SQUAD_SIZE).fill(null); };

const randomize = () => {
    slots.value = Array.from({ length: SQUAD_SIZE }, () => BONUS_ELEMENTS[Math.floor(Math.random() * BONUS_ELEMENTS.length)]);
};
</script>

<template>
    <div class="builder">
        <div class="builder-slots">
            <button
                v-for="(element, i) in slots"
                :key="i"
                type="button"
                class="builder-slot"
                :class="{ filled: element }"
                :style="element ? { '--slot-color': ELEMENT_TINT[element] } : undefined"
                :title="element ? elementLabel(element) : undefined"
                @click="element && removeAt(i)"
            >
                <SquadBonusIcon v-if="element" :key="element + i" class="builder-slot-icon" :bonus="monoBonus(element, 1)" :show-tier="false" size="46px" />
                <span v-else class="builder-slot-empty">+</span>
            </button>
        </div>

        <div class="builder-elements">
            <button
                v-for="element in BONUS_ELEMENTS"
                :key="element"
                type="button"
                class="builder-element"
                :style="{ '--slot-color': ELEMENT_TINT[element] }"
                @click="add(element)"
            >
                <img v-if="ELEMENT_ICONS[element]" :src="ELEMENT_ICONS[element]" alt="" class="builder-element-gem">
                <span>{{ elementLabel(element) }}</span>
            </button>
        </div>

        <div class="builder-result" :style="{ '--result-color': bonus ? bonusColor(bonus) : '#94a3b8' }">
            <template v-if="bonus">
                <SquadBonusFigure :bonus="bonus" size="64px" animated :pulse="pulse" />
                <div class="builder-result-text">
                    <div class="builder-result-name">{{ bonusName(bonus) }}</div>
                    <div class="builder-result-shape">{{ t(`squadBonuses.shapes.${bonus.shape}`) }}</div>
                    <p class="builder-result-description">{{ bonusDescription(bonus) }}</p>
                </div>
            </template>
            <div v-else class="builder-result-text">
                <div class="builder-result-name">{{ t('squadBonuses.none') }}</div>
                <p class="builder-result-description">{{ t('landing.builderNone') }}</p>
            </div>
        </div>

        <SquadBonusHints class="builder-hints" :elements="slots" />

        <div class="builder-actions">
            <button type="button" class="builder-action" @click="randomize">{{ t('landing.builderRandom') }}</button>
            <button type="button" class="builder-action" @click="clear">{{ t('landing.builderClear') }}</button>
        </div>
    </div>
</template>

<style scoped>
.builder {
    display: flex;
    flex-direction: column;
    gap: 20px;
    margin-top: 22px;
}

.builder-slots {
    display: flex;
    justify-content: center;
    gap: 12px;
}

.builder-slot {
    --slot-color: rgba(255, 255, 255, 0.25);
    display: flex;
    align-items: center;
    justify-content: center;
    width: 68px;
    height: 68px;
    padding: 0;
    border-radius: 50%;
    border: 2px dashed rgba(255, 255, 255, 0.25);
    background: rgba(0, 0, 0, 0.3);
    cursor: default;
    transition: border-color 0.3s, box-shadow 0.3s, background 0.3s;
}

.builder-slot.filled {
    border: 2px solid var(--slot-color);
    background: radial-gradient(circle, color-mix(in srgb, var(--slot-color) 30%, transparent), rgba(0, 0, 0, 0.35) 70%);
    box-shadow: 0 0 16px color-mix(in srgb, var(--slot-color) 45%, transparent);
    cursor: pointer;
}

.builder-slot-icon {
    animation: builder-pop 0.35s cubic-bezier(0.3, 1.6, 0.5, 1);
}

.builder-slot-empty {
    color: rgba(255, 255, 255, 0.4);
    font-size: 28px;
    font-weight: 300;
}

.builder-elements {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: 8px;
}

.builder-element {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    padding: 7px 12px 7px 8px;
    border-radius: 999px;
    border: 1px solid color-mix(in srgb, var(--slot-color) 55%, transparent);
    background: color-mix(in srgb, var(--slot-color) 12%, rgba(0, 0, 0, 0.35));
    color: #f1f5f9;
    font: inherit;
    font-size: 14px;
    font-weight: 700;
    cursor: pointer;
    transition: transform 0.12s, background 0.2s;
}

.builder-element:hover {
    background: color-mix(in srgb, var(--slot-color) 28%, rgba(0, 0, 0, 0.35));
    transform: translateY(-2px);
}

.builder-element:active {
    transform: translateY(1px);
}

.builder-element-gem {
    width: 22px;
    height: 22px;
    object-fit: contain;
}

.builder-result {
    display: flex;
    align-items: center;
    gap: 26px;
    min-height: 120px;
    padding: 18px 22px 18px 34px;
    border-radius: 14px;
    border: 1px solid color-mix(in srgb, var(--result-color) 40%, transparent);
    background: color-mix(in srgb, var(--result-color) 8%, rgba(0, 0, 0, 0.35));
    transition: border-color 0.4s, background 0.4s;
}

.builder-result-name {
    font-size: 22px;
    font-weight: 800;
    color: var(--result-color);
    text-shadow: 0 2px 0 rgba(0, 0, 0, 0.5);
}

.builder-result-shape {
    margin-top: 2px;
    font-size: 13px;
    color: #94a3b8;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.builder-result-description {
    margin: 8px 0 0;
    font-size: 14px;
    line-height: 1.5;
    color: #d6e8ea;
}

.builder-hints {
    justify-content: center;
}

.builder-actions {
    display: flex;
    justify-content: center;
    gap: 12px;
}

.builder-action {
    padding: 8px 16px;
    border-radius: 8px;
    border: 1px solid rgba(255, 255, 255, 0.2);
    background: rgba(255, 255, 255, 0.06);
    color: #cfe6e9;
    font: inherit;
    font-size: 14px;
    font-weight: 600;
    cursor: pointer;
}

.builder-action:hover {
    background: rgba(255, 255, 255, 0.12);
    color: #fff;
}

@keyframes builder-pop {
    from { transform: scale(0.3); opacity: 0; }
}

@media (max-width: 760px) {
    .builder-slots {
        gap: 6px;
    }

    .builder-slot {
        width: 54px;
        height: 54px;
    }

    .builder-slot-icon {
        width: 38px;
        height: 38px;
    }

    .builder-result {
        flex-direction: column;
        align-items: flex-start;
        gap: 18px;
        padding: 26px 18px 18px;
    }
}
</style>
