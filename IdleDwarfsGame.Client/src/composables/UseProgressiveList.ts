import { computed, onBeforeUnmount, ref, watch, type Ref } from 'vue'

// Длинный список рисуется порциями: первая — сразу, следующие — по одной за кадр.
// Окно с сотнями тяжёлых карточек открывается без подвисания, а прокрутка до конца успевает догнать дорисовку.
// Короткий список (не длиннее порции) выходит целиком в первом же кадре — разницы не видно
export function useProgressiveList<T>(source: Ref<readonly T[]>, chunk = 24) {
  const limit = ref(chunk)
  let frame = 0

  const grow = () => {
    frame = 0
    if (limit.value >= source.value.length) return
    limit.value += chunk
    frame = requestAnimationFrame(grow)
  }

  // Новый список (или он вырос) — дорисовываем дальше с текущего места, уже видимое не сбрасываем
  watch(() => source.value.length, () => {
    if (!frame && limit.value < source.value.length) frame = requestAnimationFrame(grow)
  }, { immediate: true })

  onBeforeUnmount(() => cancelAnimationFrame(frame))

  return computed(() => source.value.slice(0, limit.value))
}
