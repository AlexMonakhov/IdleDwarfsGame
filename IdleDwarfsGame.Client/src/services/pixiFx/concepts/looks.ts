import { Container } from 'pixi.js';
import { Fx, bump, clamp01, type ConceptStage, type ConceptUnit, type SpellLevel } from './kit';

// Облик — то, что висит на юните, пока на нём аура или статус: купол ауры на носителе, пламя горения, звёздочки
// оглушения. В отличие от концепта у облика нет конца: он живёт, пока его не снимут.
// Две стадии: висит (frame каждый кадр) и срабатывает (trigger: тик статуса, пульс ауры).
// На одном юните может висеть несколько обликов сразу: у каждого свои контейнеры и свой Fx.
//
// Правило для trigger: создавать в нём можно только то, что уходит в hide своего твина (pop, ringWave, groundGlow
// и fx.dot с hide) — Fx облика уничтожает такие спрайты, когда твин отыграл. Всё постоянное (частицы, Graphics,
// Beam) заводится в build и переиспользуется

export type LookKind = 'aura' | 'status';

export interface LookRun {
  // time — секунды с наложения; a — 0…1, плавное появление и снятие; surge — 0 → 1 → 0 всплеск после срабатывания
  frame?(time: number, a: number, surge: number): void;
  // Срабатывание: разовые всплески ставятся от fx.now
  trigger?(): void;
}

export interface Look {
  id: string;
  name: string;
  kind: LookKind;
  // Группа в списке и эффект с сервера (имя статуса или эффект ауры)
  group: string;
  spell: string;
  about: string;
  // Меняется по уровню ауры (fx.lvl)
  levels?: boolean;
  // На кого вешается на странице: на врагов (по умолчанию) или на союзников (регенерация)
  side?: 'enemies' | 'allies';
  // Мгновенный эффект без висящей стадии (выжигание энергии): только срабатывание
  instant?: boolean;
  build(fx: Fx, unit: ConceptUnit): LookRun;
}

const FADE_IN = 0.3;
const FADE_OUT = 0.45;
// Сколько длится всплеск surge после срабатывания
const SURGE = 0.5;
// После угасания ещё столько секунд даём долететь частицам
const LINGER = 0.8;

export interface LookInstance {
  look: Look;
  unit: ConceptUnit;
  fx: Fx;
  run: LookRun;
  t: number;
  removedAt: number | null;
  triggeredAt: number;
  layers: Container[];
}

export const attachLook = (
  look: Look,
  unit: ConceptUnit,
  stage: Omit<ConceptStage, 'back' | 'front'>,
  parent: { back: Container; front: Container },
  level: SpellLevel,
): LookInstance => {
  const back = new Container();
  const front = new Container();
  parent.back.addChild(back);
  parent.front.addChild(front);
  const fx = new Fx({ ...stage, back, front }, look.levels ? level : 3);
  fx.reclaim = true;
  return { look, unit, fx, run: look.build(fx, unit), t: 0, removedAt: null, triggeredAt: -10, layers: [back, front] };
};

export const lookAlpha = (inst: LookInstance) =>
  Math.min(clamp01(inst.t / FADE_IN), inst.removedAt === null ? 1 : clamp01(1 - (inst.t - inst.removedAt) / FADE_OUT));

// Шаг на dt секунд; true — облик снят, догорел и убран
export const stepLook = (inst: LookInstance, dt: number) => {
  inst.t += dt;
  inst.fx.step(inst.t, dt);
  inst.run.frame?.(inst.t, lookAlpha(inst), bump(clamp01((inst.t - inst.triggeredAt) / SURGE)));
  if (inst.removedAt === null || inst.t - inst.removedAt < FADE_OUT + LINGER) return false;
  disposeLook(inst);
  return true;
};

export const triggerLook = (inst: LookInstance) => {
  if (inst.removedAt !== null) return;
  inst.triggeredAt = inst.t;
  inst.run.trigger?.();
};

// Снять: облик плавно гаснет и убирается сам (stepLook вернёт true)
export const removeLook = (inst: LookInstance) => {
  if (inst.removedAt === null) inst.removedAt = inst.t;
};

export const disposeLook = (inst: LookInstance) => {
  inst.fx.dispose();
  inst.layers.forEach(layer => {
    if (!layer.destroyed) layer.destroy({ children: true });
  });
};

// Высота тела юнита, px
export const bodyOf = (u: ConceptUnit) => Math.max(60, u.feet.y - u.head.y);
