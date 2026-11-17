import {
  Beam, C, GROUND, P, TAU, bump, clamp01, color, crackle, ease, jag, lerp, palette, polar, pop, rand, ringWave,
  type ConceptUnit, type Fx, type ParticleSim, type Pt,
} from './kit';
import { bodyOf, type Look, type LookRun } from './looks';
import { bubbleTexture, dropTexture, iceFangTexture, leafTexture, snowflakeTexture } from './sprites';

// Облики аур: что висит на носителе весь бой (раньше — купол aura-sphere на канвасе) и как он вздрагивает, когда
// аура срабатывает в начале раунда (сама волна к целям — концепты из auras.ts, страница проигрывает их вместе).
// У каждой стихии три варианта: свой купол и два других силуэта. Сила растёт с уровнем ауры

// Купол: прозрачный в центре, к краю ярче (как обод старой сферы), дымка внутри и блик сверху-слева.
// Дымка — под юнитом, обод и блик — над ним, чтобы носитель был «внутри»
const dome = (fx: Fx, css: string, accent: string, haze = css) => {
  const inner = fx.radial([[0, 0.08], [0.6, 0.12], [0.92, 0.3], [1, 0]], haze, 'back');
  const rim = fx.radial([[0, 0], [0.62, 0], [0.86, 0.45], [0.96, 0.2], [1, 0]], css);
  const edge = fx.radial([[0, 0], [0.86, 0], [0.96, 1], [1, 0]], accent);
  const shine = fx.radial([[0, 0.4], [1, 0]], accent);
  return (x: number, y: number, rx: number, ry: number, a: number) => {
    inner.oval(x, y, rx, ry, a);
    rim.oval(x, y, rx, ry, a * 0.8);
    edge.oval(x, y, rx, ry, a * 0.45);
    shine.oval(x - rx * 0.4, y - ry * 0.55, rx * 0.32, ry * 0.1, a * 0.7, -0.6);
  };
};

// Круг на земле под носителем
const floor = (fx: Fx, css: string) => {
  const glow = fx.radial([[0, 0.55], [0.6, 0.28], [1, 0]], css, 'back');
  return (u: ConceptUnit, r: number, a: number) => glow.oval(u.feet.x, u.feet.y, r, r * GROUND, a);
};

// Точка на ободе купола под углом ang (0 — справа, π/2 — снизу)
const onRim = (cx: number, cy: number, rx: number, ry: number, ang: number): Pt => ({ x: cx + Math.cos(ang) * rx, y: cy + Math.sin(ang) * ry });

// Размер купола вокруг юнита
const domeSize = (u: ConceptUnit, k = 1) => ({ rx: 82 * k, ry: bodyOf(u) * 0.72 * k, cy: u.center.y + 4 });

// Дыхание: медленная пульсация 1 ± amp
const breath = (time: number, period: number, amp = 0.035) => 1 + Math.sin((time / period) * TAU) * amp;

// Общая подпись аур в списке
const aura = (look: Omit<Look, 'kind' | 'levels'>): Look => ({ ...look, kind: 'aura', levels: true });

const FLAME = { texture: 'smoke' as const, drag: 0.2, gravity: -380, shrink: 0.2, stretch: [0.7, 2] as [number, number], tint: palette(C.fire.slice(1)), alpha: (life: number) => bump(life) * 0.8 };

export const AURA_LOOKS: Look[] = [
  // ===== Горение =====
  aura({
    id: 'al-burn-dome', name: 'Купол жара', group: 'Аура · Горение', spell: 'Burn',
    about: 'Оранжевый купол дрожит от жара, по его краю лижут язычки пламени и скользят угли, внутри поднимаются угольки; при срабатывании купол вспыхивает',
    build(fx, u): LookRun {
      const set = dome(fx, '#ff7a1a', '#fff1a8', '#ff9a3c');
      const ground = floor(fx, '#ff6a00');
      const embers = fx.sim(P.ember(C.fire, 90));
      const tongues = fx.sim({ ...FLAME, gravity: -260 });
      const { rx, ry, cy } = domeSize(u, fx.lvl([0.9, 0.95, 1]));
      return {
        frame(time, a, surge) {
          const s = breath(time, 1.6) * (1 + surge * 0.12);
          set(u.center.x, cy, rx * s, ry * s, a * (0.55 + 0.45 * surge) * (0.9 + 0.1 * Math.sin(time * 23)));
          ground(u, rx * 1.2, a * (0.7 + 0.3 * surge));
          // Язычки по верхней половине обода — купол будто горит по краю
          for (let n = fx.count(fx.lvl([30, 45, 60]) * a * (1 + surge * 2)); n > 0; n--) {
            const pt = onRim(u.center.x, cy, rx * s, ry * s, rand(Math.PI * 1.05, Math.PI * 1.95));
            tongues.spawn({ x: pt.x, y: pt.y, vx: rand(-10, 10), vy: rand(-60, -20), lifetime: rand(0.25, 0.45), size: rand(4, 8) });
          }
          for (let n = fx.count(fx.lvl([10, 16, 24]) * a * (1 + surge * 3)); n > 0; n--) {
            const pt = polar(u.feet, rand(0, rx * 0.8), rand(0, TAU), GROUND);
            embers.spawn({ x: pt.x, y: pt.y - 4, vx: rand(-10, 10), vy: rand(-110, -50), lifetime: rand(0.7, 1.2), size: rand(2.5, 5) });
          }
        },
        trigger() {
          ringWave(fx, { at: fx.now, dur: 0.45, x: u.center.x, y: cy, from: rx * 0.9, to: rx * 1.5, css: '#ffb347', squash: ry / rx });
        },
      };
    },
  }),
  aura({
    id: 'al-burn-mantle', name: 'Огненная мантия', group: 'Аура · Горение', spell: 'Burn',
    about: 'Без купола: по контуру носителя лениво лижут языки пламени, у ног — обугленный круг с огненными трещинами; при срабатывании пламя взмывает',
    build(fx, u) {
      const h = bodyOf(u);
      const flames = fx.sim(FLAME, 'back');
      const front = fx.sim(FLAME);
      const ground = floor(fx, '#c2410c');
      const cracks = fx.gfx('back', 'add');
      const lines = Array.from({ length: 7 }, (_, k) => jag(u.feet, polar(u.feet, rand(55, 85), (k / 7) * TAU + rand(-0.2, 0.2), GROUND), 5, 6));
      return {
        frame(time, a, surge) {
          ground(u, 80, a * 0.6);
          cracks.clear();
          for (const line of lines) {
            cracks.moveTo(line[0].x, line[0].y);
            line.slice(1).forEach(pt => cracks.lineTo(pt.x, pt.y));
          }
          cracks.stroke({ width: 2, color: color('#ffb347'), alpha: a * (0.45 + 0.2 * Math.sin(time * 3) + 0.35 * surge) });
          const rate = fx.lvl([40, 60, 85]) * a * (1 + surge * 3);
          for (let n = fx.count(rate); n > 0; n--) {
            const side = Math.random() < 0.5 ? -1 : 1;
            const y = rand(u.head.y + h * 0.2, u.feet.y);
            const x = u.center.x + side * rand(30, 44);
            (Math.random() < 0.5 ? flames : front).spawn({ x, y, vx: -side * rand(0, 15), vy: rand(-120, -60) * (1 + surge), lifetime: rand(0.3, 0.55), size: rand(5, 10) * (1 + surge * 0.5) });
          }
        },
      };
    },
  }),
  aura({
    id: 'al-burn-orbit', name: 'Кружащие огоньки', group: 'Аура · Горение', spell: 'Burn',
    about: 'Вокруг пояса носителя кружат огоньки со шлейфами (заходят за спину); при срабатывании они разгоняются и разгораются',
    build(fx, u) {
      const trail = fx.sim(P.ember(C.fire, 40));
      const backTrail = fx.sim(P.ember(C.fire, 40), 'back');
      const count = fx.lvl([2, 3, 4]);
      const orbs = Array.from({ length: count }, (_, k) => ({
        front: [fx.dot('glow', '#ff7a1a'), fx.dot('glow', '#fff1a8')],
        back: [fx.dot('glow', '#ff7a1a', 'back'), fx.dot('glow', '#fff1a8', 'back')],
        phase: (k / count) * TAU,
      }));
      const c = { x: u.center.x, y: u.center.y + 10 };
      let angle = 0;
      return {
        frame(_time, a, surge) {
          angle += fx.dt * (2.2 + surge * 5);
          for (const orb of orbs) {
            const ang = angle + orb.phase;
            const pt = polar(c, 62, ang, 0.32);
            const behind = Math.sin(ang) < 0;
            const [show, hide] = behind ? [orb.back, orb.front] : [orb.front, orb.back];
            hide.forEach(d => d.hide());
            const r = 11 * (1 + surge * 0.6);
            show[0].put(pt.x, pt.y, r * 2.4, a * 0.8);
            show[1].put(pt.x, pt.y, r, a);
            for (let n = fx.count(50 * a); n > 0; n--) (behind ? backTrail : trail).spawn({ x: pt.x, y: pt.y, vx: rand(-8, 8), vy: rand(-20, 0), lifetime: rand(0.25, 0.45), size: rand(3, 6) });
          }
        },
      };
    },
  }),

  // ===== Мороз =====
  aura({
    id: 'al-frost-dome', name: 'Ледяная сфера', group: 'Аура · Мороз', spell: 'Frost',
    about: 'Бледно-голубой купол, внутри медленно падают снежинки, на земле — круг инея с ветвистыми узорами; при срабатывании по сфере пробегает иней',
    build(fx, u) {
      const set = dome(fx, '#38bdf8', '#e0f6ff', '#7dd3fc');
      const ground = floor(fx, '#7dd3fc');
      const rime = fx.gfx('back', 'add');
      const flakes = fx.sim({ texture: snowflakeTexture(), drag: 0.8, gravity: 30, shrink: 0.9, tint: palette(['#ffffff', '#e0f6ff']), alpha: life => bump(life) });
      const { rx, ry, cy } = domeSize(u, fx.lvl([0.9, 0.95, 1]));
      const branches = Array.from({ length: 10 }, (_, k) => ({ a: (k / 10) * TAU + rand(-0.15, 0.15), len: rand(0.8, 1.1) * rx }));
      return {
        frame(time, a, surge) {
          const s = breath(time, 2.6);
          set(u.center.x, cy, rx * s, ry * s, a * (0.55 + 0.4 * surge));
          ground(u, rx * 1.15, a * 0.7);
          rime.clear();
          for (const b of branches) {
            const end = polar(u.feet, b.len, b.a, GROUND);
            const mid = polar(u.feet, b.len * 0.6, b.a, GROUND);
            rime.moveTo(u.feet.x, u.feet.y).lineTo(end.x, end.y);
            for (const side of [-1, 1]) {
              const twig = polar(mid, b.len * 0.22, b.a + side * 0.7, GROUND);
              rime.moveTo(mid.x, mid.y).lineTo(twig.x, twig.y);
            }
          }
          rime.stroke({ width: 1.6, color: color('#e0f6ff'), alpha: a * (0.55 + 0.4 * surge) });
          for (let n = fx.count(fx.lvl([4, 6, 9]) * a); n > 0; n--) flakes.spawn({ x: u.center.x + rand(-rx * 0.7, rx * 0.7), y: cy - ry * 0.8, vx: rand(-10, 10), vy: rand(25, 50), lifetime: rand(1.6, 2.2), size: rand(3.5, 6), rotation: rand(0, TAU), spin: rand(-2, 2) });
        },
        trigger() {
          pop(fx, { at: fx.now, dur: 0.4, x: u.center.x, y: cy, r: rx * 1.1, css: '#bae6fd', alpha: 0.5 });
        },
      };
    },
  }),
  aura({
    id: 'al-frost-crystals', name: 'Кристальный венец', group: 'Аура · Мороз', spell: 'Frost',
    about: 'Вокруг ног носителя кольцом стоят ледяные кристаллы, по ним бегут блики, от земли поднимается холодный пар; при срабатывании кристаллы вспыхивают',
    build(fx, u) {
      const count = fx.lvl([5, 7, 9]);
      const mist = fx.sim({ texture: 'smoke', blend: 'normal', drag: 0.5, gravity: -14, shrink: 1.4, tint: palette(['#f0f9ff', '#e0f2fe']), alpha: life => bump(life) * 0.3 });
      const glints = fx.sim(P.star(['#ffffff', '#e0f6ff'], 0));
      const crystals = Array.from({ length: count }, (_, k) => {
        const ang = (k / count) * TAU + 0.2;
        const base = polar(u.feet, 64, ang, GROUND);
        const behind = Math.sin(ang) < 0;
        const height = rand(34, 52) * (behind ? 1.1 : 0.9);
        const body = fx.spike(iceFangTexture((k % 3) as 0 | 1 | 2), base.x, base.y, height, behind ? 'back' : 'front');
        const glow = fx.spike(iceFangTexture((k % 3) as 0 | 1 | 2), base.x, base.y, height, behind ? 'back' : 'front', 'add');
        body.sprite.rotation = glow.sprite.rotation = Math.cos(ang) * 0.35;
        return { body, glow, base, height, phase: rand(0, TAU) };
      });
      const ground = floor(fx, '#7dd3fc');
      return {
        frame(time, a, surge) {
          ground(u, 80, a * 0.6);
          for (const c of crystals) {
            const grow = ease.out3(clamp01(time / 0.35));
            c.body.sprite.scale.set(c.body.k, c.body.k * grow);
            c.glow.sprite.scale.set(c.glow.k, c.glow.k * grow);
            c.body.sprite.alpha = 0.85 * a;
            c.glow.sprite.alpha = a * (0.15 + 0.15 * Math.max(0, Math.sin(time * 2 + c.phase)) + 0.6 * surge);
          }
          if (Math.random() < fx.count(3 * a)) {
            const c = crystals[Math.floor(Math.random() * crystals.length)];
            glints.spawn({ x: c.base.x, y: c.base.y - c.height * rand(0.4, 0.9), vx: 0, vy: 0, lifetime: 0.35, size: rand(5, 8) });
          }
          for (let n = fx.count(8 * a); n > 0; n--) {
            const pt = polar(u.feet, rand(30, 70), rand(0, TAU), GROUND);
            mist.spawn({ x: pt.x, y: pt.y, vx: rand(-10, 10), vy: rand(-10, 0), lifetime: rand(1, 1.5), size: rand(14, 22) });
          }
        },
      };
    },
  }),
  aura({
    id: 'al-frost-swirl', name: 'Снежный вихрь', group: 'Аура · Мороз', spell: 'Frost',
    about: 'Вокруг носителя столбом вьются снежинки — спереди и за спиной; при срабатывании вихрь ускоряется и расширяется',
    build(fx, u) {
      const h = bodyOf(u);
      const flake = snowflakeTexture();
      const count = fx.lvl([8, 11, 14]);
      const list = Array.from({ length: count }, (_, k) => ({ front: fx.tex(flake, 'front', 'add', '#e0f6ff'), back: fx.tex(flake, 'back', 'add', '#bae6fd'), phase: rand(0, TAU), y: k / count, r: rand(0.85, 1.15) }));
      const dust = fx.sim(P.glow(C.ice, 0));
      let spin = 0;
      return {
        frame(time, a, surge) {
          spin += fx.dt * (1.6 + surge * 4);
          for (const f of list) {
            const ang = spin + f.phase;
            const t = (f.y + time * 0.12) % 1;
            const pt = polar({ x: u.center.x, y: u.feet.y - t * h * 1.05 }, (52 + 14 * t) * f.r * (1 + surge * 0.3), ang, 0.3);
            const behind = Math.sin(ang) < 0;
            const [show, hide] = behind ? [f.back, f.front] : [f.front, f.back];
            hide.hide();
            show.put(pt.x, pt.y, 7, a * bump(t) * (behind ? 0.5 : 0.95), ang * 2);
            if (Math.random() < 0.08 * a) dust.spawn({ x: pt.x, y: pt.y, vx: 0, vy: 0, lifetime: 0.4, size: 4 });
          }
        },
      };
    },
  }),

  // ===== Шок =====
  aura({
    id: 'al-shock-dome', name: 'Грозовая сфера', group: 'Аура · Шок', spell: 'Shock',
    about: 'Тёплый жёлтый купол, по его поверхности то и дело пробегают дуги; при срабатывании сферу разом опутывает сетка разрядов',
    build(fx, u) {
      const set = dome(fx, '#ffb300', '#ffffff', '#fde68a');
      const ground = floor(fx, '#fde68a');
      const { rx, ry, cy } = domeSize(u, fx.lvl([0.9, 0.95, 1]));
      const c = { x: u.center.x, y: cy };
      const beams = Array.from({ length: 6 }, () => new Beam(fx, 8, 2.2, '#fde68a'));
      const busy = beams.map(() => 0);
      const sparks = fx.sim(P.spark(C.bolt, 300));
      // Дуга по поверхности купола между двумя углами
      const arcOn = () => {
        const a0 = rand(0, TAU);
        const a1 = a0 + rand(0.5, 1.2);
        return Array.from({ length: 8 }, (_, k) => {
          const pt = polar(c, 1, lerp(a0, a1, k / 7));
          return { x: c.x + (pt.x - c.x) * rx * rand(0.94, 1.04), y: c.y + (pt.y - c.y) * ry * rand(0.94, 1.04) };
        });
      };
      const fire = (k: number, at: number, dur: number) => {
        busy[k] = at + dur;
        crackle(fx, beams[k], at, dur, arcOn);
      };
      return {
        frame(time, a, surge) {
          set(c.x, c.y, rx, ry, a * (0.5 + 0.4 * surge) * (Math.random() < 0.06 ? 1.25 : 1));
          ground(u, rx * 1.1, a * 0.5);
          if (a > 0.6 && Math.random() < fx.dt * fx.lvl([2, 3, 4.5])) {
            const k = busy.findIndex(until => until <= time);
            if (k >= 0) fire(k, time, 0.18);
          }
        },
        trigger() {
          beams.forEach((_, k) => fire(k, fx.now + k * 0.03, 0.3));
          burst(sparks, c.x, c.y);
        },
      };
    },
  }),
  aura({
    id: 'al-shock-coil', name: 'Катушка', group: 'Аура · Шок', spell: 'Shock',
    about: 'Две наклонные орбиты, как у атома, по ним бегут яркие искры, между ними проскакивает разряд; при срабатывании орбиты раскручиваются',
    build(fx, u) {
      const g = fx.gfx('front', 'add');
      const gBack = fx.gfx('back', 'add');
      const c = { x: u.center.x, y: u.center.y };
      const orbits = [{ tilt: 0.5 }, { tilt: -0.5 }];
      const runners = orbits.map(() => [fx.dot('glow', '#fff7d6'), fx.dot('glow', '#fff7d6', 'back')]);
      const zap = new Beam(fx, 6, 2, '#fde68a');
      let spin = 0;
      let zapUntil = 0;
      const R = 70;
      const onOrbit = (tilt: number, ang: number): Pt => {
        const x = Math.cos(ang) * R;
        const y = Math.sin(ang) * R * 0.3;
        return { x: c.x + x * Math.cos(tilt) - y * Math.sin(tilt), y: c.y + x * Math.sin(tilt) + y * Math.cos(tilt) };
      };
      return {
        frame(time, a, surge) {
          spin += fx.dt * (3 + surge * 8);
          g.clear();
          gBack.clear();
          orbits.forEach((o, k) => {
            for (const [layer, from, alpha] of [[gBack, Math.PI, 0.25], [g, 0, 0.5]] as const) {
              const pts = Array.from({ length: 13 }, (_, n) => onOrbit(o.tilt, from + (n / 12) * Math.PI));
              layer.moveTo(pts[0].x, pts[0].y);
              pts.slice(1).forEach(pt => layer.lineTo(pt.x, pt.y));
              layer.stroke({ width: 1.5, color: color('#fde68a'), alpha: a * (alpha + surge * 0.3) });
            }
            const ang = spin * (k ? -1 : 1) * fx.lvl([0.8, 1, 1.2]);
            const pt = onOrbit(o.tilt, ang);
            const behind = Math.sin(ang) < 0;
            runners[k][behind ? 0 : 1].hide();
            runners[k][behind ? 1 : 0].put(pt.x, pt.y, 10 * (1 + surge), a);
          });
          if (time > zapUntil && Math.random() < fx.dt * 2.5 * a) {
            zapUntil = time + 0.15;
            crackle(fx, zap, time, 0.15, () => jag(onOrbit(0.5, spin), onOrbit(-0.5, -spin), 6, 10));
          }
        },
      };
    },
  }),
  aura({
    id: 'al-shock-cloud', name: 'Грозовое облачко', group: 'Аура · Шок', spell: 'Shock',
    about: 'Над головой носителя висит маленькая тучка, внутри мигают вспышки, иногда в землю рядом бьёт молния; при срабатывании — удар прямо у ног',
    build(fx, u) {
      const top = { x: u.head.x, y: u.head.y - 70 };
      const cloud = fx.sim({ texture: 'smoke', blend: 'normal', drag: 0.7, gravity: 0, shrink: 1.1, tint: palette(['#64748b', '#475569', '#334155']), alpha: life => bump(life) * 0.6 });
      const inner = fx.dot('glow', '#fde68a');
      const bolt = new Beam(fx, 9, 3, '#fde68a');
      const sparks = fx.sim(P.spark(C.bolt, 500));
      let next = 1;
      const strike = (at: number, x: number) => {
        crackle(fx, bolt, at, 0.18, () => jag({ x: top.x + rand(-15, 15), y: top.y + 10 }, { x, y: u.feet.y + 4 }, 9, 14));
        fx.at(at, () => burst(sparks, x, u.feet.y));
        ringWave(fx, { at, dur: 0.35, x, y: u.feet.y, from: 10, to: 50, css: '#fde68a', squash: GROUND, layer: 'back' });
      };
      return {
        frame(time, a) {
          for (let n = fx.count(fx.lvl([14, 18, 24]) * a); n > 0; n--) cloud.spawn({ x: top.x + rand(-34, 34), y: top.y + rand(-10, 10), vx: rand(-6, 6), vy: rand(-3, 3), lifetime: rand(0.8, 1.2), size: rand(14, 22) });
          inner.oval(top.x + rand(-15, 15), top.y, 30, 16, a * (Math.random() < 0.08 ? 0.8 : 0.1));
          if (a > 0.8 && time > next) {
            next = time + rand(1.2, 2.4);
            strike(time, u.feet.x + rand(-70, 70));
          }
        },
        trigger() {
          strike(fx.now, u.feet.x + 20);
        },
      };
    },
  }),

  // ===== Яд =====
  aura({
    id: 'al-poison-bubble', name: 'Ядовитый пузырь', group: 'Аура · Яд', spell: 'Poison',
    about: 'Носитель внутри переливающегося зелёного пузыря; по краю набухают и отрываются пузырьки, снизу капает яд; при срабатывании пузырь вздрагивает и дымит',
    build(fx, u) {
      const set = dome(fx, '#65a30d', '#ecfccb', '#a3e635');
      const ground = floor(fx, '#65a30d');
      const { rx, ry, cy } = domeSize(u, fx.lvl([0.9, 0.95, 1]));
      const puffs = fx.sim({ texture: 'smoke', blend: 'normal', drag: 0.5, gravity: -30, shrink: 1.4, tint: palette(['#a3e635', '#4d7c0f']), alpha: life => bump(life) * 0.4 });
      const bubbles = fx.sim({ texture: bubbleTexture(), blend: 'normal', drag: 0.6, gravity: -40, shrink: 1.15, alpha: life => Math.min(1, life * 3) });
      const drips = fx.sim({ texture: dropTexture('venom'), blend: 'normal', drag: 0.1, gravity: 700, shrink: 0.9 });
      const tint = fx.dot('glow', '#84cc16', 'back');
      return {
        frame(time, a, surge) {
          const wx = 1 + Math.sin(time * 2.3) * 0.04 + surge * 0.08;
          const wy = 1 + Math.cos(time * 2.3) * 0.04 - surge * 0.05;
          set(u.center.x, cy, rx * wx, ry * wy, a * 0.6);
          tint.oval(u.center.x, cy, rx * 0.8, ry * 0.8, a * 0.15);
          ground(u, rx * 1.1, a * 0.6);
          // Пузырьки на поверхности: появляются по ободу и уплывают вверх
          if (Math.random() < fx.count(fx.lvl([5, 7, 10]) * a)) {
            const pt = onRim(u.center.x, cy, rx * wx, ry * wy, rand(0, TAU));
            bubbles.spawn({ x: pt.x, y: pt.y, vx: rand(-6, 6), vy: rand(-30, -10), lifetime: rand(0.8, 1.3), size: rand(4, 8) });
          }
          // Капли яда срываются с нижней кромки
          if (Math.random() < fx.count(1.5 * a)) {
            const pt = onRim(u.center.x, cy, rx * wx, ry * wy, rand(Math.PI * 0.25, Math.PI * 0.75));
            drips.spawn({ x: pt.x, y: pt.y, vx: 0, vy: 0, lifetime: 0.3, size: 6, rotation: Math.PI / 2 });
          }
          for (let n = fx.count(40 * surge); n > 0; n--) {
            const pt = onRim(u.center.x, cy, rx, ry, rand(-Math.PI, 0));
            puffs.spawn({ x: pt.x, y: pt.y, vx: rand(-20, 20), vy: -30, lifetime: rand(0.6, 1), size: rand(12, 20) });
          }
        },
      };
    },
  }),
  aura({
    id: 'al-poison-mire', name: 'Болотная дымка', group: 'Аура · Яд', spell: 'Poison',
    about: 'Без купола: у ног носителя клубится зелёный туман и булькают пузыри, на земле — ядовитая лужа; при срабатывании туман вздымается',
    build(fx, u) {
      const fog = fx.sim({ texture: 'smoke', blend: 'normal', drag: 0.5, gravity: -12, shrink: 1.5, tint: palette(['#bef264', '#65a30d', '#365314']), alpha: life => bump(life) * 0.4 }, 'back');
      const fogFront = fx.sim({ texture: 'smoke', blend: 'normal', drag: 0.5, gravity: -12, shrink: 1.5, tint: palette(['#bef264', '#65a30d', '#365314']), alpha: life => bump(life) * 0.35 });
      const bubbles = fx.sim(P.glow(C.venom, 60));
      const puddle = fx.radial([[0, 0.8], [0.7, 0.55], [1, 0]], '#3f6212', 'back', 'normal');
      const sheen = fx.radial([[0, 0.4], [1, 0]], '#a3e635', 'back');
      return {
        frame(time, a, surge) {
          puddle.oval(u.feet.x, u.feet.y, 85, 85 * GROUND, a * 0.7);
          sheen.oval(u.feet.x, u.feet.y, 70, 70 * GROUND, a * (0.3 + 0.1 * Math.sin(time * 2)));
          for (let n = fx.count(fx.lvl([14, 20, 28]) * a * (1 + surge * 3)); n > 0; n--) {
            const pt = polar(u.feet, rand(10, 80), rand(0, TAU), GROUND);
            (Math.sin(pt.y) > 0 ? fog : fogFront).spawn({ x: pt.x, y: pt.y - 6, vx: rand(-15, 15), vy: rand(-20, -5) * (1 + surge * 4), lifetime: rand(1, 1.5), size: rand(16, 28) });
          }
          if (Math.random() < fx.count(4 * a)) {
            const pt = polar(u.feet, rand(0, 60), rand(0, TAU), GROUND);
            bubbles.spawn({ x: pt.x, y: pt.y, vx: 0, vy: -40, lifetime: 0.6, size: rand(4, 7) });
          }
        },
      };
    },
  }),
  aura({
    id: 'al-poison-spores', name: 'Облако спор', group: 'Аура · Яд', spell: 'Poison',
    about: 'Вокруг носителя медленно дрейфуют светящиеся споры, каждая по своей петле; при срабатывании они разлетаются шире и снова собираются',
    build(fx, u) {
      const h = bodyOf(u);
      const count = fx.lvl([10, 14, 18]);
      const spores = Array.from({ length: count }, () => ({ dot: fx.dot('glow', '#d9f99d', Math.random() < 0.5 ? 'back' : 'front'), core: fx.dot('glow', '#f7fee7'), phase: rand(0, TAU), speed: rand(0.4, 0.9), r: rand(40, 75), y: rand(0.1, 0.95) }));
      return {
        frame(time, a, surge) {
          for (const s of spores) {
            const ang = s.phase + time * s.speed;
            const r = s.r * (1 + surge * 0.6);
            const x = u.center.x + Math.cos(ang) * r;
            const y = u.feet.y - h * s.y + Math.sin(ang * 1.7) * 14;
            const flicker = 0.7 + 0.3 * Math.sin(time * 4 + s.phase);
            s.dot.put(x, y, 10, a * 0.7 * flicker);
            s.core.put(x, y, 3, a * flicker);
          }
        },
      };
    },
  }),

  // ===== Выжигание энергии (тьма) =====
  aura({
    id: 'al-void-dome', name: 'Купол тьмы', group: 'Аура · Выжигание энергии', spell: 'EnergyBurn',
    about: 'Тёмный, почти чёрный купол с фиолетовой кромкой, по краю ползут дымные завитки; при срабатывании купол на миг сжимается',
    build(fx, u) {
      const murk = fx.dot('glow', '#09030f', 'back', 'normal');
      const set = dome(fx, '#7e22ce', '#f5d0fe', '#3b0764');
      const wisps = fx.sim({ texture: 'smoke', blend: 'normal', drag: 0.4, gravity: -30, shrink: 1.2, tint: palette(['#2e1065', '#1e0b36']), alpha: life => bump(life) * 0.5 });
      const motes = fx.sim(P.glow(C.dark, 30));
      const { rx, ry, cy } = domeSize(u, fx.lvl([0.9, 0.95, 1]));
      return {
        frame(time, a, surge) {
          const s = breath(time, 2.4) * (1 - surge * 0.12);
          murk.oval(u.center.x, cy, rx * s * 0.95, ry * s * 0.95, a * 0.35);
          set(u.center.x, cy, rx * s, ry * s, a * (0.6 + 0.3 * surge));
          for (let n = fx.count(fx.lvl([10, 14, 20]) * a); n > 0; n--) {
            const pt = polar({ x: u.center.x, y: cy }, 1, rand(0, TAU));
            const x = u.center.x + (pt.x - u.center.x) * rx * s;
            const y = cy + (pt.y - cy) * ry * s;
            wisps.spawn({ x, y, vx: rand(-10, 10), vy: rand(-20, 0), lifetime: rand(0.8, 1.2), size: rand(10, 18) });
            if (Math.random() < 0.3) motes.spawn({ x, y, vx: 0, vy: -20, lifetime: 0.6, size: rand(3, 6) });
          }
        },
      };
    },
  }),
  aura({
    id: 'al-void-eye', name: 'Око над головой', group: 'Аура · Выжигание энергии', spell: 'EnergyBurn',
    about: 'Над головой носителя в клубке тьмы висит одинокое злобное око, медленно оглядывает поле и моргает; при срабатывании широко распахивается',
    build(fx, u) {
      const c = { x: u.head.x, y: u.head.y - 62 };
      const murk = fx.sim({ texture: 'smoke', blend: 'normal', drag: 0.6, gravity: 0, shrink: 1.2, tint: palette(['#2e1065', '#1e0b36', '#09030f']), alpha: life => bump(life) * 0.6 });
      const glow = fx.dot('glow', '#a855f7');
      const eye = fx.gfx('front', 'normal');
      const pupil = fx.gfx('front', 'normal');
      return {
        frame(time, a, surge) {
          for (let n = fx.count(22 * a); n > 0; n--) murk.spawn({ x: c.x + rand(-26, 26), y: c.y + rand(-12, 12), vx: rand(-8, 8), vy: rand(-6, 6), lifetime: rand(0.6, 1), size: rand(12, 20) });
          glow.oval(c.x, c.y, 52, 30, a * (0.35 + 0.4 * surge));
          const blink = (time % 3.4) > 3.25 ? 0.1 : 1;
          const open = clamp01(time / 0.4) * blink * (1 + surge * 0.5);
          const w = fx.lvl([16, 18, 21]);
          eye.clear();
          pupil.clear();
          if (a < 0.02) return;
          eye.ellipse(c.x, c.y, w, w * 0.55 * Math.min(open, 1.4)).fill({ color: color('#f5d0fe'), alpha: a * 0.95 });
          const look = Math.sin(time * 0.7) * w * 0.4;
          pupil.ellipse(c.x + look, c.y, 3.5, w * 0.45 * Math.min(open, 1.2)).fill({ color: color('#1e0b36'), alpha: a });
        },
      };
    },
  }),
  aura({
    id: 'al-void-tendrils', name: 'Живая тень', group: 'Аура · Выжигание энергии', spell: 'EnergyBurn',
    about: 'Без купола: тень под носителем растеклась пятном, из неё колышутся чёрные щупальца; при срабатывании они хлещут вверх',
    build(fx, u) {
      const h = bodyOf(u);
      const pool = fx.radial([[0, 0.95], [0.6, 0.7], [1, 0]], '#09030f', 'back', 'normal');
      const rim = fx.dot('ring', '#7e22ce', 'back');
      const count = fx.lvl([3, 4, 6]);
      const tendrils = Array.from({ length: count }, (_, k) => {
        const ang = (k / count) * TAU + 0.4;
        const base = polar(u.feet, 55, ang, GROUND);
        const behind = Math.sin(ang) < 0;
        const body = fx.strand(10, 9, '#1e0b36', 1, behind ? 'back' : 'front');
        body.rope.blendMode = 'normal';
        const glow = fx.strand(10, 2.5, '#a855f7', 1, behind ? 'back' : 'front');
        return { base, body, glow, phase: rand(0, TAU), len: h * rand(0.35, 0.55), lean: Math.cos(ang) * 0.4 };
      });
      return {
        frame(time, a, surge) {
          pool.oval(u.feet.x, u.feet.y, 80, 80 * GROUND, a * 0.85);
          rim.oval(u.feet.x, u.feet.y, 80, 80 * GROUND, a * 0.5);
          for (const t of tendrils) {
            const len = t.len * clamp01(time / 0.5) * (1 + surge * 0.8);
            const pts = Array.from({ length: 10 }, (_, k) => {
              const q = k / 9;
              const sway = Math.sin(time * 2.4 + t.phase - q * 3) * 14 * q;
              return { x: t.base.x + sway + t.lean * len * q, y: t.base.y - len * q };
            });
            for (const strand of [t.body, t.glow]) {
              strand.rope.visible = a > 0.02;
              strand.rope.alpha = a * (strand === t.glow ? 0.5 : 0.9);
              strand.set(pts);
            }
          }
        },
      };
    },
  }),

  // ===== Кровотечение =====
  aura({
    id: 'al-blood-dome', name: 'Кровавая сфера', group: 'Аура · Кровотечение', spell: 'Bleed',
    about: 'Яркий алый купол бьётся, как сердце; по его краю стекают капли крови и тлеют красные искры; при срабатывании сфера вспыхивает сильнее',
    build(fx, u) {
      const set = dome(fx, '#ef4444', '#fecaca', '#dc2626');
      // Вторая подсветка обода: кровавая сфера ярче остальных
      const glowRim = fx.radial([[0, 0], [0.7, 0], [0.9, 0.55], [1, 0]], '#f87171');
      const inner = fx.dot('glow', '#b91c1c', 'back');
      const ground = floor(fx, '#dc2626');
      const drops = fx.sim({ texture: dropTexture('blood'), blend: 'normal', drag: 0.2, gravity: 500, shrink: 0.9 });
      const motes = fx.sim(P.ember(C.blood, 40));
      const { rx, ry, cy } = domeSize(u, fx.lvl([0.9, 0.95, 1]));
      // Капли, стекающие по ободу сверху вниз: угол от верха к низу по своей стороне
      const runners = Array.from({ length: fx.lvl([3, 4, 6]) }, () => ({ dot: fx.tex(dropTexture('blood')), t: rand(0, 1), side: Math.random() < 0.5 ? -1 : 1, speed: rand(0.35, 0.6) }));
      return {
        frame(time, a, surge) {
          const phase = time % 1.1;
          const beat = Math.max(Math.exp(-phase * 10), phase > 0.22 ? 0.7 * Math.exp(-(phase - 0.22) * 10) : 0);
          const s = 1 + beat * 0.05 + surge * 0.1;
          set(u.center.x, cy, rx * s, ry * s, a * (0.85 + 0.15 * beat));
          glowRim.oval(u.center.x, cy, rx * s, ry * s, a * (0.45 + 0.35 * beat + 0.4 * surge));
          inner.oval(u.center.x, cy, rx * 0.8 * s, ry * 0.8 * s, a * (0.12 + 0.1 * beat + 0.15 * surge));
          ground(u, rx * 1.15, a * (0.7 + 0.2 * beat));
          for (const r of runners) {
            r.t += fx.dt * r.speed;
            if (r.t >= 1) {
              // Капля сорвалась с низа купола и падает
              const end = onRim(u.center.x, cy, rx * s, ry * s, Math.PI / 2 - r.side * 0.5);
              if (a > 0.3) drops.spawn({ x: end.x, y: end.y, vx: 0, vy: 40, lifetime: 0.3, size: 6, rotation: Math.PI / 2 });
              r.t = 0;
              r.side = Math.random() < 0.5 ? -1 : 1;
            }
            const ang = -Math.PI / 2 + r.side * (0.25 + r.t * 1.35) * (Math.PI / 2) / 1.1;
            const pt = onRim(u.center.x, cy, rx * s, ry * s, ang);
            r.dot.put(pt.x, pt.y, 6 + r.t * 2, a * Math.min(1, r.t * 5), ang + r.side * Math.PI / 2);
          }
          if (Math.random() < fx.count(fx.lvl([6, 9, 12]) * a * (1 + surge * 3))) {
            const pt = onRim(u.center.x, cy, rx * s, ry * s, rand(0, TAU));
            motes.spawn({ x: pt.x, y: pt.y, vx: rand(-8, 8), vy: rand(-40, -15), lifetime: rand(0.5, 0.9), size: rand(2.5, 4.5) });
          }
        },
        trigger() {
          ringWave(fx, { at: fx.now, dur: 0.45, x: u.center.x, y: cy, from: rx * 0.9, to: rx * 1.45, css: '#f87171', squash: ry / rx });
        },
      };
    },
  }),
  aura({
    id: 'al-blood-heart', name: 'Бьющееся сердце', group: 'Аура · Кровотечение', spell: 'Bleed',
    about: 'Носитель окружён ярким алым ореолом, который бьётся «тук-тук», от каждого удара по земле расходится алое кольцо; при срабатывании удар сильнее',
    build(fx, u) {
      const h = bodyOf(u);
      // Ореол в два слоя: широкое свечение за юнитом и светлая сердцевина поверх — чтобы был заметен на любой арене
      const glow = fx.dot('glow', '#dc2626', 'back');
      const halo = fx.dot('glow', '#ef4444');
      const core = fx.dot('glow', '#fca5a5', 'back');
      const rim = fx.dot('ring', '#f87171');
      const rimGlow = fx.dot('ring', '#dc2626');
      const ground = floor(fx, '#b91c1c');
      let nextBeat = 0.3;
      return {
        frame(time, a, surge) {
          const phase = time % 1.1;
          const beat = Math.max(Math.exp(-phase * 12), 0.7 * Math.exp(-Math.max(0, phase - 0.22) * 12) * (phase > 0.22 ? 1 : 0));
          const k = 1 + beat * 0.08 + surge * 0.15;
          glow.oval(u.center.x, u.center.y, 85 * k, h * 0.72 * k, a * (0.6 + 0.35 * beat + 0.3 * surge));
          core.oval(u.center.x, u.center.y, 55 * k, h * 0.5 * k, a * (0.3 + 0.35 * beat));
          halo.oval(u.center.x, u.center.y, 80 * k, h * 0.68 * k, a * (0.18 + 0.3 * beat + 0.3 * surge));
          rim.oval(u.center.x, u.center.y, 72 * k, h * 0.62 * k, a * (0.55 + 0.45 * beat));
          rimGlow.oval(u.center.x, u.center.y, 76 * k, h * 0.65 * k, a * (0.35 + 0.4 * beat));
          ground(u, 90, a * (0.75 + 0.25 * beat));
          if (a > 0.5 && time > nextBeat) {
            nextBeat += 1.1;
            ringWave(fx, { at: time, dur: 0.6, x: u.feet.x, y: u.feet.y, from: 30, to: fx.lvl([100, 120, 140]), css: '#f87171', squash: GROUND, layer: 'back', alpha: 0.85 * a });
          }
        },
        trigger() {
          ringWave(fx, { at: fx.now, dur: 0.5, x: u.center.x, y: u.center.y, from: 50, to: 130, css: '#f87171', squash: h / 120 });
        },
      };
    },
  }),
  aura({
    id: 'al-blood-drops', name: 'Кружащие капли', group: 'Аура · Кровотечение', spell: 'Bleed',
    about: 'Вокруг носителя по наклонной орбите плывут капли крови, оставляя алую дымку; при срабатывании они вытягиваются и ускоряются',
    build(fx, u) {
      const tex = dropTexture('blood');
      const count = fx.lvl([4, 6, 8]);
      const drops = Array.from({ length: count }, (_, k) => ({ front: fx.tex(tex), back: fx.tex(tex, 'back'), phase: (k / count) * TAU }));
      const mist = fx.sim(P.glow(C.blood, 0));
      const mistBack = fx.sim(P.glow(C.blood, 0), 'back');
      const c = { x: u.center.x, y: u.center.y };
      let spin = 0;
      return {
        frame(_time, a, surge) {
          spin += fx.dt * (1.4 + surge * 4);
          for (const d of drops) {
            const ang = spin + d.phase;
            const x = Math.cos(ang) * 64;
            const y = Math.sin(ang) * 20;
            const pt = { x: c.x + x, y: c.y + y - x * 0.25 };
            const behind = Math.sin(ang) < 0;
            const [show, hide] = behind ? [d.back, d.front] : [d.front, d.back];
            hide.hide();
            const dir = ang + Math.PI / 2;
            show.oval(pt.x, pt.y, 10 * (1 + surge * 0.6), 7, a * (behind ? 0.6 : 1), dir + Math.PI);
            if (Math.random() < 0.4 * a) (behind ? mistBack : mist).spawn({ x: pt.x, y: pt.y, vx: 0, vy: 0, lifetime: 0.4, size: 6 });
          }
        },
      };
    },
  }),
  aura({
    id: 'al-blood-circle', name: 'Кровавый круг', group: 'Аура · Кровотечение', spell: 'Bleed',
    about: 'Под ногами медленно вращается круг из алых рун, вверх тянутся красные искры; при срабатывании руны вспыхивают и круг сжимается',
    build(fx, u) {
      const g = fx.gfx('back', 'add');
      const motes = fx.sim(P.ember(C.blood, 60));
      const stain = fx.radial([[0, 0.7], [0.7, 0.5], [1, 0]], '#450a0a', 'back', 'normal');
      return {
        frame(time, a, surge) {
          const R = 82 * (1 - surge * 0.15);
          stain.oval(u.feet.x, u.feet.y, R, R * GROUND, a * 0.6);
          g.clear();
          if (a < 0.02) return;
          g.ellipse(u.feet.x, u.feet.y, R, R * GROUND).stroke({ width: 2.5, color: color('#ef4444'), alpha: a * (0.7 + 0.3 * surge) });
          g.ellipse(u.feet.x, u.feet.y, R * 0.8, R * 0.8 * GROUND).stroke({ width: 1.2, color: color('#f87171'), alpha: a * 0.6 });
          const runes = fx.lvl([6, 8, 10]);
          for (let k = 0; k < runes; k++) {
            const ang = (k / runes) * TAU + time * 0.4;
            const pt = polar(u.feet, R * 0.9, ang, GROUND);
            g.poly([pt.x, pt.y - 7, pt.x + 4, pt.y, pt.x, pt.y + 3, pt.x - 4, pt.y]).fill({ color: color('#fecaca'), alpha: a * (0.5 + 0.5 * surge) });
          }
          for (let n = fx.count(10 * a * (1 + surge * 4)); n > 0; n--) {
            const pt = polar(u.feet, R * rand(0.7, 1), rand(0, TAU), GROUND);
            motes.spawn({ x: pt.x, y: pt.y, vx: 0, vy: rand(-80, -40), lifetime: rand(0.5, 0.9), size: rand(2.5, 4.5) });
          }
        },
      };
    },
  }),

  // ===== Регенерация =====
  aura({
    id: 'al-regen-dome', name: 'Светлый купол', group: 'Аура · Регенерация', spell: 'Regen', side: 'allies',
    about: 'Тёплый золотисто-зелёный купол, вокруг его края и внутри неспешно кружат мерцающие светлячки; при срабатывании купол светлеет, а светлячки разлетаются шире и вспыхивают',
    build(fx, u) {
      const set = dome(fx, '#22c55e', '#fef9c3', '#bef264');
      const ground = floor(fx, '#bef264');
      const trail = fx.sim(P.glow(['#fffbeb', '#fde68a', '#bef264'], 0));
      const { rx, ry, cy } = domeSize(u, fx.lvl([0.9, 0.95, 1]));
      // Светлячок: летит по своей петле у обода (часть — внутри купола), мерцает в своём ритме
      const count = fx.lvl([6, 8, 11]);
      const flies = Array.from({ length: count }, (_, k) => ({
        halo: fx.dot('glow', '#facc15', k % 3 ? 'front' : 'back'),
        core: fx.dot('glow', '#fffbeb', k % 3 ? 'front' : 'back'),
        phase: rand(0, TAU),
        speed: rand(0.25, 0.5) * (k % 2 ? 1 : -1),
        r: k % 3 === 2 ? rand(0.45, 0.75) : rand(0.9, 1.08),
        wobble: rand(0, TAU),
        blink: rand(2, 4),
      }));
      return {
        frame(time, a, surge) {
          const s = breath(time, 2.2);
          set(u.center.x, cy, rx * s, ry * s, a * (0.5 + 0.4 * surge));
          ground(u, rx * 1.1, a * 0.6);
          for (const f of flies) {
            const ang = f.phase + time * f.speed;
            const spread = f.r * (1 + surge * 0.35);
            const pt = onRim(u.center.x, cy, rx * s * spread, ry * s * spread, ang);
            // Петляют: небольшое покачивание поверх движения по кругу
            const x = pt.x + Math.sin(time * 1.7 + f.wobble) * 8;
            const y = pt.y + Math.cos(time * 2.3 + f.wobble) * 6;
            const flicker = 0.5 + 0.5 * Math.pow(0.5 + 0.5 * Math.sin(time * f.blink + f.wobble), 2);
            const glow = Math.min(1, flicker + surge * 0.8);
            f.halo.put(x, y, 24 * (1 + surge * 0.4), a * glow);
            f.core.put(x, y, 7, a * glow);
            if (Math.random() < 0.4 * a * glow) trail.spawn({ x, y, vx: 0, vy: -4, lifetime: 0.5, size: 5 });
          }
        },
      };
    },
  }),
  aura({
    id: 'al-regen-wreath', name: 'Венок листьев', group: 'Аура · Регенерация', spell: 'Regen', side: 'allies',
    about: 'У ног носителя медленно кружит венок из листьев, под ним светится мягкий зелёный круг; при срабатывании листья приподнимаются',
    build(fx, u) {
      const leaf = leafTexture();
      const count = fx.lvl([6, 8, 10]);
      const leaves = Array.from({ length: count }, (_, k) => ({ front: fx.tex(leaf), back: fx.tex(leaf, 'back'), phase: (k / count) * TAU }));
      const ground = floor(fx, '#4ade80');
      const spores = fx.sim(P.glow(C.nature, 40));
      return {
        frame(time, a, surge) {
          ground(u, 80, a * 0.6);
          for (const l of leaves) {
            const ang = l.phase + time * 0.6;
            const pt = polar(u.feet, 66, ang, GROUND);
            const behind = Math.sin(ang) < 0;
            const [show, hide] = behind ? [l.back, l.front] : [l.front, l.back];
            hide.hide();
            show.put(pt.x, pt.y - 6 - surge * 30 - Math.sin(time * 2 + l.phase) * 3, 11, a * (behind ? 0.6 : 1), ang + Math.PI / 2);
          }
          if (Math.random() < fx.count(4 * a)) {
            const pt = polar(u.feet, 66, rand(0, TAU), GROUND);
            spores.spawn({ x: pt.x, y: pt.y, vx: 0, vy: -40, lifetime: 0.8, size: rand(4, 7) });
          }
        },
      };
    },
  }),
  aura({
    id: 'al-regen-halo', name: 'Нимб и лучи', group: 'Аура · Регенерация', spell: 'Regen', side: 'allies',
    about: 'Над головой носителя парит золотой нимб, из-за спины мягко расходятся лучи, вокруг мерцают искорки; при срабатывании лучи вытягиваются',
    build(fx, u) {
      const nimbus = fx.dot('ring', '#fde68a');
      const nimbusGlow = fx.dot('glow', '#fbbf24');
      const rays = fx.gfx('back', 'add');
      const motes = fx.sim(P.star(C.light, 20));
      return {
        frame(time, a, surge) {
          const top = { x: u.head.x, y: u.head.y - 16 + Math.sin(time * 1.8) * 2 };
          nimbus.oval(top.x, top.y, 26, 8, a);
          nimbusGlow.oval(top.x, top.y, 40, 16, a * 0.5);
          rays.clear();
          if (a > 0.02) {
            const count = fx.lvl([6, 8, 10]);
            for (let k = 0; k < count; k++) {
              const ang = -Math.PI / 2 + (k - (count - 1) / 2) * 0.32 + Math.sin(time * 0.5) * 0.05;
              const len = (k % 2 ? 70 : 100) * (1 + surge * 0.6) * (0.9 + 0.1 * Math.sin(time * 2 + k));
              const p1 = polar(u.center, 25, ang);
              const p2 = polar(u.center, 25 + len, ang);
              rays.moveTo(p1.x, p1.y).lineTo(p2.x, p2.y).stroke({ width: 5, color: color('#fde68a'), alpha: a * 0.18 });
            }
          }
          if (Math.random() < fx.count(5 * a)) motes.spawn({ x: u.center.x + rand(-50, 50), y: rand(u.head.y - 20, u.feet.y), vx: 0, vy: -15, lifetime: 0.7, size: rand(4, 7) });
        },
      };
    },
  }),
];

// Искры во все стороны из точки
function burst(sim: ParticleSim, x: number, y: number) {
  for (let i = 0; i < 12; i++) {
    const ang = rand(0, TAU);
    const speed = rand(100, 300);
    sim.spawn({ x, y, vx: Math.cos(ang) * speed, vy: Math.sin(ang) * speed, lifetime: rand(0.2, 0.45), size: rand(2, 4) });
  }
}
