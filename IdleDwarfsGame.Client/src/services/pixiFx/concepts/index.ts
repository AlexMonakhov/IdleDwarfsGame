import type { Concept } from './kit';
import type { Look } from './looks';
import { FIRE_CONCEPTS } from './fire';
import { FROST_CONCEPTS } from './frost';
import { LIGHTNING_CONCEPTS } from './lightning';
import { NATURE_CONCEPTS } from './nature';
import { DARK_CONCEPTS } from './dark';
import { BLOOD_CONCEPTS } from './blood';
import { LIGHT_CONCEPTS } from './light';
import { AURA_CONCEPTS } from './auras';
import { AURA_LOOKS } from './auraLooks';
import { STATUS_LOOKS } from './statusLooks';

// Концепты анимаций для страницы /fx-concepts: наброски, из которых выбираются будущие эффекты заклинаний.
// Срабатывания аур (kind: 'aura') страница проигрывает вместе с обликом ауры
export const CONCEPTS: Concept[] = [
  ...FIRE_CONCEPTS,
  ...FROST_CONCEPTS,
  ...LIGHTNING_CONCEPTS,
  ...NATURE_CONCEPTS,
  ...DARK_CONCEPTS,
  ...BLOOD_CONCEPTS,
  ...LIGHT_CONCEPTS,
  ...AURA_CONCEPTS,
];

// Облики аур и статусов: висят на юнитах, срабатывают по команде, накладываются друг на друга
export const LOOKS: Look[] = [...AURA_LOOKS, ...STATUS_LOOKS];

// Концепт по id
export const conceptById = (id: string) => CONCEPTS.find(concept => concept.id === id);

export { Fx, countFor } from './kit';
export type { ArtifactDemo, ArtifactTrigger, Concept, ConceptKind, ConceptStage, ConceptUnit, SpellLevel, TargetCount } from './kit';
export { startConcept, stepConcept, stopConcept } from './runner';
export type { ConceptRun, RunOptions, StageUnits } from './runner';
export { attachLook, disposeLook, removeLook, stepLook, triggerLook } from './looks';
export type { Look, LookInstance, LookKind } from './looks';
