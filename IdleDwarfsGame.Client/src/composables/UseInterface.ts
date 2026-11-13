import { ref } from 'vue'

// Скрыта ли верхняя панель игры (кнопки разделов, ресурсы, аккаунт). Общее на приложение:
// панель живёт в InterfaceView, а прячет её страница, которой нужен чистый экран (нагрузочный тест арены)
const interfaceHidden = ref(false)

// Окна разделов. Сами модалки живут в InterfaceView, а открываются кликом по зданиям города (AboutView)
const expeditionsOpen = ref(false)
const inventoryOpen = ref(false)
const upgradeOpen = ref(false)
// Герой, которого окно алтаря покажет при открытии на вкладке героев (клик по карточке на складе)
const upgradeHeroId = ref<string | null>(null)
const miningOpen = ref(false)
const shopOpen = ref(false)
const forgeOpen = ref(false)
// Ежедневные задания и ивент недели открываются кнопками в шапке
const questsOpen = ref(false)
const eventOpen = ref(false)

export function useInterface() {
  return {
    interfaceHidden,
    expeditionsOpen,
    inventoryOpen,
    upgradeOpen,
    upgradeHeroId,
    miningOpen,
    shopOpen,
    forgeOpen,
    questsOpen,
    eventOpen
  }
}
