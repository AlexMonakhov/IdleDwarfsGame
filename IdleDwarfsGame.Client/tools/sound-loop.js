// Бесшовный музыкальный луп из исходника (WAV и любой формат, который читает ffmpeg).
//
//   npm run sounds:loop -- <исходник> <ключ> [кроссфейд, с]
//   npm run sounds:loop -- sounds-src/music/dwarf_menu_fantasy_loop_60s.wav music_town
//
// Если конец трека не совпадает с началом сэмпл в сэмпл, на стыке слышен щелчок или скачок. Скрипт склеивает
// стык кроссфейдом: хвост (последние N секунд) плавно уходит, начало плавно входит, и они складываются в начало
// нового файла. Трек короче на N секунд, а при повторе за последним сэмплом идёт ровно то, что шло в исходнике.
//
// Результат — src/assets/sounds/music/<ключ>.ogg (бесшовный повтор в Chrome, Firefox, Edge) и <ключ>.mp3
// (запасной: у MP3 кодировщик добавляет на краях миллисекунды тишины). Сервис сам выберет, что играет браузер.

const { execFileSync, spawnSync } = require('child_process');
const fs = require('fs');
const path = require('path');
const ffmpeg = require('ffmpeg-static');

const [input, key, fadeArg] = process.argv.slice(2);
if (!input || !key) {
  console.error('Использование: npm run sounds:loop -- <исходник> <ключ> [кроссфейд, с]');
  process.exit(1);
}
const FADE_SECONDS = Number(fadeArg ?? 0.75);
const CHANNELS = 2;

// Частота исходника — её и сохраняем: передискретизация качества не добавит
// (ffmpeg без выходного файла пишет сведения о входе в stderr и завершается с ошибкой — это нормально)
const probe = spawnSync(ffmpeg, ['-hide_banner', '-i', input], { encoding: 'utf8' }).stderr;
const rateMatch = /Audio:.*?(\d+) Hz/.exec(probe);
const SAMPLE_RATE = rateMatch ? Number(rateMatch[1]) : 48000;

// Исходник → 32-битные float, стерео, без сжатия
const raw = execFileSync(ffmpeg, ['-hide_banner', '-loglevel', 'error', '-i', input, '-f', 'f32le', '-ac', String(CHANNELS), '-ar', String(SAMPLE_RATE), '-'], { maxBuffer: 1 << 30 });
const src = new Float32Array(raw.buffer, raw.byteOffset, raw.byteLength / 4);
const frames = src.length / CHANNELS;
const fade = Math.round(FADE_SECONDS * SAMPLE_RATE);
if (fade * 2 >= frames) {
  console.error(`Кроссфейд ${FADE_SECONDS} с слишком длинный для трека ${(frames / SAMPLE_RATE).toFixed(1)} с`);
  process.exit(1);
}

// Начало нового трека: хвост исходника уходит, начало входит. Равная мощность (cos/sin) — без провала громкости,
// если хвост и начало звучат по-разному
const outFrames = frames - fade;
const out = new Float32Array(outFrames * CHANNELS);
for (let i = 0; i < outFrames; i++) {
  for (let c = 0; c < CHANNELS; c++) {
    const head = src[i * CHANNELS + c];
    if (i < fade) {
      const t = (i + 0.5) / fade;
      const tail = src[(frames - fade + i) * CHANNELS + c];
      out[i * CHANNELS + c] = tail * Math.cos(t * Math.PI / 2) + head * Math.sin(t * Math.PI / 2);
    } else {
      out[i * CHANNELS + c] = head;
    }
  }
}

const jump = (data, len) => Math.max(...[0, 1].map(c => Math.abs(data[c] - data[(len - 1) * CHANNELS + c])));
console.log(`Скачок на стыке: было ${jump(src, frames).toFixed(3)}, стало ${jump(out, outFrames).toFixed(3)} (1.0 — полная шкала)`);

const outDir = path.join(__dirname, '..', 'src', 'assets', 'sounds', 'music');
fs.mkdirSync(outDir, { recursive: true });
const pcm = Buffer.from(out.buffer);
const encode = (file, codecArgs) => {
  execFileSync(ffmpeg, ['-hide_banner', '-loglevel', 'error', '-y', '-f', 'f32le', '-ac', String(CHANNELS), '-ar', String(SAMPLE_RATE), '-i', '-', ...codecArgs, path.join(outDir, file)], { input: pcm });
  const size = fs.statSync(path.join(outDir, file)).size;
  console.log(`  ${path.relative(process.cwd(), path.join(outDir, file))} — ${(size / 1024 / 1024).toFixed(2)} МБ`);
};
console.log(`Трек ${(outFrames / SAMPLE_RATE).toFixed(2)} с, ${SAMPLE_RATE} Гц:`);
encode(`${key}.ogg`, ['-c:a', 'libvorbis', '-q:a', '5']);
encode(`${key}.mp3`, ['-c:a', 'libmp3lame', '-b:a', '192k']);
