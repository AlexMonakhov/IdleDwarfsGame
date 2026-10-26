import type { ObjectDirective } from 'vue';

// Прокрутка без полосы: колесом, как обычно, и перетаскиванием мышью — как пальцем на телефоне, с инерцией.
// Края мягко гаснут, пока за ними есть содержимое. Стили — классы drag-scroll-* в App.vue.
// { fade: false } — без затухания: маска срезала бы внешнюю тень самого элемента
export interface DragScrollOptions {
    fade?: boolean;
}

// Сдвиг, после которого нажатие считается перетаскиванием, а не кликом
const DRAG_THRESHOLD = 6;
// Инерция: доля скорости, что остаётся за кадр (16 мс), и скорость остановки, px/мс
const FRICTION = 0.94;
const MIN_SPEED = 0.02;
// Скорость берём по последним 100 мс движения
const VELOCITY_WINDOW = 100;
const FADE_SIZE = 28;

// Здесь мышь нужна для своего: выделение текста или перетаскивание карточки
const IGNORE = 'input, textarea, select, [contenteditable="true"], [draggable="true"]';

// Вложенные области: нажатие забирает самая внутренняя, внешняя его пропускает
const claimed = new WeakSet<Event>();
const cleanups = new WeakMap<HTMLElement, () => void>();

const swallowClick = (e: Event) => {
    e.stopPropagation();
    e.preventDefault();
};

export const vDragScroll: ObjectDirective<HTMLElement, DragScrollOptions | undefined> = {
    mounted(el, binding) {
        const fade = binding.value?.fade !== false;
        el.classList.add('drag-scroll');
        if (fade) el.classList.add('drag-scroll-fade');

        let pointerId: number | null = null;
        let start = { x: 0, y: 0, left: 0, top: 0 };
        let dragging = false;
        let samples: { t: number; x: number; y: number }[] = [];
        let inertiaFrame = 0;

        const canScroll = () => el.scrollHeight > el.clientHeight || el.scrollWidth > el.clientWidth;

        const update = () => {
            el.classList.toggle('drag-scroll-scrollable', canScroll());
            if (!fade) return;
            const top = Math.min(el.scrollTop, FADE_SIZE);
            const bottom = Math.min(el.scrollHeight - el.clientHeight - el.scrollTop, FADE_SIZE);
            el.style.setProperty('--fade-top', `${Math.max(0, top)}px`);
            el.style.setProperty('--fade-bottom', `${Math.max(0, bottom)}px`);
        };

        const stopInertia = () => {
            cancelAnimationFrame(inertiaFrame);
            inertiaFrame = 0;
        };

        const startInertia = (vx: number, vy: number) => {
            let prev = performance.now();
            const step = (now: number) => {
                const dt = now - prev;
                prev = now;
                el.scrollLeft += vx * dt;
                el.scrollTop += vy * dt;
                const decay = Math.pow(FRICTION, dt / 16);
                vx *= decay;
                vy *= decay;
                inertiaFrame = Math.hypot(vx, vy) > MIN_SPEED ? requestAnimationFrame(step) : 0;
            };
            inertiaFrame = requestAnimationFrame(step);
        };

        const onMove = (e: PointerEvent) => {
            if (e.pointerId !== pointerId) return;
            const dx = e.clientX - start.x;
            const dy = e.clientY - start.y;
            if (!dragging) {
                if (Math.hypot(dx, dy) < DRAG_THRESHOLD) return;
                dragging = true;
                el.classList.add('drag-scroll-active');
                window.getSelection()?.removeAllRanges();
            }
            el.scrollLeft = start.left - dx;
            el.scrollTop = start.top - dy;
            samples.push({ t: e.timeStamp, x: e.clientX, y: e.clientY });
            while (samples.length > 2 && e.timeStamp - samples[0].t > VELOCITY_WINDOW) samples.shift();
        };

        const release = () => {
            pointerId = null;
            window.removeEventListener('pointermove', onMove);
            window.removeEventListener('pointerup', onUp);
            window.removeEventListener('pointercancel', onUp);
        };

        const onUp = (e: PointerEvent) => {
            if (e.pointerId !== pointerId) return;
            release();
            if (!dragging) return;
            dragging = false;
            el.classList.remove('drag-scroll-active');

            // Отпустили над карточкой или кнопкой — это конец прокрутки, а не клик по ней.
            // click приходит в той же задаче, что и pointerup, так что таймаут снимет перехват после него
            window.addEventListener('click', swallowClick, { capture: true, once: true });
            setTimeout(() => window.removeEventListener('click', swallowClick, { capture: true }));

            const first = samples[0];
            const last = samples[samples.length - 1];
            const dt = last.t - first.t;
            // Мышь остановили перед тем, как отпустить, — без инерции
            if (dt > 0 && e.timeStamp - last.t < 50) startInertia(-(last.x - first.x) / dt, -(last.y - first.y) / dt);
        };

        const onDown = (e: PointerEvent) => {
            if (e.pointerType !== 'mouse' || e.button !== 0 || claimed.has(e)) return;
            stopInertia();
            if ((e.target as Element).closest(IGNORE) || !canScroll()) return;
            claimed.add(e);
            pointerId = e.pointerId;
            start = { x: e.clientX, y: e.clientY, left: el.scrollLeft, top: el.scrollTop };
            samples = [{ t: e.timeStamp, x: e.clientX, y: e.clientY }];
            window.addEventListener('pointermove', onMove);
            window.addEventListener('pointerup', onUp);
            window.addEventListener('pointercancel', onUp);
        };

        // Картинки и ссылки браузер тащит сам и обрывает нашу прокрутку; карточки с draggable — не трогаем
        const onDragStart = (e: DragEvent) => {
            if (!(e.target as Element).closest?.('[draggable="true"]')) e.preventDefault();
        };

        el.addEventListener('pointerdown', onDown);
        el.addEventListener('dragstart', onDragStart);
        el.addEventListener('wheel', stopInertia, { passive: true });
        el.addEventListener('scroll', update, { passive: true });

        // Затухание зависит и от размеров содержимого: список догрузился, вкладку переключили
        const resizeObserver = new ResizeObserver(update);
        const observeChildren = () => {
            resizeObserver.disconnect();
            resizeObserver.observe(el);
            Array.from(el.children).forEach(child => resizeObserver.observe(child));
        };
        const mutationObserver = new MutationObserver(() => {
            observeChildren();
            update();
        });
        observeChildren();
        mutationObserver.observe(el, { childList: true, subtree: true });
        update();

        cleanups.set(el, () => {
            stopInertia();
            release();
            el.removeEventListener('pointerdown', onDown);
            el.removeEventListener('dragstart', onDragStart);
            el.removeEventListener('wheel', stopInertia);
            el.removeEventListener('scroll', update);
            resizeObserver.disconnect();
            mutationObserver.disconnect();
        });
    },

    beforeUnmount(el) {
        cleanups.get(el)?.();
        cleanups.delete(el);
    },
};
