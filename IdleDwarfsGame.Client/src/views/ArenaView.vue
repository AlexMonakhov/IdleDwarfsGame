<template>
  <div class="arena" :style="bgStyle">
    <CanvasLayer>
      <div class="grid">
        <template v-for="(hero, n) in teamA" :key="hero">
          <CanvasComponent
            ref="teamARefs"
            :class="['grid-item', `grid-item-left-${n + 1}`]"
            :animationKey="animationType"
            :effects="[{ func: 'god-rays-radiance', padding: 100, isUnder: true }, { func: 'particles', padding: 100, isUnder: true }].concat([{ func: 'particles', padding: 20, isUnder: false, from: teamARefs[n], to: teamBRefs[n], args: { color: `#${n}${n}${n}${n}${n}${n}`, duration: 650, speed: 2 } }])"
            style="position: absolute;"
          >
            <CharacterComponent :entity="hero" :animation-type="animationType" />
          </CanvasComponent>
        </template>

        <template v-for="(hero, n) in teamB" :key="hero">
          <CanvasComponent
            ref="teamBRefs"
            :class="['grid-item', 'grid-item-right', `grid-item-right-${n + 1}`]"
            :animationKey="animationType"
            :effects="[{ func: 'god-rays-radiance', padding: 100, isUnder: true }]"
            style="position: absolute;"
          >
            <CharacterComponent :entity="hero" rotate :animation-type="animationType" />
          </CanvasComponent>
        </template>
      </div>

      <div class="arena-controls">
        <button @click="startArenaFight()">Start fight</button>
        <button @click="changeAnim('idle')">Toggle Idle</button>
        <button @click="changeAnim('victory')">Toggle victory</button>
        <button @click="changeAnim('attack')">Toggle attack</button>
      </div>

      <div v-if="fightLog.length" class="combat-log-panel">
        <h3>Combat log</h3>
        <div class="combat-log-list">
          <div v-for="(log, index) in fightLog.slice().reverse().slice(0, 15)" :key="`${log.round}-${log.eventType}-${index}`" class="combat-log-item">
            <span class="log-round">R{{ log.round ?? 1 }}</span>
            <span class="log-phase">{{ log.phase || 'action' }}</span>
            <span class="log-text">{{ formatLogText(log) }}</span>
          </div>
        </div>
      </div>
    </CanvasLayer>
  </div>
</template>

<script lang="ts" setup>
import { battleManager } from '../ecs/service/BattleManager';
import { computed, onMounted, ref } from 'vue';
import { buildHeroes } from '../models/composables/heroes';
import { Entity } from '@/ecs/entities/Storage';
import arenaBgBlueCrystals from '@/assets/arena_red_crystals.png';
import CharacterComponent from '@/components/character-component.vue';
import CombatApiService, { CombatLogEntry } from '@/services/apiServices/CombatApiService';
import CanvasComponent from '@/components/canvas/CanvasComponent.vue';
import CanvasLayer from '@/components/canvas/CanvasLayer.vue';

const bgImage = ref('arena_blue_crystals');
const teamA = ref<Entity[]>([]);
const teamB = ref<Entity[]>([]);
const animationType = ref<'idle' | 'attack' | 'victory'>('idle');
const fightresult = ref<any>(null);
const teamARefs = ref<any[]>([]);
const teamBRefs = ref<any[]>([]);
const fightLog = ref<CombatLogEntry[]>([]);

const bgMap = {
  arena_blue_crystals: arenaBgBlueCrystals,
};

onMounted(() => {
  battleManager.createEntities();
  buildHeroes();
  [teamA.value, teamB.value] = battleManager.getTeams();
  battleManager.StartBattle();
});

const round = computed(() => battleManager.state.round);
const winner = computed(() => battleManager.state.winner);

const bgStyle = computed(() => ({
  backgroundImage: `url('${bgMap[bgImage.value]}')`,
  backgroundRepeat: 'round',
}));

const formatLogText = (log: CombatLogEntry) => {
  if (log.text) return log.text;

  const attacker = log.attackerId ?? '—';
  const target = log.targetId ?? '—';

  if (log.eventType === 'SkillCast') {
    return `Skill ${log.skillName || 'unknown'}: ${attacker} -> ${target}`;
  }

  if (log.eventType === 'StatusApplied') {
    const effect = log.appliedEffects?.[0];
    return `${effect?.name || 'Effect'} on ${target} (${effect?.duration ?? 0} turns)`;
  }

  if (log.eventType === 'StatusTick') {
    const effect = log.activeEffects?.[0];
    return `${effect?.name || 'Tick'} on ${target}: ${log.damage ?? 0} ${log.damageType || ''} damage`;
  }

  if (log.eventType === 'Damage' || !log.eventType) {
    return `${attacker} hits ${target} for ${log.damage ?? 0} ${log.damageType || ''} dmg`;
  }

  if (log.eventType === 'RoundStart') {
    return `Round ${log.round ?? 1} started`;
  }

  if (log.eventType === 'Death') {
    return `Unit ${target} was defeated`;
  }

  return `${log.actionType || log.eventType || 'Combat'}: ${attacker} -> ${target}`;
};

const changeAnim = async (animType: 'idle' | 'attack' | 'victory') => {
  animationType.value = animType;
};

const startArenaFight = async () => {
  try {
    fightresult.value = await CombatApiService.start();
    fightLog.value = fightresult.value?.logs || [];
    animationType.value = 'attack';
  } catch (error) {
    console.error('Ошибка запуска боя:', error);
  }
};
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
  top: 20px;
  left: 20px;
  z-index: 10;
  display: flex;
  gap: 10px;
}

.arena-controls button {
  border: 1px solid rgba(255, 255, 255, 0.5);
  background: rgba(14, 19, 34, 0.7);
  color: white;
  border-radius: 8px;
  padding: 8px 12px;
  cursor: pointer;
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
