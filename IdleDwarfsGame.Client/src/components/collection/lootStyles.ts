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
    Epic: '#c084fc',
    Legendary: '#fbbf24',
};

export const CHEST_ICONS: Record<ChestType, string> = {
    Hero: '🛡',
    Gem: '💎',
    Infinity: '♾',
    Equipment: '⚒',
};
