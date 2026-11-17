import {
  Beam, C, GROUND, P, TAU, angryEye, bump, clamp01, color, crackle, ease, jag, lerp, palette, polar, pop, rand, ringWave, spray, taper,
  type Pt,
} from './kit';
import { bodyOf, type Look } from './looks';
import { bubbleTexture, dropTexture, flameTexture, iceChipTexture, iceFangTexture, leafTexture, snowflakeTexture } from './sprites';

// Облики статусов (ECS/StatusEffects): две стадии — висит на юните (frame) и срабатывает (trigger).
// Срабатывание — тик в начале хода носителя (урон, лечение), у оглушения и обледенения — пропуск хода,
// у мороза — замедленный набор энергии на ударе, у метки — попадание заклинания по цели.
// Несколько статусов на одном юните рисуются поверх друг друга — страница концептов позволяет их накладывать

const status = (look: Omit<Look, 'kind'>): Look => ({ ...look, kind: 'status' });

const FLAME = { texture: 'smoke' as const, drag: 0.2, gravity: -420, shrink: 0.2, stretch: [0.7, 2] as [number, number], tint: palette(C.fire.slice(1)), alpha: (life: number) => bump(life) * 0.85 };
const ENERGY = ['#e0f2fe', '#67e8f9', '#a78bfa', '#7e22ce', '#2e1065'];
const CHIPS = { texture: iceChipTexture(), blend: 'normal' as const, drag: 0.3, gravity: 1000, shrink: 0.6 };

// Снятие: облик начал гаснуть (для того, что при снятии рассыпается — лёд, корка)
const fading = () => {
  let last = 0;
  let done = false;
  return (time: number, a: number) => {
    const now = time > 0.6 && a < last && a < 0.97 && !done;
    last = a;
    if (now) done = true;
    return now;
  };
};

export const STATUS_LOOKS: Look[] = [
  // ===== Горение =====
  status({
    id: 'sl-burn-tongues', name: 'Языки пламени', group: 'Эффект · Горение', spell: 'Burn',
    about: 'Висит: по телу пляшут языки пламени, у ног тлеет пятно жара. Тик: пламя взвивается выше головы',
    build(fx, u) {
      const flames = fx.sim(FLAME);
      const sparks = fx.sim(P.spark(C.fire, -150));
      const glow = fx.radial([[0, 0.6], [0.5, 0.3], [1, 0]], '#ff6a00', 'back');
      return {
        frame(_time, a, surge) {
          glow.oval(u.feet.x, u.feet.y, 80, 80 * GROUND, a * (0.6 + 0.4 * surge));
          for (let n = fx.count((70 + 260 * surge) * a); n > 0; n--) {
            flames.spawn({ x: u.center.x + rand(-34, 34), y: rand(u.center.y, u.feet.y), vx: rand(-15, 15), vy: rand(-180, -80) * (1 + surge), lifetime: rand(0.25, 0.45), size: rand(6, 13) * (1 + surge * 0.6) });
          }
          for (let n = fx.count((10 + 60 * surge) * a); n > 0; n--) sparks.spawn({ x: u.center.x + rand(-30, 30), y: rand(u.head.y, u.feet.y), vx: rand(-30, 30), vy: rand(-160, -60), lifetime: rand(0.4, 0.8), size: rand(2, 4) });
        },
        trigger: () => u.hit(false, '#ff7a1a'),
      };
    },
  }),
  status({
    id: 'sl-burn-coals', name: 'Тлеющие угли', group: 'Эффект · Горение', spell: 'Burn',
    about: 'Висит: тело налито тёмно-красным жаром, у ног медленно кружат угли, чадит дымок. Тик: угли разом вспыхивают и взлетают',
    build(fx, u) {
      const h = bodyOf(u);
      const smoke = fx.sim(P.smoke('#27272a', 60));
      const embers = fx.sim(P.ember(C.fire, 120));
      const heat = fx.dot('glow', '#c2410c');
      const coals = Array.from({ length: 9 }, (_, k) => ({ dot: fx.dot('glow', k % 3 ? '#ff7a1a' : '#ffd166'), a: (k / 9) * TAU }));
      return {
        frame(time, a, surge) {
          heat.oval(u.center.x, u.center.y, 58, h * 0.58, a * (0.3 + 0.08 * Math.sin(time * 6) + 0.35 * surge));
          for (const c of coals) {
            const ang = c.a + time * 0.8;
            const pt = polar(u.feet, 70, ang, GROUND);
            c.dot.put(pt.x, pt.y - 6, (8 + 3 * Math.sin(time * 9 + c.a * 4)) * (1 + surge), a * (Math.sin(ang) < 0 ? 0.5 : 1));
          }
          for (let n = fx.count(6 * a); n > 0; n--) smoke.spawn({ x: u.center.x + rand(-20, 20), y: u.head.y + rand(0, 30), vx: rand(-10, 10), vy: -30, lifetime: rand(0.8, 1.2), size: rand(10, 16) });
          for (let n = fx.count(80 * surge * a); n > 0; n--) {
            const pt = polar(u.feet, 70, rand(0, TAU), GROUND);
            embers.spawn({ x: pt.x, y: pt.y, vx: rand(-20, 20), vy: rand(-220, -90), lifetime: rand(0.4, 0.8), size: rand(3, 6) });
          }
        },
        trigger: () => u.hit(false, '#ff7a1a'),
      };
    },
  }),
  status({
    id: 'sl-burn-crown', name: 'Огненный венец', group: 'Эффект · Горение', spell: 'Burn',
    about: 'Висит: над головой горит венец из язычков пламени — горящего видно сразу. Тик: венец разгорается и роняет искры',
    build(fx, u) {
      const sparks = fx.sim(P.spark(C.fire, 300));
      const flame = flameTexture();
      const tongues = Array.from({ length: 5 }, (_, k) => ({ dot: fx.tex(flame, 'front', 'add'), a: (k / 5) * TAU }));
      const glow = fx.dot('glow', '#ff7a1a');
      return {
        frame(time, a, surge) {
          const top = { x: u.head.x, y: u.head.y - 14 };
          glow.oval(top.x, top.y - 10, 50 * (1 + surge * 0.5), 26 * (1 + surge * 0.5), a * 0.6);
          for (const t of tongues) {
            const ang = t.a + time * 2.2;
            const pt = polar(top, 24, ang, 0.35);
            const size = (16 + 4 * Math.sin(time * 18 + t.a * 3)) * (1 + surge * 0.7);
            t.dot.put(pt.x, pt.y - size * 0.4, size, a * (Math.sin(ang) < 0 ? 0.55 : 1), -Math.PI / 2 + Math.sin(time * 9 + t.a) * 0.15);
          }
          if (surge > 0.4 && Math.random() < 0.4) sparks.spawn({ x: top.x + rand(-20, 20), y: top.y, vx: rand(-80, 80), vy: rand(-200, -60), lifetime: rand(0.4, 0.7), size: rand(2, 4) });
        },
        trigger: () => u.hit(false, '#ff7a1a'),
      };
    },
  }),

  // ===== Яд =====
  status({
    id: 'sl-poison-bubbles', name: 'Пузыри', group: 'Эффект · Яд', spell: 'Poison',
    about: 'Висит: тело отдаёт болотной зеленью, вокруг всплывают и лопаются ядовитые пузыри. Тик: с тела срываются капли яда',
    build(fx, u) {
      const h = bodyOf(u);
      const bubbles = fx.sim({ texture: bubbleTexture(), blend: 'normal', drag: 0.5, gravity: -70, shrink: 1.1, alpha: life => Math.min(1, life * 3) });
      const pops = fx.sim(P.spark(C.venom, 200));
      const drops = fx.sim({ texture: dropTexture('venom'), blend: 'normal', drag: 0.3, gravity: 1100, shrink: 0.8 });
      const tint = fx.dot('glow', '#65a30d');
      return {
        frame(time, a) {
          tint.oval(u.center.x, u.center.y, 58, h * 0.58, a * (0.3 + 0.08 * Math.sin(time * 4)));
          if (Math.random() < fx.count(7 * a)) bubbles.spawn({ x: u.center.x + rand(-38, 38), y: rand(u.center.y - 10, u.feet.y - 10), vx: rand(-8, 8), vy: -30, lifetime: rand(0.6, 1), size: rand(5, 10) });
          if (Math.random() < fx.count(4 * a)) spray(pops, { x: u.center.x + rand(-40, 40), y: rand(u.head.y, u.center.y), count: 5, speed: [40, 100], life: [0.15, 0.3], size: [2, 3] });
        },
        trigger() {
          u.hit(false, '#a3e635');
          spray(drops, { x: u.center.x, y: u.center.y, count: 8, speed: [120, 300], dir: -Math.PI / 2, spread: 1.4, life: [0.4, 0.7], size: [5, 8], align: true });
        },
      };
    },
  }),
  status({
    id: 'sl-poison-skull', name: 'Ядовитое облачко', group: 'Эффект · Яд', spell: 'Poison',
    about: 'Висит: над головой зелёное облачко с черепком, медленно капает. Тик: черепок вздрагивает, облачко густеет',
    build(fx, u) {
      const cloud = fx.sim({ texture: 'smoke', blend: 'normal', drag: 0.6, gravity: -8, shrink: 1.2, tint: palette(['#a3e635', '#65a30d', '#365314']), alpha: life => bump(life) * 0.5 });
      const drops = fx.sim({ texture: dropTexture('venom'), blend: 'normal', drag: 0.1, gravity: 900, shrink: 1 });
      const skull = fx.gfx('front', 'normal');
      return {
        frame(time, a, surge) {
          const c = { x: u.head.x + Math.sin(time * 2) * 4, y: u.head.y - 55 - surge * 8 };
          for (let n = fx.count((22 + 60 * surge) * a); n > 0; n--) cloud.spawn({ x: c.x + rand(-30, 30), y: c.y + rand(-12, 12), vx: rand(-10, 10), vy: rand(-6, 6), lifetime: rand(0.6, 1), size: rand(14, 24) });
          if (Math.random() < fx.count(1.5 * a)) drops.spawn({ x: c.x + rand(-16, 16), y: c.y + 18, vx: 0, vy: 0, lifetime: 0.35, size: 7, rotation: Math.PI / 2 });
          skull.clear();
          if (a < 0.02) return;
          const s = 1 + surge * 0.25;
          skull.circle(c.x, c.y - 2 * s, 11 * s).fill({ color: color('#ecfccb'), alpha: a });
          skull.roundRect(c.x - 6 * s, c.y + 4 * s, 12 * s, 8 * s, 2).fill({ color: color('#ecfccb'), alpha: a });
          skull.circle(c.x - 4.5 * s, c.y - 2 * s, 3.2 * s).circle(c.x + 4.5 * s, c.y - 2 * s, 3.2 * s).fill({ color: color('#1a2e05'), alpha: a });
          skull.rect(c.x - 0.8 * s, c.y + 6 * s, 1.6 * s, 6 * s).fill({ color: color('#1a2e05'), alpha: a });
        },
        trigger: () => u.hit(false, '#a3e635'),
      };
    },
  }),
  status({
    id: 'sl-poison-veins', name: 'Отравленные вены', group: 'Эффект · Яд', spell: 'Poison',
    about: 'Висит: по телу ползут светящиеся зелёные прожилки и пульсируют. Тик: пульс прокатывается по ним от ног к голове',
    build(fx, u) {
      const h = bodyOf(u);
      const mist = fx.sim(P.glow(C.venom, 40));
      const veins = Array.from({ length: 5 }, () => {
        const x = u.center.x + rand(-26, 26);
        return jag({ x, y: u.feet.y - 6 }, { x: x + rand(-14, 14), y: u.head.y + h * rand(0.1, 0.35) }, 9, 10);
      });
      const g = fx.gfx('front', 'add');
      let trig = -10;
      return {
        frame(time, a) {
          const grow = clamp01(time / 0.5);
          const wave = clamp01((time - trig) / 0.5);
          g.clear();
          if (a < 0.02) return;
          for (const vein of veins) {
            const n = Math.max(2, Math.ceil(vein.length * grow));
            for (let k = 1; k < n; k++) {
              const t = k / (vein.length - 1);
              const pulse = wave > 0 && wave < 1 ? Math.max(0, 1 - Math.abs(t - wave) * 5) : 0;
              const beat = 0.5 + 0.2 * Math.sin(time * 5 - t * 4);
              g.moveTo(vein[k - 1].x, vein[k - 1].y).lineTo(vein[k].x, vein[k].y).stroke({ width: 2 + pulse * 3, color: color(pulse > 0.3 ? '#ecfccb' : '#a3e635'), alpha: a * (beat + pulse * 0.5) });
            }
          }
          for (let n = fx.count(8 * a); n > 0; n--) mist.spawn({ x: u.center.x + rand(-30, 30), y: rand(u.head.y, u.feet.y), vx: 0, vy: -20, lifetime: 0.6, size: rand(5, 8) });
        },
        trigger() {
          trig = fx.now;
          u.hit(false, '#a3e635');
        },
      };
    },
  }),

  // ===== Шок =====
  status({
    id: 'sl-shock-arcs', name: 'Ползущие дуги', group: 'Эффект · Шок', spell: 'Shock',
    about: 'Висит: по телу то тут, то там проскакивают короткие дуги, юнит мерцает. Тик: по телу бьёт сноп разрядов',
    build(fx, u) {
      const h = bodyOf(u);
      const sparks = fx.sim(P.spark(C.bolt, 300));
      const flicker = fx.dot('glow', '#fde68a');
      const small = Array.from({ length: 3 }, () => new Beam(fx, 6, 2.2, '#fde68a'));
      const big = Array.from({ length: 4 }, () => new Beam(fx, 7, 3.5, '#fff7d6'));
      const busy = small.map(() => 0);
      const arc = () => {
        const y0 = u.feet.y - rand(0, h * 0.6);
        return jag({ x: u.center.x + rand(-34, 34), y: y0 }, { x: u.center.x + rand(-34, 34), y: y0 - rand(h * 0.2, h * 0.45) }, 6, 9);
      };
      return {
        frame(time, a) {
          flicker.oval(u.center.x, u.center.y, 56, h * 0.55, a * (Math.random() < 0.15 ? 0.45 : 0.12));
          if (a > 0.6 && Math.random() < fx.dt * 4) {
            const k = busy.findIndex(until => until <= time);
            if (k >= 0) {
              busy[k] = time + 0.14;
              crackle(fx, small[k], time, 0.14, arc);
            }
          }
        },
        trigger() {
          big.forEach((beam, n) => crackle(fx, beam, fx.now + n * 0.05, 0.25, arc));
          u.hit(false, '#fde68a');
          spray(sparks, { x: u.center.x, y: u.center.y, count: 18, speed: [120, 380], life: [0.2, 0.45], size: [2, 5] });
        },
      };
    },
  }),
  status({
    id: 'sl-shock-ring', name: 'Кольцо статики', group: 'Эффект · Шок', spell: 'Shock',
    about: 'Висит: вокруг пояса вращается потрескивающее кольцо с бегущими искрами. Тик: кольцо сжимается и бьёт в тело',
    build(fx, u) {
      const sparks = fx.sim(P.spark(C.bolt, 200));
      const ring = new Beam(fx, 24, 2.5, '#fde68a');
      const runners = Array.from({ length: 3 }, () => fx.dot('glow', '#fff7d6'));
      const c = { x: u.center.x, y: u.center.y + 10 };
      let next = 0;
      let pts: Pt[] = [];
      return {
        frame(time, a, surge) {
          const R = 62 * (1 - surge * 0.45);
          if (time >= next) {
            next = time + 0.05;
            pts = Array.from({ length: 24 }, (_, k) => polar(c, R + rand(-5, 5), (k / 23) * TAU + time * 3, 0.32));
          }
          ring.set(pts, a * (0.6 + 0.4 * surge));
          runners.forEach((dot, k) => {
            const ang = time * 7 + (k / 3) * TAU;
            const q = polar(c, R, ang, 0.32);
            dot.put(q.x, q.y, 9, a * (Math.sin(ang) < 0 ? 0.4 : 1));
          });
          const spot = polar(c, R, rand(0, TAU), 0.32);
          if (Math.random() < 0.12 * a) spray(sparks, { x: spot.x, y: spot.y, count: 3, speed: [40, 140], life: [0.15, 0.3], size: [2, 3] });
        },
        trigger() {
          const at = fx.now + 0.15;
          fx.at(at, () => {
            u.hit(false, '#fde68a');
            spray(sparks, { x: c.x, y: c.y, count: 16, speed: [100, 320], life: [0.2, 0.4], size: [2, 4] });
          });
          pop(fx, { at, dur: 0.25, x: c.x, y: c.y, r: 80, css: '#fff7d6' });
        },
      };
    },
  }),
  status({
    id: 'sl-shock-short', name: 'Короткое замыкание', group: 'Эффект · Шок', spell: 'Shock',
    about: 'Висит: из случайных точек тела с треском выбивает искры, как из коротнувшего механизма. Тик: яркий разряд в землю',
    build(fx, u) {
      const h = bodyOf(u);
      const sparks = fx.sim(P.spark(C.bolt, 700));
      const ground = new Beam(fx, 8, 4, '#fde68a');
      return {
        frame(_time, a) {
          if (Math.random() < fx.dt * 5 * a) {
            const spot = { x: u.center.x + rand(-30, 30), y: u.feet.y - rand(h * 0.2, h * 0.9) };
            spray(sparks, { x: spot.x, y: spot.y, count: 7, speed: [100, 300], dir: rand(0, TAU), spread: 0.5, life: [0.2, 0.4], size: [1.5, 3] });
            pop(fx, { at: fx.now, dur: 0.12, x: spot.x, y: spot.y, r: 26, css: '#fff7d6' });
          }
        },
        trigger() {
          crackle(fx, ground, fx.now, 0.3, () => jag(u.center, { x: u.feet.x + rand(-40, 40), y: u.feet.y + 6 }, 8, 14));
          ringWave(fx, { at: fx.now + 0.05, dur: 0.4, x: u.feet.x, y: u.feet.y, from: 10, to: 90, css: '#fde68a', squash: GROUND, layer: 'back' });
          u.hit(false, '#fde68a');
        },
      };
    },
  }),

  // ===== Мороз (замедление энергии) =====
  status({
    id: 'sl-frost-mist', name: 'Морозный пар', group: 'Эффект · Мороз', spell: 'Frost',
    about: 'Висит: у ног стелется холодный пар, сверху падают снежинки, изо рта идёт пар. Срабатывание (удар с замедленной энергией): холодная волна от тела',
    build(fx, u) {
      const mist = fx.sim({ texture: 'smoke', blend: 'normal', drag: 0.5, gravity: -10, shrink: 1.4, tint: palette(['#f0f9ff', '#e0f2fe', '#bae6fd']), alpha: life => bump(life) * 0.35 });
      const flakes = fx.sim({ texture: snowflakeTexture(), drag: 0.8, gravity: 60, shrink: 0.9, tint: palette(['#ffffff', '#e0f6ff']), alpha: life => bump(life) });
      const ground = fx.radial([[0, 0.5], [0.6, 0.25], [1, 0]], '#bae6fd', 'back');
      return {
        frame(time, a, surge) {
          ground.oval(u.feet.x, u.feet.y, 85 * (1 + surge * 0.3), 85 * GROUND * (1 + surge * 0.3), a * 0.7);
          for (let n = fx.count((16 + 60 * surge) * a); n > 0; n--) {
            const pt = polar(u.feet, rand(20, 70), rand(0, TAU), GROUND);
            mist.spawn({ x: pt.x, y: pt.y - 6, vx: rand(-20, 20), vy: rand(-8, 2), lifetime: rand(0.8, 1.2), size: rand(16, 26) });
          }
          for (let n = fx.count(7 * a); n > 0; n--) flakes.spawn({ x: u.center.x + rand(-50, 50), y: u.head.y - rand(30, 60), vx: rand(-15, 15), vy: rand(20, 50), lifetime: rand(1, 1.5), size: rand(4, 7), rotation: rand(0, TAU), spin: rand(-2, 2) });
          if (Math.sin(time * 4) > 0.95 && a > 0.5) mist.spawn({ x: u.head.x + 16, y: u.head.y + 20, vx: 40, vy: -10, lifetime: 0.6, size: 8 });
        },
        trigger() {
          ringWave(fx, { at: fx.now, dur: 0.5, x: u.center.x, y: u.center.y, from: 30, to: 100, css: '#bae6fd', alpha: 0.7 });
        },
      };
    },
  }),
  status({
    id: 'sl-frost-crust', name: 'Ледяная корка', group: 'Эффект · Мороз', spell: 'Frost',
    about: 'Висит: ноги схвачены коркой из мелких ледяных кристаллов с бликами. Срабатывание: кристаллы вспыхивают. Снятие: корка осыпается крошкой',
    build(fx, u) {
      const chips = fx.sim(CHIPS);
      const glints = fx.sim(P.star(['#ffffff', '#e0f6ff'], 0));
      const shards = Array.from({ length: 6 }, (_, k) => {
        const a = (k / 6) * TAU + rand(-0.2, 0.2);
        const base = polar(u.feet, rand(14, 30), a, GROUND);
        const layer = Math.sin(a) < 0 ? 'back' : 'front';
        const height = rand(36, 56);
        const body = fx.spike(iceFangTexture((k % 3) as 0 | 1 | 2), base.x, base.y, height, layer);
        const glow = fx.spike(iceFangTexture((k % 3) as 0 | 1 | 2), base.x, base.y, height, layer, 'add');
        body.sprite.rotation = glow.sprite.rotation = Math.cos(a) * 0.45;
        return { body, glow, base };
      });
      const sheen = fx.radial([[0, 0.55], [0.6, 0.3], [1, 0]], '#bae6fd', 'back');
      const breaking = fading();
      return {
        frame(time, a, surge) {
          const grow = ease.out3(clamp01(time / 0.3));
          sheen.oval(u.feet.x, u.feet.y, 70, 70 * GROUND, a * 0.8);
          shards.forEach(s => {
            s.body.sprite.scale.set(s.body.k, s.body.k * grow);
            s.glow.sprite.scale.set(s.glow.k, s.glow.k * grow);
            s.body.sprite.alpha = 0.85 * a;
            s.glow.sprite.alpha = a * surge * 0.8;
          });
          if (Math.random() < fx.count(5 * a)) glints.spawn({ x: u.feet.x + rand(-30, 30), y: u.feet.y - rand(0, 40), vx: 0, vy: 0, lifetime: 0.3, size: rand(5, 9) });
          if (breaking(time, a)) shards.forEach(s => spray(chips, { x: s.base.x, y: s.base.y - 20, count: 3, speed: [80, 240], dir: -Math.PI / 2, spread: 1.2, life: [0.4, 0.7], size: [5, 9], spin: 8 }));
        },
      };
    },
  }),
  status({
    id: 'sl-frost-orbit', name: 'Кружащие снежинки', group: 'Эффект · Мороз', spell: 'Frost',
    about: 'Висит: вокруг тела медленно-медленно кружат снежинки — образ замедления. Срабатывание: снежинки на миг замирают и вспыхивают',
    build(fx, u) {
      const h = bodyOf(u);
      const trail = fx.sim(P.glow(C.ice, 0));
      const flake = snowflakeTexture();
      const list = Array.from({ length: 5 }, (_, k) => ({ front: fx.tex(flake, 'front', 'add', '#e0f6ff'), back: fx.tex(flake, 'back', 'add', '#e0f6ff'), a: (k / 5) * TAU, y: lerp(0.2, 0.85, k / 4) }));
      let spin = 0;
      return {
        frame(_time, a, surge) {
          spin += fx.dt * 1.1 * (1 - surge);
          for (const s of list) {
            const ang = s.a + spin;
            const pt = polar({ x: u.center.x, y: u.feet.y - h * s.y }, 55, ang, 0.3);
            const behind = Math.sin(ang) < 0;
            const [show, hide] = behind ? [s.back, s.front] : [s.front, s.back];
            hide.hide();
            show.put(pt.x, pt.y, 11 * (1 + surge * 0.5), a * (behind ? 0.55 : 1), spin * 2 + s.a);
            if (Math.random() < 0.3 * a) trail.spawn({ x: pt.x, y: pt.y, vx: 0, vy: 5, lifetime: 0.5, size: 5 });
          }
        },
      };
    },
  }),

  // ===== Обледенение (пропуск хода) =====
  status({
    id: 'sl-freeze-block', name: 'Ледяная глыба', group: 'Эффект · Обледенение', spell: 'Freeze',
    about: 'Висит: юнит в гранёной глыбе льда с бегущим бликом. Срабатывание (ход пропущен): по льду бежит трещина. Снятие: глыба раскалывается',
    build(fx, u) {
      const h = bodyOf(u);
      const chips = fx.sim(CHIPS);
      const W = 62;
      const top = u.head.y - 20;
      const bottom = u.feet.y + 10;
      const block = fx.gfx('front', 'normal');
      const shine = fx.gfx('front', 'add');
      let crack: Pt[] = [];
      let trig = -10;
      const breaking = fading();
      return {
        frame(time, a) {
          const grow = ease.out3(clamp01(time / 0.35));
          const y0 = lerp(bottom, top, grow);
          block.clear();
          shine.clear();
          if (breaking(time, a)) spray(chips, { x: u.center.x, y: u.center.y, count: 26, speed: [150, 460], life: [0.5, 0.9], size: [7, 14], jitter: h * 0.3, spin: 8 });
          if (a < 0.02) return;
          const poly = [u.center.x - W, bottom, u.center.x - W - 8, lerp(bottom, y0, 0.6), u.center.x - W * 0.5, y0, u.center.x + W * 0.6, y0 + 8, u.center.x + W + 6, lerp(bottom, y0, 0.55), u.center.x + W, bottom];
          block.poly(poly).fill({ color: color('#bae6fd'), alpha: 0.35 * a }).stroke({ width: 2, color: color('#e0f6ff'), alpha: 0.8 * a });
          block.moveTo(u.center.x - W * 0.5, y0).lineTo(u.center.x - W * 0.2, lerp(bottom, y0, 0.5)).lineTo(u.center.x - W, bottom).stroke({ width: 1.2, color: color('#e0f6ff'), alpha: 0.5 * a });
          const sweep = (time * 0.6) % 1;
          const sx = lerp(u.center.x - W, u.center.x + W, sweep);
          shine.moveTo(sx - 10, bottom - 10).lineTo(sx + 20, y0 + 10).stroke({ width: 6, color: color('#ffffff'), alpha: 0.35 * a * bump(sweep) });
          const cracked = clamp01((time - trig) / 0.2);
          if (crack.length && cracked > 0) {
            const pts = crack.slice(0, Math.max(2, Math.ceil(crack.length * cracked)));
            shine.moveTo(pts[0].x, pts[0].y);
            pts.slice(1).forEach(pt => shine.lineTo(pt.x, pt.y));
            shine.stroke({ width: 2, color: color('#ffffff'), alpha: a * (1 - clamp01((time - trig - 1.2) / 0.5)) });
          }
        },
        trigger() {
          trig = fx.now;
          crack = jag({ x: u.center.x + rand(-30, 0), y: top + 10 }, { x: u.center.x + rand(0, 30), y: bottom - 20 }, 8, 14);
          u.hit(false, '#7dd3fc');
        },
      };
    },
  }),
  status({
    id: 'sl-freeze-fangs', name: 'Ледяная клетка', group: 'Эффект · Обледенение', spell: 'Freeze',
    about: 'Висит: вокруг юнита сомкнулись ледяные клыки, тело глазировано инеем. Срабатывание: вспышка холода. Снятие: клыки рассыпаются',
    build(fx, u) {
      const h = bodyOf(u);
      const chips = fx.sim(CHIPS);
      const glaze = fx.dot('glow', '#e0f6ff', 'front', 'normal');
      const fangs = Array.from({ length: 7 }, (_, k) => {
        const a = (k / 7) * TAU + 0.3;
        const base = polar(u.feet, rand(32, 46), a, 0.36);
        const behind = Math.sin(a) < 0;
        const spike = fx.spike(iceFangTexture((k % 3) as 0 | 1 | 2), base.x, base.y, h * rand(0.75, 1) * (behind ? 1.1 : 0.8), behind ? 'back' : 'front');
        spike.sprite.rotation = -Math.cos(a) * 0.3;
        return { ...spike, base, delay: k * 0.03 };
      });
      const breaking = fading();
      let ringed = false;
      return {
        frame(time, a) {
          if (!ringed) {
            ringed = true;
            ringWave(fx, { at: fx.now, dur: 0.45, x: u.feet.x, y: u.feet.y, from: 20, to: 120, css: '#e0f6ff', squash: GROUND, layer: 'back' });
          }
          glaze.oval(u.center.x, u.center.y + 10, 66, h * 0.62, a * 0.35);
          fangs.forEach(f => {
            f.sprite.scale.set(f.k, f.k * ease.out3(clamp01((time - f.delay) / 0.16)));
            f.sprite.alpha = 0.85 * a;
          });
          if (breaking(time, a)) fangs.forEach(f => spray(chips, { x: f.base.x, y: f.base.y - 30, count: 4, speed: [120, 360], life: [0.4, 0.8], size: [6, 12], align: true, spin: 8 }));
        },
        trigger() {
          u.hit(false, '#7dd3fc');
          pop(fx, { at: fx.now, dur: 0.3, x: u.center.x, y: u.center.y, r: 80, css: '#bae6fd' });
        },
      };
    },
  }),
  status({
    id: 'sl-freeze-statue', name: 'Ледяная статуя', group: 'Эффект · Обледенение', spell: 'Freeze',
    about: 'Висит: юнит бледно-голубая статуя в снежной пыли (стужа поднялась от ног до макушки). Срабатывание: холодное кольцо. Снятие: лёд стекает каплями',
    build(fx, u) {
      const h = bodyOf(u);
      const dust = fx.sim(P.star(['#ffffff', '#e0f6ff', '#bae6fd'], 20));
      const melt = fx.sim({ texture: 'disc', drag: 0.1, gravity: 600, shrink: 0.8, tint: palette(['#e0f6ff', '#7dd3fc']), alpha: life => life });
      const frost = fx.dot('glow', '#e0f6ff', 'front', 'normal');
      const rim = fx.dot('glow', '#7dd3fc');
      let last = 0;
      return {
        frame(time, a) {
          const rise = ease.out(clamp01(time / 0.45));
          const top = lerp(u.feet.y, u.head.y - 10, rise);
          const cy = (u.feet.y + top) / 2;
          const ry = Math.max(6, (u.feet.y - top) / 2 + 12);
          frost.oval(u.center.x, cy, 52, ry, a * 0.5);
          rim.oval(u.center.x, cy, 64, ry + 10, a * (0.25 + 0.05 * Math.sin(time * 5)));
          if (Math.random() < fx.count(10 * a)) dust.spawn({ x: u.center.x + rand(-45, 45), y: rand(top, u.feet.y), vx: rand(-10, 10), vy: -10, lifetime: rand(0.4, 0.7), size: rand(4, 8) });
          if (a < last && Math.random() < 0.6) melt.spawn({ x: u.center.x + rand(-35, 35), y: rand(u.center.y, u.feet.y), vx: 0, vy: 40, lifetime: 0.4, size: 3 });
          last = a;
        },
        trigger() {
          u.hit(false, '#7dd3fc');
          ringWave(fx, { at: fx.now, dur: 0.4, x: u.center.x, y: u.center.y, from: 30, to: h * 0.7, css: '#e0f6ff', alpha: 0.7 });
        },
      };
    },
  }),

  // ===== Оглушение =====
  status({
    id: 'sl-stun-stars', name: 'Звёздочки', group: 'Эффект · Оглушение', spell: 'Stun',
    about: 'Висит: над головой по кругу носятся золотые звёздочки. Срабатывание (ход пропущен): звёзды вспыхивают и сыплют искры',
    build(fx, u) {
      const dust = fx.sim(P.star(C.light, 30));
      const stars = Array.from({ length: 4 }, (_, k) => ({ front: fx.dot('star5', '#fde047'), back: fx.dot('star5', '#fde047', 'back'), a: (k / 4) * TAU }));
      return {
        frame(time, a, surge) {
          const c = { x: u.head.x, y: u.head.y - 16 };
          for (const s of stars) {
            const ang = s.a + time * 5;
            const pt = polar(c, 34, ang, 0.32);
            const behind = Math.sin(ang) < 0;
            const [show, hide] = behind ? [s.back, s.front] : [s.front, s.back];
            hide.hide();
            show.put(pt.x, pt.y, (11 + surge * 6) * (behind ? 0.8 : 1), a, time * 4);
          }
          if (surge > 0.5 && Math.random() < 0.5) dust.spawn({ x: c.x + rand(-30, 30), y: c.y + rand(-10, 10), vx: rand(-40, 40), vy: -40, lifetime: 0.5, size: rand(4, 7) });
        },
        trigger: () => u.hit(false, '#fde047'),
      };
    },
  }),
  status({
    id: 'sl-stun-spiral', name: 'Спираль', group: 'Эффект · Оглушение', spell: 'Stun',
    about: 'Висит: над головой крутится светящаяся спираль-водоворот, как в мультиках. Срабатывание: спираль раскручивается шире',
    build(fx, u) {
      const g = fx.gfx('front', 'add');
      return {
        frame(time, a, surge) {
          const c = { x: u.head.x, y: u.head.y - 22 };
          g.clear();
          if (a < 0.02) return;
          for (const [css, width, share] of [['#c4b5fd', 7, 0.35], ['#ede9fe', 2.5, 1]] as const) {
            const pts: Pt[] = [];
            for (let k = 0; k <= 40; k++) {
              const t = k / 40;
              pts.push(polar(c, (4 + 30 * t) * (1 + surge * 0.4), t * TAU * 2.2 - time * 6, 0.38));
            }
            g.moveTo(pts[0].x, pts[0].y);
            pts.slice(1).forEach(pt => g.lineTo(pt.x, pt.y));
            g.stroke({ width, color: color(css), alpha: a * share });
          }
        },
        trigger: () => u.hit(false, '#c4b5fd'),
      };
    },
  }),
  status({
    id: 'sl-stun-bell', name: 'Звон в ушах', group: 'Эффект · Оглушение', spell: 'Stun',
    about: 'Висит: от головы расходятся тусклые звуковые кольца, словно после удара по шлему, сверху сыплются искорки. Срабатывание: громкий «бом»',
    build(fx, u) {
      const motes = fx.sim(P.star(['#fef9c3', '#fde047', '#ca8a04'], -40));
      const c = { x: u.head.x, y: u.head.y + 10 };
      let next = 0;
      return {
        frame(time, a) {
          if (time >= next && a > 0.5) {
            next = time + 0.45;
            ringWave(fx, { at: time, dur: 0.7, x: c.x, y: c.y, from: 20, to: 70, css: '#fde68a', alpha: 0.5 * a });
          }
          if (Math.random() < fx.count(6 * a)) motes.spawn({ x: c.x + rand(-30, 30), y: c.y - 50, vx: rand(-10, 10), vy: 30, lifetime: 0.7, size: rand(4, 7) });
        },
        trigger() {
          ringWave(fx, { at: fx.now, dur: 0.5, x: c.x, y: c.y, from: 20, to: 120, css: '#fef9c3' });
          pop(fx, { at: fx.now, dur: 0.25, x: c.x, y: c.y, r: 70, css: '#fde047' });
          u.hit(false, '#fde047');
        },
      };
    },
  }),

  // ===== Проклятие =====
  status({
    id: 'sl-curse-sigil', name: 'Печать проклятия', group: 'Эффект · Проклятие', spell: 'Curse',
    about: 'Висит: над головой медленно вращается фиолетовая печать с рунами, из неё в тело сочится тьма. Тик: печать вспыхивает и выжигает энергию',
    build(fx, u) {
      const motes = fx.sim({ ...P.glow(ENERGY, 80), tint: palette(ENERGY) });
      const ooze = fx.sim({ texture: 'smoke', blend: 'normal', drag: 0.3, gravity: 60, shrink: 1.2, tint: palette(['#3b0764', '#1e0b36']), alpha: life => bump(life) * 0.45 });
      const g = fx.gfx('front', 'add');
      return {
        frame(time, a, surge) {
          const c = { x: u.head.x, y: u.head.y - 40 };
          const R = 30 * (1 + surge * 0.25);
          g.clear();
          if (a < 0.02) return;
          g.ellipse(c.x, c.y, R, R * 0.35).stroke({ width: 2.5, color: color('#c084fc'), alpha: a * (0.8 + surge * 0.2) });
          g.ellipse(c.x, c.y, R * 0.75, R * 0.26).stroke({ width: 1.2, color: color('#e9d5ff'), alpha: a * 0.7 });
          for (let k = 0; k < 6; k++) {
            const ang = (k / 6) * TAU + time * 1.4;
            const pt = polar(c, R * 0.87, ang, 0.35);
            g.poly([pt.x, pt.y - 5, pt.x + 3, pt.y, pt.x, pt.y + 5, pt.x - 3, pt.y]).fill({ color: color('#f5d0fe'), alpha: a * (Math.sin(ang) < 0 ? 0.4 : 1) });
          }
          for (let n = fx.count(10 * a); n > 0; n--) ooze.spawn({ x: c.x + rand(-16, 16), y: c.y + 6, vx: 0, vy: rand(30, 60), lifetime: rand(0.5, 0.8), size: rand(8, 14) });
        },
        trigger() {
          u.hit(false, '#a855f7');
          spray(motes, { x: u.center.x, y: u.center.y, count: 12, speed: [60, 200], dir: -Math.PI / 2, spread: 1, life: [0.4, 0.8], size: [5, 9] });
        },
      };
    },
  }),
  status({
    id: 'sl-curse-chains', name: 'Призрачные цепи', group: 'Эффект · Проклятие', spell: 'Curse',
    about: 'Висит: тело по спирали обвивают две светящиеся фиолетовые цепи и ползут навстречу друг другу (за спиной тусклее). Тик: цепи рывком затягиваются и вспыхивают',
    build(fx, u) {
      const h = bodyOf(u);
      const front = fx.gfx('front', 'add');
      const back = fx.gfx('back', 'add');
      const motes = fx.sim(P.glow(C.dark, 20));
      const LINKS = 16;
      return {
        frame(time, a, surge) {
          front.clear();
          back.clear();
          if (a < 0.02) return;
          const R = 54 * (1 - surge * 0.3);
          for (let c = 0; c < 2; c++) {
            const dir = c ? -1 : 1;
            // Спираль: высота звена меняется по кругу, цепи идут навстречу друг другу
            const at = (ang: number) => polar({ x: u.center.x, y: u.feet.y - h * (0.5 + 0.18 * dir * Math.cos(ang)) + (c ? 14 : -14) }, R, ang, 0.32);
            // Тонкая нить через звенья — чтобы звенья читались цепью
            for (let k = 0; k < LINKS * 2; k++) {
              const a0 = (k / (LINKS * 2)) * TAU + time * 1.5 * dir;
              const a1 = ((k + 1) / (LINKS * 2)) * TAU + time * 1.5 * dir;
              const p0 = at(a0);
              const p1 = at(a1);
              const behind = Math.sin((a0 + a1) / 2) < 0;
              (behind ? back : front).moveTo(p0.x, p0.y).lineTo(p1.x, p1.y).stroke({ width: 1.5, color: color('#a855f7'), alpha: a * (behind ? 0.25 : 0.6) });
            }
            for (let k = 0; k < LINKS; k++) {
              const ang = (k / LINKS) * TAU + time * 1.5 * dir;
              const pt = at(ang);
              const behind = Math.sin(ang) < 0;
              const g = behind ? back : front;
              // Звенья через одно повёрнуты ребром
              const w = k % 2 ? 3.6 : 8;
              const alpha = a * (behind ? 0.4 : 1);
              g.ellipse(pt.x, pt.y, w, 5).stroke({ width: 6, color: color('#7e22ce'), alpha: alpha * (0.35 + surge * 0.4) });
              g.ellipse(pt.x, pt.y, w, 5).stroke({ width: 2.2, color: color(surge > 0.3 ? '#f5d0fe' : '#c084fc'), alpha });
            }
          }
          if (Math.random() < 6 * fx.dt * a) motes.spawn({ x: u.center.x + rand(-50, 50), y: rand(u.head.y, u.feet.y), vx: 0, vy: -20, lifetime: 0.6, size: rand(4, 7) });
        },
        trigger() {
          u.hit(false, '#a855f7');
          pop(fx, { at: fx.now, dur: 0.3, x: u.center.x, y: u.center.y, r: 80, css: '#a855f7', alpha: 0.6 });
        },
      };
    },
  }),
  status({
    id: 'sl-curse-shadow', name: 'Тень-паразит', group: 'Эффект · Проклятие', spell: 'Curse',
    about: 'Висит: из чёрной лужи под ногами поднимаются фиолетовые клубы, над плечом колышется тёмный сгусток с горящими глазами и моргает. Тик: сгусток тянется к голове и высасывает голубые искры энергии',
    build(fx, u) {
      const h = bodyOf(u);
      const smoke = fx.sim({ texture: 'smoke', blend: 'normal', drag: 0.3, gravity: -70, shrink: 1.3, tint: palette(['#3b0764', '#1e0b36', '#09030f']), alpha: life => bump(life) * 0.55 });
      const wisps = fx.sim({ texture: 'smoke', drag: 0.3, gravity: -60, shrink: 1.2, tint: palette(['#c084fc', '#9333ea', '#3b0764']), alpha: life => bump(life) * 0.35 });
      const motes = fx.sim({ ...P.glow(ENERGY, 0), tint: palette(ENERGY) });
      const pool = fx.radial([[0, 0.9], [0.6, 0.6], [1, 0]], '#09030f', 'back', 'normal');
      const poolRim = fx.dot('ring', '#9333ea', 'back');
      const blob = fx.dot('glow', '#1e0b36', 'front', 'normal');
      const blobGlow = fx.dot('glow', '#7e22ce');
      const eyes = fx.gfx('front', 'normal');
      return {
        frame(time, a, surge) {
          const r = 80 + Math.sin(time * 3) * 4;
          pool.oval(u.feet.x, u.feet.y, 85, 85 * GROUND, a * 0.85);
          poolRim.oval(u.feet.x, u.feet.y, r, r * GROUND, a * 0.6);
          for (let n = fx.count(18 * a); n > 0; n--) {
            const pt = polar(u.feet, rand(0, 60), rand(0, TAU), GROUND);
            smoke.spawn({ x: pt.x, y: pt.y, vx: rand(-10, 10), vy: rand(-80, -40), lifetime: rand(0.8, 1.2), size: rand(14, 24) });
            if (Math.random() < 0.5) wisps.spawn({ x: pt.x, y: pt.y, vx: rand(-10, 10), vy: rand(-90, -50), lifetime: rand(0.6, 1), size: rand(10, 18) });
          }
          // Сгусток над плечом колышется, на тике тянется к голове
          const c = { x: u.head.x + 40 - surge * 20 + Math.sin(time * 1.7) * 6, y: u.head.y + 4 - Math.cos(time * 1.3) * 5 };
          blob.oval(c.x, c.y, 34 * (1 + surge * 0.3), 28, a * 0.85);
          blobGlow.oval(c.x, c.y, 46, 36, a * (0.35 + 0.1 * Math.sin(time * 4) + 0.4 * surge));
          for (let n = fx.count(10 * a); n > 0; n--) smoke.spawn({ x: c.x + rand(-16, 16), y: c.y + rand(-10, 10), vx: rand(-10, 10), vy: -25, lifetime: 0.7, size: rand(10, 16) });
          const blink = time % 2.8 > 2.68 ? 0.1 : 1;
          eyes.clear();
          for (const side of [-1, 1]) angryEye(eyes, c.x + side * 11, c.y - 2, side, 8, 8, blink * clamp01(time / 0.4) * (1 + surge * 0.4), '#c084fc', a);
          for (let n = fx.count(120 * surge * a); n > 0; n--) {
            const from = { x: u.center.x + rand(-30, 30), y: u.center.y + rand(-h * 0.3, h * 0.2) };
            motes.spawn({ x: from.x, y: from.y, vx: (c.x - from.x) * 2.4, vy: (c.y - from.y) * 2.4, lifetime: 0.4, size: rand(5, 8) });
          }
        },
        trigger: () => u.hit(false, '#a855f7'),
      };
    },
  }),

  // ===== Кровотечение =====
  status({
    id: 'sl-bleed-drips', name: 'Капающая кровь', group: 'Эффект · Кровотечение', spell: 'Bleed',
    about: 'Висит: с тела срываются капли крови, под ногами растёт тёмная лужица, тело слабо пульсирует красным. Тик: капли брызгают струёй',
    build(fx, u) {
      const h = bodyOf(u);
      const drops = fx.sim({ texture: dropTexture('blood'), blend: 'normal', drag: 0.2, gravity: 1200, shrink: 0.85 });
      const pool = fx.radial([[0, 0.95], [0.7, 0.75], [1, 0]], '#450a0a', 'back', 'normal');
      const sheen = fx.dot('glow', '#dc2626');
      return {
        frame(time, a) {
          const r = 70 * ease.out(clamp01(time / 1.5));
          pool.oval(u.feet.x, u.feet.y, r, r * GROUND, a * 0.75);
          sheen.oval(u.center.x, u.center.y, 50, h * 0.5, a * (0.12 + 0.1 * Math.pow(Math.max(0, Math.sin(time * 6)), 4)));
          for (let n = fx.count(12 * a); n > 0; n--) drops.spawn({ x: u.center.x + rand(-26, 26), y: u.feet.y - rand(h * 0.3, h * 0.7), vx: rand(-20, 20), vy: rand(0, 40), lifetime: rand(0.3, 0.5), size: rand(6, 9), rotation: Math.PI / 2 });
        },
        trigger() {
          u.hit(false, '#dc2626');
          spray(drops, { x: u.center.x, y: u.center.y, count: 12, speed: [150, 380], dir: -Math.PI / 2, spread: 1.4, life: [0.4, 0.7], size: [5, 8], align: true });
        },
      };
    },
  }),
  status({
    id: 'sl-bleed-wounds', name: 'Открытые раны', group: 'Эффект · Кровотечение', spell: 'Bleed',
    about: 'Висит: на теле алеют порезы (по одному на стак), пульсируют в такт сердцу. Тик: самый свежий порез затягивается — стак снят, новый открывается от следующей ауры',
    build(fx, u) {
      const h = bodyOf(u);
      const drops = fx.sim({ texture: dropTexture('blood'), blend: 'normal', drag: 0.3, gravity: 1100, shrink: 0.8 });
      const cuts = Array.from({ length: 3 }, (_, k) => {
        const c = { x: u.center.x + rand(-18, 18), y: u.feet.y - h * lerp(0.3, 0.75, k / 2) };
        const ang = rand(-1, -0.5) * (k % 2 ? -1 : 1);
        return { pts: Array.from({ length: 7 }, (_, n) => polar(c, lerp(-20, 20, n / 6), ang)), c, born: k * 0.18 };
      });
      const g = fx.gfx('front', 'normal');
      const glow = fx.gfx('front', 'add');
      let trig = -10;
      return {
        frame(time, a) {
          const beat = Math.pow(Math.max(0, Math.sin(time * 6)), 6);
          g.clear();
          glow.clear();
          if (a < 0.02) return;
          cuts.forEach((cut, k) => {
            const open = ease.out3(clamp01((time - cut.born) / 0.15));
            const since = time - trig;
            const healed = k !== cuts.length - 1 ? 1 : since < 0.35 ? 1 - since / 0.35 : since < 0.9 ? 0 : ease.out3(clamp01((since - 0.9) / 0.15));
            const w = 7 * open * healed;
            if (w < 0.3) return;
            taper(g, cut.pts, w + beat * 2, '#7f1d1d', a);
            taper(glow, cut.pts, (w + beat * 2) * 0.5, '#f87171', a * (0.6 + beat * 0.4));
          });
        },
        trigger() {
          trig = fx.now;
          u.hit(false, '#dc2626');
          const last = cuts[cuts.length - 1].c;
          spray(drops, { x: last.x, y: last.y, count: 8, speed: [120, 300], dir: -Math.PI / 2, spread: 1.3, life: [0.4, 0.7], size: [4, 7], align: true });
        },
      };
    },
  }),
  status({
    id: 'sl-bleed-pulse', name: 'Пульс крови', group: 'Эффект · Кровотечение', spell: 'Bleed',
    about: 'Висит: вокруг тела бьётся алый пульс «тук-тук», с каждым ударом выплёскивается струйка. Тик: пульс сбивается и бьёт сильнее',
    build(fx, u) {
      const h = bodyOf(u);
      const drops = fx.sim({ texture: dropTexture('blood'), blend: 'normal', drag: 0.3, gravity: 1100, shrink: 0.8 });
      const ground = fx.radial([[0, 0.7], [0.5, 0.35], [1, 0]], '#991b1b', 'back');
      const beat = (strong: boolean) => {
        const ring = fx.dot('ring', strong ? '#f87171' : '#dc2626');
        const k = strong ? 1.3 : 1;
        fx.tween(fx.now, 0.35, p => ring.oval(u.center.x, u.center.y, 50 * (0.8 + 0.5 * ease.out(p)) * k, h * 0.5 * (0.8 + 0.5 * ease.out(p)), (1 - p) * (strong ? 1 : 0.6)), [ring]);
        spray(drops, { x: u.center.x + rand(-15, 15), y: u.center.y, count: strong ? 10 : 2, speed: [120, 280], dir: -Math.PI / 2 + rand(-0.6, 0.6), spread: 0.4, life: [0.35, 0.6], size: [4, 7], align: true });
      };
      let next = 0.1;
      let second = false;
      return {
        frame(time, a) {
          ground.oval(u.feet.x, u.feet.y, 70, 70 * GROUND, a * 0.7);
          if (a > 0.5 && time >= next) {
            beat(false);
            next = time + (second ? 0.42 : 0.18);
            second = !second;
          }
        },
        trigger() {
          beat(true);
          u.hit(false, '#dc2626');
        },
      };
    },
  }),

  // ===== Метка =====
  status({
    id: 'sl-mark-crosshair', name: 'Прицел', group: 'Эффект · Метка', spell: 'Mark',
    about: 'Висит: на теле сошёлся и медленно вращается красно-золотой прицел. Срабатывание (заклинание попало по метке): прицел сжимается вспышкой',
    build(fx, u) {
      const g = fx.gfx('front', 'add');
      return {
        frame(time, a, surge) {
          const lock = ease.out3(clamp01(time / 0.35));
          const R = lerp(90, 34, lock) * (1 - surge * 0.3);
          const c = u.center;
          g.clear();
          if (a < 0.02) return;
          g.circle(c.x, c.y, R).stroke({ width: 2 + surge * 2, color: color('#fca5a5'), alpha: a * 0.9 });
          g.circle(c.x, c.y, R * 0.45).stroke({ width: 1.5, color: color('#fde68a'), alpha: a * 0.8 });
          for (let k = 0; k < 4; k++) {
            const ang = time * 0.8 + (k * Math.PI) / 2;
            const p1 = polar(c, R * 0.7, ang);
            const p2 = polar(c, R * 1.3, ang);
            g.moveTo(p1.x, p1.y).lineTo(p2.x, p2.y).stroke({ width: 3, color: color('#f87171'), alpha: a });
          }
          g.circle(c.x, c.y, 3 + surge * 4).fill({ color: color('#ffffff'), alpha: a * (0.6 + 0.4 * Math.sin(time * 8)) });
        },
        trigger() {
          u.hit(true, '#f87171');
          pop(fx, { at: fx.now, dur: 0.3, x: u.center.x, y: u.center.y, r: 70, css: '#fca5a5' });
        },
      };
    },
  }),
  status({
    id: 'sl-mark-rune', name: 'Руна уязвимости', group: 'Эффект · Метка', spell: 'Mark',
    about: 'Висит: над головой пульсирует ромб-руна, тонкая нить света тянется к телу (при наложении руна падает сверху). Срабатывание: руна бьёт лучом в тело',
    build(fx, u) {
      const g = fx.gfx('front', 'add');
      const glow = fx.dot('glow', '#f59e0b');
      return {
        frame(time, a, surge) {
          const drop = ease.back(clamp01(time / 0.35));
          const c = { x: u.head.x, y: lerp(u.head.y - 140, u.head.y - 38, drop) };
          const s = (1 + 0.12 * Math.sin(time * 5)) * (1 + surge * 0.3);
          g.clear();
          glow.put(c.x, c.y, 34 * s, a * (0.5 + surge * 0.4));
          if (a < 0.02) return;
          g.poly([c.x, c.y - 16 * s, c.x + 11 * s, c.y, c.x, c.y + 16 * s, c.x - 11 * s, c.y]).fill({ color: color('#fde68a'), alpha: a * 0.85 }).stroke({ width: 2, color: color('#fff7d6'), alpha: a });
          g.poly([c.x, c.y - 7 * s, c.x + 4 * s, c.y, c.x, c.y + 7 * s, c.x - 4 * s, c.y]).fill({ color: color('#b45309'), alpha: a });
          g.moveTo(c.x, c.y + 16 * s).lineTo(u.center.x, u.center.y).stroke({ width: 1.2 + surge * 5, color: color('#fde68a'), alpha: a * (0.35 + surge * 0.6) });
        },
        trigger: () => u.hit(true, '#fbbf24'),
      };
    },
  }),
  status({
    id: 'sl-mark-target', name: 'Мишень на земле', group: 'Эффект · Метка', spell: 'Mark',
    about: 'Висит: под ногами проступает круг-мишень из колец и засечек, сверху цель подсвечивает узкий луч, как прожектором. Срабатывание: луч вспыхивает',
    build(fx, u) {
      const g = fx.gfx('back', 'add');
      const beam = fx.dot('glow', '#fca5a5');
      return {
        frame(time, a, surge) {
          const R = 80 * ease.out3(clamp01(time / 0.3));
          const c = u.feet;
          beam.oval(u.center.x, u.feet.y - 200, 34 * (1 + surge * 0.4), 220, a * (0.22 + 0.05 * Math.sin(time * 4) + 0.4 * surge));
          g.clear();
          if (a < 0.02) return;
          for (const [r, w] of [[1, 2.5], [0.62, 1.5], [0.28, 2]] as const) g.ellipse(c.x, c.y, R * r, R * r * GROUND).stroke({ width: w, color: color('#f87171'), alpha: a * 0.85 });
          for (let k = 0; k < 8; k++) {
            const ang = (k / 8) * TAU + time * 0.5;
            const p1 = polar(c, R * 0.9, ang, GROUND);
            const p2 = polar(c, R * 1.1, ang, GROUND);
            g.moveTo(p1.x, p1.y).lineTo(p2.x, p2.y).stroke({ width: 2, color: color('#fde68a'), alpha: a });
          }
        },
        trigger: () => u.hit(true, '#f87171'),
      };
    },
  }),

  // ===== Регенерация =====
  status({
    id: 'sl-regen-pluses', name: 'Плюсики', group: 'Эффект · Регенерация', spell: 'Regen', side: 'allies',
    about: 'Висит: тело мягко светится, вверх неспешно поднимаются зелёно-золотые плюсики. Тик: тёплая вспышка и пачка плюсов',
    build(fx, u) {
      const h = bodyOf(u);
      const pluses = fx.sim(P.plus(['#f7fee7', '#bef264', '#4ade80'], 70));
      const glow = fx.dot('glow', '#bef264');
      return {
        frame(time, a, surge) {
          glow.oval(u.center.x, u.center.y, 56, h * 0.58, a * (0.2 + 0.06 * Math.sin(time * 3) + 0.35 * surge));
          for (let n = fx.count((3 + 30 * surge) * a); n > 0; n--) pluses.spawn({ x: u.center.x + rand(-36, 36), y: rand(u.center.y, u.feet.y - 10), vx: 0, vy: rand(-50, -25), lifetime: rand(0.8, 1.2), size: rand(6, 10) });
        },
        trigger: () => u.heal(),
      };
    },
  }),
  status({
    id: 'sl-regen-leaves', name: 'Вьющиеся листья', group: 'Эффект · Регенерация', spell: 'Regen', side: 'allies',
    about: 'Висит: вокруг тела по спирали вверх вьются листочки. Тик: зелёная вспышка, листья осыпаются искрами',
    build(fx, u) {
      const h = bodyOf(u);
      const spores = fx.sim(P.glow(C.nature, 60));
      const leaf = leafTexture();
      const leaves = Array.from({ length: 4 }, (_, k) => ({ front: fx.tex(leaf), back: fx.tex(leaf, 'back'), phase: k / 4 }));
      return {
        frame(time, a) {
          for (const l of leaves) {
            const t = (time * 0.4 + l.phase) % 1;
            const ang = t * TAU * 2 + l.phase * TAU;
            const pt = polar({ x: u.center.x, y: u.feet.y - h * t }, 48 - 10 * t, ang, 0.3);
            const behind = Math.sin(ang) < 0;
            const [show, hide] = behind ? [l.back, l.front] : [l.front, l.back];
            hide.hide();
            show.put(pt.x, pt.y, 11, a * bump(t) * (behind ? 0.6 : 1), ang + Math.PI / 2);
          }
        },
        trigger() {
          u.heal();
          spray(spores, { x: u.center.x, y: u.center.y, count: 14, speed: [40, 140], life: [0.5, 0.9], size: [6, 10], jitter: 30 });
        },
      };
    },
  }),
  status({
    id: 'sl-regen-halo', name: 'Тёплый ореол', group: 'Эффект · Регенерация', spell: 'Regen', side: 'allies',
    about: 'Висит: под ногами золотой круг, от него вверх тонкие лучи-нити, над головой бледный нимб. Тик: свет поднимается волной от ног к голове',
    build(fx, u) {
      const h = bodyOf(u);
      const motes = fx.sim(P.star(C.light, 50));
      const ground = fx.radial([[0, 0.6], [0.6, 0.3], [1, 0]], '#fde68a', 'back');
      const nimbus = fx.dot('ring', '#fde68a');
      const rays = fx.gfx('front', 'add');
      let trig = -10;
      return {
        frame(time, a) {
          const wave = clamp01((time - trig) / 0.5);
          ground.oval(u.feet.x, u.feet.y, 70, 70 * GROUND, a * 0.8);
          nimbus.oval(u.head.x, u.head.y - 16 + Math.sin(time * 2) * 2, 22, 7, a * 0.8);
          rays.clear();
          if (a > 0.02) {
            for (let k = 0; k < 6; k++) {
              const ang = (k / 6) * TAU + time * 0.6;
              const base = polar(u.feet, 50, ang, GROUND);
              const len = h * (0.5 + 0.2 * Math.sin(time * 3 + k)) * (1 + bump(wave) * 0.5);
              rays.moveTo(base.x, base.y).lineTo(base.x, base.y - len).stroke({ width: 2, color: color('#fde68a'), alpha: a * (Math.sin(ang) < 0 ? 0.25 : 0.5) });
            }
          }
          if (wave > 0 && wave < 1 && Math.random() < 0.6) motes.spawn({ x: u.center.x + rand(-30, 30), y: lerp(u.feet.y, u.head.y, wave), vx: 0, vy: -30, lifetime: 0.6, size: rand(5, 8) });
        },
        trigger() {
          trig = fx.now;
          u.heal();
        },
      };
    },
  }),

  // ===== Выжигание энергии (мгновенное, статусом не висит) =====
  status({
    id: 'sl-eburn-flare', name: 'Сгорание энергии', group: 'Эффект · Выжигание энергии', spell: 'EnergyBurn', instant: true,
    about: 'Мгновенно, не висит: из тела вырывается голубое пламя энергии, догорает фиолетовым и оседает чёрным пеплом',
    build(fx, u) {
      const flame = fx.sim({ texture: 'smoke', drag: 0.2, gravity: -360, shrink: 0.3, stretch: [0.7, 1.8], tint: palette(ENERGY), alpha: life => bump(life) * 0.9 });
      const ash = fx.sim({ texture: 'debris', blend: 'normal', drag: 0.6, gravity: 120, shrink: 0.8, tint: () => color('#1e0b36'), alpha: life => life });
      return {
        trigger() {
          const at = fx.now;
          u.hit(false, '#67e8f9');
          fx.tween(at, 0.6, p => {
            for (let n = fx.count(220 * (1 - p)); n > 0; n--) flame.spawn({ x: u.center.x + rand(-28, 28), y: rand(u.center.y, u.feet.y - 10), vx: rand(-20, 20), vy: rand(-260, -120), lifetime: rand(0.35, 0.6), size: rand(8, 15) });
          });
          fx.at(at + 0.5, () => spray(ash, { x: u.center.x, y: u.head.y, count: 12, speed: [20, 80], life: [0.6, 1], size: [3, 6], jitter: 30, spin: 4 }));
          pop(fx, { at, dur: 0.35, x: u.center.x, y: u.center.y, r: 90, css: '#67e8f9' });
        },
      };
    },
  }),
  status({
    id: 'sl-eburn-shatter', name: 'Раскол заряда', group: 'Эффект · Выжигание энергии', spell: 'EnergyBurn', instant: true,
    about: 'Мгновенно, не висит: над головой проявляется голубая сфера заряда, трескается и разлетается гаснущими осколками',
    build(fx, u) {
      const shards = fx.sim({ ...P.shard(ENERGY, 600), tint: palette(ENERGY) });
      const c = { x: u.head.x, y: u.head.y - 40 };
      const orb = fx.dot('glow', '#67e8f9');
      const core = fx.dot('glow', '#e0f2fe');
      const cracks = fx.gfx('front', 'add');
      const BREAK = 0.4;
      return {
        trigger() {
          const at = fx.now;
          const lines = Array.from({ length: 4 }, () => jag(c, polar(c, 22, rand(0, TAU)), 4, 5));
          fx.tween(at, BREAK, p => {
            const s = ease.back(Math.min(p * 2, 1));
            orb.put(c.x, c.y, 30 * s, p < 1 ? 0.85 : 0);
            core.put(c.x, c.y, 12 * s, p < 1 ? 1 : 0);
            cracks.clear();
            if (p >= 1) return;
            lines.slice(0, Math.floor(clamp01((p - 0.5) * 2) * 4)).forEach(l => {
              cracks.moveTo(l[0].x, l[0].y);
              l.slice(1).forEach(pt => cracks.lineTo(pt.x, pt.y));
              cracks.stroke({ width: 2, color: color('#ffffff'), alpha: 1 });
            });
          });
          fx.at(at + BREAK, () => {
            u.hit(false, '#67e8f9');
            spray(shards, { x: c.x, y: c.y, count: 18, speed: [120, 380], life: [0.4, 0.8], size: [5, 10], align: true });
          });
          pop(fx, { at: at + BREAK, dur: 0.3, x: c.x, y: c.y, r: 70, css: '#a78bfa' });
        },
      };
    },
  }),
  status({
    id: 'sl-eburn-sink', name: 'Утечка в воронку', group: 'Эффект · Выжигание энергии', spell: 'EnergyBurn', instant: true,
    about: 'Мгновенно, не висит: под ногами раскрывается чёрная воронка, и голубые искры энергии стекают в неё из тела',
    build(fx, u) {
      const h = bodyOf(u);
      const motes = fx.sim({ ...P.glow(ENERGY, 0), tint: palette(ENERGY) });
      const pit = fx.radial([[0, 1], [0.6, 0.7], [1, 0]], '#09030f', 'back', 'normal');
      const rim = fx.dot('ring', '#7e22ce', 'back');
      return {
        trigger() {
          const at = fx.now;
          fx.at(at + 0.15, () => u.hit(false, '#67e8f9'));
          fx.tween(at, 1.2, p => {
            const a = p >= 1 ? 0 : Math.min(clamp01(p / 0.15), clamp01((1 - p) / 0.3));
            const r = 70 * ease.out3(Math.min(p * 3, 1));
            pit.oval(u.feet.x, u.feet.y, r, r * GROUND, a * 0.9);
            rim.oval(u.feet.x, u.feet.y, r, r * GROUND, a * 0.8);
            for (let n = fx.count(80 * a * (p < 0.7 ? 1 : 0)); n > 0; n--) {
              const from = { x: u.center.x + rand(-30, 30), y: u.feet.y - rand(h * 0.2, h * 0.9) };
              const to = polar(u.feet, 10, Math.random() * TAU, GROUND);
              motes.spawn({ x: from.x, y: from.y, vx: (to.x - from.x) * 2.2, vy: (to.y - from.y) * 2.2, lifetime: 0.45, size: rand(4, 7) });
            }
          });
        },
      };
    },
  }),
];
