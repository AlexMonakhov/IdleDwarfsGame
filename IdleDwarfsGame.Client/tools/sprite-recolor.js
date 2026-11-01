// Остальные цвета из синего листа: npm run sprites:recolor -- юнит [юнит ...] [--only цвет[,цвет]] [--filter часть_имени]
//
// Лист:   sprite-lists/{юнит}-{анимация}/{юнит}_{анимация}_lvl{N}_blue_{вариант}.png
// Новые:  тот же путь с {цвет} вместо blue. Множитель кадра (zoom) копируется в sprite-meta.json.
//
// Перекрашиваются только синие пиксели: кожа, кожаная броня и контур других оттенков остаются как есть.
// Яркость и оттенки внутри синего сохраняются, поэтому блики, тени и шлейфы атак выглядят так же.
// Атласы не собираются: после проверки — npm run atlas
// Синие листы должны лежать в sprite-lists: если они вынесены из проекта, сначала верните их
const fs = require('fs');
const path = require('path');
const { PNG } = require('pngjs');

const SPRITES = path.join(__dirname, '..', 'src', 'assets', 'sprite-lists');
const META = path.join(SPRITES, 'sprite-meta.json');

// Что считается синим: полностью — в HUE_FULL градусах от центра, дальше плавно до HUE_FADE
const BLUE_HUE = 215;
const HUE_FULL = 38;
const HUE_FADE = 60;
// Почти серое не красим: у него оттенок случайный
const SAT_FROM = 0.08;
const SAT_FULL = 0.22;
// Насколько сохраняется разброс оттенков внутри синего (голубые блики, тёмно-синие тени)
const HUE_SPREAD = 0.4;

// hue — новый оттенок; sat — множитель насыщенности; gamma < 1 осветляет, value — множитель яркости
const COLORS = {
  red: { hue: 2, sat: 1.05, gamma: 1, value: 1 },
  green: { hue: 135, sat: 1, gamma: 1, value: 0.92 },
  yellow: { hue: 48, sat: 1.4, gamma: 0.62, value: 1.1 },
  purple: { hue: 275, sat: 1, gamma: 1.05, value: 0.92 },
  white: { hue: 215, sat: 0.1, gamma: 0.62, value: 1.08 },
  basic: { hue: 215, sat: 0.08, gamma: 1, value: 0.82 },
};

const smooth = (from, to, v) => {
  const t = Math.min(1, Math.max(0, (v - from) / (to - from)));
  return t * t * (3 - 2 * t);
};

function recolor(src, target) {
  const out = new PNG({ width: src.width, height: src.height });
  const d = src.data, o = out.data;
  for (let i = 0; i < d.length; i += 4) {
    const r = d[i], g = d[i + 1], b = d[i + 2], a = d[i + 3];
    o[i] = r; o[i + 1] = g; o[i + 2] = b; o[i + 3] = a;
    if (!a) continue;

    const max = Math.max(r, g, b), min = Math.min(r, g, b), delta = max - min;
    if (!delta) continue;
    const sat = delta / max;
    let hue = max === r ? ((g - b) / delta) % 6 : max === g ? (b - r) / delta + 2 : (r - g) / delta + 4;
    hue = (hue * 60 + 360) % 360;
    const off = hue - BLUE_HUE;
    const weight = (1 - smooth(HUE_FULL, HUE_FADE, Math.abs(off))) * smooth(SAT_FROM, SAT_FULL, sat);
    if (weight <= 0) continue;

    // Новый цвет из HSV
    const h = (((target.hue + off * HUE_SPREAD) % 360) + 360) % 360;
    const s = Math.min(1, sat * target.sat);
    const v = Math.min(1, (max / 255) ** target.gamma * target.value);
    const c = v * s, x = c * (1 - Math.abs(((h / 60) % 2) - 1)), m = v - c;
    const [nr, ng, nb] = h < 60 ? [c, x, 0] : h < 120 ? [x, c, 0] : h < 180 ? [0, c, x] : h < 240 ? [0, x, c] : h < 300 ? [x, 0, c] : [c, 0, x];
    o[i] = r + ((nr + m) * 255 - r) * weight;
    o[i + 1] = g + ((ng + m) * 255 - g) * weight;
    o[i + 2] = b + ((nb + m) * 255 - b) * weight;
  }
  return PNG.sync.write(out);
}

const args = process.argv.slice(2);
const option = name => {
  const at = args.indexOf(`--${name}`);
  return at < 0 ? null : args.splice(at, 2)[1];
};
const only = option('only')?.split(',') ?? Object.keys(COLORS);
const filter = option('filter') ?? '';
const meta = fs.existsSync(META) ? JSON.parse(fs.readFileSync(META, 'utf8')) : {};

for (const unit of args) {
  const dirs = fs.readdirSync(SPRITES, { withFileTypes: true })
    .filter(entry => entry.isDirectory() && entry.name.startsWith(`${unit}-`))
    .map(entry => path.join(SPRITES, entry.name));
  for (const dir of dirs) {
    for (const name of fs.readdirSync(dir).filter(file => /_blue_\d+\.png$/.test(file) && file.includes(filter))) {
      const src = PNG.sync.read(fs.readFileSync(path.join(dir, name)));
      for (const color of only) {
        const target = name.replace('_blue_', `_${color}_`);
        fs.writeFileSync(path.join(dir, target), recolor(src, COLORS[color]));
        if (meta[name]) meta[target] = meta[name];
        else delete meta[target];
        console.log(`${name} -> ${target}`);
      }
    }
  }
}

const sorted = Object.fromEntries(Object.keys(meta).sort().map(key => [key, meta[key]]));
fs.writeFileSync(META, JSON.stringify(sorted, null, 2) + '\n');
