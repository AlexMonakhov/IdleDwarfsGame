import type { Concept } from './kit';
import { FIRE_CONCEPTS } from './fire';
import { FROST_CONCEPTS } from './frost';
import { LIGHTNING_CONCEPTS } from './lightning';
import { NATURE_CONCEPTS } from './nature';
import { DARK_CONCEPTS } from './dark';
import { BLOOD_CONCEPTS } from './blood';
import { LIGHT_CONCEPTS } from './light';

// Концепты анимаций для страницы /fx-concepts: наброски, из которых выбираются будущие эффекты заклинаний
export const CONCEPTS: Concept[] = [
  ...FIRE_CONCEPTS,
  ...FROST_CONCEPTS,
  ...LIGHTNING_CONCEPTS,
  ...NATURE_CONCEPTS,
  ...DARK_CONCEPTS,
  ...BLOOD_CONCEPTS,
  ...LIGHT_CONCEPTS,
];

// Концепт по id
export const conceptById = (id: string) => CONCEPTS.find(concept => concept.id === id);

export { Fx, countFor } from './kit';
export type { ArtifactDemo, ArtifactTrigger, Concept, ConceptStage, ConceptUnit, SpellLevel, TargetCount } from './kit';
export { startConcept, stepConcept, stopConcept } from './runner';
export type { ConceptRun, RunOptions, StageUnits } from './runner';
