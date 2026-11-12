<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import SocialApiService from '@/services/apiServices/SocialApiService';
import HeroesApiService from '@/services/apiServices/HeroesApiService';
import { useResources } from '@/composables/UseResources';
import { HeroInfo, isBusy } from '@/models/types/Heroes';
import type { GuildDepthOverview } from '@/models/types/Social';
import { elementText as elementColor, formatLeft, formatNumber, socialErrorText } from '@/components/social/socialShared';
import { IconCoin, IconHonorBadge, IconMinecart, IconPickaxe, IconTrophy } from '@/components/icons/ui';
import { GuildRun } from './guildShared';

// «Гонка глубины» — недельное соревнование гильдий (сервер: GuildDepthRules). Вкладка окна гильдии под боссами
const props = defineProps<{
    busy: string | null;
    run: GuildRun;
    serverNow: number;
}>();

const { t } = useI18n();
const { set: setResources } = useResources();

// Добычу дворфов досчитываем на фронте между перечитываниями, а раз в столько — берём с сервера
const REFRESH_MS = 30_000;

const overview = ref<GuildDepthOverview | null>(null);
const heroes = ref<HeroInfo[]>([]);
const loadError = ref(false);
// Момент, на который сервер посчитал добычу (по часам сервера)
let loadedAt = 0;
let timer = 0;

const load = async () => {
    try {
        const [next, myHeroes] = await Promise.all([SocialApiService.getDepth(), HeroesApiService.getMyHeroes()]);
        overview.value = next;
        heroes.value = myHeroes;
        loadedAt = new Date(next.serverTime).getTime();
        loadError.value = false;
    } catch {
        loadError.value = true;
    }
};

onMounted(() => {
    load();
    timer = window.setInterval(() => {
        if (!props.busy) load();
    }, REFRESH_MS);
});
onBeforeUnmount(() => window.clearInterval(timer));

// Часы с последнего ответа сервера — добыча растёт и без запросов (до конца гонки)
const hoursSinceLoad = computed(() => {
    if (!overview.value?.active) return 0;
    const until = Math.min(props.serverNow, new Date(overview.value.endsAt).getTime());
    return Math.max(0, (until - loadedAt) / 3_600_000);
});

const myDwarf = computed(() => overview.value?.myDwarf ?? null);
const liveStored = computed(() => {
    const dwarf = myDwarf.value;
    if (!dwarf) return 0;
    return Math.max(dwarf.stored, Math.min(dwarf.storage, dwarf.stored + dwarf.rate * hoursSinceLoad.value));
});
const liveStoredGold = computed(() => {
    const dwarf = myDwarf.value;
    if (!dwarf || dwarf.stored <= 0) return dwarf && dwarf.rate > 0 ? Math.floor(dwarf.goldPerHour / dwarf.rate * liveStored.value) : 0;
    return Math.floor(dwarf.storedGold / dwarf.stored * liveStored.value);
});
const guildRate = computed(() => overview.value?.members.reduce((sum, m) => sum + m.rate, 0) ?? 0);
const liveGuildScore = computed(() => (overview.value?.guildScore ?? 0) + guildRate.value * hoursSinceLoad.value);
const storagePercent = computed(() => (myDwarf.value?.storage ? Math.min(100, liveStored.value / myDwarf.value.storage * 100) : 0));

const timeText = computed(() => {
    if (!overview.value) return '';
    return overview.value.active
        ? t('social.depth.endsIn', { time: formatLeft(new Date(overview.value.endsAt).getTime() - props.serverNow) })
        : t('social.depth.results', { time: formatLeft(new Date(overview.value.nextWeekAt).getTime() - props.serverNow) });
});

// Свободные дворфы: не в отряде и не заняты; свой нынешний дворф — тоже не в списке
const freeHeroes = computed(() => heroes.value
    .filter(h => h.placeInSquad == null && !isBusy(h) && h.id !== myDwarf.value?.heroId)
    .sort((a, b) => b.level - a.level || a.name.localeCompare(b.name)));
const choosing = ref(false);
const showPicker = computed(() => overview.value?.active && !overview.value.placedElsewhere && (!myDwarf.value?.heroId || choosing.value));

const gold = computed(() => overview.value?.resources.gold ?? 0);
const x = (factor: number) => factor.toFixed(2);

/* ===== Действия ===== */
const act = (key: string, action: () => Promise<GuildDepthOverview>, done?: (next: GuildDepthOverview) => string | void) =>
    props.run(key, async () => {
        try {
            const next = await action();
            overview.value = next;
            loadedAt = new Date(next.serverTime).getTime();
            setResources(next.resources);
            heroes.value = await HeroesApiService.getMyHeroes();
            return done?.(next) ?? undefined;
        } catch (error) {
            // run окна гильдии показывает message ответа — отдаём ему уже переведённый текст
            throw { response: { data: { message: socialErrorText(error, 'depth') } } };
        }
    });

const place = (hero: HeroInfo) => act(`depth-place-${hero.id}`, () => SocialApiService.placeDepthDwarf(hero.id), () => {
    choosing.value = false;
});
const recall = () => act('depth-recall', () => SocialApiService.recallDepthDwarf());
const collect = () => act('depth-collect', () => SocialApiService.collectDepth(), () => t('social.depth.collected'));
const upgradePick = () => act('depth-pick', () => SocialApiService.upgradeDepthPick());
const upgradeShaft = () => act('depth-shaft', () => SocialApiService.upgradeDepthShaft());
const claim = () => {
    const badges = overview.value?.pending?.honorBadges ?? 0;
    return act('depth-claim', () => SocialApiService.claimDepth(), () => t('social.depth.claimed', { badges }));
};

const rewardPlace = (index: number) => {
    const tiers = overview.value?.rewards ?? [];
    const from = tiers[index].fromPlace;
    const next = tiers[index + 1]?.fromPlace;
    if (!next) return t('social.depth.placeFrom', { from });
    return next - from > 1 ? `${from}–${next - 1}` : `${from}`;
};
</script>

<template>
    <div class="guild-page depth">
        <div v-if="!overview" class="guild-hint">{{ loadError ? t('social.loadError') : t('common.loading') }}</div>

        <template v-else>
            <p class="guild-hint">{{ t('social.depth.intro') }}</p>

            <!-- Сводка гильдии: глубина, место, время -->
            <section class="depth-summary">
                <div class="depth-summary-main">
                    <span class="depth-score"><IconMinecart /> {{ t('social.depth.depth') }}: {{ formatNumber(liveGuildScore) }}</span>
                    <span class="depth-muted">
                        {{ overview.guildPlace ? t('social.depth.guildPlace', { place: overview.guildPlace, count: overview.guildCount }) : t('social.depth.notRanked') }}
                    </span>
                    <span class="depth-muted">{{ timeText }}</span>
                </div>
                <GemButton
                    v-if="overview.pending"
                    :color="'yellow'"
                    :disabled="!!busy"
                    @click="claim"
                ><IconHonorBadge /> {{ t('social.depth.claim', { badges: overview.pending.honorBadges, place: overview.pending.place }) }}</GemButton>
            </section>

            <!-- Подъёмник — общее улучшение -->
            <section class="guild-section">
                <h4 class="guild-subtitle">{{ t('social.depth.shaft') }}</h4>
                <div class="guild-row">
                    <div class="guild-row-main">
                        <span>{{ t('social.depth.shaftLevel', { level: overview.shaftLevel, max: overview.maxShaftLevel, factor: x(overview.shaftFactor) }) }}</span>
                        <span v-if="overview.shaftCostGold" class="depth-muted">→ ×{{ x(overview.nextShaftFactor) }}</span>
                    </div>
                    <GemButton
                        v-if="overview.shaftCostGold && overview.active"
                        :color="'green'"
                        :disabled="!!busy || gold < overview.shaftCostGold"
                        @click="upgradeShaft"
                    ><IconCoin /> {{ formatNumber(overview.shaftCostGold) }} · {{ t('social.depth.upgradeShaft') }}</GemButton>
                </div>
            </section>

            <!-- Свой дворф -->
            <section class="guild-section">
                <h4 class="guild-subtitle">{{ t('social.depth.myDwarf') }}</h4>

                <div v-if="overview.placedElsewhere" class="guild-hint">{{ t('social.depth.placedElsewhere') }}</div>

                <div v-else-if="myDwarf" class="guild-row depth-dwarf">
                    <div class="guild-row-main">
                        <span v-if="myDwarf.heroId" class="depth-name" :style="{ color: elementColor(myDwarf.element) }">
                            {{ myDwarf.name }} · ★{{ myDwarf.rank }}
                        </span>
                        <span class="depth-muted">{{ t('social.depth.rate', { rate: myDwarf.rate.toFixed(1), gold: formatNumber(myDwarf.goldPerHour) }) }}</span>
                        <div class="depth-bar"><div class="depth-bar-fill" :style="{ width: `${storagePercent}%` }"></div></div>
                        <span class="depth-muted">
                            {{ t('social.depth.storage', { stored: formatNumber(liveStored), storage: formatNumber(myDwarf.storage), gold: formatNumber(liveStoredGold) }) }}
                        </span>
                        <span class="depth-muted">{{ t('social.depth.mined', { mined: formatNumber(myDwarf.mined + myDwarf.rate * hoursSinceLoad) }) }}</span>
                        <span class="depth-muted"><IconPickaxe /> {{ t('social.depth.pick', { level: myDwarf.pickLevel, max: myDwarf.maxPickLevel, factor: x(myDwarf.pickFactor) }) }}</span>
                    </div>
                    <div class="depth-actions">
                        <GemButton :color="'yellow'" :disabled="!!busy || liveStoredGold <= 0" @click="collect"><IconCoin /> {{ t('social.depth.collect') }}</GemButton>
                        <GemButton
                            v-if="myDwarf.pickCostGold && overview.active && myDwarf.heroId"
                            :color="'green'"
                            :disabled="!!busy || gold < myDwarf.pickCostGold"
                            @click="upgradePick"
                        ><IconCoin /> {{ formatNumber(myDwarf.pickCostGold) }} · {{ t('social.depth.upgradePick') }}</GemButton>
                        <template v-if="overview.active && myDwarf.heroId">
                            <GemButton :color="'blue'" :disabled="!!busy" @click="choosing = !choosing">{{ t('social.depth.replace') }}</GemButton>
                            <GemButton :color="'red'" :disabled="!!busy" @click="recall">{{ t('social.depth.recall') }}</GemButton>
                        </template>
                    </div>
                </div>

                <template v-if="showPicker">
                    <div class="guild-hint">{{ myDwarf && !myDwarf.heroId ? t('social.depth.recalled') : t('social.depth.noDwarf') }}</div>
                    <ul v-if="freeHeroes.length" class="guild-list depth-picker">
                        <li v-for="hero in freeHeroes" :key="hero.id" class="guild-row">
                            <div class="guild-row-main">
                                <span class="depth-name" :style="{ color: elementColor(hero.element) }">{{ hero.name }} · ★{{ hero.level }}</span>
                                <span class="depth-muted">{{ hero.profession ? t(`professions.${hero.profession}`) : '' }}</span>
                            </div>
                            <GemButton :color="'green'" :disabled="!!busy" @click="place(hero)">{{ t('social.depth.place') }}</GemButton>
                        </li>
                    </ul>
                    <div v-else class="guild-hint">{{ t('social.depth.noFreeDwarves') }}</div>
                </template>
            </section>

            <!-- Шахтёры гильдии -->
            <section v-if="overview.members.length" class="guild-section">
                <h4 class="guild-subtitle">{{ t('social.depth.members') }}</h4>
                <ul class="guild-list">
                    <li v-for="member in overview.members" :key="member.userId" class="guild-row depth-compact" :class="{ 'depth-me': member.isMe }">
                        <div class="guild-row-main">
                            <span>{{ member.username }}<template v-if="member.heroName"> · <span :style="{ color: elementColor(member.element) }">{{ member.heroName }} ★{{ member.rank }}</span></template></span>
                            <span class="depth-muted">{{ member.rate.toFixed(1) }}/h · <IconPickaxe /> {{ member.pickLevel }}</span>
                        </div>
                        <span class="depth-score-small">{{ formatNumber(member.mined + member.rate * hoursSinceLoad) }}</span>
                    </li>
                </ul>
            </section>

            <!-- Рейтинг гильдий и награды -->
            <section class="guild-section depth-columns">
                <div>
                    <h4 class="guild-subtitle"><IconTrophy /> {{ t('social.depth.leaderboard') }}</h4>
                    <ol class="guild-list">
                        <li v-for="standing in overview.leaderboard" :key="standing.guildId" class="guild-row depth-compact" :class="{ 'depth-me': standing.isMine }">
                            <span class="depth-place">{{ standing.place }}</span>
                            <div class="guild-row-main">
                                <span>{{ standing.name }}</span>
                                <span class="depth-muted">👥 {{ standing.members }} · <IconHonorBadge /> {{ standing.honorBadges }}</span>
                            </div>
                            <span class="depth-score-small">{{ formatNumber(standing.score) }}</span>
                        </li>
                    </ol>
                </div>
                <div>
                    <h4 class="guild-subtitle">{{ t('social.depth.rewards') }}</h4>
                    <ul class="guild-list">
                        <li v-for="(tier, index) in overview.rewards" :key="tier.fromPlace" class="guild-row depth-compact">
                            <span class="depth-place">{{ rewardPlace(index) }}</span>
                            <span class="depth-score-small"><IconHonorBadge /> {{ tier.honorBadges }}</span>
                        </li>
                    </ul>
                </div>
            </section>
        </template>
    </div>
</template>

<style scoped src="./guild.css"></style>

<style scoped>
.depth {
    color: #f8fafc;
    font-size: 13px;
}

.depth-summary {
    display: flex;
    align-items: center;
    gap: 12px;
    flex-wrap: wrap;
    margin-bottom: 14px;
    padding: 10px 14px;
    border-radius: 12px;
    border: 2px solid rgba(250, 204, 21, 0.45);
    background: linear-gradient(135deg, rgba(66, 45, 16, 0.85), rgba(15, 23, 32, 0.85));
}

.depth-summary-main {
    flex: 1;
    display: flex;
    flex-direction: column;
    row-gap: 3px;
}

.depth-score {
    font-size: 18px;
    font-weight: bold;
    color: #fde68a;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.depth-score-small {
    font-weight: bold;
    color: #fde68a;
    white-space: nowrap;
}

.depth-muted {
    color: #cbd5e1;
    font-size: 12px;
}

.depth-name {
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.depth-dwarf {
    align-items: flex-start;
    padding: 10px 12px;
}

.depth-actions {
    display: flex;
    flex-direction: column;
    gap: 6px;
    align-items: stretch;
}

.depth-bar {
    height: 8px;
    margin: 3px 0;
    border-radius: 4px;
    background: rgba(0, 0, 0, 0.5);
    overflow: hidden;
}

.depth-bar-fill {
    height: 100%;
    background: linear-gradient(90deg, #f59e0b, #fde047);
    transition: width 1s linear;
}

.depth-picker {
    max-height: 260px;
    overflow-y: auto;
}

.depth-compact {
    min-height: 40px;
}

.depth-me {
    border-color: rgba(250, 204, 21, 0.7);
}

.depth-place {
    min-width: 34px;
    font-weight: bold;
    color: #fbbf24;
}

.depth-columns {
    display: grid;
    grid-template-columns: 2fr 1fr;
    gap: 12px;
}

@media (max-width: 640px) {
    .depth-columns {
        grid-template-columns: 1fr;
    }
}
</style>
