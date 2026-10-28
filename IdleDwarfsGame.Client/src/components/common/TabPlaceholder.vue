<script lang="ts" setup>
// Заглушка для вкладок, у которых ещё нет содержимого
defineProps<{
    icon: string;
    title: string;
    hint: string;
}>();
</script>

<template>
    <div class="tab-placeholder">
        <span class="tab-placeholder-icon">{{ icon }}</span>
        <h3 class="tab-placeholder-title">{{ title }}</h3>
        <p class="tab-placeholder-hint">{{ hint }}</p>
    </div>
</template>

<style scoped>
.tab-placeholder {
    flex: 1;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    row-gap: 8px;
    font-family: 'Arial', sans-serif;
    text-align: center;
}

.tab-placeholder-icon {
    font-size: 64px;
    line-height: 1;
    opacity: 0.85;
    filter: drop-shadow(2px 3px 2px rgba(0, 0, 0, 0.8));
}

.tab-placeholder-title {
    margin: 0;
    color: #f8fafc;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.tab-placeholder-hint {
    margin: 0;
    max-width: 360px;
    color: #cbd5e1;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}
</style>
