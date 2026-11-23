<template>
  <div v-if="authStore.isLoggedIn && user.hasUser" class="user-info-card">
    <div class="card-header">
      <div class="avatar">{{ userInitial }}</div>
      <div class="title-group">
        <!-- Режим просмотра имени -->
        <div v-if="!isEditingName" class="name-row">
          <h2>{{ player.name || 'Пользователь' }}</h2>
          <button
            type="button"
            class="btn-icon"
            title="Изменить имя"
            @click="startEditingName"
          >
            ✎
          </button>
        </div>

        <!-- Режим редактирования имени -->
        <form v-else class="edit-name-form" @submit.prevent="saveName">
          <input
            v-model="newName"
            type="text"
            class="name-input"
            placeholder="Новое имя"
            required
            :disabled="isLoading"
          />
          <div class="edit-actions">
            <button type="submit" class="btn-small btn-save" :disabled="isLoading">
              Сохранить
            </button>
            <button
              type="button"
              class="btn-small btn-cancel"
              :disabled="isLoading"
              @click="cancelEditingName"
            >
              Отмена
            </button>
          </div>
        </form>

        <span v-if="player.role" class="role-badge">{{ formatRole(player.role) }}</span>
      </div>
    </div>

    <!-- Сообщения об ошибках / успехе -->
    <div v-if="errorMessage" class="error-message">{{ errorMessage }}</div>
    <div v-if="successMessage" class="success-message">{{ successMessage }}</div>

    <!-- Информация о пользователе -->
    <div class="profile-details">
      <div class="detail-row">
        <span class="detail-label">Email</span>
        <span class="detail-value">{{ player.email || 'Не указан' }}</span>
      </div>

      <div class="detail-row">
        <span class="detail-label">ID пользователя</span>
        <span class="detail-value mono">{{ player.id }}</span>
      </div>

      <div class="detail-row">
        <span class="detail-label">Дата регистрации</span>
        <span class="detail-value">{{ formattedCreatedAt }}</span>
      </div>
    </div>

    <!-- Кнопки действий -->
    <div class="profile-actions">
      <LogOut />

      <!-- Обычная кнопка удаления -->
      <button
        v-if="!confirmDelete"
        type="button"
        class="btn btn-danger-outline"
        :disabled="isLoading"
        @click="confirmDelete = true"
      >
        Удалить аккаунт
      </button>

      <!-- Подтверждение удаления -->
      <div v-else class="delete-confirm-box">
        <p class="delete-warning">Вы уверены? Это действие нельзя отменить.</p>
        <div class="delete-confirm-buttons">
          <button
            type="button"
            class="btn btn-danger"
            :disabled="isLoading"
            @click="handleDeleteAccount"
          >
            {{ isLoading ? 'Удаление...' : 'Да, удалить' }}
          </button>
          <button
            type="button"
            class="btn btn-secondary"
            :disabled="isLoading"
            @click="confirmDelete = false"
          >
            Отмена
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import axios from 'axios';
import { useAuthStore } from '@/stores/authStore';
import { auth } from '@/api/auth';
import { api } from '@/api/api';
import LogOut from './LogOut.vue';

const authStore = useAuthStore();

const isEditingName = ref(false);
const newName = ref('');
const confirmDelete = ref(false);
const isLoading = ref(false);
const errorMessage = ref('');
const successMessage = ref('');

const user = computed(() => authStore.user);
const player = computed(() => authStore.user.user);

const userInitial = computed(() => {
  const name = player.value.name?.trim();
  return name ? name.charAt(0).toUpperCase() : '?';
});

const formattedCreatedAt = computed(() => {
  if (!player.value.createdAt) return 'Не указана';

  const date = new Date(player.value.createdAt);
  if (Number.isNaN(date.getTime())) return player.value.createdAt;

  return new Intl.DateTimeFormat('ru-RU', {
    day: 'numeric',
    month: 'long',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(date);
});

function formatRole(role: string): string {
  const roleMap: Record<string, string> = {
    Admin: 'Администратор',
    User: 'Пользователь',
    Viewer: 'Смотритель'
  };
  return roleMap[role] || role;
}

function startEditingName() {
  newName.value = player.value.name;
  isEditingName.value = true;
  errorMessage.value = '';
  successMessage.value = '';
}

function cancelEditingName() {
  isEditingName.value = false;
  newName.value = '';
}

async function saveName() {
  const trimmed = newName.value.trim();
  if (!trimmed || trimmed === player.value.name) {
    cancelEditingName();
    return;
  }

  isLoading.value = true;
  errorMessage.value = '';
  successMessage.value = '';

  try {
    await api.post('/me/update-name', { name: trimmed });
    player.value.name = trimmed;
    isEditingName.value = false;
    successMessage.value = 'Имя успешно обновлено';
  } catch (error) {
    errorMessage.value = extractError(error, 'Не удалось изменить имя');
  } finally {
    isLoading.value = false;
  }
}

async function handleDeleteAccount() {
  isLoading.value = true;
  errorMessage.value = '';

  try {
    await api.delete('/me/delete');
    authStore.reset();
    auth.clear();
  } catch (error) {
    errorMessage.value = extractError(error, 'Не удалось удалить аккаунт');
    confirmDelete.value = false;
  } finally {
    isLoading.value = false;
  }
}

function extractError(error: unknown, fallback: string): string {
  if (axios.isAxiosError(error)) {
    return error.response?.data?.error || error.response?.data?.message || fallback;
  }
  if (error instanceof Error) {
    return error.message;
  }
  return fallback;
}
</script>

<style scoped>
.user-info-card {
  font-family: Arial, sans-serif;
  background-color: white;
  padding: 40px;
  border-radius: 16px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
  max-width: 760px;
  width: 92%;
  margin: 0 auto;
  text-align: left;
  box-sizing: border-box;
}

.card-header {
  display: flex;
  align-items: flex-start;
  gap: 16px;
  margin-bottom: 24px;
  padding-bottom: 20px;
  border-bottom: 2px solid #f0f0f0;
}

.avatar {
  width: 56px;
  height: 56px;
  border-radius: 50%;
  background-color: #007bff;
  color: white;
  font-size: 24px;
  font-weight: bold;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.title-group {
  flex: 1;
  min-width: 0;
}

.name-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 6px;
}

.title-group h2 {
  margin: 0;
  color: #333;
  font-size: 20px;
  word-break: break-word;
}

.btn-icon {
  background: none;
  border: 1px solid #e0e0e0;
  border-radius: 6px;
  color: #555;
  cursor: pointer;
  padding: 2px 8px;
  font-size: 14px;
  transition: all 0.2s ease;
}

.btn-icon:hover {
  background-color: #f0f0f0;
  color: #007bff;
  border-color: #007bff;
}

/* Форма редактирования имени */
.edit-name-form {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 8px;
}

.name-input {
  padding: 8px 12px;
  border: 2px solid #007bff;
  border-radius: 8px;
  font-size: 14px;
  width: 100%;
  box-sizing: border-box;
}

.name-input:focus {
  outline: none;
}

.edit-actions {
  display: flex;
  gap: 8px;
}

.btn-small {
  padding: 6px 12px;
  font-size: 13px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
}

.btn-save {
  background-color: #28a745;
  color: white;
}

.btn-save:hover {
  background-color: #1e7e34;
}

.btn-cancel {
  background-color: #e9ecef;
  color: #333;
}

.btn-cancel:hover {
  background-color: #dde2e6;
}

.role-badge {
  display: inline-block;
  padding: 4px 10px;
  background-color: #e9ecef;
  color: #555;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
}

/* Уведомления */
.error-message {
  margin-bottom: 20px;
  padding: 12px;
  background-color: #f8d7da;
  color: #721c24;
  border-radius: 8px;
  font-size: 14px;
}

.success-message {
  margin-bottom: 20px;
  padding: 12px;
  background-color: #d4edda;
  color: #155724;
  border-radius: 8px;
  font-size: 14px;
}

/* Детали профиля */
.profile-details {
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-bottom: 30px;
}

.detail-row {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.detail-label {
  font-size: 13px;
  color: #666;
  font-weight: 500;
}

.detail-value {
  font-size: 15px;
  color: #333;
  font-weight: 500;
  word-break: break-all;
}

.detail-value.mono {
  font-family: 'Consolas', 'Monaco', monospace;
  font-size: 13px;
  color: #555;
}

/* Основные кнопки */
.profile-actions {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.btn {
  width: 100%;
  padding: 12px 24px;
  font-size: 15px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
  font-weight: 500;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-secondary {
  background-color: #e9ecef;
  color: #333;
}

.btn-secondary:hover:not(:disabled) {
  background-color: #dde2e6;
}

.btn-danger-outline {
  background-color: transparent;
  color: #dc3545;
  border: 2px solid #dc3545;
}

.btn-danger-outline:hover:not(:disabled) {
  background-color: #dc3545;
  color: white;
}

.btn-danger {
  background-color: #dc3545;
  color: white;
}

.btn-danger:hover:not(:disabled) {
  background-color: #c82333;
}

.delete-warning {
  margin: 0 0 10px 0;
  font-size: 14px;
  color: #721c24;
}

.delete-confirm-box {
  padding: 14px;
  background-color: #f8d7da;
  border: 1px solid #f5c6cb;
  border-radius: 10px;
}

.delete-confirm-buttons {
  display: flex;
  gap: 8px;
}

@media (max-width: 640px) {
  .user-info-card {
    padding: 24px;
  }
}
</style>
