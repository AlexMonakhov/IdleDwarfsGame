import { computed, ref } from 'vue'

// Полноэкранная загрузка (арена, город) сейчас на экране. Пока она видна, облачко обучения
// и поднятая над окнами табличка ресурсов не показываются — они висят выше и легли бы поверх загрузки
const shown = ref(0)

export const loadingScreenVisible = computed(() => shown.value > 0)

export function useLoadingScreen() {
  let mine = false
  const set = (visible: boolean) => {
    if (visible === mine) return
    mine = visible
    shown.value += visible ? 1 : -1
  }
  return { set }
}
