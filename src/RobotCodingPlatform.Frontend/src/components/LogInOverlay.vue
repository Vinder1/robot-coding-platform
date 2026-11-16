<template>
  <div v-if="authStore.showLoginOverlay" class="overlay" @click.self="close">
    <div class="overlay-content login-overlay">
      <h2>Вход в систему</h2>

      <form @submit.prevent="handleLogin" class="auth-form">
        <div class="form-group">
          <label for="login-email">Email</label>
          <input id="login-email" v-model="loginForm.email" type="email" required placeholder="Введите ваш email" />
        </div>

        <div class="form-group">
          <label for="login-password">Пароль</label>
          <input id="login-password" v-model="loginForm.password" type="password" required
            placeholder="Введите пароль" />
        </div>

        <button type="submit" class="btn btn-primary">Войти</button>
        <button class="close" @click="close">&times;</button>
      </form>

      <div v-if="loginError" class="error-message">{{ loginError }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { api } from '@/api/api';
import { auth } from '@/api/auth';
import { useAuthStore } from '@/stores/authStore';
import axios from 'axios';
import { ref } from 'vue'

const authStore = useAuthStore();

const loginForm = ref({ email: '', password: '' })
const loginError = ref('')

function close() {
  authStore.showLoginOverlay = false;
  loginError.value = '';
  loginForm.value = { email: '', password: '' };
}

const handleLogin = async () => {
  try {
    const response = await api.post('/auth/login', {
      email: loginForm.value.email,
      password: loginForm.value.password
    })

    console.log('Вход выполнен успешно:', response.data)

    authStore.isLoggedIn = true
    authStore.user.user = response.data.user
    authStore.user.hasUser = true
    auth.setToken(response.data.newAccessToken.token);

    authStore.showLoginOverlay = false
    loginError.value = ''
    loginForm.value = { email: '', password: '' }
  } catch (error) {
    let errorMessage = 'Ошибка входа. Пожалуйста, проверьте свои данные.';
    if (axios.isAxiosError(error)) {
      errorMessage = error.response?.data?.error || errorMessage;
    } else if (error instanceof Error) {
      errorMessage = error.message;
    }

    loginError.value = errorMessage
    console.error('Ошибка входа:', errorMessage)
  }
}

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

/* Оверлеи */
.overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.overlay-content {
  background-color: white;
  padding: 40px;
  border-radius: 16px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
  max-width: 400px;
  width: 90%;
}

.overlay-content h2 {
  margin-top: 0;
  color: #333;
  text-align: center;
  margin-bottom: 30px;
}

/* Форма */
.auth-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-size: 14px;
  color: #555;
  font-weight: 500;
}

.form-group input {
  padding: 12px 16px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 14px;
  transition: border-color 0.3s ease;
}

.form-group input:focus {
  outline: none;
  border-color: #007bff;
}

.error-message {
  margin-top: 16px;
  padding: 12px;
  background-color: #f8d7da;
  color: #721c24;
  border-radius: 8px;
  font-size: 14px;
}

/* Адаптивность */
@media (max-width: 600px) {
  .auth-buttons {
    flex-direction: column;
  }

  .btn {
    width: 100%;
  }
}
</style>
