<script lang="ts" setup>
import { computed, nextTick, ref, watch } from "vue";
import { useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import ModalComponent from "@/components/common/ModalComponent.vue";
import GemButton from "@/components/common/GemButton.vue";
import FightPreviewList from "@/components/interface/FightPreviewList.vue";
import ClearedStageRow from "@/components/interface/ClearedStageRow.vue";
import StageRewardsFooter from "@/components/interface/StageRewardsFooter.vue";
import CampaignApiService from "@/services/apiServices/CampaignApiService";
import { CampaignOverview, CampaignStageInfo } from "@/models/types/Campaign";
import { scrollToAnchor, useStageRewards } from "@/composables/UseStageRewards";
import { useSquadModal } from "@/composables/UseSquadModal";
import { vDragScroll } from "@/directives/dragScroll";
import { IconBanner, IconLock } from '@/components/icons/ui';
import { IconPower } from '@/components/icons/stats';

// Окно кампании. Список: пройденные уровни с незабранной наградой и последний пройденный (без врагов и прогноза),
// дальше текущий и несколько заблокированных. При открытии список прокручен к последнему пройденному —
// выше остаются награды, которые ещё можно забрать
const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

const { t } = useI18n();
const router = useRouter();

const campaign = ref<CampaignOverview | null>(null);
const loading = ref(false);
// Флаг, а не текст: сообщение переводится в шаблоне и меняется вместе с языком
const error = ref(false);
const scroller = ref<HTMLElement | null>(null);

const load = async (scroll = false) => {
    loading.value = true;
    error.value = false;
    try {
        campaign.value = await CampaignApiService.getCampaign();
        rewards.reset();
    } catch {
        error.value = true;
    } finally {
        loading.value = false;
    }
    if (scroll) {
        await nextTick();
        scrollToAnchor(scroller.value);
    }
};

const rewards = useStageRewards({
    unclaimed: () => campaign.value?.unclaimedRewards ?? 0,
    claimOne: id => CampaignApiService.claimReward(id),
    claimAll: () => CampaignApiService.claimAll(),
    reload: () => load(),
});

// Открыли окно — свежий обзор и прокрутка к текущему уровню; закрыли — забываем, чтобы не мелькал старый список
watch(() => props.modelValue, isOpen => {
    if (isOpen) {
        load(true);
    } else {
        campaign.value = null;
        rewards.dismiss();
    }
}, { immediate: true });

// Бой проигрывается на арене, она сама запросит его у сервера по номеру уровня
const startStage = (stage: CampaignStageInfo) => {
    if (stage.status !== 'Available') return;
    // attack — разовая метка: перезагрузка арены покажет итог этого боя, а не начнёт новый
    router.push({ name: 'arena', query: { stage: stage.id, attack: Date.now() } });
};

// Отряд меняется в общем окне отряда поверх кампании; когда его закрыли — прогноз пересчитывается с новым отрядом
const { squadOpen } = useSquadModal();
watch(squadOpen, (isOpen, wasOpen) => {
    if (!isOpen && wasOpen && props.modelValue) load();
});

const lastClearedIndex = computed(() => (campaign.value?.clearedStages.length ?? 0) - 1);
const status = computed(() => campaign.value
    ? t('campaign.progress', { cleared: campaign.value.clearedCount, total: campaign.value.totalStages })
    : '');
</script>

<template>
  <!-- Ширина — под строку уровня: два отряда по 5 портретов, арена и кнопка боя без переноса -->
  <ModalComponent
    v-model="open"
    :header-message="t('campaign.header')"
    width="min(980px, calc(94 * var(--vw)))"
    height="min(780px, calc(88 * var(--vh)))"
  >
    <div class="stage-modal">
        <div v-if="campaign" class="stage-modal-header">
            <div class="stage-modal-status">
                <span>{{ status }}</span>
                <button class="stage-button" v-tooltip="t('tower.squadHint')" @click="squadOpen = true"><IconBanner /> {{ t('tower.squad') }}</button>
            </div>
        </div>

        <!-- Пока список перезагружается, не прячем его за «Загрузка…» -->
        <div v-if="loading && !campaign" class="stage-message">{{ t('common.loading') }}</div>
        <div v-else-if="error && !campaign" class="stage-message error">{{ t('campaign.loadError') }}</div>
        <div v-else-if="campaign" ref="scroller" v-drag-scroll class="stage-scroll">
            <ul class="stage-list">
                <li
                  v-for="(stage, index) in campaign.clearedStages"
                  :key="`cleared-${stage.id}`"
                  :data-anchor="index === lastClearedIndex ? '' : undefined"
                >
                    <ClearedStageRow
                      :number="stage.id"
                      :title="stage.name"
                      :background="stage.background"
                      :element="stage.element"
                      :boss="stage.type === 'Boss'"
                      :reward-state="rewards.stateOf(stage.rewardId)"
                      :claiming="rewards.busy.value === stage.rewardId"
                      :disabled="!!rewards.busy.value"
                      @claim="rewards.claim(stage.rewardId)"
                    />
                </li>

                <li v-for="stage in campaign.stages" :key="stage.id">
                    <FightPreviewList
                      :title="stage.name"
                      :player-units="campaign.squad"
                      :enemy-units="stage.enemies"
                      :arena="stage.arena"
                      :forecast="stage.forecast"
                      :locked="stage.status === 'Locked'"
                      :current="stage.status === 'Available'"
                      :boss="stage.type === 'Boss'"
                    >
                        <template #list-start>
                            <span :class="{ 'stage-number-boss': stage.type === 'Boss' }">{{ stage.id }}</span>
                        </template>
                        <template #list-end>
                            <GemButton
                              v-if="stage.status === 'Available'"
                              :color="'red'"
                              class="stage-fight stage-fight-ready"
                              v-tooltip="stage.description"
                              @click="startStage(stage)"
                            ><IconPower /> {{ t('campaign.fight') }}</GemButton>
                            <span v-else class="stage-lock" v-tooltip="t('campaign.locked')"><IconLock /></span>
                        </template>
                    </FightPreviewList>
                </li>
            </ul>

            <div v-if="campaign.currentStageId === null" class="stage-list-note">
                {{ t('campaign.completed', { cleared: campaign.clearedCount, total: campaign.totalStages }) }}
            </div>
        </div>
    </div>

    <template #modalFooter>
        <StageRewardsFooter
          :remaining="rewards.remaining.value"
          :busy="rewards.busy.value === 'all'"
          :notice="rewards.notice.value"
          :error="rewards.error.value"
          @claim-all="rewards.claimAll()"
          @dismiss="rewards.dismiss()"
        />
    </template>
  </ModalComponent>
</template>

<style scoped src="./stageModal.css"></style>
