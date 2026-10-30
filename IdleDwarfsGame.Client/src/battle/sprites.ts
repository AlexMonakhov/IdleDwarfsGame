import { HeroElement } from './types';

export type SpriteAnimation = 'idle' | 'walk' | 'attack' | 'victory' | 'death';

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

// Листы, чей кадр охватывает в zoom раз большую область (атаки с прыжками и шлейфами) — рисуются в zoom раз крупнее.
// Файл пишет tools/sprite-sheets.js; размер кадра в пикселях берётся из самого листа (ширина / колонки)
interface SpriteMeta {
  zoom?: number;
}
const metaContext = require.context('../assets/sprite-lists', false, /sprite-meta\.json$/);
const spriteMeta: Record<string, SpriteMeta> = metaContext.keys().length ? metaContext(metaContext.keys()[0]) : {};
const zoomByUrl = new Map<string, number>();

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

// Атлас листа (tools/sprite-atlas.js): те же кадры, обрезанные по персонажу и плотно уложенные.
// Игра грузит и рисует атлас, а не сам лист — в памяти он в 2-5 раз меньше
// Кадр: x, y, ширина, высота в атласе и смещение от угла своей клетки в листе; null — пустой кадр
export type AtlasFrame = [number, number, number, number, number, number];
export interface SpriteAtlas {
  url: string;
  // Сторона клетки листа (кадра) в пикселях атласа
  cell: number;
  width: number;
  height: number;
  frames: (AtlasFrame | null)[];
}
type AtlasMeta = Omit<SpriteAtlas, 'url'>;

const atlasContext = require.context('../assets/sprite-atlas', false, /\.webp$/);
const atlasMetaContext = require.context('../assets/sprite-atlas', false, /atlas-meta\.json$/);
const atlasMeta: Record<string, AtlasMeta> = atlasMetaContext.keys().length ? atlasMetaContext(atlasMetaContext.keys()[0]) : {};
const atlasUrls = new Map(atlasContext.keys().map(path => [path.split('/').pop() ?? '', atlasContext(path) as string]));
const atlasBySheet = new Map<string, SpriteAtlas>();

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
    const url = requireContext(path);
    sheet.variants.push(url);
    const name = path.split('/').pop() ?? '';
    const zoom = spriteMeta[name]?.zoom;
    if (zoom) zoomByUrl.set(url, zoom);
    const atlasUrl = atlasUrls.get(name.replace(/\.png$/, '.webp'));
    if (atlasMeta[name] && atlasUrl) atlasBySheet.set(url, { ...atlasMeta[name], url: atlasUrl });
  });

export const getSpriteZoom = (url: string): number => zoomByUrl.get(url) ?? 1;

// Атлас листа; undefined — атлас не собран, рисуется сам лист
export const getSpriteAtlas = (url: string): SpriteAtlas | undefined => atlasBySheet.get(url);

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

// Иконки героев: голова с портрета, assets/portraits/icons/{цвет}_lvl{уровень}.png (собирает npm run portraits).
// Цвет — как у спрайтов, только герои без стихии нарисованы серыми
const PORTRAIT_FILE = /([a-z]+)_lvl(\d+)\.png$/;
const PORTRAIT_COLORS: Partial<Record<SpriteColor, string>> = { basic: 'gray' };

const portraitContext = require.context('../assets/portraits/icons', false, /\.png$/);
const portraits = portraitContext.keys().flatMap(path => {
  const match = PORTRAIT_FILE.exec(path);
  return match ? [{ color: match[1], level: Number(match[2]), url: portraitContext(path) as string }] : [];
});

// Портрет того же цвета ближайшего уровня; пустая строка — портрета этого цвета нет
export const getPortraitUrl = (color: SpriteColor, level: number): string => {
  const name = PORTRAIT_COLORS[color] ?? color;
  const same = portraits.filter(p => p.color === name);
  if (!same.length) return '';
  return same.reduce((best, p) => (Math.abs(p.level - level) < Math.abs(best.level - level) ? p : best)).url;
};
