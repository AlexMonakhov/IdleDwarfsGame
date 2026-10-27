import { ref } from 'vue'

// Открыто ли окно гильдии. Общее на приложение: арена после боя с боссом возвращает игрока прямо в него
const guildOpen = ref(false)

export function useGuild() {
  return {
    guildOpen
  }
}
