<template>
  <div class="arena">
    <!-- Фон отдельным слоем: временные фоны арен перекрашиваются фильтром, юниты — нет -->
    <div class="arena-bg" :style="bgStyle"></div>
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
        <div v-if="pvpOpponentId" class="floor-select">{{ pvpResult ? t('arena.pvpVs', { name: pvpResult.opponentName }) : t('arena.pvp') }}</div>
        <div v-else-if="guildBossTier" class="floor-select">{{ t('arena.guildBoss', { tier: t(`guild.bosses.tiers.${guildBossTier}`) }) }}</div>
        <div v-else-if="stageId" class="floor-select">{{ t('arena.stage', { id: stageId }) }}</div>
        <label v-else class="floor-select">
          {{ t('arena.floor') }}
          <input v-model.number="floor" type="number" min="1" max="50" :disabled="state.playing" />
        </label>
        <!-- По боссу гильдии бьют раз в час из окна гильдии, соперника PvP выбирают в окне PvP — повтора здесь нет -->
        <button v-if="!guildBossTier && !pvpOpponentId" :disabled="state.playing || loading" @click="startArenaFight()">{{ t('arena.startFight') }}</button>
        <button v-if="!stageId && !guildBossTier && !pvpOpponentId" :disabled="state.playing || loading" @click="startBossFight()">{{ t('arena.boss') }}</button>
        <button :disabled="!state.playing" @click="skipFight()">{{ t('arena.skip') }}</button>
        <button v-for="value in [1, 2, 4]" :key="value" :class="{ active: speed === value }" @click="setSpeed(value)">x{{ value }}</button>
        <div class="sound-control">
          <button :title="sound.muted ? t('arena.unmute') : t('arena.mute')" @click="soundService.toggleMute()">{{ sound.muted ? '🔇' : '🔊' }}</button>
          <input type="range" min="0" max="1" step="0.05" :value="sound.muted ? 0 : sound.volume" @input="onVolumeInput" />
        </div>
      </div>

      <div v-if="state.round" class="round-banner">
        {{ t('arena.round', { round: state.round }) }}
        <!-- Арена PvP наращивает весь урон каждые несколько раундов -->
        <span v-if="state.damageBonus" :key="state.damageBonus" class="fury-badge" :title="t('arena.furyHint')">🔥 +{{ state.damageBonus }}%</span>
      </div>

      <div v-if="error !== null" class="arena-message error">
        {{ error || t('arena.startError') }}
        <button v-if="guildBossTier" class="result-next" @click="goToGuild()">{{ t('arena.toGuild') }}</button>
        <button v-if="pvpOpponentId" class="result-next" @click="goToPvp()">{{ t('arena.toPvp') }}</button>
      </div>

      <!-- PvP: победа или поражение и сколько очков сезона это стоило -->
      <div v-if="state.finished && pvpResult" class="arena-message" :class="pvpResult.won ? 'victory' : 'defeat'">
        <div class="result-title">{{ pvpResult.won ? t('arena.victory') : t('arena.defeat') }}</div>
        <div class="result-rewards">{{ t('arena.pvpVs', { name: pvpResult.opponentName }) }}</div>
        <div class="result-rewards" :class="pvpResult.points >= 0 ? 'pvp-points-up' : 'pvp-points-down'">
          {{ t('arena.pvpPoints', { points: pvpResult.points > 0 ? `+${pvpResult.points}` : pvpResult.points }) }}
        </div>
        <div class="result-rewards">
          <span v-for="season in pvpResult.seasons" :key="season.type" class="pvp-season-points">
            {{ t(`pvp.seasons.${season.type}`) }}: {{ season.points }}
          </span>
        </div>
        <div v-if="pvpResult.won" class="result-rewards">{{ t('arena.pvpBeaten') }}</div>
        <div class="result-rounds">{{ t('arena.rounds', { rounds: result?.rounds ?? 0 }) }}</div>
        <button class="result-next" @click="goToPvp()">{{ t('arena.toPvp') }}</button>
      </div>

      <!-- Босса гильдии за один бой обычно не убить: вместо победы/поражения — нанесённый урон -->
      <div v-else-if="state.finished && guildResult" class="arena-message" :class="guildResult.killed ? 'victory' : 'guild-hit'">
        <div class="result-title">{{ guildResult.killed ? t('arena.guildBossKilled') : t('arena.guildBossHit') }}</div>
        <div class="result-rewards">{{ t('arena.guildBossDamage', { damage: guildResult.damage.toLocaleString(locale) }) }}</div>
        <div class="result-rewards">
          {{ t('arena.guildBossHp', { hp: guildResult.bossHp.toLocaleString(locale), max: guildResult.maxHp.toLocaleString(locale) }) }}
        </div>
        <div v-if="guildResult.killed" class="result-rewards">{{ t('arena.guildRewardWaiting') }}</div>
        <div class="result-rounds">{{ t('arena.rounds', { rounds: result?.rounds ?? 0 }) }}</div>
        <button class="result-next" @click="goToGuild()">{{ t('arena.toGuild') }}</button>
      </div>

      <div v-else-if="state.finished" class="arena-message" :class="playerWon ? 'victory' : 'defeat'">
        <div class="result-title">{{ playerWon ? t('arena.victory') : t('arena.defeat') }}</div>
        <div v-if="result?.rewards" class="result-rewards">
          {{ t('arena.rewards', { gold: result.rewards.gold, experience: result.rewards.experience }) }}
        </div>
        <div v-if="campaignResult?.firstClear" class="result-rewards">{{ t('arena.stageCleared', { id: campaignResult.stageId }) }}</div>
        <div v-if="campaignResult?.firstClear" class="result-rewards">{{ t('arena.rewardWaiting') }}</div>
        <div class="result-rounds">{{ t('arena.rounds', { rounds: result?.rounds ?? 0 }) }}</div>
        <!-- Следующий бой кампании начнётся только после получения награды за этот уровень -->
        <button v-if="state.finished && campaignResult?.firstClear" class="result-next" @click="goToCampaign()">{{ t('arena.toCampaign') }}</button>
        <button v-else-if="nextStageId" class="result-next" @click="goToNextStage()">{{ t('arena.nextStage') }}</button>
      </div>

      <CombatLogFeed v-if="result" :logs="result.logs" :shown-logs="state.shownLogs" :units="state.units" :speed="speed" />
    </CanvasLayer>
  </div>
</template>

<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, provide, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { arenaBackground, arenaBackgroundFilter } from '@/battle/arenaBackgrounds';
import CharacterComponent from '@/components/character-component.vue';
import CombatApiService from '@/services/apiServices/CombatApiService';
import CampaignApiService from '@/services/apiServices/CampaignApiService';
import { CampaignFightResult } from '@/models/types/Campaign';
import GuildApiService from '@/services/apiServices/GuildApiService';
import { GuildBossAttackResult, GuildBossTier } from '@/models/types/Guild';
import { guildErrorText } from '@/components/guild/guildShared';
import { useGuild } from '@/composables/UseGuild';
import PvpApiService from '@/services/apiServices/PvpApiService';
import { PvpFightResult } from '@/models/types/Pvp';
import { pvpErrorText } from '@/components/pvp/pvpShared';
import { usePvp } from '@/composables/UsePvp';
import CanvasComponent from '@/components/canvas/CanvasComponent.vue';
import CanvasLayer from '@/components/canvas/CanvasLayer.vue';
import CombatLogFeed from '@/components/interface/CombatLogFeed.vue';
import { createBattleState, teamUnits } from '@/battle/BattleState';
import { BattlePlayer } from '@/battle/BattlePlayer';
import { BATTLE_FX_KEY, BattleFx } from '@/battle/BattleFx';
import { CombatResult } from '@/battle/types';
import { soundService } from '@/services/soundService/SoundService';
import { useI18n } from 'vue-i18n';

const { t, locale } = useI18n();

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

// Режим босса гильдии: /arena?guildBoss=Easy&attack=<метка> — удар по общему боссу гильдии
const GUILD_BOSS_TIERS: GuildBossTier[] = ['Easy', 'Medium', 'Hard'];
const guildBossTier = computed(() => {
  const tier = route.query.guildBoss as GuildBossTier;
  return GUILD_BOSS_TIERS.includes(tier) ? tier : null;
});
const guildResult = ref<GuildBossAttackResult | null>(null);
const { guildOpen } = useGuild();

// Режим PvP: /arena?pvp=<userId соперника>&attack=<метка> — бой с отрядом другого игрока
const pvpOpponentId = computed(() => (typeof route.query.pvp === 'string' && route.query.pvp) || null);
const pvpResult = ref<PvpFightResult | null>(null);
const { pvpOpen } = usePvp();

const state = createBattleState();
const fx = new BattleFx(() => layerRef.value?.particleService ?? null, () => speed.value);
const player = new BattlePlayer(state, fx);
provide(BATTLE_FX_KEY, fx);

const teamA = computed(() => teamUnits(state, 1));
const teamB = computed(() => teamUnits(state, 2));
const playerWon = computed(() => state.winningTeam === 1);

const bgStyle = computed(() => ({
  // Ключи lava/ice приходят с сервера (Arena.Background, фон босса гильдии)
  backgroundImage: `url('${arenaBackground(bgImage.value)}')`,
  backgroundRepeat: 'round',
  filter: arenaBackgroundFilter(bgImage.value),
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
  // Итог прошлого боя PvP не должен остаться на экране после боя другого режима
  pvpResult.value = null;

  try {
    result.value = await load();
  } catch (e) {
    console.error('Ошибка запуска боя:', e);
    // Сервер отвечает 400 с текстом, например "Campaign stage 3 is locked"; гильдия — { code, message }
    const message = (e as { response?: { data?: unknown } }).response?.data;
    error.value = typeof message === 'string'
      ? message
      : pvpOpponentId.value ? pvpErrorText(e, '') : guildBossTier.value ? guildErrorText(e, '') : '';
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

const startGuildBossFight = (tier: GuildBossTier) => playFight(async () => {
  // Метку удара убираем сразу: перезагрузка страницы не должна бить босса ещё раз
  router.replace({ query: { guildBoss: tier } });
  campaignResult.value = null;
  guildResult.value = null;
  const attack = await GuildApiService.attack(tier);
  guildResult.value = attack;
  bgImage.value = attack.background;
  return attack.combat;
});

const startPvpFight = (opponentId: string) => playFight(async () => {
  // Метку боя убираем сразу: перезагрузка страницы не должна начинать бой ещё раз
  router.replace({ query: { pvp: opponentId } });
  campaignResult.value = null;
  guildResult.value = null;
  const fight = await PvpApiService.fight(opponentId);
  pvpResult.value = fight;
  bgImage.value = fight.arena.background;
  return fight.combat;
});

const startArenaFight = () => {
  if (pvpOpponentId.value) return startPvpFight(pvpOpponentId.value);
  if (guildBossTier.value) return startGuildBossFight(guildBossTier.value);
  return stageId.value
    ? startCampaignFight(stageId.value)
    : playFight(() => CombatApiService.start({ floor: floor.value }));
};

// Новый удар из окна гильдии или бой из окна PvP, когда арена уже открыта: компонент тот же, меняется только метка attack
watch(() => route.query.attack, (attack, previous) => {
  if (attack && attack !== previous && (guildBossTier.value || pvpOpponentId.value)) startArenaFight();
});

const goToGuild = () => {
  guildOpen.value = true;
};
const goToPvp = () => {
  pvpOpen.value = true;
};
const startBossFight = () => playFight(() => CombatApiService.startBoss(undefined, floor.value));

const goToNextStage = async () => {
  if (!nextStageId.value) return;
  await router.replace({ query: { stage: nextStageId.value } });
  startArenaFight();
};
// Список кампании (и кнопка получения награды) — на странице «О игре»
const goToCampaign = () => router.push({ name: 'about' });
const skipFight = () => player.skip();

onMounted(() => {
  // Арена босса гильдии без метки удара (например, после перезагрузки) — возвращаем в окно гильдии
  if (guildBossTier.value && !route.query.attack) {
    guildOpen.value = true;
    return;
  }
  // То же для PvP — в окно PvP
  if (pvpOpponentId.value && !route.query.attack) {
    pvpOpen.value = true;
    return;
  }
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

/* fixed-родитель — свой контекст наложения, так что -1 кладёт фон под всё содержимое арены */
.arena-bg {
  position: absolute;
  inset: 0;
  z-index: -1;
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

/* Прибавка к урону от арены PvP: вспыхивает заново при каждом росте */
.fury-badge {
  display: inline-block;
  margin-left: 12px;
  padding: 2px 10px;
  border-radius: 12px;
  background: rgba(127, 29, 29, 0.75);
  border: 1px solid #fb923c;
  color: #fdba74;
  font-size: 20px;
  vertical-align: middle;
  animation: fury-pop 0.6s ease-out;
}

@keyframes fury-pop {
  0% { transform: scale(1.8); box-shadow: 0 0 24px 8px rgba(251, 146, 60, 0.9); }
  100% { transform: scale(1); box-shadow: 0 0 0 0 rgba(251, 146, 60, 0); }
}

@media (prefers-reduced-motion: reduce) {
  .fury-badge {
    animation: none;
  }
}

.pvp-points-up {
  color: #4ade80;
  font-size: 18px;
  font-weight: 800;
}

.pvp-points-down {
  color: #f87171;
  font-size: 18px;
  font-weight: 800;
}

.pvp-season-points + .pvp-season-points::before {
  content: ' · ';
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

.arena-message.guild-hit .result-title {
  color: #fca5a5;
}

.arena-message.error .result-next {
  display: block;
  margin: 12px auto 0;
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
