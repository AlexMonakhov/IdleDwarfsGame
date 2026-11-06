<script lang="ts" setup>
import { computed } from 'vue';
import { spellIcon } from './index';

// Заклинание в строке: иконка и название. spell — имя («Flame Wave») или строка героя с уровнем («Flame Wave 2»)
const props = defineProps<{ spell: string }>();

const icon = computed(() => spellIcon(props.spell));
</script>

<template>
    <span class="spell-name">
        <component :is="icon" v-if="icon" />
        {{ spell }}
    </span>
</template>

<style scoped>
.spell-name {
    display: inline-flex;
    align-items: center;
    column-gap: 4px;
    white-space: nowrap;
}
</style>
