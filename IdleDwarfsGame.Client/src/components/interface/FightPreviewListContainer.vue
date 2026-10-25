<script lang="ts" setup>
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import FightPreviewList from "@/components/interface/FightPreviewList.vue";
import IconButton from "../common/IconButton.vue";
import IconHelm from "../icons/IconHelm.vue";
import CanvasComponent from "../canvas/CanvasComponent.vue";
import CanvasLayer from "../canvas/CanvasLayer.vue";
import CampaignApiService from "@/services/apiServices/CampaignApiService";
import { CampaignOverview, CampaignStageInfo } from "@/models/types/Campaign";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

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
    if (stage.status !== 'Available') return;
    router.push({ name: 'arena', query: { stage: stage.id } });
};

onMounted(loadCampaign);
</script>

<template>
    <div v-if="loading" class="campaign-message">{{ t('common.loading') }}</div>
    <div v-else-if="error" class="campaign-message error">{{ t('campaign.loadError') }}</div>
    <div v-else-if="campaign && campaign.currentStageId === null" class="campaign-message">
        {{ t('campaign.completed', { cleared: campaign.clearedCount, total: campaign.totalStages }) }}
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
                    <CanvasLayer v-if="stage.status === 'Available'" :page="false">
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
</template>

<style scoped>
.list-wrapper{
    width:100%;
    padding:0;
}

.list{
    list-style: none;
    margin-bottom: 25px;
}

.stage-boss{
    color: #ffd166;
    text-shadow: 0 0 10px #ff7b00, 1px 1px 2px #000000;
}

.stage-lock{
    font-size: 36px;
    filter: drop-shadow(0 2px 3px #000);
}

.campaign-message{
    width: 100%;
    align-self: center;
    text-align: center;
    font-size: 1.2em;
    text-shadow: 0 1px 3px #000;
}

.campaign-message.error{
    color: #fca5a5;
}
</style>
