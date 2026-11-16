import type { ColorMatrix } from 'pixi.js';

// Матрицы цвета 4×5 (как у ColorMatrixFilter Pixi: смещения в долях 0..1) для вида юнита:
// серость мёртвого, вид статусов, наложение цвета (как source-atop заливка у canvas-юнита)

export const IDENTITY: ColorMatrix = [1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0];

// Произведение a ∘ b: сначала b, потом a
export const multiply = (a: ColorMatrix, b: ColorMatrix): ColorMatrix => {
  const out: number[] = new Array(20).fill(0);
  for (let row = 0; row < 4; row++) {
    for (let col = 0; col < 5; col++) {
      let sum = col === 4 ? a[row * 5 + 4] : 0;
      for (let k = 0; k < 4; k++) sum += a[row * 5 + k] * b[k * 5 + col];
      out[row * 5 + col] = sum;
    }
  }
  return out as ColorMatrix;
};

// CSS saturate(s)
export const saturate = (s: number): ColorMatrix => [
  0.213 + 0.787 * s, 0.715 - 0.715 * s, 0.072 - 0.072 * s, 0, 0,
  0.213 - 0.213 * s, 0.715 + 0.285 * s, 0.072 - 0.072 * s, 0, 0,
  0.213 - 0.213 * s, 0.715 - 0.715 * s, 0.072 + 0.928 * s, 0, 0,
  0, 0, 0, 1, 0,
];

// CSS brightness(b)
export const brightness = (b: number): ColorMatrix => [b, 0, 0, 0, 0, 0, b, 0, 0, 0, 0, 0, b, 0, 0, 0, 0, 0, 1, 0];

// Заливка цветом поверх непрозрачных пикселей с силой alpha (как source-atop у canvas): rgb → rgb·(1−a) + цвет·a
export const overlay = (color: number, alpha: number): ColorMatrix => {
  const a = Math.min(Math.max(alpha, 0), 1);
  const r = ((color >> 16) & 255) / 255;
  const g = ((color >> 8) & 255) / 255;
  const b = (color & 255) / 255;
  return [1 - a, 0, 0, 0, r * a, 0, 1 - a, 0, 0, g * a, 0, 0, 1 - a, 0, b * a, 0, 0, 0, 1, 0];
};

// Силуэт цветом (для свечения): rgb = цвет, альфа своя
export const solid = (color: number): ColorMatrix => overlay(color, 1);
