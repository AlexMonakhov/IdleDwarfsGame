<script lang="ts" setup>
import { reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import GemButton from '@/components/common/GemButton.vue';
import InputComponent from '@/components/common/InputComponent.vue';
import PanelComponent from '@/components/common/PanelComponent.vue';
import { useAuth } from '@/composables/UseAuth';
import { AuthApiError } from '@/services/apiServices/AuthApiService';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const { login } = useAuth();

const form = reactive({ login: '', password: '' });
const fieldErrors = ref<Record<string, string>>({});
const errorMessage = ref<string | null>(null);
const loading = ref(false);

const validate = (): boolean => {
  const errors: Record<string, string> = {};
  if (!form.login.trim()) errors.login = t('auth.errors.enterLogin');
  if (!form.password) errors.password = t('auth.errors.enterPassword');
  fieldErrors.value = errors;
  return Object.keys(errors).length === 0;
};

const onSubmit = async () => {
  errorMessage.value = null;
  if (!validate() || loading.value) return;

  loading.value = true;
  try {
    await login({ login: form.login.trim(), password: form.password });
    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/';
    router.push(redirect);
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
    <PanelComponent :header-message="t('auth.loginHeader')">
      <form class="auth-form" novalidate @submit.prevent="onSubmit">
        <InputComponent
          v-model="form.login"
          :label="t('auth.loginField')"
          autocomplete="username"
          :error="fieldErrors.login"
          :disabled="loading"
        />
        <InputComponent
          v-model="form.password"
          :label="t('auth.password')"
          type="password"
          autocomplete="current-password"
          :error="fieldErrors.password"
          :disabled="loading"
        />

        <div class="auth-error">{{ errorMessage ?? '' }}</div>

        <GemButton type="submit" :color="'green'" width="100%" :disabled="loading">
          {{ loading ? t('auth.signingIn') : t('auth.signIn') }}
        </GemButton>
      </form>

      <div class="auth-switch">
        {{ t('auth.noAccount') }}
        <router-link to="/register" class="auth-link">{{ t('auth.toRegister') }}</router-link>
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
