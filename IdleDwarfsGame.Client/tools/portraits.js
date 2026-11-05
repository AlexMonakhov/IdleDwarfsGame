// Иконки героев из портретов: npm run portraits [-- часть_имени]
//
// Портрет: src/assets/portraits/{цвет}_lvl{N}.jpg  (дворф в полный рост на однотонном фоне)
// Иконка:  src/assets/portraits/icons/{цвет}_lvl{N}.png  (голова крупным планом, фон прозрачный, ICON x ICON)
//
// Цвет — как в именах спрайт-листов (см. battle/sprites.ts), иконку по нему выбирает UnitAvatar.
// Сами портреты ~2000px и по 1.5 МБ: в кружке на 40px браузер распаковывал бы и ужимал их при каждой перерисовке.
//
// Рангам и цветам, у которых портрета-иллюстрации нет, иконка вырезается из спрайта: первый кадр стоячей анимации
// дворфа (атлас sprite-atlas/dwarf_idle_lvl{N}_{цвет}_1.webp). Портрет-иллюстрация, если есть, важнее спрайта.
// Мобы (драконид, гоблин, голем, скелет) — тоже из стоячего кадра: icons/{вид}_{цвет}.png, ранг у них один
const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');
const { PNG } = require('pngjs');
const ffmpeg = require('ffmpeg-static');
const { paperColor, cutout } = require('./cutout');

const PORTRAITS = path.join(__dirname, '..', 'src', 'assets', 'portraits');
const ICONS = path.join(PORTRAITS, 'icons');
const ATLAS = path.join(__dirname, '..', 'src', 'assets', 'sprite-atlas');

// Сторона иконки: самый крупный аватар 64px на экране с плотностью 3
const ICON = 192;
// Квадрат вокруг головы: центр в долях ширины/высоты портрета, сторона в долях ширины
const CROP = { x: 0.56, y: 0.374, size: 0.55 };
// Портреты с другой компоновкой
const CROP_OVERRIDES = {
  'white_lvl3.jpg': { x: 0.63, y: 0.36, size: 0.52 },
};

const PORTRAIT_FILE = /^([a-z]+)_lvl(\d+)\.jpg$/;

// Иконки из спрайтов: квадрат вокруг головы в долях клетки листа (центр и сторона) — как у иллюстраций:
// макушка у верхнего края, глаза чуть выше середины. Дворфы разных рангов разного роста, поэтому кадрирование по рангу
const SPRITE_CROPS = {
  1: { x: 0.575, y: 0.41, size: 0.26 },
  2: { x: 0.555, y: 0.437, size: 0.26 },
  // Высокая кристальная корона: квадрат крупнее, чтобы зубцы не срезались
  4: { x: 0.56, y: 0.455, size: 0.33 },
};
const SPRITE_CROP = SPRITE_CROPS[2];
// Мобы: голова у каждого вида своя — голем ниже и левее, у остальных голова над правым плечом, как у дворфа
const MONSTER_CROPS = {
  draconid: { x: 0.51, y: 0.42, size: 0.27 },
  goblin: { x: 0.565, y: 0.4, size: 0.25 },
  golem: { x: 0.43, y: 0.45, size: 0.28 },
  skeleton: { x: 0.575, y: 0.395, size: 0.25 },
};
const IDLE_ATLAS = /^([a-z]+)_idle_lvl(\d+)_([a-z]+)_1\.png$/;
// Цвет иконки по цвету спрайта — как PORTRAIT_COLORS в battle/sprites.ts: базовые герои серые, тьма — dark, кровь — purple
const PORTRAIT_COLORS = { basic: 'gray', purple: 'dark', violet: 'purple' };

const readImage = file =>
  PNG.sync.read(execFileSync(ffmpeg, ['-v', 'error', '-i', file, '-f', 'image2pipe', '-c:v', 'png', '-'], { maxBuffer: 2 ** 30 }));

// Квадрат crop, уменьшенный до ICON: каждый пиксель иконки — среднее k x k точек портрета.
// Цвет вырезки премультиплицирован, поэтому края усредняются без светлой каймы
function renderIcon({ W, H, px }, crop) {
  const side = Math.min(crop.size * W, W, H);
  const left = Math.min(Math.max(crop.x * W - side / 2, 0), W - side);
  const top = Math.min(Math.max(crop.y * H - side / 2, 0), H - side);
  const step = side / ICON;
  const k = Math.max(1, Math.ceil(step));
  const icon = new PNG({ width: ICON, height: ICON });
  const acc = [0, 0, 0, 0];

  for (let Y = 0; Y < ICON; Y++) for (let X = 0; X < ICON; X++) {
    acc[0] = acc[1] = acc[2] = acc[3] = 0;
    for (let j = 0; j < k; j++) for (let i = 0; i < k; i++) {
      const p = (Math.floor(top + (Y + (j + 0.5) / k) * step) * W + Math.floor(left + (X + (i + 0.5) / k) * step)) * 4;
      for (let c = 0; c < 4; c++) acc[c] += px[p + c];
    }
    if (acc[3] <= 0) continue;
    const o = (Y * ICON + X) * 4;
    for (let c = 0; c < 3; c++) icon.data[o + c] = Math.min(255, Math.round((acc[c] / acc[3]) * 255));
    icon.data[o + 3] = Math.round(acc[3] / (k * k));
  }
  return PNG.sync.write(icon);
}

// Первый кадр атласа в клетке листа на своём месте, цвет премультиплицирован — как у cutout, для renderIcon
function idleCell(atlasName, meta) {
  const atlas = readImage(path.join(ATLAS, atlasName.replace(/\.png$/, '.webp')));
  const [ax, ay, w, h, dx, dy] = meta.frames.find(Boolean);
  const W = Math.round(meta.cell);
  const px = new Float32Array(W * W * 4);
  const ox = Math.round(dx), oy = Math.round(dy);
  for (let y = 0; y < h; y++) for (let x = 0; x < w; x++) {
    const cx = ox + x, cy = oy + y;
    if (cx < 0 || cy < 0 || cx >= W || cy >= W) continue;
    const s = ((ay + y) * atlas.width + ax + x) * 4;
    const t = (cy * W + cx) * 4;
    const a = atlas.data[s + 3] / 255;
    for (let c = 0; c < 3; c++) px[t + c] = atlas.data[s + c] * a;
    px[t + 3] = atlas.data[s + 3];
  }
  return { W, H: W, px };
}

const filter = process.argv[2] ?? '';
fs.mkdirSync(ICONS, { recursive: true });
const illustrated = new Set();

for (const file of fs.readdirSync(PORTRAITS).sort()) {
  const match = PORTRAIT_FILE.exec(file);
  if (!match) continue;
  illustrated.add(`${match[1]}_lvl${match[2]}`);
  if (!file.includes(filter)) continue;

  const png = readImage(path.join(PORTRAITS, file));
  // Эффектов на портрете нет: контуром считается и светлая обводка (грани кристаллов, золото)
  const icon = renderIcon(cutout(png, paperColor(png), true), CROP_OVERRIDES[file] ?? CROP);
  const name = `${match[1]}_lvl${match[2]}.png`;
  fs.writeFileSync(path.join(ICONS, name), icon);
  console.log(`${file} -> icons/${name}  (${Math.round(icon.length / 1024)} КБ)`);
}

// Остальные ранги и цвета — из спрайта
const atlasMeta = JSON.parse(fs.readFileSync(path.join(ATLAS, 'atlas-meta.json'), 'utf8'));
for (const atlasName of Object.keys(atlasMeta).sort()) {
  const match = IDLE_ATLAS.exec(atlasName);
  if (!match) continue;
  const [, unit, level, color] = match;
  let name, crop;
  if (unit === 'dwarf') {
    name = `${PORTRAIT_COLORS[color] ?? color}_lvl${level}`;
    crop = SPRITE_CROPS[level] ?? SPRITE_CROP;
  } else if (MONSTER_CROPS[unit]) {
    name = `${unit}_${color}`;
    crop = MONSTER_CROPS[unit];
  } else {
    continue;
  }
  if (illustrated.has(name) || !`${atlasName} ${name}`.includes(filter)) continue;

  const icon = renderIcon(idleCell(atlasName, atlasMeta[atlasName]), crop);
  fs.writeFileSync(path.join(ICONS, `${name}.png`), icon);
  console.log(`${atlasName} -> icons/${name}.png  (${Math.round(icon.length / 1024)} КБ, из спрайта)`);
}
