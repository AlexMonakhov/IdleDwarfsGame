import type { Component } from 'vue';

// Иконка над подписью: компонент-иконка (components/icons) или короткий символ
export type TabIcon = string | Component;

// Вкладка-кнопка справа от рамки ModalComponent
export interface ModalTab {
    key: string;
    label: string;
    icon?: TabIcon;
    badgeCount?: number;
}
