import arenaBlue from '@/assets/arenas/arena_blue.png';
import arenaDark from '@/assets/arenas/arena_dark.png';
import arenaGreen from '@/assets/arenas/arena_green.png';
import arenaRed from '@/assets/arenas/arena_red.png';
import arenaViolet from '@/assets/arenas/arena_violet.png';
import arenaWhite from '@/assets/arenas/arena_white.png';
import arenaYellow from '@/assets/arenas/arena_yellow.png';

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

// Неизвестный ключ — арена по умолчанию
export const arenaBackground = (key: string | null | undefined): string =>
    (key && ARENA_BACKGROUNDS[key]) || arenaRed;
