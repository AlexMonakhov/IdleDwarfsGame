import { Container, Graphics } from 'pixi.js';
import {
  Beam, C, GROUND, P, TAU, angle, angryEye, ribbon, bez, bump, color, ease, fade, fly, groundGlow, lerp, levelAccent, mix, palette, path, polar, pop, rand, ringWave, spray, taper,
  perTarget, type Concept, type ConceptUnit, type Fx,
} from './kit';

// Концепты тьмы
const SHADE = '#c084fc';
const WISP = { texture: 'glow' as const, drag: 0.3, gravity: -60, shrink: 0.5, tint: palette(C.dark), alpha: (life: number) => bump(life) * 0.9 };
const MIST = { texture: 'smoke' as const, blend: 'normal' as const, drag: 0.3, gravity: -20, shrink: 1.4, tint: () => 0x1a0b2e, alpha: (life: number) => bump(life) * 0.6 };

// Огромная призрачная коса над вражеским строем: проступает из тьмы, замахивается и проходит через всех.
// double — второй взмах обратно (следы крест-накрест) и волна-полумесяц в конце
const reaperSweep = (fx: Fx, double: boolean) => {
  const { enemies } = fx.stage;
  const cx = enemies.reduce((sum, e) => sum + e.center.x, 0) / enemies.length;
  const cy = enemies.reduce((sum, e) => sum + e.center.y, 0) / enemies.length;
  const pivot = { x: cx - 20, y: cy + 250 };
  const L = 360;
  const B = 230;
  // Коса в своих координатах: древко из (0, 0) вверх на L, у верха лезвие загибается вправо
  const scythe = new Container();
  const glowLayer = new Graphics();
  glowLayer.blendMode = 'add';
  const body = new Graphics();
  const edge = new Graphics();
  edge.blendMode = 'add';
  // Кончик лезвия — с крюком; от него считаются след и искры
  const TIP = { x: B * 1.02, y: -L + B * 0.36 };
  const outer = [
    ...path(16, t => bez({ x: -4, y: -L + 4 }, { x: B * 0.45, y: -L - B * 0.6 }, { x: B * 0.98, y: -L + B * 0.2 }, t)),
    TIP,
  ];
  const inner = path(18, t => bez(TIP, { x: B * 0.6, y: -L - B * 0.16 }, { x: 10, y: -L + 30 }, t));
  const bladePoly = [...outer, ...inner].flatMap(p => [p.x, p.y]);
  // Древко: слегка S-образное, кольца, обмотка рукояти, боковая ручка, шип-навершие
  const snath = path(14, t => ({ x: Math.sin(t * Math.PI * 1.3) * 7, y: -L * t }));
  const snathW = snath.map((_, i) => 11 - (i / 13) * 3);
  ribbon(glowLayer, snath, snathW.map(w => w * 3), '#581c87', 0.25);
  glowLayer.poly(bladePoly).fill({ color: color('#7e22ce'), alpha: 0.32 });
  ribbon(body, snath, snathW, '#120818', 0.97);
  ribbon(body, snath, snathW.map(w => w * 0.3), '#5b21b6', 0.8, -0.25);
  for (const t of [0.22, 0.48, 0.74, 0.95]) {
    const i = Math.round(t * 13);
    body.ellipse(snath[i].x, snath[i].y, snathW[i] * 0.85, 4).fill({ color: color('#a78bfa'), alpha: 0.9 });
    body.ellipse(snath[i].x, snath[i].y - 1, snathW[i] * 0.6, 1.5).fill({ color: 0xffffff, alpha: 0.6 });
  }
  for (let k = 0; k < 6; k++) {
    const y0 = -L * 0.05 - k * 9;
    body.moveTo(-6, y0).lineTo(6, y0 - 6).stroke({ width: 2, color: color('#3b0764'), alpha: 0.9 });
  }
  const nib = path(5, t => ({ x: snath[7].x + t * 34, y: snath[7].y - t * 8 + Math.sin(t * Math.PI) * -4 }));
  ribbon(body, nib, nib.map((_, i) => 9 - i), '#120818', 0.97);
  ribbon(body, nib, nib.map(() => 2), '#7c3aed', 0.7, -0.25);
  body.poly([-7, 0, 7, 0, 0, 26]).fill({ color: color('#c4b5fd'), alpha: 0.9 });
  // Лезвие: тёмное тело, светлая полоса у кромки, дол, шипы по обуху, шпора у пятки, руны
  body.poly(bladePoly).fill({ color: color('#1a0a2e'), alpha: 0.96 });
  ribbon(body, inner, inner.map((_, i) => 4 + 16 * Math.sin((i / (inner.length - 1)) * Math.PI)), '#4c1d95', 0.9, 0.25);
  const fuller = path(12, t => bez({ x: B * 0.85, y: -L + B * 0.12 }, { x: B * 0.5, y: -L - B * 0.32 }, { x: 22, y: -L + 6 }, t));
  ribbon(body, fuller, fuller.map((_, i) => 1 + 5 * Math.sin((i / 11) * Math.PI)), '#07030d', 0.9);
  for (const t of [0.25, 0.42, 0.6, 0.76]) {
    const i = Math.floor(t * (outer.length - 2));
    const pt = outer[i];
    const next = outer[i + 1];
    const n = Math.atan2(next.y - pt.y, next.x - pt.x) - Math.PI / 2;
    const tip = polar(pt, 18 + 6 * Math.sin(t * Math.PI), n - 0.5);
    const side = polar(pt, 9, Math.atan2(next.y - pt.y, next.x - pt.x));
    body.poly([pt.x, pt.y, tip.x, tip.y, side.x, side.y]).fill({ color: color('#1a0a2e'), alpha: 0.96 });
    edge.moveTo(pt.x, pt.y).lineTo(tip.x, tip.y).stroke({ width: 1.2, color: color('#c084fc'), alpha: 0.6 });
  }
  body.poly([4, -L + 24, 26, -L + 36, 8, -L + 62]).fill({ color: color('#1a0a2e'), alpha: 0.96 });
  edge.moveTo(26, -L + 36).lineTo(8, -L + 62).stroke({ width: 1.5, color: color('#e879f9'), alpha: 0.8 });
  // Руны вдоль дола
  for (let k = 0; k < 6; k++) {
    const pt = fuller[2 + k];
    const r = 4;
    edge.moveTo(pt.x - r, pt.y - r).lineTo(pt.x + r, pt.y + r).moveTo(pt.x + r, pt.y - r).lineTo(pt.x, pt.y)
      .stroke({ width: 1.5, color: color('#e879f9'), alpha: 0.85 });
  }
  // Режущая кромка — вогнутая сторона, яркая; обух — тусклая линия
  ribbon(edge, inner, inner.map((_, i) => 2 + 7 * Math.sin((i / (inner.length - 1)) * Math.PI)), '#e879f9', 1);
  ribbon(edge, inner, inner.map((_, i) => 0.8 + 2.5 * Math.sin((i / (inner.length - 1)) * Math.PI)), '#ffffff', 1);
  ribbon(edge, outer, outer.map((_, i) => 1 + 2 * Math.sin((i / (outer.length - 1)) * Math.PI)), '#c084fc', 0.5);
  // Череп на креплении лезвия: кость, светящиеся глазницы, рожки
  const skull = { x: 2, y: -L + 6 };
  body.poly([skull.x - 12, skull.y - 6, skull.x - 26, skull.y - 26, skull.x - 8, skull.y - 14]).fill({ color: color('#d8b4fe'), alpha: 0.95 });
  body.poly([skull.x + 12, skull.y - 6, skull.x + 26, skull.y - 26, skull.x + 8, skull.y - 14]).fill({ color: color('#d8b4fe'), alpha: 0.95 });
  body.circle(skull.x, skull.y, 15).fill({ color: color('#ede9fe'), alpha: 0.96 });
  body.roundRect(skull.x - 8, skull.y + 8, 16, 10, 3).fill({ color: color('#ddd6fe'), alpha: 0.96 });
  body.circle(skull.x - 5.5, skull.y, 4.5).fill({ color: color('#1a0a2e') });
  body.circle(skull.x + 5.5, skull.y, 4.5).fill({ color: color('#1a0a2e') });
  edge.circle(skull.x - 5.5, skull.y, 2.5).fill({ color: color('#e879f9') });
  edge.circle(skull.x + 5.5, skull.y, 2.5).fill({ color: color('#e879f9') });
  edge.circle(skull.x, skull.y, 22).fill({ color: color('#a855f7'), alpha: 0.25 });
  for (let k = 0; k < 3; k++) body.moveTo(skull.x - 5 + k * 5, skull.y + 9).lineTo(skull.x - 5 + k * 5, skull.y + 17).stroke({ width: 1, color: color('#1a0a2e') });
  scythe.addChild(glowLayer, body, edge);
  scythe.position.set(pivot.x, pivot.y);
  scythe.visible = false;
  fx.stage.front.addChild(scythe);

  const wisps = fx.sim(WISP);
  const mist = fx.sim(MIST, 'back');
  const souls = fx.sim({ ...WISP, gravity: -260 });
  const sparks = fx.sim(P.spark(C.dark, 300));
  const trail = fx.gfx('front', 'normal');
  const trailEdge = fx.gfx('front', 'add');
  const FROM = -1.0;
  const TO = 1.0;
  const APPEAR = 0.15;
  const SWING = 0.8;
  const DUR = 0.28;
  const BACK = SWING + DUR + 0.35;
  const END = double ? BACK + DUR + 0.4 : SWING + DUR + 0.5;
  // Кончик лезвия в мировых координатах при повороте rot
  const tipAt = (rot: number) => {
    const lx = TIP.x;
    const ly = TIP.y;
    return { x: pivot.x + lx * Math.cos(rot) - ly * Math.sin(rot), y: pivot.y + lx * Math.sin(rot) + ly * Math.cos(rot) };
  };
  // След-серп: кольцевой сектор между радиусами лезвия за угол span позади головы
  const drawTrail = (head: number, span: number, alpha: number, dir: number) => {
    const r1 = L - 10;
    const r2 = Math.hypot(TIP.x, TIP.y) + 10;
    for (let band = 0; band < 4; band++) {
      const from = head - dir * span * (band / 4);
      const to = head - dir * span * ((band + 1) / 4);
      const pts: number[] = [];
      for (let i = 0; i <= 8; i++) {
        const a = lerp(from, to, i / 8) - Math.PI / 2;
        pts.push(pivot.x + Math.cos(a) * r2, pivot.y + Math.sin(a) * r2);
      }
      for (let i = 8; i >= 0; i--) {
        const a = lerp(from, to, i / 8) - Math.PI / 2;
        pts.push(pivot.x + Math.cos(a) * r1, pivot.y + Math.sin(a) * r1);
      }
      trail.poly(pts).fill({ color: color(band === 0 ? '#3b0764' : '#1e0b36'), alpha: alpha * (1 - band / 4) * 0.6 });
    }
    // Яркая кромка по внешнему краю следа
    const edgePts = path(14, t => {
      const a = lerp(head, head - dir * span, t) - Math.PI / 2;
      return { x: pivot.x + Math.cos(a) * (r2 - 6), y: pivot.y + Math.sin(a) * (r2 - 6) };
    });
    taper(trailEdge, edgePts, 14, '#c026d3', alpha);
    taper(trailEdge, edgePts, 4, '#ffffff', alpha);
  };
  fx.at(0, () => {
    fx.stage.cast(SHADE);
    fx.stage.flash('#0b0414', 0.45, END + 0.3);
  });
  // Проступает из тьмы и замахивается
  fx.tween(APPEAR, SWING - APPEAR, p => {
    scythe.visible = true;
    scythe.alpha = ease.out(Math.min(p * 1.6, 1));
    scythe.rotation = lerp(FROM + 0.25, FROM - 0.12, ease.inOut(p));
    scythe.scale.set(0.85 + 0.15 * ease.out(p));
    const tip = tipAt(scythe.rotation);
    spray(wisps, { x: tip.x, y: tip.y, count: fx.count(40), speed: [10, 60], life: [0.4, 0.8], size: [12, 26], jitter: 60 });
  });
  const swing = (start: number, a: number, b: number, alpha: number) => {
    const dir = Math.sign(b - a);
    fx.tween(start, DUR, p => {
      const rot = lerp(a, b, ease.inOut(p));
      scythe.rotation = rot;
      trail.clear();
      trailEdge.clear();
      drawTrail(rot, Math.min(Math.abs(rot - a), 1.3), alpha, dir);
      const tip = tipAt(rot);
      spray(sparks, { x: tip.x, y: tip.y, count: fx.count(220), speed: [40, 260], life: [0.3, 0.7], size: [3, 8] });
      // Дымка тянется с лезвия
      const mid = { x: (tip.x + pivot.x + Math.sin(rot) * L) / 2, y: (tip.y + pivot.y - Math.cos(rot) * L) / 2 };
      spray(wisps, { x: mid.x, y: mid.y, count: fx.count(160), speed: [10, 60], life: [0.4, 0.8], size: [16, 30], jitter: 50 });
    });
    // След гаснет
    fx.tween(start + DUR, 0.3, p => {
      trail.clear();
      trailEdge.clear();
      drawTrail(b, 1.3 * (1 - p * 0.5), alpha * (1 - p), dir);
    });
    for (const enemy of enemies) {
      const ang = Math.atan2(enemy.center.y - pivot.y, enemy.center.x - pivot.x) + Math.PI / 2;
      const share = Math.min(Math.max((ang - a) / (b - a), 0), 1);
      const at = start + DUR * share;
      const cut = fx.gfx('front', 'add');
      const c = enemy.center;
      const slant = dir > 0 ? 1 : -1;
      const pts = path(10, t => ({ x: c.x - 60 * slant + 120 * slant * t, y: c.y - 50 + 100 * t + Math.sin(t * Math.PI) * -12 }));
      fx.tween(at, 0.5, p => {
        cut.clear();
        const reach = Math.min(p * 5, 1);
        const shown = pts.slice(0, Math.max(2, Math.ceil(pts.length * reach)));
        taper(cut, shown, 16 * (1 - p * 0.6), '#c026d3', 1 - ease.in(p));
        taper(cut, shown, 5 * (1 - p * 0.6), '#ffffff', 1 - ease.in(p));
      });
      fx.at(at, () => {
        enemy.hit(true, SHADE);
        fx.stage.shake(4, 0.15);
        spray(sparks, { x: c.x, y: c.y, count: 14, speed: [100, 380], life: [0.3, 0.7], size: [3, 7] });
      });
      // Души вырываются и спиралью уходят к лезвию
      if (fx.since(2)) fx.tween(at + 0.1, 0.9, p => spray(souls, { x: c.x + Math.sin(p * 12) * 20 * (1 - p), y: c.y - p * 120, count: fx.count(18), speed: [10, 50], dir: -Math.PI / 2, spread: 0.6, life: [0.5, 0.9], size: [12, 24], jitter: 10 }));
    }
  };
  swing(SWING, FROM - 0.12, TO, 1);
  if (double) {
    // Короткая передышка и взмах обратно
    fx.tween(SWING + DUR, BACK - SWING - DUR, p => (scythe.rotation = lerp(TO, TO + 0.15, ease.out(p))));
    swing(BACK, TO + 0.15, FROM, 0.85);
    const waveY = cy + 40;
    fx.at(BACK + DUR, () => {
      fx.stage.shake(10, 0.35);
      fx.stage.flash('#a855f7', 0.25, 0.25);
    });
    if (fx.since(2)) ringWave(fx, { at: BACK + DUR, dur: 0.6, x: cx, y: waveY + 60, from: 40, to: 420, css: '#e879f9', squash: GROUND, layer: 'back' });
    if (fx.since(2)) pop(fx, { at: BACK + DUR, dur: 0.45, x: cx, y: cy, r: 260, css: '#a855f7', alpha: 0.8 });
  }
  if (fx.since(3)) fx.stage.enemies.forEach(e => levelAccent(fx, 'dark', double ? BACK + DUR : SWING + DUR, e));
  // Растворяется во тьме
  fx.tween(END - 0.4, 0.4, p => {
    scythe.alpha = 1 - p;
    if (p >= 1) scythe.visible = false;
  });
  // Руна жатвы под строем
  const rune = fx.dot('ring', '#a855f7', 'back');
  if (fx.since(2)) fx.tween(APPEAR, END, p => {
    const r = 260 * ease.out(Math.min(p * 3, 1));
    rune.oval(cx, cy + 70, r, r * GROUND, fade(p, 0.1, 0.25) * 0.8);
    spray(mist, { x: cx + rand(-220, 220), y: cy + 70 + rand(-30, 30), count: fx.count(12), speed: [5, 30], life: [0.8, 1.3], size: [50, 80] });
  }, [rune]);
  groundGlow(fx, { at: APPEAR, dur: END, x: cx, y: cy + 70, r: 300, css: '#581c87' });
};

// Метка бездны («Печать бездны»): над целью вращается фиолетовый знак, пока метка усиливает урон заклинаний
const abyssMark = (fx: Fx, at: number, T: ConceptUnit, hold = 1.3) => {
  const sigil = fx.gfx('front', 'add');
  const glow = fx.dot('glow', '#9333ea');
  const y0 = T.head.y - 40;
  fx.tween(at, hold, p => {
    const a = fade(p, 0.15, 0.3);
    const r = 34 * ease.back(Math.min(p * 4, 1));
    const spin = p * 3;
    sigil.clear();
    glow.put(T.head.x, y0, 70, 0.5 * a);
    if (r < 1) return;
    sigil.circle(T.head.x, y0, r).stroke({ width: 2.5, color: color('#e9d5ff'), alpha: a });
    sigil.circle(T.head.x, y0, r * 0.7).stroke({ width: 1.2, color: color('#c084fc'), alpha: a * 0.8 });
    const star = Array.from({ length: 5 }, (_, i) => polar({ x: T.head.x, y: y0 }, r * 0.7, (i * 2 / 5) * TAU + spin));
    star.forEach((q, i) => {
      const n = star[(i + 1) % 5];
      sigil.moveTo(q.x, q.y).lineTo(n.x, n.y).stroke({ width: 1.8, color: color('#f5d0fe'), alpha: a });
    });
  }, [glow]);
  ringWave(fx, { at, dur: 0.45, x: T.head.x, y: y0, from: 10, to: 90, css: '#c084fc' });
};

export const DARK_CONCEPTS: Concept[] = [
  {
    id: 'void-orb', name: 'Сфера пустоты', group: 'Тьма', spell: 'Shadow Bolt', duration: 2.1,
    levels: true,
    targets: [1, 1, 1],
    artifacts: [{
      key: 'AbyssalSigil', name: 'Печать бездны', kind: 'MarkTargets',
      about: 'Цель получает метку бездны: над ней вращается знак, урон заклинаний по ней выше на 2 хода',
    }],
    about: 'Чёрный шар втягивает в себя свет по пути, на цели схлопывается и рвётся. Срабатывание от куклы — шар сгущается прямо над проклятым',
    build: perTarget(fx => {
      const { caster, enemies } = fx.stage;
      const T = enemies[0];
      const to = T.center;
      // «Кукла проклятия»: тик проклятия сам рождает шар над целью — он падает на неё сверху
      const doll = fx.triggered('StatusTickProc');
      const from = doll ? { x: T.head.x + 20, y: T.head.y - 230 } : { x: caster.center.x + 40, y: caster.center.y - 30 };
      const halo = fx.dot('glow', '#9333ea');
      const rim = fx.dot('ring', SHADE);
      const orb = fx.dot('disc', '#07030d', 'front', 'normal');
      const pull = fx.sim({ texture: 'dot', drag: 1, shrink: 0.4, tint: palette(C.dark), alpha: life => bump(life) });
      const sparks = fx.sim(P.spark(C.dark, 100));
      const collapse = fx.dot('ring', '#f5d0fe');
      const HIT = 0.95;
      // Шар крупнее и прожорливее с уровнем
      const K = fx.lvl([0.7, 0.85, 1]);
      fx.at(0, () => fx.stage.cast(SHADE));
      fly(fx, 0.25, HIT - 0.25, p => mix(from, to, ease.in(p) * 0.4 + p * 0.6), (pt, p, dir) => {
        const r = (26 + 4 * Math.sin(p * 50)) * K;
        halo.put(pt.x, pt.y, 90, 0.8);
        rim.put(pt.x, pt.y, r + 6, 0.9);
        orb.put(pt.x, pt.y, r);
        for (let i = fx.count(fx.lvl([70, 110, 160])); i > 0; i--) {
          const a = Math.random() * TAU;
          const start = polar(pt, rand(70, 130), a);
          pull.spawn({ x: start.x, y: start.y, vx: -Math.cos(a) * 420 + Math.cos(dir) * 300, vy: -Math.sin(a) * 420 + Math.sin(dir) * 300, lifetime: 0.24, size: rand(3, 7) });
        }
      }, [halo, rim, orb]);
      fx.tween(HIT, 0.18, p => collapse.put(to.x, to.y, lerp(170 * K, 6, ease.in(p)), 0.4 + 0.6 * p), [collapse]);
      fx.at(HIT + 0.18, () => {
        T.hit(true, SHADE);
        fx.stage.shake(8, 0.3);
        spray(sparks, { x: to.x, y: to.y, count: 60, speed: [150, 620], life: [0.4, 1], size: [3, 9] });
      });
      pop(fx, { at: HIT + 0.18, dur: 0.4, x: to.x, y: to.y, r: 190 * K, css: '#a855f7' });
      if (fx.since(3)) levelAccent(fx, 'dark', HIT + 0.18, T);
      if (fx.has('MarkTargets')) abyssMark(fx, HIT + 0.3, T);
      // Кукла: над целью сгущается мгла, из неё рождается шар
      if (doll) {
        const mist = fx.sim(MIST);
        fx.tween(0, 0.35, () => spray(mist, { x: from.x, y: from.y, count: fx.count(30), speed: [10, 50], life: [0.5, 0.9], size: [40, 65], jitter: 30 }));
      }
      ringWave(fx, { at: HIT + 0.18, dur: 0.5, x: to.x, y: to.y, from: 10, to: 190, css: '#f5d0fe' });
      groundGlow(fx, { at: HIT, dur: 1, x: T.feet.x, y: T.feet.y, r: 130, css: '#7e22ce' });
    }, 0.16),
  },
  {
    id: 'shadow-claws', name: 'Когти тени', group: 'Тьма', spell: 'Shadow Bolt', duration: 2.1,
    levels: true,
    about: 'За целью раскрывается разлом, из него выхлёстывают три когтя и оставляют светящиеся раны',
    build(fx) {
      const { enemies } = fx.stage;
      const T = enemies[0];
      const c = T.center;
      const riftAt = { x: c.x + 30, y: c.y - 30 };
      const rift = fx.dot('disc', '#07030d', 'back', 'normal');
      const rim = fx.dot('ring', SHADE, 'back');
      const riftGlow = fx.dot('glow', '#9333ea', 'back');
      const mist = fx.sim(MIST);
      const wisps = fx.sim(WISP);
      const sparks = fx.sim(P.spark(C.dark, 300));
      const OPEN = 0.15;
      const SLASH = 0.5;
      fx.at(0, () => fx.stage.cast(SHADE));
      fx.tween(OPEN, 1.7, p => {
        const open = p < 0.2 ? ease.back(p / 0.2) : p > 0.8 ? 1 - ease.in((p - 0.8) / 0.2) : 1;
        rift.oval(riftAt.x, riftAt.y, 46 * open, 150 * open, 0.95);
        rim.oval(riftAt.x, riftAt.y, 54 * open, 160 * open, 0.9);
        riftGlow.oval(riftAt.x, riftAt.y, 130 * open, 230 * open, 0.7);
        spray(wisps, { x: riftAt.x, y: riftAt.y, count: fx.count(30 * open), speed: [20, 80], life: [0.4, 0.8], size: [10, 22], jitter: 40 });
      }, [rift, rim, riftGlow]);
      fx.at(OPEN, () => spray(mist, { x: c.x, y: c.y, count: 8, speed: [10, 50], life: [0.8, 1.2], size: [40, 70], jitter: 50 }));
      // Когтей: 1 / 2 / 3 по уровню
      const CLAWS = fx.lvl([1, 2, 3]);
      for (let i = 0; i < CLAWS; i++) {
        const at = SLASH + i * 0.08;
        const offset = (i - (CLAWS - 1) / 2) * 46;
        const a = { x: c.x + 90 + offset, y: c.y - 130 };
        const b = { x: c.x - 80 + offset, y: c.y + 110 };
        const control = { x: c.x - 20 + offset, y: c.y - 60 };
        const dark = fx.gfx('front', 'normal');
        const bright = fx.gfx('front', 'add');
        const curve = (from: number, to: number) => path(16, t => bez(a, control, b, lerp(from, to, t)));
        fx.tween(at, 0.1, p => {
          const points = curve(0, ease.out(p));
          taper(dark.clear(), points, 48, '#1e0b36', 0.9);
          taper(bright.clear(), points, 20, '#e879f9', 1);
          taper(bright, points, 6, '#ffffff', 1);
        });
        // Коготь уходит, рана остаётся и медленно гаснет
        fx.tween(at + 0.1, 0.9, p => {
          const points = curve(0, 1);
          taper(dark.clear(), points, 48 * (1 - p), '#1e0b36', 0.9 * (1 - ease.out(Math.min(p * 3, 1))));
          taper(bright.clear(), points, 14, '#e879f9', 1 - ease.in(p));
          taper(bright, points, 4, '#ffffff', 1 - p);
        });
        fx.at(at + 0.08, () => spray(sparks, { x: b.x, y: b.y, count: 14, speed: [100, 420], dir: angle(a, b), spread: 0.6, life: [0.3, 0.7], size: [3, 7] }));
      }
      fx.at(SLASH + 0.12, () => {
        T.hit(true, SHADE);
        fx.stage.shake(10, 0.3);
        spray(wisps, { x: c.x, y: c.y, count: 20, speed: [60, 260], life: [0.5, 1], size: [14, 28] });
      });
      pop(fx, { at: SLASH + 0.12, dur: 0.4, x: c.x, y: c.y, r: 190, css: '#a855f7', alpha: 0.85 });
      groundGlow(fx, { at: OPEN, dur: 1.6, x: T.feet.x, y: T.feet.y, r: 140, css: '#581c87' });
      if (fx.since(3)) levelAccent(fx, 'dark', SLASH + 0.12, T);
    },
  },
  {
    id: 'hex-seal', name: 'Печать проклятия', group: 'Тьма', spell: 'Hex', duration: 2.6,
    levels: true,
    extras: true,
    about: 'Над целью вращается печать, опускается и впечатывается в неё',
    build(fx) {
      const { enemies } = fx.stage;
      const T = enemies[0];
      const seal = new Container();
      seal.blendMode = 'add';
      fx.stage.front.addChild(seal);
      // Печать крупнее с уровнем
      const R = 100 * fx.lvl([0.7, 0.85, 1]);
      const outer = new Graphics().circle(0, 0, R).stroke({ width: 4, color: color(SHADE) }).circle(0, 0, R * 0.82).stroke({ width: 1.5, color: color('#f5d0fe') });
      for (let i = 0; i < 12; i++) {
        const a = (i / 12) * TAU;
        outer.moveTo(Math.cos(a) * R * 0.82, Math.sin(a) * R * 0.82).lineTo(Math.cos(a) * R, Math.sin(a) * R);
      }
      outer.stroke({ width: 2, color: color('#f5d0fe') });
      const star = new Graphics();
      for (const turn of [0, Math.PI]) {
        star.poly([0, 1, 2].flatMap(i => {
          const a = turn + (i / 3) * TAU - Math.PI / 2;
          return [Math.cos(a) * R * 0.8, Math.sin(a) * R * 0.8];
        })).stroke({ width: 3, color: color('#e879f9') });
      }
      star.circle(0, 0, R * 0.3).stroke({ width: 3, color: color('#f5d0fe') });
      seal.addChild(outer, star);
      // До начала спуска печати нет: иначе первые кадры она стоит в (0, 0) — в углу экрана
      seal.visible = false;
      const glow = fx.dot('glow', '#9333ea');
      const wisps = fx.sim(WISP);
      const STAMP = 1;
      fx.at(0, () => fx.stage.cast(SHADE));
      fx.tween(0.15, STAMP - 0.15, p => {
        const y = lerp(T.head.y - 190, T.center.y, ease.in3(p));
        const s = lerp(1.7, 0.75, ease.in(p));
        seal.visible = true;
        seal.position.set(T.center.x, y);
        seal.scale.set(s, s * lerp(0.45, 1, ease.in3(p)));
        seal.alpha = Math.min(1, p * 4);
        outer.rotation = p * 3;
        star.rotation = -p * 4.5;
        glow.put(T.center.x, y, 150 * s, 0.35);
      }, [glow]);
      fx.tween(STAMP, 1.2, p => {
        seal.scale.set(0.75 + ease.out3(p) * 0.25);
        seal.alpha = fade(p, 0, 0.5);
        outer.rotation = 3 + p * 0.6;
        star.rotation = -4.5 - p * 0.9;
        spray(wisps, { x: T.center.x, y: T.center.y, count: fx.count(26), speed: [20, 90], dir: -Math.PI / 2, spread: 1, life: [0.6, 1.1], size: [14, 30], jitter: 40 });
      });
      fx.tween(STAMP + 1.2, 0.01, () => {
        seal.visible = false;
      });
      fx.at(STAMP, () => {
        T.hit(true, SHADE);
        fx.stage.shake(8, 0.3);
        fx.stage.flash('#7e22ce', 0.25, 0.25);
      });
      pop(fx, { at: STAMP, dur: 0.45, x: T.center.x, y: T.center.y, r: 220, css: '#c026d3' });
      if (fx.since(2)) ringWave(fx, { at: STAMP, dur: 0.6, x: T.feet.x, y: T.feet.y, from: 30, to: 240, css: SHADE, squash: GROUND, layer: 'back' });
      if (fx.since(3)) levelAccent(fx, 'dark', STAMP, T);
      // Доп. эффект: порча тикает тьмой
      if (fx.extras) {
        [STAMP + 0.5, STAMP + 0.95].forEach(time => fx.at(time, () => {
          T.hit(false, SHADE);
          spray(wisps, { x: T.center.x, y: T.center.y, count: 14, speed: [40, 160], life: [0.5, 0.9], size: [14, 26] });
        }));
      }
    },
  },
  {
    id: 'dark-eyes', name: 'Глаза во тьме', group: 'Тьма', spell: 'Hex', duration: 2.8,
    levels: true,
    targets: ['all', 'all', 'all'],
    artifacts: [{
      key: 'CurseDoll', name: 'Кукла проклятия', kind: 'StatusTickProc',
      about: 'Каждый тик проклятия с шансом 25% рождает над проклятым шар тьмы',
      then: { concept: 'void-orb', level: 1, targets: 'one', after: 0.8 },
    }],
    about: 'Каждую цель окутывает мгла, из неё по одной открываются светящиеся глаза и рвут когтями',
    build: perTarget(fx => {
      const { enemies } = fx.stage;
      const T = enemies[0];
      const c = T.center;
      const mist = fx.sim(MIST);
      const backMist = fx.sim(MIST, 'back');
      fx.at(0, () => fx.stage.cast(SHADE));
      fx.tween(0.1, 2.1, p => {
        const power = fade(p, 0.2, 0.2);
        for (const sim of [mist, backMist]) {
          spray(sim, { x: c.x, y: c.y + 10, count: fx.count(14 * power), speed: [5, 40], life: [0.8, 1.4], size: [45, 80], jitter: 90 });
        }
      });
      // Пар глаз: 3 / 4 / 5 по уровню (по многим целям — реже)
      const EYES = Math.max(2, Math.round(fx.lvl([3, 4, 5]) * fx.density));
      const eyeSpots = Array.from({ length: EYES }, (_, i) => polar({ x: c.x, y: c.y - 30 }, rand(90, 150), (i / EYES) * TAU + 0.6, 0.75));
      for (let i = 0; i < EYES; i++) {
        const at = 0.5 + i * 0.16;
        const pos = eyeSpots[i];
        const size = rand(0.8, 1.3);
        const tilt = rand(-0.25, 0.25);
        const tint = i % 2 ? '#f0abfc' : '#fb7185';
        // Злобные глаза: мягкий ореол и клиновидный глаз, прищуренный к переносице
        const halo = fx.dot('glow', tint);
        const shape = fx.gfx('front', 'add');
        fx.tween(at, 1.4, p => {
          // Открывается, пару раз моргает, закрывается
          const blink = Math.abs(Math.sin(p * Math.PI * 3)) > 0.12 ? 1 : 0.1;
          const open = fade(p, 0.12, 0.12) * blink;
          halo.oval(pos.x, pos.y, 60 * size, 26 * size * open, 0.45, tilt);
          shape.clear();
          for (const side of [-1, 1]) angryEye(shape, pos.x + side * 17 * size, pos.y + side * tilt * 17 * size, side, 14 * size, 15 * size, open, tint);
        }, [halo]);
      }
      // Тени из мглы рвут цель: шквал маленьких когтей (как у «Когтей тени»), каждый летит со стороны одного из открытых глаз
      const sparks = fx.sim(P.spark(C.dark, 300));
      // Когтей в шквале: 7 / 11 / 16 по уровню
      const CLAWS = Math.max(4, Math.round(fx.lvl([7, 11, 16]) * fx.density));
      const FROM = 0.75;
      const TO = 2.0;
      for (let i = 0; i < CLAWS; i++) {
        const at = lerp(FROM, TO, i / (CLAWS - 1)) + rand(-0.03, 0.03);
        // Глаз k открыт с 0.5 + k·0.16 на 1.4 с
        const open = eyeSpots.filter((_, k) => at >= 0.5 + k * 0.16 && at <= 1.9 + k * 0.16);
        const eye = open[Math.floor(Math.random() * open.length)] ?? eyeSpots[EYES - 1];
        const hitAt = { x: c.x + rand(-45, 45), y: c.y + rand(-60, 50) };
        const dir = angle(eye, hitAt) + rand(-0.5, 0.5);
        const len = rand(70, 100);
        const bend = rand(-1, 1) * 18;
        const a = polar(hitAt, len * 0.55, dir + Math.PI);
        const b = polar(hitAt, len * 0.45, dir);
        const control = { x: hitAt.x - Math.sin(dir) * bend, y: hitAt.y + Math.cos(dir) * bend };
        const dark = fx.gfx('front', 'normal');
        const bright = fx.gfx('front', 'add');
        const curve = (to: number) => path(10, t => bez(a, control, b, t * to));
        fx.tween(at, 0.06, p => {
          const points = curve(ease.out(p));
          taper(dark.clear(), points, 18, '#1e0b36', 0.85);
          taper(bright.clear(), points, 8, '#e879f9', 1);
          taper(bright, points, 2.5, '#ffffff', 1);
        });
        fx.tween(at + 0.06, 0.45, p => {
          const points = curve(1);
          taper(dark.clear(), points, 18 * (1 - p), '#1e0b36', 0.85 * (1 - ease.out(Math.min(p * 3, 1))));
          taper(bright.clear(), points, 6, '#e879f9', 1 - ease.in(p));
          taper(bright, points, 2, '#ffffff', 1 - p);
        });
        const last = i === CLAWS - 1;
        fx.at(at + 0.05, () => {
          spray(sparks, { x: b.x, y: b.y, count: 6, speed: [80, 280], dir, spread: 0.7, life: [0.2, 0.5], size: [2, 5] });
          if (last) {
            T.hit(true, SHADE);
            fx.stage.shake(6, 0.3);
          } else if (i % 3 === 0) {
            T.hit(false, SHADE);
            fx.stage.shake(2, 0.08);
          }
        });
        pop(fx, { at: at + 0.05, dur: 0.18, x: b.x, y: b.y, r: last ? 200 : 50, css: '#a21caf', alpha: last ? 0.8 : 0.6 });
      }
      groundGlow(fx, { at: 0.2, dur: 2.2, x: T.feet.x, y: T.feet.y, r: 170, css: '#581c87' });
      if (fx.since(3)) levelAccent(fx, 'dark', 2.05, T);
    }, 0.12),
  },
  {
    id: 'soul-harvest', name: 'Жатва душ', group: 'Тьма', spell: 'Soul Harvest', duration: 3.4,
    levels: true,
    targets: [2, 2, 3],
    artifacts: [{
      key: 'ReaperScythe', name: 'Коса жнеца', kind: 'ConsumedCurseHeal', heals: 'all',
      about: 'Снятые проклятия становятся лечением: собранные души рассыпаются огоньками и исцеляют весь отряд',
    }],
    about: 'Из каждого врага поднимается белёсая душа с глазами, сжимается в фиолетовый шар и по дуге улетает к кастеру',
    build(fx) {
      const { caster, enemies, allies } = fx.stage;
      const to = { x: caster.center.x, y: caster.center.y - 20 };
      // Душа — белёсая, почти прозрачная: светлая дымка вместо тёмной мглы, бело-голубые огоньки
      const GHOST = ['#ffffff', '#f1f5f9', '#e0f2fe', '#cbd5e1'];
      const GHOST_MIST = { texture: 'smoke' as const, blend: 'normal' as const, drag: 0.3, gravity: -20, shrink: 1.4, tint: () => 0xe2e8f0, alpha: (life: number) => bump(life) * 0.55 };
      // Собранная душа — уже фиолетовый шар тьмы
      const trail = fx.sim(WISP);
      const sparks = fx.sim(P.spark(GHOST, 0));
      const orbSparks = fx.sim(P.spark(C.dark, 0));
      const mist = fx.sim(GHOST_MIST);
      const backMist = fx.sim(GHOST_MIST, 'back');
      const RISE = 0.55;
      const HOVER = 0.3;
      const MORPH = 0.25;
      const FLY = 0.75;
      fx.at(0, () => fx.stage.cast(SHADE));
      // Душа из каждой цели (2 / 2 / 3 по уровню)
      const targets = enemies;
      targets.forEach((enemy, i) => {
        const at = 0.3 + i * 0.16;
        const from = enemy.center;
        const lifted = { x: from.x, y: from.y - 95 };
        const control = { x: (lifted.x + to.x) / 2 + rand(-60, 60), y: Math.min(lifted.y, to.y) - rand(180, 300) };
        const soul = fx.dot('glow', '#f5d0fe');
        const aura = fx.dot('glow', '#a855f7');
        const tint = i % 2 ? '#e0f2fe' : '#f1f5f9';
        const eyes = [-1, 1].map(side => ({ side, glow: fx.dot('glow', tint), pupil: fx.dot('disc', '#ffffff') }));
        const tilt = rand(-0.2, 0.2);
        // Пара глаз: (x, y) — между глазами, gap — полурасстояние, open — насколько открыты
        const drawEyes = (x: number, y: number, gap: number, open: number, size = 1) => {
          for (const eye of eyes) {
            const ex = x + eye.side * gap;
            eye.glow.oval(ex, y, 24 * size, 13 * size * open, 0.9, tilt * eye.side);
            eye.pupil.oval(ex, y, 7 * size, 4.5 * size * open, 1, tilt * eye.side);
          }
        };
        const hideEyes = eyes.flatMap(eye => [eye.glow, eye.pupil]);
        if (fx.since(3)) levelAccent(fx, 'dark', at + 0.1, enemy);
        fx.at(at + 0.1, () => {
          enemy.hit(false, SHADE);
          spray(sparks, { x: from.x, y: from.y, count: 14, speed: [80, 300], life: [0.3, 0.7], size: [3, 7] });
        });
        // Из дворфа поднимается клок мглы, в нём открываются глаза
        const cloudAt = (p: number) => ({ x: from.x + Math.sin(p * 7) * 6, y: lerp(from.y + 10, lifted.y, ease.out(p)) });
        fx.tween(at, 0.1 + RISE + HOVER, p => {
          const power = fade(p, 0.1, 0.3);
          const t = Math.min(Math.max((p * (0.1 + RISE + HOVER) - 0.1) / RISE, 0), 1);
          const pt = cloudAt(t);
          spray(backMist, { x: pt.x, y: pt.y, count: fx.count(20 * power), speed: [5, 30], life: [0.6, 1], size: [40, 65], jitter: 24 });
          spray(mist, { x: pt.x, y: pt.y + 15, count: fx.count(10 * power), speed: [5, 25], life: [0.5, 0.9], size: [30, 50], jitter: 24 });
        });
        fx.tween(at + 0.1, RISE, p => {
          const pt = cloudAt(p);
          drawEyes(pt.x, pt.y, 16, fade(p, 0.35, 0));
        }, hideEyes);
        // Висит над целью и моргает
        fx.tween(at + 0.1 + RISE, HOVER, p => {
          const blink = Math.abs(Math.sin(p * Math.PI * 2)) > 0.15 ? 1 : 0.1;
          const y = lifted.y - Math.sin(p * Math.PI) * 5;
          drawEyes(lifted.x, y, 16, blink);
        }, hideEyes);
        // Глаза щурятся и сходятся в одну точку, там разгорается шар
        const morphAt = at + 0.1 + RISE + HOVER;
        fx.tween(morphAt, MORPH, p => {
          const e = ease.in(p);
          drawEyes(lifted.x, lifted.y, 16 * (1 - e), 1 - 0.7 * e, 1 - 0.5 * e);
          soul.put(lifted.x, lifted.y, 18 * ease.out(p), p);
          aura.put(lifted.x, lifted.y, 60 * ease.out(p), 0.8 * p);
        }, hideEyes);
        fx.at(morphAt + MORPH, () => spray(orbSparks, { x: lifted.x, y: lifted.y, count: 10, speed: [60, 200], life: [0.2, 0.5], size: [2, 5] }));
        pop(fx, { at: morphAt + MORPH, dur: 0.25, x: lifted.x, y: lifted.y, r: 70, css: '#d8b4fe', alpha: 0.7 });
        const flyAt = morphAt + MORPH;
        fly(fx, flyAt, FLY, p => bez(lifted, control, to, ease.inOut(p)), (pt, p) => {
          soul.put(pt.x, pt.y, 18 - 6 * p);
          aura.put(pt.x, pt.y, 50 - 16 * p, 0.8);
          spray(trail, { x: pt.x, y: pt.y, count: fx.count(70), speed: [5, 40], life: [0.3, 0.7], size: [8, 18], jitter: 5 });
        }, [soul, aura]);
        pop(fx, { at: flyAt + FLY, dur: 0.3, x: to.x, y: to.y, r: 100, css: '#d8b4fe', alpha: 0.8 });
      });
      const arrive = 0.3 + (targets.length - 1) * 0.16 + 0.1 + RISE + HOVER + MORPH + FLY;
      fx.at(arrive + 0.05, () => allies[0].heal());
      groundGlow(fx, { at: arrive - FLY, dur: FLY + 0.8, x: caster.feet.x, y: caster.feet.y, r: 150, css: '#7e22ce' });
      // «Коса жнеца»: души рассыпаются лечащими огоньками по отряду
      if (fx.has('ConsumedCurseHeal')) {
        fx.stage.healed.forEach((ally, i) => {
          const at = arrive + 0.15 + i * 0.07;
          const wisp = fx.dot('glow', '#86efac');
          const core = fx.dot('glow', '#f0fdf4');
          const ctrl = { x: (to.x + ally.center.x) / 2, y: Math.min(to.y, ally.center.y) - 110 };
          fly(fx, at, 0.5, p => bez(to, ctrl, ally.center, ease.inOut(p)), pt => {
            wisp.put(pt.x, pt.y, 30, 0.85);
            core.put(pt.x, pt.y, 10);
            spray(trail, { x: pt.x, y: pt.y, count: fx.count(40), speed: [5, 30], life: [0.25, 0.5], size: [6, 12] });
          }, [wisp, core]);
          fx.at(at + 0.5, () => ally.heal());
          pop(fx, { at: at + 0.5, dur: 0.4, x: ally.center.x, y: ally.center.y, r: 110, css: '#86efac', alpha: 0.8 });
        });
      }
    },
  },
  {
    id: 'reaper-scythe', name: 'Коса жнеца', group: 'Тьма', spell: 'Soul Harvest', duration: 2.4,
    levels: true,
    about: 'Огромное лезвие описывает дугу через весь вражеский строй, следом поднимаются души',
    build(fx) {
      const { enemies } = fx.stage;
      const cx = enemies.reduce((sum, e) => sum + e.center.x, 0) / enemies.length;
      const cy = enemies.reduce((sum, e) => sum + e.center.y, 0) / enemies.length;
      const pivot = { x: cx - 30, y: cy + 190 };
      const R = 300;
      const FROM = -Math.PI * 0.86;
      const TO = -Math.PI * 0.14;
      // Лезвие шире с уровнем, души — со 2-го
      const blade = new Beam(fx, 14, fx.lvl([20, 26, 32]), '#a855f7', 'front', '#ffffff');
      const ghost = new Beam(fx, 14, 60, '#581c87', 'front', '#a855f7');
      const souls = fx.sim({ ...WISP, gravity: -260 });
      const sparks = fx.sim(P.spark(C.dark, 200));
      const SWING = 0.4;
      fx.at(0, () => fx.stage.cast(SHADE));
      fx.tween(SWING, 0.34, p => {
        const head = lerp(FROM, TO, ease.inOut(p));
        const tail = (span: number) => path(14, t => polar(pivot, R - t * 30, Math.max(FROM, head - t * span), 0.9));
        blade.set(tail(0.55), fade(p, 0.1, 0.1));
        ghost.set(tail(1.1), fade(p, 0.1, 0.1) * 0.6);
        const tip = polar(pivot, R, head, 0.9);
        spray(sparks, { x: tip.x, y: tip.y, count: fx.count(160), speed: [40, 260], life: [0.3, 0.7], size: [3, 8] });
      });
      fx.tween(SWING + 0.34, 0.4, p => {
        ghost.set(path(14, t => polar(pivot, R - t * 30, lerp(TO, FROM, t), 0.9)), (1 - p) * 0.5);
        if (p >= 1) ghost.hide();
      });
      fx.at(SWING + 0.34, () => blade.hide());
      for (const enemy of enemies) {
        const share = (angle(pivot, enemy.center) - FROM) / (TO - FROM);
        const at = SWING + 0.34 * Math.min(Math.max(share, 0), 1);
        fx.at(at, () => {
          enemy.hit(false, SHADE);
          fx.stage.shake(4, 0.15);
        });
        if (fx.since(2)) fx.tween(at + 0.15, 0.9, () => spray(souls, { x: enemy.center.x, y: enemy.center.y, count: fx.count(14), speed: [10, 60], dir: -Math.PI / 2, spread: 0.5, life: [0.6, 1.1], size: [12, 26], jitter: 24 }));
      }
      if (fx.since(3)) fx.stage.enemies.forEach(e => levelAccent(fx, 'dark', SWING + 0.34, e));
    },
  },
  {
    id: 'reaper-ghost', name: 'Призрачная коса', group: 'Тьма', spell: 'Soul Harvest', duration: 2.9,
    levels: true,
    about: 'Сцена темнеет, над строем проступает огромная коса и взмахом проходит через всех; со 2-го уровня из разрезов рвутся души, на 3-м — взмах туда и обратно',
    build(fx) {
      // 1-й уровень — один простой взмах, 2-й — с душами и руной, 3-й — двойной взмах с волной
      reaperSweep(fx, fx.since(3));
    },
  },
  {
    id: 'reaper-double', name: 'Двойной взмах', group: 'Тьма', spell: 'Soul Harvest', duration: 2.9,
    levels: true,
    about: 'Призрачная коса проходит через строй туда и обратно, следы ложатся крест-накрест, в конце — волна тьмы',
    build(fx) {
      reaperSweep(fx, true);
    },
  },
];
