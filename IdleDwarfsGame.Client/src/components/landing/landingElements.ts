import type { BonusElement, SquadBonus } from '@/battle/squadBonuses';

// Цвета стихий лендинга: камень гем-кнопки (как --*-gem-color в App.vue; у Тьмы и Света своих гем-кнопок нет —
// фиолетовый и бело-голубой) и оттенок, которым подсвечивается фон страницы, пока бонус этой стихии активен
export const ELEMENT_GEM: Record<BonusElement, string> = {
    Fire: '#d40808',
    Frost: '#2196f3',
    Lightning: '#ffb300',
    Nature: '#00c853',
    Dark: '#9b30ff',
    Radiance: '#9fc4e0',
    Blood: '#e91e63',
};

export const ELEMENT_TINT: Record<BonusElement, string> = {
    Fire: '#ff5a1f',
    Frost: '#38bdf8',
    Lightning: '#ffd84a',
    Nature: '#4ade80',
    Dark: '#a855f7',
    Radiance: '#e0f2fe',
    Blood: '#f43f5e',
};

// Оттенки фона бонуса: у моно — один, у пары — два, у призмы — радуга
export const bonusTints = (bonus: SquadBonus): string[] =>
    bonus.elements.length ? bonus.elements.map(element => ELEMENT_TINT[element]) : ['#f472b6', '#38bdf8', '#facc15'];
