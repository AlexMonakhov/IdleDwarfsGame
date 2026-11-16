import { Container, Graphics, Sprite, type BLEND_MODES, type Texture } from 'pixi.js';
import { ParticleSim, clamp01, paletteTint, rand, type CustomParticleTexture, type ParticleBehavior } from '../particles';
import { fxTexture, TEXTURE_FIGURE, type FxTextureName } from '../textures';
import { RADIAL_RADIUS, radialTexture } from '../art';
import { Strand } from '../strand';
import { tintOf } from '../effect';
import { jaggedPath } from '../geometry';
import { dropTexture, leafTexture } from './sprites';

// Набор для концептов анимаций (страница /fx-concepts): сцена с кастером и целями, время в секундах, размеры в px.
// Концепт описывает себя твинами (окно времени → прогресс 0..1) и событиями по времени; частицы шагает и убирает
// за собой сам набор. Единицы — как у остальных Pixi-эффектов (particles.ts), чтобы понравившийся концепт потом
// переехал в настоящий эффект заклинания без пересчёта

export interface Pt {
  x: number;
  y: number;
}

// Юнит на сцене: точки на теле и реакции
export interface ConceptUnit {
  center: Pt;
  feet: Pt;
  head: Pt;
  // Первый hit/heal по цели в бою применяет к ней урон или лечение из лога; следующие — только вспышка
  hit(crit?: boolean, flash?: string): void;
  heal(): void;
  // На эту цель сработал артефакт заклинания (в бою — есть запись от артефакта; на странице концептов — всегда)
  proc?: boolean;
}

export interface ConceptStage {
  // Под юнитами и над ними
  back: Container;
  front: Container;
  caster: ConceptUnit;
  // Цели заклинания (сколько — по механике уровня, Concept.targets): первая — главная.
  // Союзники — весь отряд кастера, первый — сам кастер
  enemies: ConceptUnit[];
  allies: ConceptUnit[];
  // Враги, в которых заклинание не попало (для добавок «в соседей»)
  bystanders: ConceptUnit[];
  // Кого заклинание лечит (Concept.heals; в бою — союзники с записью лечения)
  healed: ConceptUnit[];
  width: number;
  height: number;
  // Кастер взмахивает оружием
  cast(color?: string): void;
  // Тряска сцены: сила в px, длительность в секундах
  shake(power: number, seconds?: number): void;
  // Вспышка на весь экран
  flash(color: string, alpha?: number, seconds?: number): void;
}

export const TAU = Math.PI * 2;
// Сжатие по вертикали для всего, что лежит на земле (круги, волны)
export const GROUND = 0.32;

export const lerp = (a: number, b: number, t: number) => a + (b - a) * t;
export const mix = (a: Pt, b: Pt, t: number): Pt => ({ x: lerp(a.x, b.x, t), y: lerp(a.y, b.y, t) });
export const dist = (a: Pt, b: Pt) => Math.hypot(b.x - a.x, b.y - a.y);
export const angle = (a: Pt, b: Pt) => Math.atan2(b.y - a.y, b.x - a.x);
export const polar = (c: Pt, r: number, a: number, squash = 1): Pt => ({ x: c.x + Math.cos(a) * r, y: c.y + Math.sin(a) * r * squash });
// Полёт по дуге высотой lift
export const arc = (a: Pt, b: Pt, t: number, lift: number): Pt => ({ x: lerp(a.x, b.x, t), y: lerp(a.y, b.y, t) - Math.sin(Math.PI * t) * lift });
// Квадратичная кривая Безье через опорную точку c
export const bez = (a: Pt, c: Pt, b: Pt, t: number): Pt => {
  const u = 1 - t;
  return { x: u * u * a.x + 2 * u * t * c.x + t * t * b.x, y: u * u * a.y + 2 * u * t * c.y + t * t * b.y };
};

export const ease = {
  out: (t: number) => 1 - (1 - t) * (1 - t),
  out3: (t: number) => 1 - Math.pow(1 - t, 3),
  in: (t: number) => t * t,
  in3: (t: number) => t * t * t,
  inOut: (t: number) => (t < 0.5 ? 2 * t * t : 1 - Math.pow(-2 * t + 2, 2) / 2),
  // С перелётом
  back: (t: number) => 1 + 2.70158 * Math.pow(t - 1, 3) + 1.70158 * Math.pow(t - 1, 2),
};

// 0 → 1 → 0 за прогресс
export const bump = (p: number) => Math.sin(Math.PI * clamp01(p));
// Быстро появиться (доля fadeIn) и плавно погаснуть (доля fadeOut)
export const fade = (p: number, fadeIn = 0.12, fadeOut = 0.4) =>
  Math.min(clamp01(p / fadeIn), clamp01((1 - p) / fadeOut));

export const color = (css: string) => tintOf(css);
export const palette = (colors: string[]) => paletteTint(colors.map(c => tintOf(c)));

// Спрайт с размером «радиус фигуры»: put ставит, показывает и задаёт прозрачность
export class Dot {
  readonly s: Sprite;

  constructor(texture: Texture, private readonly figure: number, tint: number, blend: BLEND_MODES) {
    this.s = new Sprite(texture);
    this.s.anchor.set(0.5);
    this.s.tint = tint;
    this.s.blendMode = blend;
    this.s.visible = false;
  }

  put(x: number, y: number, r: number, alpha = 1, rotation?: number) {
    return this.oval(x, y, r, r, alpha, rotation);
  }

  oval(x: number, y: number, rx: number, ry: number, alpha = 1, rotation?: number) {
    this.s.visible = alpha > 0.003 && rx > 0 && ry > 0;
    this.s.position.set(x, y);
    this.s.scale.set(rx / this.figure, ry / this.figure);
    this.s.alpha = clamp01(alpha);
    if (rotation !== undefined) this.s.rotation = rotation;
    return this;
  }

  hide() {
    this.s.visible = false;
  }
}

export interface SprayOptions {
  x: number;
  y: number;
  count: number;
  // px/с
  speed: [number, number];
  // Направление и разброс, рад (по умолчанию — во все стороны)
  dir?: number;
  spread?: number;
  // секунды
  life: [number, number];
  size: [number, number];
  // Разброс точки рождения
  jitter?: number;
  // Повернуть частицу по направлению полёта (для вытянутых)
  align?: boolean;
  spin?: number;
  tint?: number;
  // Сжатие скорости по вертикали (разлёт по земле)
  squash?: number;
}

export const spray = (sim: ParticleSim, o: SprayOptions) => {
  for (let i = 0; i < o.count; i++) {
    const a = o.dir === undefined ? Math.random() * TAU : o.dir + rand(-(o.spread ?? 0.4), o.spread ?? 0.4);
    const speed = rand(o.speed[0], o.speed[1]);
    const j = o.jitter ?? 0;
    sim.spawn({
      x: o.x + rand(-j, j),
      y: o.y + rand(-j, j),
      vx: Math.cos(a) * speed,
      vy: Math.sin(a) * speed * (o.squash ?? 1),
      lifetime: rand(o.life[0], o.life[1]),
      size: rand(o.size[0], o.size[1]),
      rotation: o.align ? a : o.spin ? Math.random() * TAU : 0,
      spin: o.spin ? rand(-o.spin, o.spin) : 0,
      tint: o.tint,
    });
  }
};

interface Tween {
  start: number;
  duration: number;
  run: (p: number) => void;
  hide: Dot[];
  state: 'wait' | 'run' | 'done';
}

export type Layer = 'front' | 'back';

// Сборщик одного проигрывания концепта: создаёт детали на сцене, ведёт твины и события, убирает всё за собой
export class Fx {
  private readonly sims: ParticleSim[] = [];
  private readonly tweens: Tween[] = [];
  private readonly events: { time: number; run: () => void; done: boolean }[] = [];

  // Шаг текущего кадра, секунды
  dt = 1 / 60;

  private readonly children: { fx: Fx; offset: number; update: Frame | void }[] = [];

  // Доп. эффекты (Concept.extras): отложенные добавки, из которых потом сделаем отдельные заклинания.
  // В бою выключены, на странице концептов — переключателем
  extras = false;
  // Виды эффектов артефактов, надетых на заклинание (ItemEffectInfo.Kind с сервера): анимация каста с артефактом
  artifacts = new Set<string>();
  // Каст пришёл от артефакта: вид эффекта и заклинание, на котором артефакт (повтор, срабатывание, раскол…)
  trigger: ArtifactTrigger | null = null;
  // Откуда идёт срабатывание без кастера (умерший носитель яда, расколотый лёд); null — от кастера
  origin: Pt | null = null;
  // Насколько гуще рисовать, когда одна и та же анимация идёт по многим целям сразу (perTarget): 1 — одна цель
  density = 1;
  // На сколько секунд проигрывание дольше Concept.duration (разбег по целям у perTarget)
  tail = 0;

  // level — уровень прокачки заклинания (1…3): концепты с levels: true меняют по нему размах и добавки
  constructor(readonly stage: ConceptStage, readonly level: SpellLevel = 3) {}

  // Надет артефакт с таким видом эффекта
  has(kind: string) {
    return this.artifacts.has(kind);
  }

  // Каст — срабатывание артефакта такого вида (и, если задано, с такого заклинания)
  triggered(kind: string, spell?: string) {
    return this.trigger?.kind === kind && (!spell || this.trigger.spell === spell);
  }

  // Та же анимация со сдвигом по времени и своей сценой (другая цель): шагает и убирается вместе с этой
  fork(offset: number, stage: Partial<ConceptStage>, build: (fx: Fx) => Frame | void) {
    const child = new Fx({ ...this.stage, ...stage }, this.level);
    child.artifacts = this.artifacts;
    child.trigger = this.trigger;
    child.origin = this.origin;
    child.extras = this.extras;
    child.density = this.density;
    this.children.push({ fx: child, offset, update: build(child) });
    return child;
  }

  // Значение для текущего уровня: fx.lvl([на 1, на 2, на 3])
  lvl<T>(values: [T, T, T]): T {
    return values[this.level - 1];
  }

  // Уровень не ниже min — для добавок, которые появляются со 2-го или только на 3-м
  since(min: SpellLevel) {
    return this.level >= min;
  }

  // Сколько частиц родить в этом кадре при perSecond штук в секунду (дробная часть — с шансом)
  count(perSecond: number) {
    const n = perSecond * this.dt;
    return Math.floor(n) + (Math.random() < n % 1 ? 1 : 0);
  }

  private layer(layer: Layer) {
    return layer === 'front' ? this.stage.front : this.stage.back;
  }

  dot(texture: FxTextureName, css: string, layer: Layer = 'front', blend: BLEND_MODES = 'add') {
    const dot = new Dot(fxTexture(texture), TEXTURE_FIGURE[texture], tintOf(css), blend);
    this.layer(layer).addChild(dot.s);
    return dot;
  }

  // Рисованная текстура (sprites.ts); у цветных tint белый
  tex(texture: CustomParticleTexture, layer: Layer = 'front', blend: BLEND_MODES = 'normal', css = '#ffffff') {
    const dot = new Dot(texture.texture, texture.figure, tintOf(css), blend);
    this.layer(layer).addChild(dot.s);
    return dot;
  }

  // Спрайт, стоящий на основании (кристалл, шип): опора внизу по центру, height задаёт высоту на экране
  spike(texture: CustomParticleTexture, x: number, y: number, height: number, layer: Layer = 'front', blend: BLEND_MODES = 'normal') {
    const sprite = new Sprite(texture.texture);
    sprite.anchor.set(0.5, 1);
    sprite.blendMode = blend;
    sprite.position.set(x, y);
    const k = height / texture.figure;
    sprite.scale.set(k, 0);
    this.layer(layer).addChild(sprite);
    return { sprite, k };
  }

  // Круг с прозрачностью по профилю [смещение 0..1, альфа]
  radial(stops: [number, number][], css: string, layer: Layer = 'front', blend: BLEND_MODES = 'add') {
    const dot = new Dot(radialTexture(stops), RADIAL_RADIUS, tintOf(css), blend);
    this.layer(layer).addChild(dot.s);
    return dot;
  }

  sim(behavior: ParticleBehavior, layer: Layer = 'front') {
    const sim = new ParticleSim(behavior);
    this.layer(layer).addChild(sim.container);
    this.sims.push(sim);
    return sim;
  }

  strand(points: number, width: number, css: string, alpha = 1, layer: Layer = 'front') {
    const strand = new Strand(points, width, tintOf(css), alpha);
    strand.rope.visible = false;
    this.layer(layer).addChild(strand.rope);
    return strand;
  }

  gfx(layer: Layer = 'front', blend: BLEND_MODES = 'add') {
    const g = new Graphics();
    g.blendMode = blend;
    this.layer(layer).addChild(g);
    return g;
  }

  box(layer: Layer = 'front', blend?: BLEND_MODES) {
    const box = new Container();
    if (blend) box.blendMode = blend;
    this.layer(layer).addChild(box);
    return box;
  }

  // Пока время в окне [start, start + duration] — run(прогресс 0..1); после окна hide прячутся
  tween(start: number, duration: number, run: (p: number) => void, hide: Dot[] = []) {
    this.tweens.push({ start, duration, run, hide, state: 'wait' });
  }

  // Один раз в момент time
  at(time: number, run: () => void) {
    this.events.push({ time, run, done: false });
  }

  step(t: number, dt: number) {
    this.dt = dt;
    for (const event of this.events) {
      if (event.done || t < event.time) continue;
      event.done = true;
      event.run();
    }
    for (const tween of this.tweens) {
      if (tween.state === 'done' || t < tween.start) continue;
      const p = clamp01((t - tween.start) / tween.duration);
      tween.run(p);
      tween.state = 'run';
      if (p >= 1) {
        tween.state = 'done';
        tween.hide.forEach(dot => dot.hide());
      }
    }
    for (const sim of this.sims) sim.step(dt);
    for (const child of this.children) {
      if (t < child.offset) continue;
      child.fx.step(t - child.offset, dt);
      child.update?.(t - child.offset, dt);
    }
  }

  private disposeSims() {
    this.sims.forEach(sim => sim.destroy());
    this.sims.length = 0;
    this.children.forEach(child => child.fx.disposeSims());
  }

  dispose() {
    this.disposeSims();
    this.stage.front.removeChildren().forEach(child => child.destroy({ children: true }));
    this.stage.back.removeChildren().forEach(child => child.destroy({ children: true }));
  }
}

// ===== Готовые куски =====

// n точек вдоль кривой f(0..1)
export const path = (n: number, f: (t: number) => Pt): Pt[] => Array.from({ length: n }, (_, i) => f(i / (n - 1)));
// Ломаная молнии из n точек
export const jag = (a: Pt, b: Pt, n: number, jitter: number): Pt[] => jaggedPath(a.x, a.y, b.x, b.y, n - 1, jitter);

// Светящаяся линия из трёх нитей: ореол, тело и белая сердцевина. Точек в пути должно быть ровно points
export class Beam {
  private readonly layers: [Strand, number][];

  constructor(fx: Fx, points: number, width: number, css: string, layer: Layer = 'front', core = '#ffffff') {
    this.layers = [
      [fx.strand(points, width * 2.8, css, 1, layer), 0.16],
      [fx.strand(points, width, css, 1, layer), 1],
      [fx.strand(points, width * 0.35, core, 1, layer), 1],
    ];
  }

  set(points: Pt[], alpha = 1) {
    for (const [strand, share] of this.layers) {
      strand.rope.visible = alpha > 0.003;
      strand.rope.alpha = clamp01(alpha) * share;
      if (strand.rope.visible) strand.set(points);
    }
  }

  hide() {
    this.layers.forEach(([strand]) => (strand.rope.visible = false));
  }
}

// Залитая лента вдоль пути, толщина сходит на нет к обоим концам (коготь, разрез, росчерк)
export const taper = (g: Graphics, points: Pt[], width: number, css: string, alpha = 1) => {
  if (points.length < 2) return g;
  const left: number[] = [];
  const right: number[] = [];
  points.forEach((p, i) => {
    const a = points[Math.max(i - 1, 0)];
    const b = points[Math.min(i + 1, points.length - 1)];
    const n = Math.atan2(b.y - a.y, b.x - a.x) + Math.PI / 2;
    const w = (width / 2) * Math.sin(Math.PI * (i / (points.length - 1)));
    left.push(p.x + Math.cos(n) * w, p.y + Math.sin(n) * w);
    right.push(p.x - Math.cos(n) * w, p.y - Math.sin(n) * w);
  });
  const back: number[] = [];
  for (let i = right.length - 2; i >= 0; i -= 2) back.push(right[i], right[i + 1]);
  return g.poly([...left, ...back]).fill({ color: tintOf(css), alpha });
};

// ===== Отметка 3-го уровня =====
// У каждой стихии свой акцент на цели в момент удара — по нему с первого взгляда видно максимальную прокачку.
// Хореография и число ударов от уровня не зависят, меняется только подача
export type Element = 'fire' | 'frost' | 'bolt' | 'nature' | 'dark' | 'blood' | 'light' | 'arcane';

const isUnit = (place: ConceptUnit | Pt): place is ConceptUnit => 'feet' in place;

// place — юнит (акцент обвивает тело) или точка на земле; scale — размер относительно обычного юнита
export const levelAccent = (fx: Fx, element: Element, at: number, place: ConceptUnit | Pt, scale = 1) => {
  const feet = isUnit(place) ? place.feet : place;
  const center = isUnit(place) ? place.center : { x: feet.x, y: feet.y - 110 * scale };
  const head = isUnit(place) ? place.head : { x: feet.x, y: feet.y - 210 * scale };
  const body = Math.max(60, feet.y - head.y);
  const k = scale * fx.density;
  ACCENTS[element](fx, at, { feet, center, head, body, scale, k });
};

interface AccentPlace {
  feet: Pt;
  center: Pt;
  head: Pt;
  body: number;
  scale: number;
  // Густота частиц: меньше, когда акцент идёт сразу на многих целях
  k: number;
}

// Огонь: раскалённое добела ядро в теле и кольцо тлеющих углей вокруг ног, угли взлетают искрами
const fireAccent = (fx: Fx, at: number, o: AccentPlace) => {
  const white = fx.dot('glow', '#ffffff');
  const hot = fx.dot('glow', '#ffe27a');
  fx.tween(at, 0.55, p => {
    const r = 60 * o.scale * (0.4 + 0.6 * ease.out3(Math.min(p * 3, 1)));
    hot.put(o.center.x, o.center.y, r * 1.9, (1 - ease.in(p)) * 0.8);
    white.put(o.center.x, o.center.y, r * (1 - 0.4 * p), 1 - ease.in(p));
  }, [white, hot]);
  const coals = Array.from({ length: 14 }, (_, i) => ({ dot: fx.dot('glow', i % 3 ? '#ff7a1a' : '#ffd166', i % 2 ? 'back' : 'front'), a: (i / 14) * TAU + rand(-0.15, 0.15), r: rand(0.85, 1.1) }));
  const R = 95 * o.scale;
  fx.tween(at, 1.3, p => {
    const grow = ease.out3(Math.min(p * 3, 1));
    for (const coal of coals) {
      const pt = polar(o.feet, R * coal.r * grow, coal.a + p * 0.6, GROUND);
      const behind = Math.sin(coal.a + p * 0.6) < 0;
      coal.dot.put(pt.x, pt.y, 12 * o.scale * (0.8 + 0.3 * Math.sin(p * 30 + coal.a * 5)), fade(p, 0.05, 0.4) * (behind ? 0.7 : 1));
    }
  }, coals.map(c => c.dot));
  const ring = fx.dot('ring', '#ff7a1a', 'back');
  fx.tween(at, 1.3, p => ring.oval(o.feet.x, o.feet.y, R * ease.out3(Math.min(p * 3, 1)), R * GROUND * ease.out3(Math.min(p * 3, 1)), fade(p, 0.05, 0.4) * 0.8), [ring]);
  const embers = fx.sim(P.ember(['#fffbe0', '#ffd166', '#ff7a1a', '#e2401b'], 160));
  fx.tween(at + 0.1, 1.1, p => {
    for (let i = fx.count(40 * o.k * (1 - p)); i > 0; i--) {
      const pt = polar(o.feet, R * rand(0.85, 1.1), Math.random() * TAU, GROUND);
      embers.spawn({ x: pt.x, y: pt.y, vx: rand(-20, 20), vy: rand(-180, -80), lifetime: rand(0.5, 0.9), size: rand(4, 8) * o.scale });
    }
  });
};

// Лёд: иней расползается по земле ветвистыми узорами, по телу и вокруг мерцают блёстки
const frostAccent = (fx: Fx, at: number, o: AccentPlace) => {
  const rime = fx.gfx('back', 'add');
  const branches = Array.from({ length: 9 }, (_, i) => ({ a: (i / 9) * TAU + rand(-0.2, 0.2), len: rand(80, 130) * o.scale, seed: rand(0, 10) }));
  fx.tween(at, 1.5, p => {
    const grow = ease.out3(Math.min(p * 2.2, 1));
    const a = fade(p, 0.04, 0.35);
    rime.clear();
    for (const b of branches) {
      const end = polar(o.feet, b.len * grow, b.a, GROUND);
      rime.moveTo(o.feet.x, o.feet.y).lineTo(end.x, end.y).stroke({ width: 2.5, color: color('#e0f6ff'), alpha: 0.85 * a });
      for (const t of [0.4, 0.7]) {
        const m = polar(o.feet, b.len * grow * t, b.a, GROUND);
        for (const side of [-1, 1]) {
          const e = polar(m, b.len * grow * 0.22, b.a + side * 0.7, GROUND);
          rime.moveTo(m.x, m.y).lineTo(e.x, e.y).stroke({ width: 1.5, color: color('#bae6fd'), alpha: 0.75 * a });
        }
      }
    }
  });
  const sheen = fx.radial([[0, 0.55], [0.6, 0.3], [1, 0]], '#bae6fd', 'back');
  fx.tween(at, 1.5, p => sheen.oval(o.feet.x, o.feet.y, 130 * o.scale * ease.out3(Math.min(p * 2.2, 1)), 130 * o.scale * GROUND, fade(p, 0.04, 0.35) * 0.7), [sheen]);
  const glitter = fx.sim({ texture: 'star', drag: 0.6, gravity: -20, shrink: 0.6, tint: palette(['#ffffff', '#e0f6ff', '#7dd3fc']), alpha: life => bump(life) });
  fx.tween(at, 1.2, p => {
    for (let i = fx.count(36 * o.k * (1 - p * 0.6)); i > 0; i--) {
      glitter.spawn({ x: o.center.x + rand(-55, 55) * o.scale, y: rand(o.head.y - 20, o.feet.y), vx: 0, vy: rand(-30, 0), lifetime: rand(0.35, 0.7), size: rand(6, 13) * o.scale, rotation: rand(0, TAU) });
    }
  });
};

// Молния: дуги ползут по телу снизу вверх и перескакивают с боку на бок
const boltAccent = (fx: Fx, at: number, o: AccentPlace) => {
  const sparks = fx.sim(P.spark(['#ffffff', '#fff7d6', '#fde68a', '#ffb300'], 300));
  const W = 42 * o.scale;
  for (let n = 0; n < 4; n++) {
    const beam = new Beam(fx, 8, 3.5, '#fde68a');
    const start = at + n * 0.14;
    crackle(fx, beam, start, 0.42, () => {
      const y0 = o.feet.y - rand(0, o.body * 0.5);
      const y1 = y0 - rand(o.body * 0.3, o.body * 0.6);
      return jag({ x: o.center.x + rand(-W, W), y: y0 }, { x: o.center.x + rand(-W, W), y: Math.max(o.head.y, y1) }, 8, 14 * o.scale);
    });
    fx.at(start, () => spray(sparks, { x: o.center.x + rand(-W, W), y: o.center.y + rand(-o.body * 0.3, o.body * 0.3), count: Math.ceil(8 * o.k), speed: [80, 260], life: [0.2, 0.45], size: [2, 5] }));
  }
  const glow = fx.dot('glow', '#fde68a');
  fx.tween(at, 0.7, p => glow.oval(o.center.x, o.center.y, 70 * o.scale, o.body * 0.6, (0.35 + 0.25 * Math.sin(p * 50)) * (1 - p)), [glow]);
};

// Природа: вокруг ног распускается венок листьев, вверх по спирали поднимаются споры
const natureAccent = (fx: Fx, at: number, o: AccentPlace) => {
  const leaf = leafTexture();
  const leaves = Array.from({ length: 10 }, (_, i) => ({ dot: fx.tex(leaf, Math.sin((i / 10) * TAU) < 0 ? 'back' : 'front'), a: (i / 10) * TAU }));
  const R = 85 * o.scale;
  fx.tween(at, 1.3, p => {
    const open = ease.back(Math.min(p * 3, 1));
    for (const l of leaves) {
      const pt = polar(o.feet, R * open, l.a + p * 0.4, GROUND);
      l.dot.put(pt.x, pt.y - 6, 16 * o.scale * open, fade(p, 0.05, 0.35), l.a + Math.PI / 2 + p * 0.4);
    }
  }, leaves.map(l => l.dot));
  const spores = fx.sim(P.glow(['#f7fee7', '#bef264', '#4ade80'], 90));
  fx.tween(at, 1.1, p => {
    for (let i = fx.count(30 * o.k * (1 - p)); i > 0; i--) {
      const a = p * TAU * 2 + rand(-0.3, 0.3);
      const pt = polar(o.feet, R * 0.7, a, GROUND);
      spores.spawn({ x: pt.x, y: pt.y - rand(0, o.body * 0.8), vx: -Math.sin(a) * 50, vy: rand(-90, -40), lifetime: rand(0.6, 1), size: rand(7, 13) * o.scale });
    }
  });
};

// Тьма: тёмный ореол вокруг тела, дымные завитки поднимаются по спирали
const darkAccent = (fx: Fx, at: number, o: AccentPlace) => {
  const halo = fx.dot('glow', '#1e0b36', 'back', 'normal');
  const rim = fx.dot('ring', '#a855f7', 'back');
  fx.tween(at, 1.4, p => {
    const a = fade(p, 0.08, 0.4);
    const s = 0.8 + 0.2 * ease.out(p);
    halo.oval(o.center.x, o.center.y, 110 * o.scale * s, o.body * 0.75 * s, 0.75 * a);
    rim.oval(o.center.x, o.center.y, 95 * o.scale * s, o.body * 0.65 * s, 0.5 * a);
  }, [halo, rim]);
  const curls = fx.sim({ texture: 'smoke', blend: 'normal', drag: 0.4, gravity: -50, shrink: 1.2, tint: palette(['#1e0b36', '#2e1065', '#3b0764']), alpha: life => bump(life) * 0.6 });
  const wisps = fx.sim(P.glow(['#f5d0fe', '#c084fc', '#9333ea'], 60));
  for (let n = 0; n < 3; n++) {
    const phase = (n / 3) * TAU;
    fx.tween(at + n * 0.08, 1.1, p => {
      const a = phase + p * TAU * 1.3;
      const pt = polar({ x: o.center.x, y: o.feet.y - p * o.body * 1.1 }, 60 * o.scale * (1 - p * 0.4), a, 0.4);
      spray(curls, { x: pt.x, y: pt.y, count: fx.count(26 * o.k), speed: [5, 20], life: [0.5, 0.8], size: [18, 30], jitter: 4 });
      spray(wisps, { x: pt.x, y: pt.y, count: fx.count(10 * o.k), speed: [5, 25], life: [0.3, 0.6], size: [6, 10] });
    });
  }
};

// Кровь: брызги разлетаются из тела, вокруг дважды бьётся красный пульс, как сердце
const bloodAccent = (fx: Fx, at: number, o: AccentPlace) => {
  const drops = fx.sim({ texture: dropTexture('blood'), blend: 'normal', drag: 0.4, gravity: 1100, shrink: 0.75 });
  fx.at(at, () => spray(drops, { x: o.center.x, y: o.center.y, count: Math.ceil(26 * o.k), speed: [220, 620], dir: -Math.PI / 2, spread: 1.5, life: [0.5, 0.9], size: [6, 11], align: true }));
  for (const [i, beat] of [0, 0.32].entries()) {
    const pulse = fx.dot('ring', '#ef4444');
    const glow = fx.dot('glow', '#dc2626');
    fx.tween(at + beat, 0.45, p => {
      const s = 0.7 + 0.5 * ease.out3(p);
      pulse.oval(o.center.x, o.center.y, 80 * o.scale * s, o.body * 0.6 * s, (1 - p) * 0.9);
      glow.oval(o.center.x, o.center.y, 90 * o.scale, o.body * 0.65, bump(p) * (i ? 0.45 : 0.6));
    }, [pulse, glow]);
    ringWave(fx, { at: at + beat, dur: 0.5, x: o.feet.x, y: o.feet.y, from: 30 * o.scale, to: 140 * o.scale, css: '#f87171', squash: GROUND, layer: 'back', alpha: 0.8 });
  }
  const stain = fx.radial([[0, 0.85], [0.7, 0.6], [1, 0]], '#7f1d1d', 'back', 'normal');
  fx.tween(at + 0.15, 1.3, p => stain.oval(o.feet.x, o.feet.y, 90 * o.scale * ease.out(Math.min(p * 2, 1)), 90 * o.scale * GROUND, fade(p, 0.1, 0.4) * 0.75), [stain]);
};

// Свет и тайная магия: над головой загорается нимб-ореол, из-за головы расходятся лучи
const haloAccent = (colors: { ring: string; glow: string; ray: string }) => (fx: Fx, at: number, o: AccentPlace) => {
  const nimbus = fx.dot('ring', colors.ring);
  const nimbusGlow = fx.dot('glow', colors.glow);
  const rays = Array.from({ length: 8 }, () => new Beam(fx, 2, 4 * o.scale, colors.ray));
  const top = { x: o.head.x, y: o.head.y - 14 * o.scale };
  fx.tween(at, 1.3, p => {
    const a = fade(p, 0.1, 0.4);
    const y = top.y - 8 * ease.out(p);
    nimbus.oval(top.x, y, 34 * o.scale, 10 * o.scale, a);
    nimbusGlow.oval(top.x, y, 52 * o.scale, 20 * o.scale, 0.55 * a);
    rays.forEach((ray, i) => {
      const ang = -Math.PI / 2 + (i - 3.5) * 0.36 + p * 0.15;
      const len = (i % 2 ? 90 : 140) * o.scale * ease.out3(Math.min(p * 2.5, 1));
      ray.set([polar(o.center, 30 * o.scale, ang), polar(o.center, 30 * o.scale + len, ang)], 0.7 * a);
    });
    if (p >= 1) rays.forEach(ray => ray.hide());
  }, [nimbus, nimbusGlow]);
  const motes = fx.sim({ texture: 'star', drag: 0.4, gravity: -90, shrink: 0.6, tint: palette([colors.ring, colors.glow, '#ffffff']), alpha: life => bump(life) });
  fx.at(at, () => spray(motes, { x: o.center.x, y: o.center.y, count: Math.ceil(16 * o.k), speed: [40, 180], life: [0.5, 1], size: [5, 10], jitter: 30 }));
};

const ACCENTS: Record<Element, (fx: Fx, at: number, o: AccentPlace) => void> = {
  fire: fireAccent,
  frost: frostAccent,
  bolt: boltAccent,
  nature: natureAccent,
  dark: darkAccent,
  blood: bloodAccent,
  light: haloAccent({ ring: '#fde68a', glow: '#fbbf24', ray: '#fffbeb' }),
  arcane: haloAccent({ ring: '#c4b5fd', glow: '#8b5cf6', ray: '#ede9fe' }),
};

// Лента вдоль пути со своей шириной в каждой точке; shift сдвигает её поперёк (доля ширины) — для бликов и теней на боку
export const ribbon = (g: Graphics, points: Pt[], widths: number[], css: string, alpha = 1, shift = 0) => {
  if (points.length < 2) return g;
  const left: number[] = [];
  const right: number[] = [];
  points.forEach((p, i) => {
    const a = points[Math.max(i - 1, 0)];
    const b = points[Math.min(i + 1, points.length - 1)];
    const n = Math.atan2(b.y - a.y, b.x - a.x) + Math.PI / 2;
    const w = widths[i] / 2;
    const o = shift * widths[i];
    left.push(p.x + Math.cos(n) * (w + o), p.y + Math.sin(n) * (w + o));
    right.push(p.x - Math.cos(n) * (w - o), p.y - Math.sin(n) * (w - o));
  });
  const back: number[] = [];
  for (let i = right.length - 2; i >= 0; i -= 2) back.push(right[i], right[i + 1]);
  return g.poly([...left, ...back]).fill({ color: tintOf(css), alpha });
};

// Злобный глаз: верхняя кромка прямая и опускается к переносице, нижняя — дугой; сам глаз белый, цвет — только
// в ореоле вокруг. (x, y) — центр глаза, side = −1 левый / +1 правый, w — полуширина, h — высота, open — 0…1 (прищур, моргание)
export const angryEye = (g: Graphics, x: number, y: number, side: number, w: number, h: number, open: number, css: string, alpha = 1) => {
  if (open <= 0.02 || alpha <= 0.01) return g;
  const shape = (k: number) => {
    const W = w * k;
    const H = h * open * k;
    const inner = x - side * W;
    const outer = x + side * W;
    const points = [inner, y + H * 0.15, outer, y - H * 0.55];
    for (let i = 1; i < 10; i++) {
      const t = i / 10;
      points.push(lerp(outer, inner, t), lerp(y - H * 0.55, y + H * 0.15, t) + Math.sin(t * Math.PI) * H);
    }
    return points;
  };
  g.poly(shape(1.45)).fill({ color: tintOf(css), alpha: 0.35 * alpha });
  g.poly(shape(1)).fill({ color: 0xffffff, alpha });
  return g;
};

// Непрозрачный жгут (корень, лоза, нить): тело и светлая полоска по нему, обычное наложение
export class Cord {
  private readonly layers: Strand[];

  constructor(fx: Fx, points: number, width: number, css: string, light: string, layer: Layer = 'front') {
    this.layers = [fx.strand(points, width, css, 1, layer), fx.strand(points, width * 0.35, light, 1, layer)];
    this.layers.forEach(strand => (strand.rope.blendMode = 'normal'));
  }

  set(points: Pt[], alpha = 1) {
    for (const strand of this.layers) {
      strand.rope.visible = alpha > 0.003;
      strand.rope.alpha = clamp01(alpha);
      if (strand.rope.visible) strand.set(points);
    }
  }

  hide() {
    this.layers.forEach(strand => (strand.rope.visible = false));
  }
}

// Полёт: в окне времени каждый кадр даёт точку траектории pos(p), прогресс и направление движения
export const fly = (fx: Fx, at: number, dur: number, pos: (p: number) => Pt, each: (pt: Pt, p: number, dir: number) => void, hide: Dot[] = []) => {
  fx.tween(at, dur, p => {
    const pt = pos(p);
    const next = pos(Math.min(p + 0.02, 1));
    const prev = pos(Math.max(p - 0.02, 0));
    each(pt, p, Math.atan2(next.y - prev.y, next.x - prev.x));
  }, hide);
};

// Треск разряда: линия перестраивается каждые 50 мс, пока идёт окно времени
export const crackle = (fx: Fx, beam: Beam, at: number, dur: number, make: () => Pt[], alpha: (p: number) => number = p => fade(p, 0.05, 0.45)) => {
  let next = 0;
  let points = make();
  fx.tween(at, dur, p => {
    const elapsed = p * dur;
    if (elapsed >= next) {
      points = make();
      next = elapsed + 0.05;
    }
    if (p >= 1) beam.hide();
    else beam.set(points, alpha(p));
  });
};

// Расходящееся кольцо (ударная волна); squash < 1 — по земле
export const ringWave = (fx: Fx, o: { at: number; dur: number; x: number; y: number; from: number; to: number; css: string; squash?: number; layer?: Layer; alpha?: number }) => {
  const ring = fx.dot('ring', o.css, o.layer ?? 'front');
  fx.tween(o.at, o.dur, p => {
    const r = lerp(o.from, o.to, ease.out3(p));
    ring.oval(o.x, o.y, r, r * (o.squash ?? 1), (1 - p) * (o.alpha ?? 1));
  }, [ring]);
};

// Вспышка: мягкий круг раздувается и гаснет
export const pop = (fx: Fx, o: { at: number; dur: number; x: number; y: number; r: number; css: string; layer?: Layer; alpha?: number; squash?: number }) => {
  const glow = fx.dot('glow', o.css, o.layer ?? 'front');
  fx.tween(o.at, o.dur, p => {
    const r = o.r * (0.35 + 0.65 * ease.out3(p));
    glow.oval(o.x, o.y, r, r * (o.squash ?? 1), (1 - ease.in(p)) * (o.alpha ?? 1));
  }, [glow]);
};

// Пятно света на земле под точкой: появляется, держится и гаснет
export const groundGlow = (fx: Fx, o: { at: number; dur: number; x: number; y: number; r: number; css: string; alpha?: number }) => {
  const glow = fx.radial([[0, 0.7], [0.5, 0.35], [1, 0]], o.css, 'back');
  fx.tween(o.at, o.dur, p => glow.oval(o.x, o.y, o.r, o.r * GROUND, fade(p, 0.1, 0.5) * (o.alpha ?? 1)), [glow]);
};

// Поведение частиц: цвета — палитра от свежей к угасающей
export const P = {
  spark: (colors: string[], gravity = 500): ParticleBehavior => ({ texture: 'dot', drag: 0.08, gravity, shrink: 0.4, tint: palette(colors) }),
  ember: (colors: string[], rise = 140): ParticleBehavior => ({ texture: 'disc', drag: 0.15, gravity: -rise, shrink: 0.12, tint: palette(colors), alpha: life => life * 0.85 }),
  streak: (colors: string[], gravity = 0): ParticleBehavior => ({ texture: 'disc', drag: 0.1, gravity, shrink: 0.3, stretch: [3.2, 0.3], tint: palette(colors) }),
  shard: (colors: string[], gravity = 900): ParticleBehavior => ({ texture: 'shard', drag: 0.3, gravity, shrink: 0.6, tint: palette(colors) }),
  debris: (css: string, gravity = 1400): ParticleBehavior => ({ texture: 'debris', blend: 'normal', drag: 0.5, gravity, shrink: 0.7, tint: () => tintOf(css), alpha: life => Math.min(1, life * 3) }),
  smoke: (css: string, rise = 40): ParticleBehavior => ({ texture: 'smoke', blend: 'normal', drag: 0.1, gravity: -rise, shrink: 1.9, tint: () => tintOf(css), alpha: life => bump(life) * 0.45 }),
  glow: (colors: string[], rise = 0): ParticleBehavior => ({ texture: 'glow', drag: 0.2, gravity: -rise, shrink: 0.5, tint: palette(colors), alpha: life => bump(life) }),
  star: (colors: string[], rise = 60): ParticleBehavior => ({ texture: 'star', drag: 0.1, gravity: -rise, shrink: 0.5, tint: palette(colors), alpha: life => bump(life) }),
  plus: (colors: string[], rise = 90): ParticleBehavior => ({ texture: 'plus', drag: 0.3, gravity: -rise, shrink: 0.7, tint: palette(colors), alpha: life => bump(life) }),
};

// Палитры стихий: от раскалённого к тёмному
export const C = {
  fire: ['#fffbe0', '#ffe27a', '#ffb347', '#ff7a1a', '#e2401b', '#8a1f0f'],
  ice: ['#ffffff', '#e0f6ff', '#bae6fd', '#7dd3fc', '#38bdf8', '#1e40af'],
  bolt: ['#ffffff', '#fff7d6', '#fde68a', '#ffb300', '#b45309'],
  nature: ['#f7fee7', '#bef264', '#4ade80', '#16a34a', '#14532d'],
  venom: ['#ecfccb', '#a3e635', '#65a30d', '#3f6212'],
  dark: ['#f5d0fe', '#c084fc', '#9333ea', '#581c87', '#1e0b36'],
  blood: ['#fecaca', '#f87171', '#dc2626', '#991b1b', '#450a0a'],
  light: ['#ffffff', '#fffbeb', '#fde68a', '#fbbf24', '#f59e0b'],
  arcane: ['#ffffff', '#e9d5ff', '#a78bfa', '#6366f1', '#312e81'],
};

export { rand, clamp01, Strand };
export type { ParticleSim };

// Уровень прокачки заклинания: на 2-м спел заметно отличается от 1-го, на 3-м — эффекты максимальной прокачки
export type SpellLevel = 1 | 2 | 3;

export type Frame = (t: number, dt: number) => void;

export interface ArtifactTrigger {
  kind: string;
  spell: string;
}

// Сколько целей у заклинания на уровнях 1…3 (как на сервере); 'all' — все враги (или весь отряд для лечения)
export type TargetCount = number | 'all';
export type TargetRule = [TargetCount, TargetCount, TargetCount];

// Артефакт на странице концептов: что он добавляет к заклинанию.
// kind — вид эффекта (ItemEffectInfo.Kind); then — срабатывание отдельным кастом другого (или того же) заклинания
export interface ArtifactDemo {
  key: string;
  name: string;
  about: string;
  kind: string;
  then?: {
    // Концепт срабатывания; несколько — выбирается случайный (пул заклинаний «Призменной линзы»)
    concept: string | string[];
    level: SpellLevel;
    // В кого срабатывание: те же цели, соседи главной цели, одна случайная, лечение союзников
    targets: 'same' | 'neighbours' | 'one' | 'all' | 'allies';
    // Срабатывание идёт от главной цели (раскол, заражение) или от лечённого союзника
    origin?: 'target' | 'healed';
    // Когда начинается, доля длительности основного каста
    after?: number;
  };
  // Сколько союзников лечит заклинание с этим артефактом (лечение внутри каста, без отдельного срабатывания)
  heals?: TargetCount;
}

export interface Concept {
  id: string;
  name: string;
  // Концепт меняется по уровню (fx.level): на странице показывается переключатель уровней
  levels?: boolean;
  // Число целей и лечимых по уровням — как в механике заклинания (страница концептов собирает по нему сцену)
  targets?: TargetRule;
  heals?: TargetRule;
  // Есть доп. эффекты (fx.extras) — на странице показывается переключатель
  extras?: boolean;
  // Артефакты этого заклинания и их анимации
  artifacts?: ArtifactDemo[];
  // Стихия (группа в списке) и заклинание, под которое концепт придуман
  group: string;
  spell: string;
  about: string;
  // секунды
  duration: number;
  // Строит сцену; может вернуть покадровую функцию (t — секунды от начала, dt — шаг)
  build(fx: Fx): void | Frame;
}

// Одиночная анимация по каждой цели: копии идут с разбегом stagger, у каждой своя цель в enemies[0].
// Кастер взмахивает один раз, доп. эффекты — только у первой копии; при многих целях рисуется реже (fx.density)
export const perTarget = (build: (fx: Fx) => void | Frame, stagger = 0.16, key: 'enemies' | 'healed' = 'enemies') => (fx: Fx): void => {
  const targets = fx.stage[key];
  if (targets.length <= 1) {
    const update = build(fx);
    if (update) fx.fork(0, {}, () => update);
    return;
  }
  fx.density = Math.max(0.45, 1 / Math.sqrt(targets.length));
  fx.tail = (targets.length - 1) * stagger;
  const extras = fx.extras;
  targets.forEach((target, i) => {
    fx.extras = extras && i === 0;
    fx.fork(i * stagger, { [key]: [target], cast: i === 0 ? fx.stage.cast : () => undefined }, build);
  });
  fx.extras = extras;
};

// Сколько целей по правилу при доступных available
export const countFor = (rule: TargetRule | undefined, level: SpellLevel, available: number) => {
  const value = rule ? rule[level - 1] : 'all';
  return value === 'all' ? available : Math.min(value, available);
};
