<template>
  <div class="attempts-block">
    <div class="attempts-header">
      <span class="detail-label">Мои попытки</span>
      <button type="button" class="btn-refresh" :disabled="isLoading" @click="loadAttempts()">
        {{ isLoading ? 'Загрузка...' : 'Обновить' }}
      </button>
    </div>

    <div v-if="errorMessage" class="attempts-error">{{ errorMessage }}</div>

    <div v-if="isLoading && attempts.length === 0" class="attempts-empty">Загрузка попыток...</div>
    <div v-else-if="attempts.length === 0" class="attempts-empty">
      Пока нет попыток. Отправьте решение из редактора кода.
    </div>
    <ul v-else class="attempts-list">
      <li v-for="attempt in attempts" :key="attempt.id" class="attempt-item">
        <div class="attempt-top">
          <span class="attempt-status" :class="statusClass(attempt.status)">
            {{ statusLabel(attempt.status) }}
          </span>
          <span class="attempt-date">{{ formatDate(attempt.createdAt) }}</span>
        </div>

        <div class="attempt-result" :class="resultClass(attempt)">
          {{ resultText(attempt) }}
        </div>

        <p v-if="attempt.error" class="attempt-error-text">{{ attempt.error }}</p>

        <div class="attempt-meta">
          <span class="attempt-id mono">{{ shortId(attempt.id) }}</span>
          <span v-if="attempt.completedAt" class="attempt-completed">
            завершено {{ formatDate(attempt.completedAt) }}
          </span>
        </div>
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import { onUnmounted, ref, watch } from 'vue';
import axios from 'axios';
import { api } from '@/api/api';
import type { SolutionAttemptStatus, SolutionAttemptSummary } from '@/types/solution';

const props = defineProps<{
  challengeId: string;
  refreshToken?: number;
}>();

const attempts = ref<SolutionAttemptSummary[]>([]);
const isLoading = ref(false);
const errorMessage = ref('');

let pollTimer: ReturnType<typeof setTimeout> | null = null;

function clearPollTimer() {
  if (pollTimer !== null) {
    clearTimeout(pollTimer);
    pollTimer = null;
  }
}

async function loadAttempts(silent = false) {
  if (!silent) isLoading.value = true;
  errorMessage.value = '';

  try {
    const { data } = await api.get<SolutionAttemptSummary[]>('/solutions', {
      params: { challengeId: props.challengeId },
    });

    attempts.value = data ?? [];

    if (attempts.value.some((attempt) => attempt.status === 'Pending')) {
      schedulePoll();
    } else {
      clearPollTimer();
    }
  } catch (error) {
    errorMessage.value = describeError(error, 'Не удалось загрузить попытки.');
  } finally {
    if (!silent) isLoading.value = false;
  }
}

function schedulePoll() {
  if (pollTimer !== null) {
    return;
  }

  pollTimer = setTimeout(() => {
    pollTimer = null;
    void loadAttempts(true);
  }, 2000);
}

function statusClass(status: SolutionAttemptStatus) {
  return {
    pending: status === 'Pending',
    succeeded: status === 'Succeeded',
    failed: status === 'Failed',
  };
}

function statusLabel(status: SolutionAttemptStatus): string {
  switch (status) {
    case 'Pending':
      return 'Выполняется';
    case 'Succeeded':
      return 'Успех';
    case 'Failed':
      return 'Провал';
    default:
      return status;
  }
}

function resultClass(attempt: SolutionAttemptSummary) {
  if (attempt.status === 'Pending') return 'pending';
  return attempt.success ? 'success' : 'failure';
}

function resultText(attempt: SolutionAttemptSummary): string {
  if (attempt.status === 'Pending') {
    return 'Решение отправлено, ожидаем результат...';
  }

  if (attempt.success) {
    return 'Цель достигнута.';
  }

  return attempt.error ? 'Ошибка выполнения.' : 'Цель не достигнута.';
}

function shortId(id: string): string {
  return id.slice(0, 8);
}

function formatDate(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;

  return new Intl.DateTimeFormat('ru-RU', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(date);
}

function describeError(error: unknown, fallback: string): string {
  if (axios.isAxiosError(error)) {
    if (error.response?.status === 401) return 'Требуется авторизация. Пожалуйста, войдите в систему.';
    return error.response?.data?.message || error.response?.data?.error || fallback;
  }

  if (error instanceof Error) return error.message;
  return fallback;
}

watch(
  () => props.challengeId,
  () => {
    clearPollTimer();
    attempts.value = [];
    void loadAttempts();
  },
  { immediate: true },
);

watch(
  () => props.refreshToken,
  () => {
    void loadAttempts();
  },
);

onUnmounted(clearPollTimer);
</script>

<style scoped>
.attempts-block {
  margin-top: 20px;
  padding-top: 16px;
  border-top: 1px solid #e9ecef;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.attempts-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
}

.detail-label {
  font-size: 12px;
  color: #666;
  font-weight: 500;
}

.btn-refresh {
  padding: 6px 12px;
  font-size: 13px;
  border: none;
  border-radius: 7px;
  background-color: #e9ecef;
  color: #333;
  cursor: pointer;
  transition: background-color 0.15s ease;
}

.btn-refresh:hover:not(:disabled) {
  background-color: #dde2e6;
}

.btn-refresh:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.attempts-empty {
  font-size: 13px;
  color: #888;
  font-style: italic;
}

.attempts-error {
  padding: 10px 12px;
  background-color: #f8d7da;
  color: #721c24;
  border-radius: 8px;
  font-size: 13px;
}

.attempts-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
  max-height: 320px;
  overflow-y: auto;
}

.attempt-item {
  padding: 10px 12px;
  background-color: white;
  border: 1px solid #dee2e6;
  border-radius: 10px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.attempt-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.attempt-status {
  font-size: 12px;
  font-weight: 600;
  padding: 2px 9px;
  border-radius: 999px;
}

.attempt-status.pending {
  background-color: #e7f1ff;
  color: #0b5ed7;
}

.attempt-status.succeeded {
  background-color: #d4edda;
  color: #155724;
}

.attempt-status.failed {
  background-color: #f8d7da;
  color: #721c24;
}

.attempt-date {
  font-size: 12px;
  color: #777;
}

.attempt-result {
  font-size: 13px;
  font-weight: 500;
}

.attempt-result.pending {
  color: #0b5ed7;
}

.attempt-result.success {
  color: #155724;
}

.attempt-result.failure {
  color: #721c24;
}

.attempt-error-text {
  margin: 0;
  font-size: 12px;
  color: #721c24;
  background-color: #f8d7da;
  padding: 6px 8px;
  border-radius: 6px;
  white-space: pre-wrap;
  word-break: break-word;
}

.attempt-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  font-size: 11px;
  color: #888;
}

.mono {
  font-family: 'Consolas', 'Monaco', monospace;
}
</style>
