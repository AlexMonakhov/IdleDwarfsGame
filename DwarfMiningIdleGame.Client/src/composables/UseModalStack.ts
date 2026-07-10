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

  const pop = () => {
    stack.value.pop()
    zIndex.value = stack.value.length ? Math.max(...stack.value) : initialZ
  }

  return {
    zIndex,
    push,
    pop
  }
}