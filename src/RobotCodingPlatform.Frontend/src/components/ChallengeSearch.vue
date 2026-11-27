<template>
  <div class="challenge-search-card">
    <div class="card-header">
      <h2>Поиск challenge</h2>
      <span class="endpoint-badge">api/challenges</span>
    </div>

    <!-- Поиск по названию -->
    <div class="search-row">
      <input
        v-model.trim="search"
        type="text"
        placeholder="Поиск по названию"
        :disabled="isLoading"
      />
      <button type="button" class="btn btn-secondary" :disabled="isLoading" @click="loadChallenges">
        {{ isLoading ? 'Загрузка...' : 'Обновить' }}
      </button>
    </div>

    <div v-if="errorMessage" class="error-message">{{ errorMessage }}</div>

    <!-- Список -->
    <div v-if="isLoading && challenges.length === 0" class="empty-state">Загрузка...</div>
    <div v-else-if="filteredChallenges.length === 0" class="empty-state">
      {{ challenges.length === 0 ? 'Пока нет ни одного challenge.' : 'По вашему запросу ничего не найдено.' }}
    </div>
    <ul v-else class="challenge-list">
      <li
        v-for="item in filteredChallenges"
        :key="item.id"
        class="challenge-item"
        :class="{ selected: selected?.id === item.id }"
        @click="openChallenge(item)"
      >
        <div class="challenge-item-main">
          <h3>{{ item.name }}</h3>
          <div class="challenge-meta">
            <span class="meta-chip">Поле {{ item.width }} × {{ item.height }}</span>
            <span class="meta-chip">Старт ({{ item.startX }}, {{ item.startY }})</span>
            <span class="meta-chip">Цель ({{ item.goalX }}, {{ item.goalY }})</span>
            <span class="meta-chip">Препятствий: {{ item.obstacleCount }}</span>
          </div>
        </div>
        <button type="button" class="btn btn-small" @click.stop="openChallenge(item)">
          Открыть
        </button>
      </li>
    </ul>

    <!-- Детали выбранного challenge -->
    <div v-if="selected" class="challenge-detail">
      <div class="detail-header">
        <h3>{{ selected.name }}</h3>
        <div class="detail-actions">
          <button type="button" class="btn btn-primary" @click="tryChallenge">
            Попробовать испытание
          </button>
          <button type="button" class="btn-close" title="Закрыть" @click="selected = null">×</button>
        </div>
      </div>

      <p class="detail-description">{{ selected.description }}</p>

      <div class="detail-grid">
        <div class="detail-row">
          <span class="detail-label">Размер поля</span>
          <span class="detail-value">{{ selected.width }} × {{ selected.height }}</span>
        </div>
        <div class="detail-row">
          <span class="detail-label">Старт</span>
          <span class="detail-value">({{ selected.startX }}, {{ selected.startY }})</span>
        </div>
        <div class="detail-row">
          <span class="detail-label">Цель</span>
          <span class="detail-value">({{ selected.goalX }}, {{ selected.goalY }})</span>
        </div>
        <div class="detail-row">
          <span class="detail-label">Создан</span>
          <span class="detail-value">{{ formatDate(selected.createdAt) }}</span>
        </div>
        <div class="detail-row">
          <span class="detail-label">ID</span>
          <span class="detail-value mono">{{ selected.id }}</span>
        </div>
      </div>

      <div class="obstacles-block">
        <span class="detail-label">Препятствия ({{ selected.obstacles.length }})</span>
        <div v-if="selected.obstacles.length === 0" class="no-obstacles">Препятствий нет.</div>
        <div v-else class="obstacles-list">
          <span v-for="(obstacle, index) in selected.obstacles" :key="index" class="obstacle-chip">
            ({{ obstacle.x }}, {{ obstacle.y }})
          </span>
        </div>
      </div>

      <ChallengeRating :challenge-id="selected.id" :refresh-token="refreshToken" />
      <SolutionAttempts :challenge-id="selected.id" :refresh-token="refreshToken" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import axios from 'axios';
import { api } from '@/api/api';
import type { Challenge, ChallengeSummary } from '@/types/challenge';
import ChallengeRating from './ChallengeRating.vue';
import SolutionAttempts from './SolutionAttempts.vue';

const props = defineProps<{
  refreshToken?: number;
}>();

const emit = defineEmits<{
  try: [challenge: Challenge];
}>();

const challenges = ref<ChallengeSummary[]>([]);
const selected = ref<Challenge | null>(null);
const search = ref('');
const isLoading = ref(false);
const isDetailLoading = ref(false);
const errorMessage = ref('');

const filteredChallenges = computed(() => {
  const term = search.value.trim().toLowerCase();
  if (!term) return challenges.value;
  return challenges.value.filter((item) => item.name.toLowerCase().includes(term));
});

function formatDate(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;

  return new Intl.DateTimeFormat('ru-RU', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(date);
}

async function loadChallenges() {
  isLoading.value = true;
  errorMessage.value = '';

  try {
    const { data } = await api.get<ChallengeSummary[]>('/challenges');
    challenges.value = data ?? [];

    // Обновляем открытую карточку, если она есть в новом списке
    if (selected.value) {
      const fresh = challenges.value.find((item) => item.id === selected.value?.id);
      if (!fresh) selected.value = null;
    }
  } catch (error) {
    errorMessage.value = describeError(error, 'Не удалось загрузить список challenge.');
  } finally {
    isLoading.value = false;
  }
}

async function openChallenge(item: ChallengeSummary) {
  isDetailLoading.value = true;
  errorMessage.value = '';

  try {
    const { data } = await api.get<Challenge>(`/challenges/${item.id}`);
    selected.value = data;
  } catch (error) {
    errorMessage.value = describeError(error, 'Не удалось загрузить challenge.');
  } finally {
    isDetailLoading.value = false;
  }
}

function tryChallenge() {
  if (!selected.value) return;
  emit('try', selected.value);
}

function describeError(error: unknown, fallback: string): string {
  if (axios.isAxiosError(error)) {
    if (error.response?.status === 404) return 'Challenge не найден.';
    if (error.response?.status === 401) return 'Требуется авторизация. Пожалуйста, войдите в систему.';

    return error.response?.data?.message || error.response?.data?.error || fallback;
  }

  if (error instanceof Error) return error.message;
  return fallback;
}

watch(
  () => props.refreshToken,
  () => {
    void loadChallenges();
  }
);

onMounted(loadChallenges);
</script>

<style scoped>
.challenge-search-card {
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

/* Поиск */
.search-row {
  display: flex;
  gap: 10px;
  margin-bottom: 20px;
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

/* Список */
.challenge-list {
  list-style: none;
  margin: 0 0 24px 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 10px;
  max-height: 420px;
  overflow-y: auto;
}

.challenge-item {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 14px 16px;
  border: 1px solid #e9ecef;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.challenge-item:hover {
  border-color: #b6d4fe;
  background-color: #f8fbff;
}

.challenge-item.selected {
  border-color: #007bff;
  background-color: #eaf3ff;
}

.challenge-item-main {
  flex: 1;
  min-width: 0;
}

.challenge-item-main h3 {
  margin: 0 0 6px 0;
  font-size: 16px;
  color: #333;
  word-break: break-word;
}

.challenge-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.meta-chip {
  font-size: 12px;
  color: #555;
  background-color: #f1f3f5;
  padding: 3px 8px;
  border-radius: 6px;
  white-space: nowrap;
}

/* Детали */
.challenge-detail {
  padding: 20px;
  background-color: #f8f9fa;
  border: 1px solid #e9ecef;
  border-radius: 12px;
}

.detail-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 12px;
}

.detail-header h3 {
  margin: 0;
  font-size: 18px;
  color: #333;
  word-break: break-word;
}

.detail-actions {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-shrink: 0;
}

.btn-close {
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

.detail-description {
  margin: 0 0 20px 0;
  font-size: 14px;
  line-height: 1.6;
  color: #444;
  white-space: pre-wrap;
}

.detail-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
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
  font-size: 12px;
  color: #555;
}

.obstacles-block {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding-top: 14px;
  border-top: 1px solid #e9ecef;
}

.no-obstacles {
  font-size: 13px;
  color: #888;
  font-style: italic;
}

.obstacles-list {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  max-height: 140px;
  overflow-y: auto;
}

.obstacle-chip {
  font-family: 'Consolas', 'Monaco', monospace;
  font-size: 12px;
  background-color: white;
  border: 1px solid #dee2e6;
  color: #555;
  padding: 3px 8px;
  border-radius: 6px;
}

/* Состояния */
.empty-state {
  padding: 24px;
  font-size: 14px;
  color: #777;
  font-style: italic;
  text-align: center;
  background-color: #f8f9fa;
  border: 1px dashed #dee2e6;
  border-radius: 12px;
  margin-bottom: 20px;
}

.error-message {
  margin-bottom: 20px;
  padding: 12px;
  background-color: #f8d7da;
  color: #721c24;
  border-radius: 8px;
  font-size: 14px;
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

.btn-secondary {
  background-color: #e9ecef;
  color: #333;
}

.btn-secondary:hover:not(:disabled) {
  background-color: #dde2e6;
}

.btn-primary {
  background-color: #007bff;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background-color: #0069d9;
}

.btn-small {
  padding: 6px 12px;
  font-size: 13px;
  background-color: #e9ecef;
  color: #333;
  white-space: nowrap;
}

.btn-small:hover {
  background-color: #dde2e6;
}

@media (max-width: 640px) {
  .challenge-search-card {
    padding: 24px;
  }

  .search-row {
    flex-direction: column;
  }

  .detail-grid {
    grid-template-columns: 1fr;
  }

  .challenge-item {
    flex-direction: column;
    align-items: stretch;
  }
}
</style>
