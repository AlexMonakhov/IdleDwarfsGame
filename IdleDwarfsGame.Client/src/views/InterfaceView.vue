<script lang="ts" setup>
import GemButton from '@/components/common/GemButton.vue';
import NavButton from '@/components/interface/NavButton.vue';
import ExpeditionsModal from '@/components/expeditions/ExpeditionsModal.vue';
import MiningModal from '@/components/mining/MiningModal.vue';
import FriendsModal from '@/components/friends/FriendsModal.vue';
import GuildModal from '@/components/guild/GuildModal.vue';
import PvpModal from '@/components/pvp/PvpModal.vue';
import InventoryModal from '@/components/inventory/InventoryModal.vue';
import CollectionModal from '@/components/collection/CollectionModal.vue';
import HeroUpgradeModal from '@/components/upgrade/HeroUpgradeModal.vue';
import SquadModal from '@/components/inventory/SquadModal.vue';
import ShopModal from '@/components/shop/ShopModal.vue';
import BlacksmithModal from '@/components/forge/BlacksmithModal.vue';
import QuestsModal from '@/components/quests/QuestsModal.vue';
import EventModal from '@/components/events/EventModal.vue';
import DelveModal from '@/components/delve/DelveModal.vue';
import MailModal from '@/components/mail/MailModal.vue';
import RewardsModal from '@/components/rewards/RewardsModal.vue';
import { useRewards } from '@/composables/UseRewards';
import { useMail } from '@/composables/UseMail';
import { useSocial } from '@/composables/UseSocial';
import AccountButton from '@/components/account/AccountButton.vue';
import AccountModal from '@/components/account/AccountModal.vue';
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch, watchEffect, type Component } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import backgroundMusic from "@/assets/sounds/Beneath_The_Marble_Dome.mp3";
import { useAuth } from '@/composables/UseAuth';
import { useResources } from '@/composables/UseResources';
import { useHeroes } from '@/composables/UseHeroes';
import { usePresence } from '@/composables/UsePresence';
import { useGuild } from '@/composables/UseGuild';
import { usePvp } from '@/composables/UsePvp';
import { useSquadModal } from '@/composables/UseSquadModal';
import { useInterface } from '@/composables/UseInterface';
import { useQuests } from '@/composables/UseQuests';
import { soundService } from '@/services/soundService/SoundService';
import { useI18n } from 'vue-i18n';
import { IconBook, IconCoin, IconDoor, IconDwarf, IconExperience, IconFriends, IconHome, IconMail, IconMore, IconPickaxe, IconRainbowCoin, IconScroll, IconStar, IconTown, IconTrophy } from '@/components/icons/ui';
import ConfirmModal from '@/components/common/ConfirmModal.vue';
import TutorialOverlay from '@/components/tutorial/TutorialOverlay.vue';
import { useTutorial } from '@/composables/UseTutorial';
import { activityByNav } from '@/tutorial/activities';
import { topModalZ } from '@/composables/UseModalStack';
import { loadingScreenVisible } from '@/composables/UseLoadingScreen';
import { hudLayout, hudScale } from '@/composables/UseUiScale';

const { t, locale } = useI18n();
const router = useRouter();
const route = useRoute();
const onArena = computed(() => route.name === 'arena');
// «!» на кнопке раздела, с которым игрок ещё не знаком (обучение)
const tutorial = useTutorial();
const badge = (nav: string) => tutorial.needsBadge(activityByNav(nav)) ? '!' : undefined;
// Пропуск стартового обучения: сервер выдал дворфов, золото и шахту сразу — счётчики перечитываем
tutorial.onOnboardingFinished(() => {
    refreshResources();
    clearHeroes();
});
// Выход из аккаунта — только после подтверждения
const logoutConfirmOpen = ref(false);
const { isAuthenticated, logout } = useAuth();
const { resources, refresh: refreshResources, clear: clearResources } = useResources();
const { clear: clearHeroes } = useHeroes();
const { incomingFriendRequests, start: startPresence, stop: stopPresence } = usePresence();

// Окна друзей и спуска — общие (useSocial): после боя арена возвращает в них
const { friendsOpen, delveOpen } = useSocial();
const { guildOpen } = useGuild();
const { pvpOpen } = usePvp();
const { squadOpen } = useSquadModal();
// Экспедиции, склад (инвентарь), алтарь (прокачка) и шахты открываются кликом по зданиям города
const { interfaceHidden, expeditionsOpen, inventoryOpen, upgradeOpen, miningOpen, shopOpen, forgeOpen, questsOpen, eventOpen, mailOpen, collectionOpen, rewardsOpen, rewardsTab } = useInterface();
const { claimable: questsClaimable, start: startQuests, clear: clearQuests } = useQuests();
const accountOpen = ref(false);
// Почта: значок — непрочитанные письма и незабранные вложения
const { badge: mailBadge, refresh: refreshMail, clear: clearMail } = useMail();
// Награды: значок — сегодняшняя награда календаря и готовые ступени достижений
const { claimable: rewardsClaimable, calendar: loginCalendar, start: startRewards, clear: clearRewards } = useRewards();

// Ресурсы аккаунта подтягиваем при входе и сбрасываем при выходе; там же открываем и закрываем сессию вкладки
// и следим за доской заданий (значок готовых наград на кнопке)
watch(isAuthenticated, authenticated => {
    if (authenticated) {
        refreshResources();
        startPresence();
        startQuests();
        refreshMail();
        startRewards();
    } else {
        clearResources();
        clearHeroes();
        stopPresence();
        clearQuests();
        clearMail();
        clearRewards();
    }
}, { immediate: true });

// Первый вход за сутки: сегодняшняя награда календаря не забрана — открываем календарь сам, один раз за сутки
// на вкладку. Не во время стартового обучения и не поверх уже открытого окна или арены
const AUTO_OPEN_KEY = 'idle-dwarfs-calendar-shown'
const calendarShownFor = (): string | null => {
    try { return sessionStorage.getItem(AUTO_OPEN_KEY); } catch { return null; }
};
watch([loginCalendar, () => tutorial.onboardingActive.value || !!tutorial.active.value, topModalZ, onArena, loadingScreenVisible], ([calendar, tutorialBusy, modalZ, arena, loadingScreen]) => {
    if (!calendar || calendar.claimedToday || tutorialBusy || modalZ > 0 || arena || loadingScreen) return;
    const day = calendar.resetAt;
    if (calendarShownFor() === day) return;
    try { sessionStorage.setItem(AUTO_OPEN_KEY, day); } catch { /* покажем ещё раз после перезагрузки */ }
    rewardsTab.value = 'calendar';
    rewardsOpen.value = true;
});

const onLogout = () => {
    expeditionsOpen.value = false;
    friendsOpen.value = false;
    guildOpen.value = false;
    pvpOpen.value = false;
    inventoryOpen.value = false;
    collectionOpen.value = false;
    upgradeOpen.value = false;
    miningOpen.value = false;
    shopOpen.value = false;
    forgeOpen.value = false;
    questsOpen.value = false;
    eventOpen.value = false;
    delveOpen.value = false;
    squadOpen.value = false;
    accountOpen.value = false;
    mailOpen.value = false;
    rewardsOpen.value = false;
    // Пока токен ещё есть — им закрывается сессия
    stopPresence();
    logout();
    router.push('/login');
};

// ===== Меню =====
// Раскладка — по размеру окна (UseUiScale): ряд кнопок сверху на компьютере, столбец слева на телефоне
const compact = computed(() => hudLayout.value === 'landscape');
// Под полноэкранной загрузкой (город, арена) меню не показываем: оно теперь вне сцены и легло бы поверх
const hudVisible = computed(() => !interfaceHidden.value && !loadingScreenVisible.value);

interface NavItem {
    key: string;
    label: string;
    icon: Component;
    color: 'pink' | 'green' | 'blue' | 'yellow' | 'red';
    action: () => void;
    badgeCount?: number;
    badgeText?: string;
    // Якорь обучения (v-tutorial)
    anchor?: string;
}

const navItems = computed<NavItem[]>(() => {
    const items: NavItem[] = [
        { key: 'home', label: t('nav.home'), icon: IconHome, color: 'blue', action: () => router.push('/') },
        { key: 'town', label: t('nav.town'), icon: IconTown, color: 'blue', anchor: 'nav.town', action: () => router.push({ name: 'town' }) },
    ];
    // На арене меню урезано до «Главная» и «Город»; кнопки результата боя — в самой арене (ArenaView)
    if (onArena.value) return items;
    if (!isAuthenticated.value) {
        if (route.name != 'login') items.push({ key: 'login', label: t('nav.login'), icon: IconDoor, color: 'green', action: () => router.push('/login') });
        if (route.name != 'register') items.push({ key: 'register', label: t('nav.register'), icon: IconDwarf, color: 'yellow', action: () => router.push('/register') });
        return items;
    }
    items.push(
        { key: 'collection', label: t('nav.collection'), icon: IconBook, color: 'yellow', anchor: 'nav.collection', badgeText: badge('collection'), action: () => (collectionOpen.value = true) },
        { key: 'friends', label: t('nav.friends'), icon: IconFriends, color: 'green', anchor: 'nav.friends', badgeText: badge('friends'), badgeCount: incomingFriendRequests.value, action: () => (friendsOpen.value = true) },
        { key: 'quests', label: t('nav.quests'), icon: IconScroll, color: 'yellow', anchor: 'nav.quests', badgeText: badge('quests'), badgeCount: questsClaimable.value, action: () => (questsOpen.value = true) },
        { key: 'rewards', label: t('nav.rewards'), icon: IconTrophy, color: 'green', badgeCount: rewardsClaimable.value, action: () => (rewardsOpen.value = true) },
        { key: 'mail', label: t('nav.mail'), icon: IconMail, color: 'blue', anchor: 'nav.mail', badgeText: badge('mail'), badgeCount: mailBadge.value, action: () => (mailOpen.value = true) },
        { key: 'event', label: t('nav.event'), icon: IconStar, color: 'red', anchor: 'nav.event', badgeText: badge('event'), action: () => (eventOpen.value = true) },
        { key: 'delve', label: t('social.delve.nav'), icon: IconPickaxe, color: 'pink', anchor: 'nav.delve', badgeText: badge('delve'), action: () => (delveOpen.value = true) },
    );
    return items;
});

// Компьютер: текстовые кнопки-кристаллы, пока весь ряд влезает в одну строку, иначе — круглые с иконками.
// Ширину ряда меряет невидимая копия текстовых кнопок: она же следит за сменой языка и значками входа/выхода
const navMenuRef = ref<HTMLElement | null>(null);
const navMeasureRef = ref<HTMLElement | null>(null);
const textNavFits = ref(true);
const measureNav = () => {
    const menu = navMenuRef.value;
    const row = navMeasureRef.value;
    if (!menu || !row) return;
    const style = getComputedStyle(menu);
    const available = menu.clientWidth - parseFloat(style.paddingLeft) - parseFloat(style.paddingRight);
    textNavFits.value = row.scrollWidth <= available;
};
let navObserver: ResizeObserver | undefined;
watch([navMenuRef, navMeasureRef], ([menu, row]) => {
    navObserver?.disconnect();
    if (!menu || !row) return;
    navObserver = new ResizeObserver(measureNav);
    navObserver.observe(menu);
    navObserver.observe(row);
    measureNav();
});
onBeforeUnmount(() => navObserver?.disconnect());

// Телефон: «Главной» нет (там нечего делать с телефона), в столбце помещается пять кнопок.
// Если разделов больше — четыре главных и «Ещё» с остальными
const BAR_SLOTS = 5;
const PRIMARY_KEYS = ['town', 'quests', 'mail', 'event'];
const compactItems = computed(() => navItems.value.filter(item => item.key !== 'home'));
const barItems = computed(() => compactItems.value.length <= BAR_SLOTS
    ? compactItems.value
    : compactItems.value.filter(item => PRIMARY_KEYS.includes(item.key)));
const moreItems = computed(() => compactItems.value.filter(item => !barItems.value.includes(item)));
const moreOpen = ref(false);
// На «Ещё» — сумма значков спрятанных разделов, а «!» обучения — если ни у одного нет числа
const moreBadgeCount = computed(() => moreItems.value.reduce((sum, item) => sum + (item.badgeCount ?? 0), 0));
const moreBadgeText = computed(() => moreItems.value.some(item => item.badgeText) ? '!' : undefined);
// Пока «Ещё» закрыто, якоря обучения спрятанных разделов указывают на саму кнопку «Ещё»
const moreAnchors = computed(() => moreOpen.value ? [] : moreItems.value.map(item => item.anchor).filter((id): id is string => !!id));

const onNav = (item: NavItem) => {
    moreOpen.value = false;
    item.action();
};
watch([() => route.fullPath, hudLayout], () => (moreOpen.value = false));

// Ресурсы видны и при открытом окне: окна телепортируются в body над всей сценой, поэтому табличка на это время
// уезжает туда же — в слой чуть выше верхнего окна, ровно на своё место. В шапке остаётся пустое место её размера.
// Под полноэкранной загрузкой (город, арена) не поднимаем — она закрывает и шапку
const resourcesSlot = ref<HTMLElement | null>(null);
// На телефоне не поднимаем: окно там почти во весь экран, и табличка легла бы на его вкладки и крестик
const resourcesRaised = computed(() => !compact.value && topModalZ.value > 0 && !interfaceHidden.value && !loadingScreenVisible.value);
const slotBox = ref({ left: 0, top: 0 });
// Размер таблички запоминаем, пока она стоит в шапке и видна: в момент подъёма меню может быть ещё скрыто
// (окно открылось под полноэкранной загрузкой — возврат с арены в окно друзей), и замер дал бы ноль
const slotSize = ref({ width: 0, height: 0 });
let slotObserver: ResizeObserver | undefined;
watch(resourcesSlot, el => {
    slotObserver?.disconnect();
    if (!el) return;
    slotObserver = new ResizeObserver(() => {
        if (resourcesRaised.value || !el.offsetWidth) return;
        slotSize.value = { width: el.offsetWidth, height: el.offsetHeight };
    });
    slotObserver.observe(el);
});
onBeforeUnmount(() => slotObserver?.disconnect());
const measureSlot = () => {
    const el = resourcesSlot.value;
    if (!el) return;
    if (!resourcesRaised.value && el.offsetWidth) slotSize.value = { width: el.offsetWidth, height: el.offsetHeight };
    const rect = el.getBoundingClientRect();
    // Меню ещё скрыто (display: none) — положения нет; перемеряем после отрисовки, когда оно покажется
    if (!rect.width && !rect.height) {
        nextTick(() => {
            const shown = resourcesSlot.value?.getBoundingClientRect();
            if (shown && (shown.width || shown.height)) slotBox.value = { left: shown.left, top: shown.top };
        });
        return;
    }
    // Размер — в логических пикселях (offset* не видят масштаб), положение — в пикселях окна
    slotBox.value = { left: rect.left, top: rect.top };
};
// flush: 'pre' — замер до перерисовки, пока табличка ещё стоит в шапке и задаёт размер места
watch([resourcesRaised, hudScale], ([raised]) => { if (raised) measureSlot(); }, { flush: 'pre' });
const onResize = () => { if (resourcesRaised.value) measureSlot(); };
onMounted(() => window.addEventListener('resize', onResize));
onBeforeUnmount(() => window.removeEventListener('resize', onResize));
const slotStyle = computed(() => resourcesRaised.value
    ? { width: `${slotSize.value.width}px`, height: `${slotSize.value.height}px` }
    : undefined);
const resourcesLayerStyle = computed(() => resourcesRaised.value
    ? { left: `${slotBox.value.left}px`, top: `${slotBox.value.top}px`, zIndex: topModalZ.value + 5 }
    : undefined);

// Фоновая музыка: громкость и mute — из настроек звука
const audioRef = ref<HTMLAudioElement | null>(null);

watchEffect(() => {
    const audio = audioRef.value;
    if (!audio) return;
    audio.volume = soundService.musicGain;
    // Браузер мог не дать автозапуск до первого действия — включение музыки в настройках и есть такое действие
    if (soundService.musicGain > 0 && audio.paused) audio.play().catch(() => undefined);
});
</script>

<template>
    <audio ref="audioRef" :src="backgroundMusic" autoplay loop :muted="soundService.settings.musicMuted"></audio>
    <div v-show="hudVisible" class="interface-view" :class="`hud-${hudLayout}`">
        <!-- ===== Компьютер: ряд кнопок слева ===== -->
        <template v-if="!compact">
            <div ref="navMenuRef" class="nav-menu" :class="{ 'nav-menu-text': textNavFits }" role="navigation">
                <!-- Пока текстовые кнопки влезают в один ряд — они; иначе круглые с иконками (textNavFits) -->
                <template v-if="textNavFits">
                    <GemButton
                        v-for="item in navItems"
                        :key="item.key"
                        v-tutorial="item.anchor"
                        :color="item.color"
                        :badge-count="item.badgeCount"
                        :badge-text="item.badgeText"
                        @click="onNav(item)"
                    >{{ item.label }}</GemButton>
                </template>
                <template v-else>
                <NavButton
                    v-for="item in navItems"
                    :key="item.key"
                    v-tutorial="item.anchor"
                    :label="item.label"
                    :color="item.color"
                    :badge-count="item.badgeCount"
                    :badge-text="item.badgeText"
                    @click="onNav(item)"
                ><component :is="item.icon" /></NavButton>
                </template>
                <!-- Невидимый замер: ширина ряда текстовых кнопок, без якорей обучения и значков -->
                <div ref="navMeasureRef" class="nav-measure" aria-hidden="true">
                    <GemButton v-for="item in navItems" :key="item.key" tabindex="-1" :color="item.color">{{ item.label }}</GemButton>
                </div>
            </div>
        </template>

        <!-- ===== Телефон (горизонтально): столбец кнопок слева поверх сцены ===== -->
        <template v-else>
            <div class="hud-bar" role="navigation">
                <NavButton
                    v-for="item in barItems"
                    :key="item.key"
                    v-tutorial="item.anchor"
                    :label="item.label"
                    :color="item.color"
                    :badge-count="item.badgeCount"
                    :badge-text="item.badgeText"
                    @click="onNav(item)"
                ><component :is="item.icon" /></NavButton>
                <div v-if="moreItems.length" class="hud-more-slot">
                    <NavButton
                        :label="t('nav.more')"
                        color="green"
                        :active="moreOpen"
                        :badge-count="moreBadgeCount"
                        :badge-text="moreBadgeText"
                        @click="moreOpen = !moreOpen"
                    ><IconMore /></NavButton>
                    <span v-for="id in moreAnchors" :key="id" v-tutorial="id" class="hud-anchor-proxy"></span>
                </div>
            </div>
            <template v-if="moreOpen">
                <!-- Нажатие мимо закрывает «Ещё» -->
                <div class="hud-more-backdrop" @click="moreOpen = false"></div>
                <div class="hud-more" :style="{ '--more-count': moreItems.length }">
                    <NavButton
                        v-for="item in moreItems"
                        :key="item.key"
                        v-tutorial="item.anchor"
                        :label="item.label"
                        :color="item.color"
                        :badge-count="item.badgeCount"
                        :badge-text="item.badgeText"
                        @click="onNav(item)"
                    ><component :is="item.icon" /></NavButton>
                </div>
            </template>
        </template>

        <!-- Ресурсы и аватар — справа сверху в обеих раскладках; на арене не нужны и только занимают место -->
        <div v-if="!onArena" class="settings">
            <div v-if="isAuthenticated && resources" ref="resourcesSlot" class="resources-slot" :style="slotStyle">
                <Teleport to="body" :disabled="!resourcesRaised">
                    <div :class="{ 'resources-layer': resourcesRaised }" :style="resourcesLayerStyle">
                        <div v-tutorial="'resources'" class="resources" :class="{ 'resources-compact': compact }">
                            <span class="resource resource-gold" v-tooltip="t('resources.gold')"><IconCoin /> {{ resources.gold.toLocaleString(locale) }}</span>
                            <span class="resource resource-experience" v-tooltip="t('resources.experience')"><IconExperience /> {{ resources.experience.toLocaleString(locale) }}</span>
                            <span v-if="resources.rainbowCoins !== undefined" class="resource resource-rainbow" v-tooltip="t('shop.currency.rainbowCoins')"><IconRainbowCoin /> {{ resources.rainbowCoins.toLocaleString(locale) }}</span>
                        </div>
                    </div>
                </Teleport>
            </div>
            <AccountButton v-if="isAuthenticated" :size="compact ? 56 : 124" @click="accountOpen = true" />
        </div>
    </div>
    <AccountModal v-if="isAuthenticated" v-model="accountOpen" @logout="logoutConfirmOpen = true" />
    <ExpeditionsModal v-if="isAuthenticated" v-model="expeditionsOpen" />
    <MiningModal v-if="isAuthenticated" v-model="miningOpen" />
    <FriendsModal v-if="isAuthenticated" v-model="friendsOpen" />
    <GuildModal v-if="isAuthenticated" v-model="guildOpen" />
    <PvpModal v-if="isAuthenticated" v-model="pvpOpen" />
    <InventoryModal v-if="isAuthenticated" v-model="inventoryOpen" />
    <CollectionModal v-if="isAuthenticated" v-model="collectionOpen" />
    <HeroUpgradeModal v-if="isAuthenticated" v-model="upgradeOpen" />
    <SquadModal v-if="isAuthenticated" v-model="squadOpen" />
    <ShopModal v-if="isAuthenticated" v-model="shopOpen" />
    <BlacksmithModal v-if="isAuthenticated" v-model="forgeOpen" />
    <QuestsModal v-if="isAuthenticated" v-model="questsOpen" />
    <MailModal v-if="isAuthenticated" v-model="mailOpen" />
    <RewardsModal v-if="isAuthenticated" v-model="rewardsOpen" />
    <EventModal v-if="isAuthenticated" v-model="eventOpen" />
    <DelveModal v-if="isAuthenticated" v-model="delveOpen" />
    <ConfirmModal
        v-if="isAuthenticated"
        v-model="logoutConfirmOpen"
        :message="t('nav.logoutConfirm')"
        @confirm="onLogout"
    />
    <!-- Шаг обучения — поверх всех окон -->
    <TutorialOverlay />
</template>

<style scoped>
/* Меню лежит вне .ui-stage (App.vue) и масштабируется само на --hud-scale (UseUiScale.ts):
   на компьютере — как сцена, но не мельче 75%, на телефоне — 1, то есть в настоящих пикселях */
.interface-view {
  position: fixed;
  top: 0;
  left: 0;
  width: calc(100vw / var(--hud-scale));
  height: calc(100vh / var(--hud-scale));
  transform: scale(var(--hud-scale));
  transform-origin: 0 0;
  /* Выше сцены (.ui-stage идёт следом в DOM), ниже окон */
  z-index: 1;
  pointer-events: none;
  display: flex;
}

/* ===== Компьютер ===== */
.nav-menu {
  position: relative;
  flex: 1;
  min-width: 0;
  display: flex;
  justify-content: flex-start;
  align-items: flex-start;
  flex-wrap: wrap;
  column-gap: 10px;
  row-gap: 8px;
  padding: 16px 20px;
  --nav-gem: 52px;
  --nav-caption: 12px;
}
/* Текстовые кнопки стоят теснее круглых — как было до иконок */
.nav-menu-text {
  column-gap: 5px;
}

/* Замер: та же строка текстовых кнопок, вне потока и невидимая */
.nav-measure {
  position: absolute;
  top: 0;
  left: 0;
  display: flex;
  column-gap: 5px;
  width: max-content;
  visibility: hidden;
  pointer-events: none;
}

.settings {
  display: flex;
  justify-content: flex-end;
  align-items: flex-start;
  padding: 20px;
}
.resources-slot {
  margin-right: 10px;
}

/* Табличка над открытыми окнами: в body, масштаб — как у меню */
.resources-layer {
  position: fixed;
  transform: scale(var(--hud-scale));
  transform-origin: 0 0;
}

/* Ресурсы аккаунта: табличка в стиле шапки модалки */
.resources {
  display: flex;
  align-items: center;
  column-gap: 14px;
  height: 40px;
  padding: 0 14px;
  box-sizing: border-box;
  border-radius: 10px;
  background: var(--wood-background);
  box-shadow: inset 0 0 6px black, 0 0 0 2px rgb(157, 173, 175), 0 0 0 4px #333;
  font-family: 'Arial', sans-serif;
  font-size: 15px;
  font-weight: bold;
  text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
  white-space: nowrap;
}

.resource-gold {
  color: #ffd166;
}

.resource-rainbow {
  color: #f5d0fe;
}

.resource-experience {
  color: #a5f3fc;
}

.nav-menu > *, .settings > *, .hud-bar > *, .hud-more, .hud-more-backdrop {
  pointer-events: auto;
}

/* ===== Телефон: всё поверх сцены, без подложек — как на компьютере ===== */
.hud-landscape .settings {
  margin-left: auto;
  padding: 8px 10px;
}
.hud-landscape .resources-slot {
  margin-right: 8px;
}
/* Класс на самой табличке: поднятая над окнами, она уезжает в body и выходит из .hud-landscape */
.resources-compact {
  height: 30px;
  column-gap: 12px;
  padding: 0 10px;
  font-size: 14px;
}

/* Столбец кнопок слева */
.hud-bar {
  position: absolute;
  top: 0;
  bottom: 0;
  left: 0;
  display: flex;
  flex-direction: column;
  justify-content: space-evenly;
  align-items: center;
  padding: 6px 8px;
  --nav-gem: 42px;
  --nav-caption: 10px;
}

/* Кнопка «Ещё» и невидимые якоря обучения поверх неё (разделы, спрятанные в «Ещё») */
.hud-more-slot {
  position: relative;
}
.hud-anchor-proxy {
  position: absolute;
  inset: 0;
  pointer-events: none;
}

.hud-more-backdrop {
  position: absolute;
  inset: 0;
}

/* Раскрытое «Ещё»: сетка рядом со столбцом */
.hud-more {
  position: absolute;
  left: 84px;
  top: 50%;
  transform: translateY(-50%);
  display: grid;
  /* Колонок — сколько разделов спрятано, но не больше четырёх в ряд */
  grid-template-columns: repeat(min(var(--more-count), 4), 84px);
  justify-items: center;
  row-gap: 14px;
  padding: 14px 8px;
  border-radius: 14px;
  background: var(--wood-background);
  box-shadow: inset 0 0 8px black, 0 0 0 2px rgb(157, 173, 175), 0 0 0 4px #333, 0 8px 20px rgba(0, 0, 0, 0.6);
  --nav-gem: 54px;
  --nav-caption: 11px;
}
</style>
