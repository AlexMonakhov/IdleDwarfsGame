<script lang="ts" setup>
import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import GemButton from '@/components/common/GemButton.vue';
import InputComponent from '@/components/common/InputComponent.vue';
import PanelComponent from '@/components/common/PanelComponent.vue';
import { useAuth } from '@/composables/UseAuth';
import { AuthApiError } from '@/services/apiServices/AuthApiService';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
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

  if (!username) errors.username = t('auth.errors.enterUsername');
  else if (username.length < 3 || username.length > 24) errors.username = t('auth.errors.usernameLength');
  else if (!USERNAME_REGEX.test(username)) errors.username = t('auth.errors.usernameChars');

  if (!form.email.trim()) errors.email = t('auth.errors.enterEmail');
  else if (!EMAIL_REGEX.test(form.email.trim())) errors.email = t('auth.errors.invalidEmail');

  if (!form.password) errors.password = t('auth.errors.enterPassword');
  else if (form.password.length < 6) errors.password = t('auth.errors.passwordLength');

  if (form.confirmPassword !== form.password) errors.confirmPassword = t('auth.errors.passwordsMismatch');

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
      errorMessage.value = t('common.somethingWrong');
    }
  } finally {
    loading.value = false;
  }
};
</script>

<template>
  <div class="main auth-page">
    <PanelComponent :header-message="t('auth.registerHeader')">
      <form class="auth-form" novalidate @submit.prevent="onSubmit">
        <InputComponent
          v-model="form.username"
          :label="t('auth.username')"
          autocomplete="username"
          :error="fieldErrors.username"
          :disabled="loading"
        />
        <InputComponent
          v-model="form.email"
          :label="t('auth.email')"
          type="email"
          autocomplete="email"
          :error="fieldErrors.email"
          :disabled="loading"
        />
        <InputComponent
          v-model="form.password"
          :label="t('auth.password')"
          type="password"
          autocomplete="new-password"
          :error="fieldErrors.password"
          :disabled="loading"
        />
        <InputComponent
          v-model="form.confirmPassword"
          :label="t('auth.confirmPassword')"
          type="password"
          autocomplete="new-password"
          :error="fieldErrors.confirmPassword"
          :disabled="loading"
        />

        <div class="auth-error">{{ errorMessage ?? '' }}</div>

        <GemButton type="submit" :color="'green'" width="100%" :disabled="loading">
          {{ loading ? t('auth.creatingAccount') : t('auth.createAccount') }}
        </GemButton>
      </form>

      <div class="auth-switch">
        {{ t('auth.haveAccount') }}
        <router-link to="/login" class="auth-link">{{ t('auth.toLogin') }}</router-link>
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
