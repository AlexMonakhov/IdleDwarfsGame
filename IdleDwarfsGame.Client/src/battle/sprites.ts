import { HeroElement } from './types';

export type SpriteAnimation = 'idle' | 'attack' | 'victory' | 'death';

// Цвет в имени файла спрайта. Basic — герои 1-го уровня без стихии, white — бриллиант (сияние)
export type SpriteColor = 'basic' | 'red' | 'blue' | 'yellow' | 'green' | 'purple' | 'white';

export const ELEMENT_COLORS: Record<HeroElement, SpriteColor> = {
  Basic: 'basic',
  Fire: 'red',
  Frost: 'blue',
  Lightning: 'yellow',
  Nature: 'green',
  Dark: 'purple',
  Radiance: 'white',
};

export const elementColor = (element?: string | null): SpriteColor =>
  ELEMENT_COLORS[element as HeroElement] ?? 'basic';

// Спрайт-листы: 5 колонок кадров по 409.6px (лист 2048x2048)
export const SPRITE_COLUMNS = 5;

// Файлы называются {юнит}_{анимация}_lvl{уровень}_{цвет}_{вариант}.png, например dwarf_attack_lvl4_red_2.png.
// Папка не важна — индекс строится по имени файла, так что новый спрайт достаточно просто положить в sprite-lists
const SPRITE_FILE = /([a-z]+)_([a-z]+)_lvl(\d+)_([a-z]+)_(\d+)\.png$/;
const DEFAULT_UNIT = 'dwarf';

interface SpriteSheet {
  level: number;
  color: string;
  // URL вариантов по порядку номеров (_1, _2, ...)
  variants: string[];
}

const requireContext = require.context('../assets/sprite-lists', true, /\.png$/);

// "dwarf_idle" -> все листы этой анимации
const index = new Map<string, SpriteSheet[]>();
requireContext.keys()
  .flatMap(path => {
    const match = SPRITE_FILE.exec(path);
    if (!match) return [];
    const [, unit, anim, level, color, variant] = match;
    return [{ path, key: `${unit}_${anim}`, level: Number(level), color, variant: Number(variant) }];
  })
  .sort((a, b) => a.variant - b.variant)
  .forEach(({ path, key, level, color }) => {
    const sheets = index.get(key) ?? [];
    let sheet = sheets.find(s => s.level === level && s.color === color);
    if (!sheet) {
      sheet = { level, color, variants: [] };
      sheets.push(sheet);
      index.set(key, sheets);
    }
    sheet.variants.push(requireContext(path));
  });

// Если точного листа нет — берём ближайший: сначала того же цвета, потом того же уровня.
// Юниты без своих спрайтов рисуются дворфами
export const getSpriteVariants = (
  animType: SpriteAnimation,
  color: string,
  level: number,
  unitType: string = DEFAULT_UNIT,
): string[] => {
  const sheets = index.get(`${unitType.toLowerCase()}_${animType}`) ?? index.get(`${DEFAULT_UNIT}_${animType}`);
  if (!sheets?.length) return [];

  const distance = (s: SpriteSheet) => (s.color === color ? 0 : 100) + Math.abs(s.level - level);
  return sheets.reduce((best, s) => (distance(s) < distance(best) ? s : best)).variants;
};

// Пустая строка — для этой анимации нет ни одного спрайта
export const getSpriteUrl = (animType: SpriteAnimation, color: string, level: number, variant = 1, unitType?: string): string => {
  const variants = getSpriteVariants(animType, color, level, unitType);
  return variants[variant - 1] ?? variants[0] ?? '';
};
