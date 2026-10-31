import { ref } from 'vue'

// Скрыта ли верхняя панель игры (кнопки разделов, ресурсы, аккаунт). Общее на приложение:
// панель живёт в InterfaceView, а прячет её страница, которой нужен чистый экран (нагрузочный тест арены)
const interfaceHidden = ref(false)

export function useInterface() {
  return {
    interfaceHidden
  }
}
