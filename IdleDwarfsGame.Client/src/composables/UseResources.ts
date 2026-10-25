import { readonly, ref } from 'vue'
import ExpeditionsApiService from '@/services/apiServices/ExpeditionsApiService'
import { PlayerResources } from '@/models/types/Expeditions'

// Золото и опыт аккаунта. Состояние общее для всего приложения, как и в useAuth
const resources = ref<PlayerResources | null>(null)

export function useResources() {
  const refresh = async () => {
    try {
      resources.value = await ExpeditionsApiService.getResources()
    } catch {
      // Шапка просто не покажет ресурсы, пока сервер недоступен
    }
  }

  // Сервер присылает итог после начисления — так шапка обновляется без лишнего запроса
  const set = (value: PlayerResources) => {
    resources.value = value
  }

  const clear = () => {
    resources.value = null
  }

  return {
    resources: readonly(resources),
    refresh,
    set,
    clear
  }
}
