// Атласы из спрайт-листов: npm run atlas [-- часть_имени]   (npm run sprites в конце запускает и его).
// Лист, увеличенный вручную, — тоже npm run atlas, иначе игра покажет старый атлас
//
// Лист:   src/assets/sprite-lists/**/{юнит}_{анимация}_lvl{N}_{цвет}_{вариант}.png  (сетка 5x5, см. battle/sprites.ts)
// Атлас:  src/assets/sprite-atlas/{то же имя}.webp + запись в sprite-atlas/atlas-meta.json
//
// В листе персонаж занимает малую часть кадра (у атак с zoom ~15%): остальное — прозрачное поле под прыжки и шлейфы.
// Игра держит лист распакованным в размере экрана, и это поле занимает память и видеокарту впустую.
// В атласе каждый кадр обрезан по непрозрачным пикселям, одинаковые кадры хранятся один раз, всё плотно уложено;
// в meta для каждого кадра — где он в атласе и где лежал в своей клетке листа. Разрешение то же, что у листа.
// WebP без потерь: те же пиксели, файл в несколько раз меньше PNG.
// Лист пересобирается, только если он новее своего атласа
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const { execFileSync } = require('child_process');
const { PNG } = require('pngjs');
const ffmpeg = require('ffmpeg-static');

const ROOT = path.join(__dirname, '..', 'src', 'assets');
const SPRITES = path.join(ROOT, 'sprite-lists');
const ATLAS = path.join(ROOT, 'sprite-atlas');
const META = path.join(ATLAS, 'atlas-meta.json');

const SPRITE_FILE = /^[a-z]+_[a-z]+_lvl\d+_[a-z]+_\d+\.png$/;
const COLS = 5;
// Прозрачный зазор вокруг кадра: игра уменьшает атлас до 4 раз, и при сглаживании соседний кадр не должен просвечивать
const PAD = 8;
// Пиксели прозрачнее этого считаются пустыми (шум сжатия на краю свечения)
const MIN_ALPHA = 2;

const listSheets = dir => fs.readdirSync(dir, { withFileTypes: true }).flatMap(entry => {
  const full = path.join(dir, entry.name);
  if (entry.isDirectory()) return listSheets(full);
  return SPRITE_FILE.test(entry.name) ? [full] : [];
});

// Непрозрачная часть клетки [x0, x1) x [y0, y1); null — клетка пустая
function trim({ width: W, data }, x0, y0, x1, y1) {
  let minX = x1, minY = y1, maxX = -1, maxY = -1;
  for (let y = y0; y < y1; y++) for (let x = x0; x < x1; x++) {
    if (data[(y * W + x) * 4 + 3] < MIN_ALPHA) continue;
    if (x < minX) minX = x;
    if (x > maxX) maxX = x;
    if (y < minY) minY = y;
    if (y > maxY) maxY = y;
  }
  return maxX < 0 ? null : { x: minX, y: minY, w: maxX - minX + 1, h: maxY - minY + 1 };
}

function copyRect(src, sx, sy, w, h, dst, dx, dy) {
  for (let y = 0; y < h; y++) {
    const from = ((sy + y) * src.width + sx) * 4;
    src.data.copy(dst.data, ((dy + y) * dst.width + dx) * 4, from, from + w * 4);
  }
}

function hashRect(png, { x, y, w, h }) {
  const hash = crypto.createHash('md5').update(`${w}x${h}`);
  for (let row = 0; row < h; row++) {
    const from = ((y + row) * png.width + x) * 4;
    hash.update(png.data.subarray(from, from + w * 4));
  }
  return hash.digest('hex');
}

// Полки: кадры по убыванию высоты слева направо, ширина атласа — под квадрат
function pack(rects) {
  const area = rects.reduce((sum, r) => sum + (r.w + PAD) * (r.h + PAD), 0);
  const width = Math.max(Math.ceil(Math.sqrt(area * 1.1)), ...rects.map(r => r.w + PAD * 2));
  let x = PAD, y = PAD, shelf = 0;
  for (const r of [...rects].sort((a, b) => b.h - a.h)) {
    if (x + r.w + PAD > width) {
      x = PAD;
      y += shelf + PAD;
      shelf = 0;
    }
    r.ax = x;
    r.ay = y;
    x += r.w + PAD;
    shelf = Math.max(shelf, r.h);
  }
  return { width, height: y + shelf + PAD };
}

const round2 = v => Math.round(v * 100) / 100;

function build(file) {
  const sheet = PNG.sync.read(fs.readFileSync(file));
  const cell = sheet.width / COLS;
  const rows = Math.round(sheet.height / cell);

  const unique = new Map();
  const frames = [];
  for (let i = 0; i < COLS * rows; i++) {
    const col = i % COLS, row = Math.floor(i / COLS);
    const rect = trim(sheet, Math.round(col * cell), Math.round(row * cell), Math.round((col + 1) * cell), Math.round((row + 1) * cell));
    if (!rect) {
      frames.push(null);
      continue;
    }
    const key = hashRect(sheet, rect);
    if (!unique.has(key)) unique.set(key, rect);
    // Смещение кадра от угла его клетки в листе
    frames.push({ rect: unique.get(key), dx: round2(rect.x - col * cell), dy: round2(rect.y - row * cell) });
  }

  const rects = [...unique.values()];
  const { width, height } = pack(rects);
  const atlas = new PNG({ width, height });
  rects.forEach(r => copyRect(sheet, r.x, r.y, r.w, r.h, atlas, r.ax, r.ay));

  const target = path.join(ATLAS, path.basename(file, '.png') + '.webp');
  execFileSync(ffmpeg, [
    '-v', 'error', '-y',
    '-f', 'rawvideo', '-pix_fmt', 'rgba', '-s', `${width}x${height}`, '-i', '-',
    '-c:v', 'libwebp', '-lossless', '1', '-compression_level', '6', '-frames:v', '1', target,
  ], { input: atlas.data, maxBuffer: 2 ** 30 });

  return {
    meta: {
      cell: round2(cell),
      width,
      height,
      // [x, y, ширина, высота в атласе, смещение x, y в клетке] или null — пустой кадр
      frames: frames.map(f => (f ? [f.rect.ax, f.rect.ay, f.rect.w, f.rect.h, f.dx, f.dy] : null)),
    },
    info: `${sheet.width}px ${fs.statSync(file).size >> 10} КБ -> ${width}x${height} ${fs.statSync(target).size >> 10} КБ, `
      + `пикселей ${Math.round((100 * width * height) / (sheet.width * sheet.height))}%, кадров ${rects.length}/${frames.filter(Boolean).length}`,
  };
}

const filter = process.argv[2] ?? '';
fs.mkdirSync(ATLAS, { recursive: true });
const meta = fs.existsSync(META) ? JSON.parse(fs.readFileSync(META, 'utf8')) : {};
const sheets = listSheets(SPRITES);
const names = new Set(sheets.map(file => path.basename(file)));

// Лист удалён — удаляем и его атлас
for (const name of Object.keys(meta)) {
  if (names.has(name)) continue;
  delete meta[name];
  fs.rmSync(path.join(ATLAS, path.basename(name, '.png') + '.webp'), { force: true });
  console.log(`${name}: листа нет, атлас удалён`);
}

for (const file of sheets) {
  const name = path.basename(file);
  if (!name.includes(filter)) continue;
  const target = path.join(ATLAS, path.basename(name, '.png') + '.webp');
  if (meta[name] && fs.existsSync(target) && fs.statSync(target).mtimeMs >= fs.statSync(file).mtimeMs) continue;

  const { meta: entry, info } = build(file);
  meta[name] = entry;
  console.log(`${name}: ${info}`);
  // Пишем после каждого листа: прерванная сборка не теряет готовое
  const sorted = Object.fromEntries(Object.keys(meta).sort().map(key => [key, meta[key]]));
  fs.writeFileSync(META, JSON.stringify(sorted) + '\n');
}
