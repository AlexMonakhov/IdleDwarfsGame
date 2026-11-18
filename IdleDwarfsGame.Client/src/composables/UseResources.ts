import { readonly, ref } from 'vue'
import ExpeditionsApiService from '@/services/apiServices/ExpeditionsApiService'
import { PlayerResources } from '@/models/types/Expeditions'
import { soundService } from '@/services/soundService/SoundService'

// Золото и опыт аккаунта. Состояние общее для всего приложения, как и в useAuth
const resources = ref<PlayerResources | null>(null)

// Крупное начисление золота — от такой доли нового баланса
const BIG_GOLD_SHARE = 0.15

// Звук начисления: сервер прислал больше валюты, чем было. Один звук на ответ — самый ценный
const playGainSound = (before: PlayerResources | null, after: PlayerResources) => {
  if (!before) return
  const gained = (key: keyof PlayerResources) => (after[key] ?? 0) - (before[key] ?? 0)
  if (gained('rainbowCoins') > 0) soundService.play('gem_gain')
  else if (gained('gold') > 0) soundService.play(gained('gold') >= after.gold * BIG_GOLD_SHARE ? 'coin_gain_big' : 'coin_gain_small')
  else if (gained('bloodCoins') > 0 || gained('honorBadges') > 0 || gained('elderSeals') > 0) soundService.play('currency_gain')
}

export function useResources() {
  const refresh = async () => {
    try {
      resources.value = await ExpeditionsApiService.getResources()
    } catch {
      // Шапка просто не покажет ресурсы, пока сервер недоступен
    }
  }

  // Сервер присылает итог после начисления — так шапка обновляется без лишнего запроса.
  // Сливаем с прежним: ответ шахт без валют магазинов не должен их обнулять
  const set = (value: PlayerResources) => {
    playGainSound(resources.value, value)
    resources.value = { ...resources.value, ...value }
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
