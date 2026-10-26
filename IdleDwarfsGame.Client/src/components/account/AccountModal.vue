<script lang="ts" setup>
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import GemButton from '@/components/common/GemButton.vue';
import AvatarFrame from './AvatarFrame.vue';
import type { ModalTab } from '@/models/types/Modal';
import type { FrameEffectInfo } from '@/models/types/Account';
import { useAccount } from '@/composables/UseAccount';
import { useTutorial } from '@/composables/UseTutorial';
import { soundService } from '@/services/soundService/SoundService';
import { LOCALE_NAMES, setLocale, SUPPORTED_LOCALES } from '@/i18n';

const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t, locale } = useI18n();
const { user, level, frames, selectedFrame, selectFrame } = useAccount();
const tutorial = useTutorial();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

type AccountTab = 'profile' | 'settings';

const ICONS: Record<AccountTab, string> = {
    profile: '🧔',
    settings: '⚙',
};

const tab = ref<AccountTab>('profile');

// Профиль есть только у вошедшего игрока; гостю доступны одни настройки
const tabs = computed<ModalTab[]>(() => (Object.keys(ICONS) as AccountTab[])
    .filter(key => key !== 'profile' || user.value)
    .map(key => ({ key, label: t(`account.tabs.${key}`), icon: ICONS[key] })));

const activeTab = computed({
    get: () => tab.value,
    set: value => { tab.value = value as AccountTab; },
});

const notice = ref<string | null>(null);

watch(open, isOpen => {
    if (!isOpen) return;
    notice.value = null;
    if (!user.value) tab.value = 'settings';
}, { immediate: true });

/* ===== Рамки ===== */
const frameStatus = (frame: FrameEffectInfo) => {
    if (frame.key === selectedFrame.value) return t('account.frameSelected');
    return frame.unlocked ? t('account.frameSelect') : `🔒 ${t('account.frameLocked')}`;
};

/* ===== Звук ===== */
const sound = soundService.settings;
const percent = (volume: number) => `${Math.round(volume * 100)}%`;
const rangeValue = (event: Event) => Number((event.target as HTMLInputElement).value);

/* ===== Обучение ===== */
const resetTutorial = () => {
    tutorial.reset();
    notice.value = t('account.tutorialResetDone');
};

const restartTutorial = () => {
    tutorial.restart();
    open.value = false;
};
</script>

<template>
    <ModalComponent
        v-model="open"
        v-model:active-tab="activeTab"
        :tabs="tabs"
        :header-message="t(`account.tabs.${tab}`)"
        width="min(720px, calc(94vw - 140px))"
        height="min(620px, 84vh)"
    >
        <!-- ===== Профиль ===== -->
        <div v-if="tab === 'profile' && user" class="account">
            <section class="account-card">
                <AvatarFrame :effect="selectedFrame" :size="112" />
                <div class="account-info">
                    <span class="account-name">{{ user.username }}</span>
                    <span class="account-email">{{ user.email }}</span>
                    <span class="account-level">{{ t('account.level', { level }) }}</span>
                </div>
            </section>

            <section class="account-section">
                <h4 class="account-subtitle">{{ t('account.framesTitle') }}</h4>
                <p class="account-hint">{{ t('account.framesHint') }}</p>
                <ul class="frame-grid">
                    <li v-for="frame in frames" :key="frame.key">
                        <button
                            type="button"
                            class="frame-tile"
                            :class="{
                                'frame-tile-selected': frame.key === selectedFrame,
                                'frame-tile-locked': !frame.unlocked,
                            }"
                            :disabled="!frame.unlocked"
                            @click="selectFrame(frame.key)"
                        >
                            <!-- Закрытые рамки тоже анимированы — игрок видит, что получит -->
                            <AvatarFrame :effect="frame.key" :size="64" />
                            <span class="frame-name">{{ t(`account.frames.${frame.key}`) }}</span>
                            <span class="frame-status">{{ frameStatus(frame) }}</span>
                        </button>
                    </li>
                </ul>
            </section>
        </div>

        <!-- ===== Настройки ===== -->
        <div v-else class="account">
            <section class="account-section">
                <h4 class="account-subtitle">{{ t('account.language') }}</h4>
                <div class="locale-list">
                    <button
                        v-for="code in SUPPORTED_LOCALES"
                        :key="code"
                        type="button"
                        class="locale-option"
                        :class="{ 'locale-option-active': code === locale }"
                        @click="setLocale(code)"
                    >{{ LOCALE_NAMES[code] }}</button>
                </div>
            </section>

            <section class="account-section">
                <h4 class="account-subtitle">{{ t('account.sound') }}</h4>
                <div class="sound-row">
                    <span class="sound-label">{{ t('account.music') }}</span>
                    <button
                        type="button"
                        class="sound-mute"
                        :title="sound.musicMuted ? t('account.unmute') : t('account.mute')"
                        @click="soundService.setMusicMuted(!sound.musicMuted)"
                    >{{ sound.musicMuted ? '🔇' : '🔊' }}</button>
                    <input
                        class="sound-range"
                        type="range" min="0" max="1" step="0.05"
                        :value="sound.musicMuted ? 0 : sound.musicVolume"
                        @input="soundService.setMusicVolume(rangeValue($event))"
                    />
                    <span class="sound-value">{{ percent(sound.musicMuted ? 0 : sound.musicVolume) }}</span>
                </div>
                <div class="sound-row">
                    <span class="sound-label">{{ t('account.effects') }}</span>
                    <button
                        type="button"
                        class="sound-mute"
                        :title="sound.muted ? t('account.unmute') : t('account.mute')"
                        @click="soundService.toggleMute()"
                    >{{ sound.muted ? '🔇' : '🔊' }}</button>
                    <input
                        class="sound-range"
                        type="range" min="0" max="1" step="0.05"
                        :value="sound.muted ? 0 : sound.volume"
                        @input="soundService.setVolume(rangeValue($event))"
                    />
                    <span class="sound-value">{{ percent(sound.muted ? 0 : sound.volume) }}</span>
                </div>
            </section>

            <section class="account-section">
                <h4 class="account-subtitle">{{ t('account.tutorial') }}</h4>
                <p class="account-hint">{{ t('account.tutorialHint') }}</p>
                <div class="tutorial-actions">
                    <GemButton :color="'yellow'" @click="resetTutorial">{{ t('account.tutorialReset') }}</GemButton>
                    <GemButton :color="'green'" @click="restartTutorial">{{ t('account.tutorialRestart') }}</GemButton>
                </div>
                <p v-if="notice" class="account-notice">{{ notice }}</p>
            </section>
        </div>
    </ModalComponent>
</template>

<style scoped>
.account {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    row-gap: 18px;
    padding: 6px;
    font-family: 'Arial', sans-serif;
    text-align: left;
}

.account-subtitle {
    margin: 0 0 8px;
    color: #f8fafc;
    font-size: 14px;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.account-hint {
    margin: 0 0 10px;
    color: #cbd5e1;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.account-notice {
    margin: 10px 0 0;
    color: #fff;
    font-size: 14px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

/* ===== Карточка игрока ===== */
.account-card {
    display: flex;
    align-items: center;
    column-gap: 24px;
    padding: 18px 20px;
    border-radius: 12px;
    border: 2px solid rgba(157, 173, 175, 0.5);
    background: rgba(15, 23, 32, 0.7);
    box-shadow: 0 3px 8px rgba(0, 0, 0, 0.45);
}

.account-info {
    min-width: 0;
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    row-gap: 4px;
}

.account-name {
    max-width: 100%;
    overflow: hidden;
    color: #f8fafc;
    font-size: 22px;
    font-weight: bold;
    text-overflow: ellipsis;
    white-space: nowrap;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.account-email {
    color: #94a3b8;
    font-size: 13px;
}

.account-level {
    margin-top: 6px;
    padding: 4px 12px;
    border-radius: 10px;
    background: var(--wood-background);
    box-shadow: inset 0 0 6px black, 0 0 0 2px rgb(157, 173, 175), 0 0 0 4px #333;
    color: #ffd166;
    font-size: 14px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

/* ===== Рамки ===== */
.frame-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(130px, 1fr));
    gap: 12px;
    margin: 0;
    padding: 0;
    list-style: none;
}

.frame-tile {
    width: 100%;
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 6px;
    padding: 20px 8px 10px;
    box-sizing: border-box;
    border-radius: 10px;
    border: 2px solid rgba(157, 173, 175, 0.5);
    background: rgba(15, 23, 32, 0.7);
    box-shadow: 0 3px 8px rgba(0, 0, 0, 0.45);
    color: #cbd5e1;
    font-family: 'Arial', sans-serif;
    cursor: pointer;
    transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.frame-tile:hover:not(:disabled) {
    border-color: rgba(255, 209, 102, 0.7);
}

.frame-tile-selected,
.frame-tile-selected:hover:not(:disabled) {
    border-color: var(--yellow-gem-color);
    box-shadow: 0 0 12px 2px var(--yellow-gem-color);
    cursor: default;
}

.frame-tile-locked {
    cursor: not-allowed;
}

/* Сама рамка чуть приглушена, но эффект виден */
.frame-tile-locked .avatar-frame {
    filter: saturate(0.75) brightness(0.85);
}

.frame-name {
    margin-top: 6px;
    color: #f8fafc;
    font-size: 14px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.frame-status {
    font-size: 12px;
}

.frame-tile-selected .frame-status {
    color: #ffd166;
    font-weight: bold;
}

.frame-tile-locked .frame-status {
    color: #94a3b8;
}

/* ===== Язык ===== */
.locale-list {
    display: flex;
    column-gap: 8px;
}

.locale-option {
    flex: 1;
    max-width: 200px;
    height: 38px;
    border: none;
    border-radius: 10px;
    background: var(--wood-background);
    box-shadow: inset 0 0 6px black, 0 0 0 2px rgb(157, 173, 175), 0 0 0 4px #333;
    color: #cbd5e1;
    font: bold 14px 'Arial', sans-serif;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
    cursor: pointer;
    transition: color 0.15s ease, box-shadow 0.15s ease;
}

.locale-option:hover {
    color: #fff;
}

.locale-option-active {
    color: #fff;
    cursor: default;
    box-shadow: inset 0 0 6px black, 0 0 0 2px var(--yellow-gem-color), 0 0 0 4px #333, 0 0 12px 2px var(--yellow-gem-color);
}

/* ===== Звук ===== */
.sound-row {
    display: flex;
    align-items: center;
    column-gap: 12px;
    min-height: 48px;
    margin-bottom: 8px;
    padding: 4px 14px;
    box-sizing: border-box;
    border-radius: 10px;
    border: 2px solid rgba(157, 173, 175, 0.5);
    background: rgba(15, 23, 32, 0.7);
}

.sound-label {
    width: 90px;
    color: #f8fafc;
    font-size: 15px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.sound-mute {
    width: 36px;
    height: 36px;
    padding: 0;
    border: none;
    border-radius: 50%;
    background: rgba(0, 0, 0, 0.35);
    font-size: 18px;
    cursor: pointer;
}

.sound-mute:hover {
    background: rgba(255, 255, 255, 0.12);
}

.sound-range {
    flex: 1;
    accent-color: #ffd166;
    cursor: pointer;
}

.sound-value {
    width: 44px;
    color: #ffd166;
    font-size: 14px;
    font-weight: bold;
    text-align: right;
}

/* ===== Обучение ===== */
.tutorial-actions {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
}
</style>
