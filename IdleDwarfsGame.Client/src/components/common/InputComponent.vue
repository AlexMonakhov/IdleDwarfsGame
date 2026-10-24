<script lang="ts" setup>
import { computed } from 'vue';

interface Props {
  modelValue: string
  label?: string
  type?: 'text' | 'password' | 'email'
  placeholder?: string
  autocomplete?: string
  error?: string | null
  disabled?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  label: '',
  type: 'text',
  placeholder: '',
  autocomplete: 'off',
  error: null,
  disabled: false
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void
}>()

// Уникальный id, чтобы label кликался и фокусировал поле
const inputId = `gem-input-${Math.random().toString(36).slice(2, 9)}`

const value = computed<string>({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v)
})
</script>

<template>
  <div :class="['gem-input', { 'gem-input-error': !!props.error, 'gem-input-disabled': props.disabled }]">
    <label v-if="props.label" :for="inputId" class="gem-input-label">{{ props.label }}</label>
    <div class="gem-input-frame">
      <input
        :id="inputId"
        v-model="value"
        class="gem-input-field"
        :type="props.type"
        :placeholder="props.placeholder"
        :autocomplete="props.autocomplete"
        :disabled="props.disabled"
      />
    </div>
    <span class="gem-input-error-text">{{ props.error ?? '' }}</span>
  </div>
</template>

<style scoped>
.gem-input {
  display: flex;
  flex-direction: column;
  align-items: stretch;
  width: 100%;
  text-align: left;
}

.gem-input-label {
  margin: 0 0 4px 6px;
  color: #ffffff;
  font-family: 'Arial', sans-serif;
  font-weight: bold;
  font-size: 13px;
  text-transform: uppercase;
  letter-spacing: 1px;
  text-shadow: 1px 1px 0px rgba(0, 0, 0, 0.6);
  user-select: none;
}

/* Металлическая рамка — как у GemButton */
.gem-input-frame {
  background: var(--metalic-background);
  padding: 3px;
  border-radius: 8px;
  corner-shape: bevel;
  border: 0.7px solid rgba(0, 0, 0, 0.1);
  box-shadow:
    0 6px 12px rgba(0, 0, 0, 0.4),
    inset 0 1px 2px rgba(255, 255, 255, 0.8);
  transition: box-shadow 0.2s ease, filter 0.2s ease;
}

/* Само поле — тёмное дерево с вдавленной тенью, как шапка модалки */
.gem-input-field {
  box-sizing: border-box;
  width: 100%;
  height: 38px;
  padding: 0 12px;
  border: none;
  outline: none;
  border-radius: 6px;
  corner-shape: bevel;
  background: var(--wood-background);
  box-shadow: inset 0px 0px 6px black, inset 0px 0px 14px #333;
  color: #ffffff;
  font-family: 'Arial', sans-serif;
  font-size: 15px;
  letter-spacing: 0.5px;
  text-shadow: 1px 1px 0px rgba(0, 0, 0, 0.6);
}

.gem-input-field::placeholder {
  color: rgba(255, 255, 255, 0.4);
}

.gem-input-frame:hover {
  filter: brightness(1.05);
}

.gem-input-frame:focus-within {
  box-shadow:
    0 6px 12px rgba(0, 0, 0, 0.4),
    inset 0 1px 2px rgba(255, 255, 255, 0.8),
    0 0 6px 2px #65a7b8;
}

.gem-input-error .gem-input-frame {
  box-shadow:
    0 6px 12px rgba(0, 0, 0, 0.4),
    inset 0 1px 2px rgba(255, 255, 255, 0.8),
    0 0 6px 2px var(--error);
}

.gem-input-error-text {
  min-height: 18px;
  margin: 3px 0 0 6px;
  color: #ffb4b4;
  font-family: 'Arial', sans-serif;
  font-size: 12px;
  font-weight: bold;
  text-shadow: 1px 1px 0px rgba(0, 0, 0, 0.8);
}

.gem-input-disabled .gem-input-frame {
  filter: grayscale(0.6) brightness(0.9);
}

/* Убираем жёлтую подложку автозаполнения в Chrome */
.gem-input-field:-webkit-autofill {
  -webkit-text-fill-color: #ffffff;
  -webkit-box-shadow: inset 0 0 0 1000px #464449, inset 0px 0px 6px black;
  caret-color: #ffffff;
}
</style>
