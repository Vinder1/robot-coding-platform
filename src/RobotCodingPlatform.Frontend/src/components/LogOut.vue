<template>
  <button v-if="authStore.isLoggedIn" @click="handleLogout" class="btn btn-primary">Выйти</button>
</template>

<script setup lang="ts">
import { api } from '@/api/api';
import { auth } from '@/api/auth';
import { useAuthStore } from '@/stores/authStore';

const authStore = useAuthStore();

const handleLogout = async () => {
  try {
    await api.post('/auth/logout')
    authStore.reset()
    auth.clear()
    console.log('Выход выполнен')
  } catch (error) {
    console.error('Ошибка при выходе:', error)
    authStore.reset()
    auth.clear()
  }
}
</script>
