<script lang="ts" setup>
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { AxiosError } from 'axios';
import ModalComponent from '@/components/common/ModalComponent.vue';
import GemButton from '@/components/common/GemButton.vue';
import MailAttachmentCard from './MailAttachmentCard.vue';
import MailApiService from '@/services/apiServices/MailApiService';
import { useMail } from '@/composables/UseMail';
import { useResources } from '@/composables/UseResources';
import { useHeroes } from '@/composables/UseHeroes';
import type { MailInfo } from '@/models/types/Mail';

// Почта: список писем, новые сверху. Клик по письму раскрывает его панелью (текст и вложения) и отмечает прочитанным.
// «Забрать» выдаёт вложения: дальше они видны серыми с галочкой, второй раз не забрать
const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t, locale } = useI18n();
const { mails, refresh, update } = useMail();
const { set: setResources } = useResources();
const { refresh: refreshHeroes } = useHeroes();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

const expandedId = ref<string | null>(null);
const claimingId = ref<string | null>(null);
const errorMessage = ref<string | null>(null);

watch(open, value => {
    if (value) refresh();
    else expandedId.value = null;
});

const errorText = (error: unknown, fallback: string) =>
    (error as AxiosError<{ message?: string }>).response?.data?.message ?? fallback;

const toggle = async (mail: MailInfo) => {
    errorMessage.value = null;
    expandedId.value = expandedId.value === mail.id ? null : mail.id;
    if (expandedId.value && !mail.isRead) {
        try {
            update(await MailApiService.markRead(mail.id));
        } catch {
            // Отметка «прочитано» не критична: письмо всё равно открыто
        }
    }
};

const claim = async (mail: MailInfo) => {
    if (claimingId.value) return;
    claimingId.value = mail.id;
    errorMessage.value = null;
    try {
        const result = await MailApiService.claim(mail.id);
        update(result.mail);
        if (result.resources) setResources(result.resources);
        // Новые дворфы — в общий список героев (алтарь, отряд)
        if (mail.attachments.some(a => a.kind === 'Hero')) refreshHeroes();
    } catch (error) {
        errorMessage.value = errorText(error, t('mail.claimError'));
    } finally {
        claimingId.value = null;
    }
};

const dateText = (iso: string) => new Date(iso).toLocaleString(locale.value, { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' });
</script>

<template>
    <ModalComponent
        v-model="open"
        :header-message="t('mail.header')"
        width="min(980px, calc(calc(94 * var(--vw)) - 40px))"
        height="min(760px, calc(90 * var(--vh)))"
    >
        <div class="mail">
            <div v-if="!mails.length" class="mail-empty">{{ t('mail.empty') }}</div>
            <div v-if="errorMessage" class="mail-error">{{ errorMessage }}</div>

            <article
                v-for="mail in mails"
                :key="mail.id"
                class="mail-item"
                :class="{ 'mail-item-open': expandedId === mail.id, 'mail-item-unread': !mail.isRead }"
            >
                <button type="button" class="mail-item-head" @click="toggle(mail)">
                    <span class="mail-item-dot" :class="{ 'mail-item-dot-on': !mail.isRead }"></span>
                    <span class="mail-item-title">{{ mail.title }}</span>
                    <span class="mail-item-sender">{{ mail.sender }}</span>
                    <span v-if="mail.attachments.length" class="mail-item-status" :class="{ 'mail-item-status-done': mail.isClaimed }">
                        {{ mail.isClaimed ? t('mail.claimed') : t('mail.attachments', { count: mail.attachments.length }) }}
                    </span>
                    <span class="mail-item-date">{{ dateText(mail.createdAt) }}</span>
                    <span class="mail-item-chevron">▾</span>
                </button>

                <!-- Панель письма выезжает (grid 0fr → 1fr): текст и вложения -->
                <div class="mail-item-body-wrap">
                    <div class="mail-item-body">
                        <p v-if="mail.body" class="mail-item-text">{{ mail.body }}</p>
                        <div v-if="mail.attachments.length" class="mail-item-attachments">
                            <MailAttachmentCard
                                v-for="attachment in mail.attachments"
                                :key="`${attachment.id}-${attachment.index}`"
                                :attachment="attachment"
                                :claimed="mail.isClaimed"
                            />
                        </div>
                        <div v-if="mail.attachments.length" class="mail-item-actions">
                            <GemButton
                                v-if="!mail.isClaimed"
                                color="green"
                                width="170px"
                                height="44px"
                                :disabled="claimingId === mail.id"
                                @click="claim(mail)"
                            >
                                {{ claimingId === mail.id ? t('mail.claiming') : t('mail.claim') }}
                            </GemButton>
                            <span v-else class="mail-item-claimed">✔ {{ t('mail.claimedHint') }}</span>
                        </div>
                    </div>
                </div>
            </article>
        </div>
    </ModalComponent>
</template>

<style scoped>
.mail {
    display: flex;
    flex-direction: column;
    gap: 8px;
    padding: 6px 4px;
    font-family: 'Arial', sans-serif;
    color: #e2e8f0;
}

.mail-empty,
.mail-error {
    text-align: center;
    padding: 20px;
    color: #cbd5e1;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.mail-error {
    padding: 6px;
    color: #ffb4b4;
}

.mail-item {
    border-radius: 12px;
    border: 1px solid rgba(255, 255, 255, 0.12);
    background: rgba(14, 19, 34, 0.6);
    overflow: hidden;
    transition: border-color 0.25s, background 0.25s;
}

.mail-item-unread {
    border-color: rgba(255, 209, 102, 0.55);
}

.mail-item-open {
    background: rgba(20, 27, 46, 0.85);
}

.mail-item-head {
    width: 100%;
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 12px 14px;
    border: 0;
    background: none;
    color: inherit;
    font: inherit;
    text-align: left;
    cursor: pointer;
}

.mail-item-head:hover {
    background: rgba(255, 255, 255, 0.05);
}

.mail-item-dot {
    flex-shrink: 0;
    width: 9px;
    height: 9px;
    border-radius: 50%;
    background: rgba(255, 255, 255, 0.15);
}

.mail-item-dot-on {
    background: #ffd166;
    box-shadow: 0 0 8px #ffd166;
}

.mail-item-title {
    flex: 1;
    min-width: 0;
    font-weight: 700;
    font-size: 15px;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.mail-item-sender,
.mail-item-date {
    flex-shrink: 0;
    font-size: 12px;
    color: #94a3b8;
}

.mail-item-status {
    flex-shrink: 0;
    font-size: 12px;
    padding: 2px 8px;
    border-radius: 999px;
    background: rgba(255, 209, 102, 0.18);
    color: #ffd166;
}

.mail-item-status-done {
    background: rgba(74, 222, 128, 0.15);
    color: #4ade80;
}

.mail-item-chevron {
    flex-shrink: 0;
    color: #94a3b8;
    transition: transform 0.25s;
}

.mail-item-open .mail-item-chevron {
    transform: rotate(180deg);
}

.mail-item-body-wrap {
    display: grid;
    grid-template-rows: 0fr;
    transition: grid-template-rows 0.3s ease;
}

.mail-item-open .mail-item-body-wrap {
    grid-template-rows: 1fr;
}

.mail-item-body {
    min-height: 0;
    overflow: hidden;
}

.mail-item-open .mail-item-body {
    padding: 0 16px 16px;
    border-top: 1px solid rgba(255, 255, 255, 0.08);
}

.mail-item-text {
    margin: 12px 0;
    white-space: pre-wrap;
    line-height: 1.45;
    font-size: 14px;
}

.mail-item-attachments {
    display: flex;
    flex-wrap: wrap;
    gap: 12px;
    padding: 8px 4px;
}

.mail-item-actions {
    display: flex;
    justify-content: center;
    margin-top: 8px;
}

.mail-item-claimed {
    color: #4ade80;
    font-weight: 700;
}
</style>
