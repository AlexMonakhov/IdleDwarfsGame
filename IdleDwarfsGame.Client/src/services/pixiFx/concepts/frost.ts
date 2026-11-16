import {
  C, GROUND, P, TAU, angle, bez, bump, color, ease, fade, fly, groundGlow, lerp, levelAccent, mix, palette, polar, pop, rand, ringWave, spray,
  perTarget, type Concept, type ConceptUnit, type Fx, type Pt,
} from './kit';
import { iceChipTexture, iceFangTexture, snowflakeTexture } from './sprites';

// Концепты льда
const FLAKE = { texture: 'star' as const, drag: 0.4, gravity: 60, shrink: 0.6, tint: palette(C.ice), alpha: (life: number) => bump(life) };
const ICE_FLASH = '#bae6fd';
// Морозная дымка (обычное наложение) и иней-искры для реалистичных гробниц
const COLD_MIST = { texture: 'smoke' as const, blend: 'normal' as const, drag: 0.4, gravity: -25, shrink: 1.4, tint: palette(['#f0f9ff', '#e0f2fe']), alpha: (life: number) => bump(life) * 0.35 };
const CHIPS = { texture: iceChipTexture(), blend: 'normal' as const, drag: 0.3, gravity: 1000, shrink: 0.6 };

// Кольцо маленьких ледяных клыков вокруг точки: вырастают, держатся hold секунд и рассыпаются (добавка 3-го уровня)
const fangBurst = (fx: Fx, at: number, x: number, y: number, count: number, height: number, hold = 0.6) => {
  const chips = fx.sim(CHIPS);
  for (let i = 0; i < count; i++) {
    const a = (i / count) * TAU + rand(-0.2, 0.2);
    const behind = Math.sin(a) < 0;
    const base = polar({ x, y }, rand(45, 70), a, 0.36);
    const spike = fx.spike(iceFangTexture((i % 3) as 0 | 1 | 2), base.x, base.y, height * rand(0.75, 1.1), behind ? 'back' : 'front');
    spike.sprite.rotation = Math.cos(a) * 0.5;
    spike.sprite.alpha = 0.85;
    fx.tween(at, 0.12, p => spike.sprite.scale.set(spike.k, spike.k * ease.out3(p)));
    fx.tween(at + 0.12 + hold, 0.1, p => (spike.sprite.alpha = 0.85 * (1 - p)));
    fx.at(at + 0.12 + hold, () => spray(chips, { x: base.x, y: base.y - height * 0.4, count: 4, speed: [120, 380], life: [0.4, 0.8], size: [6, 12], align: true, spin: 8 }));
  }
};

// Цель вмерзает в лёд (заморозка от «Осколка зимы»): вокруг сходятся клыки, тело затягивает глазурь, держится hold секунд
const encase = (fx: Fx, at: number, T: ConceptUnit, hold = 0.9) => {
  const { x, y } = T.feet;
  const chips = fx.sim(CHIPS);
  const glaze = fx.dot('glow', '#e0f6ff', 'front', 'normal');
  const shell = fx.dot('glow', '#7dd3fc');
  fx.tween(at, hold + 0.3, p => {
    const a = fade(p, 0.12, 0.2);
    glaze.oval(T.center.x, T.center.y + 10, 72, 125, a * 0.38);
    shell.oval(T.center.x, T.center.y + 10, 90, 140, a * (0.25 + 0.1 * Math.sin(p * 20)));
  }, [glaze, shell]);
  for (let i = 0; i < 7; i++) {
    const a = (i / 7) * TAU + 0.3;
    const behind = Math.sin(a) < 0;
    const base = polar({ x, y }, rand(30, 48), a, 0.36);
    const h = rand(110, 170) * (behind ? 1.1 : 0.8);
    const spike = fx.spike(iceFangTexture((i % 3) as 0 | 1 | 2), base.x, base.y, h, behind ? 'back' : 'front');
    spike.sprite.rotation = -Math.cos(a) * 0.3;
    spike.sprite.alpha = 0.8;
    fx.tween(at + i * 0.02, 0.14, p => spike.sprite.scale.set(spike.k, spike.k * ease.out3(p)));
    fx.tween(at + hold, 0.12, p => (spike.sprite.alpha = 0.8 * (1 - p)));
    fx.at(at + hold, () => spray(chips, { x: base.x, y: base.y - h * 0.5, count: 4, speed: [120, 360], life: [0.4, 0.8], size: [6, 12], align: true, spin: 8 }));
  }
  ringWave(fx, { at, dur: 0.45, x, y, from: 20, to: 130, css: '#e0f6ff', squash: GROUND, layer: 'back' });
};

export const FROST_CONCEPTS: Concept[] = [
  {
    id: 'frost-lance', name: 'Ледяное копьё', group: 'Лёд', spell: 'Frost Bolt', duration: 1.8,
    levels: true,
    targets: [1, 1, 1],
    extras: true,
    artifacts: [{
      key: 'WinterShard', name: 'Осколок зимы', kind: 'UpgradeStatus',
      about: 'Если на цели уже был мороз, копьё вмораживает её в лёд — клыки смыкаются вокруг',
    }],
    about: 'Копьё намерзает в руке, срывается и разбивается веером осколков. От артефактов летит из расколотого льда или падает с неба',
    build: perTarget(fx => {
      const { caster, enemies } = fx.stage;
      const T = enemies[0];
      const to = T.center;
      // Срабатывания артефактов: раскол «Оков ледника» — осколок летит от расколотого (fx.origin),
      // «Реликвия ледника» — копьё падает с неба из вьюги
      const shard = fx.triggered('Shatter') && fx.origin;
      const fromSky = fx.triggered('SpellProc', 'Blizzard');
      const from = shard && fx.origin ? { x: fx.origin.x, y: fx.origin.y - 90 }
        : fromSky ? { x: to.x - 140, y: to.y - 560 }
          : { x: caster.center.x + 60, y: caster.center.y - 30 };
      const FLY = fx.trigger ? 0.1 : 0.45;
      const HIT = FLY + 0.22;
      const dir = angle(from, to);
      // Копьё крупнее с уровнем
      const K = fx.lvl([0.75, 0.88, 1]);
      const lance = fx.dot('lance', '#bae6fd');
      const lanceCore = fx.dot('lance', '#ffffff');
      const halo = fx.dot('glow', '#38bdf8');
      const flakes = fx.sim(FLAKE);
      const streaks = fx.sim(P.streak(C.ice));
      const shards = fx.sim(P.shard(C.ice, 700));
      fx.at(0, () => fx.stage.cast(ICE_FLASH));
      // Намерзает: частицы стягиваются к руке (у срабатывания — сразу летит)
      if (!fx.trigger) fx.tween(0, 0.4, p => {
        lance.put(from.x, from.y, 34 * ease.back(p), 1, dir);
        lanceCore.put(from.x, from.y, 20 * ease.back(p), 1, dir);
        halo.put(from.x, from.y, 30 + 60 * p, 0.6);
        for (let i = fx.count(70); i > 0; i--) {
          const a = Math.random() * TAU;
          const start = polar(from, rand(60, 110), a);
          flakes.spawn({ x: start.x, y: start.y, vx: -Math.cos(a) * 260, vy: -Math.sin(a) * 260, lifetime: 0.3, size: rand(4, 8), spin: 6 });
        }
      });
      fly(fx, FLY, 0.22, p => mix(from, to, p), pt => {
        lance.put(pt.x, pt.y, 40 * K, 1, dir);
        lanceCore.put(pt.x, pt.y, 24 * K, 1, dir);
        halo.put(pt.x, pt.y, 80 * K, 0.7);
        spray(streaks, { x: pt.x, y: pt.y, count: fx.count(200), speed: [40, 200], dir: dir + Math.PI, spread: 0.25, life: [0.2, 0.45], size: [5, 11], align: true, jitter: 6 });
        spray(flakes, { x: pt.x, y: pt.y, count: fx.count(80), speed: [10, 80], life: [0.4, 0.8], size: [4, 9], spin: 5, jitter: 12 });
      }, [lance, lanceCore, halo]);
      fx.at(HIT, () => {
        T.hit(false, ICE_FLASH);
        fx.stage.shake(5, 0.2);
        spray(shards, { x: to.x, y: to.y, count: fx.lvl([18, 26, 38]), speed: [200, 700], dir, spread: 0.9, life: [0.5, 1], size: [7, 16], align: true, spin: 10 });
        spray(flakes, { x: to.x, y: to.y, count: 30, speed: [40, 260], life: [0.5, 1.1], size: [5, 11], spin: 5 });
      });
      pop(fx, { at: HIT, dur: 0.35, x: to.x, y: to.y, r: 150, css: '#7dd3fc' });
      if (fx.since(2)) ringWave(fx, { at: HIT, dur: 0.5, x: to.x, y: to.y, from: 20, to: 140, css: '#e0f6ff' });
      groundGlow(fx, { at: HIT, dur: 1, x: T.feet.x, y: T.feet.y, r: 120, css: '#38bdf8', alpha: 0.8 });
      if (fx.since(3)) levelAccent(fx, 'frost', HIT, T);
      // Доп. эффект: кольцо маленьких клыков вокруг цели
      if (fx.extras) fangBurst(fx, HIT, T.feet.x, T.feet.y, 6, 70);
      // «Осколок зимы»: мороз превратился в заморозку
      if (fx.has('UpgradeStatus') && T.proc) encase(fx, HIT + 0.05, T);
    }, 0.14),
  },
  {
    id: 'frost-fan', name: 'Веер осколков', group: 'Лёд', spell: 'Frost Bolt', duration: 2,
    levels: true,
    extras: true,
    about: 'Пять осколков расходятся веером и по дугам сходятся в цель',
    build(fx) {
      const { caster, enemies } = fx.stage;
      const T = enemies[0];
      const from = caster.center;
      const to = T.center;
      const normal = angle(from, to) + Math.PI / 2;
      const streaks = fx.sim(P.streak(C.ice));
      const shards = fx.sim(P.shard(C.ice, 600));
      fx.at(0, () => fx.stage.cast(ICE_FLASH));
      // Осколков в веере: 3 / 4 / 5 по уровню
      const COUNT = fx.lvl([3, 4, 5]);
      for (let i = 0; i < COUNT; i++) {
        const at = 0.2 + i * 0.08;
        const control = polar(mix(from, to, 0.35), (i - (COUNT - 1) / 2) * 130, normal);
        const lance = fx.dot('lance', '#bae6fd');
        const core = fx.dot('lance', '#ffffff');
        fly(fx, at, 0.5, p => bez(from, control, to, ease.in(p) * 0.5 + p * 0.5), (pt, _p, dir) => {
          lance.put(pt.x, pt.y, 26, 1, dir);
          core.put(pt.x, pt.y, 15, 1, dir);
          spray(streaks, { x: pt.x, y: pt.y, count: fx.count(110), speed: [20, 90], dir: dir + Math.PI, spread: 0.2, life: [0.2, 0.4], size: [4, 8], align: true });
        }, [lance, core]);
        fx.at(at + 0.5, () => {
          T.hit(i === COUNT - 1, ICE_FLASH);
          spray(shards, { x: to.x, y: to.y, count: 10, speed: [150, 480], life: [0.4, 0.8], size: [5, 11], align: true, spin: 10 });
        });
        pop(fx, { at: at + 0.5, dur: 0.25, x: to.x + rand(-20, 20), y: to.y + rand(-30, 30), r: 80, css: '#bae6fd' });
      }
      ringWave(fx, { at: 1.02, dur: 0.5, x: to.x, y: to.y, from: 20, to: 170, css: '#e0f6ff' });
      groundGlow(fx, { at: 0.7, dur: 1.1, x: T.feet.x, y: T.feet.y, r: 130, css: '#38bdf8', alpha: 0.8 });
      if (fx.since(3)) levelAccent(fx, 'frost', 1.02, T);
      if (fx.extras) fangBurst(fx, 1.02, T.feet.x, T.feet.y, 6, 70);
    },
  },
  {
    id: 'blizzard-gale', name: 'Вьюга', group: 'Лёд', spell: 'Blizzard', duration: 2.6,
    levels: true,
    about: 'Шквал ветра со снегом проносится через вражеский строй, у ног врагов позёмка уносится вместе с ветром',
    build(fx) {
      const { caster, enemies } = fx.stage;
      const x0 = caster.center.x + 120;
      const top = Math.min(...enemies.map(e => e.head.y)) - 60;
      const bottom = Math.max(...enemies.map(e => e.feet.y)) + 30;
      const cx = enemies.reduce((sum, e) => sum + e.feet.x, 0) / enemies.length;
      const cy = (top + bottom) / 2 + 60;
      const wind = fx.sim({ texture: 'disc', drag: 0.7, shrink: 0.5, stretch: [6, 0.22], tint: palette(C.ice), alpha: life => bump(life) * 0.7 });
      const backWind = fx.sim({ texture: 'disc', drag: 0.7, shrink: 0.5, stretch: [6, 0.22], tint: palette(C.ice), alpha: life => bump(life) * 0.5 }, 'back');
      const flakes = fx.sim({ ...FLAKE, drag: 0.8, gravity: 120 });
      const frost = fx.radial([[0, 0.6], [0.6, 0.3], [1, 0]], '#7dd3fc', 'back');
      // Позёмка: сплющенная морозная дымка у ног, ветер уносит её вслед за снегом
      const DRIFT = { texture: 'smoke' as const, blend: 'normal' as const, drag: 0.5, gravity: -10, shrink: 1.5, stretch: [2, 0.55] as [number, number], tint: palette(['#f0f9ff', '#e0f2fe', '#bae6fd']), alpha: (life: number) => bump(life) * 0.3 };
      const drift = fx.sim(DRIFT);
      const backDrift = fx.sim(DRIFT, 'back');
      fx.at(0, () => fx.stage.cast(ICE_FLASH));
      fx.tween(0.25, 1.9, p => {
        const power = fade(p, 0.15, 0.25);
        frost.oval(cx, cy + 40, 420, 150, power * 0.7);
        for (const sim of [wind, backWind]) {
          for (let i = fx.count(fx.lvl([70, 95, 120]) * power); i > 0; i--) {
            const y = rand(top, bottom);
            const tilt = rand(-0.08, 0.14);
            sim.spawn({ x: x0 + rand(-60, 120), y, vx: rand(900, 1500) * Math.cos(tilt), vy: rand(900, 1500) * Math.sin(tilt), lifetime: rand(0.45, 0.8), size: rand(6, 14), rotation: tilt });
          }
        }
        // Позёмка (со 2-го уровня): срывается из-под ног каждого врага и стелется по земле через весь строй
        if (fx.since(2)) for (const enemy of enemies) {
          for (let i = fx.count(10 * power); i > 0; i--) {
            const sim = Math.random() < 0.6 ? backDrift : drift;
            sim.spawn({ x: enemy.feet.x + rand(-50, 30), y: enemy.feet.y + rand(-14, 8), vx: rand(380, 620), vy: rand(-30, 5), lifetime: rand(0.6, 1.1), size: rand(30, 54) });
          }
        }
        for (let i = fx.count(7 * power); i > 0; i--) {
          backDrift.spawn({ x: x0 + rand(-40, 400), y: bottom + rand(-50, 0), vx: rand(420, 700), vy: rand(-20, 5), lifetime: rand(0.6, 1), size: rand(36, 60) });
        }
        for (let i = fx.count(90 * power); i > 0; i--) {
          flakes.spawn({ x: x0 + rand(0, 700), y: rand(top - 40, bottom), vx: rand(300, 700), vy: rand(-40, 120), lifetime: rand(0.6, 1.2), size: rand(5, 12), spin: 6 });
        }
      }, [frost]);
      // Ударов ветра: 2 / 3 / 4 по уровню
      for (const time of fx.lvl([[0.7, 1.4], [0.6, 1.1, 1.6], [0.5, 0.9, 1.3, 1.7]])) {
        fx.at(time, () => {
          enemies.forEach(e => e.hit(false, ICE_FLASH));
          fx.stage.shake(3, 0.2);
        });
      }
      if (fx.since(3)) fx.stage.enemies.forEach(e => levelAccent(fx, 'frost', 1.3, e));
    },
  },
  {
    id: 'blizzard-icicles', name: 'Ледяной дождь', group: 'Лёд', spell: 'Blizzard', duration: 2.6,
    levels: true,
    targets: ['all', 'all', 'all'],
    extras: true,
    artifacts: [{
      key: 'GlacialRelic', name: 'Реликвия ледника', kind: 'SpellProc',
      about: 'С шансом 50% по каждой цели вьюги из неба падает ледяное копьё',
      then: { concept: 'frost-lance', level: 1, targets: 'same', after: 0.75 },
    }],
    about: 'Сосульки сыплются с неба на весь строй и раскалываются о землю',
    build(fx) {
      const { enemies } = fx.stage;
      const shards = fx.sim(P.shard(C.ice, 900));
      const flakes = fx.sim(FLAKE);
      fx.at(0, () => fx.stage.cast(ICE_FLASH));
      // Сосулек: 9 / 12 / 16 по уровню
      const COUNT = fx.lvl([9, 12, 16]);
      for (let i = 0; i < COUNT; i++) {
        const enemy = enemies[i % enemies.length];
        const to = { x: enemy.feet.x + rand(-70, 70), y: enemy.feet.y + rand(-25, 30) };
        const from = { x: to.x - 70, y: to.y - 620 };
        const at = 0.25 + i * (1.5 / COUNT);
        const dir = angle(from, to);
        const lance = fx.dot('lance', '#bae6fd');
        const core = fx.dot('lance', '#ffffff');
        fly(fx, at, 0.26, p => mix(from, to, p), pt => {
          lance.put(pt.x, pt.y, 30, 1, dir);
          core.put(pt.x, pt.y, 17, 1, dir);
        }, [lance, core]);
        // Первая сосулька в каждого — с отметкой 3-го уровня
        if (fx.since(3) && i < enemies.length) levelAccent(fx, 'frost', at + 0.26, enemy);
        fx.at(at + 0.26, () => {
          if (i < enemies.length * 2) enemy.hit(false, ICE_FLASH);
          spray(shards, { x: to.x, y: to.y, count: 9, speed: [150, 420], dir: -Math.PI / 2, spread: 1.1, life: [0.4, 0.8], size: [5, 11], align: true, spin: 9 });
          spray(flakes, { x: to.x, y: to.y, count: 5, speed: [20, 100], life: [0.4, 0.8], size: [4, 8], spin: 4 });
        });
        ringWave(fx, { at: at + 0.26, dur: 0.35, x: to.x, y: to.y, from: 8, to: 70, css: '#e0f6ff', squash: GROUND, layer: 'back' });
        groundGlow(fx, { at: at + 0.26, dur: 0.8, x: to.x, y: to.y, r: 60, css: '#38bdf8', alpha: 0.7 });
      }
      // Доп. эффект: напоследок огромная сосулька в главную цель
      if (fx.extras) {
        const T = enemies[0];
        const end = T.feet;
        const start = { x: end.x - 90, y: end.y - 720 };
        const dir = angle(start, end);
        const big = fx.dot('lance', '#bae6fd');
        const bigCore = fx.dot('lance', '#ffffff');
        const halo = fx.dot('glow', '#38bdf8');
        fly(fx, 1.75, 0.3, p => mix(start, end, ease.in(p)), pt => {
          big.put(pt.x, pt.y, 70, 1, dir);
          bigCore.put(pt.x, pt.y, 40, 1, dir);
          halo.put(pt.x, pt.y, 110, 0.6);
        }, [big, bigCore, halo]);
        fx.at(2.05, () => {
          T.hit(true, ICE_FLASH);
          fx.stage.shake(10, 0.3);
          spray(shards, { x: end.x, y: end.y, count: 30, speed: [200, 600], dir: -Math.PI / 2, spread: 1.2, life: [0.5, 1], size: [7, 15], align: true, spin: 9 });
        });
        fangBurst(fx, 2.05, end.x, end.y, 7, 80, 0.4);
      }
    },
  },
  {
    id: 'prison-fangs', name: 'Ледяные клыки', group: 'Лёд', spell: 'Glacial Prison', duration: 3.1,
    levels: true,
    targets: [1, 1, 2],
    artifacts: [{
      key: 'GlacierShackles', name: 'Оковы ледника', kind: 'Shatter',
      about: 'Когда заморозка спадает или носителя убивают, лёд раскалывается — осколки летят в соседей',
      then: { concept: 'frost-lance', level: 1, targets: 'neighbours', origin: 'target', after: 0.75 },
    }],
    about: 'Под целью вращается морозная руна, тремя волнами вырываются гранёные прозрачные клыки и смыкаются над ней; вокруг вьётся снег, по клыкам бегут блики; перед разломом лёд дрожит и разлетается',
    build: perTarget(fx => {
      const { enemies } = fx.stage;
      const T = enemies[0];
      const { x, y } = T.feet;
      const SHIVER = 1.95;
      const SHATTER = 2.2;
      const mist = fx.sim(COLD_MIST);
      const mistBack = fx.sim(COLD_MIST, 'back');
      const chips = fx.sim(CHIPS);
      const flake = snowflakeTexture();
      const snow = fx.sim({ texture: flake, drag: 0.2, gravity: -40, shrink: 0.6, tint: palette(['#ffffff', '#e0f6ff', '#bae6fd']), alpha: life => bump(life) });
      const glitter = fx.sim({ texture: 'star', drag: 0.5, gravity: 200, shrink: 0.6, tint: palette(['#ffffff', '#e0f6ff', '#7dd3fc']), alpha: life => bump(life) });
      fx.at(0, () => fx.stage.cast(ICE_FLASH));
      // Морозная руна-снежинка на земле (со 2-го уровня)
      const rune = fx.gfx('back', 'add');
      if (fx.since(2)) fx.tween(0.1, SHATTER + 0.35, p => {
        const t = 0.1 + p * (SHATTER + 0.35);
        const r = 150 * ease.back(Math.min((t - 0.1) / 0.35, 1));
        const a = (t > SHATTER ? 1 - (t - SHATTER) / 0.35 : 1) * (0.8 + 0.2 * Math.sin(t * 10));
        rune.clear();
        if (r < 2) return;
        const pt = (ang: number, k: number) => ({ x: x + Math.cos(ang) * r * k, y: y + Math.sin(ang) * r * k * GROUND });
        rune.ellipse(x, y, r, r * GROUND).stroke({ width: 2.5, color: color('#bae6fd'), alpha: 0.85 * a });
        rune.ellipse(x, y, r * 0.86, r * 0.86 * GROUND).stroke({ width: 1, color: color('#e0f6ff'), alpha: 0.6 * a });
        for (let i = 0; i < 6; i++) {
          const ang = (i / 6) * TAU + t * 0.5;
          const o = pt(ang, 0.84);
          rune.moveTo(x, y).lineTo(o.x, o.y).stroke({ width: 2, color: color('#e0f6ff'), alpha: 0.75 * a });
          for (const k of [0.45, 0.65]) {
            const m = pt(ang, k);
            for (const side of [-1, 1]) {
              const e = pt(ang + side * 0.28, k + 0.12);
              rune.moveTo(m.x, m.y).lineTo(e.x, e.y).stroke({ width: 1.5, color: color('#bae6fd'), alpha: 0.7 * a });
            }
          }
        }
      });
      groundGlow(fx, { at: 0.1, dur: SHATTER + 0.3, x, y, r: 170, css: '#38bdf8' });
      // Клыки: три волны — дальние высокие, средние, ближние низкие; все наклонены к цели
      const waves = [
        { at: 0.35, count: 6, r: [95, 120], h: [200, 270] },
        { at: 0.5, count: 6, r: [70, 95], h: [140, 200] },
        { at: 0.65, count: 5, r: [55, 80], h: [80, 130] },
      ];
      const fangs: { base: Pt; h: number; lean: number }[] = [];
      // Волн клыков: 1 / 2 / 3 по уровню
      waves.slice(0, fx.lvl([1, 2, 3])).forEach((wave, w) => {
        for (let i = 0; i < wave.count; i++) {
          const a = (i / wave.count) * TAU + w * 0.5 + rand(-0.2, 0.2);
          const behind = Math.sin(a) < 0;
          const base = polar({ x, y }, rand(wave.r[0], wave.r[1]), a, 0.36);
          const h = rand(wave.h[0], wave.h[1]) * (behind ? 1.05 : 0.8);
          const texture = iceFangTexture(((i + w) % 3) as 0 | 1 | 2);
          const layer = behind ? 'back' : 'front';
          const body = fx.spike(texture, base.x, base.y, h, layer);
          const glow = fx.spike(texture, base.x, base.y, h, layer, 'add');
          const lean = -Math.cos(a) * rand(0.35, 0.6);
          body.sprite.rotation = glow.sprite.rotation = lean;
          body.sprite.alpha = 0.85;
          fangs.push({ base, h, lean });
          const at = wave.at + rand(0, 0.08);
          fx.tween(at, 0.16, p => {
            const s = ease.out3(p);
            body.sprite.scale.set(body.k * (0.7 + 0.3 * s), body.k * s);
            glow.sprite.scale.set(glow.k * (0.7 + 0.3 * s), glow.k * s);
            glow.sprite.alpha = 0.35 * (1 - p) + 0.06;
          });
          fx.tween(at + 0.16, SHATTER - at - 0.16, p => {
            const t = at + 0.16 + p * (SHATTER - at - 0.16);
            // Перед разломом: дрожь и свет изнутри
            const shiver = t > SHIVER ? (t - SHIVER) / (SHATTER - SHIVER) : 0;
            body.sprite.rotation = glow.sprite.rotation = lean + Math.sin(t * 90 + i) * 0.03 * shiver;
            glow.sprite.alpha = 0.06 + 0.05 * Math.sin(t * 9 + i + w) + 0.35 * shiver;
          });
          fx.tween(SHATTER, 0.08, p => {
            body.sprite.alpha = 0.85 * (1 - p);
            glow.sprite.alpha = 0.4 * (1 - p);
          });
          fx.at(at, () => {
            spray(chips, { x: base.x, y: base.y, count: 3, speed: [120, 300], dir: -Math.PI / 2, spread: 0.8, life: [0.3, 0.6], size: [6, 12], align: true, spin: 8 });
            spray(behind ? mistBack : mist, { x: base.x, y: base.y, count: 2, speed: [10, 40], life: [0.6, 1], size: [26, 40] });
          });
          fx.at(SHATTER, () => {
            for (let k = 0; k < 4; k++) {
              const d = rand(-Math.PI, 0);
              const speed = rand(180, 560);
              chips.spawn({ x: base.x + Math.sin(lean) * h * 0.5, y: base.y - h * rand(0.2, 0.8), vx: Math.cos(d) * speed, vy: Math.sin(d) * speed, lifetime: rand(0.5, 1), size: rand(8, 18), rotation: d, spin: rand(-10, 10) });
            }
          });
        }
        fx.at(wave.at + 0.05, () => fx.stage.shake(3 + w, 0.15));
      });
      // Блики бегут вверх по клыкам (3-й уровень)
      const glints = Array.from({ length: fx.since(3) ? 4 : 0 }, () => fx.dot('star', '#ffffff'));
      fx.tween(0.85, SHATTER - 0.85, p => {
        const t = 0.85 + p * (SHATTER - 0.85);
        glints.forEach((glint, i) => {
          const cycle = t * 1.6 + i * 0.27;
          const local = cycle % 1;
          const fang = fangs[(Math.floor(cycle) * 5 + i * 3) % fangs.length];
          const along = 0.2 + 0.7 * local;
          const gx = fang.base.x + Math.sin(fang.lean) * fang.h * along;
          const gy = fang.base.y - Math.cos(fang.lean) * fang.h * along;
          glint.put(gx, gy, 18 * bump(local), bump(local), local * 3);
        });
      }, glints);
      // Снег спиралью вьётся вокруг цели (со 2-го уровня)
      if (fx.since(2)) fx.tween(0.6, SHATTER - 0.6, p => {
        for (let i = fx.count(26); i > 0; i--) {
          const a = rand(0, TAU);
          const r = rand(60, 120);
          snow.spawn({ x: x + Math.cos(a) * r, y: y - rand(0, 40) + Math.sin(a) * r * 0.32, vx: -Math.sin(a) * 120, vy: rand(-120, -60), lifetime: rand(0.8, 1.3), size: rand(5, 10), spin: 4 });
        }
        spray(Math.random() < 0.5 ? mist : mistBack, { x: x + rand(-90, 90), y: y - rand(0, 60), count: fx.count(6 * (1 - p * 0.5)), speed: [5, 25], life: [0.8, 1.2], size: [34, 54] });
      });
      // Цель покрывается ледяной глазурью
      const glaze = fx.dot('glow', '#bae6fd', 'front', 'normal');
      fx.tween(0.6, SHATTER - 0.6, p => glaze.oval(T.center.x, T.center.y + 10, 70, 120, fade(p, 0.2, 0.05) * 0.3), [glaze]);
      fx.at(0.7, () => T.hit(true, ICE_FLASH));
      fx.at(SHATTER, () => {
        T.hit(true, ICE_FLASH);
        fx.stage.shake(11, 0.4);
        fx.stage.flash('#e0f6ff', 0.35, 0.3);
        spray(glitter, { x, y: y - 100, count: 40, speed: [150, 520], life: [0.5, 1.1], size: [6, 12] });
        spray(snow, { x, y: y - 90, count: 30, speed: [100, 380], life: [0.6, 1.2], size: [6, 12], spin: 6 });
        spray(mist, { x, y: y - 100, count: 14, speed: [40, 180], life: [0.8, 1.3], size: [40, 70], jitter: 60 });
      });
      pop(fx, { at: SHATTER, dur: 0.4, x, y: y - 100, r: 240, css: '#e0f2fe', alpha: 0.75 });
      ringWave(fx, { at: SHATTER, dur: 0.6, x, y, from: 40, to: 320, css: '#e0f6ff', squash: GROUND, layer: 'back' });
      if (fx.since(3)) levelAccent(fx, 'frost', 0.75, T);
      if (fx.since(3)) ringWave(fx, { at: SHATTER + 0.08, dur: 0.6, x, y, from: 20, to: 220, css: '#7dd3fc', squash: GROUND, layer: 'back', alpha: 0.7 });
    }, 0.3),
  },
  {
    id: 'prison-ring', name: 'Морозное кольцо', group: 'Лёд', spell: 'Glacial Prison', duration: 2.2,
    levels: true,
    extras: true,
    about: 'Кольцо стужи сжимается вокруг цели и схлопывается столбом холода',
    build(fx) {
      const { enemies } = fx.stage;
      const T = enemies[0];
      const { x, y } = T.feet;
      const ring = fx.dot('ring', '#bae6fd', 'back');
      const ringFront = fx.dot('ring', '#ffffff');
      const flakes = fx.sim(FLAKE);
      const up = fx.sim({ ...FLAKE, gravity: -260 });
      const column = fx.dot('glow', '#7dd3fc');
      const columnCore = fx.dot('glow', '#ffffff');
      const SNAP = 0.85;
      // Столб холода шире и выше с уровнем
      const K = fx.lvl([0.7, 0.85, 1]);
      fx.at(0, () => fx.stage.cast(ICE_FLASH));
      fx.tween(0.15, SNAP - 0.15, p => {
        const r = lerp(300, 46, ease.in(p));
        ring.oval(x, y, r, r * GROUND, 0.5 + 0.5 * p);
        ringFront.oval(x, y - 90 * p, r * 0.8, r * 0.8 * GROUND, p * 0.6);
        for (let i = fx.count(140); i > 0; i--) {
          const a = Math.random() * TAU;
          const at = polar({ x, y }, r, a, GROUND);
          flakes.spawn({ x: at.x, y: at.y, vx: -Math.sin(a) * 260, vy: Math.cos(a) * 260 * GROUND - 60, lifetime: rand(0.3, 0.6), size: rand(4, 9), spin: 6 });
        }
      }, [ring, ringFront]);
      fx.tween(SNAP, 0.7, p => {
        const a = fade(p, 0.06, 0.6);
        column.oval(x, y - 150 * K, 80 * K, 300 * K, a);
        columnCore.oval(x, y - 150 * K, 22 * K, 280 * K, a);
        if (fx.since(2)) spray(up, { x, y, count: fx.count(p < 0.5 ? 160 : 0), speed: [200, 620], dir: -Math.PI / 2, spread: 0.25, life: [0.4, 0.9], size: [5, 11], spin: 6, jitter: 26 });
      }, [column, columnCore]);
      fx.at(SNAP, () => {
        T.hit(true, ICE_FLASH);
        fx.stage.shake(8, 0.3);
        fx.stage.flash('#bae6fd', 0.25, 0.25);
      });
      ringWave(fx, { at: SNAP, dur: 0.55, x, y, from: 40, to: 260, css: '#e0f6ff', squash: GROUND, layer: 'back' });
      groundGlow(fx, { at: SNAP, dur: 1.2, x, y, r: 170, css: '#38bdf8' });
      if (fx.since(3)) levelAccent(fx, 'frost', SNAP, T);
      if (fx.extras) fangBurst(fx, SNAP, x, y, 8, 90, 0.7);
    },
  },
];
