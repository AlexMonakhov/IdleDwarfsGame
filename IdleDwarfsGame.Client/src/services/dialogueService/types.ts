import type { BattleUnit } from '@/battle/BattleState';
import type { CombatLogEntry } from '@/battle/types';
import type { AppLocale } from '@/i18n';

// Кто произносит реплику: тот, кто нанёс урон, или тот, кто его получил
export type DialogueSpeaker = 'attacker' | 'target';

// Откуда урон: обычный удар, заклинание или тик статуса (горение и т.п.)
export type DamageSource = 'attack' | 'spell' | 'status';

// Всё, что известно о попадании в момент, когда решается, сказать ли что-нибудь.
// Статусы юнитов — те, что висели ДО этого удара (новые статусы применяются следующими записями лога).
export interface DialogueContext {
  // Нет для урона от статуса: у тика нет атакующего
  attacker?: BattleUnit;
  target: BattleUnit;
  source: DamageSource;
  damage: number;
  crit: boolean;
  // Удар добил цель
  killed: boolean;
  // Исходная запись лога — если правилу нужно что-то ещё (skillName, damageType...)
  log: CombatLogEntry;
}

// Фразы по языкам. Английский обязателен: он используется, если для текущего языка фраз нет.
// В тексте можно писать {attacker} и {target} — подставятся имена.
export type LocalizedPhrases = { en: string[] } & Partial<Record<AppLocale, string[]>>;

export interface DialogueRule {
  // Уникальное имя — для отладки и чтобы было проще искать правило
  id: string;
  speaker: DialogueSpeaker;
  // Какие существа могут это сказать (unitType говорящего, см. UNIT в dialogueRules.ts).
  // Не указано — любые. Тип противника проверяйте в when (isType).
  speakerTypes?: string[];
  // Из всех подошедших правил выбираются правила с наибольшим приоритетом (по умолчанию 0)
  priority?: number;
  // Функция проверки: подходит ли правило к этому попаданию
  when: (ctx: DialogueContext) => boolean;
  phrases: LocalizedPhrases;
}
