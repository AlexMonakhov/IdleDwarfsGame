<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { getSpriteUrl, SPRITE_COLUMNS } from '@/battle/sprites';

type SmoothingQuality = 'low' | 'medium' | 'high';

// Тестовая страница: качество idle — текущий лист 2048 против увеличенных (4K у зелёного, 4K и 8K у белого).
// Те же кадры и тот же размер на экране. Можно рисовать канвас с учётом плотности пикселей экрана
// (devicePixelRatio, растёт и от Ctrl+колесо) и менять качество сглаживания при сжатии кадра
const FRAME = 409.6;
const FRAMES = 24;
const FPS = 16;

// Страница больше не подключена к маршрутам (сравнение закончено) — файл можно удалить
const DWARFS = [
    {
        name: 'Белый: 4K',
        sheets: [{ label: '4K — лист в игре (4096)', src: getSpriteUrl('idle', 'white', 4) }],
    },
];

const dwarfIndex = ref(0);
const sheets = computed(() => DWARFS[dwarfIndex.value].sheets);

const SCALES = [
    { value: 0.55, label: '0.55 — экспедиция' },
    { value: 0.65, label: '0.65 — бой' },
    { value: 1, label: '1' },
    { value: 2, label: '2' },
];

const scale = ref(0.65);
const hiDpi = ref(true);
const quality = ref<SmoothingQuality>('high');
const paused = ref(false);
const light = ref(false);
const frame = ref(0);
const dpr = ref(window.devicePixelRatio || 1);
const loaded = ref(0);

// Листы грузятся по мере выбора дворфа: 8K-лист в памяти браузера занимает ~250 МБ
const imageCache = new Map<string, HTMLImageElement>();
const loadImage = (src: string) => {
    let image = imageCache.get(src);
    if (!image) {
        image = new Image();
        image.onload = () => loaded.value++;
        image.src = src;
        imageCache.set(src, image);
    }
    return image;
};
const images = computed(() => sheets.value.map(sheet => loadImage(sheet.src)));

const cssSize = computed(() => FRAME * scale.value);
const ratio = computed(() => (hiDpi.value ? dpr.value : 1));
const screenPx = computed(() => Math.round(cssSize.value * ratio.value));

// Во сколько раз браузер сжимает кадр листа до пикселей экрана (меньше 1 — растягивает)
const details = computed(() => {
    void loaded.value;
    return images.value.map(image => {
        const cell = image.naturalWidth / SPRITE_COLUMNS;
        return cell ? `кадр ${Math.round(cell)}px → ${screenPx.value}px, сжатие x${(cell / screenPx.value).toFixed(2)}` : 'загрузка…';
    });
});

const canvases: (HTMLCanvasElement | null)[] = [];
const setCanvas = (index: number) => (el: unknown) => {
    canvases[index] = el as HTMLCanvasElement | null;
};

const draw = () => {
    canvases.forEach((canvas, index) => {
        const image = images.value[index];
        if (!canvas || !image?.naturalWidth) return;
        const px = screenPx.value;
        if (canvas.width !== px) {
            canvas.width = px;
            canvas.height = px;
        }
        // Смена размера канваса сбрасывает настройки контекста — ставим их каждый раз
        const ctx = canvas.getContext('2d');
        if (!ctx) return;
        ctx.imageSmoothingEnabled = true;
        ctx.imageSmoothingQuality = quality.value;
        const cell = image.naturalWidth / SPRITE_COLUMNS;
        const col = frame.value % SPRITE_COLUMNS;
        const row = Math.floor(frame.value / SPRITE_COLUMNS);
        ctx.clearRect(0, 0, px, px);
        ctx.drawImage(image, col * cell, row * cell, cell, cell, 0, 0, px, px);
    });
};

let animationId = 0;
let started = 0;
const tick = (time: number) => {
    if (!started) started = time;
    dpr.value = window.devicePixelRatio || 1;
    if (!paused.value) frame.value = Math.floor(((time - started) * FPS) / 1000) % FRAMES;
    draw();
    animationId = requestAnimationFrame(tick);
};

const step = (delta: number) => {
    paused.value = true;
    frame.value = (frame.value + delta + FRAMES) % FRAMES;
};

onMounted(() => {
    animationId = requestAnimationFrame(tick);
});
onBeforeUnmount(() => cancelAnimationFrame(animationId));
</script>

<template>
    <div class="sprite-test" :class="{ 'sprite-test-light': light }">
        <h1>Сравнение спрайт-листов: idle</h1>

        <div class="controls">
            <label>
                Дворф
                <select v-model.number="dwarfIndex">
                    <option v-for="(dwarf, index) in DWARFS" :key="dwarf.name" :value="index">{{ dwarf.name }}</option>
                </select>
            </label>
            <label>
                Масштаб
                <select v-model.number="scale">
                    <option v-for="option in SCALES" :key="option.value" :value="option.value">{{ option.label }}</option>
                </select>
            </label>
            <label>
                <input v-model="hiDpi" type="checkbox" />
                Учитывать плотность экрана (devicePixelRatio = {{ dpr.toFixed(2) }})
            </label>
            <label>
                Сглаживание
                <select v-model="quality">
                    <option value="low">low (как сейчас в игре)</option>
                    <option value="medium">medium</option>
                    <option value="high">high</option>
                </select>
            </label>
            <label>
                <input v-model="light" type="checkbox" />
                Светлый фон
            </label>
            <span class="frame-controls">
                <button @click="step(-1)">◀</button>
                <button @click="paused = !paused">{{ paused ? 'Пуск' : 'Пауза' }}</button>
                <button @click="step(1)">▶</button>
                кадр {{ frame + 1 }} / {{ FRAMES }}
            </span>
        </div>

        <div class="columns">
            <div v-for="(sheet, index) in sheets" :key="`${dwarfIndex}-${index}`" class="column">
                <div class="column-title">{{ sheet.label }}</div>
                <canvas :ref="setCanvas(index)" :style="{ width: `${cssSize}px`, height: `${cssSize}px` }"></canvas>
                <div class="column-info">{{ details[index] }}</div>
            </div>
        </div>

        <p class="hint">
            В игре сейчас канвас рисуется без учёта плотности экрана и со сглаживанием low: чтобы увидеть, как это выглядит,
            снимите галочку и выберите low. Ctrl + колесо мыши увеличивает devicePixelRatio — так видно, сколько деталей
            есть в листе при крупном показе.
        </p>
    </div>
</template>

<style scoped>
.sprite-test {
    box-sizing: border-box;
    min-height: 100vh;
    padding: 24px;
    background: #1e293b;
    color: #f8fafc;
    font-family: 'Arial', sans-serif;
}

.sprite-test-light {
    background: #cbd5e1;
    color: #0f172a;
}

h1 {
    margin: 0 0 16px;
    font-size: 20px;
}

.controls {
    display: flex;
    flex-wrap: wrap;
    gap: 12px 24px;
    align-items: center;
    margin-bottom: 24px;
    font-size: 14px;
}

.frame-controls {
    display: inline-flex;
    gap: 6px;
    align-items: center;
}

.columns {
    display: flex;
    flex-wrap: wrap;
    gap: 32px;
}

.column {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.column-title {
    font-weight: 700;
}

.column-info {
    font-family: monospace;
    font-size: 12px;
    opacity: 0.8;
}

.column canvas {
    display: block;
    outline: 1px dashed rgba(148, 163, 184, 0.5);
}

.hint {
    max-width: 760px;
    margin-top: 24px;
    font-size: 13px;
    line-height: 1.5;
    opacity: 0.85;
}
</style>
