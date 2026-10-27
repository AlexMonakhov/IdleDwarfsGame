import { ref } from 'vue'

// Открыто ли окно отряда. Общее на приложение: его открывают и из меню, и из других окон (боссы гильдии) —
// поверх них, а те по закрытию перечитывают отряд
const squadOpen = ref(false)

export function useSquadModal() {
  return {
    squadOpen
  }
}
