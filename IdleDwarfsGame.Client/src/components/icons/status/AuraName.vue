<script lang="ts" setup>
import { computed } from 'vue';
import { STATUS_ICONS } from './index';

// Аура в строке: иконка её эффекта (как у статусов) и описание. aura — строка героя («Burn 4 → Enemies»),
// эффект — её первое слово (как в auraEffectOf, BattleState.ts)
const props = defineProps<{ aura: string; size?: string }>();

const icon = computed(() => STATUS_ICONS[props.aura.split(' ')[0]]);
</script>

<template>
    <span class="aura-name">
        <component :is="icon" v-if="icon" :size="size" />
        {{ aura }}
    </span>
</template>

<style scoped>
.aura-name {
    display: inline-flex;
    align-items: center;
    column-gap: 4px;
    white-space: nowrap;
}
</style>
