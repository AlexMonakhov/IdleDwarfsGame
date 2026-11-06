import type { Component } from 'vue';

// Значение фильтра: стихия, ранг, вид, редкость, камень, уровень
export type FilterValue = string | number;

// Кнопка ряда фильтров (FilterRow.vue): значение, иконка (компонент с пропсами или картинка), подпись и число.
// count = 0 при другом выбранном фильтре — кнопка тусклая, но остаётся на месте
export interface FilterOption<V> {
    value: V;
    label?: string;
    icon?: Component;
    iconProps?: Record<string, unknown>;
    image?: string;
    // Цвет подсветки выбранной кнопки (стихия, редкость, камень)
    color?: string;
    count: number;
    tooltip?: string;
}
