import arenaRedCrystals from '@/assets/arena_red_crystals.png';
import arenaBlueCrystals from '@/assets/arena_blue_crystals.png';

// Фоны арен по ключу с сервера (Arena.Background у кампании, Background у боссов гильдии).
// Новая картинка арены — импорт и ключ здесь
const ARENA_BACKGROUNDS: Record<string, string> = {
    arena_blue_crystals: arenaRedCrystals,
    lava: arenaRedCrystals,
    ice: arenaBlueCrystals,
};

// Неизвестный ключ — арена по умолчанию
export const arenaBackground = (key: string | null | undefined): string =>
    (key && ARENA_BACKGROUNDS[key]) || arenaRedCrystals;
