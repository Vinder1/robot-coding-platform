import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
      meta: {
        title: 'Robot Coding Platform',
        requiresAuth: false // Доступна без авторизации
      }
    },
  ],
})

// Обработка глобальных ошибок маршрутизации
router.onError((error) => {
  if (error.message?.includes('401')) {
    console.log('Авторизация не пройдена')
  }
})

export default router
