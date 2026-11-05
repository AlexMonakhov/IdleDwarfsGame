import type { ObjectDirective } from 'vue';

// Ставит CSS-анимации элемента и всего внутри на паузу, пока он не виден (прокручен за край окна или списка).
// Бесконечные анимации карточек (кольцо и свечение престижа, частицы ауры) иначе перерисовываются каждый кадр
// и за пределами экрана. Стили — класс pause-offscreen-hidden в App.vue.
// Один наблюдатель на всё приложение; IntersectionObserver сам учитывает обрезку прокручиваемыми предками
const HIDDEN = 'pause-offscreen-hidden';
// Запас вокруг видимой области: анимация оживает чуть раньше, чем карточка въедет в кадр
const MARGIN = '100px';

let observer: IntersectionObserver | null = null;

const getObserver = () => {
    observer ??= new IntersectionObserver(entries => {
        for (const entry of entries)
            entry.target.classList.toggle(HIDDEN, !entry.isIntersecting);
    }, { rootMargin: MARGIN });
    return observer;
};

export const vPauseOffscreen: ObjectDirective<HTMLElement> = {
    mounted(el) {
        getObserver().observe(el);
    },
    beforeUnmount(el) {
        observer?.unobserve(el);
    },
};
