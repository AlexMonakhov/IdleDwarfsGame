<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import UnitPortrait from '@/components/interface/UnitPortrait.vue';
import SocialApiService from '@/services/apiServices/SocialApiService';
import CampaignApiService from '@/services/apiServices/CampaignApiService';
import { useSocial } from '@/composables/UseSocial';
import type { MercenaryFriendInfo, MercenaryOverview } from '@/models/types/Social';
import { openExternalFight, socialErrorText } from '@/components/social/socialShared';
import { IconElderSeal, IconSword } from '@/components/icons/ui';

// Наёмники: герой из отряда друга на один бой кампании (сервер: FriendHelpService.FightCampaignAsync)
const emit = defineEmits<{
    (e: 'navigate'): void;
    (e: 'message', text: string, isError: boolean): void;
}>();

const { t } = useI18n();
const router = useRouter();
const { friendsOpen, friendsTab } = useSocial();

const SLOTS = [1, 2, 3, 4, 5];

const overview = ref<MercenaryOverview | null>(null);
const stageId = ref<number | null>(null);
const loadError = ref(false);
const picked = ref<{ friend: MercenaryFriendInfo; heroId: string } | null>(null);
const slot = ref(5);

onMounted(async () => {
    try {
        const [mercenaries, campaign] = await Promise.all([SocialApiService.getMercenaries(), CampaignApiService.getCampaign()]);
        overview.value = mercenaries;
        stageId.value = campaign.currentStageId;
    } catch (error) {
        loadError.value = true;
        emit('message', socialErrorText(error, 'mercenary'), true);
    }
});

const pickedName = computed(() => picked.value?.friend.heroes.find(h => h.heroId === picked.value?.heroId)?.unit.name ?? '');
const canFight = computed(() => !!picked.value && !!stageId.value && (overview.value?.hiresLeft ?? 0) > 0);

const pick = (friend: MercenaryFriendInfo, heroId: string) => {
    if (friend.hiredToday) return;
    picked.value = { friend, heroId };
};

const fight = () => {
    if (!canFight.value || !picked.value || !stageId.value) return;
    const { friend, heroId } = picked.value;
    const stage = stageId.value;
    const name = pickedName.value;
    const chosenSlot = slot.value;
    emit('navigate');
    openExternalFight(router, {
        title: t('social.mercenary.title', { stage, name }),
        load: async () => {
            try {
                const result = await SocialApiService.fightWithMercenary(stage, friend.userId, heroId, chosenSlot);
                return { combat: result.fight.combat, background: result.fight.arena.background };
            } catch (error) {
                // Арена показывает строку ответа как есть
                throw { response: { data: socialErrorText(error, 'mercenary') } };
            }
        },
        backLabel: t('social.mercenary.back'),
        back: () => {
            friendsTab.value = 'mercenaries';
            friendsOpen.value = true;
        },
    });
};
</script>

<template>
    <div class="mercenaries">
        <div v-if="!overview" class="merc-hint">{{ loadError ? t('social.loadError') : t('common.loading') }}</div>
        <template v-else>
            <p class="merc-hint">{{ t('social.mercenary.hint', { seals: overview.lenderElderSeals }) }}</p>
            <p class="merc-hint">
                {{ t('social.mercenary.left', { left: overview.hiresLeft, max: overview.maxHires }) }} ·
                {{ stageId ? t('social.mercenary.stage', { stage: stageId }) : t('social.mercenary.campaignDone') }}
            </p>

            <ul v-if="overview.friends.length" class="merc-list">
                <li v-for="friend in overview.friends" :key="friend.userId" class="merc-friend" :class="{ 'merc-friend-hired': friend.hiredToday }">
                    <div class="merc-friend-head">
                        <span class="merc-friend-name">{{ friend.username }}</span>
                        <span v-if="friend.hiredToday" class="merc-badge">{{ t('social.mercenary.hired') }}</span>
                    </div>
                    <div class="merc-heroes">
                        <button
                            v-for="hero in friend.heroes"
                            :key="hero.heroId"
                            type="button"
                            class="merc-hero"
                            :class="{ 'merc-hero-picked': picked?.heroId === hero.heroId }"
                            :disabled="friend.hiredToday"
                            @click="pick(friend, hero.heroId)"
                        >
                            <UnitPortrait :unit="hero.unit" :size="52" />
                        </button>
                    </div>
                </li>
            </ul>
            <div v-else class="merc-hint">{{ t('social.mercenary.noFriends') }}</div>

            <div v-if="picked" class="merc-footer">
                <span>{{ t('social.mercenary.slot') }}:</span>
                <button
                    v-for="value in SLOTS"
                    :key="value"
                    type="button"
                    class="merc-slot"
                    :class="{ 'merc-slot-active': slot === value }"
                    @click="slot = value"
                >{{ value }}</button>
                <GemButton :color="'red'" :disabled="!canFight" @click="fight">
                    <IconSword /> {{ t('social.mercenary.fight', { name: pickedName }) }}
                </GemButton>
                <span class="merc-hint"><IconElderSeal /> +{{ overview.lenderElderSeals }} → {{ picked.friend.username }}</span>
            </div>
        </template>
    </div>
</template>

<style scoped>
.mercenaries {
    display: flex;
    flex-direction: column;
    row-gap: 8px;
    font-family: 'Arial', sans-serif;
    text-align: left;
    color: #f8fafc;
}

.merc-hint {
    margin: 0;
    color: #cbd5e1;
    font-size: 13px;
}

.merc-list {
    display: flex;
    flex-direction: column;
    row-gap: 8px;
    margin: 0;
    padding: 0;
    list-style: none;
}

.merc-friend {
    padding: 8px 12px;
    border-radius: 10px;
    border: 2px solid rgba(157, 173, 175, 0.5);
    background: rgba(15, 23, 32, 0.7);
}

.merc-friend-hired {
    opacity: 0.55;
}

.merc-friend-head {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 6px;
}

.merc-friend-name {
    font-weight: bold;
}

.merc-badge {
    padding: 1px 8px;
    border-radius: 8px;
    background: rgba(250, 204, 21, 0.25);
    color: #fde68a;
    font-size: 11px;
}

.merc-heroes {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
}

.merc-hero {
    padding: 2px;
    border: 2px solid transparent;
    border-radius: 10px;
    background: none;
    cursor: pointer;
}

.merc-hero:disabled {
    cursor: default;
}

.merc-hero-picked {
    border-color: #fbbf24;
    box-shadow: 0 0 10px rgba(251, 191, 36, 0.6);
}

.merc-footer {
    position: sticky;
    bottom: 0;
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 8px;
    padding: 8px 10px;
    border-radius: 10px;
    background: rgba(15, 23, 32, 0.95);
    border: 2px solid rgba(251, 191, 36, 0.5);
}

.merc-slot {
    width: 30px;
    height: 30px;
    border-radius: 8px;
    border: 2px solid rgba(157, 173, 175, 0.6);
    background: rgba(0, 0, 0, 0.4);
    color: #fff;
    font-weight: bold;
    cursor: pointer;
}

.merc-slot-active {
    border-color: #fbbf24;
    color: #fde68a;
}
</style>
