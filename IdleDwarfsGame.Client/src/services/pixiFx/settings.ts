import type { EffectType } from '@/services/particleService/types/emitter';

// Отладка Pixi-эффектов (отдельный модуль без Pixi: его импортирует сцена, а сам Pixi грузится лениво).
// only — рисовать только эти типы эффектов, skip — все, кроме этих (найти, какой тормозит)
// unitGlow: false — без свечения силуэтов юнитов (размытие — самый дорогой проход у юнита)
export const pixiFxSettings: { only: Set<EffectType> | null; skip: Set<EffectType>; unitGlow: boolean } = {
  only: null,
  skip: new Set(),
  unitGlow: true,
};

// Из консоли и стенда замеров (и в релизной сборке — замеры идут на ней): window.__pixiFx.only = new Set(['explosion'])
(window as unknown as { __pixiFx: typeof pixiFxSettings }).__pixiFx = pixiFxSettings;
