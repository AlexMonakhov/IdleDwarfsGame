// Сборка спрайт-листов из видео: npm run sprites [-- часть_имени]
//
// Видео:  src/assets/videos/{юнит}_{анимация}_lvl{N}_{цвет}[_{вариант}].mp4  (фон однотонный: белый, серый или зелёный)
// Лист:   src/assets/sprite-lists/{юнит}-{анимация}/{юнит}_{анимация}_lvl{N}_{цвет}_{вариант}.png
//
// Лист 2048x2048, сетка 5x5, кадр 409.6px (см. battle/sprites.ts).
// Размер и положение персонажа задаёт idle того же юнита/уровня/цвета: остальные анимации подгоняются
// так, чтобы их первый кадр (поза покоя) совпал с первым кадром idle — иначе при смене анимации персонаж прыгает.
//
// Видео атак сняты с камерой в 2 раза дальше (прыжки и шлейфы шире персонажа), поэтому кадр атаки
// охватывает в FIELD раз большую область вокруг того же центра; если и этого мало (или прыгают в победе) —
// поле расширяется само. Игра рисует такие листы во столько же раз крупнее, канвас выходит за рамку юнита.
// Множитель каждого листа пишется в sprite-lists/sprite-meta.json
const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');
const { PNG } = require('pngjs');
const ffmpeg = require('ffmpeg-static');

const ROOT = path.join(__dirname, '..', 'src', 'assets');
const VIDEOS = path.join(ROOT, 'videos');
const SPRITES = path.join(ROOT, 'sprite-lists');
const META = path.join(SPRITES, 'sprite-meta.json');

const SHEET = 2048;
const COLS = 5;
const CELL = SHEET / COLS;
const FRAMES = 25;
// Во сколько раз кадр анимации шире кадра idle, и размер её листа (3072 — чтобы персонаж не терял в резкости)
const FIELD = { attack: 2 };
const SHEET_PX = { attack: 3072 };
// Если контент всё равно вылезает за кадр больше чем на CLIP_TOLERANCE кадра, поле расширяется до MAX_FIELD
const MAX_FIELD = 3;
const CLIP_TOLERANCE = 0.04;
// Как у dwarf_idle_lvl4: рост с мечом, линия ног и центр по X внутри кадра
const TARGET_H = 245;
const FEET_Y = 345;
const CENTER_X = 226;
// Зацикленные анимации берутся до кадра, похожего на первый; остальные — целиком
const LOOPED = new Set(['idle', 'walk', 'victory']);
// Видео, где меч в паре кадров уходит за край (обрезан ещё в исходнике): такие кадры подменяются ближайшим целым
const FIX_CLIPPED = new Set(['dwarf_victory_lvl4_purple.mp4']);
// Анимации без шлейфов и вспышек: контуром считается и светлая обводка (см. cutout)
const NO_EFFECTS = new Set(['idle', 'walk', 'victory']);

const VIDEO_FILE = /^([a-z]+)_([a-z]+)_lvl(\d+)_([a-z]+)(?:_(\d+))?\.mp4$/;

/* ===== Кадры ===== */
// Кадры читаются из ffmpeg сразу в память (RGBA), без временных файлов на диске
function readFrames(video) {
  const first = execFileSync(ffmpeg, ['-v', 'error', '-i', video, '-frames:v', '1', '-f', 'image2pipe', '-c:v', 'png', '-']);
  const { width, height } = PNG.sync.read(first);
  const raw = execFileSync(ffmpeg, ['-v', 'error', '-i', video, '-f', 'rawvideo', '-pix_fmt', 'rgba', '-'], { maxBuffer: 2 ** 31 - 1 });
  const size = width * height * 4;
  return Array.from({ length: Math.floor(raw.length / size) }, (_, i) => ({ width, height, data: raw.subarray(i * size, (i + 1) * size) }));
}

const frameDiff = (a, b) => {
  let s = 0;
  for (let i = 0; i < a.data.length; i += 16) s += Math.abs(a.data[i] - b.data[i]);
  return s;
};

function pickFrames(frames, looped) {
  const n = frames.length;
  if (!looped) return Array.from({ length: FRAMES }, (_, i) => Math.round((i * (n - 1)) / (FRAMES - 1)));

  let loopEnd = n;
  let best = Infinity;
  for (let k = Math.floor(n * 0.6); k < n; k++) {
    const v = frameDiff(frames[0], frames[k]);
    if (v < best) { best = v; loopEnd = k; }
  }
  return Array.from({ length: FRAMES }, (_, i) => Math.round((i * loopEnd) / FRAMES));
}

// Обрезанный краем видео кадр заменяется предыдущим целым (в самом начале — следующим): тайминг не сбивается
function replaceClipped(frames, paper) {
  const whole = frames.map(f => !touchesEdge(f, paper));
  const nearest = i => {
    for (let j = i - 1; j >= 0; j--) if (whole[j]) return frames[j];
    for (let j = i + 1; j < frames.length; j++) if (whole[j]) return frames[j];
    return frames[i];
  };
  return { frames: frames.map((f, i) => (whole[i] ? f : nearest(i))), replaced: whole.filter(w => !w).length };
}

/* ===== Вырезание фона ===== */
const { paperColor, cutout } = require('./cutout');

// Персонаж касается края видео — значит, что-то (обычно поднятый меч) обрезано ещё в исходнике
function touchesEdge(png, paper) {
  const { width: W, height: H, data: d } = png;
  let hits = 0;
  const check = p => {
    const i = p * 4;
    if (Math.abs(d[i] - paper[0]) + Math.abs(d[i + 1] - paper[1]) + Math.abs(d[i + 2] - paper[2]) > 90) hits++;
  };
  for (let x = 0; x < W; x++) { check(x); check((H - 1) * W + x); }
  for (let y = 0; y < H; y++) { check(y * W); check(y * W + W - 1); }
  return hits > (4 * W) / 768;
}

function bbox(img, minAlpha = 128) {
  const { W, H, px } = img;
  let x0 = W, y0 = H, x1 = -1, y1 = -1;
  for (let y = 0; y < H; y++) for (let x = 0; x < W; x++) {
    if (px[(y * W + x) * 4 + 3] < minAlpha) continue;
    if (x < x0) x0 = x; if (x > x1) x1 = x; if (y < y0) y0 = y; if (y > y1) y1 = y;
  }
  return { x0, y0, x1: x1 + 1, y1: y1 + 1 };
}

function silhouetteArea(img) {
  let n = 0;
  for (let i = 3; i < img.px.length; i += 4) if (img.px[i] >= 128) n++;
  return n;
}

// Центр стоп: средний X непрозрачных пикселей в нижних 10% фигуры — не зависит от того, куда смотрит меч
function feetX(img, b) {
  const { W, px } = img;
  let sum = 0, count = 0;
  for (let y = Math.floor(b.y1 - (b.y1 - b.y0) * 0.1); y < b.y1; y++) for (let x = b.x0; x < b.x1; x++) {
    if (px[(y * W + x) * 4 + 3] >= 128) { sum += x; count++; }
  }
  return count ? sum / count : (b.x0 + b.x1) / 2;
}

/* ===== Сборка листа ===== */
// k — масштаб (пиксели видео -> пиксели кадра), (ox, oy) — точка видео в левом верхнем углу кадра
function render(cells, { k, ox, oy }, size) {
  const sheet = new PNG({ width: size, height: size });
  const cell = size / COLS;
  const SS = Math.min(6, Math.max(3, Math.ceil(2 / k)));
  const acc = [0, 0, 0, 0];

  const sample = (img, sx, sy) => {
    const { W, H, px } = img;
    sx -= 0.5; sy -= 0.5;
    const xi = Math.floor(sx), yi = Math.floor(sy), fx = sx - xi, fy = sy - yi;
    for (let j = 0; j < 2; j++) for (let i = 0; i < 2; i++) {
      const xx = xi + i, yy = yi + j;
      if (xx < 0 || yy < 0 || xx >= W || yy >= H) continue;
      const w = (i ? fx : 1 - fx) * (j ? fy : 1 - fy), p = (yy * W + xx) * 4;
      for (let c = 0; c < 4; c++) acc[c] += px[p + c] * w;
    }
  };

  for (let Y = 0; Y < size; Y++) for (let X = 0; X < size; X++) {
    const col = Math.floor(X / cell), row = Math.floor(Y / cell);
    const img = cells[row * COLS + col];
    if (!img) continue;
    acc[0] = acc[1] = acc[2] = acc[3] = 0;
    for (let sj = 0; sj < SS; sj++) for (let si = 0; si < SS; si++) {
      const lx = X - col * cell + (si + 0.5) / SS, ly = Y - row * cell + (sj + 0.5) / SS;
      sample(img, ox + lx / k, oy + ly / k);
    }
    const a = acc[3] / (SS * SS);
    if (a < 0.5) continue;
    const o = (Y * size + X) * 4;
    for (let c = 0; c < 3; c++) sheet.data[o + c] = Math.round(Math.min(255, (acc[c] / acc[3]) * 255));
    sheet.data[o + 3] = Math.round(a);
  }
  return PNG.sync.write(sheet);
}

// Сколько пикселей контента не влезает в кадр с каждой стороны (эффекты атак бывают шире кадра)
function clipping(cells, { k, ox, oy }, cell) {
  let worst = { left: 0, top: 0, right: 0, bottom: 0 };
  cells.forEach(img => {
    const b = bbox(img, 26);
    worst = {
      left: Math.max(worst.left, -(b.x0 - ox) * k),
      top: Math.max(worst.top, -(b.y0 - oy) * k),
      right: Math.max(worst.right, (b.x1 - ox) * k - cell),
      bottom: Math.max(worst.bottom, (b.y1 - oy) * k - cell),
    };
  });
  return worst;
}

const clippingText = worst =>
  Object.entries(worst).filter(([, v]) => v > 1).map(([side, v]) => `${side} ${Math.round(v)}px`).join(', ');

/* ===== Main ===== */
function build(video, anim, rest) {
  let frames = readFrames(path.join(VIDEOS, video.file));
  const paper = paperColor(frames[0]);
  let replaced = 0;
  if (FIX_CLIPPED.has(video.file)) ({ frames, replaced } = replaceClipped(frames, paper));
  const picks = pickFrames(frames, LOOPED.has(anim));
  const cells = picks.map(i => cutout(frames[i], paper, NO_EFFECTS.has(anim)));
  const first = bbox(cells[0]);
  const area = silhouetteArea(cells[0]);

  // Подгонка считается в пикселях кадра idle (CELL): k — масштаб, (x0, y0) — точка видео в левом верхнем углу кадра idle
  let k, x0, y0;
  if (!rest && anim === 'idle') {
    // Своя подгонка: idle целиком влезает в рост TARGET_H, ноги на FEET_Y
    const all = cells.map(c => bbox(c)).reduce((a, b) => ({
      x0: Math.min(a.x0, b.x0), y0: Math.min(a.y0, b.y0), x1: Math.max(a.x1, b.x1), y1: Math.max(a.y1, b.y1),
    }));
    k = TARGET_H / (all.y1 - all.y0);
    x0 = (all.x0 + all.x1) / 2 - CENTER_X / k;
    y0 = all.y1 - FEET_Y / k;
  } else {
    // Камера в сгенерированных видео отъезжает не ровно в FIELD раз, а поза покоя бывает другой (меч поднят),
    // поэтому масштаб — по площади силуэта первого кадра, она от позы зависит меньше, чем рост.
    // Первый кадр ставится ногами туда же, где стоят ноги в первом кадре idle
    k = rest ? Math.sqrt(rest.area / area) : TARGET_H / (first.y1 - first.y0);
    const pose = rest ?? { feetX: CENTER_X, feet: FEET_Y };
    x0 = feetX(cells[0], first) - pose.feetX / k;
    y0 = first.y1 - pose.feet / k;
  }
  // В пикселях кадра idle
  const restPose = { area: area * k * k, feetX: (feetX(cells[0], first) - x0) * k, feet: (first.y1 - y0) * k };

  // Кадр с полем field > 1 охватывает в field раз большую область вокруг того же центра, что и кадр idle
  const place = (field, cell) => {
    const shift = (CELL * field - CELL) / 2;
    return { k: (k * cell) / (CELL * field), ox: x0 - shift / k, oy: y0 - shift / k };
  };
  // Прыжок или шлейф не влез — поле расширяется, а не обрезается (мелкие искры у края не в счёт).
  // idle задаёт кадр и не расширяется
  let field = FIELD[anim] ?? 1;
  const overflow = f => Math.max(...Object.values(clipping(cells, place(f, CELL), CELL)));
  while (anim !== 'idle' && field < MAX_FIELD && overflow(field) > CELL * CLIP_TOLERANCE) field += 0.25;
  // Расширенный кадр — на листе покрупнее, чтобы персонаж не терял в резкости
  const size = SHEET_PX[anim] ?? (field > 1 ? 3072 : SHEET);
  const cell = size / COLS;
  const transform = place(field, cell);
  return {
    png: render(cells, transform, size),
    size,
    field,
    restPose,
    info: `${frames.length} кадров${replaced ? ` (${replaced} с мечом за краем видео заменено соседними)` : ''} -> [${picks.join(',')}], лист ${size}px, кадр x${field}`,
    clip: clippingText(clipping(cells, transform, cell)),
  };
}

// Ширина PNG из заголовка, без чтения всего файла
const pngWidth = file => {
  const header = Buffer.alloc(24);
  const fd = fs.openSync(file, 'r');
  try { fs.readSync(fd, header, 0, 24, 0); } finally { fs.closeSync(fd); }
  return header.readUInt32BE(16);
};

const filter = process.argv[2] ?? '';
const meta = fs.existsSync(META) ? JSON.parse(fs.readFileSync(META, 'utf8')) : {};
const groups = new Map();
fs.readdirSync(VIDEOS).forEach(file => {
  const m = VIDEO_FILE.exec(file);
  if (!m) return;
  const [, unit, anim, lvl, color, variant = '1'] = m;
  const key = `${unit}_lvl${lvl}_${color}`;
  if (!groups.has(key)) groups.set(key, []);
  groups.get(key).push({ file, unit, anim, lvl, color, variant });
});

for (const [key, videos] of groups) {
  if (!videos.some(v => v.file.includes(filter))) continue;
  // idle первым: он задаёт позу покоя для остальных анимаций
  videos.sort((a, b) => (b.anim === 'idle') - (a.anim === 'idle') || a.anim.localeCompare(b.anim) || a.variant - b.variant);
  if (videos[0].anim !== 'idle') console.warn(`${key}: нет idle-видео, размер подгоняется по каждой анимации отдельно`);

  let rest = null;
  for (const v of videos) {
    const res = build(v, v.anim, rest);
    if (v.anim === 'idle' && !rest) rest = res.restPose;
    const dir = path.join(SPRITES, `${v.unit}-${v.anim}`);
    fs.mkdirSync(dir, { recursive: true });
    const name = `${v.unit}_${v.anim}_lvl${v.lvl}_${v.color}_${v.variant}.png`;
    const target = path.join(dir, name);
    // Лист крупнее собранного — апскейл, сделанный вручную: не затираем его (поза покоя всё равно берётся из видео).
    // Апскейл должен быть ровно в N раз больше листа скрипта, иначе персонаж сдвинется относительно других анимаций
    if (fs.existsSync(target) && pngWidth(target) > res.size) {
      console.log(`${v.file} -> ${v.unit}-${v.anim}/${name}  оставлен апскейл ${pngWidth(target)}px`);
      continue;
    }
    fs.writeFileSync(target, res.png);
    if (res.field !== 1) meta[name] = { zoom: res.field };
    else delete meta[name];
    console.log(`${v.file} -> ${v.unit}-${v.anim}/${name}  (${res.info})${res.clip ? `  обрезано: ${res.clip}` : ''}`);
  }
}

const sorted = Object.fromEntries(Object.keys(meta).sort().map(key => [key, meta[key]]));
fs.writeFileSync(META, JSON.stringify(sorted, null, 2) + '\n');

// Игра рисует атласы, а не сами листы: пересобираем атласы изменившихся листов (см. sprite-atlas.js)
execFileSync(process.execPath, [path.join(__dirname, 'sprite-atlas.js'), filter], { stdio: 'inherit' });
