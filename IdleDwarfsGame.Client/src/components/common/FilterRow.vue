<script lang="ts" setup>
import type { FilterOption, FilterValue } from './filterOptions';

// Ряд кнопок-фильтров: «Все» и по кнопке на значение — с иконкой (компонент или картинка), подписью и числом.
// Повторный клик по выбранной снимает фильтр. Пустые (count = 0 при другом фильтре) — тусклые, но на месте.
// Используется в фильтрах героев, предметов, камней и сундуков

const props = defineProps<{
    options: FilterOption<FilterValue>[];
    modelValue: FilterValue | null;
    allLabel: string;
}>();
const emit = defineEmits<{ (e: 'update:modelValue', value: FilterValue | null): void }>();

const pick = (value: FilterValue) => emit('update:modelValue', props.modelValue === value ? null : value);
</script>

<template>
    <div class="filter-row">
        <button
            type="button"
            class="filter-chip"
            :class="{ 'filter-chip-active': modelValue === null }"
            @click="emit('update:modelValue', null)"
        >{{ allLabel }}</button>
        <button
            v-for="option in options"
            :key="String(option.value)"
            type="button"
            class="filter-chip"
            :class="{ 'filter-chip-active': modelValue === option.value, 'filter-chip-empty': !option.count }"
            :style="option.color ? { '--filter-color': option.color } : undefined"
            v-tooltip="option.tooltip"
            @click="pick(option.value)"
        >
            <component :is="option.icon" v-if="option.icon" v-bind="option.iconProps" />
            <img v-else-if="option.image" :src="option.image" alt="" draggable="false" />
            <span v-if="option.label" class="filter-chip-label">{{ option.label }}</span>
            <span class="filter-chip-count">{{ option.count }}</span>
        </button>
    </div>
</template>

<style scoped>
.filter-row {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: 6px;
    font-family: 'Arial', sans-serif;
}

/* Как кнопки фильтров на алтаре, с цветом значения на выбранной */
.filter-chip {
    --filter-color: #ffd166;
    display: inline-flex;
    align-items: center;
    column-gap: 5px;
    height: 28px;
    padding: 0 10px;
    border: 1px solid rgba(157, 173, 175, 0.5);
    border-radius: 8px;
    background: rgba(14, 19, 34, 0.7);
    color: #cbd5e1;
    font-size: 12px;
    font-weight: bold;
    cursor: pointer;
    transition: border-color 0.15s ease, box-shadow 0.15s ease, color 0.15s ease;
}

.filter-chip:hover {
    color: #f8fafc;
    border-color: color-mix(in srgb, var(--filter-color) 70%, transparent);
}

.filter-chip-active {
    color: #f8fafc;
    border-color: var(--filter-color);
    box-shadow: 0 0 8px 1px color-mix(in srgb, var(--filter-color) 50%, transparent);
}

.filter-chip-empty {
    opacity: 0.45;
}

.filter-chip img {
    width: 18px;
    height: 18px;
}

.filter-chip-count {
    color: rgba(203, 213, 225, 0.75);
    font-size: 11px;
    font-variant-numeric: tabular-nums;
}
</style>
