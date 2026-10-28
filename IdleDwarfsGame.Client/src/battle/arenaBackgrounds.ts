import arenaRedCrystals from '@/assets/arena_red_crystals.png';
import arenaBlueCrystals from '@/assets/arena_blue_crystals.png';

// Фоны арен по ключу с сервера (Arena.Background у кампании и боссов гильдии).
// Новая картинка арены — импорт и ключ здесь
const ARENA_BACKGROUNDS: Record<string, string> = {
    arena_blue_crystals: arenaRedCrystals,
    lava: arenaRedCrystals,
    ice: arenaBlueCrystals,
    // TODO: своих картинок пока нет (см. ASSETS_TODO.md) — временно чужой фон, перекрашенный фильтром ниже
    storm: arenaBlueCrystals,
    marsh: arenaRedCrystals,
    crypt: arenaBlueCrystals,
    sanctum: arenaBlueCrystals,
    // Арена PvP
    colosseum: arenaRedCrystals,
};

// CSS-фильтр, которым временный фон перекрашивается в цвет стихии арены.
// Когда у арены появится своя картинка — убрать её строку отсюда
const PLACEHOLDER_FILTERS: Record<string, string> = {
    storm: 'hue-rotate(-150deg) saturate(1.3) brightness(1.1)',
    marsh: 'hue-rotate(95deg) saturate(0.9)',
    crypt: 'hue-rotate(60deg) saturate(1.2) brightness(0.8)',
    sanctum: 'saturate(0.25) brightness(1.25)',
    colosseum: 'sepia(0.6) saturate(1.4) hue-rotate(-15deg) brightness(0.95)',
};

// Неизвестный ключ — арена по умолчанию
export const arenaBackground = (key: string | null | undefined): string =>
    (key && ARENA_BACKGROUNDS[key]) || arenaRedCrystals;

export const arenaBackgroundFilter = (key: string | null | undefined): string =>
    (key && PLACEHOLDER_FILTERS[key]) || 'none';
