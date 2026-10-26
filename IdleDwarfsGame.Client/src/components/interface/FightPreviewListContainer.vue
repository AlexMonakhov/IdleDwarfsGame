<script lang="ts" setup>
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import FightPreviewList from "@/components/interface/FightPreviewList.vue";
import IconButton from "../common/IconButton.vue";
import IconHelm from "../icons/IconHelm.vue";
import CanvasComponent from "../canvas/CanvasComponent.vue";
import CanvasLayer from "../canvas/CanvasLayer.vue";
import CampaignApiService from "@/services/apiServices/CampaignApiService";
import { CampaignOverview, CampaignStageInfo } from "@/models/types/Campaign";
import { RewardInfo } from "@/models/types/Inventory";
import { useResources } from "@/composables/UseResources";
import { useI18n } from "vue-i18n";

const { t, locale } = useI18n();
const { set: setResources } = useResources();

interface Props {
    height?: string;
}

const props = withDefaults(defineProps<Props>(), {
  height: '30%'
})

const router = useRouter();
const campaign = ref<CampaignOverview | null>(null);
const loading = ref(false);
// Флаг, а не текст: сообщение переводится в шаблоне и меняется вместе с языком
const error = ref(false);

const loadCampaign = async () => {
    loading.value = true;
    error.value = false;
    try {
        campaign.value = await CampaignApiService.getCampaign();
    } catch {
        error.value = true;
    } finally {
        loading.value = false;
    }
};

// Бой проигрывается на арене, она сама запросит его у сервера по номеру уровня
const startStage = (stage: CampaignStageInfo) => {
    if (stage.status !== 'Available' || campaign.value?.pendingRewardId) return;
    router.push({ name: 'arena', query: { stage: stage.id } });
};

// Награда за пройденный уровень уже выдана на сервере, но на аккаунт попадает только по кнопке
const claiming = ref(false);
const claimError = ref(false);
const claimed = ref<RewardInfo | null>(null);

const claimReward = async () => {
    const rewardId = campaign.value?.pendingRewardId;
    if (!rewardId || claiming.value) return;
    claiming.value = true;
    claimError.value = false;
    try {
        const result = await CampaignApiService.claimReward(rewardId);
        setResources(result.resources);
        claimed.value = result.reward;
    } catch {
        claimError.value = true;
    } finally {
        claiming.value = false;
    }
    // И после ошибки (награду уже забрали в другой вкладке) показываем актуальное состояние
    await loadCampaign();
};

const claimedChests = computed(() => (claimed.value?.chests ?? [])
    .map(chest => t('chests.name', { type: t(`chests.types.${chest.type}`), level: chest.level }))
    .join(', '));

const format = (value: number) => value.toLocaleString(locale.value);

onMounted(loadCampaign);
</script>

<template>
  <!-- Колонка: баннер награды над списком, а не сбоку от него в ряду тела модалки -->
  <div class="campaign">
    <div v-if="claimed" class="campaign-reward">
        <span class="campaign-reward-title">{{ t('rewards.claimedTitle') }}</span>
        <span>🪙 {{ format(claimed.gold) }}</span>
        <span>✦ {{ format(claimed.experience) }}</span>
        <span v-if="claimedChests">🎁 {{ claimedChests }}</span>
        <span v-if="claimed.chests.length" class="campaign-reward-hint">{{ t('rewards.chestsHint') }}</span>
        <button class="campaign-reward-close" :title="t('common.close')" @click="claimed = null">✕</button>
    </div>
    <div v-if="claimError" class="campaign-message error">{{ t('rewards.claimError') }}</div>

    <!-- Пока список перезагружается после получения награды, не прячем его за «Загрузка…» -->
    <div v-if="loading && !campaign" class="campaign-message">{{ t('common.loading') }}</div>
    <div v-else-if="error" class="campaign-message error">{{ t('campaign.loadError') }}</div>
    <div v-else-if="campaign && campaign.currentStageId === null" class="campaign-message">
        {{ t('campaign.completed', { cleared: campaign.clearedCount, total: campaign.totalStages }) }}
        <!-- Награда за последний уровень: забрать её больше негде -->
        <button v-if="campaign.pendingRewardId" class="campaign-claim-inline" :disabled="claiming" @click="claimReward">
            🎁 {{ t('rewards.claim') }}
        </button>
    </div>
    <ul v-else-if="campaign" class="list-wrapper">
        <li v-for="stage in campaign.stages" :key="stage.id" class="list" :style="{height: props.height}">

            <FightPreviewList
              :title="stage.name"
              :player-units="campaign.squad"
              :enemy-units="stage.enemies"
              :arena="stage.arena"
              :forecast="stage.forecast"
              :locked="stage.status === 'Locked'"
            >

                <template #list-start>
                    <span :class="{ 'stage-boss': stage.type === 'Boss' }">{{ stage.id }}</span>
                </template>
                <template #list-end>
                    <!-- Пока награда за пройденный уровень не забрана, вместо боя — получение награды -->
                    <CanvasLayer v-if="stage.status === 'Available' && campaign.pendingRewardId" :page="false">
                    <CanvasComponent
                    :effects="[{ func: 'god-rays-radiance', padding: 40, isUnder: true },{ func: 'particles', padding: 100, isUnder: false }]"
                    >
                        <IconButton inner class="stage-claim" :title="t('rewards.claim')" :disabled="claiming" @click="claimReward">
                            <span class="stage-claim-icon">🎁</span>
                            <span class="stage-claim-label">{{ t('rewards.claim') }}</span>
                        </IconButton>
                    </CanvasComponent>
                    </CanvasLayer>
                    <CanvasLayer v-else-if="stage.status === 'Available'" :page="false">
                    <CanvasComponent
                    :effects="[{ func: 'god-rays-radiance', padding: 40, isUnder: true },{ func: 'particles', padding: 100, isUnder: false }]"
                    >
                        <IconButton inner :title="stage.description" @click="startStage(stage)"><IconHelm size="60px" primary-color="rgb(217 215 178)" secondary-color="rgb(64 255 251)"></IconHelm></IconButton>
                    </CanvasComponent>
                    </CanvasLayer>
                    <span v-else class="stage-lock" :title="t('campaign.locked')">🔒</span>
                </template>
            </FightPreviewList>

        </li>
    </ul>
  </div>
</template>

<style scoped>
.campaign{
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
}

/* Высота списка — ровно остаток окна: от неё считаются проценты высоты уровней (props.height),
   а сами уровни выходят за список и прокручиваются в теле модалки */
.list-wrapper{
    flex: 1 0 0;
    min-height: 0;
    width:100%;
    margin: 0;
    padding:0;
}

.list{
    list-style: none;
    margin-bottom: 25px;
}

/* После последнего уровня пустого хвоста нет — прокрутка заканчивается на нём */
.list:last-child{
    margin-bottom: 0;
}

.stage-boss{
    color: #ffd166;
    text-shadow: 0 0 10px #ff7b00, 1px 1px 2px #000000;
}

.stage-lock{
    font-size: 36px;
    filter: drop-shadow(0 2px 3px #000);
}

/* Без списка сообщение встаёт посередине окна; со списком тот забирает всё место и отступы обнуляются */
.campaign-message{
    width: 100%;
    margin: auto 0;
    text-align: center;
    font-size: 1.2em;
    text-shadow: 0 1px 3px #000;
}

.campaign-message.error{
    color: #fca5a5;
}

.campaign-reward{
    position: relative;
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    justify-content: center;
    gap: 6px 16px;
    margin: 0 0 20px;
    padding: 12px 40px;
    border: 1px solid rgba(255, 209, 102, 0.6);
    border-radius: 12px;
    background: rgba(14, 19, 34, 0.75);
    color: #f8fafc;
    text-shadow: 0 1px 3px #000;
}

.campaign-reward-title{
    color: #ffd166;
    font-weight: 700;
}

.campaign-reward-hint{
    flex-basis: 100%;
    text-align: center;
    font-size: 0.85em;
    color: #cbd5e1;
}

.campaign-reward-close{
    position: absolute;
    top: 6px;
    right: 8px;
    border: none;
    background: none;
    color: #cbd5e1;
    cursor: pointer;
}

.campaign-claim-inline{
    display: block;
    margin: 12px auto 0;
    border: 1px solid #ffd166;
    border-radius: 8px;
    padding: 8px 16px;
    background: rgba(14, 19, 34, 0.7);
    color: #ffd166;
    cursor: pointer;
}

.stage-claim{
    display: flex;
    flex-direction: column;
    align-items: center;
}

.stage-claim-icon{
    font-size: 40px;
    line-height: 1;
    filter: drop-shadow(0 2px 3px #000);
}

.stage-claim-label{
    margin-top: 2px;
    font-size: 11px;
    font-weight: 700;
    color: #ffd166;
    text-shadow: 0 1px 2px #000;
}
</style>
