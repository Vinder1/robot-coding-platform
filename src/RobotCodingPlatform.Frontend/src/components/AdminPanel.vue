<template>
  <div class="admin-panel-card">
    <div class="card-header">
      <h2>Админ-панель</h2>
      <span class="endpoint-badge">api/admin/users</span>
    </div>

    <form class="filters" @submit.prevent="applySearch">
      <div class="search-row">
        <input
          v-model.trim="search"
          type="text"
          placeholder="Поиск по имени или email"
          :disabled="isListLoading"
        />
        <button type="submit" class="btn btn-primary" :disabled="isListLoading">
          Найти
        </button>
        <button type="button" class="btn btn-secondary" :disabled="isListLoading" @click="resetFilters">
          Сбросить
        </button>
      </div>

      <label class="checkbox-row">
        <input
          v-model="includeDeleted"
          type="checkbox"
          :disabled="isListLoading"
          @change="onFiltersChanged"
        />
        Показывать удалённых
      </label>
    </form>

    <div v-if="listError" class="error-message">{{ listError }}</div>

    <div class="table-wrapper">
      <table class="users-table">
        <thead>
          <tr>
            <th>Пользователь</th>
            <th>Email</th>
            <th>Роль</th>
            <th>Статус</th>
            <th>Регистрация</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="isListLoading && users.length === 0">
            <td colspan="6" class="table-empty">Загрузка...</td>
          </tr>
          <tr v-else-if="users.length === 0">
            <td colspan="6" class="table-empty">Пользователи не найдены.</td>
          </tr>
          <tr
            v-for="item in users"
            :key="item.id"
            class="user-row"
            :class="{ selected: user?.id === item.id, deleted: !!item.deletedAt }"
            @click="selectUser(item)"
          >
            <td class="cell-name">
              <div class="mini-avatar">{{ initialOf(item.name) }}</div>
              <span class="name-text">{{ item.name || 'Без имени' }}</span>
            </td>
            <td class="cell-email">{{ item.email }}</td>
            <td>
              <span class="role-badge">{{ formatRole(item.role) }}</span>
            </td>
            <td>
              <span :class="item.deletedAt ? 'deleted-badge' : 'active-badge'">
                {{ item.deletedAt ? 'Удалён' : 'Активен' }}
              </span>
            </td>
            <td class="cell-date">{{ formatDate(item.createdAt) }}</td>
            <td class="cell-actions">
              <button type="button" class="btn btn-small" @click.stop="selectUser(item)">
                Открыть
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="pagination">
      <span class="pagination-info">{{ rangeLabel }}</span>

      <div class="pagination-controls">
        <button
          type="button"
          class="btn btn-page"
          :disabled="page <= 1 || isListLoading"
          @click="goToPage(page - 1)"
        >
          ‹ Назад
        </button>

        <span class="page-indicator">
          Стр. {{ page }} из {{ Math.max(totalPages, 1) }}
        </span>

        <button
          type="button"
          class="btn btn-page"
          :disabled="page >= totalPages || isListLoading"
          @click="goToPage(page + 1)"
        >
          Вперёд ›
        </button>

        <select
          v-model.number="pageSize"
          class="page-size"
          :disabled="isListLoading"
          @change="onPageSizeChanged"
        >
          <option :value="10">10 / стр.</option>
          <option :value="20">20 / стр.</option>
          <option :value="50">50 / стр.</option>
          <option :value="100">100 / стр.</option>
        </select>
      </div>
    </div>

    <div v-if="errorMessage" class="error-message">{{ errorMessage }}</div>
    <div v-if="successMessage" class="success-message">{{ successMessage }}</div>

    <div v-if="user" class="user-card">
      <div class="user-card-header">
        <div class="avatar">{{ initialOf(user.name) }}</div>
        <div class="user-title">
          <h3>{{ user.name || 'Без имени' }}</h3>
          <div class="badges">
            <span class="role-badge">{{ formatRole(user.role) }}</span>
            <span v-if="user.deletedAt" class="deleted-badge">Удалён</span>
            <span v-else class="active-badge">Активен</span>
          </div>
        </div>
        <button type="button" class="btn-close" title="Закрыть" @click="user = null">×</button>
      </div>

      <div class="profile-details">
        <div class="detail-row">
          <span class="detail-label">Email</span>
          <span class="detail-value">{{ user.email || 'Не указан' }}</span>
        </div>

        <div class="detail-row">
          <span class="detail-label">ID пользователя</span>
          <span class="detail-value mono">{{ user.id }}</span>
        </div>

        <div class="detail-row">
          <span class="detail-label">Дата регистрации</span>
          <span class="detail-value">{{ formatDate(user.createdAt) }}</span>
        </div>

        <div class="detail-row">
          <span class="detail-label">Последнее обновление</span>
          <span class="detail-value">{{ formatDate(user.updatedAt) }}</span>
        </div>

        <div v-if="user.deletedAt" class="detail-row">
          <span class="detail-label">Дата удаления</span>
          <span class="detail-value">{{ formatDate(user.deletedAt) }}</span>
        </div>
      </div>

      <div class="actions">
        <div class="action-block">
          <label for="admin-new-role">Новая роль</label>
          <div class="role-row">
            <select id="admin-new-role" v-model="selectedRole" :disabled="isLoading">
              <option value="User">Пользователь</option>
              <option value="Admin">Администратор</option>
            </select>
            <button
              type="button"
              class="btn btn-primary"
              :disabled="isLoading || selectedRole === user.role"
              @click="changeRole"
            >
              Сменить роль
            </button>
          </div>
        </div>

        <button
          v-if="user.deletedAt"
          type="button"
          class="btn btn-restore"
          :disabled="isLoading"
          @click="restoreUser"
        >
          Восстановить пользователя
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import axios from 'axios';
import { api } from '@/api/api';
import { useAuthStore } from '@/stores/authStore';

interface AdminUser {
  id: string;
  name: string;
  email: string;
  role: string;
  createdAt: string;
  updatedAt: string | null;
  deletedAt: string | null;
}

interface AdminUsersPage {
  items: AdminUser[];
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
}

interface RestoreResponse {
  success: boolean;
  message: string;
  user: Omit<AdminUser, 'deletedAt'>;
}

const authStore = useAuthStore();

const users = ref<AdminUser[]>([]);
const page = ref(1);
const pageSize = ref(20);
const total = ref(0);
const totalPages = ref(0);
const search = ref('');
const includeDeleted = ref(false);
const isListLoading = ref(false);
const listError = ref('');

const user = ref<AdminUser | null>(null);
const selectedRole = ref('User');
const isLoading = ref(false);
const errorMessage = ref('');
const successMessage = ref('');

const rangeLabel = computed(() => {
  if (total.value === 0) return 'Нет пользователей';

  const from = (page.value - 1) * pageSize.value + 1;
  const to = Math.min(page.value * pageSize.value, total.value);
  return `Показано ${from}–${to} из ${total.value}`;
});

function initialOf(name: string | null | undefined): string {
  const trimmed = name?.trim();
  return trimmed ? trimmed.charAt(0).toUpperCase() : '?';
}

function formatRole(role: string): string {
  const roleMap: Record<string, string> = {
    Admin: 'Администратор',
    User: 'Пользователь',
    Viewer: 'Смотритель'
  };
  return roleMap[role] || role;
}

function formatDate(value: string | null | undefined): string {
  if (!value) return '—';

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;

  return new Intl.DateTimeFormat('ru-RU', {
    day: 'numeric',
    month: 'short',
    year: 'numeric'
  }).format(date);
}

async function loadUsers() {
  isListLoading.value = true;
  listError.value = '';

  try {
    const { data } = await api.get<AdminUsersPage>('/admin/users', {
      params: {
        page: page.value,
        pageSize: pageSize.value,
        search: search.value || undefined,
        includeDeleted: includeDeleted.value
      }
    });

    users.value = data.items ?? [];
    page.value = data.page;
    pageSize.value = data.pageSize;
    total.value = data.total;
    totalPages.value = data.totalPages;

    // Обновляем открытую карточку, если она есть в новом списке
    if (user.value) {
      const fresh = users.value.find((item) => item.id === user.value?.id);
      if (fresh) user.value = fresh;
    }
  } catch (error) {
    listError.value = describeError(error, 'Не удалось загрузить список пользователей.');
  } finally {
    isListLoading.value = false;
  }
}

function applySearch() {
  page.value = 1;
  void loadUsers();
}

function resetFilters() {
  search.value = '';
  includeDeleted.value = false;
  page.value = 1;
  void loadUsers();
}

function onFiltersChanged() {
  page.value = 1;
  void loadUsers();
}

function onPageSizeChanged() {
  page.value = 1;
  void loadUsers();
}

function goToPage(target: number) {
  if (target < 1 || (totalPages.value > 0 && target > totalPages.value)) return;

  page.value = target;
  void loadUsers();
}

function selectUser(item: AdminUser) {
  user.value = item;
  selectedRole.value = item.role;
  errorMessage.value = '';
  successMessage.value = '';
}

async function changeRole() {
  if (!user.value) return;

  isLoading.value = true;
  errorMessage.value = '';
  successMessage.value = '';

  try {
    await api.post(`/admin/users/change-role/${user.value.id}`, { role: selectedRole.value });

    const newRole = selectedRole.value;
    user.value.role = newRole;

    const row = users.value.find((item) => item.id === user.value?.id);
    if (row) row.role = newRole;

    // Если админ сменил роль самому себе — обновляем и локальный профиль
    if (user.value.id === authStore.user.user.id) {
      authStore.user.user.role = newRole;
    }

    successMessage.value = `Роль изменена на «${formatRole(newRole)}».`;
  } catch (error) {
    errorMessage.value = describeError(error, 'Не удалось изменить роль.');
  } finally {
    isLoading.value = false;
  }
}

async function restoreUser() {
  if (!user.value) return;

  isLoading.value = true;
  errorMessage.value = '';
  successMessage.value = '';

  try {
    const { data } = await api.post<RestoreResponse>(`/admin/users/restore/${user.value.id}`);
    user.value = { ...user.value, ...data.user, deletedAt: null };

    const row = users.value.find((item) => item.id === user.value?.id);
    if (row) {
      row.deletedAt = null;
      row.updatedAt = data.user.updatedAt;
    }

    successMessage.value = 'Пользователь восстановлен.';
  } catch (error) {
    errorMessage.value = describeError(error, 'Не удалось восстановить пользователя.');
  } finally {
    isLoading.value = false;
  }
}

function describeError(error: unknown, fallback: string): string {
  if (axios.isAxiosError(error)) {
    const status = error.response?.status;

    if (status === 404) return 'Пользователь с таким ID не найден.';
    if (status === 403) return 'Действие запрещено: недостаточно прав или пользователь не найден.';
    if (status === 401) return 'Требуется авторизация. Пожалуйста, войдите в систему.';

    return error.response?.data?.message || error.response?.data?.error || fallback;
  }

  if (error instanceof Error) return error.message;
  return fallback;
}

onMounted(loadUsers);
</script>

<style scoped>
.admin-panel-card {
  font-family: Arial, sans-serif;
  background-color: white;
  padding: 40px;
  border-radius: 16px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
  max-width: 960px;
  width: 92%;
  margin: 0 auto;
  text-align: left;
  box-sizing: border-box;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 2px solid #f0f0f0;
  flex-wrap: wrap;
  gap: 10px;
}

.card-header h2 {
  margin: 0;
  color: #333;
  font-size: 22px;
}

.endpoint-badge {
  font-family: monospace;
  font-size: 12px;
  background-color: #e9ecef;
  color: #555;
  padding: 5px 10px;
  border-radius: 6px;
  font-weight: 600;
}

/* Поиск и фильтры */
.filters {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-bottom: 20px;
}

.search-row {
  display: flex;
  gap: 10px;
}

.search-row input {
  flex: 1;
  padding: 10px 12px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 14px;
  transition: border-color 0.2s ease;
}

.search-row input:focus {
  outline: none;
  border-color: #007bff;
}

.checkbox-row {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  color: #555;
  cursor: pointer;
  user-select: none;
}

.checkbox-row input {
  width: 16px;
  height: 16px;
  cursor: pointer;
}

/* Таблица */
.table-wrapper {
  overflow-x: auto;
  border: 1px solid #e9ecef;
  border-radius: 12px;
  margin-bottom: 16px;
}

.users-table {
  width: 100%;
  min-width: 720px;
  border-collapse: collapse;
  font-size: 14px;
}

.users-table th {
  text-align: left;
  padding: 12px 14px;
  background-color: #f8f9fa;
  color: #666;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.4px;
  border-bottom: 1px solid #e9ecef;
  white-space: nowrap;
}

.users-table td {
  padding: 10px 14px;
  border-bottom: 1px solid #f0f0f0;
  vertical-align: middle;
}

.users-table tbody tr:last-child td {
  border-bottom: none;
}

.user-row {
  cursor: pointer;
  transition: background-color 0.15s ease;
}

.user-row:hover {
  background-color: #f8f9fa;
}

.user-row.selected {
  background-color: #eaf3ff;
}

.user-row.deleted .name-text,
.user-row.deleted .cell-email {
  color: #999;
}

.cell-name {
  display: flex;
  align-items: center;
  gap: 10px;
  white-space: nowrap;
}

.name-text {
  font-weight: 500;
  color: #333;
}

.cell-email {
  color: #555;
  max-width: 220px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.cell-date {
  color: #666;
  white-space: nowrap;
}

.cell-actions {
  text-align: right;
}

.mini-avatar {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  background-color: #007bff;
  color: white;
  font-size: 13px;
  font-weight: bold;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.table-empty {
  text-align: center;
  padding: 24px 14px !important;
  color: #888;
  font-style: italic;
}

/* Пагинация */
.pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
  margin-bottom: 24px;
}

.pagination-info {
  font-size: 13px;
  color: #666;
}

.pagination-controls {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.page-indicator {
  font-size: 13px;
  color: #555;
  white-space: nowrap;
}

.page-size {
  padding: 7px 10px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 13px;
  background-color: white;
  cursor: pointer;
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

/* Карточка выбранного пользователя */
.user-card {
  padding: 20px;
  background-color: #f8f9fa;
  border: 1px solid #e9ecef;
  border-radius: 12px;
}

.user-card-header {
  display: flex;
  align-items: center;
  gap: 14px;
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 1px solid #e9ecef;
}

.avatar {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background-color: #007bff;
  color: white;
  font-size: 20px;
  font-weight: bold;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.user-title {
  flex: 1;
  min-width: 0;
}

.user-title h3 {
  margin: 0 0 6px 0;
  font-size: 18px;
  color: #333;
  word-break: break-word;
}

.btn-close {
  align-self: flex-start;
  background: none;
  border: none;
  color: #999;
  font-size: 24px;
  line-height: 1;
  cursor: pointer;
  padding: 0 4px;
}

.btn-close:hover {
  color: #555;
}

.badges {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

.role-badge,
.deleted-badge,
.active-badge {
  display: inline-block;
  padding: 3px 9px;
  border-radius: 6px;
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  white-space: nowrap;
}

.role-badge {
  background-color: #e9ecef;
  color: #555;
}

.active-badge {
  background-color: #d4edda;
  color: #155724;
}

.deleted-badge {
  background-color: #f8d7da;
  color: #721c24;
}

/* Детали */
.profile-details {
  display: flex;
  flex-direction: column;
  gap: 14px;
  margin-bottom: 20px;
}

.detail-row {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.detail-label {
  font-size: 12px;
  color: #666;
  font-weight: 500;
}

.detail-value {
  font-size: 14px;
  color: #333;
  font-weight: 500;
  word-break: break-all;
}

.detail-value.mono {
  font-family: 'Consolas', 'Monaco', monospace;
  font-size: 13px;
  color: #555;
}

/* Действия */
.actions {
  display: flex;
  flex-direction: column;
  gap: 14px;
  padding-top: 16px;
  border-top: 1px solid #e9ecef;
}

.action-block {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.role-row {
  display: flex;
  gap: 10px;
}

.role-row select {
  flex: 1;
  padding: 10px 12px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 14px;
  background-color: white;
  cursor: pointer;
}

.role-row select:focus {
  outline: none;
  border-color: #007bff;
}

/* Кнопки */
.btn {
  padding: 10px 20px;
  font-size: 14px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s ease;
  font-weight: 500;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-primary {
  background-color: #007bff;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background-color: #0056b3;
}

.btn-secondary {
  background-color: #e9ecef;
  color: #333;
}

.btn-secondary:hover:not(:disabled) {
  background-color: #dde2e6;
}

.btn-page {
  background-color: white;
  color: #333;
  border: 1px solid #dee2e6;
  padding: 7px 12px;
  font-size: 13px;
}

.btn-page:hover:not(:disabled) {
  background-color: #f1f3f5;
}

.btn-small {
  padding: 6px 12px;
  font-size: 13px;
  background-color: #e9ecef;
  color: #333;
}

.btn-small:hover {
  background-color: #dde2e6;
}

.btn-restore {
  width: 100%;
  background-color: #28a745;
  color: white;
}

.btn-restore:hover:not(:disabled) {
  background-color: #1e7e34;
}

@media (max-width: 640px) {
  .admin-panel-card {
    padding: 24px;
  }

  .search-row,
  .role-row {
    flex-direction: column;
  }

  .search-row .btn {
    width: 100%;
  }

  .pagination {
    flex-direction: column;
    align-items: stretch;
  }

  .pagination-controls {
    justify-content: space-between;
  }
}
</style>
