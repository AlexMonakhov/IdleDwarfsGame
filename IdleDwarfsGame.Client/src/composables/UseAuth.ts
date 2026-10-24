import { computed, ref } from 'vue'
import AuthApiService, { AuthResponse, AuthUser, LoginRequest, RegisterRequest } from '@/services/apiServices/AuthApiService'

const STORAGE_KEY = 'idle-dwarfs-auth'

interface StoredAuth {
  token: string
  expiresAt: string
  user: AuthUser
}

const readStored = (): StoredAuth | null => {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return null
    const stored = JSON.parse(raw) as StoredAuth
    // Протухший токен сразу выкидываем
    if (new Date(stored.expiresAt).getTime() <= Date.now()) {
      localStorage.removeItem(STORAGE_KEY)
      return null
    }
    return stored
  } catch {
    return null
  }
}

// Состояние общее для всего приложения, как и в useModalStack
const auth = ref<StoredAuth | null>(readStored())

const save = (response: AuthResponse) => {
  auth.value = { token: response.token, expiresAt: response.expiresAt, user: response.user }
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(auth.value))
  } catch {
    // localStorage может быть недоступен (приватный режим) — живём без сохранения
  }
}

export function useAuth() {
  const user = computed(() => auth.value?.user ?? null)
  const token = computed(() => auth.value?.token ?? null)
  const isAuthenticated = computed(() => auth.value !== null)

  const login = async (request: LoginRequest) => {
    save(await AuthApiService.login(request))
  }

  const register = async (request: RegisterRequest) => {
    save(await AuthApiService.register(request))
  }

  const logout = () => {
    auth.value = null
    try {
      localStorage.removeItem(STORAGE_KEY)
    } catch {
      // ignore
    }
  }

  return {
    user,
    token,
    isAuthenticated,
    login,
    register,
    logout
  }
}
