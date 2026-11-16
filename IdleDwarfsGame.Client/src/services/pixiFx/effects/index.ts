import type { EffectType } from '@/services/particleService/types/emitter';
import type { PixiEffectFactory } from '../effect';
import { createExplosion } from './explosion';
import { createSparkles } from './sparkles';
import { createFlare } from './flare';
import { createRing } from './ring';
import { createProjectile } from './projectile';
import { createSlash } from './slash';
import { createFlames } from './flames';
import { createImpact } from './impact';
import { createLightning } from './lightning';
import { createStunStars } from './stunStars';
import { createFrostAura } from './frostAura';
import { createAuraSphere } from './auraSphere';
import { createAuraBurst } from './auraBurst';
import { createGroundMark } from './groundMark';
import { createFireWall } from './fireWall';
import { createStorm } from './storm';
import { createScreenFlash } from './screenFlash';
import { createIcePrison } from './icePrison';
import { createRoots } from './roots';
import { createDrain } from './drain';
import { createSigil } from './sigil';
import { createNova } from './nova';
import { createLightPillar } from './lightPillar';
import { createArcs } from './arcs';
import { createComet } from './comet';
import { createUiParticles } from './uiParticles';
import { createGodRays, createGodRaysRadiance } from './godRays';

// Все эффекты по типу эмиттера. Что когда играет — battle/FxRegistry.ts (данные), запускает battle/BattleFx.ts
export const PIXI_EFFECTS: Record<EffectType, PixiEffectFactory> = {
  explosion: createExplosion,
  sparkles: createSparkles,
  flare: createFlare,
  ring: createRing,
  projectile: createProjectile,
  slash: createSlash,
  flames: createFlames,
  impact: createImpact,
  lightning: createLightning,
  'stun-stars': createStunStars,
  'frost-aura': createFrostAura,
  'aura-sphere': createAuraSphere,
  'aura-burst': createAuraBurst,
  'ground-mark': createGroundMark,
  'fire-wall': createFireWall,
  storm: createStorm,
  'screen-flash': createScreenFlash,
  'ice-prison': createIcePrison,
  roots: createRoots,
  drain: createDrain,
  sigil: createSigil,
  nova: createNova,
  'light-pillar': createLightPillar,
  arcs: createArcs,
  comet: createComet,
  particles: createUiParticles,
  'god-rays': createGodRays,
  'god-rays-radiance': createGodRaysRadiance,
};
