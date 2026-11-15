<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { PASSIVE_ICONS } from './index';
import type { PassiveInfo } from '@/models/types/Equipment';

// Пассивка в строке — как SpellName и AuraName: иконка, название и уровень («Острый язык 2»)
const props = defineProps<{ passive: PassiveInfo; size?: string }>();

const { t, te } = useI18n();

const icon = computed(() => PASSIVE_ICONS[props.passive.passive]);
const name = computed(() => {
    const key = `passives.names.${props.passive.passive}`;
    return te(key) ? t(key) : props.passive.passive;
});
</script>

<template>
    <span class="passive-name">
        <component :is="icon" v-if="icon" :size="size" />
        {{ name }} {{ passive.level }}
    </span>
</template>

<style scoped>
.passive-name {
    display: inline-flex;
    align-items: center;
    column-gap: 4px;
    white-space: nowrap;
}
</style>
