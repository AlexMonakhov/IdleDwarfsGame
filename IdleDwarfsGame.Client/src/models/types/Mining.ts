// Шахты (IdleDwarfsGame.Server/Controllers/Responses/MiningResponses.cs, Services/MiningService.cs)

export type GemType = 'Ruby' | 'Sapphire' | 'Topaz' | 'Emerald' | 'Amethyst' | 'Diamond' | 'Garnet';

export type MineType = 'Gold' | 'Experience' | GemType;

export const MINE_TYPES: MineType[] = ['Gold', 'Experience', 'Ruby', 'Sapphire', 'Topaz', 'Emerald', 'Amethyst', 'Diamond', 'Garnet'];

export type MineUpgrade = 'Level' | 'Speed' | 'Yield' | 'Storage';

// Кристаллы — гемы стихии gem (обычные, 1-го уровня)
export interface MineCost {
    gold: number;
    experience: number;
    gem: GemType | null;
    crystals: number;
}

export interface MiningWallet {
    gold: number;
    experience: number;
    // Камней, которых нет, в словаре нет
    crystals: Partial<Record<GemType, number>>;
}

export interface MineUpgradeInfo {
    upgrade: MineUpgrade;
    level: number;
    // Сейчас доступный максимум: у скорости, добычи и склада он растёт с уровнем шахты
    maxLevel: number;
    // null — дальше пока нельзя
    cost: MineCost | null;
    // Скорость — добыча дворфа в час, добыча — за клик, склад — вместимость, уровень — сам уровень
    value: number;
    next: number | null;
}

export interface MineInfo {
    id: string;
    plot: number;
    type: MineType;
    gem: GemType | null;
    level: number;
    maxLevel: number;
    perClick: number;
    ratePerHour: number;
    capacity: number;
    heroId: string | null;
    // Накоплено дворфом на момент storedAt — дальше досчитываем сами (storedNow)
    stored: number;
    storedAt: string;
    upgrades: MineUpgradeInfo[];
}

export interface MiningBoard {
    serverTime: string;
    plotCount: number;
    dwarfUnlockLevel: number;
    maxClicksPerSecond: number;
    buildCost: MineCost;
    mines: MineInfo[];
    wallet: MiningWallet;
}

export interface MiningActionResult {
    mine: MineInfo;
    wallet: MiningWallet;
}

export interface MiningDigResult extends MiningActionResult {
    clicks: number;
    mined: number;
}

export interface MiningCollectResult extends MiningActionResult {
    collected: number;
}

// Коды отказа сервера: по ним показываем текст mining.errors.<code>
export type MiningError =
    | 'PlotNotFound' | 'PlotTaken' | 'MineNotFound' | 'MaxLevel' | 'MineLevelTooLow'
    | 'NotEnoughGold' | 'NotEnoughExperience' | 'NotEnoughCrystals' | 'DwarfLocked'
    | 'HeroNotFound' | 'HeroInSquad' | 'HeroBusy' | 'NoDwarf' | 'NothingToCollect' | 'Changed';

// Та же формула, что MiningService.StoredAt: склад дворфа на момент serverNow (мс)
export const storedNow = (mine: MineInfo, serverNow: number): number => {
    if (!mine.heroId) return mine.stored;
    const hours = Math.max(0, serverNow - new Date(mine.storedAt).getTime()) / 3_600_000;
    return Math.max(mine.stored, Math.min(mine.capacity, mine.stored + mine.ratePerHour * hours));
};

// Хватает ли ресурсов кошелька на цену
export const canAfford = (wallet: MiningWallet | null, cost: MineCost | null): boolean => {
    if (!wallet || !cost) return false;
    if (wallet.gold < cost.gold || wallet.experience < cost.experience) return false;
    return !cost.gem || (wallet.crystals[cost.gem] ?? 0) >= cost.crystals;
};
