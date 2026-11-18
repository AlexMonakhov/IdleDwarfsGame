import { ref } from 'vue'

// Скрыта ли верхняя панель игры (кнопки разделов, ресурсы, аккаунт). Общее на приложение:
// панель живёт в InterfaceView, а прячет её страница, которой нужен чистый экран (нагрузочный тест арены)
const interfaceHidden = ref(false)

// Окна разделов. Сами модалки живут в InterfaceView, а открываются кликом по зданиям города (TownView)
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
const mailOpen = ref(false)
const collectionOpen = ref(false)
// Награды (календарь входов и достижения) и открытая в них вкладка: календарь открывается сам при первом входе за сутки
export type RewardsTab = 'calendar' | 'achievements'
const rewardsOpen = ref(false)
const rewardsTab = ref<RewardsTab>('calendar')
// Кампания и башня живут в городе (TownView), но флаги общие: их открывает и обучение
const campaignOpen = ref(false)
const towerOpen = ref(false)

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
    eventOpen,
    mailOpen,
    collectionOpen,
    rewardsOpen,
    rewardsTab,
    campaignOpen,
    towerOpen
  }
}
