import { CombatPowerBreakdown } from './types';

// Оценка боя по шансу победы из прогноза сервера — ключ локали power.verdict.*
export type ForecastVerdict = 'easy' | 'likely' | 'risky' | 'deadly';

export const forecastVerdict = (winChance: number): ForecastVerdict => {
  if (winChance >= 0.9) return 'easy';
  if (winChance >= 0.6) return 'likely';
  if (winChance >= 0.3) return 'risky';
  return 'deadly';
};

export type PowerPart = Exclude<keyof CombatPowerBreakdown, 'total'>;

// Источники мощи в порядке слоёв на сервере; нулевые не показываем (например, уровень у отряда игрока)
export const powerParts = (power: CombatPowerBreakdown): { part: PowerPart; value: number }[] =>
  (['stats', 'level', 'skills', 'passives', 'equipment'] as const)
    .map(part => ({ part, value: power[part] }))
    .filter(({ value }) => !!value);
