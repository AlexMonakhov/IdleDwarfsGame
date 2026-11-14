<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import SquadPanel from '@/components/inventory/SquadPanel.vue';

// Отдельное окно отряда — для кнопок «Сменить отряд» в башне, PvP и боссах гильдии.
// В городе отряд — первая вкладка окна алтаря (HeroUpgradeModal)
const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t } = useI18n();

const panel = ref<InstanceType<typeof SquadPanel> | null>(null);

// Закрытие (крестик, мимо окна, Esc) — только после ответа про несохранённый состав
const open = computed({
    get: () => props.modelValue,
    set: async value => {
        if (!value && panel.value && !(await panel.value.confirmLeave())) return;
        emit('update:modelValue', value);
    },
});
</script>

<template>
    <!-- Ширина — под 5 слотов в один ряд: 5 × 148 + зазоры, поля секции и тела модалки -->
    <ModalComponent
        v-model="open"
        :header-message="t('squad.header')"
        width="min(960px, calc(94 * var(--vw)))"
        height="min(800px, calc(90 * var(--vh)))"
    >
        <!-- Тело модалки монтируется при открытии — панель каждый раз загружает свежий состав -->
        <SquadPanel ref="panel" />
    </ModalComponent>
</template>
