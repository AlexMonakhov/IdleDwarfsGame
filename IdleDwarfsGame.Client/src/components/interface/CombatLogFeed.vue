<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { BattleUnit } from '@/battle/BattleState';
import { DAMAGE_COLORS } from '@/battle/FxRegistry';
import { FeedItem, FeedValue, buildCombatFeed, currentFeedIndex } from '@/battle/combatFeed';
import { CombatLogEntry } from '@/battle/types';
import { STATUS_ICONS, IconHeal } from '@/components/icons/status';
import UnitAvatar from './UnitAvatar.vue';
import { statusLabel } from '@/i18n';

const { t } = useI18n();

const props = withDefaults(defineProps<{
  // Все логи боя — по ним строятся и будущие строки
  logs: CombatLogEntry[];
  // Уже проигранные записи — по ним двигается текущая строка
  shownLogs: CombatLogEntry[];
  units: Record<number, BattleUnit>;
  speed?: number;
}>(), { speed: 1 });

// Видимые слоты: prev, cur, next, nextnext. Крайние (-2 и 3) прозрачны — через них строки въезжают и уезжают
interface Slot { name: string; y: number; scale: number; opacity: number }
const SLOTS: Record<number, Slot> = {
  [-2]: { name: 'gone', y: -42, scale: 0.7, opacity: 0 },
  [-1]: { name: 'prev', y: 0, scale: 0.82, opacity: 0.45 },
  0: { name: 'cur', y: 46, scale: 1, opacity: 1 },
  1: { name: 'next', y: 92, scale: 0.82, opacity: 0.6 },
  2: { name: 'nextnext', y: 134, scale: 0.82, opacity: 0.35 },
  3: { name: 'incoming', y: 176, scale: 0.7, opacity: 0 },
};
const FIRST_SLOT = -2;
const LAST_SLOT = 3;
const SHIFT_MS = 350;
const AVATAR_SIZE = 30;
// Больше портретов в одной строке не влезает
const MAX_TARGETS = 4;

const feed = computed(() => buildCombatFeed(props.logs));
const cursor = computed(() => {
  const lastShown = props.shownLogs.reduce((max, log) => Math.max(max, log.sequence), -Infinity);
  return currentFeedIndex(feed.value, lastShown);
});

const rows = computed(() => {
  const result: { item: FeedItem; slot: Slot }[] = [];
  for (let offset = FIRST_SLOT; offset <= LAST_SLOT; offset++) {
    const item = feed.value[cursor.value + offset];
    if (item) result.push({ item, slot: SLOTS[offset] });
  }
  return result;
});

const rowStyle = (slot: Slot) => ({
  '--y': `${slot.y}px`,
  '--scale': slot.scale,
  '--opacity': slot.opacity,
  '--shift': `${SHIFT_MS / props.speed}ms`,
});

const unitsOf = (ids: number[]) => ids.slice(0, MAX_TARGETS).map(id => props.units[id]).filter(Boolean);

const valueText = (value: FeedValue) => {
  switch (value.kind) {
    case 'damage': return value.crit ? `-${value.amount}!` : `-${value.amount}`;
    case 'heal': return `+${value.amount}`;
    case 'status': return statusLabel(value.name);
    case 'immune': return t('arena.float.immune');
    case 'stunned': return t('arena.float.stunned');
    case 'death': return t('arena.feed.death');
  }
};

const valueStyle = (value: FeedValue) => {
  if (value.kind === 'damage') return { color: DAMAGE_COLORS[value.damageType] ?? '#fff' };
  if (value.kind === 'heal') return { color: DAMAGE_COLORS.Heal };
  return {};
};

const nameOf = (id: number) => props.units[id]?.name ?? (id > 0 ? `#${id}` : '—');

// Полный текст записи — во всплывающей подсказке строки
const describe = (log: CombatLogEntry) => {
  const attacker = nameOf(log.attackerId);
  const target = nameOf(log.targetId);

  switch (log.eventType) {
    case 'RoundStart':
      return t('arena.log.roundStart', { round: log.round });
    case 'Attack':
      return t('arena.log.attack', { attacker, target, damage: log.damage });
    case 'Damage':
      return t('arena.log.damage', {
        skill: log.skillName || t('arena.log.damageDefault'),
        target,
        damage: log.damage,
        damageType: log.damageType,
      });
    case 'StatusApplied':
      return t('arena.log.statusApplied', { target, effect: statusLabel(log.appliedEffects[0]?.name ?? log.actionType) });
    case 'StatusTick':
      return t('arena.log.statusTick', { target, damage: log.damage, source: statusLabel(log.skillName || log.actionType) });
    case 'Heal':
      return log.skillName
        ? t('arena.log.healSkill', { target, heal: log.heal, skill: log.skillName })
        : t('arena.log.heal', { target, heal: log.heal });
    case 'Stunned':
      return t('arena.log.stunned', { attacker });
    case 'Immune': {
      const effect = log.appliedEffects[0]?.name;
      return t('arena.log.immune', { target, effect: effect ? statusLabel(effect) : t('arena.log.effectDefault') });
    }
    case 'Death':
      return t('arena.log.death', { target });
    case 'CombatEnd':
      return log.winningTeam === 1 ? t('arena.log.playerWins') : log.text ?? t('arena.log.enemiesWin');
    default:
      return log.text ?? `${log.actionType || log.eventType}: ${attacker} -> ${target}`;
  }
};
</script>

<template>
  <div v-if="feed.length" class="combat-feed">
    <div
      v-for="{ item, slot } in rows"
      :key="item.key"
      class="feed-row"
      :class="[`feed-${slot.name}`, `feed-type-${item.type}`]"
      :style="rowStyle(slot)"
      :title="describe(item.log)"
    >
      <span v-if="item.type === 'round'" class="feed-round">{{ t('arena.round', { round: item.log.round }) }}</span>

      <span v-else-if="item.type === 'end'" class="feed-end" :class="item.log.winningTeam === 1 ? 'feed-victory' : 'feed-defeat'">
        {{ item.log.winningTeam === 1 ? t('arena.victory') : t('arena.defeat') }}
      </span>

      <template v-else>
        <div class="feed-scene">
          <span v-if="item.actorId && units[item.actorId]" class="feed-avatar" :class="`feed-team-${units[item.actorId].team}`">
            <UnitAvatar :unit="units[item.actorId]" :size="AVATAR_SIZE" :show-level="false" />
          </span>

          <span class="feed-targets">
            <span v-for="unit in unitsOf(item.targetIds)" :key="unit.id" class="feed-avatar" :class="`feed-team-${unit.team}`">
              <UnitAvatar :unit="unit" :size="AVATAR_SIZE" :show-level="false" />
            </span>
            <span v-if="item.targetIds.length > MAX_TARGETS" class="feed-more">+{{ item.targetIds.length - MAX_TARGETS }}</span>
          </span>

          <!-- С действующим лицом иконка между ним и целью, без него — после цели: «юнит — эффект — значение» -->
          <span class="feed-icon" :class="{ 'feed-icon-between': item.actorId }">
            <svg v-if="item.icon?.kind === 'sword'" class="feed-sword" viewBox="0 0 24 24" width="22" height="22">
              <circle cx="2.6" cy="12" r="1.4" fill="#fbbf24" />
              <rect x="3" y="11" width="3.6" height="2" fill="#92400e" />
              <rect x="6.4" y="7.5" width="1.8" height="9" rx="0.6" fill="#fbbf24" />
              <path d="M8.2 10.8 H19.5 L22.5 12 L19.5 13.2 H8.2 Z" fill="#e5e7eb" stroke="#64748b" stroke-width="0.6" />
            </svg>
            <span v-else-if="item.icon?.kind === 'spell'" class="feed-spell" :style="{ color: DAMAGE_COLORS[item.icon.damageType] }">✦</span>
            <IconHeal v-else-if="item.icon?.kind === 'heal'" size="20px" />
            <span v-else-if="item.icon?.kind === 'death'" class="feed-death">💀</span>
            <template v-else-if="item.icon?.kind === 'status'">
              <component v-if="STATUS_ICONS[item.icon.name]" :is="STATUS_ICONS[item.icon.name]" size="20px" />
              <span v-else class="feed-status-letter">{{ statusLabel(item.icon.name)[0] ?? '?' }}</span>
            </template>
          </span>
        </div>

        <span v-if="item.value" class="feed-value" :class="[`feed-value-${item.value.kind}`, { 'feed-crit': item.value.kind === 'damage' && item.value.crit }]" :style="valueStyle(item.value)">
          {{ valueText(item.value) }}
        </span>
      </template>
    </div>
  </div>
</template>

<style scoped>
.combat-feed {
  position: absolute;
  right: 20px;
  top: 50%;
  width: 250px;
  height: 178px;
  transform: translateY(-50%);
  z-index: 12;
  pointer-events: none;
}

.feed-row {
  position: absolute;
  left: 0;
  right: 0;
  top: 0;
  height: 44px;
  box-sizing: border-box;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  padding: 0 12px 0 8px;
  border-radius: 12px;
  background: rgba(10, 15, 24, 0.82);
  border: 1px solid rgba(255, 255, 255, 0.18);
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.35);
  color: #edf2f7;
  transform: translateY(var(--y)) scale(var(--scale));
  opacity: var(--opacity);
  transition: transform var(--shift) ease, opacity var(--shift) ease;
  pointer-events: auto;
}

.feed-cur {
  border-color: rgba(255, 209, 102, 0.75);
  box-shadow: 0 0 14px rgba(255, 209, 102, 0.25), 0 8px 18px rgba(0, 0, 0, 0.45);
}

.feed-type-round,
.feed-type-end {
  justify-content: center;
}

.feed-round {
  color: #ffd166;
  font-weight: 800;
  letter-spacing: 1px;
  text-transform: uppercase;
  font-size: 13px;
}

.feed-end {
  font-weight: 800;
  font-size: 16px;
}

.feed-victory {
  color: #ffd166;
}

.feed-defeat {
  color: #f87171;
}

.feed-scene {
  display: flex;
  align-items: center;
  gap: 6px;
  min-width: 0;
}

.feed-targets {
  order: 2;
}

.feed-scene > .feed-icon {
  order: 3;
}

.feed-scene > .feed-icon-between {
  order: 1;
}

.feed-targets {
  display: flex;
  align-items: center;
}

.feed-targets .feed-avatar + .feed-avatar {
  margin-left: -10px;
}

.feed-avatar {
  display: inline-flex;
  border-radius: 50%;
}

/* Кольцо команды: свои — синие, враги — красные */
.feed-team-1 {
  box-shadow: 0 0 0 2px #60a5fa;
}

.feed-team-2 {
  box-shadow: 0 0 0 2px #f87171;
}

.feed-more {
  margin-left: 4px;
  font-size: 11px;
  font-weight: 700;
  color: #cbd5e1;
}

.feed-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 22px;
  filter: drop-shadow(0 1px 2px rgba(0, 0, 0, 0.8));
}

.feed-spell {
  font-size: 20px;
  line-height: 1;
}

.feed-death {
  font-size: 18px;
  line-height: 1;
}

.feed-status-letter {
  font-weight: 800;
  color: #fde68a;
}

.feed-value {
  flex-shrink: 0;
  font-weight: 800;
  font-size: 17px;
  white-space: nowrap;
  text-shadow: 0 1px 3px #000;
}

.feed-value-status,
.feed-value-immune,
.feed-value-stunned,
.feed-value-death {
  font-size: 13px;
  font-weight: 700;
  color: #fde68a;
}

.feed-value-death {
  color: #cbd5e1;
}

.feed-crit {
  font-size: 20px;
}
</style>
