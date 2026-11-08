<template>
  <!-- Клик в любом месте арены после боя открывает кнопки под баннером -->
  <div class="arena" @click="openResult">
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
            <CharacterComponent :unit="unit" :scale="UNIT_SCALE" />
          </CanvasComponent>
        </template>

        <template v-for="unit in teamB" :key="unit.id">
          <CanvasComponent
            :class="['grid-item', 'grid-item-right', `grid-item-right-${unit.position}`]"
            :active="unit.alive"
            style="position: absolute;"
          >
            <CharacterComponent :unit="unit" :scale="UNIT_SCALE" rotate />
          </CanvasComponent>
        </template>
      </div>

      <div class="arena-controls">
        <div v-if="pvpOpponentId" class="floor-select">
          {{ pvpResult ? t(pvpRanked ? 'arena.rankedVs' : 'arena.pvpVs', { name: pvpResult.opponentName }) : t(pvpRanked ? 'arena.ranked' : 'arena.pvp') }}
        </div>
        <div v-else-if="guildBossTier" class="floor-select">{{ t('arena.guildBoss', { tier: t(`guild.bosses.tiers.${guildBossTier}`) }) }}</div>
        <div v-else-if="worldBossMode" class="floor-select">
          {{ worldBossResult ? t('arena.worldBossStage', { stage: worldBossResult.stage }) : t('arena.worldBoss') }}
        </div>
        <div v-else-if="stageId" class="floor-select">{{ t('arena.stage', { id: stageId }) }}</div>
        <div v-else-if="towerFloor" class="floor-select">{{ t('arena.tower', { floor: towerFloor }) }}</div>
        <label v-else class="floor-select">
          {{ t('arena.floor') }}
          <input v-model.number="floor" type="number" min="1" max="50" :disabled="state.playing" />
        </label>
        <!-- По боссу гильдии бьют раз в час из окна гильдии, соперника PvP выбирают в окне PvP — повтора здесь нет -->
        <button v-if="!guildBossTier && !pvpOpponentId && !worldBossMode" :disabled="state.playing || loading" @click="startArenaFight()">{{ t('arena.startFight') }}</button>
        <button v-if="!stageId && !towerFloor && !guildBossTier && !pvpOpponentId && !worldBossMode" :disabled="state.playing || loading" @click="startBossFight()">{{ t('arena.boss') }}</button>
        <button :disabled="!state.playing" @click="skipFight()">{{ t('arena.skip') }}</button>
        <button v-for="value in [1, 2, 4]" :key="value" :class="{ active: speed === value }" @click="setSpeed(value)">x{{ value }}</button>
        <div class="sound-control">
          <button v-tooltip="sound.muted ? t('arena.unmute') : t('arena.mute')" @click="soundService.toggleMute()"><IconMute v-if="sound.muted" /><IconSound v-else /></button>
          <input type="range" min="0" max="1" step="0.05" :value="sound.muted ? 0 : sound.volume" @input="onVolumeInput" />
        </div>
      </div>

      <div v-if="state.round" class="round-banner">
        {{ t('arena.round', { round: state.round }) }}
        <!-- Арена PvP наращивает весь урон каждые несколько раундов -->
        <span v-if="state.damageBonus" :key="state.damageBonus" class="fury-badge" v-tooltip="t('arena.furyHint')"><IconFlame /> +{{ state.damageBonus }}%</span>
      </div>

      <div v-if="error !== null" class="arena-message error">
        {{ error || t('arena.startError') }}
        <button v-if="guildBossTier" class="result-next" @click="goToGuild()">{{ t('arena.toGuild') }}</button>
        <button v-if="pvpOpponentId" class="result-next" @click="goToPvp()">{{ t('arena.toPvp') }}</button>
        <button v-if="worldBossMode" class="result-next" @click="goToEvent()">{{ t('arena.toEvent') }}</button>
      </div>

      <!-- Итог боя: сцена под «вуалью» притеняется (при поражении — сереет), частицы вылетают из-за баннера (конфетти и звёзды или дождь и снег) -->
      <template v-if="outcome">
        <!-- Вуаль: при победе чуть притеняет сцену, при поражении — обесцвечивает -->
        <div class="arena-result-veil" :class="`arena-result-veil-${outcome.tone}`" aria-hidden="true"></div>
        <ResultParticles v-if="outcome.particles" :key="outcome.tone" :mode="outcome.tone" :anchor="bannerEl" class="arena-result-particles" />

        <div class="arena-result">
          <!-- Сначала только баннер; по клику под ним появляются кнопки -->
          <button
            ref="bannerEl"
            type="button"
            class="arena-result-banner"
            :class="{ 'arena-result-banner-open': resultOpen || !hasResultActions }"
            :aria-expanded="resultOpen"
            :disabled="!hasResultActions"
          >
            <ResultBanner :title="outcome.title" :tone="outcome.tone" />
          </button>
          <span v-if="hasResultActions && !resultOpen" class="arena-result-hint">{{ t('arena.showResults') }}</span>

          <!-- Только гем-кнопки, без таблички. Награда не держит прогресс: можно сразу в следующий бой, а забрать её — в окне -->
          <Transition name="arena-result-actions">
            <div v-if="resultOpen" class="result-actions">
              <GemButton v-if="pvpResult" :color="'blue'" @click="goToPvp()">{{ t('arena.toPvp') }}</GemButton>
              <GemButton v-else-if="guildResult" :color="'blue'" @click="goToGuild()">{{ t('arena.toGuild') }}</GemButton>
              <GemButton v-else-if="worldBossResult" :color="'blue'" @click="goToEvent()">{{ t('arena.toEvent') }}</GemButton>
              <template v-else>
                <GemButton v-if="stageId" :color="'blue'" @click="goToCampaign()">{{ t('arena.toCampaign') }}</GemButton>
                <GemButton v-if="nextStageId" :color="'green'" @click="goToNextStage()"><IconPower /> {{ t('arena.nextStage') }}</GemButton>
                <!-- Башня: после победы — дальше или в окно за наградой, после поражения — сменить отряд и попробовать снова -->
                <GemButton v-if="towerFloor" :color="'blue'" @click="goToTower()">{{ t('arena.toTower') }}</GemButton>
                <GemButton v-if="nextTowerFloor" :color="'green'" @click="goToNextFloor()"><IconPower /> {{ t('arena.nextFloor') }}</GemButton>
              </template>
            </div>
          </Transition>
        </div>
      </template>

      <CombatLogFeed v-if="result" :logs="result.logs" :shown-logs="state.shownLogs" :units="state.units" :speed="speed" />
    </CanvasLayer>

    <!-- Экран загрузки: запрос боя, фон арены, спрайт-листы юнитов. Сцена под ним уже собирается и открывается готовой -->
    <Transition name="arena-loading">
      <div v-if="loading || state.preparing" class="arena-loading-screen">
        <div class="arena-loading">{{ t('arena.loadingSprites') }}</div>
      </div>
    </Transition>
  </div>
</template>

<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, provide, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { arenaBackground } from '@/battle/arenaBackgrounds';
import CharacterComponent from '@/components/character-component.vue';
import CombatApiService from '@/services/apiServices/CombatApiService';
import CampaignApiService from '@/services/apiServices/CampaignApiService';
import { CampaignFightResult } from '@/models/types/Campaign';
import TowerApiService from '@/services/apiServices/TowerApiService';
import { TowerFightResult } from '@/models/types/Tower';
import GuildApiService from '@/services/apiServices/GuildApiService';
import { GuildBossAttackResult, GuildBossTier } from '@/models/types/Guild';
import { guildErrorText } from '@/components/guild/guildShared';
import { useGuild } from '@/composables/UseGuild';
import PvpApiService from '@/services/apiServices/PvpApiService';
import { PvpFightResult } from '@/models/types/Pvp';
import { pvpErrorText } from '@/components/pvp/pvpShared';
import { usePvp } from '@/composables/UsePvp';
import EventsApiService from '@/services/apiServices/EventsApiService';
import type { WorldBossAttackResult } from '@/models/types/WorldEvents';
import { useInterface } from '@/composables/UseInterface';
import CanvasComponent from '@/components/canvas/CanvasComponent.vue';
import CanvasLayer from '@/components/canvas/CanvasLayer.vue';
import CombatLogFeed from '@/components/interface/CombatLogFeed.vue';
import ResultBanner from '@/components/interface/ResultBanner.vue';
import ResultParticles from '@/components/interface/ResultParticles.vue';
import GemButton from '@/components/common/GemButton.vue';
import { IconPower } from '@/components/icons/stats';
import { createBattleState, teamUnits } from '@/battle/BattleState';
import { BattlePlayer } from '@/battle/BattlePlayer';
import { BATTLE_FX_KEY, BattleFx } from '@/battle/BattleFx';
import { CombatResult } from '@/battle/types';
import { soundService } from '@/services/soundService/SoundService';
import { useI18n } from 'vue-i18n';
import { IconFlame, IconMute, IconSound } from '@/components/icons/ui';

const { t, te } = useI18n();

// Размер юнитов на арене (1 — кадр 409.6px; по умолчанию у юнитов 0.65)
const UNIT_SCALE = 0.8;

const bgImage = ref('lava');
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

// Режим бесконечной башни: /arena?tower=7 — бой за этаж башни этой недели
const towerFloor = computed(() => Number(route.query.tower) || null);
const towerResult = ref<TowerFightResult | null>(null);
const nextTowerFloor = computed(() => state.finished && towerResult.value?.cleared ? towerResult.value.currentFloor : null);

// Режим босса гильдии: /arena?guildBoss=Easy&attack=<метка> — удар по общему боссу гильдии
const GUILD_BOSS_TIERS: GuildBossTier[] = ['Easy', 'Medium', 'Hard'];
const guildBossTier = computed(() => {
  const tier = route.query.guildBoss as GuildBossTier;
  return GUILD_BOSS_TIERS.includes(tier) ? tier : null;
});
const guildResult = ref<GuildBossAttackResult | null>(null);
const { guildOpen } = useGuild();

// Режим мирового босса: /arena?worldBoss=1&attack=<метка> — удар по общему боссу сервера (ивент «Мировые боссы»)
const worldBossMode = computed(() => route.query.worldBoss === '1');
const worldBossResult = ref<WorldBossAttackResult | null>(null);
const { eventOpen } = useInterface();
const worldBossErrorText = (e: unknown) => {
  const code = (e as { response?: { data?: { code?: string } } }).response?.data?.code;
  return code && te(`worldBoss.errors.${code}`) ? t(`worldBoss.errors.${code}`) : '';
};

// Режим PvP: /arena?pvp=<userId соперника>&attack=<метка> — бой с отрядом другого игрока;
// &ranked=1 — на рейтинговой арене (закреплённые снимки отрядов, очки рейтинга)
const pvpOpponentId = computed(() => (typeof route.query.pvp === 'string' && route.query.pvp) || null);
const pvpRanked = computed(() => !!pvpOpponentId.value && route.query.ranked === '1');
const pvpResult = ref<PvpFightResult | null>(null);
const { pvpOpen, pvpTab } = usePvp();

const state = createBattleState();
const fx = new BattleFx(() => layerRef.value?.particleService ?? null, () => speed.value);
const player = new BattlePlayer(state, fx);
provide(BATTLE_FX_KEY, fx);

const teamA = computed(() => teamUnits(state, 1));
const teamB = computed(() => teamUnits(state, 2));
const playerWon = computed(() => state.winningTeam === 1);

// Итог боя для баннера и частиц: победа — конфетти и звёзды, поражение — серая сцена, дождь и снег.
// Удар по боссу гильдии без убийства — не победа и не поражение: баннер без частиц
const outcome = computed<{ title: string; tone: 'victory' | 'defeat'; particles: boolean } | null>(() => {
  if (!state.finished || error.value !== null) return null;
  if (pvpResult.value) {
    const won = pvpResult.value.won;
    return { title: won ? t('arena.victory') : t('arena.defeat'), tone: won ? 'victory' : 'defeat', particles: true };
  }
  if (guildResult.value) {
    const killed = guildResult.value.killed;
    return { title: killed ? t('arena.guildBossKilled') : t('arena.guildBossHit'), tone: 'victory', particles: killed };
  }
  if (worldBossResult.value) {
    const { killed, damage, currency } = worldBossResult.value;
    return {
      title: killed ? t('arena.guildBossKilled') : t('arena.worldBossHit', { damage: damage.toLocaleString(), currency }),
      tone: 'victory',
      particles: killed,
    };
  }
  return { title: playerWon.value ? t('arena.victory') : t('arena.defeat'), tone: playerWon.value ? 'victory' : 'defeat', particles: true };
});

// Кнопки под баннером появляются по клику в любом месте арены или сами через RESULT_AUTO_OPEN_MS;
// новый бой снова начинается с одного баннера. В обычном бою на арене кнопок нет — тогда и подсказки нет
const RESULT_AUTO_OPEN_MS = 10_000;
const resultOpen = ref(false);
const hasResultActions = computed(() => !!(pvpResult.value || guildResult.value || worldBossResult.value || stageId.value || towerFloor.value));
const canOpenResult = computed(() => !!outcome.value && hasResultActions.value && !resultOpen.value);
let resultTimer = 0;

const openResult = () => {
  if (canOpenResult.value) resultOpen.value = true;
};

watch(canOpenResult, canOpen => {
  window.clearTimeout(resultTimer);
  if (canOpen) resultTimer = window.setTimeout(openResult, RESULT_AUTO_OPEN_MS);
});

watch(() => state.finished, finished => {
  if (!finished) resultOpen.value = false;
});

// Баннер — точка, из-за которой вылетают конфетти и вокруг которой вспыхивают звёзды
const bannerEl = ref<HTMLElement | null>(null);

const bgStyle = computed(() => ({
  // Ключи lava/ice приходят с сервера (Arena.Background, фон босса гильдии)
  backgroundImage: `url('${arenaBackground(bgImage.value)}')`,
  backgroundRepeat: 'round',
  // Фон приглушён, чтобы юниты и эффекты были заметнее
  filter: 'saturate(0.5)',
}));

const setSpeed = (value: number) => {
  speed.value = value;
  player.speed = value;
};

const sound = soundService.settings;
const onVolumeInput = (event: Event) => {
  soundService.setVolume(Number((event.target as HTMLInputElement).value));
};

// Загрузить и распаковать картинку; не загрузилась — не страшно, бой всё равно начнётся
const preloadImage = (url: string) => {
  const image = new Image();
  image.src = url;
  return image.decode().catch(() => undefined);
};

const playFight = async (load: () => Promise<CombatResult>) => {
  if (state.playing || loading.value) return;
  loading.value = true;
  error.value = null;
  // Итог прошлого боя PvP не должен остаться на экране после боя другого режима
  pvpResult.value = null;
  worldBossResult.value = null;

  try {
    result.value = await load();
    // Фон арены (у каждого режима свой) — тоже под экраном загрузки, а не проявляется на глазах
    await preloadImage(arenaBackground(bgImage.value));
  } catch (e) {
    console.error('Ошибка запуска боя:', e);
    // Сервер отвечает 400 с текстом, например "Campaign stage 3 is locked"; гильдия — { code, message }
    const message = (e as { response?: { data?: unknown } }).response?.data;
    error.value = typeof message === 'string'
      ? message
      : pvpOpponentId.value ? pvpErrorText(e, '')
        : guildBossTier.value ? guildErrorText(e, '')
          : worldBossMode.value ? worldBossErrorText(e) : '';
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

const startTowerFight = (floor: number) => playFight(async () => {
  towerResult.value = null;
  const fight = await TowerApiService.fight(floor);
  towerResult.value = fight;
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

const startWorldBossFight = () => playFight(async () => {
  // Метку удара убираем сразу: перезагрузка страницы не должна бить босса ещё раз
  router.replace({ query: { worldBoss: '1' } });
  campaignResult.value = null;
  guildResult.value = null;
  worldBossResult.value = null;
  const attack = await EventsApiService.attackWorldBoss();
  worldBossResult.value = attack;
  bgImage.value = attack.background;
  return attack.combat;
});

const startPvpFight = (opponentId: string, ranked: boolean) => playFight(async () => {
  // Метку боя убираем сразу: перезагрузка страницы не должна начинать бой ещё раз
  router.replace({ query: ranked ? { pvp: opponentId, ranked: '1' } : { pvp: opponentId } });
  campaignResult.value = null;
  guildResult.value = null;
  const fight = ranked ? await PvpApiService.rankedFight(opponentId) : await PvpApiService.fight(opponentId);
  pvpResult.value = fight;
  bgImage.value = fight.arena.background;
  return fight.combat;
});

const startArenaFight = () => {
  if (pvpOpponentId.value) return startPvpFight(pvpOpponentId.value, pvpRanked.value);
  if (guildBossTier.value) return startGuildBossFight(guildBossTier.value);
  if (worldBossMode.value) return startWorldBossFight();
  if (towerFloor.value) return startTowerFight(towerFloor.value);
  return stageId.value
    ? startCampaignFight(stageId.value)
    : playFight(() => CombatApiService.start({ floor: floor.value }));
};

// Новый удар из окна гильдии или бой из окна PvP, когда арена уже открыта: компонент тот же, меняется только метка attack
watch(() => route.query.attack, (attack, previous) => {
  if (attack && attack !== previous && (guildBossTier.value || pvpOpponentId.value || worldBossMode.value)) startArenaFight();
});

const goToGuild = () => {
  guildOpen.value = true;
};
const goToEvent = () => {
  eventOpen.value = true;
};
// Возвращаем на вкладку соперников той арены, на которой шёл бой
const goToPvp = () => {
  pvpTab.value = pvpRanked.value ? 'ranked' : 'fight';
  pvpOpen.value = true;
};
const startBossFight = () => playFight(() => CombatApiService.startBoss(undefined, floor.value));

const goToNextStage = async () => {
  if (!nextStageId.value) return;
  await router.replace({ query: { stage: nextStageId.value } });
  startArenaFight();
};
const goToNextFloor = async () => {
  if (!nextTowerFloor.value) return;
  await router.replace({ query: { tower: nextTowerFloor.value } });
  startArenaFight();
};
// Список кампании (и получение наград) — в городе: окно кампании откроется сразу
const goToCampaign = () => router.push({ name: 'about', query: { campaign: '1' } });
// Там же башня: окно башни откроется сразу
const goToTower = () => router.push({ name: 'about', query: { tower: '1' } });
const skipFight = () => player.skip();

onMounted(() => {
  // Арена босса гильдии без метки удара (например, после перезагрузки) — возвращаем в окно гильдии
  if (guildBossTier.value && !route.query.attack) {
    guildOpen.value = true;
    return;
  }
  // То же для PvP — в окно PvP
  if (pvpOpponentId.value && !route.query.attack) {
    goToPvp();
    return;
  }
  // И для мирового босса — в окно ивента
  if (worldBossMode.value && !route.query.attack) {
    goToEvent();
    return;
  }
  startArenaFight();
});

onBeforeUnmount(() => {
  player.stop();
  window.clearTimeout(resultTimer);
});
</script>

<style lang="scss">
.arena {
  width: var(--stage-w);
  height: var(--stage-h);
  position: fixed;
}

/* fixed-родитель — свой контекст наложения, так что -1 кладёт фон под всё содержимое арены */
.arena-bg {
  position: absolute;
  inset: 0;
  z-index: -1;
}

/* Свой контекст наложения: глубина юнитов не поднимает их над кнопками и итогом боя.
   Тела юнитов сортируются по --depth слота, а шапки, цифры и реплики всех юнитов (--unit-overlay-z) — над всеми телами */
.grid {
  isolation: isolate;
  --unit-overlay-z: 100;
}

.grid-item {
  position: absolute;
  /* Насколько каждый отряд отодвинут от центра: расстановка слотов внутри отряда не меняется,
     растёт только промежуток между командами, чтобы удары передних слотов не пересекались */
  --squad-shift: 5%;
}

/* Глубина тела (character-component): кто ниже на экране, тот ближе и рисуется поверх (--depth — top слота).
   z-index на самом слоте нельзя: он запер бы в слоте и шапку. У слотов 2 и 5 высота одна — поверх тот, что ближе к центру (2-й) */
.grid-item-left-1 {
  top: 50%;
  left: calc(35% - var(--squad-shift));
  --depth: 50;
}

.grid-item-left-2 {
  top: 65%;
  left: calc(30% - var(--squad-shift));
  --depth: 66;
}

.grid-item-left-3 {
  top: 45%;
  left: calc(25% - var(--squad-shift));
  --depth: 45;
}

.grid-item-left-4 {
  top: 55%;
  left: calc(20% - var(--squad-shift));
  --depth: 55;
}

.grid-item-left-5 {
  top: 65%;
  left: calc(15% - var(--squad-shift));
  --depth: 65;
}

.grid-item-right-1 {
  top: 50%;
  right: calc(35% - var(--squad-shift));
  --depth: 50;
}

.grid-item-right-2 {
  top: 65%;
  right: calc(30% - var(--squad-shift));
  --depth: 66;
}

.grid-item-right-3 {
  top: 45%;
  right: calc(25% - var(--squad-shift));
  --depth: 45;
}

.grid-item-right-4 {
  top: 55%;
  right: calc(20% - var(--squad-shift));
  --depth: 55;
}

.grid-item-right-5 {
  top: 65%;
  right: calc(15% - var(--squad-shift));
  --depth: 65;
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

/* Поверх всего, включая канвас эффектов (z-index 9999 в CanvasLayer) */
.arena-loading-screen {
  position: absolute;
  inset: 0;
  z-index: 10000;
  display: flex;
  justify-content: center;
  align-items: center;
  background: radial-gradient(ellipse at center, #1a2232 0%, #0a0f18 75%);
}

.arena-loading-leave-active {
  transition: opacity 0.3s ease;
}

.arena-loading-leave-to {
  opacity: 0;
}

.arena-loading {
  color: white;
  font-size: 20px;
  font-weight: 700;
  letter-spacing: 1px;
  animation: arena-loading-pulse 1s ease-in-out infinite alternate;
}

@keyframes arena-loading-pulse {
  from { opacity: 0.55; }
  to { opacity: 1; }
}

.arena-message.error .result-next {
  display: block;
  margin: 12px auto 0;
}

/* ===== Итог боя: баннер с лентой (ResultBanner), по клику под ним — табличка с итогами ===== */
/* Баннер закреплён сверху: когда под ним выезжает табличка, он стоит на месте, а не уезжает вверх */
.arena-result {
  position: absolute;
  top: 13%;
  left: 50%;
  transform: translateX(-50%);
  z-index: 13;
  display: flex;
  flex-direction: column;
  align-items: center;
}

/* Баннер — кнопка без своего вида: до клика слегка «дышит», при наведении приподнимается */
.arena-result-banner {
  position: relative;
  z-index: 1;
  padding: 0;
  border: none;
  background: none;
  cursor: pointer;
  transition: transform 0.2s ease, filter 0.2s ease;
}

.arena-result-banner:not(.arena-result-banner-open):hover {
  transform: translateY(-4px) scale(1.02);
  filter: brightness(1.08);
}

.arena-result-banner-open {
  cursor: default;
}

.arena-result-hint {
  margin-top: 10px;
  padding: 3px 14px;
  border-radius: 10px;
  background: rgba(0, 0, 0, 0.55);
  color: #ffe2a0;
  font-family: 'Arial', sans-serif;
  font-size: 13px;
  font-weight: bold;
  letter-spacing: 0.04em;
  text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.85);
  pointer-events: none;
  animation: arena-result-hint 1.6s ease-in-out 0.8s infinite both;
}

@keyframes arena-result-hint {
  0%, 100% { opacity: 0.45; }
  50% { opacity: 1; }
}

/* Под баннером — только гем-кнопки в ряд: обратно в окно и дальше. Выпрыгивают снизу по клику */
.result-actions {
  display: flex;
  justify-content: center;
  flex-wrap: wrap;
  gap: 14px;
  margin-top: 22px;
}

.arena-result-actions-enter-active {
  transition: opacity 0.25s ease, transform 0.35s cubic-bezier(0.2, 1.4, 0.4, 1);
}

.arena-result-actions-enter-from {
  opacity: 0;
  transform: translateY(16px) scale(0.85);
}

/* Вуаль над сценой (фон, юниты, эффекты): победа — лёгкая тень, чтобы баннер читался; поражение — сцена теряет цвет */
.arena-result-veil {
  position: absolute;
  inset: 0;
  z-index: 12;
  pointer-events: none;
  animation: arena-result-veil-in 1.2s ease both;
}

.arena-result-veil-victory {
  background: rgba(8, 11, 18, 0.22);
}

.arena-result-veil-defeat {
  backdrop-filter: grayscale(0.9) brightness(0.75);
  background: rgba(20, 24, 32, 0.15);
}

@keyframes arena-result-veil-in {
  from { opacity: 0; }
  to { opacity: 1; }
}

/* Частицы итога — над сценой и вуалью, под баннером: конфетти вылетают из-за него */
.arena-result-particles {
  z-index: 12;
}

@media (prefers-reduced-motion: reduce) {
  .arena-result-hint,
  .arena-result-veil {
    animation: none;
  }

  .arena-result-actions-enter-active {
    transition: none;
  }
}

/* Кнопка в сообщении об ошибке */
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
