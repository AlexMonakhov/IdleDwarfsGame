<script lang="ts" setup>
import GemButton from '@/components/common/GemButton.vue';
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
import { useMail } from '@/composables/UseMail';
import { useSocial } from '@/composables/UseSocial';
import AccountButton from '@/components/account/AccountButton.vue';
import AccountModal from '@/components/account/AccountModal.vue';
import { computed, ref, watch, watchEffect } from 'vue';
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
import { IconCoin, IconExperience, IconRainbowCoin } from '@/components/icons/ui';
import ConfirmModal from '@/components/common/ConfirmModal.vue';
import TutorialOverlay from '@/components/tutorial/TutorialOverlay.vue';
import { useTutorial } from '@/composables/UseTutorial';
import { activityByNav } from '@/tutorial/activities';

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
const { interfaceHidden, expeditionsOpen, inventoryOpen, upgradeOpen, miningOpen, shopOpen, forgeOpen, questsOpen, eventOpen, mailOpen, collectionOpen } = useInterface();
const { claimable: questsClaimable, start: startQuests, clear: clearQuests } = useQuests();
const accountOpen = ref(false);
// Почта: значок — непрочитанные письма и незабранные вложения
const { badge: mailBadge, refresh: refreshMail, clear: clearMail } = useMail();

// Ресурсы аккаунта подтягиваем при входе и сбрасываем при выходе; там же открываем и закрываем сессию вкладки
// и следим за доской заданий (значок готовых наград на кнопке)
watch(isAuthenticated, authenticated => {
    if (authenticated) {
        refreshResources();
        startPresence();
        startQuests();
        refreshMail();
    } else {
        clearResources();
        clearHeroes();
        stopPresence();
        clearQuests();
        clearMail();
    }
}, { immediate: true });

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
    // Пока токен ещё есть — им закрывается сессия
    stopPresence();
    logout();
    router.push('/login');
};

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
    <div v-show="!interfaceHidden" class="interface-view" >
        <div  class="nav-menu">
            <GemButton @click="router.push('/')" :color="'blue'">{{ t('nav.home') }}</GemButton>
            <GemButton v-tutorial="'nav.town'" @click="router.push({ name: 'town' })" :color="'blue'">{{ t('nav.town') }}</GemButton>
            <!-- На арене верхнее меню урезано до «Главная» и «Город»; кнопки результата боя — в самой арене (ArenaView) -->
            <template v-if="!onArena">
                <GemButton v-if="isAuthenticated" v-tutorial="'nav.collection'" :badge-text="badge('collection')" @click="collectionOpen = true" :color="'yellow'">{{ t('nav.collection') }}</GemButton>
                <GemButton v-if="isAuthenticated" v-tutorial="'nav.friends'" :badge-text="badge('friends')" @click="friendsOpen = true" :color="'green'" :badge-count="incomingFriendRequests">{{ t('nav.friends') }}</GemButton>
                <GemButton v-if="isAuthenticated" v-tutorial="'nav.quests'" :badge-text="badge('quests')" @click="questsOpen = true" :color="'yellow'" :badge-count="questsClaimable">{{ t('nav.quests') }}</GemButton>
                <GemButton v-if="isAuthenticated" v-tutorial="'nav.mail'" :badge-text="badge('mail')" @click="mailOpen = true" :color="'blue'" :badge-count="mailBadge">{{ t('nav.mail') }}</GemButton>
                <GemButton v-if="isAuthenticated" v-tutorial="'nav.event'" :badge-text="badge('event')" @click="eventOpen = true" :color="'red'">{{ t('nav.event') }}</GemButton>
                <GemButton v-if="isAuthenticated" v-tutorial="'nav.delve'" :badge-text="badge('delve')" @click="delveOpen = true" :color="'pink'">{{ t('social.delve.nav') }}</GemButton>
                <template v-if="!isAuthenticated">
                    <GemButton v-if="route.name != 'login'" @click="router.push('/login')" :color="'green'">{{ t('nav.login') }}</GemButton>
                    <GemButton v-if="route.name != 'register'" @click="router.push('/register')" :color="'yellow'">{{ t('nav.register') }}</GemButton>
                </template>
                <GemButton v-else @click="logoutConfirmOpen = true" :color="'red'">{{ t('nav.logout') }}</GemButton>
            </template>
        </div>
        <div class="settings">
            <div v-if="isAuthenticated && resources" v-tutorial="'resources'" class="resources">
                <span class="resource resource-gold" v-tooltip="t('resources.gold')"><IconCoin /> {{ resources.gold.toLocaleString(locale) }}</span>
                <span class="resource resource-experience" v-tooltip="t('resources.experience')"><IconExperience /> {{ resources.experience.toLocaleString(locale) }}</span>
                <span v-if="resources.rainbowCoins !== undefined" class="resource resource-rainbow" v-tooltip="t('shop.currency.rainbowCoins')"><IconRainbowCoin /> {{ resources.rainbowCoins.toLocaleString(locale) }}</span>
            </div>
            <AccountButton v-if="isAuthenticated" @click="accountOpen = true" />
        </div>
    </div>
    <AccountModal v-if="isAuthenticated" v-model="accountOpen" />
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
.interface-view {
  width: 100%;
  height: 100%;
  position: fixed;
  z-index: 1;
  pointer-events: none;
  display: flex;
}
.nav-menu {
  width: 50%;
  height: 15%;
  display: flex;
  justify-content: flex-start;
  align-items: flex-start;
  padding: 20px;
  -moz-column-gap: 5px;
  column-gap: 5px;
  flex-wrap: balance;
}
.settings {
  width: 50%;
  display: flex;
  justify-content: flex-end;
  align-items: flex-start;
  padding: 20px;
}
/* Ресурсы аккаунта: табличка в стиле шапки модалки */
.resources {
  display: flex;
  align-items: center;
  column-gap: 14px;
  height: 40px;
  margin-right: 10px;
  padding: 0 14px;
  box-sizing: border-box;
  border-radius: 10px;
  background: var(--wood-background);
  box-shadow: inset 0 0 6px black, 0 0 0 2px rgb(157, 173, 175), 0 0 0 4px #333;
  font-family: 'Arial', sans-serif;
  font-size: 15px;
  font-weight: bold;
  text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
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

.nav-menu > *, .settings > * {
  pointer-events: auto;
}
</style>
