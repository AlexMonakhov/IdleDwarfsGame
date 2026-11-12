import { ref } from 'vue'

// Окна друзей и глубинного спуска. Общие на приложение: после боя (наёмник, спуск) арена возвращает игрока в них
const friendsOpen = ref(false)
const friendsTab = ref<'friends' | 'requests' | 'mercenaries'>('friends')
const delveOpen = ref(false)

export function useSocial() {
  return {
    friendsOpen,
    friendsTab,
    delveOpen
  }
}
