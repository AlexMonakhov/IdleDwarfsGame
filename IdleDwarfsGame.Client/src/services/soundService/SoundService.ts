import { reactive } from 'vue';
import * as sounds from './sounds';
import { SynthContext } from './sounds';

const STORAGE_KEY = 'sound-settings';

// Один и тот же звук не чаще раза в N мс (AoE-касты бьют несколько целей одновременно)
const THROTTLE_MS = 40;

// volume/muted — звуки боя, music* — фоновая музыка
export interface SoundSettings {
  volume: number;
  muted: boolean;
  musicVolume: number;
  musicMuted: boolean;
}

function loadSettings(): SoundSettings {
  const defaults: SoundSettings = { volume: 0.6, muted: false, musicVolume: 0.1, musicMuted: false };
  try {
    const saved = JSON.parse(localStorage.getItem(STORAGE_KEY) ?? 'null');
    return { ...defaults, ...saved };
  } catch {
    return defaults;
  }
}

// Глобальный сервис звуков: громкость и mute общие для всей игры и сохраняются между сессиями
class SoundService {
  readonly settings: SoundSettings = reactive(loadSettings());

  private ctx: AudioContext | null = null;
  private master: GainNode | null = null;
  private noise: AudioBuffer | null = null;
  private lastPlayed = new Map<string, number>();

  constructor() {
    // Браузер не даёт запустить звук до первого действия пользователя
    const unlock = () => this.context()?.resume();
    window.addEventListener('pointerdown', unlock, { once: true });
    window.addEventListener('keydown', unlock, { once: true });
  }

  setVolume(volume: number) {
    this.settings.volume = Math.min(1, Math.max(0, volume));
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

  setMusicVolume(volume: number) {
    this.settings.musicVolume = Math.min(1, Math.max(0, volume));
    if (this.settings.musicVolume > 0) this.settings.musicMuted = false;
    this.saveSettings();
  }

  setMusicMuted(muted: boolean) {
    this.settings.musicMuted = muted;
    this.saveSettings();
  }

  // Громкость для <audio> с музыкой — по той же квадратичной шкале, что и звуки
  get musicGain(): number {
    return this.settings.musicMuted ? 0 : this.settings.musicVolume ** 2;
  }

  // ===== Звуки =====

  hit(damageType = 'Physical', crit = false) {
    this.play(`hit-${damageType}`, s => sounds.hit(s, damageType, crit));
  }

  swing() {
    this.play('swing', sounds.swing);
  }

  cast() {
    this.play('cast', sounds.cast);
  }

  heal() {
    this.play('heal', sounds.heal);
  }

  stun() {
    this.play('stun', sounds.stun);
  }

  block() {
    this.play('block', sounds.block);
  }

  death() {
    this.play('death', sounds.death);
  }

  victory() {
    this.play('victory', sounds.victory);
  }

  defeat() {
    this.play('defeat', sounds.defeat);
  }

  // ===== Внутреннее =====

  private play(key: string, render: (s: SynthContext) => void) {
    if (this.settings.muted || this.settings.volume <= 0) return;

    const ctx = this.context();
    if (!ctx || !this.master || !this.noise) return;
    // Пока контекст на паузе, звуки не копим — иначе они все проиграются разом после resume
    if (ctx.state !== 'running') {
      ctx.resume();
      return;
    }

    const now = performance.now();
    if (now - (this.lastPlayed.get(key) ?? 0) < THROTTLE_MS) return;
    this.lastPlayed.set(key, now);

    render({ ctx, out: this.master, noise: this.noise, t: ctx.currentTime });
  }

  private context() {
    if (this.ctx) return this.ctx;
    if (typeof AudioContext === 'undefined') return null;

    this.ctx = new AudioContext();
    this.noise = sounds.createNoiseBuffer(this.ctx);

    // Компрессор не даёт клиппинга, когда много звуков накладываются
    const compressor = this.ctx.createDynamicsCompressor();
    compressor.connect(this.ctx.destination);
    this.master = this.ctx.createGain();
    this.master.connect(compressor);
    this.applySettings();

    return this.ctx;
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

    if (!this.ctx || !this.master) return;
    // Квадрат даёт более естественную шкалу громкости на слух
    const gain = this.settings.muted ? 0 : this.settings.volume ** 2;
    this.master.gain.setTargetAtTime(gain, this.ctx.currentTime, 0.02);
  }
}

export const soundService = new SoundService();
