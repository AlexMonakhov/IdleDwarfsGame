import {
  Beam, C, GROUND, P, TAU, angryEye, arc, bez, bump, clamp01, color, crackle, dist, ease, fade, fly, groundGlow, jag, lerp, levelAccent, mix, palette, polar,
  pop, rand, ringWave, spray, taper,
  type Concept, type ConceptUnit, type Element, type Fx, type ParticleSim, type Pt,
} from './kit';
import { dropTexture, leafTexture, snowflakeTexture } from './sprites';

// Концепты аур (AuraBook на сервере): аура срабатывает в начале раунда — носитель (здесь кастер) собирает силу,
// и волна или снаряды доходят до всех врагов (реген — до всех союзников). Сила ауры растёт с её уровнем 1…3,
// поэтому у концептов есть уровни: размах, густота и отметка стихии на носителе на 3-м

// Носитель собирает силу до этого момента, затем аура срывается с него
const CHARGE = 0.45;

const bodyOf = (u: ConceptUnit) => Math.max(60, u.feet.y - u.head.y);

// Носитель вспыхивает: свечение вокруг тела и кольцо под ногами; на 3-м уровне — отметка стихии
const charge = (fx: Fx, element: Element, ring: string, glow: string) => {
  const B = fx.stage.caster;
  const h = bodyOf(B);
  const aura = fx.dot('glow', glow);
  const base = fx.dot('ring', ring, 'back');
  fx.at(0, () => fx.stage.cast(ring));
  fx.tween(0, CHARGE + 0.3, p => {
    const a = fade(p, 0.55, 0.3);
    const s = 0.75 + 0.25 * ease.out(p);
    aura.oval(B.center.x, B.center.y, 70 * s, h * 0.62 * s, a * fx.lvl([0.45, 0.55, 0.7]));
    base.oval(B.feet.x, B.feet.y, 85 * s, 85 * s * GROUND, a);
  }, [aura, base]);
  pop(fx, { at: CHARGE, dur: 0.4, x: B.center.x, y: B.center.y, r: fx.lvl([110, 140, 170]), css: glow, alpha: 0.8 });
  if (fx.since(3)) levelAccent(fx, element, CHARGE, B, 0.8);
};

// Когда волна со скоростью speed px/с доходит от носителя до юнита
const arrival = (fx: Fx, u: ConceptUnit, speed: number, start = CHARGE) => start + dist(fx.stage.caster.feet, u.feet) / speed;

// Точка на ломаной на доле p пути (по вершинам)
const along = (pts: Pt[], p: number): Pt => {
  const f = clamp01(p) * (pts.length - 1);
  const n = Math.min(Math.floor(f), pts.length - 2);
  return mix(pts[n], pts[n + 1], f - n);
};

// Начало ломаной на долю p (для узоров, которые растут вдоль пути)
const prefix = (pts: Pt[], p: number): Pt[] => {
  const f = clamp01(p) * (pts.length - 1);
  const n = Math.floor(f);
  const out = pts.slice(0, n + 1);
  if (n < pts.length - 1) out.push(mix(pts[n], pts[n + 1], f - n));
  return out;
};

const FLAME = { texture: 'smoke' as const, drag: 0.2, gravity: -520, shrink: 0.15, stretch: [0.7, 2] as [number, number], tint: palette(C.fire.slice(1)), alpha: (life: number) => bump(life) * 0.9 };
const SNOW = { texture: snowflakeTexture(), drag: 0.6, gravity: 40, shrink: 0.7, tint: palette(['#ffffff', '#e0f6ff', '#bae6fd']), alpha: (life: number) => bump(life) };
const GAS = { texture: 'smoke' as const, blend: 'normal' as const, drag: 0.5, gravity: -18, shrink: 1.5, tint: palette(['#d9f99d', '#84cc16', '#3f6212']), alpha: (life: number) => bump(life) * 0.45 };
const SOOT = { texture: 'smoke' as const, blend: 'normal' as const, drag: 0.3, gravity: -60, shrink: 1.4, tint: palette(['#3b0764', '#1e0b36', '#09030f']), alpha: (life: number) => bump(life) * 0.55 };
// Искры энергии: голубые, догорают фиолетовым и чернеют
const ENERGY = ['#e0f2fe', '#67e8f9', '#a78bfa', '#7e22ce', '#2e1065'];

// Поджог цели: языки пламени из-под ног, искры, пятно жара
const ignite = (fx: Fx, u: ConceptUnit, at: number, flames: ParticleSim, sparks: ParticleSim) => {
  const k = fx.lvl([0.7, 0.85, 1]);
  fx.at(at, () => {
    u.hit(false, '#ff7a1a');
    spray(sparks, { x: u.feet.x, y: u.feet.y - 10, count: Math.ceil(16 * k), speed: [120, 380], dir: -Math.PI / 2, spread: 0.8, life: [0.3, 0.7], size: [3, 7] });
  });
  fx.tween(at, 0.6, p => {
    for (let i = fx.count(260 * k * (1 - p)); i > 0; i--) {
      flames.spawn({ x: u.feet.x + rand(-34, 34), y: u.feet.y + rand(-6, 6), vx: rand(-20, 20), vy: rand(-440, -200) * k, lifetime: rand(0.25, 0.5), size: rand(8, 18) });
    }
  });
  groundGlow(fx, { at, dur: 0.9, x: u.feet.x, y: u.feet.y, r: 95, css: '#ff6a00' });
};

// Иней на цели: пятно холода под ногами, блёстки по телу
const chill = (fx: Fx, u: ConceptUnit, at: number, glitter: ParticleSim) => {
  const h = bodyOf(u);
  const sheen = fx.radial([[0, 0.6], [0.6, 0.3], [1, 0]], '#bae6fd', 'back');
  const glaze = fx.dot('glow', '#7dd3fc');
  fx.at(at, () => u.hit(false, '#7dd3fc'));
  fx.tween(at, 1, p => {
    sheen.oval(u.feet.x, u.feet.y, 100 * ease.out3(Math.min(p * 3, 1)), 100 * GROUND, fade(p, 0.05, 0.5) * 0.8);
    glaze.oval(u.center.x, u.center.y, 60, h * 0.6, bump(p) * 0.35);
    for (let i = fx.count(40 * (1 - p)); i > 0; i--) {
      glitter.spawn({ x: u.center.x + rand(-45, 45), y: rand(u.head.y, u.feet.y), vx: 0, vy: rand(-30, 0), lifetime: rand(0.35, 0.7), size: rand(5, 11), rotation: rand(0, TAU) });
    }
  }, [sheen, glaze]);
};

// Разряд по телу цели: несколько дуг снизу вверх и искры
const zap = (fx: Fx, u: ConceptUnit, at: number, arcs: number, sparks: ParticleSim) => {
  const h = bodyOf(u);
  fx.at(at, () => {
    u.hit(false, '#fde68a');
    spray(sparks, { x: u.center.x, y: u.center.y, count: 14, speed: [100, 340], life: [0.2, 0.45], size: [2, 5] });
  });
  for (let n = 0; n < arcs; n++) {
    const beam = new Beam(fx, 7, 3, '#fde68a');
    crackle(fx, beam, at + n * 0.1, 0.32, () => {
      const y0 = u.feet.y - rand(0, h * 0.4);
      return jag({ x: u.center.x + rand(-36, 36), y: y0 }, { x: u.center.x + rand(-36, 36), y: y0 - rand(h * 0.35, h * 0.7) }, 7, 12);
    });
  }
  const glow = fx.dot('glow', '#fde68a');
  fx.tween(at, 0.5, p => glow.oval(u.center.x, u.center.y, 60, h * 0.55, (0.35 + 0.2 * Math.sin(p * 60)) * (1 - p)), [glow]);
};

// Отравление: зелёный отсвет на теле, пузыри и капли вверх
const sicken = (fx: Fx, u: ConceptUnit, at: number, bubbles: ParticleSim) => {
  const h = bodyOf(u);
  const tint = fx.dot('glow', '#84cc16');
  fx.at(at, () => u.hit(false, '#a3e635'));
  fx.tween(at, 0.9, p => {
    tint.oval(u.center.x, u.center.y, 62, h * 0.6, bump(p) * 0.45);
    for (let i = fx.count(18 * (1 - p)); i > 0; i--) {
      bubbles.spawn({ x: u.center.x + rand(-40, 40), y: rand(u.center.y, u.feet.y), vx: rand(-10, 10), vy: rand(-90, -40), lifetime: rand(0.5, 0.9), size: rand(5, 10) });
    }
  }, [tint]);
  groundGlow(fx, { at, dur: 0.9, x: u.feet.x, y: u.feet.y, r: 90, css: '#65a30d' });
};

// Энергия сгорает: из тела вырываются голубые искры, догорают фиолетовым, остаётся копоть
const drain = (fx: Fx, u: ConceptUnit, at: number, motes: ParticleSim, soot: ParticleSim) => {
  const h = bodyOf(u);
  const k = fx.lvl([0.7, 0.85, 1]);
  fx.at(at, () => {
    u.hit(false, '#a855f7');
    spray(motes, { x: u.center.x, y: u.center.y - h * 0.1, count: Math.ceil(22 * k), speed: [80, 260], dir: -Math.PI / 2, spread: 1.2, life: [0.5, 0.9], size: [5, 10], jitter: 20 });
  });
  fx.tween(at, 0.7, p => {
    for (let i = fx.count(30 * (1 - p)); i > 0; i--) soot.spawn({ x: u.center.x + rand(-30, 30), y: u.head.y + rand(0, 40), vx: rand(-15, 15), vy: rand(-80, -30), lifetime: rand(0.5, 0.9), size: rand(14, 24) });
  });
};

// Кровотечение: брызги из тела, красный пульс
const wound = (fx: Fx, u: ConceptUnit, at: number, drops: ParticleSim) => {
  const h = bodyOf(u);
  fx.at(at, () => {
    u.hit(false, '#dc2626');
    spray(drops, { x: u.center.x, y: u.center.y, count: Math.ceil(fx.lvl([8, 12, 16])), speed: [160, 440], dir: -Math.PI / 2, spread: 1.4, life: [0.4, 0.8], size: [5, 9], align: true });
  });
  const pulse = fx.dot('ring', '#ef4444');
  fx.tween(at, 0.4, p => pulse.oval(u.center.x, u.center.y, 60 * (0.7 + 0.5 * p), h * 0.5 * (0.7 + 0.5 * p), (1 - p) * 0.8), [pulse]);
  const stain = fx.radial([[0, 0.8], [0.7, 0.55], [1, 0]], '#7f1d1d', 'back', 'normal');
  fx.tween(at + 0.1, 1, p => stain.oval(u.feet.x, u.feet.y, 70 * ease.out(Math.min(p * 2, 1)), 70 * GROUND, fade(p, 0.1, 0.4) * 0.7), [stain]);
};

// Лечение союзника: мягкая вспышка, плюсики вверх
const mend = (fx: Fx, u: ConceptUnit, at: number, pluses: ParticleSim) => {
  const h = bodyOf(u);
  const glow = fx.dot('glow', '#fde68a');
  fx.at(at, () => {
    u.heal();
    spray(pluses, { x: u.center.x, y: u.center.y, count: Math.ceil(fx.lvl([5, 7, 10])), speed: [30, 90], dir: -Math.PI / 2, spread: 0.9, life: [0.6, 1], size: [8, 13], jitter: 30 });
  });
  fx.tween(at, 0.8, p => glow.oval(u.center.x, u.center.y, 66, h * 0.62, bump(p) * 0.5), [glow]);
  groundGlow(fx, { at, dur: 0.9, x: u.feet.x, y: u.feet.y, r: 90, css: '#bef264', alpha: 0.8 });
};

// Общая подпись аур в списке
const aura = (concept: Omit<Concept, 'kind' | 'levels'>): Concept => ({ ...concept, kind: 'aura', levels: true });

export const AURA_CONCEPTS: Concept[] = [
  // ===== Горение =====
  aura({
    id: 'aura-burn-ring', name: 'Огненное кольцо', group: 'Аура · Горение', spell: 'Burn', duration: 2.2,
    about: 'Под носителем разгорается кольцо и катится по земле огненным валом; под кем прошёл вал — из-под ног бьёт пламя',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'fire', '#ff7a1a', '#ffb347');
      const SPEED = 950;
      const reach = Math.max(...fx.stage.enemies.map(e => dist(B.feet, e.feet))) + 90;
      const ring = fx.dot('ring', '#ff9a3c', 'back');
      const inner = fx.dot('ring', '#ffe27a', 'back');
      const flames = fx.sim(FLAME, 'back');
      const front = fx.sim(FLAME);
      const sparks = fx.sim(P.spark(C.fire, 400));
      fx.tween(CHARGE, reach / SPEED, p => {
        const r = reach * p;
        ring.oval(B.feet.x, B.feet.y, r, r * GROUND, 1 - p * 0.4);
        inner.oval(B.feet.x, B.feet.y, r * 0.95, r * 0.95 * GROUND, 0.6 * (1 - p));
        for (let i = fx.count(fx.lvl([200, 280, 380])); i > 0; i--) {
          const a = Math.random() * TAU;
          const pt = polar(B.feet, r, a, GROUND);
          (Math.sin(a) > 0 ? front : flames).spawn({ x: pt.x, y: pt.y, vx: rand(-20, 20), vy: rand(-180, -70), lifetime: rand(0.2, 0.4), size: rand(6, 13) });
        }
      }, [ring, inner]);
      fx.stage.enemies.forEach(e => ignite(fx, e, arrival(fx, e, SPEED), front, sparks));
    },
  }),
  aura({
    id: 'aura-burn-embers', name: 'Рой угольков', group: 'Аура · Горение', spell: 'Burn', duration: 2.1,
    about: 'Из носителя взлетает рой угольков, каждый дугой падает на своего врага и вспыхивает на нём',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'fire', '#ff7a1a', '#ffb347');
      const trail = fx.sim(P.ember(C.fire, 60));
      const flames = fx.sim(FLAME);
      const sparks = fx.sim(P.spark(C.fire, 400));
      const per = fx.lvl([1, 2, 3]);
      fx.stage.enemies.forEach((e, i) => {
        for (let n = 0; n < per; n++) {
          const start = CHARGE + i * 0.05 + n * 0.08;
          const from = { x: B.center.x + rand(-20, 20), y: B.head.y - 10 };
          const to = { x: e.center.x + rand(-18, 18), y: e.center.y + rand(-20, 20) };
          const top = { x: lerp(from.x, to.x, 0.35), y: Math.min(from.y, to.y) - rand(120, 220) };
          const dur = 0.45 + dist(from, to) / 2200;
          const core = fx.dot('glow', '#fff1a8');
          const halo = fx.dot('glow', '#ff7a1a');
          fly(fx, start, dur, p => bez(from, top, to, ease.in(p)), pt => {
            halo.put(pt.x, pt.y, 26, 0.85);
            core.put(pt.x, pt.y, 10);
            spray(trail, { x: pt.x, y: pt.y, count: fx.count(90), speed: [5, 30], life: [0.2, 0.45], size: [4, 8] });
          }, [core, halo]);
          if (n === 0) ignite(fx, e, start + dur, flames, sparks);
          else pop(fx, { at: start + dur, dur: 0.3, x: to.x, y: to.y, r: 60, css: '#ffb347' });
        }
      });
    },
  }),
  aura({
    id: 'aura-burn-heat', name: 'Марево жара', group: 'Аура · Горение', spell: 'Burn', duration: 2.3,
    about: 'Через поле прокатывается стена раскалённого марева; враги, через которых она прошла, тлеют и дымятся',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'fire', '#ff7a1a', '#ffb347');
      const xs = fx.stage.enemies.map(e => e.feet.x);
      const x0 = B.center.x + 40;
      const x1 = Math.max(...xs) + 120;
      const SPEED = 1000;
      const wall = fx.radial([[0, 0.55], [0.5, 0.25], [1, 0]], '#ff7a1a');
      const floor = fx.radial([[0, 0.7], [0.5, 0.35], [1, 0]], '#ff6a00', 'back');
      const shimmer = fx.sim({ texture: 'smoke', drag: 0.3, gravity: -260, shrink: 1.2, tint: palette(['#ffe27a', '#ff9a3c', '#c2410c']), alpha: life => bump(life) * 0.35 });
      const smoke = fx.sim(P.smoke('#3f3f46', 70));
      const embers = fx.sim(P.ember(C.fire, 120));
      const top = Math.min(...fx.stage.enemies.map(e => e.head.y)) - 60;
      const bottom = Math.max(...fx.stage.enemies.map(e => e.feet.y)) + 30;
      fx.tween(CHARGE, (x1 - x0) / SPEED, p => {
        const x = lerp(x0, x1, p);
        const a = fade(p, 0.1, 0.25);
        wall.oval(x, (top + bottom) / 2, 70 * fx.lvl([0.8, 0.9, 1]), (bottom - top) * 0.6, a);
        floor.oval(x, bottom - 20, 120, 60, a);
        for (let i = fx.count(fx.lvl([90, 140, 200])); i > 0; i--) shimmer.spawn({ x: x + rand(-40, 40), y: rand(top, bottom), vx: rand(20, 80), vy: rand(-60, 0), lifetime: rand(0.3, 0.6), size: rand(14, 28) });
      }, [wall, floor]);
      fx.stage.enemies.forEach(e => {
        const at = CHARGE + (e.feet.x - x0) / SPEED;
        const glow = fx.dot('glow', '#ff6a00');
        fx.at(at, () => e.hit(false, '#ff7a1a'));
        fx.tween(at, 1.2, p => {
          glow.oval(e.center.x, e.center.y, 60, bodyOf(e) * 0.55, bump(p) * 0.45 * (0.8 + 0.2 * Math.sin(p * 30)));
          for (let i = fx.count(30 * (1 - p)); i > 0; i--) smoke.spawn({ x: e.center.x + rand(-30, 30), y: e.head.y + rand(0, 50), vx: rand(-10, 10), vy: -30, lifetime: rand(0.6, 1), size: rand(12, 20) });
          for (let i = fx.count(40 * (1 - p)); i > 0; i--) embers.spawn({ x: e.center.x + rand(-35, 35), y: rand(e.head.y, e.feet.y), vx: rand(-15, 15), vy: rand(-90, -40), lifetime: rand(0.4, 0.8), size: rand(3, 6) });
        }, [glow]);
      });
    },
  }),

  // ===== Мороз =====
  aura({
    id: 'aura-frost-rime', name: 'Иней по земле', group: 'Аура · Мороз', spell: 'Frost', duration: 2.3,
    about: 'От ног носителя к каждому врагу ползут ветвистые дорожки инея; где дорожка дошла — ноги врага схватывает холодом',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'frost', '#7dd3fc', '#bae6fd');
      const rime = fx.gfx('back', 'add');
      const glitter = fx.sim({ texture: 'star', drag: 0.6, gravity: -20, shrink: 0.6, tint: palette(['#ffffff', '#e0f6ff', '#7dd3fc']), alpha: life => bump(life) });
      const SPEED = 800;
      const paths = fx.stage.enemies.map(e => {
        const pts = jag(B.feet, e.feet, 12, 26);
        const twigs = pts.slice(1, -1).map((pt, i) => ({ at: i / (pts.length - 1), a: rand(-1, 1) > 0 ? 1 : -1, len: rand(14, 30) }));
        return { e, pts, twigs, dur: dist(B.feet, e.feet) / SPEED };
      });
      const total = Math.max(...paths.map(path => path.dur));
      fx.tween(CHARGE, total + 0.8, p => {
        const time = p * (total + 0.8);
        const a = fade(p, 0.02, 0.3);
        rime.clear();
        for (const path of paths) {
          const q = clamp01(time / path.dur);
          const pts = prefix(path.pts, q);
          rime.moveTo(pts[0].x, pts[0].y);
          pts.slice(1).forEach(pt => rime.lineTo(pt.x, pt.y));
          rime.stroke({ width: fx.lvl([2, 2.5, 3.5]), color: color('#e0f6ff'), alpha: 0.9 * a });
          for (const twig of path.twigs) {
            if (twig.at > q) continue;
            const base = along(path.pts, twig.at);
            const dir = Math.atan2(path.e.feet.y - B.feet.y, path.e.feet.x - B.feet.x) + twig.a * 0.9;
            const end = polar(base, twig.len, dir, GROUND * 2);
            rime.moveTo(base.x, base.y).lineTo(end.x, end.y).stroke({ width: 1.5, color: color('#bae6fd'), alpha: 0.75 * a });
          }
        }
      });
      paths.forEach(path => chill(fx, path.e, CHARGE + path.dur, glitter));
    },
  }),
  aura({
    id: 'aura-frost-gust', name: 'Ледяной ветер', group: 'Аура · Мороз', spell: 'Frost', duration: 2.2,
    about: 'Носитель выдыхает поток снежинок, вьюга проносится сквозь строй врагов и оставляет на них иней',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'frost', '#7dd3fc', '#bae6fd');
      const flakes = fx.sim({ ...SNOW, drag: 0.95, gravity: 0 });
      const streaks = fx.sim(P.streak(C.ice));
      const glitter = fx.sim({ texture: 'star', drag: 0.6, gravity: -20, shrink: 0.6, tint: palette(['#ffffff', '#e0f6ff', '#7dd3fc']), alpha: life => bump(life) });
      const top = Math.min(...fx.stage.enemies.map(e => e.head.y)) - 30;
      const bottom = Math.max(...fx.stage.enemies.map(e => e.feet.y));
      const SPEED = 1100;
      fx.tween(CHARGE, 0.7, p => {
        for (let i = fx.count(fx.lvl([80, 120, 170]) * (1 - p * 0.5)); i > 0; i--) {
          const y = rand(top, bottom);
          flakes.spawn({ x: B.center.x + rand(0, 40), y: lerp(B.center.y, y, 0.3), vx: SPEED * rand(0.85, 1.1), vy: (y - B.center.y) * 0.9, lifetime: rand(0.9, 1.3), size: rand(6, 12), rotation: rand(0, TAU), spin: rand(-6, 6) });
        }
        for (let i = fx.count(110); i > 0; i--) {
          const y = rand(top, bottom);
          streaks.spawn({ x: B.center.x + rand(0, 40), y: lerp(B.center.y, y, 0.3), vx: SPEED * rand(1, 1.3), vy: (y - B.center.y) * 1.1, lifetime: rand(0.5, 0.9), size: rand(4, 7), rotation: Math.atan2(y - B.center.y, SPEED) });
        }
      });
      fx.stage.enemies.forEach(e => chill(fx, e, CHARGE + 0.1 + (e.center.x - B.center.x) / SPEED, glitter));
    },
  }),
  aura({
    id: 'aura-frost-ring', name: 'Морозное кольцо', group: 'Аура · Мороз', spell: 'Frost', duration: 2.4,
    about: 'Под каждым врагом сжимается кольцо стужи со снежным вихрем и схлопывается столбом холода — по очереди, от ближних к дальним (мотив «Морозного кольца» Ледяной темницы)',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'frost', '#7dd3fc', '#bae6fd');
      const flakes = fx.sim({ ...SNOW, drag: 0.9, gravity: 0 });
      const up = fx.sim({ ...SNOW, gravity: -260 });
      const glitter = fx.sim({ texture: 'star', drag: 0.6, gravity: -20, shrink: 0.6, tint: palette(['#ffffff', '#e0f6ff', '#7dd3fc']), alpha: life => bump(life) });
      const K = fx.lvl([0.7, 0.85, 1]);
      const order = [...fx.stage.enemies].sort((a, b) => dist(B.feet, a.feet) - dist(B.feet, b.feet));
      order.forEach((e, i) => {
        const { x, y } = e.feet;
        const start = CHARGE - 0.2 + i * 0.1;
        const CLOSE = 0.5;
        const snap = start + CLOSE;
        const ring = fx.dot('ring', '#bae6fd', 'back');
        const ringFront = fx.dot('ring', '#ffffff');
        const column = fx.dot('glow', '#7dd3fc');
        const core = fx.dot('glow', '#ffffff');
        fx.tween(start, CLOSE, p => {
          const r = lerp(170, 36, ease.in(p));
          ring.oval(x, y, r, r * GROUND, 0.5 + 0.5 * p);
          ringFront.oval(x, y - 60 * p, r * 0.8, r * 0.8 * GROUND, p * 0.6);
          for (let n = fx.count(fx.lvl([60, 80, 110])); n > 0; n--) {
            const a = Math.random() * TAU;
            const at = polar({ x, y }, r, a, GROUND);
            flakes.spawn({ x: at.x, y: at.y, vx: -Math.sin(a) * 200, vy: Math.cos(a) * 200 * GROUND - 50, lifetime: rand(0.3, 0.55), size: rand(4, 8), spin: 6 });
          }
        }, [ring, ringFront]);
        fx.tween(snap, 0.6, p => {
          const a = fade(p, 0.06, 0.6);
          column.oval(x, y - 110 * K, 55 * K, 210 * K, a * 0.9);
          core.oval(x, y - 110 * K, 16 * K, 195 * K, a);
          for (let n = fx.count(p < 0.5 ? 90 * K : 0); n > 0; n--) up.spawn({ x: x + rand(-18, 18), y, vx: rand(-20, 20), vy: rand(-520, -200), lifetime: rand(0.4, 0.8), size: rand(4, 9), spin: 6 });
        }, [column, core]);
        if (i === 0) fx.at(snap, () => fx.stage.shake(3, 0.2));
        ringWave(fx, { at: snap, dur: 0.5, x, y, from: 30, to: 150, css: '#e0f6ff', squash: GROUND, layer: 'back' });
        chill(fx, e, snap, glitter);
      });
    },
  }),
  aura({
    id: 'aura-frost-star', name: 'Снежная звезда', group: 'Аура · Мороз', spell: 'Frost', duration: 2.4,
    about: 'Над носителем раскрывается и вращается большая снежинка, рассыпается на малые — они планируют на врагов и тают кольцами',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'frost', '#7dd3fc', '#bae6fd');
      const flake = snowflakeTexture();
      const big = fx.tex(flake, 'front', 'add', '#e0f6ff');
      const halo = fx.dot('glow', '#7dd3fc');
      const top = { x: B.center.x, y: B.head.y - 70 };
      fx.tween(0.1, CHARGE + 0.25, p => {
        const s = ease.back(Math.min(p * 1.6, 1)) * fx.lvl([42, 52, 62]);
        big.put(top.x, top.y, s, p < 0.85 ? 1 : (1 - p) / 0.15, p * 5);
        halo.put(top.x, top.y, s * 1.8, 0.5);
      }, [big, halo]);
      const glitter = fx.sim({ texture: 'star', drag: 0.6, gravity: -20, shrink: 0.6, tint: palette(['#ffffff', '#e0f6ff', '#7dd3fc']), alpha: life => bump(life) });
      const dust = fx.sim(P.star(C.ice, 0));
      const START = CHARGE + 0.3;
      fx.at(START, () => spray(dust, { x: top.x, y: top.y, count: 30, speed: [80, 260], life: [0.3, 0.6], size: [4, 8] }));
      fx.stage.enemies.forEach((e, i) => {
        const small = fx.tex(flake, 'front', 'add', '#ffffff');
        const glow = fx.dot('glow', '#bae6fd');
        const to = { x: e.center.x, y: e.head.y - 10 };
        const via = { x: lerp(top.x, to.x, 0.5), y: Math.min(top.y, to.y) - 90 };
        const dur = 0.55 + i * 0.06;
        fly(fx, START, dur, p => bez(top, via, to, ease.inOut(p)), (pt, p) => {
          const wobble = Math.sin(p * 12 + i) * 10;
          small.put(pt.x + wobble, pt.y, 18, 1, p * 8 + i);
          glow.put(pt.x + wobble, pt.y, 34, 0.5);
        }, [small, glow]);
        ringWave(fx, { at: START + dur, dur: 0.5, x: e.feet.x, y: e.feet.y, from: 20, to: 110, css: '#bae6fd', squash: GROUND, layer: 'back' });
        chill(fx, e, START + dur, glitter);
      });
    },
  }),

  // ===== Шок =====
  aura({
    id: 'aura-shock-chain', name: 'Цепная дуга', group: 'Аура · Шок', spell: 'Shock', duration: 2,
    about: 'С носителя срывается молния и прыгает от врага к врагу по всему строю, на каждом оставляя треск разрядов',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'bolt', '#fde68a', '#fde68a');
      const sparks = fx.sim(P.spark(C.bolt, 300));
      const order = [...fx.stage.enemies].sort((a, b) => a.center.x - b.center.x);
      const HOP = fx.lvl([0.13, 0.11, 0.09]);
      let from: Pt = { x: B.center.x + 20, y: B.center.y - 20 };
      order.forEach((e, i) => {
        const at = CHARGE + i * HOP;
        const a = from;
        const beam = new Beam(fx, 10, fx.lvl([4, 5, 6]), '#fde68a');
        crackle(fx, beam, at, 0.35, () => jag(a, e.center, 10, 26));
        zap(fx, e, at + 0.03, fx.lvl([1, 2, 3]), sparks);
        pop(fx, { at, dur: 0.25, x: e.center.x, y: e.center.y, r: 70, css: '#fff7d6' });
        from = e.center;
      });
      fx.at(CHARGE, () => fx.stage.flash('#fff7d6', 0.18, 0.15));
    },
  }),
  aura({
    id: 'aura-shock-field', name: 'Статическое поле', group: 'Аура · Шок', spell: 'Shock', duration: 2.2,
    about: 'Вокруг носителя трещит сфера статики, затем по земле разбегается электрическая волна — враги дёргаются от разрядов',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'bolt', '#fde68a', '#fde68a');
      const h = bodyOf(B);
      const sparks = fx.sim(P.spark(C.bolt, 300));
      for (let n = 0; n < 4; n++) {
        const beam = new Beam(fx, 8, 2.5, '#fde68a');
        crackle(fx, beam, 0.05 + n * 0.08, CHARGE, () => {
          const a = rand(0, TAU);
          const b = a + rand(0.8, 1.6);
          return jag(polar(B.center, 80, a, h / 160), polar(B.center, 80, b, h / 160), 8, 18);
        });
      }
      const SPEED = 1000;
      const reach = Math.max(...fx.stage.enemies.map(e => dist(B.feet, e.feet))) + 80;
      const ring = fx.dot('ring', '#fde68a', 'back');
      const arcs = Array.from({ length: fx.lvl([4, 6, 8]) }, () => new Beam(fx, 6, 2.5, '#fff7d6', 'back'));
      let next = 0;
      fx.tween(CHARGE, reach / SPEED, p => {
        const r = reach * p;
        ring.oval(B.feet.x, B.feet.y, r, r * GROUND, 1 - p * 0.5);
        if (p * 10 >= next || p >= 1) {
          next = Math.floor(p * 10) + 1;
          arcs.forEach(arc => {
            const a = rand(-1.2, 1.2);
            arc.set(jag(polar(B.feet, r, a, GROUND), polar(B.feet, r, a + rand(0.15, 0.35), GROUND), 6, 10), p < 1 ? 0.9 : 0);
          });
        }
      }, [ring]);
      fx.stage.enemies.forEach(e => zap(fx, e, arrival(fx, e, SPEED), fx.lvl([1, 2, 3]), sparks));
    },
  }),
  aura({
    id: 'aura-shock-orbs', name: 'Шаровые искры', group: 'Аура · Шок', spell: 'Shock', duration: 2.1,
    about: 'Из носителя вылетают шаровые молнии и зигзагами несутся к врагам, на цели лопаются с треском',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'bolt', '#fde68a', '#fde68a');
      const sparks = fx.sim(P.spark(C.bolt, 300));
      const trail = fx.sim(P.glow(C.bolt, 0));
      fx.stage.enemies.forEach((e, i) => {
        const start = CHARGE + i * 0.04;
        const from = { x: B.center.x + 20, y: B.center.y - 20 };
        const route = jag(from, e.center, 6, 60);
        const dur = 0.35 + dist(from, e.center) / 2600;
        const core = fx.dot('glow', '#ffffff');
        const halo = fx.dot('glow', '#fde68a');
        const tail = new Beam(fx, 5, 2, '#fde68a');
        fly(fx, start, dur, p => along(route, p), pt => {
          const r = fx.lvl([16, 20, 26]);
          halo.put(pt.x, pt.y, r * 2.4, 0.8 + 0.2 * Math.random());
          core.put(pt.x, pt.y, r * 0.8);
          tail.set(jag({ x: pt.x - rand(10, 20), y: pt.y + rand(-15, 15) }, { x: pt.x + rand(10, 20), y: pt.y + rand(-15, 15) }, 5, 8), 0.8);
          spray(trail, { x: pt.x, y: pt.y, count: fx.count(60), speed: [5, 30], life: [0.15, 0.3], size: [6, 10] });
        }, [core, halo]);
        fx.at(start + dur, () => tail.hide());
        pop(fx, { at: start + dur, dur: 0.3, x: e.center.x, y: e.center.y, r: 90, css: '#fff7d6' });
        zap(fx, e, start + dur, 2, sparks);
      });
    },
  }),

  // ===== Яд =====
  aura({
    id: 'aura-poison-cloud', name: 'Ядовитый туман', group: 'Аура · Яд', spell: 'Poison', duration: 2.6,
    about: 'Из-под ног носителя стелется зелёный туман и ползёт по земле к врагам; кого накрыл — того начинает мутить',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'nature', '#a3e635', '#84cc16');
      const gas = fx.sim(GAS, 'back');
      const gasFront = fx.sim(GAS);
      const bubbles = fx.sim(P.glow(C.venom, 60));
      const x0 = B.feet.x + 30;
      const x1 = Math.max(...fx.stage.enemies.map(e => e.feet.x)) + 100;
      const SPEED = 700;
      const y = Math.max(...fx.stage.enemies.map(e => e.feet.y));
      const yTop = Math.min(...fx.stage.enemies.map(e => e.feet.y));
      fx.tween(CHARGE, (x1 - x0) / SPEED + 0.3, p => {
        const x = lerp(x0, x1, Math.min(p * 1.2, 1));
        for (let i = fx.count(fx.lvl([60, 90, 130])); i > 0; i--) {
          const sim = Math.random() < 0.5 ? gas : gasFront;
          sim.spawn({ x: x + rand(-50, 20), y: rand(yTop - 30, y + 30), vx: rand(20, 90), vy: rand(-15, 5), lifetime: rand(0.9, 1.4), size: rand(22, 40) });
        }
      });
      fx.stage.enemies.forEach(e => sicken(fx, e, CHARGE + (e.feet.x - x0) / SPEED + 0.15, bubbles));
    },
  }),
  aura({
    id: 'aura-poison-spores', name: 'Споры', group: 'Аура · Яд', spell: 'Poison', duration: 2.4,
    about: 'Носитель выпускает облачко спор, они дрейфуют к врагам, кружа, и оседают на них зелёной пыльцой',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'nature', '#a3e635', '#84cc16');
      const puff = fx.sim(GAS);
      const pollen = fx.sim(P.glow(C.venom, -40));
      const bubbles = fx.sim(P.glow(C.venom, 60));
      fx.at(CHARGE, () => spray(puff, { x: B.center.x, y: B.head.y, count: 14, speed: [20, 80], life: [0.8, 1.2], size: [20, 34], jitter: 20 }));
      const per = fx.lvl([2, 3, 4]);
      fx.stage.enemies.forEach((e, i) => {
        let last = 0;
        for (let n = 0; n < per; n++) {
          const start = CHARGE + 0.05 + n * 0.07 + i * 0.03;
          const from = { x: B.center.x + rand(-20, 20), y: B.head.y + rand(-10, 10) };
          const to = { x: e.center.x + rand(-30, 30), y: e.center.y + rand(-50, 10) };
          const via = { x: lerp(from.x, to.x, 0.5), y: Math.min(from.y, to.y) - rand(40, 140) };
          const dur = 0.7 + dist(from, to) / 1800;
          last = Math.max(last, start + dur);
          const spore = fx.dot('glow', '#d9f99d');
          const phase = rand(0, TAU);
          fly(fx, start, dur, p => bez(from, via, to, ease.inOut(p)), (pt, p) => {
            const q = polar(pt, 14 * bump(p), p * 14 + phase);
            spore.put(q.x, q.y, 12, 0.9);
            spray(pollen, { x: q.x, y: q.y, count: fx.count(25), speed: [5, 20], life: [0.3, 0.6], size: [3, 6] });
          }, [spore]);
        }
        sicken(fx, e, last, bubbles);
      });
    },
  }),
  aura({
    id: 'aura-poison-rain', name: 'Ядовитый дождь', group: 'Аура · Яд', spell: 'Poison', duration: 2.6,
    about: 'Над строем врагов сгущается болотная туча и роняет на каждого тяжёлые капли яда',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'nature', '#a3e635', '#84cc16');
      const enemies = fx.stage.enemies;
      const xs = enemies.map(e => e.center.x);
      const cloudY = Math.min(...enemies.map(e => e.head.y)) - 110;
      const cx0 = Math.min(...xs) - 80;
      const cx1 = Math.max(...xs) + 80;
      const cloud = fx.sim({ texture: 'smoke', blend: 'normal', drag: 0.6, gravity: 0, shrink: 1.3, tint: palette(['#4d7c0f', '#365314', '#1a2e05']), alpha: life => bump(life) * 0.55 });
      const mist = fx.sim(P.glow(C.venom, 0));
      const drops = fx.sim({ texture: dropTexture('venom'), blend: 'normal', drag: 0.05, gravity: 1500, shrink: 1 });
      const splash = fx.sim(P.spark(C.venom, 600));
      const bubbles = fx.sim(P.glow(C.venom, 60));
      const beam = { from: { x: B.head.x, y: B.head.y - 10 }, to: { x: (cx0 + cx1) / 2, y: cloudY } };
      const lift = fx.sim(P.glow(C.venom, 0));
      fx.tween(CHARGE - 0.1, 0.35, p => {
        const pt = bez(beam.from, { x: lerp(beam.from.x, beam.to.x, 0.5), y: cloudY - 120 }, beam.to, p);
        spray(lift, { x: pt.x, y: pt.y, count: fx.count(160), speed: [5, 30], life: [0.3, 0.5], size: [6, 12] });
      });
      fx.tween(CHARGE, 1.8, p => {
        const a = fade(p, 0.15, 0.3);
        for (let i = fx.count(70 * a); i > 0; i--) cloud.spawn({ x: rand(cx0, cx1), y: cloudY + rand(-25, 25), vx: rand(-15, 15), vy: rand(-5, 5), lifetime: rand(0.8, 1.3), size: rand(30, 50) });
        for (let i = fx.count(20 * a); i > 0; i--) mist.spawn({ x: rand(cx0, cx1), y: cloudY + rand(-10, 30), vx: 0, vy: 10, lifetime: 0.6, size: rand(10, 18) });
      });
      enemies.forEach((e, i) => {
        const fall = e.head.y - cloudY;
        const t = Math.sqrt((2 * fall) / 1500);
        const per = fx.lvl([1, 2, 3]);
        for (let n = 0; n < per; n++) {
          const start = CHARGE + 0.35 + i * 0.07 + n * 0.12;
          fx.at(start, () => drops.spawn({ x: e.center.x + rand(-14, 14), y: cloudY + 20, vx: 0, vy: 0, lifetime: t, size: 11, rotation: Math.PI / 2 }));
          fx.at(start + t, () => spray(splash, { x: e.center.x, y: e.head.y, count: 10, speed: [80, 220], dir: -Math.PI / 2, spread: 1.2, life: [0.25, 0.5], size: [2, 4] }));
        }
        sicken(fx, e, CHARGE + 0.35 + i * 0.07 + t, bubbles);
      });
    },
  }),

  // ===== Выжигание энергии (тьма) =====
  aura({
    id: 'aura-void-pulse', name: 'Тёмный пульс', group: 'Аура · Выжигание энергии', spell: 'EnergyBurn', duration: 2.2,
    about: 'Носитель сжимается в чёрную точку и выпускает кольцо тьмы; проходя сквозь врагов, оно вырывает из них голубые искры энергии, и те сгорают',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'dark', '#a855f7', '#7e22ce');
      const core = fx.dot('glow', '#09030f', 'front', 'normal');
      fx.tween(CHARGE - 0.25, 0.35, p => core.put(B.center.x, B.center.y, 70 * (1 - ease.in(p)) + 10, bump(p) * 0.9), [core]);
      const SPEED = 900;
      const reach = Math.max(...fx.stage.enemies.map(e => dist(B.center, e.center))) + 100;
      const shell = fx.dot('glow', '#1e0b36', 'front', 'normal');
      const rim = fx.dot('ring', '#c084fc');
      fx.tween(CHARGE, reach / SPEED, p => {
        const r = reach * ease.out(p);
        shell.oval(B.center.x, B.center.y, r, r * 0.55, (1 - p) * 0.35);
        rim.oval(B.center.x, B.center.y, r, r * 0.55, (1 - p) * fx.lvl([0.6, 0.8, 1]));
      }, [shell, rim]);
      const motes = fx.sim({ ...P.glow(ENERGY, 80), tint: palette(ENERGY) });
      const soot = fx.sim(SOOT);
      fx.stage.enemies.forEach(e => {
        const d = dist(B.center, e.center) / reach;
        drain(fx, e, CHARGE + (reach / SPEED) * (1 - Math.sqrt(1 - d)), motes, soot);
      });
    },
  }),
  aura({
    id: 'aura-void-eyes', name: 'Взгляд бездны', group: 'Аура · Выжигание энергии', spell: 'EnergyBurn', duration: 2.4,
    about: 'Над каждым врагом из тьмы открываются злобные глаза и впиваются взглядом — энергия выгорает из головы фиолетовым пламенем',
    build(fx) {
      charge(fx, 'dark', '#a855f7', '#7e22ce');
      const motes = fx.sim({ ...P.glow(ENERGY, 80), tint: palette(ENERGY) });
      const soot = fx.sim(SOOT);
      const flame = fx.sim({ texture: 'smoke', drag: 0.2, gravity: -380, shrink: 0.3, stretch: [0.7, 1.8], tint: palette(['#f5d0fe', '#c084fc', '#7e22ce', '#2e1065']), alpha: life => bump(life) * 0.9 });
      fx.stage.enemies.forEach((e, i) => {
        const at = CHARGE + 0.1 + i * 0.08;
        const s = { x: e.head.x, y: e.head.y - 70 };
        const murk = fx.dot('glow', '#09030f', 'front', 'normal');
        const eyes = fx.gfx('front', 'normal');
        const glow = fx.dot('glow', '#a855f7');
        fx.tween(at, 1.2, p => {
          const a = fade(p, 0.15, 0.3);
          const open = p < 0.2 ? ease.out(p / 0.2) : p > 0.8 ? 1 - (p - 0.8) / 0.2 : 1;
          murk.oval(s.x, s.y, 70, 38, a * 0.8);
          glow.oval(s.x, s.y, 80, 44, a * 0.4);
          eyes.clear();
          for (const side of [-1, 1]) angryEye(eyes, s.x + side * 20, s.y, side, 14, 14, open, '#a855f7', a);
        }, [murk, glow]);
        fx.at(at + 1.2, () => eyes.clear());
        const burn = at + 0.35;
        drain(fx, e, burn, motes, soot);
        fx.tween(burn, 0.6, p => {
          for (let n = fx.count(fx.lvl([60, 90, 120]) * (1 - p)); n > 0; n--) flame.spawn({ x: e.head.x + rand(-22, 22), y: e.head.y + rand(0, 20), vx: rand(-20, 20), vy: rand(-200, -100), lifetime: rand(0.3, 0.5), size: rand(8, 15) });
        });
      });
    },
  }),
  aura({
    id: 'aura-void-tendrils', name: 'Пожирающие нити', group: 'Аура · Выжигание энергии', spell: 'EnergyBurn', duration: 2.4,
    about: 'Из тени носителя тянутся чёрные нити к каждому врагу; по ним текут искры энергии и гаснут в тёмном ядре',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'dark', '#a855f7', '#7e22ce');
      const soot = fx.sim(SOOT);
      const motes = fx.sim({ ...P.glow(ENERGY, 80), tint: palette(ENERGY) });
      const core = fx.dot('glow', '#09030f', 'front', 'normal');
      const coreRim = fx.dot('ring', '#a855f7');
      const hub = { x: B.center.x, y: B.center.y - 10 };
      const GROW = 0.35;
      const HOLD = 0.9;
      fx.tween(CHARGE, GROW + HOLD + 0.3, p => {
        const a = fade(p, 0.15, 0.25);
        core.put(hub.x, hub.y, 34 + 6 * Math.sin(p * 30), a * 0.9);
        coreRim.put(hub.x, hub.y, 40, a * 0.8);
      }, [core, coreRim]);
      fx.stage.enemies.forEach((e, i) => {
        const at = CHARGE + i * 0.04;
        const to = e.center;
        const bend = { x: lerp(hub.x, to.x, 0.5), y: Math.min(hub.y, to.y) - rand(30, 90) };
        const shadow = fx.strand(16, fx.lvl([8, 10, 13]), '#1e0b36', 1);
        shadow.rope.blendMode = 'normal';
        const glow = fx.strand(16, 3, '#c084fc', 1);
        const spark = Array.from({ length: fx.lvl([2, 3, 4]) }, () => fx.dot('glow', '#67e8f9'));
        const curve = (t: number, time: number): Pt => {
          const pt = bez(hub, bend, to, t);
          const wave = Math.sin(t * 9 - time * 14) * 8 * bump(t);
          return { x: pt.x, y: pt.y + wave };
        };
        fx.tween(at, GROW + HOLD + 0.3, p => {
          const time = p * (GROW + HOLD + 0.3);
          const len = clamp01(time / GROW);
          const a = time > GROW + HOLD ? 1 - (time - GROW - HOLD) / 0.3 : 1;
          const pts = Array.from({ length: 16 }, (_, k) => curve((k / 15) * len, time));
          for (const strand of [shadow, glow]) {
            strand.rope.visible = a > 0.01;
            strand.rope.alpha = a * (strand === glow ? 0.7 : 0.85);
            strand.set(pts);
          }
          spark.forEach((dot, k) => {
            const t = 1 - (((time - GROW) * 1.6 + k / spark.length) % 1);
            const pt = curve(Math.max(t, 0), time);
            dot.put(pt.x, pt.y, 10, time > GROW && a > 0 ? 0.9 * a : 0);
          });
          if (p >= 1) {
            shadow.rope.visible = false;
            glow.rope.visible = false;
          }
        }, spark);
        drain(fx, e, at + GROW, motes, soot);
      });
    },
  }),

  // ===== Кровотечение =====
  aura({
    id: 'aura-blood-heart', name: 'Сердцебиение', group: 'Аура · Кровотечение', spell: 'Bleed', duration: 2.4,
    about: 'Носитель дважды бьётся, как сердце: от каждого удара по земле идёт алая волна, на втором ударе у врагов открываются раны',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'blood', '#ef4444', '#dc2626');
      const h = bodyOf(B);
      const drops = fx.sim({ texture: dropTexture('blood'), blend: 'normal', drag: 0.4, gravity: 1100, shrink: 0.75 });
      const SPEED = 1100;
      const reach = Math.max(...fx.stage.enemies.map(e => dist(B.feet, e.feet))) + 80;
      const beats = [CHARGE - 0.1, CHARGE + 0.25];
      beats.forEach((beat, i) => {
        const pulse = fx.dot('glow', '#dc2626');
        fx.tween(beat, 0.3, p => pulse.oval(B.center.x, B.center.y, 80 * (0.7 + 0.5 * ease.out(p)), h * 0.6 * (0.7 + 0.5 * ease.out(p)), (1 - p) * 0.7), [pulse]);
        fx.at(beat, () => fx.stage.shake(i ? 4 : 2, 0.15));
        ringWave(fx, { at: beat, dur: reach / SPEED, x: B.feet.x, y: B.feet.y, from: 40, to: reach, css: i ? '#ef4444' : '#991b1b', squash: GROUND, layer: 'back', alpha: i ? 1 : 0.6 });
      });
      fx.stage.enemies.forEach(e => {
        const at = beats[1] + dist(B.feet, e.feet) / SPEED;
        wound(fx, e, at, drops);
        const g = fx.gfx('front', 'normal');
        const center = e.center;
        fx.tween(at, 0.4, p => {
          g.clear();
          const pts = Array.from({ length: 8 }, (_, k) => polar(center, lerp(-38, 38, k / 7), -0.8));
          taper(g, pts, 10 * fx.lvl([0.8, 1, 1.2]), '#b91c1c', 1 - ease.in(p));
        });
        fx.at(at + 0.4, () => g.clear());
      });
    },
  }),
  aura({
    id: 'aura-blood-crescents', name: 'Кровавые серпы', group: 'Аура · Кровотечение', spell: 'Bleed', duration: 2.2,
    about: 'С носителя срываются алые серпы и, вращаясь, полосуют каждого врага крест-накрест',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'blood', '#ef4444', '#dc2626');
      const drops = fx.sim({ texture: dropTexture('blood'), blend: 'normal', drag: 0.4, gravity: 1100, shrink: 0.75 });
      const mist = fx.sim(P.glow(C.blood, 0));
      fx.stage.enemies.forEach((e, i) => {
        const start = CHARGE + i * 0.05;
        const from = { x: B.center.x + 30, y: B.center.y - 10 };
        const to = e.center;
        const dur = 0.25 + dist(from, to) / 2400;
        const blade = fx.gfx('front', 'add');
        fly(fx, start, dur, p => arc(from, to, p, 40), (pt, p, dir) => {
          blade.clear();
          const spin = dir + p * 10;
          const pts = Array.from({ length: 9 }, (_, k) => polar(pt, 26, spin + lerp(-1.4, 1.4, k / 8)));
          taper(blade, pts, fx.lvl([9, 11, 14]), '#f87171', 0.95);
          spray(mist, { x: pt.x, y: pt.y, count: fx.count(60), speed: [5, 30], life: [0.2, 0.4], size: [5, 9] });
        });
        const hit = start + dur;
        fx.at(hit, () => blade.clear());
        const cut = fx.gfx('front', 'add');
        fx.tween(hit, 0.45, p => {
          cut.clear();
          const a = 1 - ease.in(p);
          const grow = ease.out3(Math.min(p * 4, 1));
          for (const [ang, delay] of [[-0.8, 0], [0.8, 0.12]] as const) {
            const g2 = clamp01(grow - delay * 4);
            const pts = Array.from({ length: 8 }, (_, k) => polar(to, lerp(-42, 42, (k / 7) * g2), ang));
            taper(cut, pts, 9, '#fca5a5', a);
          }
        });
        fx.at(hit + 0.45, () => cut.clear());
        wound(fx, e, hit, drops);
      });
    },
  }),
  aura({
    id: 'aura-blood-thorns', name: 'Кровавые шипы', group: 'Аура · Кровотечение', spell: 'Bleed', duration: 2.3,
    about: 'По земле к врагам бежит тёмная кровавая жилка, под каждым из земли выстреливают алые шипы и впиваются в ноги',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'blood', '#ef4444', '#dc2626');
      const drops = fx.sim({ texture: dropTexture('blood'), blend: 'normal', drag: 0.4, gravity: 1100, shrink: 0.75 });
      const vein = fx.gfx('back', 'normal');
      const SPEED = 1000;
      const paths = fx.stage.enemies.map(e => ({ e, pts: jag(B.feet, e.feet, 10, 18), dur: dist(B.feet, e.feet) / SPEED }));
      const total = Math.max(...paths.map(p => p.dur));
      fx.tween(CHARGE, total + 0.9, p => {
        const time = p * (total + 0.9);
        const a = fade(p, 0.02, 0.3);
        vein.clear();
        for (const path of paths) {
          const pts = prefix(path.pts, time / path.dur);
          vein.moveTo(pts[0].x, pts[0].y);
          pts.slice(1).forEach(pt => vein.lineTo(pt.x, pt.y));
        }
        vein.stroke({ width: 4, color: color('#7f1d1d'), alpha: 0.85 * a });
      });
      paths.forEach(({ e, dur }) => {
        const at = CHARGE + dur;
        const spikes = fx.gfx('front', 'normal');
        const back = fx.gfx('back', 'normal');
        const count = fx.lvl([3, 4, 6]);
        const list = Array.from({ length: count }, (_, k) => {
          const a = (k / count) * TAU + rand(-0.3, 0.3);
          return { base: polar(e.feet, rand(18, 38), a, GROUND), lean: Math.cos(a) * 0.5, h: rand(55, 85), behind: Math.sin(a) < 0 };
        });
        fx.tween(at, 0.7, p => {
          spikes.clear();
          back.clear();
          const grow = p < 0.25 ? ease.out3(p / 0.25) : 1 - ease.in(clamp01((p - 0.6) / 0.4));
          for (const s of list) {
            const g = s.behind ? back : spikes;
            const tip = { x: s.base.x + Math.sin(s.lean) * s.h * grow, y: s.base.y - Math.cos(s.lean) * s.h * grow };
            g.poly([s.base.x - 7, s.base.y, tip.x, tip.y, s.base.x + 7, s.base.y]).fill({ color: color('#991b1b') });
            g.poly([s.base.x - 2, s.base.y, tip.x, tip.y, s.base.x + 3, s.base.y]).fill({ color: color('#f87171'), alpha: 0.8 });
          }
        });
        fx.at(at + 0.7, () => {
          spikes.clear();
          back.clear();
        });
        wound(fx, e, at + 0.05, drops);
      });
    },
  }),

  // ===== Регенерация =====
  aura({
    id: 'aura-regen-bloom', name: 'Тёплое кольцо', group: 'Аура · Регенерация', spell: 'Regen', duration: 2.2,
    heals: ['all', 'all', 'all'],
    about: 'Под носителем распускается золотистое кольцо и расходится по земле к союзникам; у каждого поднимаются плюсики и листья',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'light', '#fde68a', '#fbbf24');
      const pluses = fx.sim(P.plus(['#f7fee7', '#bef264', '#4ade80'], 90));
      const leaves = fx.sim({ texture: leafTexture(), blend: 'normal', drag: 0.6, gravity: -50, shrink: 0.8, alpha: life => bump(life) });
      const SPEED = 700;
      const reach = Math.max(80, ...fx.stage.healed.map(u => dist(B.feet, u.feet))) + 80;
      ringWave(fx, { at: CHARGE, dur: reach / SPEED + 0.2, x: B.feet.x, y: B.feet.y, from: 40, to: reach, css: '#fde68a', squash: GROUND, layer: 'back' });
      ringWave(fx, { at: CHARGE + 0.1, dur: reach / SPEED + 0.2, x: B.feet.x, y: B.feet.y, from: 30, to: reach * 0.92, css: '#bef264', squash: GROUND, layer: 'back', alpha: 0.6 });
      fx.stage.healed.forEach(u => {
        const at = arrival(fx, u, SPEED);
        mend(fx, u, at, pluses);
        fx.at(at, () => spray(leaves, { x: u.feet.x, y: u.feet.y - 10, count: fx.lvl([2, 4, 6]), speed: [40, 120], dir: -Math.PI / 2, spread: 1.1, life: [0.7, 1.1], size: [9, 14], spin: 4 }));
      });
    },
  }),
  aura({
    id: 'aura-regen-fireflies', name: 'Светлячки', group: 'Аура · Регенерация', spell: 'Regen', duration: 2.3,
    heals: ['all', 'all', 'all'],
    about: 'Из носителя вылетают тёплые светлячки и, петляя, слетаются к союзникам — каждый вливается в тело мягкой вспышкой',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'light', '#fde68a', '#fbbf24');
      const pluses = fx.sim(P.plus(['#fffbeb', '#fde68a', '#bef264'], 90));
      const trail = fx.sim(P.glow(['#fffbeb', '#fde68a', '#bef264'], 0));
      const per = fx.lvl([1, 2, 3]);
      fx.stage.healed.forEach((u, i) => {
        let last = 0;
        for (let n = 0; n < per; n++) {
          const start = CHARGE + i * 0.06 + n * 0.1;
          const from = { x: B.center.x + rand(-20, 20), y: B.head.y };
          const to = { x: u.center.x + rand(-15, 15), y: u.center.y + rand(-20, 10) };
          const via = { x: lerp(from.x, to.x, 0.5) + rand(-60, 60), y: Math.min(from.y, to.y) - rand(60, 160) };
          const dur = 0.6 + dist(from, to) / 1600;
          last = Math.max(last, start + dur);
          const bug = fx.dot('glow', '#fde68a');
          const core = fx.dot('glow', '#ffffff');
          fly(fx, start, dur, p => bez(from, via, to, ease.inOut(p)), (pt, p) => {
            const flicker = 0.75 + 0.25 * Math.sin(p * 40 + n);
            bug.put(pt.x, pt.y, 22, flicker);
            core.put(pt.x, pt.y, 7, flicker);
            spray(trail, { x: pt.x, y: pt.y, count: fx.count(40), speed: [5, 20], life: [0.3, 0.5], size: [4, 7] });
          }, [bug, core]);
        }
        mend(fx, u, last, pluses);
      });
    },
  }),
  aura({
    id: 'aura-regen-sunbeams', name: 'Солнечные лучи', group: 'Аура · Регенерация', spell: 'Regen', duration: 2.3,
    heals: ['all', 'all', 'all'],
    about: 'Носитель поднимает свет вверх, и на каждого союзника сверху опускается тёплый луч, над головой загорается нимб',
    build(fx) {
      const B = fx.stage.caster;
      charge(fx, 'light', '#fde68a', '#fbbf24');
      const pluses = fx.sim(P.plus(['#fffbeb', '#fde68a', '#bef264'], 90));
      const motes = fx.sim(P.star(C.light, 40));
      const rise = { x: B.center.x, y: B.head.y - 20 };
      const up = fx.dot('glow', '#fde68a');
      fx.tween(CHARGE - 0.15, 0.35, p => up.oval(rise.x, rise.y - 120 * p, 26, 70, bump(p)), [up]);
      fx.stage.healed.forEach((u, i) => {
        const at = CHARGE + 0.2 + i * 0.07;
        const beam = fx.dot('glow', '#fde68a');
        const core = fx.dot('glow', '#fffbeb');
        const nimbus = fx.dot('ring', '#fde68a');
        const top = -40;
        fx.tween(at, 1, p => {
          const a = fade(p, 0.1, 0.45);
          const reach = ease.out3(Math.min(p * 4, 1));
          const len = (u.feet.y - top) * reach;
          beam.oval(u.center.x, top + len / 2, fx.lvl([38, 46, 56]), len / 2, a * 0.6);
          core.oval(u.center.x, top + len / 2, 14, len / 2, a * 0.7);
          nimbus.oval(u.head.x, u.head.y - 18 - 6 * p, 26, 8, a);
          for (let n = fx.count(24 * a); n > 0; n--) motes.spawn({ x: u.center.x + rand(-30, 30), y: rand(u.head.y - 40, u.feet.y), vx: 0, vy: rand(-40, -10), lifetime: rand(0.4, 0.7), size: rand(4, 8) });
        }, [beam, core, nimbus]);
        mend(fx, u, at + 0.15, pluses);
      });
    },
  }),
];

