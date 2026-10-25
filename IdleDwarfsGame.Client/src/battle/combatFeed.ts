import { CombatLogEntry, DamageType } from './types';

// Что нарисовано между портретами (или после портрета цели)
export type FeedIcon =
  | { kind: 'sword' }
  | { kind: 'spell'; damageType: DamageType }
  | { kind: 'status'; name: string }
  | { kind: 'heal' }
  | { kind: 'death' };

// Что написано в конце строки
export type FeedValue =
  | { kind: 'damage'; amount: number; damageType: DamageType; crit: boolean }
  | { kind: 'heal'; amount: number }
  | { kind: 'status'; name: string }
  | { kind: 'immune' }
  | { kind: 'stunned' }
  | { kind: 'death' };

// Строка ленты боя (см. CombatLogFeed): одна запись лога или несколько одинаковых подряд
export interface FeedItem {
  // sequence первой записи: строка становится текущей, когда эту запись проиграли
  key: number;
  log: CombatLogEntry;
  type: 'round' | 'action' | 'end';
  // Кто действует (удар, заклинание, лечение союзника) — слева от иконки. Без него строка про то, что случилось с целью
  actorId?: number;
  targetIds: number[];
  icon?: FeedIcon;
  value?: FeedValue;
}

const byOther = (log: CombatLogEntry) => log.attackerId > 0 && log.attackerId !== log.targetId;

function toItem(log: CombatLogEntry): FeedItem | null {
  const base = { key: log.sequence, log, targetIds: [log.targetId] };
  const damage: FeedValue = { kind: 'damage', amount: log.damage, damageType: log.damageType, crit: log.isCritical };

  switch (log.eventType) {
    case 'RoundStart':
      return { ...base, type: 'round', targetIds: [] };
    case 'CombatEnd':
      return { ...base, type: 'end', targetIds: [] };
    case 'Attack':
      return { ...base, type: 'action', actorId: log.attackerId, icon: { kind: 'sword' }, value: damage };
    case 'Damage':
      return {
        ...base,
        type: 'action',
        actorId: byOther(log) ? log.attackerId : undefined,
        icon: { kind: 'spell', damageType: log.damageType },
        value: damage,
      };
    case 'StatusTick': {
      const name = log.activeEffects[0]?.name ?? (log.skillName || log.actionType);
      if (log.heal > 0) return { ...base, type: 'action', icon: { kind: 'status', name }, value: { kind: 'heal', amount: log.heal } };
      // Тик без урона на сцене ничего не показывает — и в ленте его нет
      if (log.damage <= 0) return null;
      return { ...base, type: 'action', icon: { kind: 'status', name }, value: damage };
    }
    case 'StatusApplied': {
      const name = log.appliedEffects[0]?.name ?? log.actionType;
      return { ...base, type: 'action', icon: { kind: 'status', name }, value: { kind: 'status', name } };
    }
    case 'Immune':
      return { ...base, type: 'action', icon: { kind: 'status', name: log.appliedEffects[0]?.name ?? '' }, value: { kind: 'immune' } };
    case 'Heal':
      return {
        ...base,
        type: 'action',
        actorId: byOther(log) ? log.attackerId : undefined,
        icon: { kind: 'heal' },
        value: { kind: 'heal', amount: log.heal },
      };
    case 'Stunned':
      return { ...base, type: 'action', targetIds: [log.attackerId], icon: { kind: 'status', name: 'Stun' }, value: { kind: 'stunned' } };
    case 'Death':
      return { ...base, type: 'action', icon: { kind: 'death' }, value: { kind: 'death' } };
    default:
      // CombatStart, StatusExpired и SkillCast: урон и эффекты каста идут отдельными записями
      return null;
  }
}

// Аура или массовое заклинание вешает один статус на всех подряд — это одна строка с несколькими целями
function canMerge(prev: FeedItem, item: FeedItem) {
  return prev.log.eventType === 'StatusApplied'
    && item.log.eventType === 'StatusApplied'
    && prev.log.attackerId === item.log.attackerId
    && prev.value?.kind === 'status'
    && item.value?.kind === 'status'
    && prev.value.name === item.value.name;
}

export function buildCombatFeed(logs: CombatLogEntry[]): FeedItem[] {
  const items: FeedItem[] = [];
  for (const log of [...logs].sort((a, b) => a.sequence - b.sequence)) {
    const item = toItem(log);
    if (!item) continue;

    const prev = items[items.length - 1];
    if (prev && canMerge(prev, item)) {
      item.targetIds.filter(id => !prev.targetIds.includes(id)).forEach(id => prev.targetIds.push(id));
      continue;
    }
    items.push(item);
  }
  return items;
}

// Текущая строка — последняя, чью первую запись уже проиграли; -1 — бой ещё не начался
export function currentFeedIndex(items: FeedItem[], lastShownSequence: number): number {
  let index = -1;
  for (let i = 0; i < items.length && items[i].key <= lastShownSequence; i++) index = i;
  return index;
}
