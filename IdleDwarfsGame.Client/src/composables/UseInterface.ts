import { ref } from 'vue'

// Скрыта ли верхняя панель игры (кнопки разделов, ресурсы, аккаунт). Общее на приложение:
// панель живёт в InterfaceView, а прячет её страница, которой нужен чистый экран (нагрузочный тест арены)
const interfaceHidden = ref(false)

// Окна разделов. Сами модалки живут в InterfaceView, а открываются кликом по зданиям города (AboutView)
const expeditionsOpen = ref(false)
const inventoryOpen = ref(false)
const upgradeOpen = ref(false)
const miningOpen = ref(false)

export function useInterface() {
  return {
    interfaceHidden,
    expeditionsOpen,
    inventoryOpen,
    upgradeOpen,
    miningOpen
  }
}
