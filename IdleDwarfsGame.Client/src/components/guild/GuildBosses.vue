<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import UnitPortrait from '@/components/interface/UnitPortrait.vue';
import CssAuraEffect from '@/components/effects/CssAuraEffect.vue';
import type { FrameEffect } from '@/models/types/Account';
import SquadRow from '@/components/inventory/SquadRow.vue';
import GuildApiService from '@/services/apiServices/GuildApiService';
import HeroesApiService from '@/services/apiServices/HeroesApiService';
import { HeroInfo, squadSlotIds } from '@/models/types/Heroes';
import { useResources } from '@/composables/UseResources';
import { useSquadModal } from '@/composables/UseSquadModal';
import { GuildBossesOverview, GuildBossInfo, GuildBossTier } from '@/models/types/Guild';
import { RewardInfo } from '@/models/types/Inventory';
import { GuildRun, formatTime } from './guildShared';
import { arenaBackground } from '@/battle/arenaBackgrounds';

// Боссы гильдии: общее на гильдию HP, удар раз в час, награда за убитого босса — раз в сутки на аккаунт
const props = defineProps<{
    busy: string | null;
    run: GuildRun;
    serverNow: number;
}>();

// Бой проигрывается на арене — окно нужно закрыть
const emit = defineEmits<{ (e: 'navigate'): void }>();

const { t, locale } = useI18n();
const router = useRouter();
const { set: setResources } = useResources();

// Пока вкладка открыта, перечитываем боссов — их бьют и другие участники
const REFRESH_MS = 30_000;
// Сколько лучших по урону показывать в карточке
const TOP_DAMAGE = 3;

const overview = ref<GuildBossesOverview | null>(null);
// Герои игрока — отряд показываем тот же, что в окне отряда
const heroes = ref<HeroInfo[]>([]);
const loading = ref(false);
const loadError = ref(false);
let timer = 0;

const load = async () => {
    loading.value = true;
    try {
        const [nextBosses, nextHeroes] = await Promise.all([GuildApiService.getBosses(), HeroesApiService.getMyHeroes()]);
        overview.value = nextBosses;
        heroes.value = nextHeroes;
        loadError.value = false;
    } catch {
        loadError.value = true;
    } finally {
        loading.value = false;
    }
};

onMounted(() => {
    load();
    timer = window.setInterval(() => {
        if (!loading.value && !props.busy) load();
    }, REFRESH_MS);
});

onBeforeUnmount(() => window.clearInterval(timer));

const msUntil = (iso: string | null) => (iso ? new Date(iso).getTime() - props.serverNow : 0);

// Наступили новые сутки — боссы на сервере уже свежие
watch(() => props.serverNow, () => {
    if (overview.value && !loading.value && msUntil(overview.value.resetAt) <= 0) load();
});

const bosses = computed(() => overview.value?.bosses ?? []);

/* ===== Отряд ===== */
// Окно отряда открывается поверх гильдии; когда его закрыли — перечитываем боссов вместе с новым отрядом
const { squadOpen } = useSquadModal();

watch(squadOpen, (isOpen, wasOpen) => {
    if (!isOpen && wasOpen) load();
});

const squad = computed(() => {
    const byId = new Map(heroes.value.map(h => [h.id, h]));
    return squadSlotIds(heroes.value).map(id => (id ? byId.get(id) ?? null : null));
});
const squadPower = computed(() => squad.value.reduce((sum, hero) => sum + (hero?.power.total ?? 0), 0));
const noSquad = computed(() => !!overview.value && squad.value.every(hero => !hero));

/* ===== Карточка ===== */
const format = (value: number) => value.toLocaleString(locale.value);

const hpPercent = (boss: GuildBossInfo) => (boss.maxHp ? (boss.currentHp / boss.maxHp) * 100 : 0);

const isDefeated = (boss: GuildBossInfo) => boss.defeatedAt !== null;

// Откат мог закончиться, пока окно открыто
const cooldownLeft = (boss: GuildBossInfo) => Math.max(0, msUntil(boss.nextAttackAt));

const chestsText = (reward: RewardInfo) => reward.chests
    .map(chest => t('chests.name', { type: t(`chests.types.${chest.type}`), level: chest.level }))
    .join(', ');

const topDamage = (boss: GuildBossInfo) => boss.damage.slice(0, TOP_DAMAGE);

// Аура портрета под стать боссу: орк-громила — мороз, вождь с аурой жара — угли, маг — руны.
// У поверженного босса ауры нет
const AURAS: Record<GuildBossTier, FrameEffect> = {
    Easy: 'snow',
    Medium: 'embers',
    Hard: 'runes',
};

const auraOf = (boss: GuildBossInfo): FrameEffect => (isDefeated(boss) ? 'none' : AURAS[boss.tier]);

// Состояние карточки для свечения: можно бить — рамка горит ярче, можно забрать награду — светится золотом
const cardState = (boss: GuildBossInfo) => {
    if (isDefeated(boss)) return boss.rewardClaimed ? 'boss-card-done' : 'boss-card-claimable';
    return cooldownLeft(boss) > 0 ? 'boss-card-waiting' : 'boss-card-ready';
};

/* ===== Действия ===== */
const attack = (boss: GuildBossInfo) => {
    if (props.busy || noSquad.value || isDefeated(boss) || cooldownLeft(boss) > 0) return;
    emit('navigate');
    // attack — разовая метка: повторный удар по тому же боссу, не уходя с арены, тоже запускает бой
    router.push({ name: 'arena', query: { guildBoss: boss.tier, attack: Date.now() } });
};

const claim = async (boss: GuildBossInfo) => {
    await props.run(`claim-${boss.tier}`, async () => {
        const result = await GuildApiService.claimReward(boss.tier);
        setResources(result.resources);
        const parts = [`🪙 ${format(result.reward.gold)}`, `✦ ${format(result.reward.experience)}`];
        const chests = chestsText(result.reward);
        if (chests) parts.push(`🎁 ${chests}`);
        return t('guild.bosses.claimed', { reward: parts.join(' · ') });
    });
    await load();
};
</script>

<template>
    <div class="guild-page">
        <div class="guild-bosses-heading">
            <span class="guild-hint">{{ t('guild.bosses.hint', { minutes: overview?.attackCooldownMinutes ?? 60 }) }}</span>
            <span v-if="overview" class="guild-bosses-reset">
                ⏳ {{ t('guild.bosses.resetIn', { time: formatTime(msUntil(overview.resetAt)) }) }}
            </span>
        </div>

        <!-- ===== Отряд, которым бьём боссов ===== -->
        <section v-if="overview" class="boss-squad">
            <div class="boss-squad-heading">
                <span class="boss-squad-title">{{ t('guild.bosses.squad') }}</span>
                <span v-if="noSquad" class="boss-squad-empty">{{ t('guild.bosses.noSquad') }}</span>
                <span v-else class="boss-squad-power">⚔ {{ t('power.squad', { power: squadPower }) }}</span>
                <GemButton :color="'pink'" :disabled="!!busy" @click="squadOpen = true">{{ t('guild.bosses.editSquad') }}</GemButton>
            </div>
            <!-- Тот же ряд слотов, что в окне отряда; клик по слоту открывает окно отряда -->
            <SquadRow :heroes="squad" readonly @slot-click="squadOpen = true" />
        </section>

        <div v-if="loading && !overview" class="guild-hint">{{ t('common.loading') }}</div>
        <div v-else-if="loadError && !overview" class="guild-hint guild-error">{{ t('guild.bosses.loadError') }}</div>

        <div v-else class="boss-grid">
            <article
                v-for="boss in bosses"
                :key="boss.tier"
                class="boss-card"
                :class="[`boss-card-${boss.tier}`, cardState(boss), { 'boss-card-defeated': isDefeated(boss) }]"
            >
                <!-- Фон — арена, на которой будет бой с этим боссом -->
                <div class="boss-card-arena" aria-hidden="true">
                    <div class="boss-card-arena-image" :style="{ backgroundImage: `url(${arenaBackground(boss.background)})` }"></div>
                </div>
                <div class="boss-card-top">
                    <span class="boss-tier">{{ t(`guild.bosses.tiers.${boss.tier}`) }}</span>
                    <CssAuraEffect class="boss-aura" :effect="auraOf(boss)" :size="72">
                        <UnitPortrait :unit="boss.unit" :size="72" :show-level="false" />
                    </CssAuraEffect>
                    <span class="boss-name">{{ boss.unit.name }}</span>
                    <span class="boss-level">{{ t('guild.bosses.level', { level: boss.level }) }}</span>
                </div>

                <div class="boss-hp" :title="`${format(boss.currentHp)} / ${format(boss.maxHp)}`">
                    <div class="boss-hp-fill" :style="{ width: `${hpPercent(boss)}%` }">
                        <span class="boss-hp-shine"></span>
                    </div>
                    <span class="boss-hp-text">{{ format(boss.currentHp) }} / {{ format(boss.maxHp) }}</span>
                </div>

                <div class="boss-reward">
                    <span class="boss-label">{{ t('guild.bosses.reward') }}</span>
                    <span>🪙 {{ format(boss.reward.gold) }} · ✦ {{ format(boss.reward.experience) }}</span>
                    <span v-if="boss.reward.chests.length">🎁 {{ chestsText(boss.reward) }}</span>
                </div>

                <div class="boss-damage">
                    <span class="boss-label">{{ t('guild.bosses.myDamage', { damage: format(boss.myDamage) }) }}</span>
                    <ol v-if="boss.damage.length" class="boss-damage-list">
                        <li v-for="entry in topDamage(boss)" :key="entry.userId">
                            <span class="boss-damage-name">{{ entry.username }}</span>
                            <span class="boss-damage-value">{{ format(entry.damage) }}</span>
                        </li>
                    </ol>
                    <span v-else class="boss-muted">{{ t('guild.bosses.noDamage') }}</span>
                </div>

                <div class="boss-actions">
                    <template v-if="isDefeated(boss)">
                        <span class="boss-status boss-status-defeated">
                            {{ boss.killedBy ? t('guild.bosses.killedBy', { name: boss.killedBy }) : t('guild.bosses.defeated') }}
                        </span>
                        <span v-if="boss.rewardClaimed" class="boss-status">{{ t('guild.bosses.rewardClaimed') }}</span>
                        <GemButton
                            v-else
                            :color="'yellow'"
                            :disabled="!!busy"
                            @click="claim(boss)"
                        >{{ busy === `claim-${boss.tier}` ? t('guild.bosses.claiming') : t('guild.bosses.claim') }}</GemButton>
                    </template>
                    <template v-else>
                        <span v-if="cooldownLeft(boss) > 0" class="boss-status">
                            {{ t('guild.bosses.nextAttack', { time: formatTime(cooldownLeft(boss)) }) }}
                        </span>
                        <GemButton
                            v-else
                            :color="'red'"
                            :disabled="!!busy || noSquad"
                            :title="noSquad ? t('guild.bosses.noSquad') : undefined"
                            @click="attack(boss)"
                        >⚔ {{ t('guild.bosses.attack') }}</GemButton>
                    </template>
                </div>
            </article>
        </div>
    </div>
</template>

<style scoped src="./guild.css"></style>

<style scoped>
.guild-bosses-heading {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
    column-gap: 12px;
}

.guild-bosses-reset {
    flex-shrink: 0;
    padding: 2px 8px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.65);
    color: #fde68a;
    font-size: 12px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
}

.guild-error {
    color: #ffb4b4;
}

/* Отряд над боссами: заголовок с мощью и кнопкой, под ним слоты как в окне отряда */
.boss-squad {
    margin: 4px 0 6px;
    padding: 8px;
}

.boss-squad-heading {
    display: flex;
    align-items: center;
    column-gap: 14px;
    /* Под номерами слотов, которые выступают над рядом */
    margin-bottom: 18px;
}

.boss-squad-title {
    color: #f8fafc;
    font-size: 14px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.boss-squad-power {
    color: #ffd166;
    font-size: 13px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.boss-squad-empty {
    color: #ffb4b4;
    font-size: 13px;
    font-weight: bold;
}

.boss-squad-heading > .gem-button {
    flex-shrink: 0;
    margin-left: auto;
}

.boss-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(230px, 1fr));
    gap: 22px;
    /* Место под ореол карточек: тело модалки обрезает всё, что вылезает за край */
    padding: 14px;
}

/* Карточка — как у экспедиций; цвет рамки и свечения — уровень босса.
   Свечение в два слоя: ::before — блик, бегущий по рамке, ::after — пульсирующий ореол вокруг карточки */
.boss-card {
    --tier-color: var(--green-gem-color);
    /* Скорость блика и пульса: живой босс, по которому можно бить, горит ярче и быстрее */
    --spin-duration: 4s;
    --pulse-duration: 2.4s;
    --glow-strength: 1;
    position: relative;
    isolation: isolate;
    display: flex;
    flex-direction: column;
    row-gap: 10px;
    padding: 12px;
    border-radius: 12px;
    border: 2px solid color-mix(in srgb, var(--tier-color) 70%, transparent);
    background: rgb(15, 23, 32);
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5);
    transition: transform 0.15s ease, box-shadow 0.15s ease, border-color 0.15s ease;
}

/* Наведение — как у карточек экспедиций: карточка приподнимается, рамка и ореол разгораются */
.boss-card:hover {
    --glow-strength: 1.6;
    --spin-duration: 1.6s;
    transform: translateY(-4px);
    border-color: var(--tier-color);
    box-shadow: 0 8px 18px rgba(0, 0, 0, 0.65);
}

.boss-card:hover .boss-aura {
    transform: scale(1.08);
}

/* Арена под содержимым карточки (внутри изолированной карточки -1 — уровень ореола ::after, поэтому -2).
   Обёртка обрезает картинку по скруглению, сама картинка приближается при наведении */
.boss-card-arena {
    position: absolute;
    inset: 0;
    z-index: -2;
    overflow: hidden;
    border-radius: 10px;
}

.boss-card-arena-image {
    position: absolute;
    inset: 0;
    background-size: cover;
    background-position: center 60%;
    transition: transform 0.4s ease, filter 0.3s ease;
}

/* Сверху светлее — арену видно за портретом, книзу темнее — под текстом; плюс подсветка цветом уровня */
.boss-card-arena::after {
    content: '';
    position: absolute;
    inset: 0;
    background:
        radial-gradient(ellipse 80% 45% at 50% 0%, color-mix(in srgb, var(--tier-color) 30%, transparent), transparent),
        linear-gradient(to bottom, rgba(15, 23, 32, 0.35) 0%, rgba(15, 23, 32, 0.75) 45%, rgba(15, 23, 32, 0.92) 100%);
}

.boss-card:hover .boss-card-arena-image {
    transform: scale(1.08);
}

.boss-card-defeated .boss-card-arena-image {
    filter: grayscale(0.85) brightness(0.7);
}

/* Поверженный босс с забранной наградой только приподнимается */
.boss-card-done:hover {
    border-color: rgba(157, 173, 175, 0.9);
}

.boss-card-Medium {
    --tier-color: var(--yellow-gem-color);
}

.boss-card-Hard {
    --tier-color: var(--red-gem-color);
}

/* Угол блика анимируется как число — для этого его нужно объявить */
@property --boss-angle {
    syntax: '<angle>';
    initial-value: 0deg;
    inherits: false;
}

.boss-card::before {
    content: '';
    position: absolute;
    inset: -2px;
    z-index: 1;
    padding: 2px;
    border-radius: inherit;
    pointer-events: none;
    background: conic-gradient(
        from var(--boss-angle),
        transparent 0deg,
        transparent 250deg,
        var(--tier-color) 300deg,
        #fff 330deg,
        var(--tier-color) 345deg,
        transparent 360deg
    );
    /* Остаётся только кольцо толщиной в рамку */
    -webkit-mask: linear-gradient(#000 0 0) content-box, linear-gradient(#000 0 0);
    -webkit-mask-composite: xor;
    mask: linear-gradient(#000 0 0) content-box, linear-gradient(#000 0 0);
    mask-composite: exclude;
    opacity: var(--glow-strength);
    animation: boss-border-spin var(--spin-duration) linear infinite;
}

.boss-card::after {
    content: '';
    position: absolute;
    inset: 0;
    z-index: -1;
    border-radius: inherit;
    pointer-events: none;
    box-shadow:
        0 0 14px 2px var(--tier-color),
        0 0 28px 6px color-mix(in srgb, var(--tier-color) 45%, transparent);
    opacity: calc(0.55 * var(--glow-strength));
    animation: boss-glow-pulse var(--pulse-duration) ease-in-out infinite;
}

.boss-card-ready {
    --spin-duration: 3s;
    --pulse-duration: 1.8s;
}

/* Удар на откате — свечение приглушено и медленнее */
.boss-card-waiting {
    --spin-duration: 8s;
    --pulse-duration: 4s;
    --glow-strength: 0.45;
}

/* Босс повержен, награда ждёт — карточка светится золотом */
.boss-card-claimable {
    --tier-color: #ffd166;
    --spin-duration: 2.2s;
    --pulse-duration: 1.4s;
}

.boss-card-defeated {
    border-color: rgba(157, 173, 175, 0.6);
}

/* Награда забрана — карточка гаснет */
.boss-card-done::before,
.boss-card-done::after {
    display: none;
}

@keyframes boss-border-spin {
    to { --boss-angle: 360deg; }
}

@keyframes boss-glow-pulse {
    0%, 100% { opacity: calc(0.35 * var(--glow-strength)); }
    50% { opacity: calc(0.85 * var(--glow-strength)); }
}

/* Портрет с аурой — по центру над именем */
.boss-aura {
    margin: 6px 0 4px;
    transition: transform 0.2s ease;
}

.boss-card-defeated .boss-card-top {
    filter: grayscale(0.8) brightness(0.8);
}

.boss-card-top {
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 4px;
}

.boss-tier {
    padding: 2px 10px;
    border-radius: 8px;
    background: var(--tier-color);
    color: #fff;
    font-size: 12px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.boss-name {
    color: #f8fafc;
    font-size: 16px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-align: center;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.boss-level {
    color: #cbd5e1;
    font-size: 12px;
}

.boss-hp {
    position: relative;
    height: 20px;
    overflow: hidden;
    border-radius: 10px;
    background: rgba(0, 0, 0, 0.6);
    box-shadow: inset 0 0 4px #000, 0 0 0 2px rgba(157, 173, 175, 0.6);
}

.boss-hp-fill {
    position: relative;
    overflow: hidden;
    height: 100%;
    background: linear-gradient(to bottom, #f87171, #b91c1c);
    box-shadow: 0 0 8px 1px rgba(248, 113, 113, 0.7);
    transition: width 0.4s ease;
}

/* Блик, пробегающий по полоске HP */
.boss-hp-shine {
    position: absolute;
    inset: 0;
    background: linear-gradient(110deg, transparent 30%, rgba(255, 255, 255, 0.45) 50%, transparent 70%);
    transform: translateX(-100%);
    animation: boss-hp-shine 2.6s ease-in-out infinite;
}

@keyframes boss-hp-shine {
    0% { transform: translateX(-100%); }
    60%, 100% { transform: translateX(100%); }
}

@media (prefers-reduced-motion: reduce) {
    .boss-card::before,
    .boss-card::after,
    .boss-hp-shine {
        animation: none !important;
    }

    .boss-card:hover,
    .boss-card:hover .boss-aura,
    .boss-card:hover .boss-card-arena-image {
        transform: none;
    }
}

.boss-hp-text {
    position: absolute;
    inset: 0;
    color: #fff;
    font-size: 12px;
    font-weight: bold;
    line-height: 20px;
    text-align: center;
    font-variant-numeric: tabular-nums;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.9);
}

.boss-reward,
.boss-damage {
    display: flex;
    flex-direction: column;
    row-gap: 2px;
    color: #ffd166;
    font-size: 13px;
    font-weight: bold;
}

.boss-label {
    color: #e2e8f0;
    font-size: 12px;
    text-transform: uppercase;
}

.boss-damage-list {
    margin: 0;
    padding-left: 20px;
    color: #cbd5e1;
    font-weight: normal;
}

.boss-damage-list li::marker {
    color: #ffd166;
}

.boss-damage-list li {
    display: list-item;
}

.boss-damage-name {
    display: inline-block;
    max-width: 60%;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    vertical-align: bottom;
}

.boss-damage-value {
    float: right;
    color: #ffd166;
    font-variant-numeric: tabular-nums;
}

.boss-muted {
    color: #94a3b8;
    font-weight: normal;
}

.boss-actions {
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 6px;
    margin-top: auto;
    min-height: 46px;
    justify-content: center;
}

.boss-status {
    color: #e2e8f0;
    font-size: 13px;
    font-weight: bold;
    text-align: center;
    font-variant-numeric: tabular-nums;
}

.boss-status-defeated {
    color: var(--green-gem-color);
    text-transform: uppercase;
}
</style>
