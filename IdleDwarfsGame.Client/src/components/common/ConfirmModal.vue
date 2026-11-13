<script lang="ts" setup>
import { useI18n } from 'vue-i18n';
import ModalComponent from './ModalComponent.vue';
import GemButton from './GemButton.vue';

// Окно подтверждения: вопрос и две кнопки «Да» / «Нет». «Да» — событие confirm, «Нет» и крестик просто закрывают
const props = defineProps<{ modelValue: boolean; message: string; header?: string }>();
const emit = defineEmits<{
    (e: 'update:modelValue', value: boolean): void;
    (e: 'confirm'): void;
}>();

const { t } = useI18n();

const close = () => emit('update:modelValue', false);

const confirm = () => {
    close();
    emit('confirm');
};
</script>

<template>
    <ModalComponent
        :model-value="props.modelValue"
        :header-message="props.header"
        width="min(460px, calc(92 * var(--vw)))"
        height="min(260px, calc(60 * var(--vh)))"
        @update:model-value="emit('update:modelValue', $event)"
    >
        <div class="confirm">
            <p class="confirm-message">{{ props.message }}</p>
            <div class="confirm-actions">
                <GemButton :color="'red'" @click="confirm">{{ t('common.yes') }}</GemButton>
                <GemButton :color="'blue'" @click="close">{{ t('common.no') }}</GemButton>
            </div>
        </div>
    </ModalComponent>
</template>

<style scoped>
.confirm {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 20px;
    height: 100%;
    padding: 8px 16px;
    text-align: center;
}

.confirm-message {
    margin: 0;
    color: #f8fafc;
    font-size: 18px;
    font-weight: bold;
}

.confirm-actions {
    display: flex;
    gap: 16px;
}
</style>
