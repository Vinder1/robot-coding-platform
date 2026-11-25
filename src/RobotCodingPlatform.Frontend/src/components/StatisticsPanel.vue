<template>
  <div class="statistics-card">
    <div class="card-header">
      <h2>Статистика</h2>
      <span class="endpoint-badge">api/statistics</span>
    </div>

    <form class="filters" @submit.prevent="applyFilters">
      <div class="filter-row">
        <label>
          С даты
          <input v-model="from" type="date" :disabled="isLoading" />
        </label>
        <label>
          По дату
          <input v-model="to" type="date" :disabled="isLoading" />
        </label>
        <button type="submit" class="btn btn-primary" :disabled="isLoading">Применить</button>
        <button type="button" class="btn btn-secondary" :disabled="isLoading" @click="resetFilters">
          Сбросить
        </button>
      </div>

      <div class="filter-row export-row">
        <span class="export-label">Выгрузка CSV:</span>
        <button type="button" class="btn btn-export" :disabled="isExporting" @click="download('summary')">
          Сводка по испытаниям
        </button>
        <button type="button" class="btn btn-export" :disabled="isExporting" @click="download('detail')">
          Детализация попыток
        </button>
      </div>
    </form>

    <div v-if="errorMessage" class="error-message">{{ errorMessage }}</div>
    <div v-if="successMessage" class="success-message">{{ successMessage }}</div>

    <section v-if="overview" class="overview">
      <div class="stat-tile">
        <span class="stat-label">Всего попыток</span>
        <span class="stat-value">{{ overview.totalAttempts }}</span>
      </div>
      <div class="stat-tile success">
        <span class="stat-label">Успешно</span>
        <span class="stat-value">{{ overview.succeeded }}</span>
      </div>
      <div class="stat-tile danger">
        <span class="stat-label">Провалено</span>
        <span class="stat-value">{{ overview.failed }}</span>
      </div>
      <div class="stat-tile">
        <span class="stat-label">В ожидании</span>
        <span class="stat-value">{{ overview.pending }}</span>
      </div>
      <div class="stat-tile">
        <span class="stat-label">Успешность</span>
        <span class="stat-value">{{ formatPercent(overview.successRate) }}</span>
      </div>
      <div class="stat-tile">
        <span class="stat-label">Пользователей</span>
        <span class="stat-value">{{ overview.uniqueUsers }}</span>
      </div>
      <div class="stat-tile">
        <span class="stat-label">Испытаний</span>
        <span class="stat-value">{{ overview.challenges }}</span>
      </div>
    </section>

    <p v-if="overview" class="period-note">
      Период попыток: {{ formatDate(overview.firstAttemptAt) }} — {{ formatDate(overview.lastAttemptAt) }}
    </p>

    <section class="table-section">
      <h3>По испытаниям</h3>

      <div v-if="isLoading" class="loading-state">Загрузка статистики...</div>
      <div v-else-if="challenges.length === 0" class="empty-state">За выбранный период данных нет.</div>

      <div v-else class="table-wrapper">
        <table class="stats-table">
          <thead>
            <tr>
              <th>Испытание</th>
              <th>Всего</th>
              <th>Успешно</th>
              <th>Провалено</th>
              <th>В ожидании</th>
              <th>Пользователей</th>
              <th>Успешность</th>
              <th>Последняя попытка</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in challenges" :key="row.challengeId">
              <td class="challenge-name">{{ row.challengeName || 'Без названия' }}</td>
              <td>{{ row.totalAttempts }}</td>
              <td class="text-success">{{ row.succeeded }}</td>
              <td class="text-danger">{{ row.failed }}</td>
              <td>{{ row.pending }}</td>
              <td>{{ row.uniqueUsers }}</td>
              <td>{{ formatPercent(row.successRate) }}</td>
              <td>{{ formatDate(row.lastAttemptAt) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <section class="opensearch-section">
      <div class="section-heading">
        <h3>OpenSearch</h3>
        <button type="button" class="btn btn-secondary" :disabled="isReindexing" @click="reindex">
          {{ isReindexing ? 'Индексация...' : 'Переиндексировать' }}
        </button>
      </div>

      <div v-if="openSearchStatus" class="status-row">
        <span class="status-badge" :class="statusClass">
          {{ statusLabel }}
        </span>
        <span v-if="openSearchStatus.clusterName" class="status-detail">
          Кластер: {{ openSearchStatus.clusterName }}
        </span>
        <span v-if="openSearchStatus.nodes !== null" class="status-detail">
          Узлов: {{ openSearchStatus.nodes }}
        </span>
        <span v-if="openSearchStatus.message" class="status-detail status-message">
          {{ openSearchStatus.message }}
        </span>
      </div>

      <template v-if="openSearchStatus?.enabled">
        <form class="search-row" @submit.prevent="searchIndex">
          <input
            v-model.trim="searchQuery"
            type="text"
            placeholder="Поиск по испытанию, пользователю или статусу"
            :disabled="isSearching"
          />
          <button type="submit" class="btn btn-primary" :disabled="isSearching">Искать в индексе</button>
        </form>

        <div v-if="isSearching" class="loading-state">Поиск...</div>
        <div v-else-if="searchResults.length > 0" class="table-wrapper">
          <table class="stats-table">
            <thead>
              <tr>
                <th>Испытание</th>
                <th>Пользователь</th>
                <th>Статус</th>
                <th>Создана</th>
                <th>Длительность</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in searchResults" :key="item.attemptId">
                <td class="challenge-name">{{ item.challengeName || 'Без названия' }}</td>
                <td>{{ item.userName || '—' }}</td>
                <td>{{ item.status }}</td>
                <td>{{ formatDate(item.createdAt) }}</td>
                <td>{{ formatDuration(item.durationMs) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-else class="empty-state">В индексе пока нет подходящих записей.</div>
      </template>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import axios from 'axios';
import { api } from '@/api/api';

interface StatisticsOverview {
  totalAttempts: number;
  succeeded: number;
  failed: number;
  pending: number;
  uniqueUsers: number;
  challenges: number;
  successRate: number;
  firstAttemptAt: string | null;
  lastAttemptAt: string | null;
}

interface ChallengeStatistics {
  challengeId: string;
  challengeName: string;
  totalAttempts: number;
  succeeded: number;
  failed: number;
  pending: number;
  uniqueUsers: number;
  successRate: number;
  firstAttemptAt: string | null;
  lastAttemptAt: string | null;
}

interface OpenSearchStatus {
  enabled: boolean;
  healthy: boolean;
  clusterName: string | null;
  status: string | null;
  nodes: number | null;
  message: string | null;
}

interface IndexedAttempt {
  attemptId: string;
  challengeId: string;
  challengeName: string;
  userId: string;
  userName: string;
  status: string;
  success: boolean;
  error: string | null;
  createdAt: string;
  completedAt: string | null;
  durationMs: number | null;
}

interface SearchResponse {
  enabled: boolean;
  items: IndexedAttempt[];
}

interface ReindexResponse {
  enabled: boolean;
  indexed: number;
  message: string | null;
}

const from = ref('');
const to = ref('');
const overview = ref<StatisticsOverview | null>(null);
const challenges = ref<ChallengeStatistics[]>([]);
const isLoading = ref(false);
const isExporting = ref(false);
const errorMessage = ref('');
const successMessage = ref('');

const openSearchStatus = ref<OpenSearchStatus | null>(null);
const isReindexing = ref(false);
const isSearching = ref(false);
const searchQuery = ref('');
const searchResults = ref<IndexedAttempt[]>([]);

const statusClass = computed(() => {
  if (!openSearchStatus.value?.enabled) return 'disabled';
  return openSearchStatus.value.healthy ? 'healthy' : 'unhealthy';
});

const statusLabel = computed(() => {
  if (!openSearchStatus.value) return 'Неизвестно';
  if (!openSearchStatus.value.enabled) return 'Отключён';
  if (!openSearchStatus.value.healthy) return 'Недоступен';
  return `Работает${openSearchStatus.value.status ? ` (${openSearchStatus.value.status})` : ''}`;
});

function rangeParams() {
  return {
    from: from.value ? `${from.value}T00:00:00Z` : undefined,
    to: to.value ? `${to.value}T23:59:59.999Z` : undefined
  };
}

async function loadStatistics() {
  isLoading.value = true;
  errorMessage.value = '';

  try {
    const params = rangeParams();
    const [overviewResponse, challengesResponse] = await Promise.all([
      api.get<StatisticsOverview>('/statistics/overview', { params }),
      api.get<ChallengeStatistics[]>('/statistics/challenges', { params })
    ]);

    overview.value = overviewResponse.data;
    challenges.value = challengesResponse.data ?? [];
  } catch (error) {
    errorMessage.value = describeError(error, 'Не удалось загрузить статистику.');
  } finally {
    isLoading.value = false;
  }
}

async function loadOpenSearchStatus() {
  try {
    const { data } = await api.get<OpenSearchStatus>('/statistics/opensearch/status');
    openSearchStatus.value = data;

    if (data.enabled && data.healthy) {
      await searchIndex();
    }
  } catch (error) {
    errorMessage.value = describeError(error, 'Не удалось получить статус OpenSearch.');
  }
}

async function searchIndex() {
  isSearching.value = true;

  try {
    const { data } = await api.get<SearchResponse>('/statistics/opensearch/search', {
      params: { query: searchQuery.value || undefined, take: 50 }
    });

    searchResults.value = data.items ?? [];
  } catch (error) {
    errorMessage.value = describeError(error, 'Не удалось выполнить поиск в OpenSearch.');
  } finally {
    isSearching.value = false;
  }
}

async function reindex() {
  isReindexing.value = true;
  errorMessage.value = '';
  successMessage.value = '';

  try {
    const { data } = await api.post<ReindexResponse>('/statistics/opensearch/reindex');

    if (data.message) {
      errorMessage.value = data.message;
    } else {
      successMessage.value = `Проиндексировано попыток: ${data.indexed}.`;
    }

    await Promise.all([loadOpenSearchStatus(), loadStatistics()]);
  } catch (error) {
    errorMessage.value = describeError(error, 'Не удалось переиндексировать данные.');
  } finally {
    isReindexing.value = false;
  }
}

async function download(kind: 'summary' | 'detail') {
  isExporting.value = true;
  errorMessage.value = '';
  successMessage.value = '';

  try {
    const response = await api.get<Blob>('/statistics/export', {
      params: {
        ...rangeParams(),
        detail: kind === 'detail'
      },
      responseType: 'blob'
    });

    const fileName =
      extractFileName(response.headers['content-disposition']) ??
      `statistics-${kind}.csv`;

    const url = URL.createObjectURL(response.data);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    link.remove();
    URL.revokeObjectURL(url);

    successMessage.value = 'Файл CSV сформирован.';
  } catch (error) {
    errorMessage.value = describeError(error, 'Не удалось выгрузить статистику.');
  } finally {
    isExporting.value = false;
  }
}

function applyFilters() {
  successMessage.value = '';
  void loadStatistics();
}

function resetFilters() {
  from.value = '';
  to.value = '';
  successMessage.value = '';
  void loadStatistics();
}

function extractFileName(header: unknown): string | null {
  if (typeof header !== 'string') return null;

  const match = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(header);
  return match?.[1] ? decodeURIComponent(match[1]) : null;
}

function formatPercent(value: number): string {
  return `${Number(value).toFixed(2)} %`;
}

function formatDate(value: string | null | undefined): string {
  if (!value) return '—';

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

function formatDuration(value: number | null | undefined): string {
  if (value === null || value === undefined) return '—';

  if (value < 1000) return `${value} мс`;

  const seconds = value / 1000;
  return `${seconds.toFixed(seconds < 10 ? 2 : 1)} с`;
}

function describeError(error: unknown, fallback: string): string {
  if (axios.isAxiosError(error)) {
    const status = error.response?.status;

    if (status === 401) return 'Требуется авторизация. Пожалуйста, войдите в систему.';
    if (status === 403) return 'Действие доступно только администратору.';

    return error.response?.data?.message || error.response?.data?.error || fallback;
  }

  if (error instanceof Error) return error.message;
  return fallback;
}

onMounted(() => {
  void loadStatistics();
  void loadOpenSearchStatus();
});
</script>

<style scoped>
.statistics-card {
  font-family: Arial, sans-serif;
  background-color: white;
  padding: 40px;
  border-radius: 16px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
  max-width: 1080px;
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

.filters {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 20px;
}

.filter-row {
  display: flex;
  align-items: flex-end;
  gap: 10px;
  flex-wrap: wrap;
}

.filter-row label {
  display: flex;
  flex-direction: column;
  gap: 4px;
  font-size: 12px;
  font-weight: 600;
  color: #666;
}

.filter-row input {
  padding: 9px 11px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 14px;
  font-family: inherit;
}

.filter-row input:focus {
  outline: none;
  border-color: #007bff;
}

.export-row {
  align-items: center;
}

.export-label {
  font-size: 13px;
  color: #666;
}

.btn {
  padding: 9px 16px;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.2s ease;
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
  background-color: #0069d9;
}

.btn-secondary {
  background-color: #e9ecef;
  color: #333;
}

.btn-secondary:hover:not(:disabled) {
  background-color: #dde1e5;
}

.btn-export {
  background-color: #28a745;
  color: white;
}

.btn-export:hover:not(:disabled) {
  background-color: #1e7e34;
}

.error-message,
.success-message {
  padding: 12px 16px;
  border-radius: 8px;
  margin-bottom: 16px;
  font-size: 14px;
}

.error-message {
  background-color: #f8d7da;
  color: #721c24;
}

.success-message {
  background-color: #d4edda;
  color: #155724;
}

.overview {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(130px, 1fr));
  gap: 12px;
  margin-bottom: 12px;
}

.stat-tile {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 16px;
  border-radius: 12px;
  background-color: #f8f9fa;
  border: 1px solid #e9ecef;
}

.stat-tile.success {
  background-color: #eef8f1;
  border-color: #cdebd8;
}

.stat-tile.danger {
  background-color: #fdf0f0;
  border-color: #f5d3d3;
}

.stat-label {
  font-size: 12px;
  color: #777;
  text-transform: uppercase;
  letter-spacing: 0.4px;
}

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: #333;
}

.period-note {
  font-size: 13px;
  color: #777;
  margin: 0 0 24px;
}

.table-section,
.opensearch-section {
  margin-top: 28px;
  padding-top: 20px;
  border-top: 1px solid #f0f0f0;
}

.table-section h3,
.opensearch-section h3 {
  margin: 0 0 14px;
  color: #333;
  font-size: 17px;
}

.section-heading {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.section-heading h3 {
  margin: 0;
}

.table-wrapper {
  overflow-x: auto;
  border: 1px solid #e9ecef;
  border-radius: 12px;
}

.stats-table {
  width: 100%;
  min-width: 720px;
  border-collapse: collapse;
  font-size: 14px;
}

.stats-table th {
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

.stats-table td {
  padding: 11px 14px;
  border-bottom: 1px solid #f0f0f0;
  color: #444;
}

.stats-table tbody tr:last-child td {
  border-bottom: none;
}

.challenge-name {
  font-weight: 600;
  color: #333;
}

.text-success {
  color: #1e7e34;
}

.text-danger {
  color: #b02a37;
}

.loading-state,
.empty-state {
  padding: 18px;
  color: #777;
  font-size: 14px;
  text-align: center;
  background-color: #f8f9fa;
  border-radius: 10px;
}

.status-row {
  display: flex;
  align-items: center;
  gap: 14px;
  flex-wrap: wrap;
  margin: 14px 0;
}

.status-badge {
  padding: 5px 12px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.4px;
}

.status-badge.healthy {
  background-color: #d4edda;
  color: #155724;
}

.status-badge.unhealthy {
  background-color: #f8d7da;
  color: #721c24;
}

.status-badge.disabled {
  background-color: #e9ecef;
  color: #555;
}

.status-detail {
  font-size: 13px;
  color: #666;
}

.status-message {
  color: #b02a37;
}

.search-row {
  display: flex;
  gap: 10px;
  margin-bottom: 14px;
}

.search-row input {
  flex: 1;
  padding: 10px 12px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 14px;
  font-family: inherit;
}

.search-row input:focus {
  outline: none;
  border-color: #007bff;
}

@media (max-width: 600px) {
  .statistics-card {
    padding: 24px 16px;
  }

  .search-row {
    flex-direction: column;
  }
}
</style>
