import { readonly, ref } from 'vue'
import { AxiosError } from 'axios'
import SessionsApiService from '@/services/apiServices/SessionsApiService'
import { useAuth } from '@/composables/UseAuth'

// Сессия этой вкладки: пока игрок залогинен и вкладка открыта, сервер видит его «в сети».
// Состояние общее для всего приложения, как и в useAuth
const incomingFriendRequests = ref(0)

let sessionId: string | null = null
let timer = 0
// Игрок залогинен и сессия нужна — start/stop могут прийти, пока предыдущий запрос ещё в пути
let wanted = false
let starting: Promise<void> | null = null

const openSession = async () => {
  try {
    const session = await SessionsApiService.start()
    if (!wanted) {
      // Пока открывали, игрок успел выйти
      const token = useAuth().token.value
      if (token) SessionsApiService.end(session.sessionId, token)
      return
    }
    sessionId = session.sessionId
    window.clearInterval(timer)
    timer = window.setInterval(heartbeat, session.heartbeatSeconds * 1000)
    // Первый пинг сразу — чтобы значок заявок не ждал минуту
    await heartbeat()
  } catch {
    // Сервер недоступен — попробуем при следующем входе или возврате на вкладку
  }
}

const heartbeat = async () => {
  if (!sessionId) return
  try {
    const result = await SessionsApiService.heartbeat(sessionId)
    incomingFriendRequests.value = result.incomingFriendRequests
  } catch (error) {
    const status = (error as AxiosError).response?.status
    if (status === 404) {
      // Сервер закрыл сессию (например, вкладка долго спала) — открываем новую
      sessionId = null
      window.clearInterval(timer)
      start()
    } else if (status === 401) {
      stop()
    }
    // Остальное (сеть моргнула) — следующий пинг попробует снова
  }
}

const start = () => {
  wanted = true
  if (sessionId || starting) return
  starting = openSession().finally(() => {
    starting = null
  })
}

const stop = () => {
  wanted = false
  window.clearInterval(timer)
  const token = useAuth().token.value
  if (sessionId && token) SessionsApiService.end(sessionId, token)
  sessionId = null
  incomingFriendRequests.value = 0
}

// Закрытие и перезагрузка вкладки заканчивают сессию; вкладка, вернувшаяся из bfcache, открывает новую
window.addEventListener('pagehide', () => {
  const keep = wanted
  stop()
  wanted = keep
})
window.addEventListener('pageshow', event => {
  if (event.persisted && wanted) start()
})

export function usePresence() {
  // Сервер присылает актуальное число вместе с пингом; окно друзей обновляет его сразу после своих действий
  const setIncomingFriendRequests = (count: number) => {
    incomingFriendRequests.value = count
  }

  return {
    incomingFriendRequests: readonly(incomingFriendRequests),
    setIncomingFriendRequests,
    start,
    stop
  }
}
