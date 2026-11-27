<template>
  <div class="rating-block">
    <div class="rating-header">
      <span class="detail-label">Рейтинг испытания</span>
      <button type="button" class="btn-refresh" :disabled="isLoading" @click="loadRating()">
        {{ isLoading ? 'Загрузка...' : 'Обновить' }}
      </button>
    </div>

    <div v-if="errorMessage" class="rating-error">{{ errorMessage }}</div>

    <div v-if="isLoading && rating === null" class="rating-empty">Загрузка рейтинга...</div>
    <div v-else-if="!rating || rating.entries.length === 0" class="rating-empty">
      Пока нет успешных решений. Станьте первым!
    </div>
    <template v-else>
      <p class="rating-summary">
        Участников с успешным решением: <strong>{{ rating.totalParticipants }}</strong>
        <span v-if="rating.totalParticipants > rating.entries.length">
          · показаны первые {{ rating.entries.length }}
        </span>
      </p>

      <div class="table-wrapper">
        <table class="rating-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Пользователь</th>
              <th>Итераций</th>
              <th>Завершено</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="entry in rating.entries" :key="entry.userId">
              <td class="place">{{ entry.place }}</td>
              <td class="user">{{ entry.userName }}</td>
              <td class="iterations">{{ entry.iterations }}</td>
              <td class="when">
                {{ entry.completedAt ? formatDate(entry.completedAt) : '—' }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import axios from 'axios';
import { api } from '@/api/api';

interface ChallengeRatingEntry {
  place: number;
  userId: string;
  userName: string;
  iterations: number;
  completedAt: string | null;
}

interface ChallengeRating {
  challengeId: string;
  challengeName: string;
  totalParticipants: number;
  entries: ChallengeRatingEntry[];
}

const props = defineProps<{
  challengeId: string;
  refreshToken?: number;
}>();

const rating = ref<ChallengeRating | null>(null);
const isLoading = ref(false);
const errorMessage = ref('');

async function loadRating(silent = false) {
  if (!silent) isLoading.value = true;
  errorMessage.value = '';

  try {
    const { data } = await api.get<ChallengeRating>(
      `/challenges/${props.challengeId}/rating`,
      { params: { limit: 10 } },
    );

    rating.value = data;
  } catch (error) {
    errorMessage.value = describeError(error, 'Не удалось загрузить рейтинг.');
  } finally {
    if (!silent) isLoading.value = false;
  }
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
    if (error.response?.status === 404) return 'Испытание не найдено.';
    if (error.response?.status === 401) return 'Требуется авторизация. Пожалуйста, войдите в систему.';

    return error.response?.data?.message || error.response?.data?.error || fallback;
  }

  if (error instanceof Error) return error.message;
  return fallback;
}

watch(
  () => props.challengeId,
  () => {
    rating.value = null;
    void loadRating();
  },
  { immediate: true },
);

watch(
  () => props.refreshToken,
  () => {
    void loadRating(true);
  },
);
</script>

<style scoped>
.rating-block {
  margin-top: 20px;
  padding-top: 16px;
  border-top: 1px solid #e9ecef;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.rating-header {
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

.rating-summary {
  margin: 0;
  font-size: 12px;
  color: #666;
}

.rating-empty {
  font-size: 13px;
  color: #888;
  font-style: italic;
}

.rating-error {
  padding: 10px 12px;
  background-color: #f8d7da;
  color: #721c24;
  border-radius: 8px;
  font-size: 13px;
}

.table-wrapper {
  overflow-x: auto;
}

.rating-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
  background-color: white;
  border: 1px solid #dee2e6;
  border-radius: 8px;
  overflow: hidden;
}

.rating-table th {
  text-align: left;
  padding: 8px 10px;
  background-color: #f1f3f5;
  color: #555;
  font-weight: 600;
  font-size: 12px;
  white-space: nowrap;
}

.rating-table td {
  padding: 8px 10px;
  border-top: 1px solid #edf0f2;
  color: #333;
}

.rating-table .place {
  width: 40px;
  font-weight: 700;
  color: #0b5ed7;
}

.rating-table .user {
  font-weight: 500;
  word-break: break-word;
}

.rating-table .iterations {
  font-family: 'Consolas', 'Monaco', monospace;
  font-weight: 700;
  color: #155724;
  white-space: nowrap;
}

.rating-table .when {
  font-size: 12px;
  color: #777;
  white-space: nowrap;
}
</style>
