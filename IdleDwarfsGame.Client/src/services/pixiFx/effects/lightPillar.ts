import { Container, Sprite, Texture } from 'pixi.js';
import { ellipseSprite, placeEllipse, radialTexture } from '../art';
import { ParticleSim, clamp01, easeOut, fromFrames, rand } from '../particles';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';

// Столб рассветного света (как LightPillarStrategy): луч сверху раскрывается, у ног разливается сияние,
// сверху опускаются искорки. args: color, core, width, height (высота столба над точкой), rate

let beamTexture: Texture | null = null;
// Столб: по ширине — свет в середине, по высоте — к земле ярче, вверху растворяется
const beam = () => {
  if (beamTexture) return beamTexture;
  const canvas = document.createElement('canvas');
  canvas.width = 64;
  canvas.height = 128;
  const ctx = canvas.getContext('2d');
  if (ctx) {
    const across = ctx.createLinearGradient(0, 0, 64, 0);
    across.addColorStop(0, 'rgba(255,255,255,0)');
    across.addColorStop(0.5, 'rgba(255,255,255,0.5)');
    across.addColorStop(1, 'rgba(255,255,255,0)');
    ctx.fillStyle = across;
    ctx.fillRect(0, 0, 64, 128);
    ctx.globalCompositeOperation = 'destination-in';
    const along = ctx.createLinearGradient(0, 0, 0, 128);
    along.addColorStop(0, 'rgba(255,255,255,0.12)');
    along.addColorStop(1, 'rgba(255,255,255,1)');
    ctx.fillStyle = along;
    ctx.fillRect(0, 0, 64, 128);
  }
  beamTexture = Texture.from(canvas);
  return beamTexture;
};

export const createLightPillar: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const color = tintOf(args.color, '#ffd166');
  const core = tintOf(args.core, '#fffbeb');
  const height = Number(args.height ?? 320);
  const fullWidth = Number(args.width ?? 70);
  const rate = clamp01(Number(args.rate ?? 0.6));

  const root = new Container();
  root.blendMode = 'add';
  const pillar = new Sprite(beam());
  pillar.anchor.set(0.5, 1);
  pillar.tint = core;
  const pool = ellipseSprite(radialTexture([[0, 0.8], [0.5, 0.4], [1, 0]]), core);
  const halos = new ParticleSim({ texture: 'glow', halo: 3, alpha: life => Math.sin(Math.PI * clamp01(life)) });
  const motes = new ParticleSim({ texture: 'disc', alpha: life => Math.sin(Math.PI * clamp01(life)) });
  root.addChild(pillar, pool, halos.container, motes.container);

  return {
    root,
    update(e, dt) {
      const opacity = e.opacity ?? 1;
      root.alpha = opacity;
      const p = progressOf(e);
      const open = easeOut(clamp01(p / 0.2));
      const fade = p > 0.6 ? 1 - (p - 0.6) / 0.4 : 1;
      const width = fullWidth * (0.25 + 0.75 * open);

      pillar.position.set(e.x, e.y);
      pillar.width = width;
      pillar.height = height;
      pillar.alpha = fade;
      placeEllipse(pool, e.x, e.y, width * 1.1, width * 0.3);
      pool.alpha = fade;

      if (p < 0.7 && Math.random() < 1 - Math.pow(1 - rate, dt * 60)) {
        const init = {
          x: e.x + rand(-fullWidth * 0.35, fullWidth * 0.35), y: e.y - rand(height * 0.3, height),
          vx: 0, vy: fromFrames.speed(rand(0.6, 1.4)),
          lifetime: fromFrames.lifetime(rand(0.012, 0.02)),
          size: rand(1.5, 3),
        };
        halos.spawn({ ...init, tint: color });
        motes.spawn({ ...init, size: init.size * 0.6, tint: core });
      }
      halos.step(dt);
      motes.step(dt);
    },
    destroy() {
      halos.destroy();
      motes.destroy();
      root.destroy({ children: true });
    },
  };
};
