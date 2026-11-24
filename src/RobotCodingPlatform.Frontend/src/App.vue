<template>
  <div id="app">
    <header class="app-header">
      <h1>Robot Coding Platform</h1>
      <p>Добро пожаловать в платформу!</p>
    </header>

    <!-- Пока профиль загружается, не показываем вкладки, зависящие от роли -->
    <div v-if="!authStore.isInitialized" class="loading-state">Загрузка профиля...</div>

    <!-- Кнопки входа и регистрации -->
    <div v-else-if="!authStore.isLoggedIn" class="auth-buttons">
      <button @click="authStore.showLoginOverlay = true" class="btn btn-login">Войти</button>
      <button @click="authStore.showRegisterOverlay = true" class="btn btn-register">Зарегистрироваться</button>
    </div>

    <!-- Переключение окон -->
    <template v-else>
      <nav class="window-tabs" role="tablist" aria-label="Окна платформы">
        <button
          type="button"
          role="tab"
          class="window-tab"
          :class="{ active: activeTab === 'code' }"
          :aria-selected="activeTab === 'code'"
          @click="openFreeCode"
        >
          Редактор кода
        </button>
        <button
          type="button"
          role="tab"
          class="window-tab"
          :class="{ active: activeTab === 'profile' }"
          :aria-selected="activeTab === 'profile'"
          @click="activeTab = 'profile'"
        >
          Профиль
        </button>
        <button
          type="button"
          role="tab"
          class="window-tab"
          :class="{ active: activeTab === 'challenges' }"
          :aria-selected="activeTab === 'challenges'"
          @click="activeTab = 'challenges'"
        >
          Поиск challenge
        </button>
        <button
          v-if="authStore.isAdmin"
          type="button"
          role="tab"
          class="window-tab"
          :class="{ active: activeTab === 'create-challenge' }"
          :aria-selected="activeTab === 'create-challenge'"
          @click="activeTab = 'create-challenge'"
        >
          Создание challenge
        </button>
        <button
          v-if="authStore.isAdmin"
          type="button"
          role="tab"
          class="window-tab"
          :class="{ active: activeTab === 'admin' }"
          :aria-selected="activeTab === 'admin'"
          @click="activeTab = 'admin'"
        >
          Админ-панель
        </button>
      </nav>

      <main class="window">
        <CodeRunner
          v-show="activeTab === 'code'"
          :challenge="activeChallenge"
          @exit="exitChallenge"
          @submitted="onSolutionSubmitted"
        />
        <UserInfo v-show="activeTab === 'profile'" />
        <ChallengeSearch
          v-show="activeTab === 'challenges'"
          :refresh-token="challengeRefreshToken"
          @try="onTryChallenge"
        />
        <ChallengeCreate
          v-if="authStore.isAdmin"
          v-show="activeTab === 'create-challenge'"
          @created="onChallengeCreated"
        />
        <AdminPanel v-if="authStore.isAdmin" v-show="activeTab === 'admin'" />
      </main>
    </template>

    <LogInOverlay />
    <SignUpOverlay />
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import SignUpOverlay from './components/SignUpOverlay.vue';
import LogInOverlay from './components/LogInOverlay.vue';
import UserInfo from './components/UserInfo.vue';
import AdminPanel from './components/AdminPanel.vue';
import CodeRunner from './components/CodeRunner.vue';
import ChallengeSearch from './components/ChallengeSearch.vue';
import ChallengeCreate from './components/ChallengeCreate.vue';
import { useAuthStore } from './stores/authStore.ts';
import type { Challenge } from './types/challenge';

const authStore = useAuthStore();
authStore.initialize();

type WindowTab = 'code' | 'profile' | 'challenges' | 'create-challenge' | 'admin';
const activeTab = ref<WindowTab>('code');
const challengeRefreshToken = ref(0);
const activeChallenge = ref<Challenge | null>(null);

function onChallengeCreated() {
  challengeRefreshToken.value += 1;
  activeTab.value = 'challenges';
}

// После отправки решения обновляем список попыток в карточке испытания
function onSolutionSubmitted() {
  challengeRefreshToken.value += 1;
}

function onTryChallenge(challenge: Challenge) {
  activeChallenge.value = challenge;
  activeTab.value = 'code';
}

function exitChallenge() {
  activeChallenge.value = null;
  activeTab.value = 'challenges';
}

// Клик по вкладке «Редактор кода» возвращает к свободному запуску кода
function openFreeCode() {
  activeChallenge.value = null;
  activeTab.value = 'code';
}

// После выхода из аккаунта возвращаемся на вкладку с редактором
watch(
  () => authStore.isLoggedIn,
  (isLoggedIn) => {
    if (!isLoggedIn) {
      activeChallenge.value = null;
      activeTab.value = 'code';
    }
  }
);

// Если роль администратора сняли, закрываем админ-панель и создание challenge
watch(
  () => authStore.isAdmin,
  (isAdminNow) => {
    if (!isAdminNow && (activeTab.value === 'admin' || activeTab.value === 'create-challenge')) {
      activeTab.value = 'code';
    }
  }
);
</script>

<style scoped>
#app {
  font-family: Arial, sans-serif;
  text-align: center;
  padding: 40px;
  max-width: 1200px;
  margin: 0 auto;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  box-sizing: border-box;
}

.app-header h1 {
  color: #333;
  margin: 0 0 20px 0;
}

.app-header p {
  color: #666;
  line-height: 1.6;
  margin: 0 0 30px 0;
}

/* Кнопки */
.auth-buttons {
  display: flex;
  gap: 20px;
  justify-content: center;
  margin-top: 30px;
}

.loading-state {
  margin-top: 30px;
  color: #777;
  font-size: 15px;
}

.btn {
  padding: 12px 30px;
  font-size: 16px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
  font-weight: 500;
}

.btn-login {
  background-color: #28a745;
  color: white;
}

.btn-login:hover {
  background-color: #1e7e34;
}

.btn-register {
  background-color: #ffc107;
  color: #333;
}

.btn-register:hover {
  background-color: #e0a800;
}

/* Переключение окон */
.window-tabs {
  display: inline-flex;
  gap: 4px;
  padding: 4px;
  background-color: #eef1f5;
  border-radius: 12px;
  margin-bottom: 24px;
}

.window-tab {
  padding: 10px 24px;
  border: none;
  border-radius: 9px;
  background-color: transparent;
  color: #555;
  font-family: inherit;
  font-size: 15px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.window-tab:hover {
  color: #007bff;
}

.window-tab.active {
  background-color: white;
  color: #007bff;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.12);
}

.window {
  display: flex;
  justify-content: center;
  width: 100%;
}

@media (max-width: 600px) {
  #app {
    padding: 24px 16px;
  }

  .auth-buttons {
    flex-direction: column;
    width: 100%;
  }

  .btn {
    width: 100%;
  }

  .window-tabs {
    display: flex;
    width: 100%;
  }

  .window-tab {
    flex: 1;
    padding: 10px 12px;
  }
}
</style>
