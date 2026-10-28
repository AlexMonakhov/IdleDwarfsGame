import { ref } from 'vue'

// Открыто ли окно PvP. Общее на приложение: арена после боя PvP возвращает игрока прямо в него
const pvpOpen = ref(false)

export function usePvp() {
  return {
    pvpOpen
  }
}
