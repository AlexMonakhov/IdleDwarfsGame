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
    // Сила кирки на момент digEnergyAt (время сервера в ответе) — дальше досчитываем сами (digEnergyNow)
    digEnergy: number;
    digEnergyAt: string | null;
    // Уровни улучшений кирки
    pickaxeCapacityLevel: number;
    pickaxeRecoveryLevel: number;
    pickaxeStrengthLevel: number;
}

// Улучшения кирки — общие на все шахты: запас кликов, восстановление в час, сила (добыча клика)
export type PickaxeUpgrade = 'Capacity' | 'Recovery' | 'Strength';

// value — что даёт сейчас (запас кликов, кликов в час или процент добычи клика), next — после улучшения
export interface PickaxeUpgradeInfo {
    upgrade: PickaxeUpgrade;
    level: number;
    maxLevel: number;
    cost: MineCost | null;
    value: number;
    next: number | null;
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
    // Сколько приносит клик — дробное (у кристаллов меньше единицы); накопленная кликами доля — digProgress
    perClick: number;
    digProgress: number;
    // Добыча всех дворфов в час (без дворфов — сколько даст один) и одного дворфа
    ratePerHour: number;
    ratePerDwarf: number;
    // Склад растёт вместе с числом дворфов
    capacity: number;
    // Дворфы по местам; мест открыто dwarfSlots из maxDwarves, следующее — на уровне nextSlotLevel
    heroIds: string[];
    dwarfSlots: number;
    maxDwarves: number;
    nextSlotLevel: number | null;
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
    // Сила кирки: запас кликов на все шахты и сколько восстанавливается в час
    digEnergyMax: number;
    digEnergyPerHour: number;
    pickaxe: PickaxeUpgradeInfo[];
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
    | 'HeroNotFound' | 'HeroInSquad' | 'HeroBusy' | 'NoFreeSlot' | 'NoDwarf' | 'NothingToCollect' | 'Changed';

// Та же формула, что MiningService.StoredAt: склад дворфов на момент serverNow (мс)
export const storedNow = (mine: MineInfo, serverNow: number): number => {
    if (!mine.heroIds.length) return mine.stored;
    const hours = Math.max(0, serverNow - new Date(mine.storedAt).getTime()) / 3_600_000;
    return Math.max(mine.stored, Math.min(mine.capacity, mine.stored + mine.ratePerHour * hours));
};

// Хватает ли ресурсов кошелька на цену
export const canAfford = (wallet: MiningWallet | null, cost: MineCost | null): boolean => {
    if (!wallet || !cost) return false;
    if (wallet.gold < cost.gold || wallet.experience < cost.experience) return false;
    return !cost.gem || (wallet.crystals[cost.gem] ?? 0) >= cost.crystals;
};

// Та же формула, что MiningService.DigEnergyAt: сила кирки на момент serverNow (мс). null — ни разу не тратилась, полная
export const digEnergyNow = (wallet: MiningWallet, max: number, perHour: number, serverNow: number): number => {
    if (!wallet.digEnergyAt) return max;
    const hours = Math.max(0, serverNow - new Date(wallet.digEnergyAt).getTime()) / 3_600_000;
    return Math.min(max, wallet.digEnergy + hours * perHour);
};

// Сколько приносит клик: у кристаллов это доли — показываем без округления до нуля (0.075, 2.5, 140)
export const formatPerClick = (amount: number, locale: string): string => amount >= 10
    ? Math.round(amount).toLocaleString(locale)
    : amount.toLocaleString(locale, { maximumSignificantDigits: amount < 1 ? 2 : 3 });
