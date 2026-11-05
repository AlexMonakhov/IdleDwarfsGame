// Фоны-превью арен (карточки кампании, башни, PvP, боссов гильдии): npm run caves [-- --only цвет[,цвет]]
//
// Исходник: src/assets/red_crystal_arena.png (красная пещера с кристаллами).
// Готовые:  src/assets/caves/cave_{цвет}.webp — их импортирует battle/arenaBackgrounds.ts (arenaPreview).
//
// Красная — исходник как есть, остальные цвета — перекраска: в пещере красное всё (камень, кристаллы, свет),
// поэтому сдвигается оттенок каждого цветного пикселя, а яркость и разброс оттенков сохраняются
// (золотые грани и руны остаются светлее и теплее камня). Белая — почти без насыщенности и светлее.
// Превью уменьшены и сжаты в WebP: в бою фоном остаются большие арены из assets/arenas
const fs = require('fs');
const os = require('os');
const path = require('path');
const { execFileSync } = require('child_process');
const { PNG } = require('pngjs');
const ffmpeg = require('ffmpeg-static');

const ASSETS = path.join(__dirname, '..', 'src', 'assets');
const SOURCE = path.join(ASSETS, 'red_crystal_arena.png');
const CAVES = path.join(ASSETS, 'caves');

// Срез по бокам: справа внизу у исходника значок генератора, а срезая поровну, тоннель остаётся по центру
const CROP_X = 80;
// Ширина превью и качество WebP (0–100)
const WIDTH = 1024;
const QUALITY = 82;

// Основной оттенок красной пещеры (камень); кристаллы чуть в малиновый, грани и руны — в золото
const SOURCE_HUE = 8;
// Почти серое не красим: у него оттенок случайный
const SAT_FROM = 0.06;
const SAT_FULL = 0.2;
// Насколько сохраняется разброс оттенков: золотые грани не должны слиться с камнем
const HUE_SPREAD = 0.45;

// hue — новый оттенок; sat — множитель насыщенности; gamma < 1 осветляет, value — множитель яркости.
// iridescent — радужный перелив кристаллов, как у мифических предметов тьмы и сияния: камень красится основным
// цветом, а каждая грань кристалла — своим оттенком из диапазона from..to (см. iridescentHue).
// null — исходник как есть
const COLORS = {
  red: null,
  blue: { hue: 218, sat: 1.15, gamma: 1, value: 1 },
  // Золото, а не лимон: без осветления — тёмный камень уходит в бурый
  yellow: { hue: 38, sat: 1.05, gamma: 0.95, value: 1.05 },
  // Зелёный на глаз ярче остальных: камень затемнён, чтобы не был кислотным
  green: { hue: 128, sat: 0.9, gamma: 1.12, value: 0.85 },
  // Кровь: багровый с уходом в малиновый
  violet: { hue: 335, sat: 1, gamma: 1, value: 0.95 },
  // Без стихии (базовые герои 1-го ранга): тёплый серый камень, почти без цвета
  basic: { hue: 32, sat: 0.14, gamma: 0.95, value: 1 },
  // Тьма: почти чёрно-фиолетовый камень, кристаллы — чёрный опал (бирюза, зелень, синева, фиолет)
  dark: {
    hue: 268, sat: 0.75, gamma: 1.15, value: 0.62,
    iridescent: { from: 150, to: 310, sat: 0.8, gamma: 1, value: 0.95 },
  },
  // Сияние: холодное серебро; кристаллы — светлое стекло с пастельной радугой по граням
  white: {
    hue: 228, sat: 0.2, gamma: 1.05, value: 1.15,
    // Как у короны сияния: мята, голубой, лаванда, розовый — без жёлто-зелёного, на сером он выходит болотным.
    // Тёмные грани сильно осветлены: пастель на низкой яркости выходит грязной
    iridescent: { from: 165, to: 345, sat: 0.3, gamma: 0.45, value: 1.1 },
  },
};

// Что в исходнике кристалл, а не камень: камень почти чисто красный (оттенок у SOURCE_HUE),
// у тёмных граней кристаллов оттенок уходит в малиновый, у светлых граней и рун — в золото; ещё кристаллы ярче камня
const CRYSTAL_HUE_FROM = 16;
const CRYSTAL_HUE_FULL = 26;
// Самые тёмные места камня тоже малиновые — по оттенку кристаллом считается только то, что не темнее этого
const CRYSTAL_DARK_FROM = 0.14;
const CRYSTAL_DARK_FULL = 0.24;
const CRYSTAL_VALUE_FROM = 0.66;
const CRYSTAL_VALUE_FULL = 0.8;
// Перелив по картинке: полный круг оттенков на столько пикселей по диагонали
const IRIDESCENT_PERIOD = 900;

const smooth = (from, to, v) => {
  const t = Math.min(1, Math.max(0, (v - from) / (to - from)));
  return t * t * (3 - 2 * t);
};

function crop(src) {
  const width = src.width - CROP_X * 2;
  const out = new PNG({ width, height: src.height });
  PNG.bitblt(src, out, CROP_X, 0, width, src.height, 0, 0);
  return out;
}

// HSV (оттенок в градусах, s и v — 0..1) → RGB 0..255
function hsvToRgb(h, s, v) {
  h = ((h % 360) + 360) % 360;
  const c = v * s, x = c * (1 - Math.abs(((h / 60) % 2) - 1)), m = v - c;
  const [r, g, b] = h < 60 ? [c, x, 0] : h < 120 ? [x, c, 0] : h < 180 ? [0, c, x] : h < 240 ? [0, x, c] : h < 300 ? [x, 0, c] : [c, 0, x];
  return [(r + m) * 255, (g + m) * 255, (b + m) * 255];
}

// Оттенок перелива. Грани кристаллов в исходнике залиты ровными тонами разной яркости и оттенка,
// поэтому оттенок от яркости и исходного оттенка красит каждую грань своим цветом, ровно по всей грани;
// медленный сдвиг по диагонали картинки — чтобы соседние кристаллы не повторяли друг друга.
// Диапазон уже полного круга проходится туда-обратно, без скачка с конца на начало
function iridescentHue(range, x, y, off, value) {
  const phase = (((x + y * 0.7) / IRIDESCENT_PERIOD) * 360 + off * 3 + value * 160) / 360;
  const span = range.to - range.from;
  if (span >= 360) return range.from + (phase % 1) * 360;
  const wave = 1 - Math.abs(((phase % 1) + 1) % 1 * 2 - 1);
  return range.from + wave * span;
}

function recolor(src, target) {
  const out = new PNG({ width: src.width, height: src.height });
  const d = src.data, o = out.data;
  for (let i = 0; i < d.length; i += 4) {
    const r = d[i], g = d[i + 1], b = d[i + 2];
    o[i] = r; o[i + 1] = g; o[i + 2] = b; o[i + 3] = 255;
    if (!target) continue;

    const max = Math.max(r, g, b), min = Math.min(r, g, b), delta = max - min;
    if (!delta) continue;
    const sat = delta / max;
    let hue = max === r ? ((g - b) / delta) % 6 : max === g ? (b - r) / delta + 2 : (r - g) / delta + 4;
    hue = (hue * 60 + 360) % 360;
    const weight = smooth(SAT_FROM, SAT_FULL, sat);
    if (weight <= 0) continue;

    // Отклонение от основного оттенка по кратчайшей дуге
    const off = ((hue - SOURCE_HUE + 540) % 360) - 180;
    const value = max / 255;
    let [nr, ng, nb] = hsvToRgb(
      target.hue + off * HUE_SPREAD,
      Math.min(1, sat * target.sat),
      Math.min(1, value ** target.gamma * target.value));

    // Кристаллы с переливом: свой цвет на каждой грани поверх основного
    const irid = target.iridescent;
    if (irid) {
      const crystal = Math.max(
        smooth(CRYSTAL_HUE_FROM, CRYSTAL_HUE_FULL, Math.abs(off)) * smooth(CRYSTAL_DARK_FROM, CRYSTAL_DARK_FULL, value),
        smooth(CRYSTAL_VALUE_FROM, CRYSTAL_VALUE_FULL, value));
      if (crystal > 0) {
        const pixel = i / 4;
        const [ir, ig, ib] = hsvToRgb(
          iridescentHue(irid, pixel % src.width, Math.floor(pixel / src.width), off, value),
          Math.min(1, sat * irid.sat),
          Math.min(1, value ** irid.gamma * irid.value));
        nr += (ir - nr) * crystal;
        ng += (ig - ng) * crystal;
        nb += (ib - nb) * crystal;
      }
    }

    o[i] = r + (nr - r) * weight;
    o[i + 1] = g + (ng - g) * weight;
    o[i + 2] = b + (nb - b) * weight;
  }
  return PNG.sync.write(out);
}

// PNG → уменьшенный WebP через ffmpeg (временный файл: ffmpeg читает картинку с диска)
function writeWebp(pngBuffer, target) {
  const tmp = path.join(os.tmpdir(), `cave-${process.pid}-${path.basename(target, '.webp')}.png`);
  fs.writeFileSync(tmp, pngBuffer);
  try {
    execFileSync(ffmpeg, [
      '-y', '-loglevel', 'error', '-i', tmp,
      '-vf', `scale=${WIDTH}:-2:flags=lanczos`,
      '-c:v', 'libwebp', '-quality', String(QUALITY), '-compression_level', '6', target,
    ], { stdio: ['ignore', 'inherit', 'inherit'] });
  } finally {
    fs.rmSync(tmp, { force: true });
  }
  console.log(`${path.basename(target)}: ${Math.round(fs.statSync(target).size / 1024)} KB`);
}

const args = process.argv.slice(2);
const onlyAt = args.indexOf('--only');
const only = onlyAt < 0 ? Object.keys(COLORS) : args[onlyAt + 1].split(',');

fs.mkdirSync(CAVES, { recursive: true });
const source = crop(PNG.sync.read(fs.readFileSync(SOURCE)));
for (const color of only) {
  if (!(color in COLORS)) {
    console.warn(`Неизвестный цвет: ${color}`);
    continue;
  }
  writeWebp(recolor(source, COLORS[color]), path.join(CAVES, `cave_${color}.webp`));
}
