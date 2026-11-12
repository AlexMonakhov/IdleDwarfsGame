<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import UnitAvatar from './UnitAvatar.vue';
import { IconSkull } from '@/components/icons/ui';
import type { CombatResult, CombatUnitInfo, CombatUnitStats } from '@/battle/types';

// Детали боя: урон по источникам (удары, умения, аура, пассивки), лечение, полученный урон и добивания каждого бойца
const props = defineProps<{
    modelValue: boolean;
    result: CombatResult | null;
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', value: boolean): void;
}>();

const { t } = useI18n();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

type Kind = 'attack' | 'skill' | 'aura' | 'passive';
const KINDS: Kind[] = ['attack', 'skill', 'aura', 'passive'];
const KIND_COLORS: Record<Kind, string> = {
    attack: '#e8b04b',
    skill: '#6f9bff',
    aura: '#b47cff',
    passive: '#4fcf86',
};

interface Row {
    unit: CombatUnitInfo;
    stats: CombatUnitStats;
    dead: boolean;
    damage: Record<Kind, number>;
    healing: Record<Kind, number>;
}

const EMPTY_STATS = (unit: CombatUnitInfo): CombatUnitStats => ({
    entityId: unit.entityId, team: unit.team,
    damageDealt: 0, attackDamage: 0, skillDamage: 0, auraDamage: 0, passiveDamage: 0,
    healingDone: 0, skillHealing: 0, auraHealing: 0, passiveHealing: 0,
    damageTaken: 0, healingReceived: 0, kills: 0,
});

const hasStats = computed(() => !!props.result?.stats?.length);

const rows = computed<Row[]>(() => {
    const result = props.result;
    if (!result) return [];
    const byId = new Map((result.stats ?? []).map(s => [s.entityId, s]));
    const dead = new Set(result.logs.filter(l => l.eventType === 'Death').map(l => l.targetId));
    return result.units.map(unit => {
        const stats = byId.get(unit.entityId) ?? EMPTY_STATS(unit);
        return {
            unit,
            stats,
            dead: dead.has(unit.entityId),
            damage: { attack: stats.attackDamage, skill: stats.skillDamage, aura: stats.auraDamage, passive: stats.passiveDamage },
            healing: { attack: 0, skill: stats.skillHealing, aura: stats.auraHealing, passive: stats.passiveHealing },
        };
    });
});

const byPosition = (a: Row, b: Row) => a.unit.position - b.unit.position;
const squad = computed(() => rows.value.filter(r => r.unit.team === 1).sort(byPosition));
const enemies = computed(() => rows.value.filter(r => r.unit.team !== 1).sort(byPosition));

// Полоски общие на весь бой: самая длинная — у лучшего бойца из обеих команд
const maxDamage = computed(() => Math.max(1, ...rows.value.map(r => r.stats.damageDealt)));
const maxHealing = computed(() => Math.max(1, ...rows.value.map(r => r.stats.healingDone)));
const maxTaken = computed(() => Math.max(1, ...rows.value.map(r => r.stats.damageTaken)));

const sections = computed(() => [
    { key: 'squad', title: t('arena.stats.mySquad'), rows: squad.value },
    { key: 'enemies', title: t('arena.stats.enemies'), rows: enemies.value },
].filter(s => s.rows.length));

const totals = (list: Row[]) => ({
    damage: format(list.reduce((sum, r) => sum + r.stats.damageDealt, 0)),
    healing: format(list.reduce((sum, r) => sum + r.stats.healingDone, 0)),
});

const format = (value: number) => value.toLocaleString();

const segments = (parts: Record<Kind, number>, max: number) =>
    KINDS.filter(kind => parts[kind] > 0).map(kind => ({
        kind,
        style: { width: `${(parts[kind] / max) * 100}%`, background: KIND_COLORS[kind] },
    }));

const breakdown = (parts: Record<Kind, number>) =>
    KINDS.filter(kind => parts[kind] > 0).map(kind => ({ kind, color: KIND_COLORS[kind], value: format(parts[kind]) }));
</script>

<template>
    <ModalComponent v-model="open" width="64%" height="74%" :header-message="t('arena.stats.title')">
        <div class="battle-stats">
            <p v-if="!hasStats" class="battle-stats-empty">{{ t('arena.stats.empty') }}</p>
            <template v-else>
                <div class="battle-stats-legend">
                    <span v-for="kind in KINDS" :key="kind" class="battle-stats-legend-item">
                        <i :style="{ background: KIND_COLORS[kind] }"></i>{{ t(`arena.stats.${kind}`) }}
                    </span>
                    <span v-if="result" class="battle-stats-rounds">{{ t('arena.rounds', { rounds: result.rounds }) }}</span>
                </div>

                <section v-for="section in sections" :key="section.key" class="battle-stats-section">
                    <header class="battle-stats-section-head">
                        <h3>{{ section.title }}</h3>
                        <span>{{ t('arena.stats.totals', totals(section.rows)) }}</span>
                    </header>

                    <table class="battle-stats-table">
                        <thead>
                            <tr>
                                <th class="col-unit">{{ t('arena.stats.unit') }}</th>
                                <th>{{ t('arena.stats.damage') }}</th>
                                <th>{{ t('arena.stats.healing') }}</th>
                                <th>{{ t('arena.stats.taken') }}</th>
                                <th class="col-kills">{{ t('arena.stats.kills') }}</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="row in section.rows" :key="row.unit.entityId" :class="{ 'row-dead': row.dead }">
                                <td class="col-unit">
                                    <div class="unit-cell">
                                        <UnitAvatar :unit="row.unit" :size="40" />
                                        <span class="unit-name">{{ row.unit.name }}</span>
                                        <IconSkull v-if="row.dead" v-tooltip="t('arena.stats.dead')" class="unit-dead" />
                                    </div>
                                </td>
                                <td>
                                    <div class="stat-value">{{ format(row.stats.damageDealt) }}</div>
                                    <div class="stat-bar">
                                        <span v-for="seg in segments(row.damage, maxDamage)" :key="seg.kind" :style="seg.style"></span>
                                    </div>
                                    <div class="stat-breakdown">
                                        <span v-for="part in breakdown(row.damage)" :key="part.kind" v-tooltip="t(`arena.stats.${part.kind}`)">
                                            <i :style="{ background: part.color }"></i>{{ part.value }}
                                        </span>
                                    </div>
                                </td>
                                <td>
                                    <div class="stat-value stat-heal">{{ format(row.stats.healingDone) }}</div>
                                    <div class="stat-bar">
                                        <span v-for="seg in segments(row.healing, maxHealing)" :key="seg.kind" :style="seg.style"></span>
                                    </div>
                                    <div class="stat-breakdown">
                                        <span v-for="part in breakdown(row.healing)" :key="part.kind" v-tooltip="t(`arena.stats.${part.kind}`)">
                                            <i :style="{ background: part.color }"></i>{{ part.value }}
                                        </span>
                                    </div>
                                </td>
                                <td>
                                    <div class="stat-value stat-taken">{{ format(row.stats.damageTaken) }}</div>
                                    <div class="stat-bar">
                                        <span class="stat-bar-taken" :style="{ width: `${(row.stats.damageTaken / maxTaken) * 100}%` }"></span>
                                    </div>
                                </td>
                                <td class="col-kills">{{ row.stats.kills }}</td>
                            </tr>
                        </tbody>
                    </table>
                </section>
            </template>
        </div>
    </ModalComponent>
</template>

<style scoped>
.battle-stats {
    display: flex;
    flex-direction: column;
    gap: 18px;
    padding: 6px 18px 18px;
    color: #f1ead8;
    font-family: Arial, sans-serif;
}

.battle-stats-empty {
    margin: 40px 0;
    text-align: center;
    opacity: 0.8;
}

.battle-stats-legend {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 16px;
    font-size: 13px;
}

.battle-stats-legend-item {
    display: inline-flex;
    align-items: center;
    gap: 6px;
}

.battle-stats-legend i,
.stat-breakdown i {
    display: inline-block;
    width: 10px;
    height: 10px;
    border-radius: 3px;
}

.battle-stats-rounds {
    margin-left: auto;
    opacity: 0.75;
}

.battle-stats-section {
    border-radius: 12px;
    background: rgba(10, 14, 22, 0.55);
    box-shadow: inset 0 0 10px rgba(0, 0, 0, 0.6);
    padding: 10px 14px 6px;
}

.battle-stats-section-head {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
    gap: 12px;
    margin-bottom: 6px;
}

.battle-stats-section-head h3 {
    margin: 0;
    color: #ffd166;
    font-size: 17px;
    letter-spacing: 0.06em;
    text-transform: uppercase;
}

.battle-stats-section-head span {
    font-size: 12px;
    opacity: 0.75;
}

.battle-stats-table {
    width: 100%;
    border-collapse: collapse;
    table-layout: fixed;
}

.battle-stats-table th {
    padding: 4px 8px;
    font-size: 12px;
    font-weight: bold;
    text-align: left;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: #b9c4cc;
}

.battle-stats-table td {
    padding: 6px 8px;
    vertical-align: middle;
    border-top: 1px solid rgba(255, 255, 255, 0.07);
}

.col-unit {
    width: 28%;
}

.col-kills {
    width: 9%;
    text-align: center !important;
    font-size: 16px;
    font-weight: bold;
}

.unit-cell {
    display: flex;
    align-items: center;
    gap: 10px;
    min-width: 0;
}

.unit-name {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    font-weight: bold;
}

.unit-dead {
    flex-shrink: 0;
    width: 16px;
    height: 16px;
    opacity: 0.8;
}

.row-dead .unit-cell :deep(.portrait) {
    filter: grayscale(0.9) brightness(0.7);
}

.stat-value {
    font-size: 15px;
    font-weight: bold;
    color: #ffe2a0;
}

.stat-heal {
    color: #8ff0b0;
}

.stat-taken {
    color: #ff9c8f;
}

.stat-bar {
    display: flex;
    height: 7px;
    margin: 4px 0 3px;
    border-radius: 4px;
    overflow: hidden;
    background: rgba(255, 255, 255, 0.08);
}

.stat-bar span {
    height: 100%;
}

.stat-bar-taken {
    background: #e2574c;
}

.stat-breakdown {
    display: flex;
    flex-wrap: wrap;
    gap: 2px 10px;
    min-height: 14px;
    font-size: 11px;
    opacity: 0.85;
}

.stat-breakdown span {
    display: inline-flex;
    align-items: center;
    gap: 4px;
}
</style>
