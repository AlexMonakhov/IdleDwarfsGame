import { computed, ref, type Ref } from 'vue'
import type { HeroElement } from '@/battle/types'
import type { HeroInfo } from '@/models/types/Heroes'

// Фильтры списков героев: по стихии и по рангу (hero.level — ранг 1–4, не уровень прокачки).
// null — фильтр выключен. Кнопки фильтров — HeroFilters.vue
export function useHeroFilters(heroes: Ref<readonly HeroInfo[] | null | undefined>) {
  const element = ref<HeroElement | null>(null)
  const rank = ref<number | null>(null)

  const filtered = computed(() => (heroes.value ?? [])
    .filter(h => (element.value === null || h.element === element.value) && (rank.value === null || h.level === rank.value)))

  const reset = () => {
    element.value = null
    rank.value = null
  }

  return { element, rank, filtered, reset }
}
