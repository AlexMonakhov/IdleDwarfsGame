<script lang="ts" setup>
import { useI18n } from 'vue-i18n';
import ModalComponent from './ModalComponent.vue';
import GemButton from './GemButton.vue';

// Окно подтверждения: компактное, без шапки и крестика — только вопрос и крупные кнопки (по умолчанию «Да» / «Нет»).
// Первая — событие confirm, вторая — cancel; обе закрывают окно. С dismissible есть третья — «Отмена»:
// просто закрывает, как клик мимо окна и Esc, без событий
type GemColor = 'red' | 'blue' | 'green' | 'yellow' | 'pink';
const props = withDefaults(defineProps<{
    modelValue: boolean;
    message: string;
    confirmText?: string;
    cancelText?: string;
    confirmColor?: GemColor;
    cancelColor?: GemColor;
    dismissible?: boolean;
}>(), {
    confirmColor: 'red',
    cancelColor: 'blue',
});
const emit = defineEmits<{
    (e: 'update:modelValue', value: boolean): void;
    (e: 'confirm'): void;
    (e: 'cancel'): void;
}>();

const { t } = useI18n();

const close = () => emit('update:modelValue', false);

const confirm = () => {
    close();
    emit('confirm');
};

const cancel = () => {
    close();
    emit('cancel');
};
</script>

<template>
    <ModalComponent
        :model-value="props.modelValue"
        :show-close="false"
        :width="props.dismissible ? 'min(460px, calc(92 * var(--vw)))' : 'min(400px, calc(90 * var(--vw)))'"
        height="200px"
        @update:model-value="emit('update:modelValue', $event)"
    >
        <div class="confirm">
            <p class="confirm-message">{{ props.message }}</p>
            <div class="confirm-actions">
                <GemButton :color="props.confirmColor" width="120px" height="54px" @click="confirm">{{ props.confirmText ?? t('common.yes') }}</GemButton>
                <GemButton :color="props.cancelColor" width="120px" height="54px" @click="cancel">{{ props.cancelText ?? t('common.no') }}</GemButton>
                <GemButton v-if="props.dismissible" :color="'blue'" width="120px" height="54px" @click="close">{{ t('common.cancel') }}</GemButton>
            </div>
        </div>
    </ModalComponent>
</template>

<style scoped>
/* Тело модалки — flex-строка: растягиваемся на всю её ширину, чтобы центрирование работало по окну */
.confirm {
    flex: 1;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 22px;
    text-align: center;
}

.confirm-message {
    margin: 0;
    color: #f8fafc;
    font-size: 20px;
    font-weight: bold;
    line-height: 1.3;
    text-shadow: var(--text-outline);
}

.confirm-actions {
    display: flex;
    justify-content: center;
    gap: 20px;
}

.confirm-actions :deep(.gem-text) {
    font-size: 18px;
}
</style>
