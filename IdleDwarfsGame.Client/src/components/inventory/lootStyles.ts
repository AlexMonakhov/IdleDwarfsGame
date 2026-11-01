import { HeroElement } from '@/battle/types';
import { EquipmentType } from '@/models/types/Equipment';
import { ChestType, Rarity } from '@/models/types/Inventory';

// Цвета камней совпадают со стихиями героев: рубин — огонь, сапфир — мороз, топаз — молния, изумруд — природа
export const GEM_COLORS: Record<string, string> = {
    Ruby: '#ef4444',
    Sapphire: '#3b82f6',
    Topaz: '#facc15',
    Emerald: '#22c55e',
    Diamond: '#e2e8f0',
    Amethyst: '#a855f7',
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

export const ELEMENT_COLORS: Record<HeroElement, string> = {
    Basic: '#cbd5e1',
    Fire: GEM_COLORS.Ruby,
    Frost: GEM_COLORS.Sapphire,
    Lightning: GEM_COLORS.Topaz,
    Nature: GEM_COLORS.Emerald,
    Dark: GEM_COLORS.Amethyst,
    Radiance: GEM_COLORS.Diamond,
};

export const CHEST_ICONS: Record<ChestType, string> = {
    Hero: '🛡',
    Gem: '💎',
    Infinity: '♾',
    Equipment: '⚒',
};
