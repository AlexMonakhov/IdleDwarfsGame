import { HeroElement } from './types';

export type SpriteAnimation = 'idle' | 'walk' | 'attack' | 'victory' | 'death';

// Анимация с конкретным листом (номер варианта в имени файла)
export interface SpritePose {
  animation: SpriteAnimation;
  variant: number;
}

// Цвет героя на спрайте. Basic — герои 1-го ранга без стихии, white — бриллиант (сияние), purple — тьма.
// Violet — кровь, rainbow и negative — дворфы 5-го ранга
export type SpriteColor = 'basic' | 'red' | 'blue' | 'yellow' | 'green' | 'purple' | 'white' | 'violet' | 'rainbow' | 'negative';

export const ELEMENT_COLORS: Record<HeroElement, SpriteColor> = {
  Basic: 'basic',
  Fire: 'red',
  Frost: 'blue',
  Lightning: 'yellow',
  Nature: 'green',
  Dark: 'purple',
  Radiance: 'white',
  Blood: 'violet',
};

export const elementColor = (element?: string | null): SpriteColor =>
  ELEMENT_COLORS[element as HeroElement] ?? 'basic';

const DEFAULT_UNIT = 'dwarf';
// Высший ранг: здесь только два дворфа, свои для тьмы и для всех остальных стихий
const TOP_RANK = 5;

// Цвет спрайта героя: не только стихия, но и ранг. Для рамок и фонов по стихии — elementColor
export const spriteColor = (unit: { unitType?: string; element?: string | null; level: number }): SpriteColor => {
  const color = elementColor(unit.element);
  if (unit.unitType && unit.unitType.toLowerCase() !== DEFAULT_UNIT) return color;
  if (unit.level >= TOP_RANK) return color === 'purple' ? 'negative' : 'rainbow';
  return color;
};

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

// Поправка рисования юнита: размер и положение меняются без пересборки листов и атласов.
// size — во сколько раз крупнее; shiftX, shiftY — сдвиг кадра в долях его ширины (ноги должны остаться на линии 0.84 кадра);
// head — где макушка, в долях высоты рамки, если она не там, куда её переносит size
interface UnitDraw {
  size: number;
  shiftX: number;
  shiftY: number;
  head?: number;
}
const UNIT_DRAW: Record<string, UnitDraw> = {
  // В листах тело левее центра кадра: эффекты и тень попадали мимо него. Голем сутулый: голова ниже плеч и кулака
  golem: { size: 1.4, shiftX: 0.122, shiftY: -0.136, head: 0.22 },
  draconid: { size: 1.2, shiftX: 0, shiftY: -0.068 },
};
const NO_SHIFT = { shiftX: 0, shiftY: 0 };
const drawByUrl = new Map<string, UnitDraw>();

// Файлы называются {юнит}_{анимация}_lvl{ранг}_{цвет}_{вариант}.png, например dwarf_attack_lvl4_red_2.png.
// Индекс строится по именам листов в atlas-meta.json: новый лист попадает в игру после npm run atlas
const SPRITE_FILE = /([a-z]+)_([a-z]+)_lvl(\d+)_([a-z]+)_(\d+)\.png$/;

// Чем рисовать анимацию, которой у цвета ещё нет: у дворфов 5-го ранга есть только часть листов,
// у мобов нет листов крови — они рисуются листами тьмы
const FALLBACK_COLORS: Partial<Record<SpriteColor, SpriteColor>> = { rainbow: 'white', negative: 'purple', violet: 'purple' };

interface SpriteSheet {
  level: number;
  color: string;
  // URL вариантов по порядку номеров (_1, _2, ...)
  variants: string[];
}

// Атлас листа (tools/sprite-atlas.js): те же кадры, обрезанные по персонажу и плотно уложенные.
// Игра грузит и рисует только атлас — в памяти он в 2-5 раз меньше, а сами листы в сборку не попадают
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

// "dwarf_idle" -> все листы этой анимации
const index = new Map<string, SpriteSheet[]>();
Object.keys(atlasMeta)
  .flatMap(name => {
    const match = SPRITE_FILE.exec(name);
    const url = atlasUrls.get(name.replace(/\.png$/, '.webp'));
    if (!match || !url) return [];
    const [, unit, anim, level, color, variant] = match;
    return [{ name, url, unit, key: `${unit}_${anim}`, level: Number(level), color, variant: Number(variant) }];
  })
  .sort((a, b) => a.variant - b.variant)
  .forEach(({ name, url, unit, key, level, color }) => {
    const sheets = index.get(key) ?? [];
    let sheet = sheets.find(s => s.level === level && s.color === color);
    if (!sheet) {
      sheet = { level, color, variants: [] };
      sheets.push(sheet);
      index.set(key, sheets);
    }
    sheet.variants.push(url);
    const draw = UNIT_DRAW[unit];
    if (draw) drawByUrl.set(url, draw);
    const zoom = (spriteMeta[name]?.zoom ?? 1) * (draw?.size ?? 1);
    if (zoom !== 1) zoomByUrl.set(url, zoom);
    atlasBySheet.set(url, { ...atlasMeta[name], url });
  });

export const getSpriteZoom = (url: string): number => zoomByUrl.get(url) ?? 1;

// Сдвиг кадра листа на экране, в долях ширины обычного кадра
export const getSpriteShift = (url: string): { shiftX: number; shiftY: number } => drawByUrl.get(url) ?? NO_SHIFT;

// Во сколько раз юнит крупнее обычного: по этому множителю растут тень и точки привязки эффектов
export const getUnitSize = (unitType?: string): number => UNIT_DRAW[unitType?.toLowerCase() ?? '']?.size ?? 1;

// Своя точка головы юнита, в долях высоты рамки; undefined — обычная
export const getUnitHead = (unitType?: string): number | undefined => UNIT_DRAW[unitType?.toLowerCase() ?? '']?.head;

// Атлас листа по его адресу из getSpriteVariants
export const getSpriteAtlas = (url: string): SpriteAtlas | undefined => atlasBySheet.get(url);

// Лист для ранга героя. Если точного нет — берём ближайший: сначала того же цвета, потом запасного, потом того же уровня.
// Юниты без своих спрайтов рисуются дворфами
export const getSpriteVariants = (
  animType: SpriteAnimation,
  color: SpriteColor,
  rank: number,
  unitType: string = DEFAULT_UNIT,
): string[] => {
  const sheets = index.get(`${unitType.toLowerCase()}_${animType}`) ?? index.get(`${DEFAULT_UNIT}_${animType}`);
  if (!sheets?.length) return [];

  const fallback = FALLBACK_COLORS[color];
  const isColor = (s: SpriteSheet, c: SpriteColor) => s.color === c;
  const distance = (s: SpriteSheet) => (isColor(s, color) ? 0 : fallback && isColor(s, fallback) ? 50 : 100) + Math.abs(s.level - rank);
  return sheets.reduce((best, s) => (distance(s) < distance(best) ? s : best)).variants;
};

// Пустая строка — для этой анимации нет ни одного спрайта
export const getSpriteUrl = (animType: SpriteAnimation, color: SpriteColor, rank: number, variant = 1, unitType?: string): string => {
  const variants = getSpriteVariants(animType, color, rank, unitType);
  return variants[variant - 1] ?? variants[0] ?? '';
};

// Иконки героев: голова с портрета, assets/portraits/icons/{цвет}_lvl{уровень}.png (собирает npm run portraits).
// Цвет — как у спрайтов, кроме этих: герои без стихии нарисованы серыми, тьма лежит в dark, кровь — в purple.
// Своего портрета у негативного дворфа ещё нет
const PORTRAIT_FILE = /([a-z]+)_lvl(\d+)\.png$/;
const PORTRAIT_COLORS: Partial<Record<SpriteColor, string>> = { basic: 'gray', purple: 'dark', violet: 'purple', negative: 'dark' };

const portraitContext = require.context('../assets/portraits/icons', false, /\.png$/);
const portraits = portraitContext.keys().flatMap(path => {
  const match = PORTRAIT_FILE.exec(path);
  return match ? [{ color: match[1], level: Number(match[2]), url: portraitContext(path) as string }] : [];
});

// Иконки мобов: assets/portraits/icons/{вид}_{цвет}.png (цвет как у листов, ранг у мобов один)
const MONSTER_PORTRAIT_FILE = /\/([a-z]+)_([a-z]+)\.png$/;
const monsterPortraits = new Map<string, string>(portraitContext.keys().flatMap((path: string): [string, string][] => {
  const match = MONSTER_PORTRAIT_FILE.exec(path);
  return match ? [[`${match[1]}_${match[2]}`, portraitContext(path) as string]] : [];
}));

// Портрет того же цвета ближайшего уровня; пустая строка — портрета этого цвета нет.
// У мобов свой портрет по виду: того же цвета, запасного (кровь — тьмой, как и спрайт) или без стихии
export const getPortraitUrl = (color: SpriteColor, level: number, unitType: string = DEFAULT_UNIT): string => {
  const unit = unitType.toLowerCase();
  if (unit !== DEFAULT_UNIT) {
    const fallback = FALLBACK_COLORS[color];
    const monster = monsterPortraits.get(`${unit}_${color}`)
      ?? (fallback && monsterPortraits.get(`${unit}_${fallback}`))
      ?? monsterPortraits.get(`${unit}_basic`);
    if (monster) return monster;
  }
  const name = PORTRAIT_COLORS[color] ?? color;
  const same = portraits.filter(p => p.color === name);
  if (!same.length) return '';
  return same.reduce((best, p) => (Math.abs(p.level - level) < Math.abs(best.level - level) ? p : best)).url;
};
