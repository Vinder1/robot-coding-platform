<template>
  <div id="app">
    <h1>Robot Coding Platform</h1>
    <p>Добро пожаловать в платформу!</p>

    <!-- Кнопки входа и регистрации -->
    <div class="auth-buttons" v-if="!authStore.isLoggedIn">
      <button @click="authStore.showLoginOverlay = true" class="btn btn-login">Войти</button>
      <button @click="authStore.showRegisterOverlay = true" class="btn btn-register">Зарегистрироваться</button>
    </div>

    <LogInOverlay/>
    <SignUpOverlay/>
    <LogOut/>
  </div>
</template>

<script setup lang="ts">
import SignUpOverlay from './components/SignUpOverlay.vue'
import LogInOverlay from './components/LogInOverlay.vue'
import { useAuthStore } from './stores/authStore.ts'
import LogOut from './components/LogOut.vue';

const authStore = useAuthStore()
authStore.initialize()
</script>

<style scoped>
#app {
  font-family: Arial, sans-serif;
  text-align: center;
  padding: 40px;
  max-width: 800px;
  margin: 0 auto;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

h1 {
  color: #333;
  margin-bottom: 20px;
}

p {
  color: #666;
  line-height: 1.6;
  margin-bottom: 30px;
}

/* Кнопки */
.auth-buttons {
  display: flex;
  gap: 20px;
  justify-content: center;
  margin-top: 30px;
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

.btn-primary {
  background-color: #007bff;
  color: white;
}

.btn-primary:hover {
  background-color: #0056b3;
  transform: translateY(-2px);
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

@media (max-width: 600px) {
  .auth-buttons {
    flex-direction: column;
  }

  .btn {
    width: 100%;
  }
}
</style>
