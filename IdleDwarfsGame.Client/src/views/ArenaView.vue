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
        <div v-if="stageId" class="floor-select">{{ t('arena.stage', { id: stageId }) }}</div>
        <label v-else class="floor-select">
          {{ t('arena.floor') }}
          <input v-model.number="floor" type="number" min="1" max="50" :disabled="state.playing" />
        </label>
        <button :disabled="state.playing || loading" @click="startArenaFight()">{{ t('arena.startFight') }}</button>
        <button v-if="!stageId" :disabled="state.playing || loading" @click="startBossFight()">{{ t('arena.boss') }}</button>
        <button :disabled="!state.playing" @click="skipFight()">{{ t('arena.skip') }}</button>
        <button v-for="value in [1, 2, 4]" :key="value" :class="{ active: speed === value }" @click="setSpeed(value)">x{{ value }}</button>
        <div class="sound-control">
          <button :title="sound.muted ? t('arena.unmute') : t('arena.mute')" @click="soundService.toggleMute()">{{ sound.muted ? '🔇' : '🔊' }}</button>
          <input type="range" min="0" max="1" step="0.05" :value="sound.muted ? 0 : sound.volume" @input="onVolumeInput" />
        </div>
      </div>

      <div v-if="state.round" class="round-banner">{{ t('arena.round', { round: state.round }) }}</div>

      <div v-if="error !== null" class="arena-message error">{{ error || t('arena.startError') }}</div>

      <div v-if="state.finished" class="arena-message" :class="playerWon ? 'victory' : 'defeat'">
        <div class="result-title">{{ playerWon ? t('arena.victory') : t('arena.defeat') }}</div>
        <div v-if="result?.rewards" class="result-rewards">
          {{ t('arena.rewards', { gold: result.rewards.gold, experience: result.rewards.experience }) }}
        </div>
        <div v-if="campaignResult?.firstClear" class="result-rewards">{{ t('arena.stageCleared', { id: campaignResult.stageId }) }}</div>
        <div class="result-rounds">{{ t('arena.rounds', { rounds: result?.rounds ?? 0 }) }}</div>
        <button v-if="nextStageId" class="result-next" @click="goToNextStage()">{{ t('arena.nextStage') }}</button>
      </div>

      <CombatLogFeed v-if="result" :logs="result.logs" :shown-logs="state.shownLogs" :units="state.units" :speed="speed" />
    </CanvasLayer>
  </div>
</template>

<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, provide, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import arenaBgBlueCrystals from '@/assets/arena_red_crystals.png';
import arenaBgIce from '@/assets/arena_blue_crystals.png';
import CharacterComponent from '@/components/character-component.vue';
import CombatApiService from '@/services/apiServices/CombatApiService';
import CampaignApiService from '@/services/apiServices/CampaignApiService';
import { CampaignFightResult } from '@/models/types/Campaign';
import CanvasComponent from '@/components/canvas/CanvasComponent.vue';
import CanvasLayer from '@/components/canvas/CanvasLayer.vue';
import CombatLogFeed from '@/components/interface/CombatLogFeed.vue';
import { createBattleState, teamUnits } from '@/battle/BattleState';
import { BattlePlayer } from '@/battle/BattlePlayer';
import { BATTLE_FX_KEY, BattleFx } from '@/battle/BattleFx';
import { CombatResult } from '@/battle/types';
import { soundService } from '@/services/soundService/SoundService';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const bgImage = ref('arena_blue_crystals');
const layerRef = ref<InstanceType<typeof CanvasLayer> | null>(null);
const floor = ref(2);
const speed = ref(1);
const loading = ref(false);
// null — ошибки нет; '' — своя ошибка (переводится в шаблоне); иначе текст ошибки с сервера
const error = ref<string | null>(null);
const result = ref<CombatResult | null>(null);

// Режим кампании: /arena?stage=3 — бьёмся за уровень, а не за этаж башни
const route = useRoute();
const router = useRouter();
const stageId = computed(() => Number(route.query.stage) || null);
const campaignResult = ref<CampaignFightResult | null>(null);
const nextStageId = computed(() => {
  const next = campaignResult.value?.currentStageId;
  return state.finished && next && next !== campaignResult.value?.stageId ? next : null;
});

const state = createBattleState();
const fx = new BattleFx(() => layerRef.value?.particleService ?? null, () => speed.value);
const player = new BattlePlayer(state, fx);
provide(BATTLE_FX_KEY, fx);

// Ключи lava/ice приходят с сервера (Arena.Background)
const bgMap: Record<string, string> = {
  arena_blue_crystals: arenaBgBlueCrystals,
  lava: arenaBgBlueCrystals,
  ice: arenaBgIce,
};

const teamA = computed(() => teamUnits(state, 1));
const teamB = computed(() => teamUnits(state, 2));
const playerWon = computed(() => state.winningTeam === 1);

const bgStyle = computed(() => ({
  backgroundImage: `url('${bgMap[bgImage.value] ?? arenaBgBlueCrystals}')`,
  backgroundRepeat: 'round',
}));

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
  error.value = null;

  try {
    result.value = await load();
  } catch (e) {
    console.error('Ошибка запуска боя:', e);
    // Сервер отвечает 400 с текстом, например "Campaign stage 3 is locked"
    const message = (e as { response?: { data?: unknown } }).response?.data;
    error.value = typeof message === 'string' ? message : '';
    return;
  } finally {
    loading.value = false;
  }

  await player.play(result.value);
};

const startCampaignFight = (id: number) => playFight(async () => {
  campaignResult.value = null;
  const fight = await CampaignApiService.fight(id);
  campaignResult.value = fight;
  bgImage.value = fight.arena.background;
  return fight.combat;
});

const startArenaFight = () => stageId.value
  ? startCampaignFight(stageId.value)
  : playFight(() => CombatApiService.start({ floor: floor.value }));
const startBossFight = () => playFight(() => CombatApiService.startBoss(undefined, floor.value));

const goToNextStage = async () => {
  if (!nextStageId.value) return;
  await router.replace({ query: { stage: nextStageId.value } });
  startArenaFight();
};
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

.result-next {
  margin-top: 12px;
  border: 1px solid #ffd166;
  background: rgba(14, 19, 34, 0.7);
  color: #ffd166;
  border-radius: 8px;
  padding: 8px 16px;
  cursor: pointer;
}
</style>
