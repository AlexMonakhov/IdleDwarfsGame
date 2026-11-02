import { HeroElement } from '@/battle/types';
import { EquipmentType } from '@/models/types/Equipment';
import { ChestType, Rarity } from '@/models/types/Inventory';
import itemPlaceholder from '@/assets/portraits/icons/gray_lvl3.png';
import darkIcon from '@/assets/svgs/elements/dark.svg';
import fireIcon from '@/assets/svgs/elements/red.svg';
import bloodIcon from '@/assets/svgs/elements/blood.svg';
import frostIcon from '@/assets/svgs/elements/frost.svg';
import lightningIcon from '@/assets/svgs/elements/lighting.svg';
import natureIcon from '@/assets/svgs/elements/nature.svg';
import radianceIcon from '@/assets/svgs/elements/radiance.svg';

// Цвета камней совпадают со стихиями героев: рубин — огонь, сапфир — мороз, топаз — молния, изумруд — природа
export const GEM_COLORS: Record<string, string> = {
    Ruby: '#ef4444',
    Sapphire: '#3b82f6',
    Topaz: '#facc15',
    Emerald: '#22c55e',
    Diamond: '#e2e8f0',
    Amethyst: '#a855f7',
    Garnet: '#e91e63',
};

export const RARITY_COLORS: Record<Rarity, string> = {
    Common: '#94a3b8',
    Uncommon: '#4ade80',
    Rare: '#60a5fa',
    Mythic: '#c084fc',
    Legendary: '#fbbf24',
    Artifact: '#ef4444',
};

// Значки предметов и пустых слотов, пока нет своей графики (см. ASSETS_TODO.md)
export const EQUIPMENT_ICONS: Record<EquipmentType, string> = {
    Weapon: '⚔',
    Helmet: '⛑',
    Neck: '📿',
    Ring: '💍',
};

// Картинки предметов: assets/items/{ключ предмета}.webp. Пока нарисованы не все, остальным — заглушка
const itemImages = require.context('@/assets/items', false, /\.webp$/);

export const getItemImage = (key: string): string => {
    const path = `./${key}.webp`;
    return itemImages.keys().includes(path) ? itemImages(path) as string : itemPlaceholder;
};

export const ELEMENT_COLORS: Record<HeroElement, string> = {
    Basic: '#cbd5e1',
    Fire: GEM_COLORS.Ruby,
    Frost: GEM_COLORS.Sapphire,
    Lightning: GEM_COLORS.Topaz,
    Nature: GEM_COLORS.Emerald,
    Dark: GEM_COLORS.Amethyst,
    Radiance: GEM_COLORS.Diamond,
    Blood: GEM_COLORS.Garnet,
};

// У стихии Basic значка нет
export const ELEMENT_ICONS: Partial<Record<HeroElement, string>> = {
    Fire: fireIcon,
    Frost: frostIcon,
    Lightning: lightningIcon,
    Nature: natureIcon,
    Dark: darkIcon,
    Radiance: radianceIcon,
    Blood: bloodIcon,
};

export const CHEST_ICONS: Record<ChestType, string> = {
    Hero: '🛡',
    Gem: '💎',
    Infinity: '♾',
    Equipment: '⚒',
};
