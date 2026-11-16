import { Container } from 'pixi.js';
import { Fx, type ArtifactTrigger, type Concept, type ConceptStage, type ConceptUnit, type Frame, type Pt, type SpellLevel } from './kit';
import { prismFlourish } from './light';

// Проигрывание концепта на сцене: свои контейнеры под и над юнитами (несколько концептов идут разом и убираются
// независимо), зеркало для кастера справа, время в секундах. Общее для страницы концептов и боя (battle/ConceptSpells)

// Сцена без контейнеров: их создаёт запуск
export type StageUnits = Omit<ConceptStage, 'back' | 'front'>;

export interface RunOptions {
  level: SpellLevel;
  // Кастер справа: концепты нарисованы для кастера слева, поэтому всё отражается по горизонтали
  mirror?: boolean;
  extras?: boolean;
  artifacts?: Iterable<string>;
  trigger?: ArtifactTrigger | null;
  origin?: Pt | null;
}

export interface ConceptRun {
  fx: Fx;
  update: Frame | void;
  t: number;
  duration: number;
  layers: Container[];
}

// Особая подача отдельных срабатываний, общая для любых заклинаний (случайное заклинание «Призменной линзы»)
const FLOURISH: Record<string, (fx: Fx) => void> = {
  SpellProcPool: prismFlourish,
};

const mirrorPt = (p: Pt, width: number): Pt => ({ x: width - p.x, y: p.y });

// Юнит в зеркальных координатах; реакции те же. Один и тот же юнит отражается один раз (кастер — он же allies[0])
const mirrorUnits = (stage: StageUnits): StageUnits => {
  const cache = new Map<ConceptUnit, ConceptUnit>();
  const flip = (unit: ConceptUnit): ConceptUnit => {
    let flipped = cache.get(unit);
    if (!flipped) {
      flipped = {
        center: mirrorPt(unit.center, stage.width),
        feet: mirrorPt(unit.feet, stage.width),
        head: mirrorPt(unit.head, stage.width),
        hit: unit.hit,
        heal: unit.heal,
        proc: unit.proc,
      };
      cache.set(unit, flipped);
    }
    return flipped;
  };
  return {
    ...stage,
    caster: flip(stage.caster),
    enemies: stage.enemies.map(flip),
    allies: stage.allies.map(flip),
    bystanders: stage.bystanders.map(flip),
    healed: stage.healed.map(flip),
  };
};

export const startConcept = (concept: Concept, units: StageUnits, parent: { back: Container; front: Container }, options: RunOptions): ConceptRun => {
  const back = new Container();
  const front = new Container();
  if (options.mirror) {
    for (const layer of [back, front]) {
      layer.scale.x = -1;
      layer.x = units.width;
    }
  }
  parent.back.addChild(back);
  parent.front.addChild(front);
  const stage = options.mirror ? mirrorUnits(units) : units;
  const fx = new Fx({ ...stage, back, front }, concept.levels ? options.level : 3);
  fx.extras = !!options.extras;
  fx.artifacts = new Set(options.artifacts ?? []);
  fx.trigger = options.trigger ?? null;
  fx.origin = options.origin ? (options.mirror ? mirrorPt(options.origin, units.width) : options.origin) : null;
  const flourish = fx.trigger ? FLOURISH[fx.trigger.kind] : undefined;
  if (flourish) fx.fork(0, {}, flourish);
  const update = concept.build(fx);
  return { fx, update, t: 0, duration: concept.duration + fx.tail, layers: [back, front] };
};

// Шаг на dt секунд; true — проигрывание закончилось (и всё за собой убрало)
export const stepConcept = (run: ConceptRun, dt: number) => {
  run.t += dt;
  run.fx.step(run.t, dt);
  run.update?.(run.t, dt);
  if (run.t < run.duration) return false;
  stopConcept(run);
  return true;
};

export const stopConcept = (run: ConceptRun) => {
  run.fx.dispose();
  run.layers.forEach(layer => layer.destroy({ children: true }));
};
