import { computed, ref } from 'vue'
import MailApiService from '@/services/apiServices/MailApiService'
import type { MailInfo } from '@/models/types/Mail'

// Почта — общая на приложение: окно почты её показывает, кнопка в шапке берёт значок.
// Пока без push (позже — SignalR): перечитываем при входе в аккаунт и при каждом заходе в город (TownView)
const mails = ref<MailInfo[]>([])

const refresh = async () => {
  try {
    mails.value = await MailApiService.getMine()
  } catch {
    // Значок просто не обновится, пока сервер недоступен
  }
}

// Письмо требует внимания: не прочитано или в нём лежат незабранные вложения
const needsAttention = (mail: MailInfo) => !mail.isRead || (mail.attachments.length > 0 && !mail.isClaimed)

export function useMail() {
  // Ответ сервера по одному письму (прочитано, забрано) — подменяем его в списке
  const update = (mail: MailInfo) => {
    mails.value = mails.value.map(m => (m.id === mail.id ? mail : m))
  }

  const clear = () => {
    mails.value = []
  }

  return {
    mails: computed(() => mails.value),
    badge: computed(() => mails.value.filter(needsAttention).length),
    refresh,
    update,
    clear
  }
}
