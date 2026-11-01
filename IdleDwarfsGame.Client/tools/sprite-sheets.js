// Сборка спрайт-листов из видео: npm run sprites [-- часть_имени] [--no-atlas]
// --no-atlas — только PNG-листы, без webp-атласов (игра рисует лист, пока у него нет атласа)
//
// Видео:  src/assets/videos/[подпапка/]{юнит}_{анимация}_lvl{N}_{цвет}[_{вариант}].mp4  (фон однотонный: белый, серый или зелёный)
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
const ATLAS_META = path.join(ROOT, 'sprite-atlas', 'atlas-meta.json');

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
// Как у dwarf_idle_lvl3: рост с мечом, линия ног и центр по X внутри кадра
const TARGET_H = 245;
const FEET_Y = 345;
const CENTER_X = 226;
// Зацикленные анимации берутся до кадра, похожего на первый; остальные — целиком
const LOOPED = new Set(['idle', 'walk', 'victory']);
// Видео, где меч в паре кадров уходит за край (обрезан ещё в исходнике): такие кадры подменяются ближайшим целым
const FIX_CLIPPED = new Set(['dwarf_victory_lvl3_purple.mp4']);
// Дворфы этого уровня собраны по-старому (эти листы и атласы уже в игре), у всех остальных юнитов правила такие.
// В победах меч за краем видео бывает в половине кадров: подмена дала бы застывания, поэтому такие
// кадры просто не берутся в лист. Атаки не трогаем: края там касаются искры удара, а меч не выходит.
// Кроме того, меч в позе покоя бывает ниже ног и выше головы, а камера у видео разная: размер и положение считаются
// по телу без меча (см. bodyAnchor). Площадь тела и центр стоп в пикселях кадра idle — как у дворфов 3-го уровня.
// По-старому собран и rainbow: его видео сняты вместе с этим уровнем
const LEGACY_LVL = 3;
const BODY_AREA = 24300;
const BODY_FEET_X = 220;
// Дворфы, у которых вся броня светлая (без тёмных частей контур на сером фоне не замыкается)
const LIGHT_ARMOR = new Set(['lvl4_white']);
// Атаки белых дворфов, у которых обычное вырезание съедает меч, корону и кристаллы: грани почти как фон.
// Шлейфы при этом выходят плотнее, поэтому только для этих видео (см. cutout: lightOutline, faintOutline, lightDelta, bigHole).
// keepScale — масштаб считается по обычному вырезанию: он был верным, а площадь плотного силуэта завышена и уменьшила бы дворфа.
// У lvl2 наоборот: в обычном вырезании меч дырявый, площадь занижена и дворф выходит крупнее idle
const WHITE_ATTACK_FIX = {
  dwarf_attack_lvl2_white_1: { faint: true, lightDelta: 30, bigHole: 1000 },
  dwarf_attack_lvl2_white_2: { faint: true, lightDelta: 30, bigHole: 1000 },
  dwarf_attack_lvl3_white_3: { faint: false, lightDelta: 60, keepScale: true },
  dwarf_attack_lvl3_white_4: { faint: false, lightDelta: 60, bigHole: 1000 },
  dwarf_attack_lvl4_white_1: { faint: true, lightDelta: 30, keepScale: true },
  dwarf_attack_lvl4_white_3: { faint: true, lightDelta: 30, keepScale: true },
};
// Тени на полу в этих видео почти нет, а заливка съедает сапоги и кольчугу: цвет у них как у тени (см. eraseGroundShadow)
const KEEP_FLOOR = new Set(['goblin_victory_lvl1_blue', 'skeleton_victory_lvl1_blue']);
// Поправка масштаба, подобранная по росту от короны до пяток. В начале этих побед меч спрятан за спиной: силуэт меньше,
// чем в idle, и подгонка по площади (см. build) завышает дворфа. Эталон — жёлтый: в кадре покоя рост как в idle.
// Скелет в ударах нарисован выше, чем в idle, при том же черепе: размер подобран на глаз в стресс-арене
const SCALE_FIX = {
  dwarf_victory_lvl3_blue: 0.92,
  dwarf_victory_lvl3_green: 0.91,
  dwarf_victory_lvl3_red: 0.9,
  dwarf_victory_lvl3_white: 0.925,
  skeleton_attack_lvl1_blue_1: 0.96,
  skeleton_attack_lvl1_blue_2: 0.96,
};
// Кадр победы с силуэтом меньше этой доли от соседних — контур на смазанном кадре порвался и броня просвечивает
const MIN_AREA_SHARE = 0.96;
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

// pool — номера кадров, из которых можно выбирать (без обрезанных краем видео)
function pickFrames(frames, looped, pool = frames.map((_, i) => i)) {
  const n = pool.length;
  if (!looped) return Array.from({ length: FRAMES }, (_, i) => pool[Math.round((i * (n - 1)) / (FRAMES - 1))]);

  let loopEnd = n;
  let best = Infinity;
  for (let k = Math.floor(n * 0.6); k < n; k++) {
    const v = frameDiff(frames[pool[0]], frames[pool[k]]);
    if (v < best) { best = v; loopEnd = k; }
  }
  return Array.from({ length: FRAMES }, (_, i) => pool[Math.round((i * loopEnd) / FRAMES)]);
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

// Тело без меча: то, что остаётся от силуэта, если катать по нему квадрат шире клинка (морфологическое открытие).
// Возвращает площадь тела, центр стоп и линию подошвы в пикселях видео
function bodyAnchor(img) {
  const { W, H, px } = img;
  const win = Math.sqrt(silhouetteArea(img)) * 0.2;
  const step = Math.max(1, Math.floor(win / 16));
  const w = Math.floor(W / step), h = Math.floor(H / step), size = Math.max(1, Math.round(win / step));

  // Замкнутые просветы считаются телом: щель между рукой и торсом не должна менять масштаб персонажа
  const empty = new Uint8Array(w * h);
  for (let y = 0; y < h; y++) for (let x = 0; x < w; x++) empty[y * w + x] = px[(y * step * W + x * step) * 4 + 3] < 128 ? 1 : 0;
  const stack = [];
  const reach = (x, y) => {
    if (x < 0 || y < 0 || x >= w || y >= h || empty[y * w + x] !== 1) return;
    empty[y * w + x] = 2;
    stack.push([x, y]);
  };
  for (let x = 0; x < w; x++) { reach(x, 0); reach(x, h - 1); }
  for (let y = 0; y < h; y++) { reach(0, y); reach(w - 1, y); }
  while (stack.length) {
    const [x, y] = stack.pop();
    reach(x - 1, y); reach(x + 1, y); reach(x, y - 1); reach(x, y + 1);
  }

  const sum = new Int32Array((w + 1) * (h + 1));
  for (let y = 0; y < h; y++) for (let x = 0; x < w; x++) {
    const on = empty[y * w + x] === 2 ? 0 : 1;
    sum[(y + 1) * (w + 1) + x + 1] = on + sum[y * (w + 1) + x + 1] + sum[(y + 1) * (w + 1) + x] - sum[y * (w + 1) + x];
  }
  const fits = (x, y) => x >= 0 && y >= 0 && x + size <= w && y + size <= h
    && sum[(y + size) * (w + 1) + x + size] - sum[y * (w + 1) + x + size] - sum[(y + size) * (w + 1) + x] + sum[y * (w + 1) + x] === size * size;

  // Самое большое связное место, куда помещается квадрат: это корпус, а не отставленная рука
  const seen = new Uint8Array(w * h);
  let body = [];
  for (let y = 0; y < h; y++) for (let x = 0; x < w; x++) {
    if (seen[y * w + x] || !fits(x, y)) continue;
    seen[y * w + x] = 1;
    const part = [[x, y]];
    for (let i = 0; i < part.length; i++) {
      const [cx, cy] = part[i];
      for (const [nx, ny] of [[cx - 1, cy], [cx + 1, cy], [cx, cy - 1], [cx, cy + 1]]) {
        if (!fits(nx, ny) || seen[ny * w + nx]) continue;
        seen[ny * w + nx] = 1;
        part.push([nx, ny]);
      }
    }
    if (part.length > body.length) body = part;
  }

  const top = Math.min(...body.map(([, y]) => y));
  const bottom = Math.max(...body.map(([, y]) => y + size));
  const low = bottom - Math.max(1, Math.round((bottom - top) * 0.1));
  const covered = new Uint8Array(w * h);
  let sumX = 0, count = 0, x0 = w, x1 = 0;
  for (const [x, y] of body) {
    for (let j = 0; j < size; j++) for (let i = 0; i < size; i++) covered[(y + j) * w + x + i] = 1;
    if (y + size < low) continue;
    sumX += x + size / 2;
    count++;
    x0 = Math.min(x0, x);
    x1 = Math.max(x1, x + size);
  }
  // Подошва — низ силуэта в колонках стоп: меч ниже ног проходит в стороне от них
  let sole = 0;
  for (let y = 0; y < H; y++) for (let x = x0 * step; x < x1 * step; x++) {
    if (px[(y * W + x) * 4 + 3] >= 128) sole = y + 1;
  }
  return { area: covered.reduce((a, b) => a + b, 0) * step * step, feetX: (sumX / count) * step, feet: sole };
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

// Кадры, где силуэт просел относительно соседних, — порвался контур (см. MIN_AREA_SHARE)
function dropBroken(pool, frames, cut) {
  const areas = pool.map(i => silhouetteArea(cut(frames[i])));
  return pool.filter((_, i) => {
    const near = areas.slice(Math.max(0, i - 5), i + 6).sort((a, b) => a - b);
    return areas[i] >= near[near.length >> 1] * MIN_AREA_SHARE;
  });
}

// Тень на полу из видео (тень под юнитом рисует сама игра): серое, темнее фона, у самых ног, связанное с прозрачным фоном.
// Только на сером фоне; цветная броня и тёмный контур дворфа останавливают заливку
// darkShadow — тень гуще обычной; только для юнитов без серых и потёртых сапог
function eraseGroundShadow(cell, frame, paper, darkShadow) {
  const paperSat = Math.max(...paper) - Math.min(...paper);
  const paperLum = 0.299 * paper[0] + 0.587 * paper[1] + 0.114 * paper[2];
  if (paperSat > 20) return cell;

  const { W, H, px } = cell;
  const d = frame.data;
  const b = bbox(cell);
  const top = Math.floor(b.y1 - (b.y1 - b.y0) * 0.18);
  const isShadow = p => {
    const i = p * 4;
    const sat = Math.max(d[i], d[i + 1], d[i + 2]) - Math.min(d[i], d[i + 1], d[i + 2]);
    const lum = 0.299 * d[i] + 0.587 * d[i + 1] + 0.114 * d[i + 2];
    // На белом фоне тень светлая и бывает с голубым отливом; потёртые сапоги с таким же оттенком намного темнее
    const shadow = paperLum > 230 ? sat <= 34 && lum > (darkShadow ? 90 : 150) : sat <= 14 && lum > 40;
    return px[i + 3] > 0 && shadow && lum < paperLum - 8;
  };
  const stack = [];
  const seen = new Uint8Array(W * H);
  const visit = (x, y) => {
    if (x < 0 || x >= W || y < top || y >= H) return;
    const p = y * W + x;
    if (seen[p] || !isShadow(p)) return;
    seen[p] = 1;
    stack.push(p);
  };
  // Заливка стартует от прозрачных пикселей полосы, то есть снаружи силуэта
  for (let y = top; y < H; y++) for (let x = 0; x < W; x++) {
    if (px[(y * W + x) * 4 + 3] > 0) continue;
    visit(x - 1, y); visit(x + 1, y); visit(x, y - 1); visit(x, y + 1);
  }
  while (stack.length) {
    const p = stack.pop(), x = p % W, y = (p - x) / W;
    px.fill(0, p * 4, p * 4 + 4);
    visit(x - 1, y); visit(x + 1, y); visit(x, y - 1); visit(x, y + 1);
  }
  return cell;
}

// Кадры с фоном, который преобладает в видео: остальные выглядели бы иначе (отсвет другого фона на персонаже).
// Нет явного большинства — берём все: выкинув половину кадров, анимацию не собрать
function dominantBackground(frames) {
  const papers = frames.map(paperColor);
  const close = (a, b) => Math.abs(a[0] - b[0]) + Math.abs(a[1] - b[1]) + Math.abs(a[2] - b[2]) <= 24;
  const support = papers.map(p => papers.filter(q => close(p, q)).length);
  const center = papers[support.indexOf(Math.max(...support))];
  const pool = frames.map((_, i) => i).filter(i => close(papers[i], center));
  return pool.length >= frames.length / 2 ? pool : undefined;
}

/* ===== Main ===== */
function build(video, anim, rest) {
  let frames = readFrames(video.path);
  const byBody = video.unit !== 'dwarf' || (Number(video.lvl) !== LEGACY_LVL && video.color !== 'rainbow');
  const firstPaper = paperColor(frames[0]);
  // С 5-го уровня фон в видео бывает разным от кадра к кадру (серый, красный, зелёный) — цвет берётся по каждому кадру
  const paperOf = frame => (byBody ? paperColor(frame) : firstPaper);
  let replaced = 0;
  if (FIX_CLIPPED.has(video.file)) ({ frames, replaced } = replaceClipped(frames, firstPaper));
  // Светлая броня на сером фоне без светлой обводки становится полупрозрачной, в том числе в атаках
  const lightArmor = LIGHT_ARMOR.has(`lvl${video.lvl}_${video.color}`);
  const lightOutline = NO_EFFECTS.has(anim) || lightArmor;
  const name = path.basename(video.file, '.mp4');
  const attackFix = WHITE_ATTACK_FIX[name];
  const plainCut = frame => {
    const paper = paperOf(frame);
    // У светлой брони часть граней цвета фона: там просветы ищем только строго (в атаках фон у неё темнее брони)
    // На белом фоне у цветного дворфа чисто белых деталей нет: любое закрытое белое пятно — просвет.
    // Кроме белых кристаллов: эти белые пятна — сам персонаж (кости скелета желтоватые, с фоном не путаются)
    const whitePaper = paper.every(v => v > 230);
    const holeEdge = !byBody || (lightArmor && NO_EFFECTS.has(anim)) || (whitePaper && video.color === 'white') ? 0.7 : whitePaper ? 0 : 0.35;
    return cutout(frame, paper, lightOutline, holeEdge, lightArmor && NO_EFFECTS.has(anim));
  };
  const cut = frame => (attackFix
    ? cutout(frame, paperOf(frame), true, 0.7, attackFix.faint, attackFix.lightDelta, attackFix.bigHole)
    : plainCut(frame));
  const eraseShadow = (cell, frame) => eraseGroundShadow(cell, frame, paperOf(frame), video.unit === 'draconid');

  let pool;
  if (byBody && anim === 'victory') {
    pool = dropBroken(frames.map((_, i) => i).filter(i => !touchesEdge(frames[i], paperOf(frames[i]))), frames, cut);
  } else if (byBody && LOOPED.has(anim)) {
    pool = dominantBackground(frames);
  }
  const dropped = pool ? frames.length - pool.length : 0;
  const picks = pickFrames(frames, LOOPED.has(anim), pool);
  const erases = byBody && NO_EFFECTS.has(anim) && !lightArmor;
  const cells = picks.map(i => {
    const cell = cut(frames[i]);
    return erases && !KEEP_FLOOR.has(name) ? eraseShadow(cell, frames[i]) : cell;
  });
  // По этому кадру считаются масштаб и положение. Без стёртой тени остатки тени сдвинули бы центр стоп
  const scaleCell = KEEP_FLOOR.has(name) ? eraseShadow(cut(frames[picks[0]]), frames[picks[0]])
    : attackFix?.keepScale ? plainCut(frames[picks[0]]) : cells[0];
  const first = bbox(scaleCell);
  const anchor = byBody
    ? bodyAnchor(scaleCell)
    : { area: silhouetteArea(scaleCell), feetX: feetX(scaleCell, first), feet: first.y1 };

  // Подгонка считается в пикселях кадра idle (CELL): k — масштаб, (x0, y0) — точка видео в левом верхнем углу кадра idle
  let k, x0, y0;
  if (!rest && anim === 'idle' && !byBody) {
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
    // В атаках — по силуэту с мечом: в начале удара обе руки на торсе, и площадь тела без меча занижена, а персонаж вышел бы крупнее
    const standard = byBody ? Math.sqrt(BODY_AREA / anchor.area) : TARGET_H / (first.y1 - first.y0);
    const toRest = rest && (anim === 'attack' ? Math.sqrt(rest.silhouette / silhouetteArea(scaleCell)) : Math.sqrt(rest.area / anchor.area));
    k = (rest ? toRest : standard) * (SCALE_FIX[path.basename(video.file, '.mp4')] ?? 1);
    const pose = rest ?? { feetX: byBody ? BODY_FEET_X : CENTER_X, feet: FEET_Y };
    x0 = anchor.feetX - pose.feetX / k;
    y0 = anchor.feet - pose.feet / k;
  }
  // В пикселях кадра idle
  const restPose = { area: anchor.area * k * k, silhouette: silhouetteArea(scaleCell) * k * k, feetX: (anchor.feetX - x0) * k, feet: (anchor.feet - y0) * k };

  // Кадр с полем field > 1 охватывает в field раз большую область вокруг того же центра, что и кадр idle
  const place = (field, cell) => {
    const shift = (CELL * field - CELL) / 2;
    return { k: (k * cell) / (CELL * field), ox: x0 - shift / k, oy: y0 - shift / k };
  };
  // Прыжок или шлейф не влез — поле расширяется, а не обрезается (мелкие искры у края не в счёт).
  // idle задаёт кадр и не расширяется
  let field = FIELD[anim] ?? 1;
  const overflow = f => Math.max(...Object.values(clipping(cells, place(f, CELL), CELL)));
  const tolerance = NO_EFFECTS.has(anim) ? 1 : CELL * CLIP_TOLERANCE; // без эффектов за край мог бы уйти только меч
  while (anim !== 'idle' && field < MAX_FIELD && overflow(field) > tolerance) field += 0.25;
  // Расширенный кадр — на листе покрупнее, чтобы персонаж не терял в резкости
  const size = SHEET_PX[anim] ?? (field > 1 ? 3072 : SHEET);
  const cell = size / COLS;
  const transform = place(field, cell);
  return {
    png: render(cells, transform, size),
    size,
    field,
    restPose,
    info: `${frames.length} кадров${replaced ? ` (${replaced} с мечом за краем видео заменено соседними)` : ''}${dropped ? ` (${dropped} с обрезанным мечом, порванным контуром или другим фоном пропущено)` : ''} -> [${picks.join(',')}], лист ${size}px, кадр x${field}`,
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

const args = process.argv.slice(2);
const noAtlas = args.includes('--no-atlas');
const filter = args.find(arg => !arg.startsWith('--')) ?? '';
const meta = fs.existsSync(META) ? JSON.parse(fs.readFileSync(META, 'utf8')) : {};
const atlasMeta = fs.existsSync(ATLAS_META) ? JSON.parse(fs.readFileSync(ATLAS_META, 'utf8')) : {};
const groups = new Map();
const listVideos = dir => fs.readdirSync(dir, { withFileTypes: true }).flatMap(entry => (
  entry.isDirectory() ? listVideos(path.join(dir, entry.name)) : [{ file: entry.name, path: path.join(dir, entry.name) }]
));
listVideos(VIDEOS).forEach(({ file, path: videoPath }) => {
  const m = VIDEO_FILE.exec(file);
  if (!m) return;
  const [, unit, anim, lvl, color, variant = '1'] = m;
  const key = `${unit}_lvl${lvl}_${color}`;
  if (!groups.has(key)) groups.set(key, []);
  groups.get(key).push({ file, path: videoPath, unit, anim, lvl, color, variant });
});

for (const [key, videos] of groups) {
  if (!videos.some(v => v.file.includes(filter))) continue;
  // idle первым: он задаёт позу покоя для остальных анимаций
  videos.sort((a, b) => (b.anim === 'idle') - (a.anim === 'idle') || a.anim.localeCompare(b.anim) || a.variant - b.variant);
  if (videos[0].anim !== 'idle') console.warn(`${key}: нет idle-видео, размер подгоняется по каждой анимации отдельно`);

  let rest = null;
  for (const v of videos) {
    // Пересобираются только подходящие под фильтр; idle всё равно считается — он задаёт позу покоя
    const selected = v.file.includes(filter);
    if (!selected && v.anim !== 'idle') continue;
    const res = build(v, v.anim, rest);
    if (v.anim === 'idle' && !rest) rest = res.restPose;
    if (!selected) continue;
    const dir = path.join(SPRITES, `${v.unit}-${v.anim}`);
    fs.mkdirSync(dir, { recursive: true });
    const name = `${v.unit}_${v.anim}_lvl${v.lvl}_${v.color}_${v.variant}.png`;
    const target = path.join(dir, name);
    // Листа нет, а атлас есть: листы лежат вне проекта. Новый лист из видео заменил бы атлас, собранный из апскейла.
    // Чтобы пересобрать, верните лист в sprite-lists или удалите его атлас из atlas-meta.json
    if (!fs.existsSync(target) && atlasMeta[name]) {
      console.log(`${v.file} -> ${name}  пропущен: есть атлас, а листа нет`);
      continue;
    }
    // Лист крупнее собранного — апскейл, сделанный вручную: не затираем его (поза покоя всё равно берётся из видео).
    // Апскейл должен быть ровно в N раз больше листа скрипта, иначе персонаж сдвинется относительно других анимаций
    // Не кратный (3072 при 2048) — прошлая сборка этого же листа с расширенным полем, а не апскейл
    if (fs.existsSync(target) && pngWidth(target) > res.size && pngWidth(target) % res.size === 0) {
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
if (!noAtlas) execFileSync(process.execPath, [path.join(__dirname, 'sprite-atlas.js'), filter], { stdio: 'inherit' });
