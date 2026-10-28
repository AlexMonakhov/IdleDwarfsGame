// Процедурные звуки на Web Audio API — без аудиофайлов.
// Каждый звук собирается из осцилляторов и отфильтрованного шума с короткой огибающей.

export interface SynthContext {
  ctx: AudioContext;
  out: AudioNode;
  noise: AudioBuffer;
  t: number;
}

interface ToneOptions {
  type?: OscillatorType;
  freq: number;
  freqEnd?: number;
  at?: number;
  dur: number;
  gain: number;
  attack?: number;
}

interface NoiseOptions {
  filter?: BiquadFilterType;
  freq: number;
  freqEnd?: number;
  q?: number;
  at?: number;
  dur: number;
  gain: number;
  attack?: number;
}

// Небольшой разброс, чтобы повторяющиеся удары не звучали одинаково
const vary = (value: number, spread = 0.08) => value * (1 + (Math.random() * 2 - 1) * spread);

function envelope(s: SynthContext, t: number, dur: number, peak: number, attack = 0.005) {
  const gain = s.ctx.createGain();
  gain.gain.setValueAtTime(0.0001, t);
  gain.gain.exponentialRampToValueAtTime(peak, t + attack);
  gain.gain.exponentialRampToValueAtTime(0.0001, t + dur);
  gain.connect(s.out);
  return gain;
}

function tone(s: SynthContext, o: ToneOptions) {
  const t = s.t + (o.at ?? 0);
  const osc = s.ctx.createOscillator();
  osc.type = o.type ?? 'sine';
  osc.frequency.setValueAtTime(o.freq, t);
  if (o.freqEnd) osc.frequency.exponentialRampToValueAtTime(o.freqEnd, t + o.dur);
  osc.connect(envelope(s, t, o.dur, o.gain, o.attack));
  osc.start(t);
  osc.stop(t + o.dur + 0.05);
}

function noise(s: SynthContext, o: NoiseOptions) {
  const t = s.t + (o.at ?? 0);
  const src = s.ctx.createBufferSource();
  src.buffer = s.noise;
  src.loop = true;

  const filter = s.ctx.createBiquadFilter();
  filter.type = o.filter ?? 'lowpass';
  filter.Q.value = o.q ?? 1;
  filter.frequency.setValueAtTime(o.freq, t);
  if (o.freqEnd) filter.frequency.exponentialRampToValueAtTime(o.freqEnd, t + o.dur);

  src.connect(filter).connect(envelope(s, t, o.dur, o.gain, o.attack));
  src.start(t, Math.random() * 0.5);
  src.stop(t + o.dur + 0.05);
}

export function createNoiseBuffer(ctx: AudioContext) {
  const buffer = ctx.createBuffer(1, ctx.sampleRate, ctx.sampleRate);
  const data = buffer.getChannelData(0);
  for (let i = 0; i < data.length; i++) data[i] = Math.random() * 2 - 1;
  return buffer;
}

// Низкий «тумп» — основа почти любого попадания
function thump(s: SynthContext, gain: number) {
  tone(s, { freq: vary(140), freqEnd: 45, dur: 0.18, gain });
}

const HIT_SOUNDS: Record<string, (s: SynthContext) => void> = {
  Physical: s => {
    thump(s, 0.8);
    noise(s, { freq: vary(2500), freqEnd: 300, dur: 0.12, gain: 0.6 });
  },
  Magical: s => {
    thump(s, 0.4);
    tone(s, { type: 'triangle', freq: vary(880), freqEnd: 440, dur: 0.25, gain: 0.25 });
    tone(s, { type: 'triangle', freq: vary(1320), freqEnd: 660, dur: 0.25, gain: 0.15 });
  },
  Fire: s => {
    thump(s, 0.4);
    noise(s, { freq: 3000, freqEnd: 400, dur: 0.4, gain: 0.5, attack: 0.02 });
  },
  Frost: s => {
    noise(s, { filter: 'highpass', freq: 4000, dur: 0.15, gain: 0.35 });
    tone(s, { freq: vary(2400), freqEnd: 1800, dur: 0.3, gain: 0.12 });
    tone(s, { freq: vary(3100), freqEnd: 2600, at: 0.03, dur: 0.25, gain: 0.08 });
  },
  Lightning: s => {
    for (let i = 0; i < 4; i++) {
      noise(s, { filter: 'highpass', freq: vary(2000, 0.3), at: i * 0.035, dur: 0.05, gain: 0.45 });
    }
    thump(s, 0.5);
  },
  Poison: s => {
    for (let i = 0; i < 3; i++) {
      tone(s, { freq: vary(300, 0.2), freqEnd: vary(500, 0.2), at: i * 0.06, dur: 0.08, gain: 0.2 });
    }
  },
  Bleed: s => {
    noise(s, { filter: 'bandpass', freq: vary(900), freqEnd: 300, q: 3, dur: 0.15, gain: 0.5 });
    thump(s, 0.3);
  },
  True: s => {
    thump(s, 0.6);
    tone(s, { type: 'square', freq: vary(1200), freqEnd: 900, dur: 0.12, gain: 0.08 });
  },
  // Глухой провал вниз и шипение тьмы
  Dark: s => {
    thump(s, 0.5);
    tone(s, { type: 'sawtooth', freq: vary(220), freqEnd: 70, dur: 0.35, gain: 0.12 });
    noise(s, { filter: 'lowpass', freq: 900, freqEnd: 200, dur: 0.3, gain: 0.3 });
  },
  // Хрустальный звон: несколько высоких нот разом
  Radiant: s => {
    thump(s, 0.3);
    [1, 1.5, 2].forEach((ratio, i) => {
      tone(s, { type: 'sine', freq: vary(1320) * ratio, at: i * 0.02, dur: 0.4, gain: 0.1 });
    });
  },
};

// Металлический звон поверх крита
function clang(s: SynthContext) {
  const base = vary(520);
  [1, 2.76, 5.4].forEach((ratio, i) => {
    tone(s, { type: 'triangle', freq: base * ratio, dur: 0.5 - i * 0.1, gain: 0.15 / (i + 1) });
  });
  noise(s, { filter: 'highpass', freq: 6000, dur: 0.06, gain: 0.4 });
  thump(s, 0.6);
}

export function hit(s: SynthContext, damageType: string, crit: boolean) {
  (HIT_SOUNDS[damageType] ?? HIT_SOUNDS.Physical)(s);
  if (crit) clang(s);
}

export function swing(s: SynthContext) {
  noise(s, { filter: 'bandpass', freq: vary(600), freqEnd: vary(2400), q: 1.2, dur: 0.18, gain: 0.25, attack: 0.06 });
}

export function cast(s: SynthContext) {
  tone(s, { type: 'triangle', freq: vary(300), freqEnd: 900, dur: 0.3, gain: 0.15, attack: 0.1 });
  noise(s, { filter: 'bandpass', freq: 800, freqEnd: 3000, q: 2, dur: 0.3, gain: 0.12, attack: 0.1 });
}

export function heal(s: SynthContext) {
  [523.25, 659.25, 783.99, 1046.5].forEach((freq, i) => {
    tone(s, { freq, at: i * 0.06, dur: 0.35, gain: 0.15 });
  });
}

export function stun(s: SynthContext) {
  for (let i = 0; i < 3; i++) {
    tone(s, { freq: 1500, freqEnd: 2200, at: i * 0.09, dur: 0.08, gain: 0.1 });
  }
}

export function block(s: SynthContext) {
  tone(s, { type: 'triangle', freq: vary(1800), dur: 0.2, gain: 0.15 });
  tone(s, { type: 'triangle', freq: vary(2700), dur: 0.15, gain: 0.1 });
  noise(s, { filter: 'highpass', freq: 5000, dur: 0.04, gain: 0.3 });
}

export function death(s: SynthContext) {
  tone(s, { type: 'triangle', freq: 220, freqEnd: 55, dur: 0.6, gain: 0.3 });
  noise(s, { freq: 800, freqEnd: 100, dur: 0.5, gain: 0.3 });
}

function jingle(s: SynthContext, notes: number[], step: number, gain: number) {
  notes.forEach((freq, i) => {
    const last = i === notes.length - 1;
    tone(s, { type: 'triangle', freq, at: i * step, dur: last ? 0.7 : 0.3, gain });
  });
}

export function victory(s: SynthContext) {
  jingle(s, [523.25, 659.25, 783.99, 1046.5], 0.12, 0.2);
}

export function defeat(s: SynthContext) {
  jingle(s, [392, 349.23, 311.13, 261.63], 0.18, 0.18);
}
