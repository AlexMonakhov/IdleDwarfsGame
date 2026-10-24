import type { InjectionKey } from 'vue';
import { GlobalParticleService } from '@/services/particleService/ParticleSystem';
import { Point } from '@/services/particleService/types/interfaces/canvas-interfaces';
import {
  Anchor,
  DAMAGE_COLORS,
  FxSpec,
  HEAL_VISUAL,
  HIT_VISUALS,
  STATUS_VISUALS,
  UNIT_ANCHORS,
  resolveSpellVisual,
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

// Одноразовые и постоянные боевые эффекты поверх particle-сервиса арены
export class BattleFx {
  constructor(
    private readonly service: () => GlobalParticleService | null,
    private readonly speed: () => number,
  ) {}

  // Точка на спрайте юнита в координатах канваса нужного слоя
  point(el: UnitEl, anchor: Anchor = 'center', layer: FxSpec['layer'] = 'foreground'): Point | null {
    const canvas = this.service()?.getCanvas(layer ?? 'foreground');
    if (!el || !canvas || !el.isConnected) return null;
    const rect = el.getBoundingClientRect();
    const canvasRect = canvas.getBoundingClientRect();
    return {
      x: rect.left + rect.width / 2 - canvasRect.left,
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
  }

  heal(target: UnitEl) {
    this.play(HEAL_VISUAL, target);
  }

  // Магический круг под кастером на время подготовки заклинания
  castCircle(caster: UnitEl, color: string, duration: number) {
    this.play([{ type: 'ring', anchor: 'feet', layer: 'background', duration, args: { color, radius: 58, mode: 'rune', width: 3 } }], caster);
  }

  // Снаряд/луч от кастера к цели и взрыв на цели. Промис резолвится в момент попадания.
  async spell(caster: UnitEl, target: UnitEl, skillName: string | undefined, damageType: string) {
    const visual = resolveSpellVisual(skillName, damageType);
    const from = this.point(caster);
    const to = this.point(target);
    if (!from || !to) return;

    if (visual.beam) {
      this.emit(visual.beam, from, to);
      await wait((visual.hitDelay ?? 0) / this.speed());
    } else if (visual.projectile) {
      this.emit(visual.projectile, from, to);
      await wait(visual.projectile.duration / this.speed());
    }

    this.play(visual.impact, target, { direction: from.x > to.x ? -1 : 1 });
  }

  // Тик статуса (горение и т.п.). Если для статуса нет своего визуала — обычное попадание по типу урона.
  statusTick(target: UnitEl, statusName: string, damageType?: string) {
    const tick = STATUS_VISUALS[statusName]?.tick;
    if (tick) this.play(tick, target);
    else this.hit(target, damageType);
  }

  // Постоянные эффекты статуса, следующие за юнитом. Возвращает id эмиттеров для stopAura.
  startAura(el: UnitEl, statusName: string): string[] {
    const service = this.service();
    const aura = STATUS_VISUALS[statusName]?.aura;
    if (!service || !el || !aura) return [];

    return aura.map(spec => {
      const id = `aura-${statusName}-${++emitterSequence}`;
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

  stopAura(ids: string[]) {
    const service = this.service();
    ids.forEach(id => service?.removeEmitter(id, 300));
  }
}
