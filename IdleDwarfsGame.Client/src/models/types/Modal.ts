// Вкладка-кнопка справа от рамки ModalComponent
export interface ModalTab {
    key: string;
    label: string;
    // Короткая иконка над подписью (эмодзи или символ)
    icon?: string;
    badgeCount?: number;
}
