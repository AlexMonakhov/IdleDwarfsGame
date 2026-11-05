import { computed, readonly, ref, shallowRef } from 'vue'
import HeroesApiService from '@/services/apiServices/HeroesApiService'
import { HeroInfo } from '@/models/types/Heroes'

// Герои игрока, общие для всего приложения (как useResources). Окно, открытое повторно, сразу показывает
// прошлый список, а свежий подтягивает в фоне — без «Загрузка…» и без пустого кадра.
// Одновременные запросы склеиваются в один. Сбрасывается при выходе из аккаунта (InterfaceView)
const heroes = shallowRef<HeroInfo[] | null>(null)
const error = ref(false)
let pending: Promise<HeroInfo[] | null> | null = null
// Ответ, пришедший после выхода из аккаунта, не должен вернуть чужих героев
let generation = 0

// Тот же JSON — тот же список: не подменяем массив, чтобы карточки не перерисовывались зря
let lastJson = ''

const apply = (next: HeroInfo[]) => {
  const json = JSON.stringify(next)
  if (json === lastJson) return
  lastJson = json
  heroes.value = next
}

export function useHeroes() {
  const refresh = (): Promise<HeroInfo[] | null> => {
    if (pending) return pending
    const started = generation
    pending = HeroesApiService.getMyHeroes()
      .then(next => {
        if (started !== generation) return null
        error.value = false
        apply(next)
        return heroes.value
      })
      .catch(() => {
        // Старый список остаётся на экране; ошибку показываем, только если показать нечего
        if (started === generation) error.value = true
        return heroes.value
      })
      .finally(() => {
        pending = null
      })
    return pending
  }

  // Сервер прислал итог после действия (сохранение отряда и т.п.) — обновляем без лишнего запроса
  const set = (next: HeroInfo[]) => {
    error.value = false
    apply(next)
  }

  const clear = () => {
    generation++
    pending = null
    lastJson = ''
    heroes.value = null
    error.value = false
  }

  return {
    // computed, а не readonly: карточки ждут обычный HeroInfo, а менять список всё равно можно только через set
    heroes: computed(() => heroes.value),
    error: readonly(error),
    refresh,
    set,
    clear
  }
}
