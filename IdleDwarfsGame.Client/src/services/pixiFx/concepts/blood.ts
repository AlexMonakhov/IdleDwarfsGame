import {
  Beam, C, GROUND, P, TAU, angle, angryEye, bez, bump, color, ease, fade, fly, groundGlow, lerp, levelAccent, mix, palette, path, polar, pop, rand, ringWave, spray, taper,
  perTarget, type Concept, type ConceptUnit, type Fx, type Pt,
} from './kit';
import { dropTexture } from './sprites';

// Концепты крови
const BLOOD = '#dc2626';
// Нарисованная капля крови: летит носом по ходу и падает
const DROP = { texture: 'disc' as const, blend: 'normal' as const, drag: 0.5, gravity: 1100, shrink: 0.6, tint: palette(['#ef4444', '#b91c1c', '#7f1d1d']) };
const DROP_FX = { texture: dropTexture('blood'), blend: 'normal' as const, drag: 0.4, gravity: 1100, shrink: 0.75 };

// «Кинжал тысячи порезов»: стаки кровотечения лопаются — по цели шквал мелких порезов и фонтан крови
const thousandCuts = (fx: Fx, at: number, T: ConceptUnit) => {
  const c = T.center;
  const drips = fx.sim(DROP_FX);
  for (let i = 0; i < 12; i++) {
    const t = at + i * 0.035;
    const dir = rand(0, TAU);
    const mid = { x: c.x + rand(-40, 40), y: c.y + rand(-60, 50) };
    const len = rand(50, 80);
    const a = polar(mid, len / 2, dir + Math.PI);
    const b = polar(mid, len / 2, dir);
    const dark = fx.gfx('front', 'normal');
    const bright = fx.gfx('front', 'add');
    fx.tween(t, 0.32, p => {
      const grow = ease.out(Math.min(p * 5, 1));
      const points = path(8, k => mix(a, b, k * grow));
      const alpha = 1 - ease.in(p);
      taper(dark.clear(), points, 12, '#7f1d1d', 0.85 * alpha);
      taper(bright.clear(), points, 5, '#ef4444', alpha);
      taper(bright, points, 1.6, '#fff1f2', alpha);
    });
  }
  const burst = at + 12 * 0.035;
  fx.at(burst, () => {
    T.hit(true, BLOOD);
    fx.stage.shake(7, 0.25);
    spray(drips, { x: c.x, y: c.y, count: 40, speed: [220, 680], dir: -Math.PI / 2, spread: 1.6, life: [0.5, 1], size: [7, 12], align: true });
  });
  pop(fx, { at: burst, dur: 0.4, x: c.x, y: c.y, r: 170, css: '#dc2626' });
  ringWave(fx, { at: burst, dur: 0.45, x: c.x, y: c.y, from: 20, to: 170, css: '#fecaca' });
};

export const BLOOD_CONCEPTS: Concept[] = [
  {
    id: 'blood-soul-drain', name: 'Душа в кровь', group: 'Кровь', spell: 'Life Drain', duration: 3.2,
    levels: true,
    targets: [1, 1, 1],
    extras: true,
    artifacts: [{
      key: 'BloodMoonFang', name: 'Клык кровавой луны', kind: 'EchoBelowHp',
      about: 'Если у цели осталось меньше половины здоровья, над ней всходит кровавая луна и душу вытягивают ещё раз',
      then: { concept: 'blood-soul-drain', level: 3, targets: 'same', after: 0.8 },
    }],
    about: 'На враге вспыхивает разрез крест-накрест, из него выходит светлая душа и летит к кастеру, по пути превращаясь в пучок крови',
    build: perTarget(fx => {
      const { caster, enemies } = fx.stage;
      const T = enemies[0];
      const c = T.center;
      const to = { x: caster.center.x, y: caster.center.y - 10 };
      const drips = fx.sim(DROP_FX);
      const splash = fx.sim({ texture: 'glow', drag: 0.4, gravity: 0, shrink: 0.5, tint: palette(C.blood), alpha: life => bump(life) * 0.7 });
      // Дымка души — как у «Жатвы душ»: белёсая; по пути розовеет и краснеет
      const smoke = (tint: number[], alpha: number) => ({ texture: 'smoke' as const, blend: 'normal' as const, drag: 0.3, gravity: -20, shrink: 1.4, tint: () => tint[Math.floor(Math.random() * tint.length)], alpha: (life: number) => bump(life) * alpha });
      const whiteMist = fx.sim(smoke([0xe2e8f0, 0xf1f5f9], 0.55));
      const whiteBack = fx.sim(smoke([0xe2e8f0, 0xf1f5f9], 0.55), 'back');
      const pinkMist = fx.sim(smoke([0xfecdd3, 0xfda4af, 0xfb7185], 0.45));
      const redMist = fx.sim(smoke([0xdc2626, 0xb91c1c, 0x7f1d1d], 0.5));
      const blood = fx.sim({ ...DROP, gravity: 0, drag: 0.2 });
      const glow = fx.sim({ texture: 'glow', drag: 0.3, gravity: 0, shrink: 0.5, tint: palette(['#fecaca', '#f87171', '#dc2626']), alpha: life => bump(life) * 0.8 });
      const CUT = [0.2, 0.32];
      const EMERGE = 0.45;
      const DEPART = 1.05;
      const ARRIVE = 2.5;
      fx.at(0, () => fx.stage.cast(BLOOD));
      // Повтор «Клыка кровавой луны»: над целью всходит кровавая луна
      if (fx.triggered('EchoBelowHp')) {
        const moon = fx.dot('disc', '#7f1d1d', 'back', 'normal');
        const moonGlow = fx.dot('glow', '#dc2626', 'back');
        const moonAt = { x: T.head.x + 30, y: T.head.y - 190 };
        fx.tween(0, 1.6, p => {
          const a = fade(p, 0.2, 0.3);
          const y = moonAt.y + 30 * (1 - ease.out(Math.min(p * 2, 1)));
          moonGlow.put(moonAt.x, y, 150, 0.6 * a);
          moon.put(moonAt.x, y, 46, 0.9 * a);
        }, [moon, moonGlow]);
      }
      // Разрез крест-накрест
      const cuts = [
        { a: { x: c.x - 70, y: c.y - 70 }, b: { x: c.x + 70, y: c.y + 60 }, at: CUT[0] },
        { a: { x: c.x + 70, y: c.y - 70 }, b: { x: c.x - 70, y: c.y + 60 }, at: CUT[1] },
      ];
      // Разрез: один на 1-м уровне, крест-накрест со 2-го
      for (const cut of cuts.slice(0, fx.lvl([1, 2, 2]))) {
        const dark = fx.gfx('front', 'normal');
        const bright = fx.gfx('front', 'add');
        const dir = angle(cut.a, cut.b);
        const line = (from: number, to2: number) => path(10, t => mix(cut.a, cut.b, lerp(from, to2, t)));
        fx.tween(cut.at, 0.08, p => {
          const points = line(0, ease.out(p));
          taper(dark.clear(), points, 26, '#7f1d1d', 0.9);
          taper(bright.clear(), points, 13, '#ef4444', 1);
          taper(bright, points, 4, '#fff1f2', 1);
        });
        fx.tween(cut.at + 0.08, 1.2, p => {
          const points = line(0.08, 0.92);
          taper(dark.clear(), points, 18 * (1 - p), '#7f1d1d', 0.85 * (1 - p));
          taper(bright.clear(), points, 8, '#ef4444', (1 - ease.in(p)) * (0.7 + 0.3 * Math.sin(p * 30)));
          taper(bright, points, 2.5, '#fecaca', 1 - p);
        });
        fx.at(cut.at + 0.06, () => {
          T.hit(false, BLOOD);
          fx.stage.shake(5, 0.18);
          spray(drips, { x: c.x, y: c.y, count: 22, speed: [200, 600], dir, spread: 0.5, life: [0.4, 0.9], size: [6, 11], align: true });
          spray(splash, { x: c.x, y: c.y, count: 8, speed: [60, 220], dir, spread: 0.6, life: [0.3, 0.6], size: [12, 22] });
        });
      }
      // Светлая душа выходит из разреза: дымка и круглые глаза, как у «Жатвы душ»
      const eyes = [-1, 1].map(side => ({ side, glow: fx.dot('glow', '#e0f2fe'), pupil: fx.dot('disc', '#ffffff') }));
      const drawEyes = (pt: Pt, open: number, alpha = 1) => {
        for (const eye of eyes) {
          const ex = pt.x + eye.side * 16;
          eye.glow.oval(ex, pt.y, 24, 13 * open, 0.9 * alpha);
          eye.pupil.oval(ex, pt.y, 7, 4.5 * open, alpha);
        }
      };
      const lifted = { x: c.x, y: c.y - 70 };
      fx.tween(EMERGE, DEPART - EMERGE, p => {
        const pt = mix(c, lifted, ease.out(p));
        spray(whiteBack, { x: pt.x, y: pt.y, count: fx.count(20), speed: [5, 30], life: [0.6, 1], size: [40, 62], jitter: 22 });
        spray(whiteMist, { x: pt.x, y: pt.y + 14, count: fx.count(10), speed: [5, 25], life: [0.5, 0.9], size: [30, 46], jitter: 20 });
        const blink = p > 0.6 && p < 0.68 ? 0.1 : 1;
        drawEyes(pt, fade(p, 0.3, 0) * blink);
      });
      // Полёт: душа тянется по дуге, глаза гаснут, дымка розовеет, к концу — пучок крови
      const control = { x: (lifted.x + to.x) / 2, y: Math.min(lifted.y, to.y) - 150 };
      const at = (u: number) => bez(lifted, control, to, u);
      fx.tween(DEPART, ARRIVE - DEPART, p => {
        const u = p < 0.12 ? ease.in(p / 0.12) * 0.12 : p;
        const pt = at(u);
        const blood01 = Math.min(Math.max((p - 0.3) / 0.35, 0), 1);
        drawEyes(pt, Math.max(0, 1 - p / 0.32), Math.max(0, 1 - p / 0.32));
        const stretch = Math.min(p * 2.5, 1) * 0.16;
        for (let k = 0; k < 4; k++) {
          const back = at(Math.max(0, u - stretch * (k / 3)));
          const sim = blood01 < 0.15 ? (k % 2 ? whiteBack : whiteMist) : blood01 < 0.7 ? pinkMist : redMist;
          spray(sim, { x: back.x, y: back.y, count: fx.count(14 * (1 - blood01 * 0.5)), speed: [5, 20], life: [0.35, 0.65], size: [30 - k * 4, 48 - k * 6], jitter: 10 });
        }
        if (blood01 > 0.2) {
          const next = at(Math.min(1, u + 0.02));
          const dir = Math.atan2(next.y - pt.y, next.x - pt.x);
          for (let i = fx.count(240 * blood01); i > 0; i--) {
            const speed = rand(60, 170);
            const a = dir + rand(-0.5, 0.5);
            blood.spawn({ x: pt.x + rand(-12, 12), y: pt.y + rand(-12, 12), vx: Math.cos(a) * speed, vy: Math.sin(a) * speed, lifetime: rand(0.25, 0.45), size: rand(5, 9) });
          }
          spray(glow, { x: pt.x, y: pt.y, count: fx.count(90 * blood01), speed: [0, 30], life: [0.25, 0.45], size: [14, 26], jitter: 10 });
        }
      }, eyes.flatMap(eye => [eye.glow, eye.pupil]));
      fx.at(ARRIVE, () => {
        caster.heal();
        spray(glow, { x: to.x, y: to.y, count: 20, speed: [60, 200], life: [0.3, 0.6], size: [10, 20] });
        spray(drips, { x: to.x, y: to.y, count: 14, speed: [80, 260], life: [0.4, 0.7], size: [5, 9], align: true });
      });
      pop(fx, { at: ARRIVE, dur: 0.4, x: to.x, y: to.y, r: 140, css: '#ef4444', alpha: 0.8 });
      ringWave(fx, { at: ARRIVE, dur: 0.5, x: caster.feet.x, y: caster.feet.y, from: 20, to: 130, css: '#fca5a5', squash: GROUND, layer: 'back' });
      groundGlow(fx, { at: ARRIVE - 0.2, dur: 0.9, x: caster.feet.x, y: caster.feet.y, r: 130, css: '#991b1b' });
      if (fx.since(3)) levelAccent(fx, 'blood', CUT[0] + 0.06, T);
      // Доп. эффект: из соседей тоже тянутся струйки крови к кастеру
      if (fx.extras) {
        fx.stage.bystanders.slice(0, 2).forEach((enemy, i) => {
          const start = enemy.center;
          const ctrl = { x: (start.x + to.x) / 2, y: Math.min(start.y, to.y) - 110 };
          const head = fx.dot('glow', '#f87171');
          const at = DEPART + 0.3 + i * 0.15;
          fx.at(at, () => enemy.hit(false, BLOOD));
          fly(fx, at, 1.1, p => bez(start, ctrl, to, ease.inOut(p)), pt => {
            head.put(pt.x, pt.y, 26, 0.8);
            spray(glow, { x: pt.x, y: pt.y, count: fx.count(50), speed: [0, 25], life: [0.2, 0.4], size: [8, 16], jitter: 6 });
            spray(redMist, { x: pt.x, y: pt.y, count: fx.count(10), speed: [5, 20], life: [0.3, 0.5], size: [16, 26] });
          }, [head]);
        });
      }
    }, 0.2),
  },
  {
    id: 'blood-slash', name: 'Разрез', group: 'Кровь', spell: 'Hemorrhage', duration: 3,
    levels: true,
    targets: [2, 2, 3],
    artifacts: [{
      key: 'ThousandCutsDagger', name: 'Кинжал тысячи порезов', kind: 'StackBurst',
      about: 'На цели с 5 и больше стаками кровотечения стаки лопаются — шквал порезов и урон за три тика сразу',
    }],
    about: 'Из земли поднимается дух в кровавой мгле, по бокам из неё вытягиваются когтистые руки и рвут цель крест-накрест',
    build: perTarget(fx => {
      const { enemies } = fx.stage;
      const T = enemies[0];
      const c = T.center;
      const drips = fx.sim(DROP_FX);
      const mist = fx.sim({ texture: 'glow', drag: 0.4, gravity: 0, shrink: 0.5, tint: palette(C.blood), alpha: life => bump(life) * 0.7 });
      const stain = fx.radial([[0, 0.85], [0.7, 0.65], [1, 0]], '#7f1d1d', 'back', 'normal');
      // Мгла духа — тёмно-красный дым и красные огоньки в нём
      const smoke = { texture: 'smoke' as const, blend: 'normal' as const, drag: 0.3, gravity: -20, shrink: 1.4, tint: () => 0x3b0707, alpha: (life: number) => bump(life) * 0.65 };
      const spiritMist = fx.sim(smoke);
      const spiritBack = fx.sim(smoke, 'back');
      const embers = fx.sim({ texture: 'glow', drag: 0.3, gravity: -60, shrink: 0.5, tint: palette(['#ef4444', '#f87171', '#b91c1c']), alpha: life => bump(life) * 0.9 });
      const RISE = 0.6;
      const STRIKE = [1.0, 1.2];
      const BURST = 1.75;
      const VANISH = 2.0;
      // Дух: поднимается из-под ног цели и встаёт над ней
      // Выше полоски здоровья цели, чтобы она не перекрывала глаза
      const top = { x: c.x, y: T.head.y - 125 };
      const spiritAt = (t: number): Pt => {
        if (t < RISE) return { x: c.x, y: lerp(T.feet.y, top.y, ease.out(t / RISE)) };
        return { x: c.x + Math.sin((t - RISE) * 3) * 6, y: top.y - Math.sin((t - RISE) * 4) * 5 };
      };
      fx.at(0, () => fx.stage.cast(BLOOD));
      fx.tween(0, VANISH + 0.3, p => {
        const t = p * (VANISH + 0.3);
        const power = t < 0.1 ? t / 0.1 : t > VANISH ? 1 - (t - VANISH) / 0.3 : 1;
        const s = spiritAt(t);
        spray(spiritBack, { x: s.x, y: s.y + 10, count: fx.count(12 * power), speed: [5, 35], life: [0.6, 1.1], size: [50, 80], jitter: 40 });
        spray(spiritMist, { x: s.x, y: s.y + 30, count: fx.count(5 * power), speed: [5, 30], life: [0.5, 0.9], size: [35, 55], jitter: 35 });
        spray(embers, { x: s.x, y: s.y, count: fx.count(10 * power), speed: [10, 60], life: [0.4, 0.8], size: [6, 14], jitter: 45 });
        // Хвост мглы тянется из земли, пока дух поднимается
        if (t < RISE + 0.2) spray(spiritBack, { x: c.x, y: lerp(T.feet.y, s.y, Math.random()), count: fx.count(6 * power), speed: [5, 25], life: [0.4, 0.8], size: [35, 55], jitter: 20 });
      });
      // Горящие злобные глаза духа: перед ударами щурятся сильнее
      const halo = fx.dot('glow', '#f97316');
      const eyeShape = fx.gfx('front', 'add');
      fx.tween(0.25, VANISH - 0.25, p => {
        const t = 0.25 + p * (VANISH - 0.25);
        const s = spiritAt(t);
        const angry = t > STRIKE[0] - 0.25 && t < BURST ? 0.65 : 1;
        const open = fade(p, 0.15, 0.12) * angry;
        halo.oval(s.x, s.y, 66, 28 * open, 0.45);
        eyeShape.clear();
        for (const side of [-1, 1]) angryEye(eyeShape, s.x + side * 19, s.y, side, 15, 17, open, '#f97316');
      }, [halo]);
      // Когти без кисти: три кровавые иглы веером и большая с внешней стороны (текстура как у «Кровавых игл»).
      // Раскладка в своих координатах (+x — куда смотрят иглы) переносится в (at, dir);
      // curl = ±1 — зеркалит веер, чтобы большая игла у обеих рук была сверху
      type P2 = [number, number];
      type Needle = { blade: ReturnType<typeof fx.dot>; core: ReturnType<typeof fx.dot> };
      // Основание иглы, угол, размер (r текстуры lance)
      const FINGERS: { base: P2; angle: number; r: number }[] = [
        { base: [22, -14], angle: -0.38, r: 27 },
        { base: [28, 0], angle: 0, r: 31 },
        { base: [22, 14], angle: 0.38, r: 27 },
        { base: [-2, -22], angle: -1.15, r: 21 },
      ];
      const drawClaws = (needles: Needle[], at: Pt, dir: number, s: number, alpha: number) => {
        const cos = Math.cos(dir);
        const sin = Math.sin(dir);
        const curl = cos >= 0 ? 1 : -1;
        const to = ([x, y]: P2): Pt => ({ x: at.x + (cos * x - sin * y * curl) * s, y: at.y + (sin * x + cos * y * curl) * s });
        FINGERS.forEach((f, i) => {
          const [bx, by] = f.base;
          const center = to([bx + Math.cos(f.angle) * f.r * 1.5, by + Math.sin(f.angle) * f.r * 1.5]);
          const rotation = dir + f.angle * curl;
          needles[i].blade.put(center.x, center.y, f.r * s, alpha, rotation);
          needles[i].core.put(center.x, center.y, f.r * 0.57 * s, alpha, rotation);
        });
      };
      const hands = [
        { side: -1, a: { x: c.x - 120, y: c.y - 100 }, b: { x: c.x + 110, y: c.y + 90 }, at: STRIKE[0] },
        { side: 1, a: { x: c.x + 120, y: c.y - 100 }, b: { x: c.x - 110, y: c.y + 90 }, at: STRIKE[1] },
      ];
      // Рук: одна на 1-м уровне, две со 2-го
      for (const hand of hands.slice(0, fx.lvl([1, 2, 2]))) {
        const dir = angle(hand.a, hand.b);
        const rest = { x: c.x + hand.side * 170, y: c.y - 60 };
        const cocked = { x: hand.a.x + hand.side * 50, y: hand.a.y - 50 };
        const restDir = angle(rest, c);
        const S = 1.05;
        const EMERGE = 0.55;
        // Запястье (где кисть растворяется в дыму): назад от ладони по её направлению
        const elbow = (at: Pt, d: number) => polar(at, 40 * S, d + Math.PI);
        // Мягкий ореол под когтями
        const halo = fx.dot('glow', '#dc2626');
        const needles: Needle[] = FINGERS.map(() => ({ blade: fx.dot('lance', BLOOD), core: fx.dot('lance', '#fecaca') }));
        const draw = (at: Pt, d: number, scale: number, a: number) => {
          drawClaws(needles, at, d, scale, a);
          halo.put(at.x + Math.cos(d) * 50 * scale, at.y + Math.sin(d) * 50 * scale, 100 * scale, 0.35 * a);
        };
        // Проступает из клуба красного дыма сбоку от цели
        fx.at(EMERGE, () => {
          spray(spiritMist, { x: rest.x, y: rest.y, count: 10, speed: [20, 90], life: [0.5, 0.9], size: [40, 70], jitter: 30 });
          spray(embers, { x: rest.x, y: rest.y, count: 14, speed: [40, 160], life: [0.3, 0.7], size: [5, 12] });
        });
        fx.tween(EMERGE, 0.3, p => {
          const at = { x: rest.x, y: rest.y + 20 * (1 - ease.out(p)) };
          draw(at, restDir, S * (0.6 + 0.4 * ease.out(p)), ease.out(p));
        });
        // Замах: кисть отводится вверх и наружу, когти разворачиваются к линии удара
        fx.tween(EMERGE + 0.3, hand.at - EMERGE - 0.3, p => {
          const d = lerp(restDir, dir, ease.inOut(p));
          const at = mix(rest, cocked, ease.inOut(p));
          draw(at, d, S, 1);
          const e = elbow(at, d);
          spray(spiritMist, { x: e.x, y: e.y, count: fx.count(6), speed: [5, 30], life: [0.4, 0.7], size: [25, 45], jitter: 12 });
        });
        // Удар наискосок через цель
        fx.tween(hand.at, 0.1, p => {
          const at = mix(cocked, hand.b, ease.out(p));
          draw(at, dir, S * 1.08, 1);
          const e = elbow(at, dir);
          spray(spiritMist, { x: e.x, y: e.y, count: fx.count(10), speed: [5, 30], life: [0.3, 0.6], size: [25, 45], jitter: 12 });
        });
        // Чуть проезжает дальше и рассыпается в дым
        fx.tween(hand.at + 0.1, 0.35, p => {
          const at = polar(hand.b, 30 * ease.out(p), dir);
          draw(at, dir, S * (1.08 - 0.25 * p), 1 - ease.in(p));
        }, [halo, ...needles.flatMap(n => [n.blade, n.core])]);
        fx.at(hand.at + 0.25, () => {
          const at = polar(hand.b, 20, dir);
          spray(spiritMist, { x: at.x, y: at.y, count: 12, speed: [20, 90], life: [0.5, 0.9], size: [40, 70], jitter: 40 });
          spray(embers, { x: at.x, y: at.y, count: 16, speed: [40, 180], life: [0.3, 0.7], size: [5, 12], jitter: 30 });
        });
        // Три следа когтей, раны держатся до прорыва
        const nx = Math.cos(dir + Math.PI / 2);
        const ny = Math.sin(dir + Math.PI / 2);
        // Следов когтей от каждой руки: 2 / 2 / 3 по уровню
        for (const k of fx.lvl([[-0.5, 0.5], [-0.5, 0.5], [-1, 0, 1]])) {
          const a = { x: hand.a.x + nx * k * 20, y: hand.a.y + ny * k * 20 };
          const b = { x: hand.b.x + nx * k * 20, y: hand.b.y + ny * k * 20 };
          const dark = fx.gfx('front', 'normal');
          const bright = fx.gfx('front', 'add');
          const line = (from: number, to: number) => path(10, t => mix(a, b, lerp(from, to, t)));
          fx.tween(hand.at, 0.09, p => {
            const points = line(0, ease.out(p));
            taper(dark.clear(), points, 20, '#7f1d1d', 0.9);
            taper(bright.clear(), points, 10, '#ef4444', 1);
            taper(bright, points, 3.5, '#fff1f2', 1);
          });
          fx.tween(hand.at + 0.09, BURST + 0.6 - hand.at - 0.09, p => {
            const swell = p < 0.75 ? 1 + 0.4 * (p / 0.75) : 1.4 * (1 - (p - 0.75) / 0.25);
            const points = line(0.1, 0.9);
            taper(dark.clear(), points, 13 * swell, '#7f1d1d', 0.85);
            taper(bright.clear(), points, 6 * swell, '#ef4444', 0.6 + 0.4 * Math.sin(p * 30 + k));
            taper(bright, points, 2 * swell, '#fecaca', 1);
          });
        }
        fx.at(hand.at + 0.07, () => {
          T.hit(false, BLOOD);
          fx.stage.shake(6, 0.2);
          spray(drips, { x: c.x, y: c.y, count: 34, speed: [220, 700], dir, spread: 0.45, life: [0.4, 0.9], size: [7, 12], align: true });
          spray(mist, { x: c.x, y: c.y, count: 10, speed: [60, 240], dir, spread: 0.6, life: [0.3, 0.6], size: [14, 26] });
        });
      }
      // Капает с ран
      fx.tween(STRIKE[1] + 0.1, BURST - STRIKE[1] - 0.1, () => spray(drips, { x: c.x + rand(-60, 60), y: c.y + rand(-50, 50), count: fx.count(18), speed: [0, 40], dir: Math.PI / 2, spread: 0.3, life: [0.4, 0.7], size: [5, 8], align: true }));
      fx.at(BURST, () => {
        T.hit(true, BLOOD);
        fx.stage.shake(10, 0.3);
        fx.stage.flash('#7f1d1d', 0.25, 0.2);
        spray(drips, { x: c.x, y: c.y, count: 60, speed: [200, 720], life: [0.5, 1], size: [7, 13], align: true });
        spray(mist, { x: c.x, y: c.y, count: 18, speed: [80, 300], life: [0.4, 0.8], size: [20, 36] });
      });
      pop(fx, { at: BURST, dur: 0.4, x: c.x, y: c.y, r: fx.lvl([150, 180, 210]), css: '#ef4444' });
      if (fx.since(3)) levelAccent(fx, 'blood', BURST, T);
      if (fx.has('StackBurst') && T.proc) thousandCuts(fx, BURST + 0.35, T);
      ringWave(fx, { at: BURST, dur: 0.5, x: c.x, y: c.y, from: 20, to: 200, css: '#fecaca' });
      groundGlow(fx, { at: 0, dur: VANISH + 0.3, x: T.feet.x, y: T.feet.y, r: 150, css: '#7f1d1d' });
      fx.tween(STRIKE[0], 2, p => {
        const r = 130 * ease.out(Math.min(p * 1.5, 1));
        stain.oval(T.feet.x, T.feet.y, r, r * GROUND, fade(p, 0.1, 0.3) * 0.85);
      }, [stain]);
    }, 0.25),
  },
  {
    id: 'blood-needles', name: 'Кровавые иглы', group: 'Кровь', spell: 'Hemorrhage', duration: 2.3,
    levels: true,
    about: 'Вокруг цели по кругу загораются иглы и разом вонзаются в неё',
    build(fx) {
      const { enemies } = fx.stage;
      const T = enemies[0];
      const c = T.center;
      const drops = fx.sim(DROP);
      const STAB = 1.05;
      // Игл: 6 / 8 / 12 по уровню
      const COUNT = fx.lvl([6, 8, 12]);
      fx.at(0, () => fx.stage.cast(BLOOD));
      for (let i = 0; i < COUNT; i++) {
        const a = (i / COUNT) * TAU - Math.PI / 2;
        const at = 0.2 + i * 0.05;
        const needle = fx.dot('lance', BLOOD);
        const core = fx.dot('lance', '#fecaca');
        const spark = fx.dot('star', '#ffffff');
        const place = (r: number, alpha: number, size: number) => {
          const pt = polar(c, r, a, 0.85);
          needle.put(pt.x, pt.y, 30 * size, alpha, a + Math.PI);
          core.put(pt.x, pt.y, 17 * size, alpha, a + Math.PI);
        };
        fx.tween(at, 0.2, p => {
          place(210, p, ease.back(p));
          spark.put(c.x + Math.cos(a) * 210, c.y + Math.sin(a) * 210 * 0.85, 18 * bump(p), bump(p), p * 3);
        }, [spark]);
        fx.tween(at + 0.2, STAB - at - 0.2, p => place(210 + 14 * ease.out(p), 1, 1));
        fx.tween(STAB, 0.1, p => place(lerp(224, 30, ease.in(p)), 1, 1), [needle, core]);
      }
      fx.at(STAB + 0.1, () => {
        T.hit(true, BLOOD);
        fx.stage.shake(10, 0.3);
        spray(drops, { x: c.x, y: c.y, count: 60, speed: [200, 720], life: [0.4, 0.9], size: [3, 8] });
      });
      pop(fx, { at: STAB + 0.1, dur: 0.4, x: c.x, y: c.y, r: 210, css: '#ef4444' });
      ringWave(fx, { at: STAB + 0.1, dur: 0.5, x: c.x, y: c.y, from: 20, to: 200, css: '#fecaca' });
      ringWave(fx, { at: 0.2, dur: 0.8, x: T.feet.x, y: T.feet.y, from: 230, to: 60, css: '#b91c1c', squash: GROUND, layer: 'back', alpha: 0.6 });
      if (fx.since(3)) levelAccent(fx, 'blood', STAB + 0.1, T);
    },
  },
  {
    id: 'blood-price', name: 'Кровавая жертва', group: 'Кровь', spell: 'Blood Price', duration: 3.2,
    levels: true,
    targets: ['all', 'all', 'all'],
    artifacts: [{
      key: 'SacrificialAltar', name: 'Жертвенный алтарь', kind: 'PaidHpHeal', heals: 'all',
      about: 'Заплаченное здоровье не пропадает: часть комет летит к союзникам и исцеляет их',
    }],
    about: 'Под кастером вспыхивает кровавая печать, из его ран к небу тянутся струи крови и собираются в бьющуюся, как сердце, сферу; она лопается, и кровавые кометы бьют всех врагов',
    build(fx) {
      const { caster, enemies } = fx.stage;
      const orbAt = { x: caster.head.x, y: caster.head.y - 150 };
      const drips = fx.sim(DROP_FX);
      const sparks = fx.sim(P.spark(C.blood, 300));
      const trail = fx.sim({ texture: 'glow', drag: 0.3, shrink: 0.4, tint: palette(C.blood), alpha: life => bump(life) * 0.8 });
      const mist = fx.sim({ texture: 'smoke', blend: 'normal', drag: 0.4, gravity: -30, shrink: 1.4, tint: palette(['#7f1d1d', '#991b1b', '#450a0a']), alpha: life => bump(life) * 0.45 });
      const pluses = fx.sim({ texture: 'plus', drag: 0.8, gravity: -130, shrink: 0.5, tint: palette(['#fecaca', '#f87171', '#ef4444']), alpha: life => bump(life) });
      const rain = fx.sim({ ...DROP_FX, gravity: 1300 });
      const FEED = 0.35;
      // Ударов сердца: 0 / 1 / 2 по уровню
      const BEATS = fx.lvl([[], [1.3], [1.05, 1.5]]);
      const K = fx.lvl([0.72, 0.86, 1]);
      const BURST = 1.95;
      fx.at(0, () => fx.stage.cast(BLOOD));
      // Кровавая печать под кастером: кольца, руны-насечки, звезда; вращается
      const seal = fx.gfx('back', 'add');
      // Печать — со 2-го уровня
      if (fx.since(2)) fx.tween(0.05, BURST + 0.4, p => {
        const t = 0.05 + p * (BURST + 0.4);
        const r = 130 * ease.back(Math.min((t - 0.05) / 0.35, 1));
        const a = (t > BURST ? 1 - (t - BURST) / 0.45 : 1) * (0.75 + 0.25 * Math.sin(t * 12));
        const { x, y } = caster.feet;
        seal.clear();
        if (r < 2) return;
        const pt = (ang: number, k: number) => ({ x: x + Math.cos(ang) * r * k, y: y + Math.sin(ang) * r * k * GROUND });
        seal.ellipse(x, y, r, r * GROUND).stroke({ width: 5, color: color('#ef4444'), alpha: a });
        seal.ellipse(x, y, r, r * GROUND).stroke({ width: 12, color: color('#dc2626'), alpha: 0.3 * a });
        seal.ellipse(x, y, r * 0.8, r * 0.8 * GROUND).stroke({ width: 1.5, color: color('#fca5a5'), alpha: 0.6 * a });
        const star = Array.from({ length: 5 }, (_, i) => pt((i * 2 / 5) * TAU - t * 0.8, 0.8));
        star.forEach((p1, i) => {
          const p2 = star[(i + 1) % 5];
          seal.moveTo(p1.x, p1.y).lineTo(p2.x, p2.y).stroke({ width: 3, color: color('#f87171'), alpha: 0.9 * a });
        });
        for (let i = 0; i < 20; i++) {
          const ang = (i / 20) * TAU + t * 0.5;
          const o1 = pt(ang, 0.84);
          const o2 = pt(ang, i % 2 ? 0.93 : 0.98);
          seal.moveTo(o1.x, o1.y).lineTo(o2.x, o2.y).stroke({ width: 2, color: color('#fecaca'), alpha: 0.7 * a });
        }
      });
      groundGlow(fx, { at: 0.05, dur: BURST + 0.4, x: caster.feet.x, y: caster.feet.y, r: 160, css: '#991b1b' });
      // Раны на кастере
      fx.at(0.2, () => {
        caster.hit(false, BLOOD);
        fx.stage.shake(4, 0.2);
        spray(drips, { x: caster.center.x, y: caster.center.y, count: 18, speed: [120, 380], life: [0.4, 0.8], size: [6, 10], align: true });
      });
      fx.tween(0.25, BURST - 0.25, () => spray(drips, { x: caster.center.x + rand(-25, 25), y: caster.center.y + rand(-30, 30), count: fx.count(14), speed: [0, 40], dir: Math.PI / 2, spread: 0.3, life: [0.4, 0.7], size: [5, 8], align: true }));
      // Струи крови из тела к сфере: извиваются, по ним бегут капли
      const streams = Array.from({ length: fx.lvl([2, 3, 4]) }, (_, i) => ({ beam: new Beam(fx, 14, 12, '#b91c1c', 'front', '#fca5a5'), from: { x: caster.center.x + (i - 1.5) * 18, y: caster.center.y + rand(-20, 30) }, phase: rand(0, TAU), side: i % 2 ? 1 : -1 }));
      fx.tween(FEED, BURST - FEED, p => {
        const t = FEED + p * (BURST - FEED);
        const a = fade(p, 0.12, 0.1);
        for (const st of streams) {
          const reach = ease.out(Math.min(p * 3, 1));
          const pts = path(14, k => {
            const q = k * reach;
            const base = mix(st.from, orbAt, q);
            return { x: base.x + Math.sin(q * 4 + t * 6 + st.phase) * 18 * st.side * Math.sin(q * Math.PI), y: base.y };
          });
          st.beam.set(pts, a * (0.75 + 0.25 * Math.sin(t * 20 + st.phase)));
          if (Math.random() < 0.5) {
            const k = Math.random() * reach;
            const pt = pts[Math.floor(k * 13)];
            spray(trail, { x: pt.x, y: pt.y, count: 1, speed: [5, 20], life: [0.2, 0.4], size: [6, 12] });
          }
          if (p >= 1) st.beam.hide();
        }
      });
      // Сфера: тёмная жидкая кровь, вихрь бликов внутри, яркий обод, капли по орбите; бьётся, как сердце
      const halo = fx.dot('glow', '#dc2626');
      const rim = fx.dot('ring', '#f87171');
      const orb = fx.dot('disc', '#450a0a', 'front', 'normal');
      const inner = fx.dot('glow', '#991b1b');
      const swirl = Array.from({ length: 3 }, () => fx.dot('glow', '#fca5a5'));
      const shine = fx.dot('glow', '#fff1f2');
      const orbit = Array.from({ length: 6 }, () => fx.dot('disc', '#dc2626', 'front', 'normal'));
      const beat = (t: number) => BEATS.reduce((acc, b) => acc + Math.max(0, 1 - Math.abs(t - b) / 0.12) * 0.22, 0);
      fx.tween(FEED, BURST - FEED, p => {
        const t = FEED + p * (BURST - FEED);
        const r = 46 * K * ease.out(Math.min(p * 1.6, 1)) * (1 + beat(t)) * (1 + Math.sin(t * 30) * 0.02);
        halo.put(orbAt.x, orbAt.y, r * 3.2, 0.7);
        rim.put(orbAt.x, orbAt.y, r * 1.08, 0.9);
        orb.put(orbAt.x, orbAt.y, r);
        inner.put(orbAt.x, orbAt.y, r * 0.9, 0.8);
        swirl.forEach((d, i) => {
          const ang = t * 4 + (i / 3) * TAU;
          d.put(orbAt.x + Math.cos(ang) * r * 0.45, orbAt.y + Math.sin(ang) * r * 0.3, r * 0.35, 0.55);
        });
        shine.put(orbAt.x - r * 0.35, orbAt.y - r * 0.4, r * 0.38, 0.9);
        orbit.forEach((d, i) => {
          const ang = t * 3 + (i / orbit.length) * TAU;
          d.put(orbAt.x + Math.cos(ang) * r * 1.6, orbAt.y + Math.sin(ang) * r * 0.5, 5 + 2 * Math.sin(t * 8 + i));
        });
        spray(mist, { x: orbAt.x, y: orbAt.y, count: fx.count(10), speed: [10, 40], life: [0.5, 0.9], size: [30, 50], jitter: r });
      }, [halo, rim, orb, inner, ...swirl, shine, ...orbit]);
      // Удары сердца: кольцо и тёмно-красная вспышка
      BEATS.forEach(t => {
        fx.at(t, () => {
          fx.stage.flash('#7f1d1d', 0.15, 0.2);
          fx.stage.shake(3, 0.12);
        });
        ringWave(fx, { at: t, dur: 0.45, x: orbAt.x, y: orbAt.y, from: 50, to: 170, css: '#f87171', alpha: 0.7 });
      });
      // Сфера лопается: волна, кровавый дождь, кометы к союзникам
      fx.at(BURST, () => {
        fx.stage.flash('#991b1b', 0.35, 0.3);
        fx.stage.shake(8, 0.3);
        spray(sparks, { x: orbAt.x, y: orbAt.y, count: 50, speed: [150, 560], life: [0.3, 0.8], size: [3, 8] });
        spray(mist, { x: orbAt.x, y: orbAt.y, count: 14, speed: [40, 180], life: [0.7, 1.2], size: [40, 70] });
        for (let i = 0; i < 40; i++) rain.spawn({ x: orbAt.x + rand(-60, 60), y: orbAt.y + rand(-20, 20), vx: rand(-260, 260), vy: rand(-420, -120), lifetime: rand(0.6, 1.1), size: rand(6, 11) });
      });
      pop(fx, { at: BURST, dur: 0.45, x: orbAt.x, y: orbAt.y, r: 230 * K, css: '#ef4444' });
      ringWave(fx, { at: BURST, dur: 0.6, x: orbAt.x, y: orbAt.y, from: 40, to: 320, css: '#fecaca' });
      // Кометы: в каждого врага (урон), с «Жертвенным алтарём» — ещё и к союзникам (лечение)
      const comets = [
        ...enemies.map(unit => ({ unit, heal: false })),
        ...fx.stage.healed.filter(unit => unit !== caster).map(unit => ({ unit, heal: true })),
      ];
      comets.forEach(({ unit: ally, heal }, i) => {
        const at = BURST + i * 0.06;
        const control = { x: (orbAt.x + ally.center.x) / 2, y: orbAt.y - 120 };
        const bolt = fx.dot('glow', '#f87171');
        const core = fx.dot('glow', '#fecaca');
        const body = fx.dot('disc', '#991b1b', 'front', 'normal');
        fly(fx, at, 0.5, p => bez(orbAt, control, ally.center, ease.inOut(p)), pt => {
          bolt.put(pt.x, pt.y, 44);
          body.put(pt.x, pt.y, 11);
          core.put(pt.x, pt.y, 12);
          spray(trail, { x: pt.x, y: pt.y, count: fx.count(110), speed: [5, 40], life: [0.3, 0.6], size: [8, 18] });
          if (Math.random() < 0.5) spray(drips, { x: pt.x, y: pt.y, count: 1, speed: [10, 60], life: [0.3, 0.6], size: [4, 7], align: true });
        }, [bolt, core, body]);
        if (!heal && fx.since(3)) levelAccent(fx, 'blood', at + 0.5, ally);
        fx.at(at + 0.5, () => {
          if (heal) ally.heal();
          else ally.hit(false, BLOOD);
          if (heal) spray(pluses, { x: ally.center.x, y: ally.center.y, count: 8, speed: [40, 130], dir: -Math.PI / 2, spread: 1, life: [0.6, 1], size: [8, 13], jitter: 26 });
          spray(sparks, { x: ally.center.x, y: ally.center.y, count: 14, speed: [80, 260], dir: -Math.PI / 2, spread: 1.2, life: [0.4, 0.8], size: [3, 6] });
        });
        pop(fx, { at: at + 0.5, dur: 0.4, x: ally.center.x, y: ally.center.y, r: 130, css: '#f87171', alpha: 0.8 });
        ringWave(fx, { at: at + 0.5, dur: 0.6, x: ally.feet.x, y: ally.feet.y, from: 30, to: 150, css: '#f87171', squash: GROUND, layer: 'back' });
        groundGlow(fx, { at: at + 0.5, dur: 0.9, x: ally.feet.x, y: ally.feet.y, r: 110, css: '#b91c1c' });
      });
    },
  },
];
