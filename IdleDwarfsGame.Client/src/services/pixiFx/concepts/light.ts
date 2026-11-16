import { Sprite, type Graphics } from 'pixi.js';
import {
  Beam, C, GROUND, P, TAU, angle, bez, bump, dist, ease, fade, fly, groundGlow, lerp, levelAccent, mix, palette, polar, pop, rand, ringWave, spray,
  color, type Concept, type Fx, type Pt,
} from './kit';
import { featherTexture } from './sprites';

// Концепты сияния и тайной магии
const GOLD = '#fde68a';
const RAINBOW = ['#f87171', '#fb923c', '#facc15', '#4ade80', '#38bdf8', '#a78bfa'];
const MOTE = { texture: 'star' as const, drag: 0.4, gravity: -110, shrink: 0.6, tint: palette(C.light), alpha: (life: number) => bump(life) };

// Стеклянная призма: треугольник с гранями и радужными бликами по рёбрам. spin — поворот вокруг вертикали (ширина ∝ cos)
const drawPrism = (g: Graphics, x: number, y: number, size: number, spin: number, alpha: number) => {
  g.clear();
  if (alpha <= 0.01) return;
  const w = Math.max(0.15, Math.abs(Math.cos(spin)));
  const depth = Math.sin(spin) * size * 0.35;
  const front: Pt[] = [{ x, y: y - size }, { x: x + size * 0.87 * w, y: y + size * 0.5 }, { x: x - size * 0.87 * w, y: y + size * 0.5 }];
  const back = front.map(p => ({ x: p.x + depth, y: p.y - Math.abs(depth) * 0.3 }));
  const poly = (pts: Pt[], css: string, a: number) => g.poly(pts.flatMap(p => [p.x, p.y])).fill({ color: color(css), alpha: a * alpha });
  poly(back, '#e0f2fe', 0.12);
  poly([front[1], back[1], back[2], front[2]], '#bae6fd', 0.14);
  poly([front[0], back[0], back[1], front[1]], '#ffffff', 0.1);
  poly(front, '#ffffff', 0.22);
  // Внутренний свет
  poly([front[0], { x: x + size * 0.3 * w, y: y + size * 0.2 }, { x: x - size * 0.3 * w, y: y + size * 0.2 }], '#ffffff', 0.25);
  for (const [a, b] of [[0, 1], [1, 2], [2, 0]] as const) {
    g.moveTo(front[a].x, front[a].y).lineTo(front[b].x, front[b].y).stroke({ width: 2, color: 0xffffff, alpha: 0.85 * alpha });
    g.moveTo(back[a].x, back[a].y).lineTo(back[b].x, back[b].y).stroke({ width: 1, color: 0xffffff, alpha: 0.35 * alpha });
    g.moveTo(front[a].x, front[a].y).lineTo(back[a].x, back[a].y).stroke({ width: 1, color: 0xffffff, alpha: 0.35 * alpha });
  }
  // Радужный блик бежит по правому ребру
  RAINBOW.forEach((css, i) => {
    const t0 = (i / RAINBOW.length) * 0.6 + 0.2;
    const t1 = t0 + 0.1;
    const a = mix(front[0], front[1], t0);
    const b = mix(front[0], front[1], t1);
    g.moveTo(a.x + 3, a.y).lineTo(b.x + 3, b.y).stroke({ width: 3, color: color(css), alpha: 0.9 * alpha });
  });
};

// Срабатывание «Призменной линзы»: случайное заклинание срывается с радужной вспышки у кастера
export const prismFlourish = (fx: Fx) => {
  const { caster } = fx.stage;
  const c = { x: caster.center.x + 40, y: caster.center.y - 40 };
  const g = fx.gfx('front', 'add');
  fx.tween(0, 0.5, p => drawPrism(g, c.x, c.y, 30 * ease.back(Math.min(p * 3, 1)), p * 6, 1 - ease.in(p)));
  RAINBOW.forEach((css, i) => ringWave(fx, { at: 0.02 * i, dur: 0.45, x: c.x, y: c.y, from: 10, to: 90 + i * 8, css, alpha: 0.7 }));
  const glitter = fx.sim({ texture: 'star', drag: 0.5, gravity: 80, shrink: 0.6, tint: palette(RAINBOW), alpha: life => bump(life) });
  fx.at(0.05, () => spray(glitter, { x: c.x, y: c.y, count: 24, speed: [80, 300], life: [0.4, 0.8], size: [5, 10] }));
};

export const LIGHT_CONCEPTS: Concept[] = [
  {
    id: 'prism-crystal', name: 'Кристальная призма', group: 'Сияние', spell: 'Prism Ray', duration: 2.6,
    levels: true,
    targets: [1, 1, 2],
    artifacts: [{
      key: 'PrismLens', name: 'Призменная линза', kind: 'SpellProcPool',
      about: 'После луча призма выплёскивает случайное заклинание другой стихии в случайного врага',
      then: { concept: ['fire-serpent', 'frost-lance', 'bolt-sky', 'void-orb', 'venom-spit'], level: 1, targets: 'one', after: 0.75 },
    }],
    about: 'Перед кастером проступает стеклянная призма; белый луч входит в неё и выходит радужными лучами в цели — у каждой свой цвет. От «Солнечного диска» луч падает в призму с неба',
    build(fx) {
      const { caster, enemies } = fx.stage;
      // Срабатывание «Солнечного диска»: призма висит над врагами, белый луч бьёт в неё с солнца
      const sun = fx.triggered('CleanseAllies');
      const ex = enemies.reduce((sum, e) => sum + e.center.x, 0) / enemies.length;
      const at = sun ? { x: ex - 140, y: Math.min(...enemies.map(e => e.head.y)) - 150 } : { x: caster.center.x + 170, y: caster.center.y - 70 };
      const hand = sun ? { x: at.x - 260, y: -40 } : { x: caster.center.x + 45, y: caster.center.y - 30 };
      if (sun) {
        const disc = fx.dot('glow', '#fde68a', 'back');
        const discCore = fx.dot('glow', '#fffbeb', 'back');
        fx.tween(0, 2, p => {
          disc.put(hand.x, hand.y + 40, 220, 0.6 * fade(p, 0.15, 0.3));
          discCore.put(hand.x, hand.y + 40, 90, 0.8 * fade(p, 0.15, 0.3));
        }, [disc, discCore]);
      }
      const prism = fx.gfx('front', 'add');
      const halo = fx.dot('glow', '#ffffff');
      const beam = new Beam(fx, 2, 12, '#fffbeb');
      // Луч в каждую цель (1 / 1 / 2 по уровню)
      const targets = enemies;
      const rays = targets.map((_, i) => new Beam(fx, 2, 8, RAINBOW[i % RAINBOW.length]));
      const motes = fx.sim(MOTE);
      const glitter = fx.sim({ texture: 'star', drag: 0.5, gravity: 120, shrink: 0.6, tint: palette(RAINBOW), alpha: life => bump(life) });
      const sparks = fx.sim(P.spark(RAINBOW, 0));
      const FORM = 0.15;
      const FIRE = 0.6;
      const STOP = 1.75;
      fx.at(0, () => fx.stage.cast(GOLD));
      fx.tween(FORM, STOP + 0.25 - FORM, p => {
        const t = FORM + p * (STOP + 0.25 - FORM);
        const a = Math.min((t - FORM) / 0.3, 1) * (t > STOP ? 1 - (t - STOP) / 0.25 : 1);
        const spin = t * 2.2;
        drawPrism(prism, at.x, at.y + Math.sin(t * 3) * 4, 46 * ease.back(Math.min((t - FORM) / 0.3, 1)), spin, a);
        halo.put(at.x, at.y, 90, (t > FIRE ? 0.55 : 0.25) * a);
        if (t < FIRE) spray(motes, { x: at.x, y: at.y, count: fx.count(40), speed: [20, 80], life: [0.3, 0.6], size: [5, 10], jitter: 50 });
      }, [halo]);
      fx.tween(FIRE - 0.1, 0.1, p => beam.set([hand, mix(hand, at, p)]));
      fx.tween(FIRE, STOP - FIRE, p => {
        const a = fade(p, 0, 0.15) * (0.85 + 0.15 * Math.sin(p * 70));
        beam.set([hand, at], a);
        targets.forEach((enemy, i) => {
          const reach = ease.out3(Math.min(p * 4 - i * 0.15, 1));
          if (reach <= 0) return;
          const end = mix(at, enemy.center, reach);
          rays[i].set([at, end], a * 0.95);
          if (reach >= 1) spray(sparks, { x: enemy.center.x, y: enemy.center.y, count: fx.count(30), speed: [80, 300], life: [0.3, 0.6], size: [3, 6] });
        });
      });
      fx.at(STOP, () => {
        beam.hide();
        rays.forEach(ray => ray.hide());
        spray(glitter, { x: at.x, y: at.y, count: 40, speed: [100, 360], life: [0.5, 1], size: [6, 12] });
      });
      targets.forEach((enemy, i) => {
        const hit = FIRE + 0.08 + i * 0.04;
        fx.at(hit, () => enemy.hit(i === 0, RAINBOW[i % RAINBOW.length]));
        fx.at(hit + 0.6, () => enemy.hit(false, RAINBOW[i % RAINBOW.length]));
        pop(fx, { at: hit, dur: 0.35, x: enemy.center.x, y: enemy.center.y, r: 90, css: RAINBOW[i % RAINBOW.length], alpha: 0.8 });
        if (fx.since(3)) levelAccent(fx, 'light', hit, enemy);
      });
      fx.at(FIRE + 0.08, () => fx.stage.shake(5, 0.25));
      pop(fx, { at: FIRE, dur: 0.35, x: at.x, y: at.y, r: 150, css: '#ffffff' });
    },
  },
  {
    id: 'light-blade', name: 'Клинок света', group: 'Сияние', spell: 'Prism Ray', duration: 2.2,
    levels: true,
    extras: true,
    about: 'В небе вспыхивает огромный меч из света и вонзается в цель',
    build(fx) {
      const { enemies } = fx.stage;
      const T = enemies[0];
      const { x, y } = T.feet;
      const blade = fx.dot('glow', GOLD);
      const core = fx.dot('glow', '#ffffff');
      const guard = fx.dot('glow', GOLD);
      const glint = fx.dot('star', '#ffffff');
      const motes = fx.sim(MOTE);
      const sparks = fx.sim(P.spark(C.light, 400));
      const DROP = 0.75;
      const HIT = DROP + 0.14;
      // Меч крупнее с уровнем
      const K = fx.lvl([0.7, 0.85, 1]);
      const draw = (cy: number, alpha: number) => {
        blade.oval(x, cy, 46 * K, 300 * K, alpha);
        core.oval(x, cy, 13 * K, 280 * K, alpha);
        guard.oval(x, cy - 170 * K, 110 * K, 22 * K, alpha);
      };
      fx.at(0, () => fx.stage.cast(GOLD));
      fx.tween(0.15, DROP - 0.15, p => {
        glint.put(x, y - 620, 70 * bump(p * 0.8 + 0.1), 1, p * 2);
        draw(y - 640, ease.in(p) * 0.8);
      }, [glint]);
      fx.tween(DROP, HIT - DROP, p => draw(lerp(y - 640, y - 230, ease.in(p)), 1));
      fx.tween(HIT, 0.7, p => {
        draw(y - 230, 1 - ease.in(p));
        spray(motes, { x, y: y - rand(0, 420), count: fx.count(60 * (1 - p)), speed: [10, 70], life: [0.5, 1], size: [5, 11], jitter: 30 });
      }, [blade, core, guard]);
      fx.at(HIT, () => {
        T.hit(true, GOLD);
        fx.stage.flash('#fffbeb', 0.4, 0.25);
        fx.stage.shake(12, 0.35);
        spray(sparks, { x, y, count: 60, speed: [200, 760], dir: -Math.PI / 2, spread: 1.3, life: [0.4, 0.9], size: [3, 8] });
      });
      pop(fx, { at: HIT, dur: 0.45, x, y: y - 60, r: 260, css: GOLD });
      ringWave(fx, { at: HIT, dur: 0.6, x, y, from: 30, to: 300, css: '#fffbeb', squash: GROUND, layer: 'back' });
      if (fx.since(2)) ringWave(fx, { at: HIT + 0.1, dur: 0.6, x, y, from: 30, to: 220, css: '#fbbf24', squash: GROUND, layer: 'back', alpha: 0.7 });
      if (fx.since(3)) levelAccent(fx, 'light', HIT, T);
      // Доп. эффект: рядом падают два клинка поменьше
      if (fx.extras) {
        fx.stage.bystanders.slice(0, 2).forEach((enemy, i) => {
          const ex = enemy.feet.x;
          const ey = enemy.feet.y;
          const b = fx.dot('glow', GOLD);
          const bc = fx.dot('glow', '#ffffff');
          const at = HIT + 0.15 + i * 0.12;
          fx.tween(at - 0.12, 0.12, p => {
            const cy = lerp(ey - 520, ey - 150, ease.in(p));
            b.oval(ex, cy, 26, 170, 0.9);
            bc.oval(ex, cy, 8, 160, 0.9);
          });
          fx.tween(at, 0.5, p => {
            b.oval(ex, ey - 150, 26, 170, 1 - ease.in(p));
            bc.oval(ex, ey - 150, 8, 160, 1 - ease.in(p));
          }, [b, bc]);
          fx.at(at, () => {
            enemy.hit(true, GOLD);
            spray(sparks, { x: ex, y: ey, count: 26, speed: [150, 500], dir: -Math.PI / 2, spread: 1.2, life: [0.4, 0.8], size: [3, 7] });
          });
          pop(fx, { at, dur: 0.35, x: ex, y: ey - 40, r: 150, css: GOLD });
        });
      }
      groundGlow(fx, { at: HIT, dur: 1.1, x, y, r: 200, css: '#fbbf24' });
    },
  },
  {
    id: 'supernova', name: 'Сверхновая', group: 'Сияние', spell: 'Radiant Nova', duration: 2.5,
    levels: true,
    targets: ['all', 'all', 'all'],
    heals: [0, 'all', 'all'],
    artifacts: [{
      key: 'SunDisc', name: 'Солнечный диск', kind: 'CleanseAllies',
      about: 'Вспышка снимает с союзников по одному негативному статусу, за каждый снятый с солнца в призму падает луч',
      then: { concept: 'prism-crystal', level: 1, targets: 'one', after: 0.7 },
    }],
    about: 'Свет стягивается к кастеру и взрывается расходящейся сферой с лучами; вспышку окружают радужное гало и солнечные блики',
    build(fx) {
      const { caster, enemies } = fx.stage;
      const c = { x: caster.center.x, y: caster.center.y - 20 };
      // Срабатывание «Утренней звезды» (союзник при смерти): вспышка рассветная, в центре — четырёхлучевая звезда
      const morning = fx.triggered('SpellProcAllyLow');
      if (morning) {
        const star = Array.from({ length: 4 }, () => new Beam(fx, 2, 6, '#fed7aa'));
        fx.tween(0.2, 1.4, p => {
          const a = fade(p, 0.2, 0.4);
          star.forEach((ray, i) => {
            const ang = (i / 4) * TAU + Math.PI / 4 + p * 0.5;
            const len = (i % 2 ? 160 : 220) * ease.out(Math.min(p * 2, 1));
            ray.set([polar(c, -len, ang), polar(c, len, ang)], a);
          });
          if (p >= 1) star.forEach(ray => ray.hide());
        });
      }
      const core = fx.dot('glow', '#ffffff');
      const halo = fx.dot('glow', GOLD);
      const pull = fx.sim({ texture: 'star', drag: 1, shrink: 0.6, alpha: life => bump(life) });
      const sphere = fx.dot('ring', '#fffbeb');
      const sphere2 = fx.dot('ring', '#fbbf24');
      // Лучей и радиус больше с уровнем
      const rays = Array.from({ length: fx.lvl([8, 11, 14]) }, () => new Beam(fx, 2, 10, GOLD));
      const motes = fx.sim(MOTE);
      const NOVA = 0.75;
      const REACH = 760 * fx.lvl([0.6, 0.8, 1]);
      fx.at(0, () => fx.stage.cast(GOLD));
      fx.tween(0, NOVA, p => {
        halo.put(c.x, c.y, 50 + 110 * p, 0.4 + 0.6 * p);
        core.put(c.x, c.y, 12 + 30 * p);
        for (let i = fx.count(130); i > 0; i--) {
          const a = Math.random() * TAU;
          const start = polar(c, rand(170, 300), a);
          pull.spawn({ x: start.x, y: start.y, vx: -Math.cos(a) * 640, vy: -Math.sin(a) * 640, lifetime: 0.34, size: rand(5, 10), tint: 0xfde68a, spin: 6 });
        }
      });
      fx.tween(NOVA, 0.8, p => {
        const r = REACH * ease.out3(p);
        sphere.put(c.x, c.y, r, (1 - p) * 0.9);
        sphere2.put(c.x, c.y, r * 0.8, (1 - p) * 0.6);
        halo.put(c.x, c.y, 160 + 260 * ease.out3(p), 1 - p);
        core.put(c.x, c.y, 42 * (1 - p));
        rays.forEach((ray, i) => {
          const a = (i / rays.length) * TAU + p * 0.6;
          ray.set([polar(c, 30 + r * 0.25, a), polar(c, 60 + r * 0.75, a)], (1 - p) * 0.8);
        });
        if (p >= 1) rays.forEach(ray => ray.hide());
      }, [sphere, sphere2, halo, core]);
      fx.at(NOVA, () => {
        fx.stage.flash('#fffbeb', 0.5, 0.3);
        fx.stage.shake(10, 0.4);
        spray(motes, { x: c.x, y: c.y, count: 70, speed: [150, 600], life: [0.6, 1.2], size: [6, 13], spin: 5 });
      });
      // Солнечные блики: радужное гало, полоса засветки, лучи-звезда и «призраки» объектива к центру экрана
      // Гало и засветка — со 2-го уровня, лучи-звезда и «призраки» объектива — на 3-м
      const halos = fx.since(2) ? RAINBOW.map(css => fx.dot('ring', css)) : [];
      fx.tween(NOVA, 1.3, p => {
        const a = (1 - ease.in(p)) * Math.min(p * 6, 1);
        halos.forEach((ring, i) => ring.put(c.x, c.y, 120 + i * 7 + 40 * ease.out(p), 0.55 * a));
      }, halos);
      const streak = fx.dot('glow', '#fffbeb');
      const streakBlue = fx.dot('glow', '#bae6fd');
      if (fx.since(2)) fx.tween(NOVA, 1.1, p => {
        const a = 1 - ease.in(p);
        streak.oval(c.x, c.y, 260 + 520 * ease.out(p), 16 * a, 0.8 * a);
        streakBlue.oval(c.x, c.y, 200 + 700 * ease.out(p), 5 * a, 0.7 * a);
      }, [streak, streakBlue]);
      const glints = Array.from({ length: fx.since(3) ? 8 : 0 }, () => new Beam(fx, 2, 3, '#ffffff'));
      fx.tween(NOVA, 1.0, p => {
        const a = (1 - p) * Math.min(p * 8, 1);
        glints.forEach((glint, i) => {
          const ang = (i / glints.length) * TAU + 0.2 + p * 0.25;
          const len = (i % 2 ? 150 : 260) * (0.6 + 0.4 * ease.out(p));
          glint.set([polar(c, 10, ang), polar(c, len, ang)], a * 0.9);
        });
        if (p >= 1) glints.forEach(glint => glint.hide());
      });
      const screen = { x: fx.stage.width / 2, y: fx.stage.height / 2 };
      const ghosts = [
        { t: 0.45, r: 26, css: '#a78bfa', ring: false },
        { t: 0.75, r: 54, css: '#38bdf8', ring: true },
        { t: 1.0, r: 18, css: '#4ade80', ring: false },
        { t: 1.3, r: 70, css: '#facc15', ring: true },
        { t: 1.55, r: 34, css: '#fb923c', ring: false },
        { t: 1.8, r: 22, css: '#f87171', ring: false },
      ].slice(0, fx.since(3) ? 6 : 0).map(g => ({ ...g, dot: fx.dot(g.ring ? 'ring' : 'glow', g.css) }));
      fx.tween(NOVA, 1.2, p => {
        const a = (1 - ease.in(p)) * Math.min(p * 5, 1);
        for (const g of ghosts) {
          const pt = mix(c, screen, g.t);
          g.dot.put(pt.x, pt.y, g.r * (0.8 + 0.3 * p), (g.ring ? 0.45 : 0.3) * a);
        }
      }, ghosts.map(g => g.dot));
      for (const enemy of enemies) {
        const at = NOVA + 0.8 * (1 - Math.cbrt(1 - Math.min(dist(c, enemy.center) / REACH, 0.999)));
        fx.at(at, () => enemy.hit(false, GOLD));
        pop(fx, { at, dur: 0.35, x: enemy.center.x, y: enemy.center.y, r: 130, css: morning ? '#fdba74' : GOLD });
        if (fx.since(3)) levelAccent(fx, 'light', at, enemy);
      }
      // Со 2-го уровня вспышка подлечивает союзников: свет оседает на них
      const pluses = fx.sim({ texture: 'plus', drag: 0.8, gravity: -120, shrink: 0.5, tint: palette(['#fffbeb', '#fde68a', '#fbbf24']), alpha: life => bump(life) });
      fx.stage.healed.forEach((ally, i) => {
        const at = NOVA + 0.35 + i * 0.05;
        fx.at(at, () => {
          ally.heal();
          spray(pluses, { x: ally.center.x, y: ally.center.y, count: 7, speed: [40, 120], dir: -Math.PI / 2, spread: 1, life: [0.6, 1], size: [7, 12], jitter: 24 });
        });
        ringWave(fx, { at, dur: 0.6, x: ally.feet.x, y: ally.feet.y, from: 20, to: 120, css: GOLD, squash: GROUND, layer: 'back', alpha: 0.7 });
      });
    },
  },
  {
    id: 'light-rings', name: 'Кольца света', group: 'Сияние', spell: 'Radiant Nova', duration: 2.8,
    levels: true,
    about: 'Под вражеским строем одно за другим расходятся три сияющих кольца',
    build(fx) {
      const { enemies } = fx.stage;
      const cx = enemies.reduce((sum, e) => sum + e.feet.x, 0) / enemies.length;
      const cy = enemies.reduce((sum, e) => sum + e.feet.y, 0) / enemies.length;
      const motes = fx.sim(MOTE);
      const REACH = 420;
      fx.at(0, () => fx.stage.cast(GOLD));
      // Колец: 1 / 2 / 3 по уровню
      const RINGS = fx.lvl([1, 2, 3]);
      [0.3, 0.75, 1.2].slice(0, RINGS).forEach((at, n) => {
        const ring = fx.dot('ring', n === RINGS - 1 ? '#ffffff' : GOLD, 'back');
        const lift = fx.dot('ring', '#fffbeb');
        fx.tween(at, 0.9, p => {
          const r = REACH * ease.out(p);
          ring.oval(cx, cy, r, r * GROUND, 1 - ease.in(p));
          lift.oval(cx, cy - 26, r, r * GROUND, (1 - p) * 0.35);
          for (let i = fx.count(110 * (1 - p)); i > 0; i--) {
            const pt = polar({ x: cx, y: cy }, r, Math.random() * TAU, GROUND);
            motes.spawn({ x: pt.x, y: pt.y, vx: rand(-20, 20), vy: rand(-160, -40), lifetime: rand(0.4, 0.9), size: rand(5, 11), spin: 5 });
          }
        }, [ring, lift]);
        for (const enemy of enemies) {
          const share = Math.min(dist({ x: cx, y: cy }, { x: enemy.feet.x, y: cy + (enemy.feet.y - cy) / GROUND }) / REACH, 1);
          const hitAt = at + 0.9 * (1 - Math.sqrt(1 - share));
          const pillar = fx.dot('glow', GOLD);
          fx.tween(hitAt, 0.4, p => pillar.oval(enemy.feet.x, enemy.feet.y - 110, 44, 210, fade(p, 0.1, 0.6) * 0.8), [pillar]);
          fx.at(hitAt, () => enemy.hit(n === RINGS - 1, GOLD));
        }
      });
      groundGlow(fx, { at: 0.3, dur: 2, x: cx, y: cy, r: 380, css: '#fbbf24', alpha: 0.6 });
      fx.at(0.3 + (RINGS - 1) * 0.45, () => fx.stage.shake(6, 0.3));
      if (fx.since(3)) fx.stage.enemies.forEach(e => levelAccent(fx, 'light', 1.2, e));
    },
  },
  {
    id: 'dawn', name: 'Рассвет', group: 'Сияние', spell: 'Dawnlight', duration: 3.2,
    levels: true,
    heals: ['all', 'all', 'all'],
    artifacts: [{
      key: 'MorningStar', name: 'Утренняя звезда', kind: 'SpellProcAllyLow',
      about: 'Если кто-то из союзников при смерти (меньше 30% здоровья), рассвет вспыхивает сверхновой по всем врагам',
      then: { concept: 'supernova', level: 1, targets: 'all', after: 0.7 },
    }],
    about: 'Позади разгорается солнце, его лучи пробиваются сквозь свод и проходят по союзникам; на земле — пятна света, над исцелёнными вспыхивают нимбы',
    build(fx) {
      const allies = fx.stage.healed.length ? fx.stage.healed : fx.stage.allies;
      const left = Math.min(...allies.map(a => a.feet.x));
      const right = Math.max(...allies.map(a => a.feet.x));
      const ground = allies.reduce((sum, a) => sum + a.feet.y, 0) / allies.length;
      const TILT = 0.38;
      const motes = fx.sim({ ...MOTE, gravity: -70 });
      // Лучей: 3 / 4 / 6 по уровню
      const SHAFTS = fx.lvl([3, 4, 6]);
      const shafts = Array.from({ length: SHAFTS }, (_, i) => ({
        soft: fx.dot('glow', GOLD),
        core: fx.dot('glow', '#fffbeb'),
        offset: (i / Math.max(SHAFTS - 1, 1) - 0.5) * (right - left + 240),
        width: rand(38, 80),
        phase: rand(0, TAU),
      }));
      // Солнце позади лучей: разгорается и гаснет
      const sun = fx.dot('glow', '#fde68a', 'back');
      const sunCore = fx.dot('glow', '#fffbeb', 'back');
      const sunAt = { x: (left + right) / 2 - Math.sin(TILT) * 900, y: ground - Math.cos(TILT) * 900 };
      // Солнце — со 2-го уровня
      if (fx.since(2)) fx.tween(0, 3.1, p => {
        const a = fade(p, 0.25, 0.25);
        sun.put(sunAt.x, sunAt.y, 520, 0.45 * a);
        sunCore.put(sunAt.x, sunAt.y, 220, 0.6 * a);
      }, [sun, sunCore]);
      // Пятна света там, где лучи падают на землю
      const pools = shafts.map(() => fx.radial([[0, 0.8], [0.5, 0.45], [1, 0]], '#fde68a', 'back'));
      const pluses = fx.sim({ texture: 'plus', drag: 0.8, gravity: -120, shrink: 0.5, tint: palette(['#fffbeb', '#fde68a', '#fbbf24']), alpha: life => bump(life) });
      fx.at(0, () => fx.stage.cast(GOLD));
      fx.tween(0.15, 2.7, p => {
        const poolSweep = lerp(-90, 90, p);
        const poolPower = fade(p, 0.2, 0.25);
        shafts.forEach((shaft, i) => {
          const x = (left + right) / 2 + shaft.offset + poolSweep;
          pools[i].oval(x, ground, shaft.width * 1.6, shaft.width * 1.6 * GROUND, poolPower * 0.6);
        });
      }, pools);
      fx.tween(0.15, 2.7, p => {
        const sweep = lerp(-90, 90, p);
        const power = fade(p, 0.2, 0.25);
        for (const shaft of shafts) {
          const x = (left + right) / 2 + shaft.offset + sweep;
          const flicker = 0.75 + 0.25 * Math.sin(p * 9 + shaft.phase);
          // Луч падает на землю в (x, ground) и уходит вверх-влево
          const cx = x - Math.sin(TILT) * 420;
          const cy = ground - Math.cos(TILT) * 420;
          shaft.soft.oval(cx, cy, shaft.width, 620, power * flicker * 0.5, -TILT);
          shaft.core.oval(cx, cy, shaft.width * 0.35, 600, power * flicker * 0.4, -TILT);
          spray(motes, { x: x - rand(0, 160), y: ground - rand(0, 380), count: fx.count(9 * power), speed: [5, 30], life: [0.8, 1.5], size: [4, 9], jitter: shaft.width * 0.5 });
        }
      }, shafts.flatMap(s => [s.soft, s.core]));
      allies.forEach((ally, i) => {
        const at = 0.8 + i * 0.25;
        fx.at(at, () => {
          ally.heal();
          spray(pluses, { x: ally.center.x, y: ally.center.y, count: 8, speed: [40, 120], dir: -Math.PI / 2, spread: 1, life: [0.6, 1], size: [8, 13], jitter: 30 });
          spray(motes, { x: ally.center.x, y: ally.center.y, count: 12, speed: [60, 200], life: [0.5, 1], size: [5, 10] });
        });
        // Нимб над головой
        const nimbus = fx.dot('ring', '#fde68a');
        const nimbusGlow = fx.dot('glow', '#fde68a');
        // Нимб — со 2-го уровня
        if (fx.since(2)) fx.tween(at, 1.1, p => {
          const a = fade(p, 0.15, 0.4);
          const y = ally.head.y - 18 - 6 * ease.out(p);
          nimbus.oval(ally.head.x, y, 30, 9, 0.95 * a);
          nimbusGlow.oval(ally.head.x, y, 44, 16, 0.5 * a);
        }, [nimbus, nimbusGlow]);
        ringWave(fx, { at, dur: 0.7, x: ally.feet.x, y: ally.feet.y, from: 30, to: 150, css: GOLD, squash: GROUND, layer: 'back' });
        if (fx.since(3)) levelAccent(fx, 'light', at, ally);
        groundGlow(fx, { at, dur: 1.4, x: ally.feet.x, y: ally.feet.y, r: 120, css: '#fbbf24', alpha: 0.8 });
      });
    },
  },
  {
    id: 'light-wings-feathers', name: 'Крылья из перьев', group: 'Сияние', spell: 'Dawnlight', duration: 3,
    levels: true,
    about: 'За спиной кастера раскрываются крылья из светящихся перьев, поднимаются и мощно бьют вниз — волна исцеления и вихрь перьев',
    build(fx) {
      const { caster, allies } = fx.stage;
      const root = { x: caster.center.x - 10, y: caster.center.y - 45 };
      const tex = featherTexture();
      // Перо 92 px от основания до кончика при масштабе 1; основание — 20% ширины текстуры слева
      const FEATHER = 92;
      // Крылья шире с уровнем
      const K = fx.lvl([0.7, 0.85, 1]);
      type Feather = { sprite: Sprite; glow: Sprite; len: number };
      const makeFeather = (len: number, css: string): Feather => {
        const mk = (blend: 'normal' | 'add', tint: string) => {
          const sprite = new Sprite(tex.texture);
          sprite.anchor.set(0.205, 0.5);
          sprite.blendMode = blend;
          sprite.tint = color(tint);
          sprite.visible = false;
          return sprite;
        };
        const glow = mk('add', GOLD);
        const sprite = mk('normal', css);
        return { sprite, glow, len };
      };
      // Слои в порядке отрисовки: маховые, второстепенные, кроющие; у каждой стороны свои
      const wings = [-1, 1].map(side => {
        const primaries = Array.from({ length: 7 }, (_, i) => makeFeather(150 - i * 8, '#fef9e7'));
        const secondaries = Array.from({ length: 7 }, (_, i) => makeFeather(100 - i * 3, '#fde9a8'));
        const coverts = Array.from({ length: 8 }, (_, i) => makeFeather(58 - i * 2, '#fcd34d'));
        for (const f of [...primaries, ...secondaries, ...coverts]) fx.stage.back.addChild(f.glow);
        for (const f of [...primaries, ...secondaries, ...coverts]) fx.stage.back.addChild(f.sprite);
        return { side, primaries, secondaries, coverts };
      });
      // Поставить перо: из точки at под углом a (экранный, вниз +), длина len·scale
      const place = (f: Feather, at: Pt, a: number, scale: number, alpha: number) => {
        const k = (f.len * scale) / FEATHER;
        for (const [sprite, extra, al] of [[f.glow, 1.15, 0.22], [f.sprite, 1, 0.85]] as const) {
          sprite.visible = alpha > 0.01;
          sprite.position.set(at.x, at.y);
          sprite.rotation = a;
          sprite.scale.set(k * extra, k * extra * 0.85);
          sprite.alpha = alpha * al;
        }
      };
      // open 0…1 — раскрытие, flap −1…1 — взмах (вверх/вниз), alpha — прозрачность
      const pose = (open: number, flapIn: number, alpha: number) => {
        // Вниз крыло уходит вдвое меньше, чем вверх, — иначе опускается к ногам
        const flap = flapIn > 0 ? flapIn * 0.45 : flapIn;
        for (const wing of wings) {
          const s = wing.side;
          const toScreen = (a: number) => (s > 0 ? -a : Math.PI + a);
          const shoulder = { x: root.x + s * 10, y: root.y };
          const upper = lerp(-1.3, 0.45, open) - flap * 0.55;
          const fore = upper + lerp(-2.0, -0.05, open) - flap * 0.25;
          const elbow = polar(shoulder, 62 * K * (0.6 + 0.4 * open), toScreen(upper));
          const wrist = polar(elbow, 72 * K * (0.6 + 0.4 * open), toScreen(fore));
          const scale = (0.35 + 0.65 * open) * K;
          wing.primaries.forEach((f, i) => place(f, wrist, toScreen(fore - i * 0.2 * open - 0.05), scale, alpha));
          wing.secondaries.forEach((f, i) => {
            const t = i / (wing.secondaries.length - 1);
            place(f, mix(wrist, elbow, t), toScreen(fore - 1.5 * open - 0.3 - t * 0.2), scale, alpha);
          });
          wing.coverts.forEach((f, i) => {
            const t = i / (wing.coverts.length - 1);
            const at = t < 0.5 ? mix(wrist, elbow, t * 2) : mix(elbow, shoulder, (t - 0.5) * 2);
            place(f, at, toScreen((t < 0.5 ? fore : upper) - 1.05 * open - 0.2), scale, alpha);
          });
        }
      };
      const halo = fx.dot('glow', GOLD, 'back');
      const motes = fx.sim(MOTE);
      const loose = fx.sim({ texture: tex, drag: 1.2, gravity: 60, shrink: 0.7, tint: palette(['#fffbeb', '#fde68a', '#fef3c7']), alpha: life => Math.min(1, life * 3) });
      const wave = fx.dot('ring', '#fffbeb');
      const OPEN = 0.2;
      const RAISE = 0.8;
      const STRIKE = 1.05;
      const FOLD = 2.3;
      fx.at(0, () => fx.stage.cast(GOLD));
      fx.tween(OPEN, 2.8 - OPEN, p => {
        const t = OPEN + p * (2.8 - OPEN);
        const open = t < RAISE ? ease.back(Math.min((t - OPEN) / 0.55, 1)) : t > FOLD ? 1 - ease.inOut((t - FOLD) / 0.5) : 1;
        let flap = 0;
        if (t >= RAISE && t < STRIKE) flap = -ease.out((t - RAISE) / (STRIKE - RAISE));
        else if (t >= STRIKE && t < STRIKE + 0.2) flap = lerp(-1, 1, ease.in((t - STRIKE) / 0.2));
        else if (t >= STRIKE + 0.2 && t < FOLD) flap = Math.cos((t - STRIKE - 0.2) * 5) * Math.exp(-(t - STRIKE - 0.2) * 2.2);
        const alpha = Math.min((t - OPEN) / 0.2, 1) * (t > FOLD + 0.25 ? 1 - (t - FOLD - 0.25) / 0.25 : 1);
        pose(open, flap, alpha);
        halo.put(root.x, root.y + 20, 230 * open, 0.45 * alpha);
        spray(motes, { x: root.x + rand(-180, 180) * open, y: root.y + rand(-60, 120), count: fx.count(25 * alpha), speed: [5, 40], life: [0.6, 1.2], size: [4, 9] });
      }, [halo]);
      // Мощный взмах вниз: волна, вспышка, вихрь перьев
      const HIT = STRIKE + 0.18;
      fx.at(HIT, () => {
        fx.stage.flash('#fffbeb', 0.3, 0.3);
        fx.stage.shake(6, 0.3);
        // Вихрь перьев — со 2-го уровня
        if (fx.since(2)) for (const side of [-1, 1]) {
          spray(loose, { x: root.x + side * 120, y: root.y + 40, count: 16, speed: [120, 380], dir: Math.PI / 2 + side * -0.6, spread: 1.4, life: [1, 1.6], size: [18, 30], spin: 4 });
        }
      });
      fx.tween(HIT, 0.75, p => {
        const r = 600 * ease.out3(p);
        wave.oval(caster.center.x, caster.center.y + 30, r, r * 0.5, (1 - p) * 0.75);
      }, [wave]);
      ringWave(fx, { at: HIT, dur: 0.7, x: caster.feet.x, y: caster.feet.y, from: 40, to: 380, css: GOLD, squash: GROUND, layer: 'back' });
      if (fx.since(3)) levelAccent(fx, 'light', HIT, caster);
      allies.forEach((ally, i) => {
        const at = HIT + 0.1 + i * 0.07;
        fx.at(at, () => {
          ally.heal();
          spray(loose, { x: ally.center.x, y: ally.center.y - 40, count: 3, speed: [30, 100], life: [0.9, 1.4], size: [14, 22], spin: 3 });
        });
        pop(fx, { at, dur: 0.5, x: ally.center.x, y: ally.center.y, r: 130, css: GOLD, alpha: 0.7 });
      });
      // Складываются — перья осыпаются
      fx.at(FOLD + 0.2, () => spray(loose, { x: root.x, y: root.y + 20, count: 18, speed: [40, 160], dir: Math.PI / 2, spread: 2.4, life: [1, 1.6], size: [14, 24], spin: 3, jitter: 90 }));
    },
  },
  {
    id: 'arcane-missiles', name: 'Магические стрелы', group: 'Тайная магия', spell: 'Arcane Bolt', duration: 2.4,
    levels: true,
    about: 'Шесть самонаводящихся стрел разлетаются по дугам и бьют по врагам',
    build(fx) {
      const { caster, enemies } = fx.stage;
      const from = { x: caster.center.x + 30, y: caster.center.y - 40 };
      const trail = fx.sim({ texture: 'glow', drag: 0.4, shrink: 0.3, tint: palette(C.arcane), alpha: life => life * 0.8 });
      const sparks = fx.sim(P.spark(C.arcane, 100));
      fx.at(0, () => fx.stage.cast('#a78bfa'));
      // Стрел: 3 / 4 / 6 по уровню
      const COUNT = fx.lvl([3, 4, 6]);
      for (let i = 0; i < COUNT; i++) {
        const enemy = enemies[i % 3];
        const to = enemy.center;
        const at = 0.25 + i * 0.1;
        const side = i % 2 ? 1 : -1;
        const control = { x: lerp(from.x, to.x, rand(0.15, 0.4)), y: from.y + side * rand(160, 300) - 60 };
        const head = fx.dot('glow', '#e9d5ff');
        const core = fx.dot('glow', '#ffffff');
        fly(fx, at, 0.6, p => bez(from, control, to, ease.in(p) * 0.6 + p * 0.4), pt => {
          head.put(pt.x, pt.y, 30);
          core.put(pt.x, pt.y, 11);
          spray(trail, { x: pt.x, y: pt.y, count: fx.count(160), speed: [0, 20], life: [0.25, 0.5], size: [7, 14] });
        }, [head, core]);
        fx.at(at + 0.6, () => {
          enemy.hit(i >= COUNT - 2, '#a78bfa');
          fx.stage.shake(3, 0.12);
          spray(sparks, { x: to.x, y: to.y, count: 16, speed: [100, 420], life: [0.3, 0.7], size: [3, 7] });
        });
        pop(fx, { at: at + 0.6, dur: 0.3, x: to.x, y: to.y, r: 110, css: '#a78bfa' });
        ringWave(fx, { at: at + 0.6, dur: 0.35, x: to.x, y: to.y, from: 10, to: 90, css: '#e9d5ff' });
      }
      if (fx.since(3)) levelAccent(fx, 'arcane', 0.85 + (COUNT - 1) * 0.1, enemies[0]);
    },
  },
  {
    id: 'portal-beam', name: 'Портал', group: 'Тайная магия', spell: 'Arcane Bolt', duration: 2.8,
    levels: true,
    extras: true,
    about: 'Над целью раскрывается портал, и из него в неё бьёт луч',
    build(fx) {
      const { enemies } = fx.stage;
      const T = enemies[0];
      const gate = { x: T.head.x + 150, y: T.head.y - 190 };
      const to = T.center;
      const tilt = angle(gate, to) + Math.PI / 2;
      const rim = fx.dot('ring', '#a78bfa');
      const rimIn = fx.dot('ring', '#e9d5ff');
      const hole = fx.dot('disc', '#0b0620', 'front', 'normal');
      const swirl = fx.sim({ texture: 'dot', drag: 1, shrink: 0.5, tint: palette(C.arcane), alpha: life => bump(life) });
      // Луч толще с уровнем
      const beam = new Beam(fx, 2, fx.lvl([16, 21, 26]), '#a78bfa');
      const sparks = fx.sim(P.spark(C.arcane, 300));
      const OPEN = 0.25;
      const FIRE = 1;
      const size = (p: number) => 78 * (p < 0.2 ? ease.back(p / 0.2) : p > 0.85 ? 1 - ease.in((p - 0.85) / 0.15) : 1);
      fx.at(0, () => fx.stage.cast('#a78bfa'));
      fx.tween(OPEN, 2.2, p => {
        const r = size(p);
        hole.oval(gate.x, gate.y, r * 0.86, r * 0.4, 0.95, tilt);
        rim.oval(gate.x, gate.y, r, r * 0.46, 1, tilt);
        rimIn.oval(gate.x, gate.y, r * 0.8, r * 0.37, 0.7, tilt);
        for (let i = fx.count(120); i > 0; i--) {
          const a = Math.random() * TAU;
          const local = { x: Math.cos(a) * r * 1.15, y: Math.sin(a) * r * 0.53 };
          const x = gate.x + local.x * Math.cos(tilt) - local.y * Math.sin(tilt);
          const y = gate.y + local.x * Math.sin(tilt) + local.y * Math.cos(tilt);
          swirl.spawn({ x, y, vx: (gate.x - x) * 2.4, vy: (gate.y - y) * 2.4, lifetime: 0.4, size: rand(2, 5) });
        }
      }, [hole, rim, rimIn]);
      fx.tween(FIRE, 0.1, p => beam.set([gate, mix(gate, to, p)]));
      fx.tween(FIRE + 0.1, 0.6, p => {
        beam.set([gate, to], fade(p, 0, 0.3) * (0.85 + 0.15 * Math.sin(p * 70)));
        spray(sparks, { x: to.x, y: to.y, count: fx.count(110), speed: [100, 480], life: [0.3, 0.7], size: [3, 7] });
        if (p >= 1) beam.hide();
      });
      [FIRE + 0.1, FIRE + 0.4].forEach((time, i) => fx.at(time, () => {
        T.hit(i === 1, '#a78bfa');
        fx.stage.shake(7, 0.25);
      }));
      pop(fx, { at: FIRE + 0.1, dur: 0.6, x: to.x, y: to.y, r: 200, css: '#a78bfa' });
      ringWave(fx, { at: FIRE + 0.1, dur: 0.5, x: T.feet.x, y: T.feet.y, from: 20, to: 200, css: '#e9d5ff', squash: GROUND, layer: 'back' });
      groundGlow(fx, { at: FIRE, dur: 1.2, x: T.feet.x, y: T.feet.y, r: 140, css: '#6366f1' });
      if (fx.since(3)) levelAccent(fx, 'arcane', FIRE + 0.1, T);
      // Доп. эффект: портал бьёт лучами и в двух соседей
      if (fx.extras) {
        fx.stage.bystanders.slice(0, 2).forEach((enemy, i) => {
          const side = new Beam(fx, 2, 14, '#c4b5fd');
          const at = FIRE + 0.35 + i * 0.12;
          fx.tween(at, 0.08, p => side.set([gate, mix(gate, enemy.center, p)]));
          fx.tween(at + 0.08, 0.4, p => {
            side.set([gate, enemy.center], fade(p, 0, 0.4) * (0.85 + 0.15 * Math.sin(p * 70)));
            if (p >= 1) side.hide();
          });
          fx.at(at + 0.08, () => {
            enemy.hit(false, '#a78bfa');
            spray(sparks, { x: enemy.center.x, y: enemy.center.y, count: 20, speed: [100, 380], life: [0.3, 0.6], size: [3, 6] });
          });
          pop(fx, { at: at + 0.08, dur: 0.35, x: enemy.center.x, y: enemy.center.y, r: 120, css: '#a78bfa' });
        });
      }
    },
  },
];
