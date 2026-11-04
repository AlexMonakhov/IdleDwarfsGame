import type { AuraTarget, HeroElement } from '@/battle/types';
import type { HeroInfo, HeroUpgradeCost } from '@/models/types/Heroes';
import type { PlayerResources } from '@/models/types/Expeditions';

// Способности дворфов (IdleDwarfsGame.Server/Services/AbilityService.cs): извлекаются распылением дворфа,
// вставляются другому, две одинаковые сливаются в одну уровнем выше. Иммунитеты не извлекаются
export type AbilityKind = 'Spell' | 'Passive' | 'Aura';

export const ABILITY_KINDS: AbilityKind[] = ['Spell', 'Passive', 'Aura'];

// Сколько дворфов одного ранга сливаются в одного рангом выше
export const FUSION_SIZE = 5;
// Доля вложенного в прокачку слитых дворфов, которая возвращается игроку (HeroUpgradeService.FusionRefund)
export const FUSION_REFUND = 0.3;
// Выше этого ранга дворфов пока не выдают — их не получить и слиянием
export const MAX_FUSION_RANK = 4;

// Способность: извлечённая (id есть) или на дворфе (id null)
export interface AbilityInfo {
    id: string | null;
    kind: AbilityKind;
    // Spell — Id первого уровня семейства, Passive — название пассивки, Aura — эффект ауры
    key: string;
    level: number;
    maxLevel: number;
    // Заклинание
    spellId: number | null;
    spellName: string | null;
    element: HeroElement | null;
    // Пассивка
    percent: number;
    turns: number;
    targets: number;
    // Аура
    auraTarget: AuraTarget | null;
    power: number;
    duration: number;
}

// Все извлечённые способности игрока и затронутый дворф (получивший способность или новый после слияния)
export interface AbilityActionResult {
    abilities: AbilityInfo[];
    hero: HeroInfo | null;
    // Только у слияния: возврат за прокачку слитых дворфов и ресурсы после него
    refund?: HeroUpgradeCost | null;
    resources?: PlayerResources | null;
}

// Коды отказа сервера: по ним показываем текст abilities.errors.<code>
export type AbilityError =
    | 'HeroNotFound' | 'HeroBusy' | 'LastHero' | 'NothingToExtract' | 'AbilityNotFound' | 'ElementMismatch'
    | 'CannotCombine' | 'MaxLevel' | 'WrongHeroCount' | 'DifferentRanks' | 'MaxRank' | 'Changed';

// Заклинание вставляется только дворфу своей стихии, пассивки и ауры — любому (как проверяет сервер)
export const canInsert = (ability: AbilityInfo, hero: HeroInfo) =>
    ability.kind !== 'Spell' || ability.element === hero.element;

// Две способности сливаются, если это одна и та же и сумма уровней не выше максимума
export const canCombine = (a: AbilityInfo, b: AbilityInfo) =>
    a.id !== b.id && a.kind === b.kind && a.key === b.key && a.level + b.level <= a.maxLevel;
