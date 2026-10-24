<template>
  <div class="arena" :style="bgStyle">
    <CanvasLayer ref="layerRef">
      <div class="grid">
        <template v-for="unit in teamA" :key="unit.id">
          <CanvasComponent
            :class="['grid-item', `grid-item-left-${unit.position}`]"  
            :active="unit.alive"
            style="position: absolute;"
          >
            <CharacterComponent :unit="unit" />
          </CanvasComponent>
        </template>

        <template v-for="unit in teamB" :key="unit.id">
          <CanvasComponent
            :class="['grid-item', 'grid-item-right', `grid-item-right-${unit.position}`]"
            :active="unit.alive"
            style="position: absolute;"
          >
            <CharacterComponent :unit="unit" rotate />
          </CanvasComponent>
        </template>
      </div>

      <div class="arena-controls">
        <label class="floor-select">
          Floor
          <input v-model.number="floor" type="number" min="1" max="50" :disabled="state.playing" />
        </label>
        <button :disabled="state.playing || loading" @click="startArenaFight()">Start fight</button>
        <button :disabled="state.playing || loading" @click="startBossFight()">Boss</button>
        <button :disabled="!state.playing" @click="skipFight()">Skip</button>
        <button v-for="value in [1, 2, 4]" :key="value" :class="{ active: speed === value }" @click="setSpeed(value)">x{{ value }}</button>
        <div class="sound-control">
          <button :title="sound.muted ? 'Unmute' : 'Mute'" @click="soundService.toggleMute()">{{ sound.muted ? '🔇' : '🔊' }}</button>
          <input type="range" min="0" max="1" step="0.05" :value="sound.muted ? 0 : sound.volume" @input="onVolumeInput" />
        </div>
      </div>

      <div v-if="state.round" class="round-banner">Round {{ state.round }}</div>

      <div v-if="error" class="arena-message error">{{ error }}</div>

      <div v-if="state.finished" class="arena-message" :class="playerWon ? 'victory' : 'defeat'">
        <div class="result-title">{{ playerWon ? 'Victory' : 'Defeat' }}</div>
        <div v-if="result?.rewards" class="result-rewards">
          +{{ result.rewards.gold }} gold · +{{ result.rewards.experience }} xp
        </div>
        <div class="result-rounds">Rounds: {{ result?.rounds }}</div>
      </div>

      <div v-if="visibleLogs.length" class="combat-log-panel">
        <h3>Combat log</h3>
        <div class="combat-log-list">
          <div v-for="log in visibleLogs" :key="log.sequence" class="combat-log-item">
            <span class="log-round">R{{ log.round }}</span>
            <span class="log-phase">{{ log.eventType }}</span>
            <span class="log-text">{{ formatLogText(log) }}</span>
          </div>
        </div>
      </div>
    </CanvasLayer>
  </div>
</template>

<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, provide, ref } from 'vue';
import arenaBgBlueCrystals from '@/assets/arena_red_crystals.png';
import CharacterComponent from '@/components/character-component.vue';
import CombatApiService from '@/services/apiServices/CombatApiService';
import CanvasComponent from '@/components/canvas/CanvasComponent.vue';
import CanvasLayer from '@/components/canvas/CanvasLayer.vue';
import { createBattleState, teamUnits } from '@/battle/BattleState';
import { BattlePlayer } from '@/battle/BattlePlayer';
import { BATTLE_FX_KEY, BattleFx } from '@/battle/BattleFx';
import { CombatLogEntry, CombatResult } from '@/battle/types';
import { soundService } from '@/services/soundService/SoundService';

// Логи, которые не стоит показывать в панели (их слишком много и они без смысла для игрока)
const HIDDEN_LOG_EVENTS = new Set(['CombatStart', 'StatusApplied', 'StatusExpired']);

const bgImage = ref('arena_blue_crystals');
const layerRef = ref<InstanceType<typeof CanvasLayer> | null>(null);
const floor = ref(2);
const speed = ref(1);
const loading = ref(false);
const error = ref('');
const result = ref<CombatResult | null>(null);

const state = createBattleState();
const fx = new BattleFx(() => layerRef.value?.particleService ?? null, () => speed.value);
const player = new BattlePlayer(state, fx);
provide(BATTLE_FX_KEY, fx);

const bgMap: Record<string, string> = {
  arena_blue_crystals: arenaBgBlueCrystals,
};

const teamA = computed(() => teamUnits(state, 1));
const teamB = computed(() => teamUnits(state, 2));
const playerWon = computed(() => state.winningTeam === 1);
const visibleLogs = computed(() =>
  state.shownLogs.filter(l => !HIDDEN_LOG_EVENTS.has(l.eventType)).slice(-15).reverse()
);

const bgStyle = computed(() => ({
  backgroundImage: `url('${bgMap[bgImage.value]}')`,
  backgroundRepeat: 'round',
}));

const nameOf = (id: number) => state.units[id]?.name ?? (id > 0 ? `#${id}` : '—');

const formatLogText = (log: CombatLogEntry) => {
  const attacker = nameOf(log.attackerId);
  const target = nameOf(log.targetId);

  switch (log.eventType) {
    case 'RoundStart':
      return `Round ${log.round} started`;
    case 'Attack':
      return `${attacker} hits ${target} for ${log.damage}`;
    case 'SkillCast':
      return `${attacker} casts ${log.skillName}`;
    case 'Damage':
      return `${log.skillName || 'Damage'}: ${target} takes ${log.damage} ${log.damageType}`;
    case 'StatusTick':
      return `${target} takes ${log.damage} from ${log.skillName || log.actionType}`;
    case 'Heal':
      return `${target} healed for ${log.heal}${log.skillName ? ` (${log.skillName})` : ''}`;
    case 'Stunned':
      return `${attacker} is stunned and skips the turn`;
    case 'Immune':
      return `${target} is immune to ${log.appliedEffects[0]?.name ?? 'effect'}`;
    case 'Death':
      return `${target} was defeated`;
    case 'CombatEnd':
      return log.winningTeam === 1 ? 'Your squad wins!' : log.text ?? 'Enemies win';
    default:
      return log.text ?? `${log.actionType || log.eventType}: ${attacker} -> ${target}`;
  }
};

const setSpeed = (value: number) => {
  speed.value = value;
  player.speed = value;
};

const sound = soundService.settings;
const onVolumeInput = (event: Event) => {
  soundService.setVolume(Number((event.target as HTMLInputElement).value));
};

const playFight = async (load: () => Promise<CombatResult>) => {
  if (state.playing || loading.value) return;
  loading.value = true;
  error.value = '';

  try {
    result.value = await load();
  } catch (e) {
    console.error('Ошибка запуска боя:', e);
    error.value = 'Failed to start the fight. Is the server running?';
    return;
  } finally {
    loading.value = false;
  }

  await player.play(result.value);
};

const startArenaFight = () => playFight(() => CombatApiService.start({ floor: floor.value }));
const startBossFight = () => playFight(() => CombatApiService.startBoss(undefined, floor.value));
const skipFight = () => player.skip();

onMounted(() => {
  startArenaFight();
});

onBeforeUnmount(() => player.stop());
</script>

<style lang="scss">
.arena {
  width: 100vw;
  height: 100vh;
  position: fixed;
}

.grid-item {
  position: absolute;
}

.grid-item-left-1 {
  top: 50%;
  left: 35%;
}

.grid-item-left-2 {
  top: 65%;
  left: 30%;
}

.grid-item-left-3 {
  top: 45%;
  left: 25%;
}

.grid-item-left-4 {
  top: 55%;
  left: 20%;
}

.grid-item-left-5 {
  top: 65%;
  left: 15%;
}

.grid-item-right-1 {
  top: 50%;
  right: 35%;
}

.grid-item-right-2 {
  top: 65%;
  right: 30%;
}

.grid-item-right-3 {
  top: 45%;
  right: 25%;
}

.grid-item-right-4 {
  top: 55%;
  right: 20%;
}

.grid-item-right-5 {
  top: 65%;
  right: 15%;
}

.arena-controls {
  position: absolute;
  top: 100px;
  left: 20px;
  z-index: 10;
  display: flex;
  gap: 10px;
  align-items: center;
}

.arena-controls button,
.floor-select {
  border: 1px solid rgba(255, 255, 255, 0.5);
  background: rgba(14, 19, 34, 0.7);
  color: white;
  border-radius: 8px;
  padding: 8px 12px;
  cursor: pointer;
}

.arena-controls button:disabled {
  opacity: 0.45;
  cursor: default;
}

.arena-controls button.active {
  border-color: #ffd166;
  color: #ffd166;
}

.floor-select input {
  width: 48px;
  margin-left: 6px;
  background: transparent;
  color: white;
  border: none;
  border-bottom: 1px solid rgba(255, 255, 255, 0.5);
}

.sound-control {
  display: flex;
  align-items: center;
  gap: 6px;
}

.sound-control input {
  width: 90px;
  accent-color: #ffd166;
}

.round-banner {
  position: absolute;
  top: 100px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 11;
  color: #ffd166;
  font-size: 28px;
  font-weight: 800;
  letter-spacing: 2px;
  text-shadow: 0 2px 8px #000;
}

.arena-message {
  position: absolute;
  top: 35%;
  left: 50%;
  transform: translate(-50%, -50%);
  z-index: 13;
  padding: 18px 36px;
  border-radius: 14px;
  background: rgba(10, 15, 24, 0.85);
  color: white;
  text-align: center;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.45);
}

.arena-message.error {
  color: #fca5a5;
}

.arena-message.victory .result-title {
  color: #ffd166;
}

.arena-message.defeat .result-title {
  color: #f87171;
}

.result-title {
  font-size: 40px;
  font-weight: 800;
}

.result-rewards,
.result-rounds {
  margin-top: 6px;
  font-size: 14px;
  color: #e2e8f0;
}

.combat-log-panel {
  position: absolute;
  right: 20px;
  top: 20px;
  width: 360px;
  max-height: 70vh;
  overflow: auto;
  background: rgba(10, 15, 24, 0.82);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  padding: 12px;
  color: white;
  z-index: 12;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.35);
}

.combat-log-panel h3 {
  margin: 0 0 12px 0;
  font-size: 18px;
}

.combat-log-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.combat-log-item {
  display: flex;
  gap: 8px;
  align-items: flex-start;
  background: rgba(255, 255, 255, 0.04);
  border-radius: 8px;
  padding: 8px 10px;
  font-size: 12px;
  line-height: 1.4;
}

.log-round {
  color: #ffd166;
  font-weight: 700;
  min-width: 36px;
}

.log-phase {
  color: #7dd3fc;
  min-width: 50px;
  text-transform: uppercase;
  font-size: 10px;
}

.log-text {
  flex: 1;
  color: #edf2f7;
}
</style>
