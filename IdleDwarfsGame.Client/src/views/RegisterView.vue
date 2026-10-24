<script lang="ts" setup>
import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import GemButton from '@/components/common/GemButton.vue';
import InputComponent from '@/components/common/InputComponent.vue';
import PanelComponent from '@/components/common/PanelComponent.vue';
import { useAuth } from '@/composables/UseAuth';
import { AuthApiError } from '@/services/apiServices/AuthApiService';

const router = useRouter();
const { register } = useAuth();

const form = reactive({ username: '', email: '', password: '', confirmPassword: '' });
const fieldErrors = ref<Record<string, string>>({});
const errorMessage = ref<string | null>(null);
const loading = ref(false);

// Те же правила, что и в RegisterRequest на сервере
const USERNAME_REGEX = /^[a-zA-Z0-9_-]+$/;
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

const validate = (): boolean => {
  const errors: Record<string, string> = {};
  const username = form.username.trim();

  if (!username) errors.username = 'Введите имя';
  else if (username.length < 3 || username.length > 24) errors.username = 'Имя должно быть от 3 до 24 символов';
  else if (!USERNAME_REGEX.test(username)) errors.username = 'Только латиница, цифры, _ и -';

  if (!form.email.trim()) errors.email = 'Введите почту';
  else if (!EMAIL_REGEX.test(form.email.trim())) errors.email = 'Некорректная почта';

  if (!form.password) errors.password = 'Введите пароль';
  else if (form.password.length < 6) errors.password = 'Пароль должен быть не короче 6 символов';

  if (form.confirmPassword !== form.password) errors.confirmPassword = 'Пароли не совпадают';

  fieldErrors.value = errors;
  return Object.keys(errors).length === 0;
};

const onSubmit = async () => {
  errorMessage.value = null;
  if (!validate() || loading.value) return;

  loading.value = true;
  try {
    await register({
      username: form.username.trim(),
      email: form.email.trim(),
      password: form.password
    });
    router.push('/');
  } catch (error) {
    if (error instanceof AuthApiError) {
      errorMessage.value = error.message;
      fieldErrors.value = error.fieldErrors;
    } else {
      errorMessage.value = 'Что-то пошло не так';
    }
  } finally {
    loading.value = false;
  }
};
</script>

<template>
  <div class="main auth-page">
    <PanelComponent :header-message="'register'">
      <form class="auth-form" novalidate @submit.prevent="onSubmit">
        <InputComponent
          v-model="form.username"
          label="Имя"
          autocomplete="username"
          :error="fieldErrors.username"
          :disabled="loading"
        />
        <InputComponent
          v-model="form.email"
          label="Почта"
          type="email"
          autocomplete="email"
          :error="fieldErrors.email"
          :disabled="loading"
        />
        <InputComponent
          v-model="form.password"
          label="Пароль"
          type="password"
          autocomplete="new-password"
          :error="fieldErrors.password"
          :disabled="loading"
        />
        <InputComponent
          v-model="form.confirmPassword"
          label="Повторите пароль"
          type="password"
          autocomplete="new-password"
          :error="fieldErrors.confirmPassword"
          :disabled="loading"
        />

        <div class="auth-error">{{ errorMessage ?? '' }}</div>

        <GemButton type="submit" :color="'green'" width="100%" :disabled="loading">
          {{ loading ? 'Создаём...' : 'Создать аккаунт' }}
        </GemButton>
      </form>

      <div class="auth-switch">
        Уже есть аккаунт?
        <router-link to="/login" class="auth-link">Войти</router-link>
      </div>
    </PanelComponent>
  </div>
</template>

<style scoped>
.auth-page {
  height: 100vh;
}

.auth-form {
  display: flex;
  flex-direction: column;
  row-gap: 6px;
}

.auth-error {
  min-height: 20px;
  margin-bottom: 8px;
  color: #ffb4b4;
  font-family: 'Arial', sans-serif;
  font-size: 14px;
  font-weight: bold;
  text-shadow: 1px 1px 0px rgba(0, 0, 0, 0.8);
}

.auth-switch {
  margin-top: 16px;
  color: #ffffff;
  font-family: 'Arial', sans-serif;
  font-size: 14px;
  text-shadow: 1px 1px 0px rgba(0, 0, 0, 0.6);
}

.auth-link {
  color: var(--yellow-gem-color);
  font-weight: bold;
  text-decoration: none;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.auth-link:hover {
  filter: brightness(1.2) drop-shadow(0 0 2px #fff);
}
</style>
