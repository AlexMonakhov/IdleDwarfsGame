import arenaBlue from '@/assets/arenas/arena_blue.png';
import arenaDark from '@/assets/arenas/arena_dark.png';
import arenaGreen from '@/assets/arenas/arena_green.png';
import arenaRed from '@/assets/arenas/arena_red.png';
import arenaViolet from '@/assets/arenas/arena_violet.png';
import arenaWhite from '@/assets/arenas/arena_white.png';
import arenaYellow from '@/assets/arenas/arena_yellow.png';
import type { HeroElement } from './types';
import caveBasic from '@/assets/caves/cave_basic.webp';
import caveBlue from '@/assets/caves/cave_blue.webp';
import caveDark from '@/assets/caves/cave_dark.webp';
import caveGreen from '@/assets/caves/cave_green.webp';
import caveRed from '@/assets/caves/cave_red.webp';
import caveViolet from '@/assets/caves/cave_violet.webp';
import caveWhite from '@/assets/caves/cave_white.webp';
import caveYellow from '@/assets/caves/cave_yellow.webp';

// Фоны арен по ключу с сервера (Arena.Background у кампании и боссов гильдии).
// Новая картинка арены — файл в assets/arenas, импорт и ключ здесь
const ARENA_BACKGROUNDS: Record<string, string> = {
    lava: arenaRed,
    ice: arenaBlue,
    storm: arenaYellow,
    marsh: arenaGreen,
    crypt: arenaDark,
    sanctum: arenaWhite,
    altar: arenaViolet,
    // Арена PvP
    colosseum: arenaViolet,
};

// Превью тех же арен для карточек (кампания, башня, PvP, боссы гильдии): пещеры с кристаллами цвета арены,
// маленькие и в WebP (~70 КБ против ~6 МБ большой арены). У тьмы и сияния кристаллы с радужным переливом.
// Собираются скриптом npm run caves
const ARENA_PREVIEWS: Record<string, string> = {
    lava: caveRed,
    ice: caveBlue,
    storm: caveYellow,
    marsh: caveGreen,
    crypt: caveDark,
    sanctum: caveWhite,
    altar: caveViolet,
    colosseum: caveViolet,
};

// Неизвестный ключ — арена по умолчанию. Это фон самого боя
export const arenaBackground = (key: string | null | undefined): string =>
    (key && ARENA_BACKGROUNDS[key]) || arenaRed;

// Фон карточки арены (превью); неизвестный ключ — красная пещера
export const arenaPreview = (key: string | null | undefined): string =>
    (key && ARENA_PREVIEWS[key]) || caveRed;

// Пещера стихии героя (сцена прокачки): та же, что у арены этой стихии; у героев без стихии — серая
const ELEMENT_PREVIEWS: Record<HeroElement, string> = {
    Basic: caveBasic,
    Fire: caveRed,
    Frost: caveBlue,
    Lightning: caveYellow,
    Nature: caveGreen,
    Dark: caveDark,
    Radiance: caveWhite,
    Blood: caveViolet,
};

export const elementPreview = (element: string | null | undefined): string =>
    ELEMENT_PREVIEWS[element as HeroElement] ?? caveBasic;
