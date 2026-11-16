import { Graphics } from 'pixi.js';
import {
  Beam, C, GROUND, P, TAU, arc, bump, clamp01, color, ease, fade, fly, groundGlow, lerp, levelAccent, mix, palette, path, polar, pop, rand, ribbon, ringWave, spray, bez,
  perTarget, type Concept, type Pt,
} from './kit';
import { bubbleTexture, dropTexture, leafTexture, petalTexture } from './sprites';

// Концепты природы
const HEAL = '#86efac';
// Земля и щепки для реалистичных корней и шипов
const DIRT = { texture: 'debris' as const, blend: 'normal' as const, drag: 0.2, gravity: 1000, shrink: 0.7, tint: palette(['#3b2a1a', '#5b4330', '#2a1d12']) };
const DUST = { texture: 'smoke' as const, blend: 'normal' as const, drag: 0.4, gravity: -20, shrink: 1.4, tint: palette(['#5b4a36', '#4a3b2b']), alpha: (life: number) => bump(life) * 0.45 };

// Корень: лента сужается к концу, тёмная подложка, светлый бок, борозда, кольца коры, сучки
const drawRoot = (g: Graphics, pts: Pt[], base: number, alpha: number) => {
  if (alpha <= 0.01 || pts.length < 2) return;
  const n = pts.length;
  const widths = pts.map((_, i) => base * Math.pow(1 - i / (n - 1), 0.7) + 1.5);
  ribbon(g, pts, widths.map(w => w + 3), '#1e1208', 0.85 * alpha);
  ribbon(g, pts, widths, '#5a3a20', alpha);
  ribbon(g, pts, widths.map(w => w * 0.32), '#8f6a45', 0.85 * alpha, -0.27);
  ribbon(g, pts, widths.map(w => w * 0.18), '#2b1a0d', 0.6 * alpha, 0.28);
  for (let i = 2; i < n - 2; i += 2) {
    const a = Math.atan2(pts[i + 1].y - pts[i - 1].y, pts[i + 1].x - pts[i - 1].x) + Math.PI / 2;
    const w = widths[i] * 0.42;
    g.moveTo(pts[i].x + Math.cos(a) * w, pts[i].y + Math.sin(a) * w).lineTo(pts[i].x - Math.cos(a) * w * 0.6, pts[i].y - Math.sin(a) * w * 0.6)
      .stroke({ width: 1.2, color: 0x1e1208, alpha: 0.45 * alpha });
  }
  for (const t of [0.3, 0.62]) {
    const i = Math.floor(t * (n - 1));
    g.ellipse(pts[i].x, pts[i].y, widths[i] * 0.22, widths[i] * 0.16).fill({ color: 0x2b1a0d, alpha: 0.8 * alpha });
  }
};
// Тонкий корешок, отходящий вбок от точки
const rootlet = (g: Graphics, from: Pt, dir: number, len: number, alpha: number) => {
  const pts = path(5, t => ({ x: from.x + Math.cos(dir + t * 0.6) * len * t, y: from.y + Math.sin(dir + t * 0.6) * len * t }));
  drawRoot(g, pts, 5, alpha);
};
// Трещины в земле от точки: тёмные ломаные в плоскости пола
const groundCracks = (g: Graphics, x: number, y: number, n: number, len: number, grow: number, alpha: number, seed: number) => {
  for (let i = 0; i < n; i++) {
    let px = x;
    let py = y;
    let a = (i / n) * TAU + Math.sin(seed + i * 3.1) * 0.3;
    g.moveTo(px, py);
    for (let k = 1; k <= 5 * grow; k++) {
      a += Math.sin(seed * 1.7 + i * 2.3 + k) * 0.45;
      px += Math.cos(a) * len / 5;
      py += Math.sin(a) * len / 5 * GROUND;
      g.lineTo(px, py);
    }
    g.stroke({ width: 3, color: 0x1a0f07, alpha: 0.75 * alpha });
  }
};
const VENOM = '#a3e635';
// Нарисованный лист: кувыркается и падает
const LEAF_FX = { texture: leafTexture(), blend: 'normal' as const, drag: 0.3, gravity: 300, shrink: 0.75, alpha: (life: number) => Math.min(1, life * 2.5) };

export const NATURE_CONCEPTS: Concept[] = [
  {
    id: 'rune-bloom', name: 'Руна-цветок', group: 'Природа', spell: 'Healing Rune', duration: 2.9,
    levels: true,
    heals: [1, 1, 1],
    extras: true,
    artifacts: [{
      key: 'LifeTreeSeed', name: 'Семя древа жизни', kind: 'OverhealProc',
      about: 'Лечение сверх полного здоровья становится ядом: из исцелённого в случайного врага летит ядовитый плевок',
      then: { concept: 'venom-spit', level: 1, targets: 'one', origin: 'healed', after: 0.7 },
    }],
    about: 'Под союзником загорается круг рун и раскрывается светящийся лотос; вверх кружат листья и искры лечения',
    build: perTarget(fx => {
      const { allies } = fx.stage;
      const A = fx.stage.healed[0] ?? allies[1] ?? allies[0];
      const { x, y } = A.feet;
      // Лечение от «Посоха древней рощи» (за каждого опутанного корнями): вокруг руны пробиваются корешки
      const grove = fx.triggered('HealPerStatus');
      const OPEN = 0.3;
      const END = 2.5;
      fx.at(0, () => fx.stage.cast(HEAL));

      // Круг рун в плоскости пола: два кольца, руны по ободу, гексаграмма; вращаются в разные стороны
      const circle = fx.box('back', 'add');
      circle.position.set(x, y);
      circle.visible = false;
      const R = 140;
      const runes = new Graphics()
        .circle(0, 0, R).stroke({ width: 5, color: color('#bef264') })
        .circle(0, 0, R * 0.8).stroke({ width: 2.5, color: color('#4ade80') });
      for (let i = 0; i < 12; i++) {
        const p = polar({ x: 0, y: 0 }, R * 0.9, (i / 12) * TAU);
        runes.moveTo(p.x - 5, p.y - 7).lineTo(p.x + 5, p.y + 7).moveTo(p.x + 5, p.y - 7).lineTo(p.x, p.y).moveTo(p.x - 6, p.y + 2).lineTo(p.x - 1, p.y + 2);
      }
      runes.stroke({ width: 3, color: color('#ecfccb') });
      const star = new Graphics();
      for (const turn of [0, Math.PI]) {
        star.poly([0, 1, 2].flatMap(i => {
          const a = turn + (i / 3) * TAU - Math.PI / 2;
          return [Math.cos(a) * R * 0.72, Math.sin(a) * R * 0.72];
        })).stroke({ width: 3, color: color('#86efac') });
      }
      circle.addChild(runes, star);
      fx.tween(0.1, END, p => {
        circle.visible = true;
        const s = ease.back(Math.min(p * 4, 1));
        circle.scale.set(s, s * GROUND);
        runes.rotation = p * 0.8;
        star.rotation = -p * 1.4;
        circle.alpha = fade(p, 0.05, 0.2);
      });
      groundGlow(fx, { at: 0.1, dur: END, x, y, r: 190, css: '#22c55e' });
      if (grove) {
        const roots = fx.gfx('back', 'normal');
        const sprouts = Array.from({ length: 7 }, (_, i) => ({ a: (i / 7) * TAU + rand(-0.2, 0.2), len: rand(50, 80) }));
        fx.tween(0.15, END, p => {
          const grow = ease.out(Math.min(p * 3, 1));
          roots.clear();
          for (const sp of sprouts) {
            const base = polar({ x, y }, 150, sp.a, GROUND);
            rootlet(roots, base, -Math.PI / 2 + Math.cos(sp.a) * 0.6, sp.len * grow, fade(p, 0.05, 0.2));
          }
        });
      }

      // Лотос: задний ряд крупных лепестков за юнитом, передний ряд светлых — перед ним
      const petal = petalTexture();
      // Задний ряд лепестков — со 2-го уровня
      const backRow = Array.from({ length: fx.since(2) ? 7 : 0 }, (_, i) => ({ dot: fx.tex(petal, 'back', 'add', '#4ade80'), a: lerp(-2.75, -0.39, i / 6), len: 130 - Math.abs(i - 3) * 9 }));
      const frontRow = Array.from({ length: 6 }, (_, i) => ({ dot: fx.tex(petal, 'front', 'add', '#bbf7d0'), a: lerp(-2.55, -0.59, i / 5), len: 84 - Math.abs(i - 2.5) * 6 }));
      const heart = fx.dot('glow', '#f7fee7');
      const column = fx.dot('glow', HEAL);
      fx.tween(OPEN, END - OPEN, p => {
        const open = ease.back(Math.min(p * 2.5, 1));
        const a = fade(p, 0.05, 0.22);
        for (const row of [backRow, frontRow]) {
          for (const leaf of row) {
            const ang = lerp(-Math.PI / 2, leaf.a, open) + Math.sin(p * 9 + leaf.a) * 0.03;
            const len = leaf.len * (0.4 + 0.6 * open);
            const center = polar({ x, y: y - 6 }, len / 2, ang);
            leaf.dot.put(center.x, center.y, len / 2, a * 0.9, ang);
          }
        }
        heart.put(x, y - 34, 70 + 10 * Math.sin(p * 12), a);
        column.oval(x, y - 150, 80, 260, fade(p, 0.25, 0.4) * 0.6);
      }, [...backRow, ...frontRow].map(leaf => leaf.dot).concat(heart, column));

      // Листья и искры кружат вверх
      const leaves = fx.sim({ texture: leafTexture(), blend: 'normal', drag: 0.5, gravity: -80, shrink: 0.8, alpha: life => Math.min(1, life * 3) });
      const pluses = fx.sim(P.plus(C.nature, 160));
      const motes = fx.sim(P.glow(C.nature, 120));
      fx.tween(OPEN + 0.2, END - OPEN - 0.5, () => {
        for (let i = fx.count(10); i > 0; i--) {
          const a = Math.random() * TAU;
          const start = polar({ x, y: y - 20 }, rand(30, 90), a, 0.4);
          leaves.spawn({ x: start.x, y: start.y, vx: -Math.sin(a) * 90, vy: rand(-200, -110), lifetime: rand(1, 1.5), size: rand(11, 17), rotation: Math.random() * TAU, spin: rand(-4, 4) });
        }
        spray(pluses, { x, y: y - 30, count: fx.count(24), speed: [20, 80], dir: -Math.PI / 2, spread: 0.5, life: [0.8, 1.3], size: [7, 12], jitter: 50 });
        spray(motes, { x, y: y - 30, count: fx.count(30), speed: [10, 60], dir: -Math.PI / 2, spread: 0.8, life: [0.7, 1.3], size: [10, 20], jitter: 60 });
      });
      // Лечений: 1 / 2 / 2 по уровню
      fx.lvl([[0.8], [0.8, 1.6], [0.8, 1.6]]).forEach(time => {
        fx.at(time, () => A.heal());
        ringWave(fx, { at: time, dur: 0.7, x, y, from: 40, to: 230, css: '#bef264', squash: GROUND, layer: 'back' });
        pop(fx, { at: time, dur: 0.5, x: A.center.x, y: A.center.y, r: 150, css: HEAL, alpha: 0.7 });
      });
      if (fx.since(3)) levelAccent(fx, 'nature', 0.8, A);
      // Доп. эффект: лечение брызгами задевает остальных союзников
      if (fx.extras) {
        allies.filter(ally => ally !== A).forEach((ally, i) => {
          const at = 1.75 + i * 0.06;
          fx.at(at, () => {
            ally.heal();
            spray(pluses, { x: ally.center.x, y: ally.center.y, count: 6, speed: [30, 100], dir: -Math.PI / 2, spread: 0.8, life: [0.6, 1], size: [6, 10], jitter: 20 });
          });
          ringWave(fx, { at, dur: 0.5, x: ally.feet.x, y: ally.feet.y, from: 20, to: 110, css: '#bef264', squash: GROUND, layer: 'back', alpha: 0.7 });
        });
      }
    }, 0.22, 'healed'),
  },
  {
    id: 'fireflies', name: 'Светлячки', group: 'Природа', spell: 'Healing Rune', duration: 4,
    levels: true,
    about: 'Маленькие добрые души — зелёные клочки дымки с круглыми глазами — спиралью поднимаются вокруг союзников и по одной ныряют в них',
    build(fx) {
      const { allies } = fx.stage;
      // Душа — клочок зелёной дымки (как белёсая у «Жатвы душ») с круглыми светящимися глазами
      const mist = fx.sim({ texture: 'smoke', blend: 'normal', drag: 0.3, gravity: -20, shrink: 1.3, tint: palette(['#4ade80', '#22c55e', '#86efac']), alpha: life => bump(life) * 0.55 });
      const glowMist = fx.sim({ texture: 'glow', drag: 0.3, gravity: -30, shrink: 0.5, tint: palette(['#86efac', '#4ade80', '#22c55e']), alpha: life => bump(life) * 0.3 });
      // Мерцающие искорки за душами и плюсики лечения при втягивании
      const twinkles = fx.sim({ texture: 'star', drag: 0.6, gravity: -40, shrink: 0.6, tint: palette(['#f7fee7', '#d9f99d', '#86efac']), alpha: life => bump(life) });
      const pluses = fx.sim({ texture: 'plus', drag: 0.8, gravity: -140, shrink: 0.5, tint: palette(['#f0fdf4', '#86efac', '#4ade80']), alpha: life => bump(life) });
      fx.at(0, () => fx.stage.cast(HEAL));
      // Душ у каждого союзника: 4 / 6 / 8 по уровню
      const COUNT = fx.lvl([4, 6, 8]);
      const GAP = 0.1;
      const LIFE = 2;
      allies.forEach((ally, n) => {
        const at = 0.15 + n * 0.12;
        const heal = at + (COUNT - 1) * GAP + LIFE;
        for (let i = 0; i < COUNT; i++) {
          const start = at + i * GAP;
          const halo = fx.dot('glow', i % 3 ? '#4ade80' : '#a3e635');
          const eyes = [-1, 1].map(side => ({ side, glow: fx.dot('glow', '#dcfce7'), pupil: fx.dot('disc', '#ffffff') }));
          const phase = (i / COUNT) * TAU;
          const size = rand(0.85, 1.1);
          const blinkAt = rand(0.2, 0.6);
          // Спираль вокруг союзника от ног к голове, потом нырок в грудь. Души идут друг за другом,
          // поэтому в центр входят по одной, а не все разом
          const spiral = (q: number) => {
            const a = phase + q * TAU * 1.3;
            const r = lerp(80, 42, q);
            const y = lerp(ally.feet.y, ally.head.y - 10, ease.inOut(q));
            return { x: ally.center.x + Math.cos(a) * r, y: y + Math.sin(a) * r * 0.3 };
          };
          fx.tween(start, LIFE, p => {
            const climb = Math.min(p / 0.78, 1);
            const dive = Math.max(0, (p - 0.78) / 0.22);
            const pt = dive > 0 ? mix(spiral(1), ally.center, ease.in(dive)) : spiral(climb);
            const k = size * (1 - 0.55 * ease.in(dive));
            const alpha = fade(p, 0.12, 0.08);
            spray(mist, { x: pt.x, y: pt.y, count: fx.count(9 * alpha), speed: [5, 20], life: [0.35, 0.6], size: [16 * k, 26 * k], jitter: 6 * k });
            spray(glowMist, { x: pt.x, y: pt.y, count: fx.count(3 * alpha), speed: [0, 15], life: [0.3, 0.5], size: [8 * k, 14 * k], jitter: 5 * k });
            if (Math.random() < 0.04) spray(twinkles, { x: pt.x, y: pt.y, count: 1, speed: [10, 40], life: [0.3, 0.6], size: [5, 9], jitter: 10 });
            halo.put(pt.x, pt.y, 26 * k, 0.35 * alpha);
            // Круглые глаза, как у душ «Жатвы», только меньше; раз моргают
            const open = (Math.abs(p - blinkAt) < 0.025 ? 0.1 : 1) * alpha;
            for (const eye of eyes) {
              const ex = pt.x + eye.side * 6.5 * k;
              eye.glow.oval(ex, pt.y, 8 * k, 8 * k * open, 0.85 * alpha);
              eye.pupil.oval(ex, pt.y, 3.4 * k, 3.4 * k * open, alpha);
            }
          }, [halo, ...eyes.flatMap(eye => [eye.glow, eye.pupil])]);
          // Входит в союзника — искорки
          fx.at(start + LIFE, () => spray(twinkles, { x: ally.center.x, y: ally.center.y, count: 4, speed: [40, 140], life: [0.3, 0.6], size: [5, 9] }));
        }
        fx.at(heal, () => {
          ally.heal();
          spray(pluses, { x: ally.center.x, y: ally.center.y, count: 10, speed: [40, 140], dir: -Math.PI / 2, spread: 1.1, life: [0.6, 1], size: [8, 14], jitter: 30 });
          spray(twinkles, { x: ally.center.x, y: ally.center.y, count: 14, speed: [60, 220], life: [0.3, 0.7], size: [5, 10] });
        });
        pop(fx, { at: heal - 0.05, dur: 0.5, x: ally.center.x, y: ally.center.y, r: 130, css: HEAL, alpha: 0.8 });
        ringWave(fx, { at: heal, dur: 0.6, x: ally.feet.x, y: ally.feet.y, from: 20, to: 120, css: HEAL, squash: GROUND, layer: 'back', alpha: 0.7 });
      });
      if (fx.since(3)) {
        fx.stage.allies.forEach(e => levelAccent(fx, 'nature', 0.15 + (allies.length - 1) * 0.12 + (COUNT - 1) * GAP + LIFE, e));
      }
    },
  },
  {
    id: 'venom-spit', name: 'Ядовитый плевок', group: 'Природа', spell: 'Venom Thorns', duration: 2.8,
    levels: true,
    targets: [1, 2, 3],
    extras: true,
    artifacts: [{
      key: 'WyvernSting', name: 'Жало виверны', kind: 'SpreadOnDeath',
      about: 'Отравленный враг умирает — его яд брызгами перескакивает на двух ближайших соседей',
      then: { concept: 'venom-spit', level: 1, targets: 'neighbours', origin: 'target', after: 0.8 },
    }],
    about: 'Светящийся ком яда летит навесом, разбивается брызгами и растекается кипящей лужей с ядовитым паром',
    build: perTarget(fx => {
      const { caster, enemies } = fx.stage;
      const T = enemies[0];
      const to = { x: T.center.x, y: T.center.y + 20 };
      // Срабатывания: заражение от «Жала виверны» летит из умершего (fx.origin), яд «Семени древа» — из исцелённого союзника
      const spread = fx.triggered('SpreadOnDeath');
      const seed = fx.triggered('OverhealProc');
      const from = fx.origin && fx.trigger ? { x: fx.origin.x, y: fx.origin.y - 60 } : { x: caster.center.x + 40, y: caster.center.y - 50 };
      const START = fx.trigger ? 0.1 : 0.25;
      const drop = dropTexture('venom');
      const glob = fx.tex(drop);
      const halo = fx.dot('glow', VENOM);
      const core = fx.dot('glow', '#f7fee7');
      const drips = fx.sim({ texture: drop, blend: 'normal', drag: 0.5, gravity: 950, shrink: 0.6 });
      const bubbles = fx.sim({ texture: bubbleTexture(), blend: 'normal', drag: 0.6, gravity: -90, shrink: 1.4, alpha: life => bump(life) });
      const vapor = fx.sim({ texture: 'smoke', drag: 0.3, gravity: -55, shrink: 1.6, tint: () => color('#65a30d'), alpha: life => bump(life) * 0.4 });
      const puddle = fx.radial([[0, 0.9], [0.75, 0.75], [0.92, 0.4], [1, 0]], '#3f6212', 'back', 'normal');
      const shine = fx.radial([[0, 0.65], [0.6, 0.3], [1, 0]], VENOM, 'back');
      const rim = fx.dot('ring', '#bef264', 'back');
      const SPLAT = START + (spread ? 0.5 : 0.7);
      // Ком и лужа крупнее с уровнем; заражение — комки поменьше
      const K = fx.lvl([0.72, 0.86, 1]) * (spread ? 0.75 : 1);
      fx.at(0, () => fx.stage.cast(VENOM));
      // Умерший выпускает облако яда, из него вылетают брызги
      if (spread) fx.at(0, () => spray(vapor, { x: from.x, y: from.y + 30, count: 10, speed: [30, 120], life: [0.6, 1], size: [40, 70], jitter: 30 }));
      // Из исцелённого: зелёная вспышка и лепестки
      const petals = seed ? fx.sim({ texture: petalTexture(), blend: 'add', drag: 0.5, gravity: 60, shrink: 0.6, tint: palette(['#bbf7d0', '#4ade80']), alpha: life => bump(life) }) : null;
      if (seed) pop(fx, { at: 0, dur: 0.4, x: from.x, y: from.y, r: 120, css: '#86efac' });
      fly(fx, START, SPLAT - START, p => arc(from, to, p, spread ? 90 : 170), (pt, p, dir) => {
        if (petals) spray(petals, { x: pt.x, y: pt.y, count: fx.count(20), speed: [10, 60], life: [0.4, 0.8], size: [8, 13], spin: 4 });
        const wobble = Math.sin(p * 40) * 0.15;
        halo.put(pt.x, pt.y, 80 * K, 0.75);
        glob.oval(pt.x, pt.y, 34 * K * (1 + wobble), 34 * K * (1 - wobble), 1, dir);
        core.put(pt.x + Math.cos(dir) * 8, pt.y + Math.sin(dir) * 8, 16, 0.8);
        spray(drips, { x: pt.x, y: pt.y, count: fx.count(40), speed: [10, 70], life: [0.3, 0.6], size: [5, 9], align: true, jitter: 10 });
        spray(bubbles, { x: pt.x, y: pt.y, count: fx.count(20), speed: [10, 50], life: [0.3, 0.6], size: [4, 8], jitter: 12 });
      }, [glob, halo, core]);
      fx.at(SPLAT, () => {
        T.hit(false, VENOM);
        fx.stage.shake(5, 0.2);
        spray(drips, { x: to.x, y: to.y, count: 50, speed: [180, 560], dir: -Math.PI / 2, spread: 1.3, life: [0.5, 1], size: [6, 12], align: true });
        spray(bubbles, { x: to.x, y: to.y, count: 14, speed: [60, 220], life: [0.5, 0.9], size: [8, 15] });
      });
      pop(fx, { at: SPLAT, dur: 0.4, x: to.x, y: to.y, r: 200, css: VENOM });
      fx.tween(SPLAT, 1.65, p => {
        const r = 165 * K * ease.out3(Math.min(p * 4, 1));
        const a = fade(p, 0.05, 0.3);
        puddle.oval(T.feet.x, T.feet.y, r, r * GROUND, a * 0.9);
        shine.oval(T.feet.x, T.feet.y - 4, r * 0.85, r * 0.85 * GROUND, a * 0.75);
        rim.oval(T.feet.x, T.feet.y, r * 0.98, r * 0.98 * GROUND, a * (0.6 + 0.3 * Math.sin(p * 16)));
        for (let i = fx.count(22 * a); i > 0; i--) {
          const pt = polar(T.feet, Math.sqrt(Math.random()) * r * 0.85, Math.random() * TAU, GROUND);
          bubbles.spawn({ x: pt.x, y: pt.y, vx: 0, vy: rand(-40, -10), lifetime: rand(0.3, 0.6), size: rand(5, 11) });
        }
        spray(vapor, { x: T.feet.x, y: T.feet.y - 10, count: fx.count(12 * a), speed: [10, 50], dir: -Math.PI / 2, spread: 0.4, life: [0.9, 1.4], size: [36, 60], jitter: r * 0.6 });
      }, [puddle, shine, rim]);
      // Тиков яда: 1 / 2 / 3 по уровню
      fx.lvl([[SPLAT + 0.8], [SPLAT + 0.6, SPLAT + 1.2], [SPLAT + 0.5, SPLAT + 1.0, SPLAT + 1.5]]).forEach(time => fx.at(time, () => T.hit(false, VENOM)));
      if (fx.since(3)) levelAccent(fx, 'nature', SPLAT, T);
      // Доп. эффект: от кома отлетают ещё два плевка в соседей
      if (fx.extras) {
        fx.stage.bystanders.slice(0, 2).forEach((enemy, i) => {
          const start = to;
          const end = { x: enemy.center.x, y: enemy.center.y + 20 };
          const blob = fx.tex(drop);
          const blobHalo = fx.dot('glow', VENOM);
          const at = SPLAT + 0.05 + i * 0.08;
          fly(fx, at, 0.4, p => arc(start, end, p, 90), (pt, _p, dir) => {
            blobHalo.put(pt.x, pt.y, 50, 0.7);
            blob.oval(pt.x, pt.y, 18, 16, 1, dir);
            spray(drips, { x: pt.x, y: pt.y, count: fx.count(25), speed: [10, 60], life: [0.3, 0.5], size: [4, 7], align: true });
          }, [blob, blobHalo]);
          fx.at(at + 0.4, () => {
            enemy.hit(false, VENOM);
            spray(drips, { x: end.x, y: end.y, count: 22, speed: [150, 420], dir: -Math.PI / 2, spread: 1.2, life: [0.4, 0.8], size: [5, 9], align: true });
            spray(bubbles, { x: end.x, y: end.y, count: 8, speed: [40, 160], life: [0.4, 0.8], size: [6, 11] });
          });
          pop(fx, { at: at + 0.4, dur: 0.35, x: end.x, y: end.y, r: 120, css: VENOM });
        });
      }
    }, 0.2),
  },
  {
    id: 'roots-gnarled', name: 'Узловатые корни', group: 'Природа', spell: 'Entangling Roots', duration: 2.9,
    levels: true,
    about: 'Земля трескается, из неё вырываются толстые корни с корой и сучками, обвивают цель и сжимаются',
    build(fx) {
      const { enemies } = fx.stage;
      const T = enemies[0];
      const { x, y } = T.feet;
      const dirt = fx.sim(DIRT);
      const dust = fx.sim(DUST);
      const dustBack = fx.sim(DUST, 'back');
      const leaves = fx.sim(LEAF_FX);
      const cracks = fx.gfx('back', 'normal');
      const SQUEEZE = 1.3;
      const END = 2.3;
      let squeeze = 0;
      fx.at(0, () => fx.stage.cast(HEAL));
      fx.tween(0.1, END + 0.4, p => {
        const t = p * (END + 0.4);
        cracks.clear();
        groundCracks(cracks, x, y, 8, 150, ease.out(Math.min(t / 0.3, 1)), t > END ? 1 - (t - END) / 0.4 : 1, 3);
      });
      // Корней: 3 / 4 / 6 по уровню
      const ROOTS = fx.lvl([3, 4, 6]);
      const roots = Array.from({ length: ROOTS }, (_, i) => ({
        g: fx.gfx(i % 2 ? 'back' : 'front', 'normal'),
        start: (i / ROOTS) * TAU + rand(-0.2, 0.2),
        turns: rand(0.55, 0.85) * (i % 2 ? 1 : -1),
        height: rand(150, 205),
        width: rand(22, 30),
        wobble: rand(0, TAU),
        at: 0.25 + i * 0.07,
        lets: [rand(0.25, 0.4), rand(0.5, 0.7)],
      }));
      type Root = typeof roots[number];
      const along = (root: Root, s: number): Pt => {
        const a = root.start + s * TAU * root.turns;
        const r = (95 - 55 * s) * (1 - squeeze * 0.28) + Math.sin(s * 9 + root.wobble) * 6;
        return { x: x + Math.cos(a) * r, y: y - s * root.height + Math.sin(a) * r * 0.32 };
      };
      for (const root of roots) {
        fx.tween(root.at, END - root.at + 0.35, p => {
          const t = root.at + p * (END - root.at + 0.35);
          const reach = t < root.at + 0.5 ? ease.out((t - root.at) / 0.5) : t > END ? 1 - ease.in((t - END) / 0.35) : 1;
          root.g.clear();
          if (reach <= 0.02) return;
          const pts = path(18, k => along(root, k * reach));
          drawRoot(root.g, pts, root.width, 1);
          for (const l of root.lets) {
            if (reach < l + 0.1) continue;
            const pt = along(root, l);
            rootlet(root.g, pt, root.start + l * TAU * root.turns + (l > 0.5 ? 0.8 : -0.8), 26, 1);
          }
        });
        fx.at(root.at, () => {
          const base = along(root, 0);
          spray(dirt, { x: base.x, y: base.y, count: 8, speed: [150, 420], dir: -Math.PI / 2, spread: 0.9, life: [0.4, 0.8], size: [4, 9], spin: 8 });
          spray(Math.sin(root.start) < 0 ? dustBack : dust, { x: base.x, y: base.y, count: 4, speed: [10, 60], life: [0.6, 1], size: [26, 42] });
        });
      }
      fx.at(0.7, () => {
        T.hit(false, HEAL);
        fx.stage.shake(5, 0.25);
      });
      fx.tween(SQUEEZE, 0.25, p => {
        squeeze = bump(p) * 0.6 + (p >= 0.5 ? 0.4 : 0);
      });
      fx.at(SQUEEZE + 0.12, () => {
        T.hit(true, HEAL);
        fx.stage.shake(8, 0.3);
        spray(leaves, { x, y: y - 90, count: 12, speed: [120, 360], life: [0.6, 1.1], size: [9, 14], spin: 9 });
        spray(dust, { x, y: y - 40, count: 8, speed: [30, 120], life: [0.7, 1.1], size: [30, 50], jitter: 40 });
      });
      groundGlow(fx, { at: 0.15, dur: END + 0.3, x, y, r: 160, css: '#3f6212' });
      if (fx.since(3)) levelAccent(fx, 'nature', SQUEEZE + 0.12, T);
    },
  },
  {
    id: 'roots-hand', name: 'Хватка древа', group: 'Природа', spell: 'Entangling Roots', duration: 2.9,
    levels: true,
    targets: [1, 2, 2],
    artifacts: [{
      key: 'ElderGroveStaff', name: 'Посох древней рощи', kind: 'HealPerStatus',
      about: 'За каждого опутанного врага самый раненый союзник лечится — под ним раскрывается руна-цветок с корешками',
      then: { concept: 'rune-bloom', level: 1, targets: 'allies', after: 0.7 },
    }],
    about: 'Земля вспучивается, под ней разгорается руна; корни-пальцы огромной руки со светящимися жилами поднимаются, обрастают листьями и смыкаются над целью',
    build: perTarget(fx => {
      const { enemies } = fx.stage;
      const T = enemies[0];
      const { x, y } = T.feet;
      const dirt = fx.sim(DIRT);
      const dust = fx.sim(DUST);
      const dustBack = fx.sim(DUST, 'back');
      const leaves = fx.sim(LEAF_FX);
      const cracks = fx.gfx('back', 'normal');
      const RISE = 0.3;
      const CLENCH = 1.15;
      const END = 2.3;
      const top = T.head.y - 30;
      const leaf = leafTexture();
      const spores = fx.sim(P.glow(C.nature, 70));
      const sap = fx.sim({ texture: dropTexture('venom'), blend: 'normal', drag: 0.4, gravity: 900, shrink: 0.7 });
      fx.at(0, () => fx.stage.cast(HEAL));
      // Руна под ладонью: два кольца и насечки, вращается в плоскости пола
      // Руна — со 2-го уровня
      const rune = fx.gfx('back', 'add');
      if (fx.since(2)) fx.tween(0.15, END + 0.35, p => {
        const t = 0.15 + p * (END + 0.35);
        const a = fade(p, 0.12, 0.2) * (0.75 + 0.25 * Math.sin(t * 8));
        const r = 140 * ease.out(Math.min((t - 0.15) / 0.4, 1));
        rune.clear();
        if (r < 2) return;
        rune.ellipse(x, y, r, r * GROUND).stroke({ width: 3, color: color('#a3e635'), alpha: 0.8 * a });
        rune.ellipse(x, y, r * 0.78, r * 0.78 * GROUND).stroke({ width: 1.5, color: color('#d9f99d'), alpha: 0.6 * a });
        for (let i = 0; i < 16; i++) {
          const ang = (i / 16) * TAU + t * 0.6;
          const inner = { x: x + Math.cos(ang) * r * 0.8, y: y + Math.sin(ang) * r * 0.8 * GROUND };
          const outer = { x: x + Math.cos(ang) * r * (i % 2 ? 0.92 : 1), y: y + Math.sin(ang) * r * (i % 2 ? 0.92 : 1) * GROUND };
          rune.moveTo(inner.x, inner.y).lineTo(outer.x, outer.y).stroke({ width: 2, color: color('#bef264'), alpha: 0.7 * a });
        }
      });
      // Споры поднимаются вокруг, пока держит (3-й уровень)
      if (fx.since(3)) fx.tween(0.5, END - 0.3, () => spray(spores, { x: x + rand(-120, 120), y: y - rand(0, 200), count: fx.count(18), speed: [10, 40], dir: -Math.PI / 2, spread: 0.8, life: [0.8, 1.4], size: [6, 12] }));
      fx.at(RISE - 0.1, () => {
        fx.stage.shake(6, 0.5);
        spray(dustBack, { x, y, count: 10, speed: [20, 90], life: [0.7, 1.2], size: [40, 70], jitter: 70 });
      });
      fx.tween(0.1, END + 0.4, p => {
        const t = p * (END + 0.4);
        cracks.clear();
        groundCracks(cracks, x, y, 10, 170, ease.out(Math.min(t / 0.35, 1)), t > END ? 1 - (t - END) / 0.4 : 1, 11);
      });
      // Ладонь: у ног цели вспучивается бугристый ком сплетённых корней, из него растут пальцы
      const palm = fx.gfx('front', 'normal');
      const palmBack = fx.gfx('back', 'normal');
      const humps = Array.from({ length: 5 }, (_, i) => ({ from: -1 + i * 0.35 + rand(-0.1, 0.1), span: rand(0.7, 1.1), h: rand(18, 34), back: i % 2 === 1 }));
      fx.tween(RISE - 0.1, END - RISE + 0.45, p => {
        const t = RISE - 0.1 + p * (END - RISE + 0.45);
        const grow = t > END ? 1 - ease.in((t - END) / 0.35) : ease.out(Math.min((t - RISE + 0.1) / 0.35, 1));
        palm.clear();
        palmBack.clear();
        if (grow <= 0.02) return;
        for (const hump of humps) {
          const g = hump.back ? palmBack : palm;
          const pts = path(12, k => {
            const u = hump.from + k * hump.span;
            return { x: x + u * 105, y: y + (hump.back ? -14 : 10) - Math.sin(k * Math.PI) * hump.h * grow };
          });
          drawRoot(g, pts, 26 * grow, 1);
        }
      });
      // Пальцы: основание по дуге вокруг цели, растут вверх и загибаются кончиком к ней
      // Пальцев: 3 на 1-м уровне, 5 со 2-го
      const fingers = (fx.since(2) ? [-1, -0.5, 0, 0.5, 1] : [-0.6, 0, 0.6]).map(u => {
        const a = Math.PI / 2 + u * 2.2;
        const behind = Math.sin(a) < 0.2 && Math.abs(u) < 0.6 ? false : Math.sin(a) < 0;
        return {
          g: fx.gfx(u === 0 || behind ? 'back' : 'front', 'normal'),
          base: polar({ x, y }, 85, a + Math.PI, 0.4),
          width: u === 0 ? 34 : rand(24, 32),
          at: RISE + Math.abs(u) * 0.12,
          top: top - rand(-10, 45),
          reachIn: rand(0.4, 0.62),
          wobble: rand(0, TAU),
        };
      });
      for (const f of fingers) {
        const side = Math.sign(f.base.x - x) || 1;
        const layer = f.g.parent === fx.stage.back ? 'back' : 'front';
        // Жила сока вдоль пальца и листья на суставах
        const vein = new Beam(fx, 18, 3, '#a3e635', layer, '#f7fee7');
        // Жилы и листья — со 2-го уровня
        const buds = (fx.since(2) ? [0.42, 0.72] : []).map(k => ({ k, dot: fx.tex(leaf, layer), turn: rand(-0.4, 0.4) }));
        // Палец — дуга-коготь: от основания наружу-вверх и к цели, кончик смотрит вниз; сжатие опускает и сводит кончики
        const tip0 = { x: x + side * rand(8, 40), y: f.top + rand(10, 45) };
        const ctrl = { x: f.base.x + side * 40, y: f.top - 70 };
        const fingerPath = (reach: number, clench: number) => {
          const tip = { x: lerp(tip0.x, x + side * 6, clench * 0.6), y: tip0.y + 55 * clench };
          return path(18, k => {
            const p = bez(f.base, ctrl, tip, k * reach);
            return { x: p.x + side * Math.sin(k * 7 + f.wobble) * 4, y: p.y };
          });
        };
        fx.tween(f.at, END - f.at + 0.35, p => {
          const t = f.at + p * (END - f.at + 0.35);
          const reach = t < f.at + 0.45 ? ease.out((t - f.at) / 0.45) : t > END ? 1 - ease.in((t - END) / 0.35) : 1;
          const clench = t < CLENCH ? 0 : ease.back(Math.min((t - CLENCH) / 0.2, 1));
          f.g.clear();
          if (reach <= 0.02) return;
          const pts = fingerPath(reach, clench);
          drawRoot(f.g, pts, f.width, 1);
          // Суставы: утолщения на трети и двух третях пальца
          for (const k of [0.33, 0.62]) {
            const i = Math.floor(k * (pts.length - 1));
            if (i >= pts.length - 1) continue;
            const w = f.width * Math.pow(1 - k, 0.7) * 0.62;
            f.g.ellipse(pts[i].x, pts[i].y, w, w * 0.8).fill({ color: color('#4a301b') });
            f.g.ellipse(pts[i].x - w * 0.3, pts[i].y - w * 0.25, w * 0.45, w * 0.3).fill({ color: color('#8f6a45'), alpha: 0.7 });
          }
          if (reach > 0.5) rootlet(f.g, pts[6], side > 0 ? 0.3 : Math.PI - 0.3, 30, 1);
          // Жила: пульсирует, после сжатия разгорается — сила течёт к кончикам
          const flow = t > CLENCH ? 1 : 0.55;
          const veinPts = pts.map((pt, i) => ({ x: pt.x - side * f.width * 0.12 * (1 - i / pts.length), y: pt.y }));
          vein.set(veinPts, reach > 0.4 && fx.since(2) ? flow * (0.55 + 0.45 * Math.sin(t * 14 + f.wobble)) : 0);
          for (const bud of buds) {
            const grown = clamp01((reach - 0.75) / 0.25) * clamp01((t - f.at - 0.35) / 0.3);
            const i = Math.floor(bud.k * (pts.length - 1));
            if (grown <= 0 || i >= pts.length - 1) {
              bud.dot.hide();
              continue;
            }
            const dir = Math.atan2(pts[i + 1].y - pts[i].y, pts[i + 1].x - pts[i].x) + side * 1.2 + bud.turn;
            const at = polar(pts[i], 10 * grown, dir);
            bud.dot.put(at.x, at.y, 14 * ease.back(grown), 1, dir);
          }
          if (p >= 1) {
            vein.hide();
            buds.forEach(bud => bud.dot.hide());
          }
          // Уходит в землю — с пальца сыплется земля
          if (t > END && Math.random() < 0.4) spray(dirt, { x: pts[pts.length - 1].x, y: pts[pts.length - 1].y, count: 1, speed: [10, 60], life: [0.3, 0.6], size: [3, 6], spin: 6 });
        });
        fx.at(f.at, () => {
          spray(dirt, { x: f.base.x, y: f.base.y, count: 10, speed: [180, 460], dir: -Math.PI / 2, spread: 0.9, life: [0.4, 0.9], size: [4, 10], spin: 8 });
          spray(dust, { x: f.base.x, y: f.base.y, count: 4, speed: [10, 60], life: [0.6, 1], size: [28, 44] });
        });
      }
      fx.at(0.75, () => T.hit(false, HEAL));
      fx.at(CLENCH + 0.15, () => {
        T.hit(true, HEAL);
        fx.stage.shake(10, 0.35);
        spray(leaves, { x, y: top + 40, count: 14, speed: [120, 380], life: [0.6, 1.1], size: [9, 14], spin: 9 });
        spray(dirt, { x, y: top + 40, count: 12, speed: [100, 300], life: [0.4, 0.8], size: [3, 7], spin: 8 });
        spray(sap, { x, y: T.center.y, count: 16, speed: [150, 380], dir: -Math.PI / 2, spread: 1.3, life: [0.4, 0.8], size: [5, 9], align: true });
      });
      pop(fx, { at: CLENCH + 0.15, dur: 0.45, x, y: T.center.y, r: 200, css: '#bef264', alpha: 0.75 });
      ringWave(fx, { at: CLENCH + 0.15, dur: 0.55, x, y, from: 40, to: 240, css: '#bef264', squash: GROUND, layer: 'back' });
      // Удушающие тики: корни сжимаются ещё дважды
      [CLENCH + 0.55, CLENCH + 0.9].slice(0, fx.lvl([0, 1, 2])).forEach(time => fx.at(time, () => {
        T.hit(false, HEAL);
        fx.stage.shake(3, 0.15);
        spray(leaves, { x: x + rand(-30, 30), y: T.center.y, count: 4, speed: [60, 200], life: [0.5, 0.9], size: [8, 12], spin: 8 });
      }));
      groundGlow(fx, { at: 0.15, dur: END + 0.3, x, y, r: 180, css: '#3f6212' });
      if (fx.since(3)) levelAccent(fx, 'nature', CLENCH + 0.15, T);
    }, 0.3),
  },
];
