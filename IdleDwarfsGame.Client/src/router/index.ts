import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import ArenaView from '@/views/ArenaView.vue'
import LoginView from '@/views/LoginView.vue'
import RegisterView from '@/views/RegisterView.vue'
import NotFoundView from '@/views/NotFoundView.vue'
import { useAuth } from '@/composables/UseAuth'

const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'home',
    component: HomeView
  },
  {
    path: '/town',
    name: 'town',
    // Ленивый чанк: город грузится при первом заходе
    component: () => import(/* webpackChunkName: "town" */ '@/views/TownView.vue')
  },
  {
    path: '/arena',
    name: 'arena',
    component: ArenaView
  },
  // Нагрузочный тест арены: 10 дворфов разом кастуют заклинания 3-го уровня
  {
    path: '/arena-stress',
    name: 'arena-stress',
    component: () => import(/* webpackChunkName: "arena-stress" */ '@/views/ArenaStressView.vue')
  },
  // Концепты анимаций заклинаний: наброски эффектов на арене, из них выбираются будущие (в меню её нет)
  {
    path: '/fx-concepts',
    name: 'fx-concepts',
    component: () => import(/* webpackChunkName: "fx-concepts" */ '@/views/FxConceptsView.vue')
  },
  // Тестовая страница компонентов (в меню её нет)
  {
    path: '/testpage',
    name: 'testpage',
    component: () => import(/* webpackChunkName: "testpage" */ '@/views/TestPageView.vue')
  },
  // Админ-панель: письма игрокам с вложениями (пока без прав доступа)
  {
    path: '/admin',
    name: 'admin',
    component: () => import(/* webpackChunkName: "admin" */ '@/views/AdminView.vue')
  },
  {
    path: '/login',
    name: 'login',
    component: LoginView,
    meta: { guestOnly: true }
  },
  {
    path: '/register',
    name: 'register',
    component: RegisterView,
    meta: { guestOnly: true }
  },
  // Должен быть последним: ловит все неизвестные пути
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: NotFoundView
  }
]

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes
})

// Уже залогиненного игрока не пускаем обратно на логин/регистрацию, гостя — на страницы игрока
router.beforeEach((to) => {
  const { isAuthenticated } = useAuth()
  if (to.meta.guestOnly && isAuthenticated.value) {
    return { name: 'home' }
  }
  if (to.meta.requiresAuth && !isAuthenticated.value) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }
})

export default router
