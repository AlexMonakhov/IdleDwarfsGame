import { reactive } from 'vue';
import * as synth from './sounds';
import { SynthContext } from './sounds';
import { damageSound, soundDef, SOUNDS, type MusicKey, type SoundChannel, type SoundDef, type SoundKey } from './soundCatalog';

const STORAGE_KEY = 'sound-settings';

// Один и тот же звук не чаще раза в N мс (AoE-касты бьют несколько целей одновременно)
const DEFAULT_THROTTLE_MS = 40;
// Файл ещё грузился, когда звук попросили: играем, только если опоздали не больше чем на столько
const LATE_START_MS = 250;
// Смена музыки — плавный переход, с
const MUSIC_FADE_S = 1.2;

// volume/muted — звуки боя, music* — фоновая музыка, ui* — интерфейс
export interface SoundSettings {
  volume: number;
  muted: boolean;
  musicVolume: number;
  musicMuted: boolean;
  uiVolume: number;
  uiMuted: boolean;
}

function loadSettings(): SoundSettings {
  const defaults: SoundSettings = { volume: 0.6, muted: false, musicVolume: 0.1, musicMuted: false, uiVolume: 0.6, uiMuted: false };
  try {
    const saved = JSON.parse(localStorage.getItem(STORAGE_KEY) ?? 'null');
    return { ...defaults, ...saved };
  } catch {
    return defaults;
  }
}

/* ===== Файлы звуков =====
   Всё, что лежит в src/assets/sounds/ (в любых подпапках): имя файла — ключ из каталога, «-2», «-3» — варианты */
const EXT_PREFERENCE = ['ogg', 'webm', 'mp3', 'wav'];
const MIME: Record<string, string> = { ogg: 'audio/ogg', webm: 'audio/webm', mp3: 'audio/mpeg', wav: 'audio/wav' };

function collectFiles(): Map<string, string[]> {
  const context = require.context('@/assets/sounds', true, /\.(mp3|ogg|wav|webm)$/);
  const probe = typeof Audio !== 'undefined' ? new Audio() : null;
  const playable = (ext: string) => !probe || probe.canPlayType(MIME[ext]) !== '';

  // Вариант (ui_click-2) → лучший формат, который браузер умеет играть
  const variants = new Map<string, { ext: string; url: string }>();
  for (const path of context.keys()) {
    const match = /([^/]+)\.(\w+)$/.exec(path);
    if (!match || !playable(match[2])) continue;
    const [, name, ext] = match;
    const current = variants.get(name);
    if (!current || EXT_PREFERENCE.indexOf(ext) < EXT_PREFERENCE.indexOf(current.ext)) {
      variants.set(name, { ext, url: context(path) as string });
    }
  }

  const files = new Map<string, string[]>();
  const unknown: string[] = [];
  variants.forEach(({ url }, name) => {
    const key = name.replace(/-\d+$/, '');
    if (!soundDef(key)) {
      unknown.push(name);
      return;
    }
    files.set(key, [...(files.get(key) ?? []), url]);
  });
  // Опечатка в имени файла — звук молча не заиграет; подсказываем в консоли при разработке
  if (unknown.length && process.env.NODE_ENV !== 'production') {
    console.warn('[sounds] Файлы без ключа в soundCatalog.ts:', unknown.join(', '));
  }
  return files;
}

// Запасной синтез для боя: пока нет файлов, арена звучит как раньше
const SYNTH: Partial<Record<string, (s: SynthContext) => void>> = {
  battle_swing: synth.swing,
  battle_cast: synth.cast,
  battle_arena_buff: synth.cast,
  battle_heal: synth.heal,
  battle_stun: synth.stun,
  battle_immune: synth.block,
  battle_death: synth.death,
  battle_crit: synth.crit,
  battle_victory: synth.victory,
  battle_defeat: synth.defeat,
  battle_hit_physical: s => synth.hit(s, 'physical'),
  battle_hit_magical: s => synth.hit(s, 'magical'),
  battle_hit_fire: s => synth.hit(s, 'fire'),
  battle_hit_frost: s => synth.hit(s, 'frost'),
  battle_hit_lightning: s => synth.hit(s, 'lightning'),
  battle_hit_poison: s => synth.hit(s, 'poison'),
  battle_hit_bleed: s => synth.hit(s, 'bleed'),
  battle_hit_true: s => synth.hit(s, 'true'),
  battle_hit_dark: s => synth.hit(s, 'dark'),
  battle_hit_radiant: s => synth.hit(s, 'radiant'),
};

export interface PlayOptions {
  // Множитель громкости поверх громкости ключа
  volume?: number;
}

type EffectChannel = Exclude<SoundChannel, 'music'>;

// Глобальный сервис звуков: громкость и mute по каналам общие для всей игры и сохраняются между сессиями
class SoundService {
  readonly settings: SoundSettings = reactive(loadSettings());
  // Какая музыка нужна арене сейчас (босс или обычный бой) — её выбирает InterfaceView
  readonly scene = reactive({ bossFight: false });

  private readonly files = collectFiles();
  private ctx: AudioContext | null = null;
  private channels: Record<EffectChannel, GainNode> | null = null;
  private noise: AudioBuffer | null = null;
  private buffers = new Map<string, AudioBuffer | null>();
  private loading = new Map<string, Promise<AudioBuffer | null>>();
  private lastPlayed = new Map<string, number>();

  // Музыка — через Web Audio: буфер повторяется с точностью до сэмпла, без паузы на стыке (у <audio loop> она есть).
  // bus — громкость канала из настроек, у каждого трека свой gain для перехода
  private musicBus: GainNode | null = null;
  // Общий выход: всё звучащее идёт через него, им звук гасится при закрытии вкладки
  private master: GainNode | null = null;
  private music: { key: MusicKey | null; url: string | null; track: { source: AudioBufferSourceNode; gain: GainNode } | null } = { key: null, url: null, track: null };

  constructor() {
    // Браузер не даёт запустить звук до первого действия пользователя
    const unlock = () => {
      this.context()?.resume();
      this.startMusic();
      this.preload();
    };
    window.addEventListener('pointerdown', unlock, { once: true });
    window.addEventListener('keydown', unlock, { once: true });

    // Закрытие и перезагрузка вкладки обрывают звук посреди волны — это слышно как щелчок. Пока страница
    // выгружается, успеваем плавно увести общую громкость в ноль (pagehide — запасной, если beforeunload не пришёл)
    window.addEventListener('beforeunload', () => this.silence());
    window.addEventListener('pagehide', () => this.silence());
    // Страница не выгрузилась, а вернулась из кэша браузера («Назад») — звук обратно
    window.addEventListener('pageshow', () => this.master?.gain.setTargetAtTime(1, this.ctx?.currentTime ?? 0, 0.05));
  }

  // ===== Настройки =====

  setVolume(volume: number) {
    this.settings.volume = clamp01(volume);
    if (this.settings.volume > 0) this.settings.muted = false;
    this.applySettings();
  }

  setMuted(muted: boolean) {
    this.settings.muted = muted;
    this.applySettings();
  }

  toggleMute() {
    this.setMuted(!this.settings.muted);
  }

  setUiVolume(volume: number) {
    this.settings.uiVolume = clamp01(volume);
    if (this.settings.uiVolume > 0) this.settings.uiMuted = false;
    this.applySettings();
  }

  setUiMuted(muted: boolean) {
    this.settings.uiMuted = muted;
    this.applySettings();
  }

  setMusicVolume(volume: number) {
    this.settings.musicVolume = clamp01(volume);
    if (this.settings.musicVolume > 0) this.settings.musicMuted = false;
    this.applySettings();
  }

  setMusicMuted(muted: boolean) {
    this.settings.musicMuted = muted;
    this.applySettings();
  }

  // Громкость музыки — по той же квадратичной шкале, что и звуки
  get musicGain(): number {
    return this.settings.musicMuted ? 0 : this.settings.musicVolume ** 2;
  }

  // ===== Звуки =====

  // Звук по ключу из каталога. Нет своего файла — файл запасного ключа, потом синтез, иначе тишина
  play(key: SoundKey, options: PlayOptions = {}) {
    const def: SoundDef = SOUNDS[key];
    if (def.channel === 'music' || !this.channelAudible(def.channel)) return;

    const ctx = this.context();
    if (!ctx || !this.channels || !this.noise) return;
    // Пока контекст на паузе, звуки не копим — иначе они все проиграются разом после resume
    if (ctx.state !== 'running') {
      ctx.resume();
      return;
    }

    const now = performance.now();
    if (now - (this.lastPlayed.get(key) ?? 0) < (def.throttle ?? DEFAULT_THROTTLE_MS)) return;
    this.lastPlayed.set(key, now);

    const out = this.channels[def.channel];
    const volume = (def.volume ?? 1) * (options.volume ?? 1);
    const rate = def.pitch ? 1 + (Math.random() * 2 - 1) * def.pitch : 1;

    const fileKey = this.chain(key).find(k => this.files.has(k));
    if (fileKey) {
      this.playFile(fileKey, out, volume, rate, now);
      return;
    }
    const synthKey = this.chain(key).find(k => SYNTH[k]);
    if (synthKey) {
      const gain = ctx.createGain();
      gain.gain.value = volume;
      gain.connect(out);
      SYNTH[synthKey]?.({ ctx, out: gain, noise: this.noise, t: ctx.currentTime });
    }
  }

  // Звук после успешного запроса: api().then(soundService.played('collect')) — значение проходит дальше как есть
  played(key: SoundKey) {
    return <T>(value: T): T => {
      this.play(key);
      return value;
    };
  }

  // Попадание: тип урона с сервера («Fire») → свой звук, крит — звон поверх
  hit(damageType?: string | null, crit = false) {
    this.play(`battle_hit_${damageSound(damageType)}`);
    if (crit) this.play('battle_crit');
  }

  // Каст заклинания: свой звук стихии, нет его — общий
  cast(damageType?: string | null) {
    this.play(damageType ? `battle_cast_${damageSound(damageType)}` : 'battle_cast');
  }

  // ===== Музыка =====

  // Сменить фоновую музыку с плавным переходом. Нет файла сцены — запасная (бой → город).
  // До первого действия игрока звук запрещён — тогда только запоминаем сцену, трек запустит unlock
  setMusic(key: MusicKey | null) {
    if (key === this.music.key) return;
    this.music.key = key;
    if (this.ctx) this.startMusic();
  }

  // ===== Внутреннее =====

  // Быстро и плавно в тишину: постоянная времени 8 мс — за ~40 мс громкость падает на 40 дБ
  private silence() {
    if (!this.ctx || !this.master) return;
    const t = this.ctx.currentTime;
    this.master.gain.cancelScheduledValues(t);
    this.master.gain.setValueAtTime(this.master.gain.value, t);
    this.master.gain.setTargetAtTime(0, t, 0.008);
  }

  // Ключ и его запасные по цепочке fallback
  private chain(key: string): string[] {
    const keys: string[] = [];
    for (let k: string | undefined = key; k && !keys.includes(k); k = soundDef(k)?.fallback) keys.push(k);
    return keys;
  }

  private channelAudible(channel: EffectChannel) {
    return channel === 'ui'
      ? !this.settings.uiMuted && this.settings.uiVolume > 0
      : !this.settings.muted && this.settings.volume > 0;
  }

  private playFile(key: string, out: AudioNode, volume: number, rate: number, requestedAt: number) {
    const urls = this.files.get(key);
    if (!urls?.length) return;
    const url = urls[Math.floor(Math.random() * urls.length)];

    const start = (buffer: AudioBuffer | null) => {
      if (!buffer || !this.ctx) return;
      const source = this.ctx.createBufferSource();
      source.buffer = buffer;
      source.playbackRate.value = rate;
      const gain = this.ctx.createGain();
      gain.gain.value = volume;
      source.connect(gain).connect(out);
      source.start();
    };

    const buffer = this.buffers.get(url);
    if (buffer !== undefined) {
      start(buffer);
      return;
    }
    this.load(url).then(loaded => {
      if (performance.now() - requestedAt <= LATE_START_MS) start(loaded);
    });
  }

  private load(url: string, cache = true): Promise<AudioBuffer | null> {
    const pending = this.loading.get(url);
    if (pending) return pending;
    const ctx = this.context();
    if (!ctx) return Promise.resolve(null);

    const promise = fetch(url)
      .then(response => response.arrayBuffer())
      .then(data => ctx.decodeAudioData(data))
      .catch(() => null)
      .then(buffer => {
        if (cache) this.buffers.set(url, buffer);
        else this.loading.delete(url);
        return buffer;
      });
    this.loading.set(url, promise);
    return promise;
  }

  // Короткие звуки грузим заранее, после первого действия: иначе первый клик каждой кнопки был бы беззвучным
  private async preload() {
    const urls = [...this.files.entries()]
      .filter(([key]) => soundDef(key)?.channel !== 'music')
      .flatMap(([, list]) => list);
    for (let i = 0; i < urls.length; i += 6) {
      await Promise.all(urls.slice(i, i + 6).map(url => this.load(url)));
    }
  }

  // Запустить трек текущей сцены (если он ещё не играет), прежний — плавно увести
  private startMusic() {
    const ctx = this.context();
    if (!ctx || !this.musicBus) return;
    const key = this.music.key;
    const fileKey = key ? this.chain(key).find(k => this.files.has(k)) : undefined;
    const url = fileKey ? this.files.get(fileKey)?.[0] ?? null : null;
    if (url === this.music.url) return;
    this.music.url = url;

    const previous = this.music.track;
    this.music.track = null;
    if (previous) {
      const t = ctx.currentTime;
      previous.gain.gain.setValueAtTime(previous.gain.gain.value, t);
      previous.gain.gain.linearRampToValueAtTime(0, t + MUSIC_FADE_S);
      previous.source.stop(t + MUSIC_FADE_S + 0.05);
    }
    if (!url) return;

    // Музыку не кэшируем: распакованный трек занимает десятки мегабайт
    this.load(url, false).then(buffer => {
      if (!buffer || this.music.url !== url || !this.musicBus) return;
      const source = ctx.createBufferSource();
      source.buffer = buffer;
      source.loop = true;
      const gain = ctx.createGain();
      gain.gain.setValueAtTime(0, ctx.currentTime);
      gain.gain.linearRampToValueAtTime(1, ctx.currentTime + MUSIC_FADE_S);
      source.connect(gain).connect(this.musicBus);
      source.start();
      this.music.track = { source, gain };
    });
  }

  private context() {
    if (this.ctx) return this.ctx;
    if (typeof AudioContext === 'undefined') return null;

    const ctx = new AudioContext();
    this.ctx = ctx;
    this.noise = synth.createNoiseBuffer(ctx);

    this.master = ctx.createGain();
    this.master.connect(ctx.destination);
    // Компрессор не даёт клиппинга, когда много звуков накладываются
    const compressor = ctx.createDynamicsCompressor();
    compressor.connect(this.master);
    const channel = () => {
      const gain = ctx.createGain();
      gain.connect(compressor);
      return gain;
    };
    this.channels = { ui: channel(), sfx: channel() };
    // Музыка — мимо компрессора: иначе он приглушал бы её под каждым громким ударом
    this.musicBus = ctx.createGain();
    this.musicBus.connect(this.master);
    this.applySettings();

    return ctx;
  }

  private saveSettings() {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(this.settings));
    } catch {
      // приватный режим / storage отключён — просто не сохраняем
    }
  }

  private applySettings() {
    this.saveSettings();

    if (!this.ctx || !this.channels || !this.musicBus) return;
    // Квадрат даёт более естественную шкалу громкости на слух
    const gain = (muted: boolean, volume: number) => (muted ? 0 : volume ** 2);
    this.channels.sfx.gain.setTargetAtTime(gain(this.settings.muted, this.settings.volume), this.ctx.currentTime, 0.02);
    this.channels.ui.gain.setTargetAtTime(gain(this.settings.uiMuted, this.settings.uiVolume), this.ctx.currentTime, 0.02);
    this.musicBus.gain.setTargetAtTime(this.musicGain, this.ctx.currentTime, 0.05);
  }
}

const clamp01 = (value: number) => Math.min(1, Math.max(0, value));

export const soundService = new SoundService();
