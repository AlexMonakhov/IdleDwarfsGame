import type { InjectionKey } from 'vue';
import { GlobalParticleService } from '@/services/particleService/ParticleSystem';
import { Point } from '@/services/particleService/types/interfaces/canvas-interfaces';
import {
  Anchor,
  AuraVisual,
  CRIT_FLARE,
  DAMAGE_COLORS,
  FxSpec,
  HEAL_VISUAL,
  HIT_VISUALS,
  STATUS_VISUALS,
  SpellVisual,
  UNIT_ANCHORS,
  UNIT_FACE_SHIFT,
} from './FxRegistry';

export { DAMAGE_COLORS };

export const BATTLE_FX_KEY: InjectionKey<BattleFx> = Symbol('battleFx');

type UnitEl = HTMLElement | null | undefined;

export interface HitOptions {
  attacker?: UnitEl;
  crit?: boolean;
}

const wait = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));
let emitterSequence = 0;

// На сколько волна заклинания уходит дальше самой дальней цели, px
const WAVE_OVERSHOOT = 60;

// Сколько живёт вспышка света в точке попадания, мс при x1
const FLARE_DURATION = 520;

// Одноразовые и постоянные боевые эффекты поверх particle-сервиса арены
export class BattleFx {
  // Вспышки света при попаданиях (выключаются в нагрузочном тесте, чтобы сравнить FPS)
  flares = true;

  constructor(
    private readonly service: () => GlobalParticleService | null,
    private readonly speed: () => number,
  ) {}

  // Вспышка света на юните: пятно цвета эффекта и отсвет на земле под ним
  flare(el: UnitEl, color: string, radius: number, intensity = 1) {
    if (!this.flares) return;
    this.play([{ type: 'flare', duration: FLARE_DURATION, padding: radius, args: { color, radius, intensity } }], el, { ground: this.groundOffset(el) });
  }

  // Точка на спрайте юнита в координатах канваса нужного слоя
  point(el: UnitEl, anchor: Anchor = 'center', layer: FxSpec['layer'] = 'foreground'): Point | null {
    const canvas = this.service()?.getCanvas(layer ?? 'foreground');
    if (!el || !canvas || !el.isConnected) return null;
    const rect = el.getBoundingClientRect();
    const canvasRect = canvas.getBoundingClientRect();
    // Из атрибута, а не getComputedStyle: точку следующих эффектов считают каждый кадр, а стили — дорого
    const face = Number(el.dataset.face) || 1;
    return {
      x: rect.left + rect.width * (0.5 + UNIT_FACE_SHIFT * face) - canvasRect.left,
      y: rect.top + rect.height * UNIT_ANCHORS[anchor] - canvasRect.top,
    };
  }

  // Запустить одноразовый эффект от from к to (для неподвижных эффектов to = from)
  private emit(spec: FxSpec, from: Point, to: Point = from, extraArgs?: Record<string, unknown>) {
    const service = this.service();
    if (!service) return;

    const start = () => {
      const id = `fx-${spec.type}-${++emitterSequence}`;
      const lifetime = spec.duration / this.speed();
      service.addEmitter({
        id,
        type: spec.type,
        x: from.x,
        y: from.y,
        startX: from.x,
        startY: from.y,
        endX: to.x,
        endY: to.y,
        padding: spec.padding ?? 40,
        layer: spec.layer ?? 'foreground',
        args: { ...spec.args, ...extraArgs },
        travelStartedAt: performance.now(),
        travelDuration: lifetime,
        instant: true,
      });
      setTimeout(() => service.removeEmitter(id, 150), lifetime);
    };

    if (spec.delay) setTimeout(start, spec.delay / this.speed());
    else start();
  }

  // Проиграть набор эффектов на юните
  play(specs: FxSpec[] | undefined, el: UnitEl, extraArgs?: Record<string, unknown>) {
    for (const spec of specs ?? []) {
      const at = this.point(el, spec.anchor, spec.layer);
      if (at) this.emit(spec, at, at, extraArgs);
    }
  }

  // Попадание: росчерк/искры по типу урона, направленные от атакующего
  hit(target: UnitEl, damageType = 'Physical', options: HitOptions = {}) {
    const from = this.point(options.attacker);
    const to = this.point(target);
    const direction = from && to && from.x > to.x ? -1 : 1;
    this.play(HIT_VISUALS[damageType] ?? HIT_VISUALS.Physical, target, {
      direction,
      ...(options.crit ? { count: 2, radius: 62, width: 16 } : {}),
    });
    if (options.crit) this.flare(target, DAMAGE_COLORS[damageType] ?? DAMAGE_COLORS.Physical, CRIT_FLARE);
  }

  heal(target: UnitEl) {
    this.play(HEAL_VISUAL, target, { ground: this.groundOffset(target) });
  }

  // Магический круг под кастером на время подготовки заклинания
  castCircle(caster: UnitEl, color: string, duration: number) {
    this.play([{ type: 'ring', anchor: 'feet', layer: 'background', duration, args: { color, radius: 58, mode: 'rune', width: 3 } }], caster);
  }

  // Заклинание по набору целей (визуал уже выбран по имени и уровню — resolveSpellVisual):
  // эффекты каста, доставка к каждой цели (снаряд, луч, волна или ничего) и взрывы на них.
  // Возвращает по промису на цель — он резолвится в момент попадания по ней
  spell(caster: UnitEl, targets: UnitEl[], visual: SpellVisual): Promise<void>[] {
    const from = this.point(caster);
    const points = targets.map(el => this.point(el));
    if (!from) return targets.map(() => Promise.resolve());

    const order = this.hitOrder(visual, from, points);
    const present = points.filter((p): p is Point => !!p);
    // Сторона, куда бьёт кастер: по ней дует ветер бурана и катится волна
    const side = present.length && present.reduce((sum, p) => sum + p.x, 0) / present.length < from.x ? -1 : 1;
    const reach = Math.max(0, ...present.map(p => Math.abs(p.x - from.x))) + WAVE_OVERSHOOT;
    const extra = { direction: side, groundOffset: this.groundOffset(caster) };

    for (const spec of visual.cast ?? []) {
      if (spec.at === 'caster') this.play([spec], caster, extra);
      else this.playOverGroup(spec, targets, extra);
    }

    if (visual.wave) {
      const start = this.point(caster, visual.wave.anchor, visual.wave.layer);
      if (start) this.emit(visual.wave, start, { x: start.x + side * reach, y: start.y }, extra);
    }
    const waveTravel = visual.wave ? visual.wave.duration * Number(visual.wave.args?.travel ?? 1) : 0;

    return points.map((to, i) => {
      if (!to) return Promise.resolve();
      const rank = order.indexOf(i);
      const previous = visual.from === 'chain' && rank > 0 ? points[order[rank - 1]] : null;
      const startIn = visual.wave
        ? (waveTravel * Math.abs(to.x - from.x)) / reach
        : (visual.windup ?? 0) + rank * (visual.stagger ?? 0);
      return this.deliver(visual, from, previous ?? from, targets[i], startIn);
    });
  }

  // Порядок попаданий: цепь идёт от кастера к ближайшей цели и дальше к ближайшей к предыдущей,
  // остальные — по удалённости от кастера
  private hitOrder(visual: SpellVisual, from: Point, points: (Point | null)[]): number[] {
    const left = points.flatMap((p, i) => (p ? [{ i, p }] : []));
    if (visual.from !== 'chain') {
      return left.sort((a, b) => Math.abs(a.p.x - from.x) - Math.abs(b.p.x - from.x)).map(t => t.i);
    }

    const order: number[] = [];
    let at = from;
    while (left.length) {
      const distance = (p: Point) => Math.hypot(p.x - at.x, p.y - at.y);
      left.sort((a, b) => distance(a.p) - distance(b.p));
      const [next] = left.splice(0, 1);
      order.push(next.i);
      at = next.p;
    }
    return order;
  }

  // Доставка к одной цели: метка на цели, снаряд или луч, затем взрыв. Промис — момент попадания
  private async deliver(visual: SpellVisual, caster: Point, source: Point, target: UnitEl, startIn: number) {
    if (startIn > 0) await wait(startIn / this.speed());
    const to = this.point(target, (visual.beam ?? visual.projectile)?.anchor);
    if (!to) return;
    const direction = caster.x > to.x ? -1 : 1;
    this.play(visual.telegraph, target, { direction });

    const origin = visual.from === 'sky'
      ? { x: to.x - direction * (visual.sky?.dx ?? 0), y: to.y - (visual.sky?.height ?? 300) }
      : source;

    if (visual.beam) {
      this.emit(visual.beam, origin, to);
      await wait((visual.hitDelay ?? 0) / this.speed());
    } else if (visual.projectile) {
      await this.launch(visual, visual.projectile, origin, to);
    } else {
      await wait((visual.hitDelay ?? 0) / this.speed());
    }

    if (visual.flare) this.flare(target, visual.color, visual.flare);
    this.play(visual.impact, target, { direction });
  }

  // Снаряд или залп из нескольких; промис резолвится, когда долетел последний
  private async launch(visual: SpellVisual, spec: FxSpec, from: Point, to: Point) {
    const volley = visual.volley;
    if (!volley) {
      this.emit(spec, from, to);
      await wait(spec.duration / this.speed());
      return;
    }

    const spread = volley.spread ?? 0;
    const jitter = () => (Math.random() * 2 - 1) * spread;
    for (let i = 0; i < volley.count; i++) {
      this.later(i * volley.interval, () => {
        const start = { x: from.x + jitter(), y: from.y + jitter() };
        const end = { x: to.x + jitter(), y: to.y + jitter() };
        this.emit(spec, start, end, { arc: Number(spec.args?.arc ?? 0) * (0.6 + Math.random() * 0.8) });
        this.later(spec.duration, () => volley.hit?.forEach(hit => this.emit(hit, end)));
      });
    }
    await wait(((volley.count - 1) * volley.interval + spec.duration) / this.speed());
  }

  // Эффект над всей группой целей: по центру, шириной во всю группу (args.spread)
  private playOverGroup(spec: FxSpec, targets: UnitEl[], extra: Record<string, unknown>) {
    const points = targets.map(el => this.point(el, spec.anchor, spec.layer)).filter((p): p is Point => !!p);
    if (!points.length) return;
    const xs = points.map(p => p.x);
    const left = Math.min(...xs);
    const right = Math.max(...xs);
    const at = { x: (left + right) / 2, y: Math.min(...points.map(p => p.y)) };
    this.emit(spec, at, at, { ...extra, spread: right - left });
  }

  // От центра юнита до его ног, px: по нему эффекты на кастере находят землю
  private groundOffset(el: UnitEl) {
    const height = el?.getBoundingClientRect().height || 290;
    return height * (UNIT_ANCHORS.feet - UNIT_ANCHORS.center);
  }

  private later(ms: number, action: () => void) {
    if (ms > 0) setTimeout(action, ms / this.speed());
    else action();
  }

  // Тик статуса (горение и т.п.). Если для статуса нет своего визуала — обычное попадание по типу урона.
  statusTick(target: UnitEl, statusName: string, damageType?: string) {
    const tick = STATUS_VISUALS[statusName]?.tick;
    if (tick) this.play(tick, target);
    else this.hit(target, damageType);
  }

  // Постоянные эффекты статуса, следующие за юнитом. Возвращает id эмиттеров для stopAura.
  startAura(el: UnitEl, statusName: string): string[] {
    return this.attach(el, STATUS_VISUALS[statusName]?.aura, `aura-${statusName}`);
  }

  // Аура героя: пульсирующая сфера цвета его эффекта. Две половины — за юнитом и перед ним.
  startHeroAura(el: UnitEl, visual: AuraVisual): string[] {
    if (!el) return [];
    const args = {
      ...this.auraArgs(el, visual),
      // Случайный сдвиг фазы, чтобы ауры разных героев не пульсировали в унисон
      seed: Math.random() * visual.period * 1000,
    };
    return this.attach(el, [
      { type: 'aura-sphere', duration: 0, layer: 'background', padding: 120, args: { ...args, part: 'back' } },
      { type: 'aura-sphere', duration: 0, layer: 'foreground', padding: 120, args: { ...args, part: 'front' } },
    ], 'hero-aura');
  }

  // Срабатывание ауры: волна от носителя и круг по земле
  auraBurst(el: UnitEl, visual: AuraVisual) {
    if (!el) return;
    const args = this.auraArgs(el, visual);
    this.flare(el, visual.color, args.rx * 2.4, 0.75);
    this.play([
      { type: 'aura-burst', duration: 900, padding: 260, args: { ...args, colors: visual.palette, reach: args.rx * 3.6 } },
      { type: 'ring', anchor: 'feet', layer: 'background', duration: 800, args: { color: visual.color, radius: args.rx * 2.6, mode: 'pulse', width: 6 } },
    ], el);
  }

  stopAura(ids: string[]) {
    const service = this.service();
    ids.forEach(id => service?.removeEmitter(id, 300));
  }

  // Размеры сферы — от высоты кадра юнита (босс крупнее — и сфера больше)
  private auraArgs(el: HTMLElement, visual: AuraVisual) {
    const height = el.getBoundingClientRect().height || 290;
    return {
      color: visual.color,
      glow: visual.glow,
      accent: visual.accent,
      mote: visual.mote,
      drift: visual.drift,
      period: visual.period,
      rx: height * 0.2,
      ry: height * 0.27,
      groundOffset: height * (UNIT_ANCHORS.feet - UNIT_ANCHORS.center),
    };
  }

  // Эмиттеры, следующие за юнитом, пока их не остановят через stopAura
  private attach(el: UnitEl, specs: FxSpec[] | undefined, prefix: string): string[] {
    const service = this.service();
    if (!service || !el || !specs) return [];

    return specs.map(spec => {
      const id = `${prefix}-${++emitterSequence}`;
      const at = this.point(el, spec.anchor, spec.layer) ?? { x: 0, y: 0 };
      service.addEmitter({
        id,
        type: spec.type,
        x: at.x,
        y: at.y,
        padding: spec.padding ?? 40,
        layer: spec.layer ?? 'foreground',
        args: spec.args,
        follow: () => this.point(el, spec.anchor, spec.layer),
      });
      return id;
    });
  }
}
