import {
  C, GROUND, P, TAU, angle, arc, bez, bump, clamp01, ease, fade, fly, groundGlow, lerp, levelAccent, mix, palette, polar, pop, rand, ringWave, spray,
  perTarget, type Concept, type Pt,
} from './kit';
import { featherTexture, flameTexture } from './sprites';

// Концепты огня
const FLAME = { texture: 'smoke' as const, drag: 0.2, gravity: -520, shrink: 0.15, stretch: [0.7, 2] as [number, number], tint: palette(C.fire.slice(1)), alpha: (life: number) => bump(life) * 0.9 };

export const FIRE_CONCEPTS: Concept[] = [
  {
    id: 'fire-spiral', name: 'Спиральная комета', group: 'Огонь', spell: 'Fireball', duration: 1.9,
    levels: true,
    about: 'Шар летит по дуге, вокруг него вьются два огонька; взрыв с кольцом и искрами',
    build(fx) {
      const { caster, enemies } = fx.stage;
      const T = enemies[0];
      const from = caster.center;
      const to = T.center;
      const trail = fx.sim(P.ember(C.fire, 60));
      const sparks = fx.sim(P.spark(C.fire, 300));
      const halo = fx.dot('glow', '#ff6a00');
      const core = fx.dot('glow', '#fff1a8');
      // Огоньков вокруг шара: 0 / 1 / 2 по уровню
      const orbs = Array.from({ length: fx.lvl([0, 1, 2]) }, () => fx.dot('glow', '#ffe27a'));
      fx.at(0, () => fx.stage.cast('#ff7a1a'));
      fly(fx, 0.25, 0.55, p => arc(from, to, p * (0.6 + 0.4 * p), 70), (pt, p, dir) => {
        halo.put(pt.x, pt.y, fx.lvl([52, 62, 72]), 0.9);
        core.put(pt.x, pt.y, 26);
        orbs.forEach((orb, i) => {
          const a = p * 22 + i * Math.PI;
          const q = polar(pt, Math.sin(a) * 34, dir + Math.PI / 2);
          orb.put(q.x, q.y, 12 + 6 * Math.cos(a));
          spray(trail, { x: q.x, y: q.y, count: fx.count(60), speed: [10, 50], life: [0.25, 0.5], size: [4, 9] });
        });
        spray(trail, { x: pt.x, y: pt.y, count: fx.count(120), speed: [20, 90], dir: dir + Math.PI, spread: 0.5, life: [0.25, 0.55], size: [7, 16], jitter: 8 });
      }, [halo, core, ...orbs]);
      fx.at(0.8, () => {
        T.hit(false, '#ff7a1a');
        fx.stage.shake(6, 0.25);
        spray(sparks, { x: to.x, y: to.y, count: fx.lvl([24, 36, 52]), speed: [150, 520], life: [0.35, 0.8], size: [4, 9] });
        spray(trail, { x: to.x, y: to.y, count: 30, speed: [40, 220], life: [0.4, 0.9], size: [10, 24] });
      });
      pop(fx, { at: 0.8, dur: 0.35, x: to.x, y: to.y, r: fx.lvl([120, 150, 185]), css: '#ffb347' });
      if (fx.since(2)) ringWave(fx, { at: 0.8, dur: 0.5, x: to.x, y: to.y, from: 20, to: 150, css: '#ffd27a' });
      groundGlow(fx, { at: 0.8, dur: 1, x: T.feet.x, y: T.feet.y, r: 130, css: '#ff7a1a' });
      if (fx.since(3)) levelAccent(fx, 'fire', 0.8, T);
    },
  },
  {
    id: 'fire-serpent', name: 'Огненный змей', group: 'Огонь', spell: 'Fireball', duration: 2.4,
    levels: true,
    targets: [1, 3, 'all'],
    artifacts: [{
      key: 'PhoenixStaff', name: 'Посох феникса', kind: 'EchoOnKill',
      about: 'Огненный шар убил цель — из пепла взлетает золотой змей-феникс и бьёт следующего врага',
      then: { concept: 'fire-serpent', level: 3, targets: 'one', origin: 'target', after: 0.8 },
    }],
    about: 'Змей из языков пламени подлетает к цели, обвивает её кольцом и ныряет внутрь — взрыв; на каждую цель свой змей',
    build: perTarget(fx => {
      const { caster, enemies } = fx.stage;
      const T = enemies[0];
      const c = T.center;
      // Повтор от «Посоха феникса»: золотой змей взлетает из пепла убитого (fx.origin), а не из рук кастера
      const phoenix = fx.triggered('EchoOnKill');
      const from = phoenix && fx.origin ? { x: fx.origin.x, y: fx.origin.y - 60 } : { x: caster.center.x + 40, y: caster.center.y - 30 };
      const ORX = 105;
      const ORY = 44;
      const entry = { x: c.x - ORX, y: c.y };
      const T0 = 0.25;
      const APPROACH = 0.5;
      const ORBIT = 0.62;
      const DIVE = 0.14;
      const HIT = T0 + APPROACH + ORBIT + DIVE;
      const normal = angle(from, entry) + Math.PI / 2;
      // Голова в момент time: подлёт волной, виток вокруг цели (сверху — за ней), нырок в центр
      const head = (time: number): Pt & { behind: boolean } => {
        if (time <= T0 + APPROACH) {
          const p = ease.inOut(clamp01((time - T0) / APPROACH));
          const base = mix(from, entry, p);
          const wave = Math.sin(p * TAU * 1.5) * 52 * bump(p);
          return { x: base.x + Math.cos(normal) * wave, y: base.y + Math.sin(normal) * wave - Math.sin(Math.PI * p) * 60, behind: false };
        }
        if (time <= T0 + APPROACH + ORBIT) {
          const u = (time - T0 - APPROACH) / ORBIT;
          const a = Math.PI + u * TAU;
          const k = 1 - 0.22 * u;
          return { x: c.x + Math.cos(a) * ORX * k, y: c.y + Math.sin(a) * ORY * k, behind: Math.sin(a) < 0 };
        }
        const p = ease.in(clamp01((time - T0 - APPROACH - ORBIT) / DIVE));
        return { ...mix({ x: c.x - ORX * 0.78, y: c.y }, c, p), behind: false };
      };
      // Змей длиннее и крупнее с уровнем
      const K = fx.lvl([0.7, 0.85, 1]);
      const SEGMENTS = fx.lvl([12, 15, 18]);
      const LAG = 0.02;
      const flame = flameTexture();
      const parts = Array.from({ length: SEGMENTS }, (_, i) => ({
        front: fx.tex(flame, 'front', 'add', phoenix ? '#ffe9a8' : '#ffffff'),
        back: fx.tex(flame, 'back', 'add', phoenix ? '#ffe9a8' : '#ffffff'),
        size: lerp(40, 11, i / (SEGMENTS - 1)) * K,
      }));
      const glowFront = fx.dot('glow', phoenix ? '#ffc94a' : '#ff6a00');
      const glowBack = fx.dot('glow', phoenix ? '#ffc94a' : '#ff6a00', 'back');
      const eyes = [fx.dot('glow', '#fffbe0'), fx.dot('glow', '#fffbe0')];
      const embers = fx.sim(P.ember(C.fire, 90));
      const sparks = fx.sim(P.spark(C.fire, 400));
      fx.at(0, () => fx.stage.cast('#ff7a1a'));
      // Феникс: вспышка возрождения и золотые перья за змеем
      const feathers = phoenix ? fx.sim({ texture: featherTexture(), drag: 0.5, gravity: 90, shrink: 0.7, tint: palette(['#fff1a8', '#ffd166', '#ffb347']), alpha: life => bump(life) }) : null;
      if (phoenix) {
        pop(fx, { at: 0.05, dur: 0.5, x: from.x, y: from.y, r: 170, css: '#ffd166' });
        ringWave(fx, { at: 0.05, dur: 0.5, x: from.x, y: from.y, from: 20, to: 160, css: '#fff1a8' });
        fx.at(0.05, () => feathers && spray(feathers, { x: from.x, y: from.y, count: 16, speed: [80, 260], life: [0.7, 1.2], size: [12, 20], spin: 5 }));
      }
      fx.tween(T0, HIT - T0, p => {
        const time = T0 + p * (HIT - T0);
        parts.forEach((part, i) => {
          const t = time - i * LAG;
          if (t < T0) {
            part.front.hide();
            part.back.hide();
            return;
          }
          const pt = head(t);
          const prev = head(Math.max(T0, t - 0.012));
          const dir = Math.atan2(pt.y - prev.y, pt.x - prev.x);
          const [show, hide] = pt.behind ? [part.back, part.front] : [part.front, part.back];
          hide.hide();
          show.put(pt.x, pt.y, part.size * (1 + Math.sin(time * 40 + i) * 0.1), lerp(1, 0.5, i / SEGMENTS), dir);
        });
        const h = head(time);
        const prev = head(Math.max(T0, time - 0.012));
        const dir = Math.atan2(h.y - prev.y, h.x - prev.x);
        (h.behind ? glowBack : glowFront).put(h.x, h.y, 100 * K, 0.85);
        (h.behind ? glowFront : glowBack).hide();
        eyes.forEach((eye, i) => {
          const q = polar(polar(h, 16, dir), (i ? 1 : -1) * 10, dir + Math.PI / 2);
          eye.put(q.x, q.y, 7, h.behind ? 0 : 1);
        });
        const tail = head(Math.max(T0, time - SEGMENTS * LAG));
        spray(embers, { x: tail.x, y: tail.y, count: fx.count(80), speed: [10, 60], life: [0.3, 0.6], size: [5, 11], jitter: 8 });
        spray(sparks, { x: h.x, y: h.y, count: fx.count(40), speed: [40, 160], life: [0.2, 0.5], size: [2, 5] });
        if (feathers) spray(feathers, { x: tail.x, y: tail.y, count: fx.count(14), speed: [20, 70], life: [0.6, 1], size: [10, 16], spin: 4, jitter: 10 });
      }, [...parts.flatMap(part => [part.front, part.back]), glowFront, glowBack, ...eyes]);
      fx.at(T0 + APPROACH, () => T.hit(false, '#ff7a1a'));
      groundGlow(fx, { at: T0 + APPROACH, dur: ORBIT + DIVE + 0.9, x: T.feet.x, y: T.feet.y, r: 150, css: '#ff6a00' });
      fx.at(HIT, () => {
        T.hit(true, '#ff7a1a');
        fx.stage.shake(10, 0.35);
        spray(sparks, { x: c.x, y: c.y, count: 70, speed: [180, 680], life: [0.4, 0.9], size: [4, 10] });
        spray(embers, { x: c.x, y: c.y, count: 45, speed: [60, 300], life: [0.5, 1], size: [10, 26] });
      });
      pop(fx, { at: HIT, dur: 0.45, x: c.x, y: c.y, r: 240 * K, css: '#ff9a3c' });
      ringWave(fx, { at: HIT, dur: 0.55, x: c.x, y: c.y, from: 20, to: 190, css: '#ffd27a' });
      if (fx.since(2)) ringWave(fx, { at: HIT, dur: 0.6, x: T.feet.x, y: T.feet.y, from: 30, to: 240, css: '#ffb347', squash: GROUND, layer: 'back' });
      if (fx.since(3)) levelAccent(fx, 'fire', HIT, T);
    }, 0.18),
  },
  {
    id: 'fire-surf-pillars', name: 'Прибой и столпы', group: 'Огонь', spell: 'Flame Wave', duration: 2.6,
    levels: true,
    targets: [2, 3, 'all'],
    artifacts: [{
      key: 'ArmageddonBlade', name: 'Клинок Армагеддона', kind: 'SpellProc',
      about: 'Волна задевает ещё одного врага, а вслед за ней в каждую цель падает метеор',
      then: { concept: 'meteor-sky', level: 1, targets: 'same', after: 0.7 },
    }],
    about: 'Вал пламени катится по земле через врагов; до кого докатился — под тем раскрывается круг и бьёт столп огня',
    build(fx) {
      const { caster, enemies } = fx.stage;
      // Детонация горения от «Сердца вулкана»: вала нет, столпы разом бьют из-под горящих врагов
      const detonate = fx.triggered('DetonateStatus');
      const x0 = caster.feet.x + 60;
      const x1 = Math.max(...enemies.map(e => e.feet.x)) + 90;
      const y0 = caster.feet.y;
      const y1 = enemies.reduce((sum, e) => sum + e.feet.y, 0) / enemies.length;
      const flames = fx.sim(FLAME);
      const backFlames = fx.sim(FLAME, 'back');
      const pillarFlames = fx.sim({ ...FLAME, gravity: -900 });
      const sparks = fx.sim(P.spark(C.fire, -200));
      const glow = fx.radial([[0, 0.8], [0.5, 0.4], [1, 0]], '#ff7a1a', 'back');
      const START = 0.3;
      const TRAVEL = 1.0;
      fx.at(0, () => fx.stage.cast('#ff7a1a'));
      if (!detonate) fx.tween(START, TRAVEL, p => {
        const x = lerp(x0, x1, p);
        const y = lerp(y0, y1, p);
        glow.oval(x, y, 190, 110, fade(p, 0.1, 0.2));
        for (const [sim, count] of [[flames, 130], [backFlames, 160]] as const) {
          for (let i = fx.count(count); i > 0; i--) {
            const dy = sim === flames ? rand(10, 110) : rand(-110, 10);
            sim.spawn({ x: x + rand(-26, 26), y: y + dy, vx: rand(60, 220), vy: rand(-320, -120), lifetime: rand(0.3, 0.6), size: rand(10, 26) });
          }
        }
        spray(sparks, { x, y: y + rand(-90, 90), count: fx.count(40), speed: [80, 320], dir: -Math.PI / 2, spread: 0.9, life: [0.4, 0.9], size: [3, 7] });
      }, [glow]);
      fx.at(START + 0.2, () => fx.stage.shake(4, 0.7));
      // Столп под каждым врагом в тот миг, когда до него докатился вал
      enemies.forEach((enemy, i) => {
        const at = detonate ? 0.3 + i * 0.07 : START + TRAVEL * clamp01((enemy.feet.x - x0) / (x1 - x0));
        const { x, y } = enemy.feet;
        // Детонация: горение стягивается в тело угольками, прежде чем рвануть
        if (detonate) fx.tween(0, at, p => {
          for (let n = fx.count(60); n > 0; n--) {
            const a = Math.random() * TAU;
            const s = polar(enemy.center, rand(60, 110), a);
            pillarFlames.spawn({ x: s.x, y: s.y, vx: -Math.cos(a) * 260 * p, vy: -Math.sin(a) * 260 * p, lifetime: 0.25, size: rand(6, 12) });
          }
        });
        const mark = fx.dot('ring', '#ffb347', 'back');
        const pillar = fx.dot('glow', '#ff7a1a');
        const core = fx.dot('glow', '#fff1a8');
        fx.tween(at - 0.12, 0.2, p => mark.oval(x, y, 20 + 55 * ease.out(p), (20 + 55 * ease.out(p)) * GROUND, 0.9), [mark]);
        fx.tween(at + 0.05, 0.55, p => {
          const a = fade(p, 0.08, 0.5);
          const h = 240 * fx.lvl([0.55, 0.8, 1]) * ease.out3(Math.min(p * 3, 1));
          pillar.oval(x, y - h / 2, 62, h * 0.75, a);
          core.oval(x, y - h / 2, 22, h * 0.7, a);
          for (let n = fx.count(p < 0.6 ? fx.lvl([90, 150, 210]) : 0); n > 0; n--) {
            pillarFlames.spawn({ x: x + rand(-30, 30), y: y + rand(-10, 10), vx: rand(-40, 40), vy: rand(-600, -260), lifetime: rand(0.3, 0.55), size: rand(10, 22) });
          }
        }, [pillar, core]);
        fx.at(at + 0.05, () => {
          enemy.hit(false, '#ff7a1a');
          spray(sparks, { x, y, count: 18, speed: [200, 500], dir: -Math.PI / 2, spread: 0.7, life: [0.5, 1], size: [3, 8] });
        });
        ringWave(fx, { at: at + 0.05, dur: 0.5, x, y, from: 30, to: 140, css: '#ffb347', squash: GROUND, layer: 'back' });
        groundGlow(fx, { at: at + 0.05, dur: 0.9, x, y, r: 110, css: '#ff6a00' });
        if (fx.since(3)) levelAccent(fx, 'fire', at + 0.05, enemy);
      });
    },
  },
  {
    id: 'fire-surf-wall', name: 'Огненная стена', group: 'Огонь', spell: 'Flame Wave', duration: 2.6,
    levels: true,
    about: 'Вал пламени катится к строю и разбивается — по всей его линии разом вырастает стена столпов, под врагами самые высокие',
    build(fx) {
      const { caster, enemies } = fx.stage;
      const x0 = caster.feet.x + 60;
      const xs = enemies.map(e => e.feet.x);
      const front = Math.min(...xs) - 70;
      const back = Math.max(...xs) + 70;
      const y0 = caster.feet.y;
      const y1 = enemies.reduce((sum, e) => sum + e.feet.y, 0) / enemies.length;
      const flames = fx.sim(FLAME);
      const backFlames = fx.sim(FLAME, 'back');
      const pillarFlames = fx.sim({ ...FLAME, gravity: -900 });
      const embers = fx.sim(P.spark(C.fire, -160));
      const glow = fx.radial([[0, 0.8], [0.5, 0.4], [1, 0]], '#ff7a1a', 'back');
      const START = 0.25;
      const TRAVEL = 0.6;
      const CRASH = START + TRAVEL;
      fx.at(0, () => fx.stage.cast('#ff7a1a'));
      // Вал разгоняется и растёт к строю
      fx.tween(START, TRAVEL, p => {
        const x = lerp(x0, front, ease.in(p));
        const y = lerp(y0, y1, p);
        const grow = 0.6 + 0.6 * p;
        glow.oval(x, y, 170 * grow, 100 * grow, fade(p, 0.1, 0));
        for (const [sim, count] of [[flames, 120], [backFlames, 150]] as const) {
          for (let i = fx.count(count * grow); i > 0; i--) {
            const dy = sim === flames ? rand(10, 100) : rand(-100, 10);
            sim.spawn({ x: x + rand(-24, 24), y: y + dy * grow, vx: rand(80, 260), vy: rand(-360, -140) * grow, lifetime: rand(0.3, 0.55), size: rand(10, 24) * grow });
          }
        }
      }, [glow]);
      fx.at(CRASH, () => {
        fx.stage.shake(9, 0.4);
        fx.stage.flash('#ff7a1a', 0.2, 0.2);
        spray(embers, { x: front, y: y1, count: 50, speed: [200, 600], dir: -Math.PI / 2, spread: 1.2, life: [0.5, 1.1], size: [3, 8] });
      });
      pop(fx, { at: CRASH, dur: 0.35, x: front, y: y1 - 40, r: 200, css: '#ffb347', alpha: 0.8 });
      // Стена столпов по линии строя: от края к краю с небольшим разбегом по времени
      // Столпов в стене: 3 / 5 / 7 по уровню
      const COLS = fx.lvl([3, 5, 7]);
      for (let i = 0; i < COLS; i++) {
        const t = i / (COLS - 1);
        const x = lerp(front, back, t) + rand(-12, 12);
        const near = enemies.reduce((best, e) => (Math.abs(e.feet.x - x) < Math.abs(best.feet.x - x) ? e : best), enemies[0]);
        const y = lerp(y1, near.feet.y, 0.7) + rand(-20, 20);
        const tall = Math.abs(near.feet.x - x) < 50 ? 1 : 0.65;
        const at = CRASH + t * 0.18;
        const pillar = fx.dot('glow', '#ff7a1a');
        const core = fx.dot('glow', '#fff1a8');
        fx.tween(at, 0.7, p => {
          const a = fade(p, 0.06, 0.45);
          const h = 270 * tall * ease.out3(Math.min(p * 3, 1));
          pillar.oval(x, y - h / 2, 56 * tall, h * 0.75, a);
          core.oval(x, y - h / 2, 20 * tall, h * 0.7, a);
          for (let n = fx.count(p < 0.55 ? 150 * tall : 0); n > 0; n--) {
            pillarFlames.spawn({ x: x + rand(-26, 26), y: y + rand(-8, 8), vx: rand(-30, 30), vy: rand(-620, -260) * tall, lifetime: rand(0.3, 0.55), size: rand(9, 22) });
          }
        }, [pillar, core]);
        groundGlow(fx, { at, dur: 1, x, y, r: 90, css: '#ff6a00' });
      }
      for (const enemy of enemies) {
        const at = CRASH + clamp01((enemy.feet.x - front) / (back - front)) * 0.18;
        fx.at(at, () => enemy.hit(fx.since(3), '#ff7a1a'));
      }
      ringWave(fx, { at: CRASH, dur: 0.7, x: (front + back) / 2, y: y1, from: 60, to: (back - front) * 0.8, css: '#ffb347', squash: GROUND, layer: 'back' });
      if (fx.since(3)) fx.stage.enemies.forEach(e => levelAccent(fx, 'fire', CRASH + 0.2, e));
    },
  },
  {
    id: 'meteor-sky', name: 'Метеор', group: 'Огонь', spell: 'Meteor', duration: 2.6,
    levels: true,
    targets: [1, 1, 1],
    extras: true,
    artifacts: [{
      key: 'VolcanoHeart', name: 'Сердце вулкана', kind: 'DetonateStatus',
      about: 'После удара горение на всех врагах детонирует: из-под каждого горящего бьёт столп огня',
      then: { concept: 'fire-surf-pillars', level: 1, targets: 'all', after: 0.75 },
    }],
    about: 'На земле разгорается метка, с неба падает глыба — вспышка, волна, обломки. Срабатывание от клинка — по метеору в каждую цель',
    build: perTarget(fx => {
      const { enemies, bystanders } = fx.stage;
      const T = enemies[0];
      const to = T.feet;
      const from = { x: to.x + 330, y: to.y - 760 };
      const mark = fx.dot('ring', '#ff7a1a', 'back');
      const markIn = fx.dot('ring', '#ffd27a', 'back');
      const trail = fx.sim(P.ember(C.fire, 0));
      const smoke = fx.sim(P.smoke('#2a1a14', 60));
      const sparks = fx.sim(P.spark(C.fire, 700));
      const debris = fx.sim(P.debris('#3b2a22'));
      const rock = fx.dot('disc', '#3b1d10', 'front', 'normal');
      const halo = fx.dot('glow', '#ff6a00');
      const core = fx.dot('glow', '#fff1a8');
      const HIT = 1.05;
      // Глыба и взрыв растут с уровнем; дождь метеоров по многим целям — мельче
      const K = fx.lvl([0.65, 0.82, 1]) * (fx.density < 1 ? 0.75 : 1);
      fx.at(0, () => fx.stage.cast('#ff7a1a'));
      fx.tween(0.1, HIT - 0.1, p => {
        mark.oval(to.x, to.y, 130, 130 * GROUND, 0.35 + 0.65 * p);
        markIn.oval(to.x, to.y, 130 * (1 - p), 130 * (1 - p) * GROUND, 0.9);
      }, [mark, markIn]);
      fly(fx, 0.6, HIT - 0.6, p => mix(from, to, ease.in(p)), (pt, _p, dir) => {
        halo.put(pt.x, pt.y, 130 * K, 0.9);
        rock.put(pt.x, pt.y, 30 * K);
        core.put(pt.x, pt.y, 44 * K, 0.9);
        spray(trail, { x: pt.x, y: pt.y, count: fx.count(260), speed: [20, 160], dir: dir + Math.PI, spread: 0.35, life: [0.3, 0.7], size: [10, 26], jitter: 14 });
        spray(smoke, { x: pt.x, y: pt.y, count: fx.count(40), speed: [10, 60], life: [0.6, 1.1], size: [14, 26], jitter: 10 });
      }, [halo, rock, core]);
      fx.at(HIT, () => {
        T.hit(true, '#ff7a1a');
        fx.stage.flash('#ffd9a0', 0.55, 0.35);
        fx.stage.shake(16, 0.5);
        spray(sparks, { x: to.x, y: to.y, count: fx.lvl([45, 70, 100]), speed: [250, 900], dir: -Math.PI / 2, spread: 1.3, life: [0.5, 1.2], size: [4, 11] });
        spray(debris, { x: to.x, y: to.y, count: fx.lvl([10, 16, 26]), speed: [300, 800], dir: -Math.PI / 2, spread: 1, life: [0.8, 1.3], size: [7, 16], spin: 8 });
        spray(smoke, { x: to.x, y: to.y - 20, count: 16, speed: [40, 160], life: [0.9, 1.5], size: [30, 55], jitter: 40 });
        spray(trail, { x: to.x, y: to.y - 20, count: 50, speed: [80, 420], dir: -Math.PI / 2, spread: 1.4, life: [0.4, 0.9], size: [12, 30] });
      });
      pop(fx, { at: HIT, dur: 0.5, x: to.x, y: to.y - 40, r: 330 * K, css: '#ffb347' });
      ringWave(fx, { at: HIT, dur: 0.6, x: to.x, y: to.y, from: 40, to: 340, css: '#ffd27a', squash: GROUND, layer: 'back' });
      if (fx.since(2)) ringWave(fx, { at: HIT + 0.12, dur: 0.7, x: to.x, y: to.y, from: 30, to: 460, css: '#ff7a1a', squash: GROUND, layer: 'back', alpha: 0.7 });
      groundGlow(fx, { at: HIT, dur: 1.4, x: to.x, y: to.y, r: 220, css: '#ff5a00' });
      if (fx.since(3)) levelAccent(fx, 'fire', HIT, T);
      // Доп. эффект: за глыбой падают два обломка поменьше по соседям
      if (fx.extras) {
        bystanders.slice(0, 2).forEach((enemy, i) => {
          const at = HIT + 0.2 + i * 0.15;
          const end = enemy.feet;
          const start = { x: end.x + 220, y: end.y - 600 };
          const shard = fx.dot('glow', '#ff7a1a');
          const shardCore = fx.dot('glow', '#fff1a8');
          fly(fx, at, 0.28, p => mix(start, end, ease.in(p)), (pt, _p, dir) => {
            shard.put(pt.x, pt.y, 60, 0.9);
            shardCore.put(pt.x, pt.y, 20);
            spray(trail, { x: pt.x, y: pt.y, count: fx.count(160), speed: [10, 90], dir: dir + Math.PI, spread: 0.3, life: [0.2, 0.45], size: [6, 14] });
          }, [shard, shardCore]);
          fx.at(at + 0.28, () => {
            enemy.hit(true, '#ff7a1a');
            spray(sparks, { x: end.x, y: end.y, count: 30, speed: [150, 500], dir: -Math.PI / 2, spread: 1.2, life: [0.4, 0.8], size: [3, 8] });
          });
          pop(fx, { at: at + 0.28, dur: 0.35, x: end.x, y: end.y - 20, r: 140, css: '#ffb347' });
        });
      }
    }, 0.2),
  },
  {
    id: 'meteor-rain', name: 'Звездопад', group: 'Огонь', spell: 'Meteor', duration: 2.4,
    levels: true,
    about: 'Россыпь мелких огненных камней бьёт по всему отряду врага',
    build(fx) {
      const { enemies } = fx.stage;
      const trail = fx.sim(P.ember(C.fire, 0));
      const sparks = fx.sim(P.spark(C.fire, 600));
      fx.at(0, () => fx.stage.cast('#ff7a1a'));
      // Камней: 5 / 7 / 10 по уровню
      const COUNT = fx.lvl([5, 7, 10]);
      for (let i = 0; i < COUNT; i++) {
        const enemy = enemies[i % enemies.length];
        const to = { x: enemy.feet.x + rand(-50, 50), y: enemy.feet.y + rand(-20, 30) };
        const from = { x: to.x + 190, y: to.y - 620 };
        const at = 0.25 + i * (1.2 / COUNT);
        const halo = fx.dot('glow', '#ff7a1a');
        const core = fx.dot('glow', '#fff1a8');
        fly(fx, at, 0.3, p => mix(from, to, p), (pt, _p, dir) => {
          halo.put(pt.x, pt.y, 60, 0.9);
          core.put(pt.x, pt.y, 20);
          spray(trail, { x: pt.x, y: pt.y, count: fx.count(150), speed: [10, 90], dir: dir + Math.PI, spread: 0.3, life: [0.2, 0.45], size: [6, 14], jitter: 6 });
        }, [halo, core]);
        fx.at(at + 0.3, () => {
          enemy.hit(false, '#ff7a1a');
          fx.stage.shake(4, 0.15);
          spray(sparks, { x: to.x, y: to.y, count: 22, speed: [150, 480], dir: -Math.PI / 2, spread: 1.2, life: [0.4, 0.8], size: [3, 8] });
        });
        pop(fx, { at: at + 0.3, dur: 0.3, x: to.x, y: to.y - 10, r: 110, css: '#ffb347' });
        ringWave(fx, { at: at + 0.3, dur: 0.4, x: to.x, y: to.y, from: 15, to: 110, css: '#ffd27a', squash: GROUND, layer: 'back' });
      }
      if (fx.since(3)) {
        fx.stage.enemies.forEach(e => levelAccent(fx, 'fire', 1.6, e));
      }
    },
  },
  {
    id: 'fire-phoenix', name: 'Феникс', group: 'Огонь', spell: 'Flame Wave', duration: 2.5,
    levels: true,
    extras: true,
    about: 'Огненная птица с крыльями из перьев проносится низко сквозь строй врагов, роняя перья и угли',
    build(fx) {
      const { caster, enemies } = fx.stage;
      const from = { x: caster.center.x - 60, y: caster.center.y - 170 };
      const to = { x: Math.max(...enemies.map(e => e.center.x)) + 320, y: Math.min(...enemies.map(e => e.head.y)) - 170 };
      const control = { x: (from.x + to.x) / 2 - 60, y: Math.max(...enemies.map(e => e.center.y)) + 130 };
      // Птица крупнее с уровнем, на 3-м за ней горит земля
      const K = fx.lvl([0.7, 0.85, 1]);
      const START = 0.3;
      const FLY = 1.2;
      const pos = (u: number) => bez(from, control, to, u);
      const feather = featherTexture();
      const flame = flameTexture();
      const wingColors = ['#fff1a8', '#ffe27a', '#ffd166', '#ffb347', '#ff9a3c', '#ff7a1a', '#e2401b'];
      const wings = [-1, 1].map(side => wingColors.map((css, i) => ({ side, i, dot: fx.tex(feather, 'front', 'add', css) })));
      const tail = Array.from({ length: 5 }, (_, i) => fx.tex(feather, 'front', 'add', i % 2 ? '#ff7a1a' : '#ffb347'));
      const body = [fx.tex(flame, 'front', 'add'), fx.tex(flame, 'front', 'add')];
      const halo = fx.dot('glow', '#ff6a00');
      const embers = fx.sim(P.ember(C.fire, -260));
      const drifting = fx.sim({ texture: feather, drag: 0.5, gravity: 80, shrink: 0.7, tint: palette(['#ffe27a', '#ffb347', '#ff7a1a', '#e2401b']), alpha: life => bump(life) });
      const flames = fx.sim({ texture: 'smoke', drag: 0.2, gravity: -560, shrink: 0.15, stretch: [0.7, 2], tint: palette(C.fire.slice(1)), alpha: life => bump(life) * 0.9 });
      fx.at(0, () => fx.stage.cast('#ff7a1a'));
      fly(fx, START, FLY, p => pos(ease.inOut(p)), (pt, p, dir) => {
        const a = fade(p, 0.08, 0.12);
        const flap = Math.sin(p * TAU * 3);
        halo.put(pt.x, pt.y, 150 * K, a * 0.8);
        body[0].put(pt.x, pt.y, 50 * K, a, dir);
        body[1].put(pt.x - Math.cos(dir) * 26 * K, pt.y - Math.sin(dir) * 26 * K, 36 * K, a * 0.8, dir);
        const shoulder = polar(pt, -8, dir);
        for (const wing of wings) {
          for (const f of wing) {
            // Веер от плеча: передние перья почти поперёк полёта, задние отведены назад; взмах качает весь веер
            const spread = dir + f.side * (1.05 + f.i * 0.2 + flap * 0.45);
            const length = (100 - Math.abs(f.i - 2) * 9) * K;
            const center = polar(shoulder, length / 2, spread);
            f.dot.put(center.x, center.y, length / 2, a * (f.side < 0 ? 0.85 : 1), spread);
            if (f.i === 3) spray(embers, { x: center.x, y: center.y, count: fx.count(30), speed: [10, 60], life: [0.5, 0.9], size: [4, 9] });
          }
        }
        const rear = polar(pt, -24, dir);
        tail.forEach((f, j) => {
          const spread = dir + Math.PI + (j - 2) * 0.17 + Math.sin(p * 22 + j) * 0.08;
          const center = polar(rear, 64 * K, spread);
          f.put(center.x, center.y, 64 * K, a, spread);
        });
        spray(drifting, { x: pt.x, y: pt.y, count: fx.count(16 * a), speed: [20, 90], dir: dir + Math.PI, spread: 1, life: [0.8, 1.4], size: [10, 18], spin: 4, jitter: 30 });
        spray(embers, { x: pt.x, y: pt.y, count: fx.count(70 * a), speed: [20, 120], dir: dir + Math.PI, spread: 0.6, life: [0.5, 1.1], size: [6, 16], jitter: 22 });
      }, [halo, ...body, ...tail, ...wings.flat().map(f => f.dot)]);
      for (const enemy of enemies) {
        // Когда птица над врагом: ищем момент, где её x ближе всего к нему
        let best = 0;
        for (let u = 0; u <= 1; u += 0.01) if (Math.abs(pos(ease.inOut(u)).x - enemy.center.x) < Math.abs(pos(ease.inOut(best)).x - enemy.center.x)) best = u;
        const at = START + FLY * best;
        fx.at(at, () => {
          enemy.hit(false, '#ff7a1a');
          for (let i = 0; i < fx.lvl([8, 14, 22]); i++) flames.spawn({ x: enemy.feet.x + rand(-40, 40), y: enemy.feet.y + rand(-8, 8), vx: rand(-40, 40), vy: rand(-520, -220), lifetime: rand(0.3, 0.55), size: rand(12, 24) });
        });
        groundGlow(fx, { at, dur: 1, x: enemy.feet.x, y: enemy.feet.y, r: 110, css: '#ff6a00' });
      }
      fx.at(START + FLY * 0.5, () => fx.stage.shake(4, 0.5));
      if (fx.since(3)) enemies.forEach(e => levelAccent(fx, 'fire', START + FLY * 0.6, e));
      // Доп. эффект: огненный след по земле под строем
      if (fx.extras) {
        const gx0 = Math.min(...enemies.map(e => e.feet.x)) - 60;
        const gx1 = Math.max(...enemies.map(e => e.feet.x)) + 60;
        const gy = enemies.reduce((sum, e) => sum + e.feet.y, 0) / enemies.length;
        fx.tween(START + FLY * 0.35, 1.1, p => {
          for (let i = fx.count(140 * (1 - p)); i > 0; i--) flames.spawn({ x: rand(gx0, gx1), y: gy + rand(-40, 40), vx: rand(-30, 30), vy: rand(-320, -140), lifetime: rand(0.25, 0.5), size: rand(8, 18) });
        });
      }
    },
  },
];
