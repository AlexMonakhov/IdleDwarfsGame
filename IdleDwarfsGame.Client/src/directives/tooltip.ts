import { type ObjectDirective, h, render } from 'vue';
import TooltipPanel from '@/components/common/TooltipPanel.vue';

// Подсказка в одну строку вместо атрибута title: v-tooltip="t('resources.gold')". Обёртка не нужна, вёрстка не меняется.
// Пустое значение — подсказки нет. Текст лежит в data-tooltip, чтобы при уходе с вложенного элемента найти внешний
const host = document.createElement('div');
let active: HTMLElement | null = null;

const hide = () => {
    if (!active) return;
    active = null;
    render(null, host);
    window.removeEventListener('dragstart', hide, true);
};

const show = (el: HTMLElement) => {
    active = el;
    const anchor = el.getBoundingClientRect();
    // key: новая подсказка заново считает своё положение
    render(h(TooltipPanel, { anchor, key: Math.random() }, { description: () => el.dataset.tooltip }), host);
    // Пока что-то тащат, mouseleave не приходит
    window.addEventListener('dragstart', hide, true);
};

const onEnter = (event: Event) => {
    const el = event.currentTarget as HTMLElement;
    if (el.dataset.tooltip) show(el);
};

const onLeave = (event: Event) => {
    const next = (event as MouseEvent).relatedTarget as Element | null;
    const outer = next?.closest?.<HTMLElement>('[data-tooltip]');
    if (outer && outer !== event.currentTarget) show(outer);
    else hide();
};

const setText = (el: HTMLElement, text?: string | null) => {
    if (text) el.dataset.tooltip = text;
    else delete el.dataset.tooltip;
    // У кнопки-значка title был ещё и её именем для экранного диктора
    if (el.tagName === 'BUTTON') {
        if (text) el.setAttribute('aria-label', text);
        else el.removeAttribute('aria-label');
    }
    if (active !== el) return;
    if (text) show(el);
    else hide();
};

export const vTooltip: ObjectDirective<HTMLElement, string | null | undefined> = {
    mounted(el, { value }) {
        setText(el, value);
        el.addEventListener('mouseenter', onEnter);
        el.addEventListener('mouseleave', onLeave);
        // Нажали — подсказка больше не нужна, как и у обычного title
        el.addEventListener('mousedown', hide);
    },
    updated(el, { value, oldValue }) {
        if (value !== oldValue) setText(el, value);
    },
    beforeUnmount(el) {
        if (active === el) hide();
        el.removeEventListener('mouseenter', onEnter);
        el.removeEventListener('mouseleave', onLeave);
        el.removeEventListener('mousedown', hide);
    },
};

declare module 'vue' {
    interface ComponentCustomProperties {
        vTooltip: typeof vTooltip;
    }
}
