// Иконки героев из портретов: npm run portraits [-- часть_имени]
//
// Портрет: src/assets/portraits/{цвет}_lvl{N}.jpg  (дворф в полный рост на однотонном фоне)
// Иконка:  src/assets/portraits/icons/{цвет}_lvl{N}.png  (голова крупным планом, фон прозрачный, ICON x ICON)
//
// Цвет — как в именах спрайт-листов (см. battle/sprites.ts), иконку по нему выбирает UnitAvatar.
// Сами портреты ~2000px и по 1.5 МБ: в кружке на 40px браузер распаковывал бы и ужимал их при каждой перерисовке
const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');
const { PNG } = require('pngjs');
const ffmpeg = require('ffmpeg-static');
const { paperColor, cutout } = require('./cutout');

const PORTRAITS = path.join(__dirname, '..', 'src', 'assets', 'portraits');
const ICONS = path.join(PORTRAITS, 'icons');

// Сторона иконки: самый крупный аватар 64px на экране с плотностью 3
const ICON = 192;
// Квадрат вокруг головы: центр в долях ширины/высоты портрета, сторона в долях ширины
const CROP = { x: 0.56, y: 0.374, size: 0.55 };
// Портреты с другой компоновкой
const CROP_OVERRIDES = {
  'white_lvl4.jpg': { x: 0.63, y: 0.36, size: 0.52 },
};

const PORTRAIT_FILE = /^([a-z]+)_lvl(\d+)\.jpg$/;

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

const filter = process.argv[2] ?? '';
fs.mkdirSync(ICONS, { recursive: true });

for (const file of fs.readdirSync(PORTRAITS).sort()) {
  const match = PORTRAIT_FILE.exec(file);
  if (!match || !file.includes(filter)) continue;

  const png = readImage(path.join(PORTRAITS, file));
  // Эффектов на портрете нет: контуром считается и светлая обводка (грани кристаллов, золото)
  const icon = renderIcon(cutout(png, paperColor(png), true), CROP_OVERRIDES[file] ?? CROP);
  const name = `${match[1]}_lvl${match[2]}.png`;
  fs.writeFileSync(path.join(ICONS, name), icon);
  console.log(`${file} -> icons/${name}  (${Math.round(icon.length / 1024)} КБ)`);
}
