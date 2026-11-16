// Замер производительности арены в настоящем Chrome (нагрузочный тест /arena-stress) на релизной сборке:
// dev-сборка (Vue в режиме разработки, горячая перезагрузка) для замеров не годится.
//
//   npm run perf:build                       # собрать клиент в dist-perf
//   npm run perf -- --label pixi             # результат в tools/perf/results/pixi.json + таблица в консоль
//
// Параметры: --seconds (на сценарий, 6), --label, --mode current|canvas (canvas — ?fx=canvas: был до перехода на Pixi,
// база снята им — docs/perf-canvas-baseline.md; сейчас параметр ни на что не влияет),
// --url (своя сборка где-то ещё; иначе скрипт сам раздаёт сборку), --dist (папка сборки, по умолчанию dist-perf),
// CHROME_PATH — путь к Chrome.
// Окно 1440×900 при DPR 2 (ретина). Кадры идут с частотой экрана, как у игрока: смотрим на пропущенные кадры
// («Кадров > 20 мс» при 60 Гц) и на занятость главного потока (мс работы за секунду).
// --uncapped снимает ограничение частоты (vsync выключен). Годится только для оценки запаса по JS: кадры при этом
// на экран не выводятся (картинка в окне стоит), а WebGL упирается в очередь видеокарты — цифры с ним не сравнивать.
// Сравнивать результаты только с одной машины.
const fs = require('fs');
const http = require('http');
const path = require('path');
const puppeteer = require('puppeteer-core');

const arg = (name, fallback) => {
  const i = process.argv.indexOf(`--${name}`);
  return i > 0 ? process.argv[i + 1] : fallback;
};

const DIST = path.resolve(arg('dist', path.join(__dirname, '../../dist-perf')));
const UNCAPPED = process.argv.includes('--uncapped');
// --only "Fireball,Залп" — только сценарии с такими подстроками в названии; --eval "<js>" — выполнить в странице до замеров
// (например "__pixiFx.unitGlow = false" или "__pixiFx.skip = new Set(['aura-sphere'])")
const ONLY = arg('only', '').split(',').filter(Boolean);
const EVAL = arg('eval', '');
const PORT = 8090;
const URL = arg('url', `http://localhost:${PORT}`);
const SECONDS = Number(arg('seconds', 6));
const LABEL = arg('label', 'current');
const MODE = arg('mode', 'current');
const CHROME = process.env.CHROME_PATH ?? '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome';

// Каждое заклинание кастует первый боец, у которого оно есть, уровень 3, раз в INTERVAL мс
const SPELLS = ['Fireball', 'Meteor', 'Flame Wave', 'Blizzard', 'Glacial Prison', 'Chain Lightning', 'Thunderclap', 'Healing Rune', 'Soul Harvest', 'Dawnlight'];
const INTERVAL = 350;

const button = (page, text) => page.evaluate(text =>
  [...document.querySelectorAll('.stress-controls button')].find(b => b.textContent.trim() === text)?.click(), text);

// Кадры за seconds, пока action(запуск) крутится в странице
const measure = (page, seconds, action) => page.evaluate(({ seconds, action, interval }) => new Promise(resolve => {
  const castSpell = name => {
    const spell = [...document.querySelectorAll('.stress-spell')].find(el => el.querySelector('span')?.textContent.trim() === name);
    [...(spell?.querySelectorAll('button') ?? [])].find(b => b.textContent.trim() === '3' && !b.disabled)?.click();
  };
  // Залп: каждый боец — своё первое заклинание
  const volley = () => {
    for (const row of document.querySelectorAll('.stress-spells-row')) {
      [...(row.querySelector('.stress-spell')?.querySelectorAll('button') ?? [])].find(b => b.textContent.trim() === '3' && !b.disabled)?.click();
    }
  };
  let timer = 0;
  if (action.kind === 'spell') {
    castSpell(action.name);
    timer = setInterval(() => castSpell(action.name), interval);
  } else if (action.kind === 'volley') {
    volley();
    timer = setInterval(volley, 1000);
  }

  const frames = [];
  let last = performance.now();
  const end = last + seconds * 1000;
  const tick = now => {
    frames.push(now - last);
    last = now;
    if (now < end) {
      requestAnimationFrame(tick);
      return;
    }
    clearInterval(timer);
    const sorted = [...frames].sort((a, b) => a - b);
    const pick = q => +sorted[Math.min(sorted.length - 1, Math.floor(sorted.length * q))].toFixed(1);
    resolve({
      fps: Math.round(frames.length / seconds),
      p95: pick(0.95),
      p99: pick(0.99),
      worst: Math.round(sorted.at(-1)),
      missedShare: +(frames.filter(f => f > 20).length / frames.length * 100).toFixed(1),
      slowShare: +(frames.filter(f => f > 33.4).length / frames.length * 100).toFixed(1),
    });
  };
  requestAnimationFrame(tick);
}), { seconds, action, interval: INTERVAL });

// Статика сборки; неизвестный путь (маршрут SPA) — index.html
const TYPES = { '.html': 'text/html', '.js': 'text/javascript', '.css': 'text/css', '.png': 'image/png', '.webp': 'image/webp',
  '.svg': 'image/svg+xml', '.json': 'application/json', '.mp3': 'audio/mpeg', '.woff2': 'font/woff2', '.jpg': 'image/jpeg' };
const serve = () => http.createServer((req, res) => {
  const clean = decodeURIComponent(req.url.split('?')[0]);
  let file = path.join(DIST, clean);
  if (!file.startsWith(DIST) || !fs.existsSync(file) || fs.statSync(file).isDirectory()) file = path.join(DIST, 'index.html');
  res.writeHead(200, { 'Content-Type': TYPES[path.extname(file)] ?? 'application/octet-stream' });
  fs.createReadStream(file).pipe(res);
}).listen(PORT);

(async () => {
  let server = null;
  if (!process.argv.includes('--url')) {
    if (!fs.existsSync(path.join(DIST, 'index.html'))) throw new Error('Нет сборки: сначала npm run perf:build');
    server = serve();
  }
  const browser = await puppeteer.launch({
    executablePath: CHROME,
    headless: false,
    args: ['--enable-gpu', '--ignore-gpu-blocklist', '--use-angle=metal', '--window-size=1440,900',
      ...(UNCAPPED ? ['--disable-frame-rate-limit', '--disable-gpu-vsync'] : [])],
    defaultViewport: { width: 1440, height: 900, deviceScaleFactor: 2 },
  });
  const page = await browser.newPage();
  // load, а не networkidle: dev-сервер держит WebSocket горячей перезагрузки
  await page.goto(`${URL}/arena-stress${MODE === 'canvas' ? '?fx=canvas' : ''}`, { waitUntil: 'load', timeout: 60000 });
  await new Promise(r => setTimeout(r, 6000));

  const cdp = await page.createCDPSession();
  await cdp.send('Performance.enable');
  // Занятость главного потока (скрипты, стили, раскладка, отрисовка), секунды с начала
  const busy = async () => (await cdp.send('Performance.getMetrics')).metrics.find(m => m.name === 'TaskDuration').value;

  const gpu = await page.evaluate(() => {
    const gl = document.createElement('canvas').getContext('webgl2');
    const info = gl?.getExtension('WEBGL_debug_renderer_info');
    return info ? gl.getParameter(info.UNMASKED_RENDERER_WEBGL) : 'unknown';
  });

  const scenarios = [
    { name: 'Автобой', setup: 'auto', action: { kind: 'none' } },
    { name: 'Простой (без кастов)', setup: 'manual', action: { kind: 'none' } },
    ...SPELLS.map(name => ({ name, setup: 'manual', action: { kind: 'spell', name } })),
    { name: 'Залп всех 10 бойцов', setup: 'manual', action: { kind: 'volley' } },
  ].filter(s => !ONLY.length || ONLY.some(part => s.name.includes(part)));
  if (EVAL) await page.evaluate(EVAL);

  let manual = false;
  const results = [];
  for (const scenario of scenarios) {
    const wantManual = scenario.setup === 'manual';
    if (wantManual !== manual) {
      await button(page, manual ? 'Ручной режим' : 'Автобой');
      manual = wantManual;
    }
    // Каждый сценарий — с чистой сцены: статусы прошлых заклинаний копятся на юнитах
    await button(page, 'Снять статусы');
    await new Promise(r => setTimeout(r, 1500));
    const before = await busy();
    const result = await measure(page, SECONDS, scenario.action);
    result.busyMs = Math.round((await busy() - before) * 1000 / SECONDS);
    results.push({ scenario: scenario.name, ...result });
    console.log(`${scenario.name.padEnd(22)} ${JSON.stringify(result)}`);
  }
  await browser.close();
  server?.close();

  const out = path.join(__dirname, 'results');
  fs.mkdirSync(out, { recursive: true });
  const report = { label: LABEL, mode: MODE, uncapped: UNCAPPED, date: new Date().toISOString(), gpu, viewport: '1440x900@2', seconds: SECONDS, results };
  fs.writeFileSync(path.join(out, `${LABEL}.json`), JSON.stringify(report, null, 2));

  console.log(`\n| Сценарий | FPS | p95, мс | p99, мс | Худший, мс | Кадров > 20 мс | Кадров > 33 мс | Главный поток, мс/с |\n|---|---|---|---|---|---|---|---|`);
  for (const r of results) console.log(`| ${r.scenario} | ${r.fps} | ${r.p95} | ${r.p99} | ${r.worst} | ${r.missedShare}% | ${r.slowShare}% | ${r.busyMs} |`);
  console.log(`\nGPU: ${gpu}; сохранено: tools/perf/results/${LABEL}.json`);
})().catch(error => {
  console.error(error);
  process.exit(1);
});
