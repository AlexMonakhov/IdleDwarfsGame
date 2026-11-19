// Медиа заднего слоя лендинга: клипы геймплея и щиты бонусов стихий плывут за блоками описаний медленнее
// страницы (параллакс). У каждого блока — один клип про то, о чём текст рядом, и пара щитов его бонусов.
//
// Клипы пока заглушки (ролики дворфов, фоны пещер). Чтобы поставить настоящий — заменить src на свой .mp4
// (4–8 с, бесшовная петля, ~720p, до 1–2 МБ; gif не нужен — mp4 в разы легче) или картинку.
//
// Каждый элемент привязан к блоку (section) и стоит относительно его центра:
//   x      — центр по горизонтали, % ширины окна;
//   dy     — сдвиг от центра блока по вертикали, доли высоты окна;
//   width  — ширина (CSS), высота — по пропорциям (у щита — квадрат);
//   speed  — скорость прокрутки относительно страницы: 0 — висит на месте, 1 — едет вместе с текстом.
//            Чем меньше, тем «дальше» кажется элемент — дальние ещё и темнее;
//   rotate — наклон, градусы;
//   src    — клип или скриншот в рамке; bonus — вместо него щит бонуса стихий со свечением своей стихии.
import { PRISM, SquadBonus, monoBonus, pairBonus } from '@/battle/squadBonuses';
// Записан на настоящей арене (кампания, отряд Плазмы: Огненная волна R4 впереди, остальные R2–R3)
import bonusPlasma from '@/assets/landing/bonus-plasma.mp4';
import dwarvesTavern from '@/assets/landing/dwarves-tavern.webp';
import dwarvesTower from '@/assets/landing/dwarves-tower.webp';
// Настоящие арены, ужатые для лендинга (960 px WebP ~80–100 КБ вместо ~6–7 МБ PNG из assets/arenas)
import arenaRed from '@/assets/landing/arena-red.webp';
import arenaGreen from '@/assets/landing/arena-green.webp';
import arenaViolet from '@/assets/landing/arena-violet.webp';
// Открытие сундука снаряжения с легендарным предметом (стенд сундуков на /testpage)
import chestLegendary from '@/assets/landing/chest-legendary.webp';

export type LandingSection = 'hero' | 'squad' | 'bonus' | 'builder' | 'battle' | 'idle' | 'modes' | 'loot' | 'faq' | 'final';

export interface LandingMedia {
    section: LandingSection;
    x: number;
    dy: number;
    width: string;
    speed: number;
    rotate?: number;
    src?: string;
    bonus?: SquadBonus;
}

export const isVideo = (src: string) => /\.mp4($|\?)/i.test(src);

// Порядок — от дальних к ближним: ближние рисуются поверх
export const LANDING_MEDIA: LandingMedia[] = [
    // Первый экран: большой арт по центру, по краям — клип и щиты
    { section: 'hero', src: '/email/welcome-dwarfs.jpg', x: 50, dy: -0.02, width: 'min(78vw, 1100px)', speed: 0.25 },
    { section: 'hero', bonus: monoBonus('Fire', 3), x: 9, dy: 0.3, width: 'min(11vw, 150px)', speed: 0.5, rotate: -10 },
    // Арт: дворфы Огня, Сияния и Тьмы чокаются кружками
    { section: 'hero', src: dwarvesTavern, x: 87, dy: 0.28, width: 'min(24vw, 330px)', speed: 0.5, rotate: 5 },

    // Отряд (описание слева — медиа справа). Арт: дворфы Огня, Тьмы и Сияния у башни
    { section: 'squad', src: dwarvesTower, x: 72, dy: -0.04, width: 'min(26vw, 360px)', speed: 0.4, rotate: -3 },
    { section: 'squad', bonus: monoBonus('Frost', 3), x: 90, dy: -0.26, width: 'min(9vw, 120px)', speed: 0.7, rotate: 8 },
    { section: 'squad', bonus: monoBonus('Nature', 1), x: 58, dy: 0.3, width: 'min(7vw, 96px)', speed: 0.85, rotate: -12 },

    // Бонусы стихий (описание справа — медиа слева): Плазма срабатывает на арене, значок бонуса в углу вспыхивает
    { section: 'bonus', src: bonusPlasma, x: 30, dy: -0.04, width: 'min(46vw, 640px)', speed: 0.4, rotate: 2 },
    { section: 'bonus', bonus: pairBonus('Fire', 'Lightning'), x: 50, dy: 0.32, width: 'min(10vw, 140px)', speed: 0.7, rotate: -8 },
    { section: 'bonus', bonus: pairBonus('Frost', 'Lightning'), x: 40, dy: -0.32, width: 'min(7vw, 96px)', speed: 0.85, rotate: 10 },

    // Бой. TODO клип: каскад заклинаний (без дворфов 4-го ранга)
    { section: 'battle', src: arenaRed, x: 72, dy: -0.06, width: 'min(38vw, 520px)', speed: 0.4, rotate: -2 },
    { section: 'battle', bonus: pairBonus('Lightning', 'Dark'), x: 90, dy: 0.3, width: 'min(10vw, 140px)', speed: 0.7, rotate: 9 },
    { section: 'battle', bonus: monoBonus('Dark', 2), x: 56, dy: -0.32, width: 'min(7vw, 96px)', speed: 0.85, rotate: -8 },

    // Добыча офлайн. TODO клип: забираем добычу экспедиции
    { section: 'idle', src: arenaGreen, x: 26, dy: -0.06, width: 'min(36vw, 480px)', speed: 0.35, rotate: -2 },
    { section: 'idle', bonus: monoBonus('Nature', 3), x: 9, dy: 0.3, width: 'min(10vw, 140px)', speed: 0.7, rotate: 8 },
    { section: 'idle', bonus: pairBonus('Nature', 'Radiance'), x: 42, dy: -0.32, width: 'min(7vw, 96px)', speed: 0.85, rotate: -10 },

    // Режимы. TODO клип: босс гильдии или PvP со ставками
    { section: 'modes', src: arenaViolet, x: 72, dy: -0.08, width: 'min(36vw, 480px)', speed: 0.35, rotate: 3 },
    { section: 'modes', bonus: pairBonus('Dark', 'Blood'), x: 90, dy: 0.3, width: 'min(10vw, 140px)', speed: 0.7, rotate: -8 },
    { section: 'modes', bonus: monoBonus('Blood', 2), x: 58, dy: 0.34, width: 'min(7vw, 96px)', speed: 0.85, rotate: 10 },

    // Лут: открытие сундука — легендарный амулет среди наград
    { section: 'loot', src: chestLegendary, x: 28, dy: -0.06, width: 'min(46vw, 640px)', speed: 0.4, rotate: -2 },
    { section: 'loot', bonus: monoBonus('Radiance', 3), x: 9, dy: 0.28, width: 'min(10vw, 140px)', speed: 0.7, rotate: 9 },
    { section: 'loot', bonus: PRISM, x: 42, dy: 0.34, width: 'min(7vw, 96px)', speed: 0.85, rotate: -10 },
];
