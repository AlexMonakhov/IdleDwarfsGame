import { ref } from 'vue'

const stack = ref<number[]>([])

export function useModalStack(initialZ = 1000) {
  const zIndex = ref(initialZ)

  const push = () => {
    const top = stack.value.length ? Math.max(...stack.value) + 10 : initialZ
    stack.value.push(top)
    zIndex.value = top
    return zIndex.value
  }

  // Убираем из стека свой слой, а не последний: окно под верхним может закрыться первым
  const pop = () => {
    const index = stack.value.lastIndexOf(zIndex.value)
    if (index >= 0) stack.value.splice(index, 1)
    else stack.value.pop()
    zIndex.value = initialZ
  }

  // Это окно сейчас верхнее: Esc закрывает только его, а не все открытые разом
  const isTop = () => stack.value.length > 0 && zIndex.value === Math.max(...stack.value)

  return {
    zIndex,
    push,
    pop,
    isTop
  }
}