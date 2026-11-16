import {
  Beam, C, GROUND, P, TAU, bez, bump, crackle, ease, fade, fly, groundGlow, jag, lerp, levelAccent, mix, polar, pop, rand, ringWave, spray,
  perTarget, type Concept, type Pt,
} from './kit';

// Концепты молнии
const BOLT = '#fde68a';
const FLASH = '#fff7d6';

export const LIGHTNING_CONCEPTS: Concept[] = [
  {
    id: 'bolt-sky', name: 'Удар с неба', group: 'Молния', spell: 'Lightning', duration: 1.7,
    levels: true,
    targets: [1, 2, 2],
    extras: true,
    artifacts: [{
      key: 'ThunderHammer', name: 'Молот грома', kind: 'EchoEveryNth',
      about: 'Каждый третий каст повторяется: по тем же врагам бьёт золотой разряд молота',
      then: { concept: 'bolt-sky', level: 3, targets: 'same', after: 0.65 },
    }],
    about: 'Тонкий лидер нащупывает цель, следом бьёт толстый разряд со вспышкой; в каждую цель свой разряд',
    build: perTarget(fx => {
      const { enemies } = fx.stage;
      const T = enemies[0];
      // Повтор «Молота грома» — золотой разряд; срабатывание «Рога грома» по оглушённым — бело-голубой, с гулом рога
      const hammer = fx.triggered('EchoEveryNth');
      const horn = fx.triggered('SpellProcOnStatus');
      const tint = hammer ? '#ffd166' : horn ? '#bfdbfe' : BOLT;
      const sky = { x: T.head.x - 90, y: -40 };
      const leader = new Beam(fx, 14, 3, tint);
      // Разряд толще с уровнем, ответвлений 0 / 1 / 2
      const main = new Beam(fx, 14, fx.lvl([10, 13, 16]) * (hammer ? 1.3 : 1), tint);
      const forks = Array.from({ length: fx.lvl([0, 1, 2]) }, () => new Beam(fx, 6, 6, tint));
      // Рог: перед ударом вокруг цели расходятся кольца звука
      if (horn) [0, 0.12, 0.24].forEach(d => ringWave(fx, { at: d, dur: 0.4, x: T.center.x, y: T.center.y, from: 30, to: 150, css: '#bfdbfe', alpha: 0.7 }));
      // Молот: печать-наковальня над целью перед ударом
      if (hammer) {
        pop(fx, { at: 0.05, dur: 0.45, x: sky.x + 40, y: 60, r: 140, css: '#ffd166' });
        ringWave(fx, { at: 0.5, dur: 0.5, x: T.feet.x, y: T.feet.y, from: 40, to: 300, css: '#ffd166', squash: GROUND, layer: 'back' });
      }
      const sparks = fx.sim(P.spark(C.bolt, 500));
      const STRIKE = 0.5;
      fx.at(0, () => fx.stage.cast(BOLT));
      // Лидер ползёт вниз рывками
      const route = jag(sky, T.head, 14, 26);
      fx.tween(0.12, STRIKE - 0.12, p => {
        const reach = Math.floor(ease.in(p) * 13);
        leader.set(route.map((_, i) => route[Math.min(i, reach)]), 0.9);
      });
      fx.at(STRIKE, () => {
        leader.hide();
        T.hit(true, FLASH);
        fx.stage.flash('#ffffff', 0.4, 0.2);
        fx.stage.shake(9, 0.3);
        spray(sparks, { x: T.feet.x, y: T.feet.y, count: 50, speed: [150, 650], dir: -Math.PI / 2, spread: 1.4, life: [0.3, 0.8], size: [3, 8] });
      });
      crackle(fx, main, STRIKE, 0.42, () => jag(sky, T.feet, 14, 34));
      forks.forEach((fork, i) => crackle(fx, fork, STRIKE, 0.3, () => {
        const start = mix(sky, T.feet, 0.35 + i * 0.25);
        return jag(start, { x: start.x + (i ? 1 : -1) * rand(90, 170), y: start.y + rand(60, 130) }, 6, 16);
      }));
      pop(fx, { at: STRIKE, dur: 0.4, x: T.center.x, y: T.center.y, r: 220, css: BOLT });
      ringWave(fx, { at: STRIKE, dur: 0.5, x: T.feet.x, y: T.feet.y, from: 30, to: 230, css: FLASH, squash: GROUND, layer: 'back' });
      groundGlow(fx, { at: STRIKE, dur: 0.9, x: T.feet.x, y: T.feet.y, r: 150, css: '#ffb300' });
      if (fx.since(3)) levelAccent(fx, 'bolt', STRIKE + 0.05, T);
      // Доп. эффект: вокруг цели добивают ещё три малых молнии
      if (fx.extras) {
        for (let i = 0; i < 3; i++) {
          const at = STRIKE + 0.18 + i * 0.1;
          const spot = { x: T.feet.x + (i - 1) * 90 + rand(-15, 15), y: T.feet.y + rand(-20, 25) };
          crackle(fx, new Beam(fx, 10, 8, BOLT), at, 0.25, () => jag({ x: spot.x - 60, y: -40 }, spot, 10, 24));
          fx.at(at, () => spray(sparks, { x: spot.x, y: spot.y, count: 18, speed: [120, 420], dir: -Math.PI / 2, spread: 1.2, life: [0.3, 0.6], size: [3, 6] }));
          ringWave(fx, { at, dur: 0.35, x: spot.x, y: spot.y, from: 10, to: 90, css: FLASH, squash: GROUND, layer: 'back' });
        }
      }
    }, 0.15),
  },
  {
    id: 'ball-lightning', name: 'Шаровая молния', group: 'Молния', spell: 'Lightning', duration: 2.4,
    levels: true,
    extras: true,
    about: 'Медленный трескучий шар доплывает до цели и лопается разрядами во все стороны',
    build(fx) {
      const { caster, enemies } = fx.stage;
      const T = enemies[0];
      const from = { x: caster.center.x + 50, y: caster.center.y - 40 };
      const to = T.center;
      const halo = fx.dot('glow', '#ffb300');
      const core = fx.dot('glow', '#ffffff');
      // Шар крупнее и трескучее с уровнем, лучей при разрыве 5 / 7 / 10
      const K = fx.lvl([0.72, 0.86, 1]);
      const arcs = Array.from({ length: fx.lvl([3, 4, 5]) }, () => new Beam(fx, 5, 3, BOLT));
      const rays = Array.from({ length: fx.lvl([5, 7, 10]) }, () => new Beam(fx, 7, 7, BOLT));
      const sparks = fx.sim(P.spark(C.bolt, 300));
      const BURST = 1.3;
      let next = 0;
      let spots: Pt[][] = [];
      fx.at(0, () => fx.stage.cast(BOLT));
      fly(fx, 0.25, BURST - 0.25, p => {
        const base = mix(from, to, ease.inOut(p));
        return { x: base.x, y: base.y + Math.sin(p * TAU * 2) * 22 };
      }, (pt, p) => {
        const pulse = 1 + Math.sin(p * 60) * 0.12;
        halo.put(pt.x, pt.y, 80 * pulse * K, 0.9);
        core.put(pt.x, pt.y, 30 * pulse * K);
        const elapsed = p * (BURST - 0.25);
        if (elapsed >= next) {
          next = elapsed + 0.06;
          spots = arcs.map(() => jag(pt, polar(pt, rand(45, 85), Math.random() * TAU), 5, 10));
        }
        arcs.forEach((arc, i) => arc.set(spots[i].map(q => ({ x: q.x + (pt.x - spots[i][0].x), y: q.y + (pt.y - spots[i][0].y) })), 0.9));
        spray(sparks, { x: pt.x, y: pt.y, count: fx.count(40), speed: [40, 200], life: [0.2, 0.5], size: [2, 5] });
      }, [halo, core]);
      fx.at(BURST, () => {
        arcs.forEach(arc => arc.hide());
        T.hit(true, FLASH);
        enemies.slice(1, fx.lvl([1, 2, 3])).forEach(e => e.hit(false, FLASH));
        fx.stage.flash('#fff7d6', 0.35, 0.2);
        fx.stage.shake(10, 0.3);
        spray(sparks, { x: to.x, y: to.y, count: 70, speed: [200, 800], life: [0.3, 0.9], size: [3, 8] });
      });
      rays.forEach((ray, i) => crackle(fx, ray, BURST, 0.35, () => jag(to, polar(to, rand(140, 230), (i / rays.length) * TAU + rand(-0.2, 0.2)), 7, 18)));
      pop(fx, { at: BURST, dur: 0.45, x: to.x, y: to.y, r: 260 * K, css: BOLT });
      ringWave(fx, { at: BURST, dur: 0.5, x: to.x, y: to.y, from: 30, to: 220, css: FLASH });
      if (fx.since(3)) levelAccent(fx, 'bolt', BURST, T);
      // Доп. эффект: от разрыва к соседям прыгают разряды
      if (fx.extras) {
        fx.stage.bystanders.slice(0, 2).forEach((e, i) => {
          crackle(fx, new Beam(fx, 10, 8, BOLT), BURST + 0.05 + i * 0.08, 0.35, () => jag(to, e.center, 10, 22));
          pop(fx, { at: BURST + 0.05 + i * 0.08, dur: 0.3, x: e.center.x, y: e.center.y, r: 110, css: BOLT });
        });
      }
    },
  },
  {
    id: 'chain-web', name: 'Цепь разрядов', group: 'Молния', spell: 'Chain Lightning', duration: 2.3,
    levels: true,
    about: 'Разряд перескакивает с врага на врага, звенья догорают по очереди',
    build(fx) {
      const { caster, enemies } = fx.stage;
      // Прыжков цепи: 2 / 3 / 4 по уровню
      const nodes = [caster.center, ...enemies.slice(0, fx.lvl([2, 3, 4])).map(e => e.center)];
      const sparks = fx.sim(P.spark(C.bolt, 400));
      fx.at(0, () => fx.stage.cast(BOLT));
      for (let i = 1; i < nodes.length; i++) {
        const at = 0.25 + (i - 1) * 0.17;
        const a = nodes[i - 1];
        const b = nodes[i];
        crackle(fx, new Beam(fx, 11, 9, BOLT), at, 0.5, () => jag(a, b, 11, 22));
        if (fx.since(2)) crackle(fx, new Beam(fx, 9, 3, '#ffffff'), at, 0.4, () => jag(a, b, 9, 40), p => fade(p, 0.05, 0.5) * 0.7);
        fx.at(at, () => {
          enemies[i - 1].hit(i === nodes.length - 1, FLASH);
          fx.stage.shake(4, 0.15);
          spray(sparks, { x: b.x, y: b.y, count: 26, speed: [150, 520], life: [0.3, 0.7], size: [3, 7] });
        });
        pop(fx, { at, dur: 0.35, x: b.x, y: b.y, r: 130, css: BOLT });
        // Дуги по телу после удара
        for (let k = 0; k < 2; k++) {
          crackle(fx, new Beam(fx, 6, 3, BOLT), at + 0.1 + k * 0.12, 0.3, () =>
            jag({ x: b.x + rand(-40, 40), y: b.y - rand(40, 90) }, { x: b.x + rand(-40, 40), y: b.y + rand(40, 90) }, 6, 14));
        }
      }
      if (fx.since(3)) {
        const last = enemies[nodes.length - 2];
        levelAccent(fx, 'bolt', 0.25 + (nodes.length - 2) * 0.17, last);
      }
    },
  },
  {
    id: 'ground-surge', name: 'Разряд по земле', group: 'Молния', spell: 'Chain Lightning', duration: 2.4,
    levels: true,
    targets: [2, 3, 4],
    artifacts: [{
      key: 'StormCoil', name: 'Грозовая катушка', kind: 'EnergyRefund',
      about: 'За каждого задетого врага в кастера возвращается энергия — искры бегут из целей обратно к нему',
    }],
    about: 'Молния змеится по камням от врага к врагу и бьёт из-под ног вверх',
    build(fx) {
      const { caster, enemies } = fx.stage;
      // Прыжков по земле — сколько целей (2 / 3 / 4 по уровню), от ближней к дальней
      const targets = [...enemies].sort((a, b) => Math.abs(a.feet.x - caster.feet.x) - Math.abs(b.feet.x - caster.feet.x));
      const stops = [caster.feet, ...targets.map(e => e.feet)];
      const sparks = fx.sim(P.spark(C.bolt, 600));
      fx.at(0, () => fx.stage.cast(BOLT));
      for (let i = 1; i < stops.length; i++) {
        const at = 0.25 + (i - 1) * 0.22;
        const a = stops[i - 1];
        const b = stops[i];
        const target = targets[i - 1];
        // Бегущая по земле голова
        const run = new Beam(fx, 9, 7, BOLT, 'back');
        const route = jag(a, b, 9, 16);
        fx.tween(at, 0.22, p => {
          const reach = Math.floor(p * 8);
          run.set(route.map((_, n) => route[Math.min(n, reach)]), 1);
          const head = route[reach];
          spray(sparks, { x: head.x, y: head.y, count: fx.count(120), speed: [80, 320], dir: -Math.PI / 2, spread: 0.9, life: [0.2, 0.5], size: [2, 6] });
        });
        crackle(fx, run, at + 0.22, 0.4, () => jag(a, b, 9, 16));
        crackle(fx, new Beam(fx, 8, 11, BOLT), at + 0.22, 0.38, () => jag(b, { x: target.head.x, y: target.head.y - 70 }, 8, 20));
        fx.at(at + 0.22, () => {
          target.hit(i === stops.length - 1, FLASH);
          fx.stage.shake(5, 0.2);
          spray(sparks, { x: b.x, y: b.y, count: 30, speed: [200, 700], dir: -Math.PI / 2, spread: 0.6, life: [0.3, 0.8], size: [3, 7] });
        });
        ringWave(fx, { at: at + 0.22, dur: 0.4, x: b.x, y: b.y, from: 20, to: 130, css: FLASH, squash: GROUND, layer: 'back' });
        groundGlow(fx, { at: at + 0.1, dur: 0.9, x: b.x, y: b.y, r: 110, css: '#ffb300' });
        if (fx.since(3)) levelAccent(fx, 'bolt', at + 0.22, target);
      }
      // «Грозовая катушка»: из каждой цели к кастеру бегут искры энергии, кастер подзаряжается
      if (fx.has('EnergyRefund')) {
        const back = 0.25 + targets.length * 0.22 + 0.25;
        targets.forEach((target, i) => {
          const spark = fx.dot('glow', '#fde68a');
          const core = fx.dot('glow', '#ffffff');
          const start = target.center;
          const end = { x: caster.center.x, y: caster.center.y - 10 };
          const ctrl = { x: (start.x + end.x) / 2, y: Math.min(start.y, end.y) - 140 - i * 30 };
          fly(fx, back + i * 0.06, 0.5, p => bez(start, ctrl, end, ease.inOut(p)), pt => {
            spark.put(pt.x, pt.y, 26, 0.9);
            core.put(pt.x, pt.y, 9);
            spray(sparks, { x: pt.x, y: pt.y, count: fx.count(50), speed: [20, 90], life: [0.15, 0.35], size: [2, 4] });
          }, [spark, core]);
        });
        const charged = back + (targets.length - 1) * 0.06 + 0.5;
        pop(fx, { at: charged, dur: 0.45, x: caster.center.x, y: caster.center.y, r: 150, css: BOLT });
        crackle(fx, new Beam(fx, 8, 4, BOLT), charged, 0.4, () => jag({ x: caster.center.x + rand(-40, 40), y: caster.feet.y - 10 }, { x: caster.center.x + rand(-40, 40), y: caster.head.y }, 8, 16));
      }
    },
  },
  {
    id: 'thunder-dome', name: 'Громовой купол', group: 'Молния', spell: 'Thunderclap', duration: 2.2,
    levels: true,
    targets: ['all', 'all', 'all'],
    artifacts: [{
      key: 'ThunderHorn', name: 'Рог грома', kind: 'SpellProcOnStatus',
      about: 'Оглушённых хлопком добивает молния с гулом рога',
      then: { concept: 'bolt-sky', level: 1, targets: 'same', after: 0.6 },
    }],
    about: 'Кастер копит заряд и разряжает его куполом: волны по земле и молнии во всех врагов',
    build(fx) {
      const { caster, enemies } = fx.stage;
      const c = caster.center;
      const charge = fx.dot('glow', BOLT);
      const chargeCore = fx.dot('glow', '#ffffff');
      const pull = fx.sim({ texture: 'dot', drag: 1, shrink: 0.5, alpha: life => bump(life) });
      const sparks = fx.sim(P.spark(C.bolt, 300));
      const dome = fx.dot('ring', FLASH);
      const CLAP = 0.6;
      fx.at(0, () => fx.stage.cast(BOLT));
      fx.tween(0, CLAP, p => {
        charge.put(c.x, c.y - 20, 40 + 80 * p, 0.5 + 0.5 * p);
        chargeCore.put(c.x, c.y - 20, 10 + 26 * p);
        for (let i = fx.count(110); i > 0; i--) {
          const a = Math.random() * TAU;
          const start = polar({ x: c.x, y: c.y - 20 }, rand(120, 200), a);
          pull.spawn({ x: start.x, y: start.y, vx: -Math.cos(a) * 520, vy: -Math.sin(a) * 520, lifetime: 0.28, size: rand(3, 6), tint: 0xfde68a });
        }
      }, [charge, chargeCore]);
      fx.at(CLAP, () => {
        fx.stage.flash('#ffffff', 0.5, 0.25);
        fx.stage.shake(14, 0.45);
        enemies.forEach(e => e.hit(false, FLASH));
        spray(sparks, { x: c.x, y: c.y, count: 60, speed: [200, 800], life: [0.3, 0.8], size: [3, 8] });
      });
      fx.tween(CLAP, 0.55, p => {
        const r = lerp(60, 520, ease.out3(p));
        dome.oval(c.x, caster.feet.y, r, r * 0.8, (1 - p) * 0.8);
      }, [dome]);
      // Волн по земле: 1 / 2 / 3 по уровню
      [0, 0.1, 0.2].slice(0, fx.lvl([1, 2, 3])).forEach((delay, i) => ringWave(fx, {
        at: CLAP + delay, dur: 0.7, x: caster.feet.x, y: caster.feet.y, from: 40, to: 700 - i * 120, css: i ? BOLT : FLASH, squash: GROUND, layer: 'back', alpha: 1 - i * 0.25,
      }));
      // Молнии во все цели
      enemies.forEach(e => {
        crackle(fx, new Beam(fx, 12, 9, BOLT), CLAP, 0.45, () => jag({ x: c.x, y: c.y - 20 }, e.center, 12, 26));
        pop(fx, { at: CLAP + 0.03, dur: 0.35, x: e.center.x, y: e.center.y, r: 120, css: BOLT });
      });
      pop(fx, { at: CLAP, dur: 0.5, x: c.x, y: c.y - 20, r: fx.lvl([220, 280, 330]), css: FLASH });
      if (fx.since(3)) enemies.forEach(e => levelAccent(fx, 'bolt', CLAP + 0.05, e));
    },
  },
  {
    id: 'storm-cloud', name: 'Грозовая туча', group: 'Молния', spell: 'Thunderclap', duration: 3,
    levels: true,
    extras: true,
    about: 'Над врагами сгущается туча и бьёт молниями по каждому',
    build(fx) {
      const { enemies } = fx.stage;
      const left = Math.min(...enemies.map(e => e.head.x)) - 80;
      const right = Math.max(...enemies.map(e => e.head.x)) + 80;
      const cloudY = Math.min(...enemies.map(e => e.head.y)) - 190;
      const cloud = fx.sim({ texture: 'smoke', blend: 'normal', drag: 0.3, shrink: 1.25, tint: () => 0x1e293b, alpha: life => bump(life) * 0.7 });
      const glow = fx.sim({ texture: 'glow', drag: 1, shrink: 1, tint: () => 0xfde68a, alpha: life => bump(life) * 0.8 });
      const sparks = fx.sim(P.spark(C.bolt, 500));
      const rain = fx.sim({ texture: 'disc', drag: 1, shrink: 1, stretch: [0.12, 2.6], tint: () => 0x93c5fd, alpha: life => bump(life) * 0.5 });
      fx.at(0, () => fx.stage.cast(BOLT));
      fx.tween(0, 2.3, p => {
        const power = fade(p, 0.2, 0.2);
        for (let i = fx.count(60 * power); i > 0; i--) {
          cloud.spawn({ x: rand(left, right), y: cloudY + rand(-30, 30), vx: rand(-30, 30), vy: rand(-8, 8), lifetime: rand(0.9, 1.5), size: rand(40, 75) });
        }
        for (let i = fx.count(fx.lvl([80, 110, 150]) * power); i > 0; i--) {
          rain.spawn({ x: rand(left, right), y: cloudY + rand(0, 40), vx: -60, vy: rand(900, 1200), lifetime: rand(0.3, 0.45), size: rand(6, 10), rotation: 0.06 });
        }
      });
      // Молний: на 1-м уровне по двум врагам, со 2-го — по всем
      enemies.slice(0, fx.lvl([2, enemies.length, enemies.length])).forEach((e, i) => {
        const at = 0.7 + i * 0.27;
        const from = { x: e.head.x + rand(-60, 60), y: cloudY + 20 };
        crackle(fx, new Beam(fx, 10, 11, BOLT), at, 0.3, () => jag(from, e.feet, 10, 22));
        fx.at(at, () => {
          e.hit(false, FLASH);
          fx.stage.flash('#fff7d6', 0.18, 0.15);
          fx.stage.shake(6, 0.2);
          glow.spawn({ x: from.x, y: from.y, vx: 0, vy: 0, lifetime: 0.3, size: 150 });
          spray(sparks, { x: e.feet.x, y: e.feet.y, count: 24, speed: [150, 560], dir: -Math.PI / 2, spread: 1.2, life: [0.3, 0.7], size: [3, 7] });
        });
        pop(fx, { at, dur: 0.3, x: e.center.x, y: e.center.y, r: 140, css: BOLT });
        if (fx.since(3)) levelAccent(fx, 'bolt', at, e);
        ringWave(fx, { at, dur: 0.4, x: e.feet.x, y: e.feet.y, from: 20, to: 130, css: FLASH, squash: GROUND, layer: 'back' });
      });
      // Доп. эффект: напоследок туча бьёт огромной молнией в главную цель
      if (fx.extras) {
        const T = enemies[0];
        const at = 0.7 + enemies.length * 0.27 + 0.1;
        const from = { x: T.head.x, y: cloudY + 20 };
        crackle(fx, new Beam(fx, 12, 20, BOLT), at, 0.4, () => jag(from, T.feet, 12, 30));
        fx.at(at, () => {
          T.hit(true, FLASH);
          fx.stage.flash('#ffffff', 0.4, 0.2);
          fx.stage.shake(11, 0.3);
          spray(sparks, { x: T.feet.x, y: T.feet.y, count: 50, speed: [150, 650], dir: -Math.PI / 2, spread: 1.4, life: [0.3, 0.8], size: [3, 8] });
        });
        pop(fx, { at, dur: 0.4, x: T.center.x, y: T.center.y, r: 220, css: BOLT });
      }
    },
  },
];
