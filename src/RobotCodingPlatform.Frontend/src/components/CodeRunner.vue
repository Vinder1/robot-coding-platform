<template>
  <div class="code-runner-card" v-if="authStore.isLoggedIn">
    <div class="card-header">
      <h2>{{ isChallengeMode ? 'Прохождение испытания' : 'Запуск кода' }}</h2>
    </div>

    <!-- Режим испытания: карта берётся из испытания -->
    <div v-if="challenge" class="challenge-banner">
      <div class="challenge-banner-main">
        <strong>{{ challenge.name }}</strong>
        <div class="challenge-meta">
          <span class="meta-chip">Поле {{ challenge.width }} × {{ challenge.height }}</span>
          <span class="meta-chip">Старт ({{ challenge.startX }}, {{ challenge.startY }})</span>
          <span class="meta-chip">Цель ({{ challenge.goalX }}, {{ challenge.goalY }})</span>
          <span class="meta-chip">Препятствий: {{ challenge.obstacles.length }}</span>
        </div>
      </div>
      <button type="button" class="btn-exit" @click="emit('exit')">
        Вернуться к испытанию
      </button>
    </div>

    <details class="lang-help">
      <summary>Команды языка</summary>
      <ul>
        <li><code>move N</code> — проехать вперёд на N клеток (по умолчанию 1)</li>
        <li><code>turn_left</code> / <code>turn_right</code> — повернуть налево / направо</li>
        <li>Датчики: <code>wall_forward</code>, <code>wall_left</code>, <code>wall_right</code></li>
        <li>
          Условия: <code>if</code> / <code>while</code> / <code>else</code>, операторы
          <code>and</code>, <code>or</code>, <code>not</code>
        </li>
        <li>Блок — отступ 4 пробела, комментарии — <code>#</code></li>
        <li>После <code>if</code> / <code>while</code> / <code>else</code> двоеточие не ставится</li>
      </ul>
      <pre class="lang-example">while not wall_forward
    move
turn_right</pre>
    </details>

    <form @submit.prevent="runCode" class="runner-form">
      <!-- Параметры поля и цели (только в свободном режиме) -->
      <div v-if="!isChallengeMode" class="params-grid">
        <div class="param-box">
          <h3>Размер поля</h3>
          <div class="inputs-row">
            <div class="form-group">
              <label for="grid-width">Width (Ширина)</label>
              <input id="grid-width" v-model.number="form.width" type="number" min="1" required />
            </div>
            <div class="form-group">
              <label for="grid-height">Height (Высота)</label>
              <input id="grid-height" v-model.number="form.height" type="number" min="1" required />
            </div>
          </div>
        </div>

        <div class="param-box">
          <h3>Старт (Start)</h3>
          <div class="inputs-row">
            <div class="form-group">
              <label for="start-x">X</label>
              <input id="start-x" v-model.number="form.start.x" type="number" min="0" required />
            </div>
            <div class="form-group">
              <label for="start-y">Y</label>
              <input id="start-y" v-model.number="form.start.y" type="number" min="0" required />
            </div>
          </div>
        </div>

        <div class="param-box">
          <h3>Цель (Goal)</h3>
          <div class="inputs-row">
            <div class="form-group">
              <label for="goal-x">X</label>
              <input id="goal-x" v-model.number="form.goal.x" type="number" min="0" required />
            </div>
            <div class="form-group">
              <label for="goal-y">Y</label>
              <input id="goal-y" v-model.number="form.goal.y" type="number" min="0" required />
            </div>
          </div>
        </div>
      </div>

      <!-- Список препятствий (только в свободном режиме) -->
      <div v-if="!isChallengeMode" class="obstacles-section">
        <div class="section-header">
          <label>Препятствия (Obstacles)</label>
          <button type="button" class="btn-small btn-add" @click="addObstacle">
            + Добавить точку
          </button>
        </div>

        <div v-if="form.obstacles.length === 0" class="empty-obstacles">
          Препятствий нет. Нажмите «+ Добавить точку», чтобы задать координаты.
        </div>

        <div v-else class="obstacles-list">
          <div v-for="(obs, index) in form.obstacles" :key="index" class="obstacle-item">
            <span class="obs-index">#{{ index + 1 }}</span>
            <label>X:</label>
            <input v-model.number="obs.x" type="number" min="0" required />
            <label>Y:</label>
            <input v-model.number="obs.y" type="number" min="0" required />
            <button type="button" class="btn-remove" title="Удалить препятствие" @click="removeObstacle(index)">
              &times;
            </button>
          </div>
        </div>
      </div>

      <!-- Многострочный редактор кода с поддержкой Tab -->
      <div class="form-group">
        <div class="code-label-row">
          <label for="code-editor">Код решения</label>
          <span class="hint">Поддерживается клавиша Tab</span>
        </div>
        <textarea id="code-editor" ref="codeTextarea" v-model="form.code" class="code-textarea" rows="14"
          spellcheck="false" placeholder="# Введите ваш код здесь..." required
          @keydown.tab.prevent="handleTab"></textarea>
      </div>

      <!-- Кнопки запуска -->
      <div class="run-actions">
        <button type="submit" class="btn btn-primary">
          {{ isChallengeMode ? 'Запустить решение' : 'Запустить код' }}
        </button>
        <button
          v-if="isChallengeMode"
          type="button"
          class="btn btn-submit"
          :disabled="isSubmitting"
          @click="submitSolution"
        >
          {{ isSubmitting ? 'Отправка...' : 'Сдать решение' }}
        </button>
        <button
          type="button"
          class="btn btn-secondary"
          :disabled="isChecking"
          @click="checkOnServer"
        >
          {{ isChecking ? 'Проверка...' : 'Проверить на сервере' }}
        </button>
      </div>
    </form>

    <!-- Отправка решения на сервер: сохранённая попытка -->
    <div v-if="submissionError" class="result-banner error">
      <strong>Ошибка отправки:</strong> {{ submissionError }}
    </div>

    <div
      v-if="submittedAttempt"
      class="submission-panel"
      :class="submissionStatusClass"
    >
      <div class="submission-head">
        <span class="submission-status">{{ submissionStatusLabel }}</span>
        <span class="submission-id mono">попытка {{ submittedAttempt.id.slice(0, 8) }}</span>
      </div>
      <p class="submission-text">{{ submissionResultText }}</p>
      <p v-if="submittedAttempt.error" class="submission-error-text">{{ submittedAttempt.error }}</p>
      <p v-if="submittedAttempt.iterations !== null" class="submission-score">
        Оценка: <strong>{{ submittedAttempt.iterations }}</strong> итераций
      </p>
      <p class="submission-meta">
        Отправлено {{ formatDate(submittedAttempt.createdAt) }}
        <span v-if="submittedAttempt.completedAt">
          · завершено {{ formatDate(submittedAttempt.completedAt) }}
        </span>
      </p>
    </div>

    <!-- Клиентское исполнение с анимацией робота -->
    <p v-if="localRun" class="run-score">
      Итераций на клиенте: <strong>{{ localRun.iterations }}</strong>
    </p>
    <div ref="arenaWrapper">
      <RobotArena
        v-if="localRun"
        :width="mapConfig.width"
        :height="mapConfig.height"
        :start="mapConfig.start"
        :goal="mapConfig.goal"
        :obstacles="mapConfig.obstacles"
        :frames="localRun.frames"
        :success="localRun.success"
        :error="localRun.error"
      />
    </div>

    <!-- Вывод ошибки серверной проверки -->
    <div v-if="errorMessage" class="result-banner error">
      <strong>Ошибка:</strong> {{ errorMessage }}
    </div>

    <!-- Вывод результата серверной проверки -->
    <div v-if="serverResult !== null" class="result-banner" :class="serverResult.success ? 'success' : 'failure'">
      <span class="result-icon">{{ serverResult.success ? '✓' : '✕' }}</span>
      <div>
        <strong>{{ serverResult.success ? 'Сервер: успех!' : 'Сервер: задача не решена' }}</strong>
        <p class="result-subtext">
          {{
            serverResult.message ||
            (serverResult.success
              ? 'Код успешно выполнен, цель достигнута.'
              : 'Код отработал, но цель не была достигнута (Success: false).')
          }}
        </p>
        <p v-if="(serverResult.iterations ?? 0) > 0" class="result-subtext">
          Итераций на сервере: {{ serverResult.iterations }}
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onUnmounted, ref, watch } from 'vue';
import axios from 'axios';
import { api } from '@/api/api';
import { useAuthStore } from '@/stores/authStore';
import RobotArena from './RobotArena.vue';
import { runRobotProgram } from '@/robot/runner';
import type { RobotMapConfig, RobotRunResult } from '@/robot/types';
import type { Challenge } from '@/types/challenge';
import type { SolutionAttempt, SolutionAttemptSummary } from '@/types/solution';

const props = defineProps<{
  challenge?: Challenge | null;
}>();

const emit = defineEmits<{
  exit: [];
  submitted: [];
}>();

const authStore = useAuthStore();

const isChallengeMode = computed(() => Boolean(props.challenge));

// Типы, соответствующие C# DTO
interface Point {
  x: number;
  y: number;
}

interface RunCodeParams {
  height: number;
  width: number;
  start: Point;
  goal: Point;
  obstacles: Point[];
  code: string;
}

interface ServerRunResult {
  success: boolean;
  message?: string;
  compiled?: boolean;
  errorKind?: string;
  iterations?: number;
}

const codeTextarea = ref<HTMLTextAreaElement | null>(null);
const arenaWrapper = ref<HTMLElement | null>(null);
const isChecking = ref(false);
const errorMessage = ref('');
const serverResult = ref<ServerRunResult | null>(null);
const localRun = ref<RobotRunResult | null>(null);

const isSubmitting = ref(false);
const submissionError = ref('');
const submittedAttempt = ref<SolutionAttemptSummary | null>(null);

let pollTimer: ReturnType<typeof setTimeout> | null = null;

function clearPollTimer() {
  if (pollTimer !== null) {
    clearTimeout(pollTimer);
    pollTimer = null;
  }
}

const form = ref<RunCodeParams>({
  width: 10,
  height: 10,
  start: { x: 0, y: 0 },
  goal: { x: 5, y: 5 },
  obstacles: [
    { x: 2, y: 2 },
    { x: 3, y: 4 },
  ],
  code: '',
});

const mapConfig = computed<RobotMapConfig>(() => {
  if (props.challenge) {
    return {
      width: props.challenge.width,
      height: props.challenge.height,
      start: { x: props.challenge.startX, y: props.challenge.startY },
      goal: { x: props.challenge.goalX, y: props.challenge.goalY },
      obstacles: props.challenge.obstacles.map((o) => ({ x: o.x, y: o.y })),
    };
  }

  return {
    width: Number(form.value.width),
    height: Number(form.value.height),
    start: { x: Number(form.value.start.x), y: Number(form.value.start.y) },
    goal: { x: Number(form.value.goal.x), y: Number(form.value.goal.y) },
    obstacles: form.value.obstacles.map((o) => ({ x: Number(o.x), y: Number(o.y) })),
  };
});

// Изменение карты или испытания сбрасывает прошлый запуск
watch(
  mapConfig,
  () => {
    localRun.value = null;
  },
  { deep: true },
);

watch(
  () => props.challenge?.id,
  () => {
    localRun.value = null;
    serverResult.value = null;
    errorMessage.value = '';
    submissionError.value = '';
    submittedAttempt.value = null;
    clearPollTimer();
  },
);

// Обработка нажатия Tab внутри textarea
async function handleTab(event: KeyboardEvent) {
  const textarea = event.target as HTMLTextAreaElement;
  if (!textarea) return;

  const start = textarea.selectionStart;
  const end = textarea.selectionEnd;

  // Вставляем символ табуляции '\t' (можно заменить на '    ' для 4 пробелов)
  const tabCharacter = '\t';

  form.value.code =
    form.value.code.substring(0, start) +
    tabCharacter +
    form.value.code.substring(end);

  // Возвращаем курсор на нужную позицию после обновления DOM
  await nextTick();
  textarea.selectionStart = textarea.selectionEnd = start + tabCharacter.length;
}

function addObstacle() {
  form.value.obstacles.push({ x: 0, y: 0 });
}

function removeObstacle(index: number) {
  form.value.obstacles.splice(index, 1);
}

// Клиентское исполнение: лексер → парсер → интерпретатор → кадры для анимации
function runCode() {
  errorMessage.value = '';
  serverResult.value = null;
  localRun.value = runRobotProgram(form.value.code, mapConfig.value);

  void nextTick(() => {
    arenaWrapper.value?.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
  });
}

// Дополнительная проверка на сервере (эндпоинт /api/code/run)
async function checkOnServer() {
  isChecking.value = true;
  errorMessage.value = '';
  serverResult.value = null;

  try {
    // В режиме испытания параметры карты не отправляются —
    // сервер сам загружает их по id испытания.
    const response = props.challenge
      ? await api.post<ServerRunResult>('/code/run/challenge', {
          challengeId: props.challenge.id,
          code: form.value.code,
        })
      : await api.post<ServerRunResult>('/code/run', form.value);

    const raw = response.data;
    serverResult.value = {
      success: Boolean(raw.success),
      message: raw.message || undefined,
      compiled: raw.compiled,
      errorKind: raw.errorKind,
      iterations: raw.iterations ?? 0,
    };
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 401) {
        errorMessage.value = 'Требуется авторизация. Пожалуйста, войдите в систему.';
      } else if (error.response?.status === 429) {
        errorMessage.value = 'Слишком много запросов. Подождите немного и попробуйте снова.';
      } else if (error.response?.status === 404) {
        const body = error.response.data as { message?: string } | undefined;
        errorMessage.value =
          body?.message ||
          'Серверная проверка недоступна: эндпоинт не найден. Перезапустите сервер.';
      } else {
        errorMessage.value =
          error.response?.data?.message ||
          error.response?.data?.error ||
          error.response?.data?.title ||
          'Ошибка при выполнении кода на сервере.';
      }
    } else if (error instanceof Error) {
      errorMessage.value = error.message;
    } else {
      errorMessage.value = 'Неизвестная ошибка.';
    }
  } finally {
    isChecking.value = false;
  }
}

// Сохранение попытки решения на сервере (outbox → воркер → результат)
async function submitSolution() {
  if (!props.challenge || isSubmitting.value) return;

  if (!form.value.code.trim()) {
    submissionError.value = 'Введите код решения перед отправкой.';
    return;
  }

  isSubmitting.value = true;
  submissionError.value = '';
  submittedAttempt.value = null;
  clearPollTimer();

  try {
    const { data } = await api.post<SolutionAttempt>('/solutions', {
      challengeId: props.challenge.id,
      code: form.value.code,
    });

    submittedAttempt.value = data;
    emit('submitted');
    schedulePoll(data.id);
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 401) {
        submissionError.value = 'Требуется авторизация. Пожалуйста, войдите в систему.';
      } else if (error.response?.status === 429) {
        submissionError.value = 'Слишком много запросов. Подождите немного и попробуйте снова.';
      } else if (error.response?.status === 404) {
        submissionError.value = 'Испытание не найдено.';
      } else {
        submissionError.value =
          error.response?.data?.errors?.[0] ||
          error.response?.data?.message ||
          error.response?.data?.error ||
          error.response?.data?.title ||
          'Не удалось отправить решение.';
      }
    } else if (error instanceof Error) {
      submissionError.value = error.message;
    } else {
      submissionError.value = 'Неизвестная ошибка.';
    }
  } finally {
    isSubmitting.value = false;
  }
}

function schedulePoll(attemptId: string) {
  clearPollTimer();

  pollTimer = setTimeout(async () => {
    pollTimer = null;

    try {
      const { data } = await api.get<SolutionAttemptSummary>(`/solutions/${attemptId}`);
      submittedAttempt.value = data;

      if (data.status === 'Pending') {
        schedulePoll(attemptId);
      } else {
        emit('submitted');
      }
    } catch {
      // Сервер временно недоступен — продолжаем опрос.
      schedulePoll(attemptId);
    }
  }, 1500);
}

const submissionStatusLabel = computed(() => {
  const attempt = submittedAttempt.value;
  if (!attempt) return '';

  switch (attempt.status) {
    case 'Pending':
      return 'Решение отправлено';
    case 'Succeeded':
      return 'Попытка успешна';
    case 'Failed':
      return 'Попытка не удалась';
    default:
      return attempt.status;
  }
});

const submissionStatusClass = computed(() => {
  const attempt = submittedAttempt.value;
  if (!attempt) return '';

  if (attempt.status === 'Pending') return 'pending';
  return attempt.success ? 'success' : 'failure';
});

const submissionResultText = computed(() => {
  const attempt = submittedAttempt.value;
  if (!attempt) return '';

  if (attempt.status === 'Pending') {
    return 'Ожидаем результат выполнения на сервере...';
  }

  if (attempt.success) {
    return 'Цель достигнута. Попытка сохранена.';
  }

  return attempt.error
    ? 'Код завершился с ошибкой. Попытка сохранена.'
    : 'Цель не достигнута. Попытка сохранена.';
});

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

onUnmounted(clearPollTimer);
</script>

<style scoped>
.code-runner-card {
  font-family: Arial, sans-serif;
  background-color: white;
  padding: 40px;
  border-radius: 16px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
  max-width: 760px;
  width: 92%;
  margin: 0 auto;
  text-align: left;
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

/* Баннер испытания */
.challenge-banner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
  margin-bottom: 20px;
  padding: 16px;
  background-color: #eaf3ff;
  border: 1px solid #b6d4fe;
  border-radius: 12px;
}

.challenge-banner-main {
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 0;
}

.challenge-banner-main strong {
  color: #1c4c8c;
  font-size: 16px;
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
  background-color: white;
  padding: 3px 8px;
  border-radius: 6px;
  white-space: nowrap;
}

.btn-exit {
  padding: 10px 18px;
  font-size: 14px;
  font-weight: 500;
  border: none;
  border-radius: 8px;
  background-color: #007bff;
  color: white;
  cursor: pointer;
  white-space: nowrap;
  transition: background-color 0.2s ease;
}

.btn-exit:hover {
  background-color: #0069d9;
}

/* Подсказка по языку */
.lang-help {
  margin-bottom: 20px;
  padding: 12px 16px;
  background-color: #f8f9fa;
  border: 1px solid #e9ecef;
  border-radius: 10px;
  font-size: 13px;
  color: #444;
}

.lang-help summary {
  cursor: pointer;
  font-weight: 600;
  color: #333;
}

.lang-help ul {
  margin: 10px 0 0 0;
  padding-left: 20px;
  line-height: 1.7;
}

.lang-help code {
  font-family: 'Fira Code', 'Consolas', monospace;
  background-color: #eef1f5;
  padding: 1px 5px;
  border-radius: 4px;
  font-size: 12px;
}

.lang-example {
  margin: 12px 0 0 0;
  padding: 10px 12px;
  background-color: #1e1e1e;
  color: #d4d4d4;
  border-radius: 8px;
  font-family: 'Fira Code', 'Consolas', monospace;
  font-size: 12px;
  line-height: 1.5;
  overflow-x: auto;
}

.runner-form {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

/* Сетка параметров поля и цели */
.params-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 20px;
}

.param-box {
  background-color: #f8f9fa;
  padding: 16px;
  border-radius: 12px;
  border: 1px solid #e9ecef;
}

.param-box h3 {
  margin: 0 0 12px 0;
  font-size: 14px;
  color: #333;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.inputs-row {
  display: flex;
  gap: 12px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
  flex: 1;
}

.form-group label {
  font-size: 13px;
  color: #555;
  font-weight: 500;
}

input[type='number'] {
  padding: 10px 12px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 14px;
  width: 100%;
  box-sizing: border-box;
  transition: border-color 0.2s ease;
}

input[type='number']:focus {
  outline: none;
  border-color: #007bff;
}

/* Препятствия */
.obstacles-section {
  background-color: #f8f9fa;
  padding: 16px;
  border-radius: 12px;
  border: 1px solid #e9ecef;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.section-header label {
  font-size: 14px;
  font-weight: 600;
  color: #333;
}

.empty-obstacles {
  font-size: 13px;
  color: #777;
  font-style: italic;
}

.obstacles-list {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.obstacle-item {
  display: flex;
  align-items: center;
  gap: 6px;
  background-color: white;
  padding: 6px 10px;
  border-radius: 8px;
  border: 1px solid #dee2e6;
}

.obs-index {
  font-size: 12px;
  font-weight: bold;
  color: #007bff;
  margin-right: 4px;
}

.obstacle-item label {
  font-size: 12px;
  color: #666;
}

.obstacle-item input {
  width: 60px;
  padding: 6px 8px;
}

.btn-remove {
  background: none;
  border: none;
  color: #dc3545;
  font-size: 18px;
  line-height: 1;
  cursor: pointer;
  padding: 0 4px;
}

.btn-remove:hover {
  color: #a71d2a;
}

.btn-small {
  padding: 6px 12px;
  font-size: 13px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  transition: background-color 0.2s ease;
}

.btn-add {
  background-color: #e9ecef;
  color: #333;
}

.btn-add:hover {
  background-color: #dde2e6;
}

/* Поле ввода кода */
.code-label-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.hint {
  font-size: 12px;
  color: #888;
}

.code-textarea {
  width: 100%;
  min-height: 300px;
  padding: 16px;
  border: 2px solid #e0e0e0;
  border-radius: 10px;
  background-color: #1e1e1e;
  color: #d4d4d4;
  font-family: 'Fira Code', 'Consolas', 'Courier New', monospace;
  font-size: 14px;
  line-height: 1.5;
  tab-size: 4;
  -moz-tab-size: 4;
  resize: vertical;
  box-sizing: border-box;
  white-space: pre;
  overflow-x: auto;
  transition: border-color 0.2s ease;
}

.code-textarea:focus {
  outline: none;
  border-color: #007bff;
}

/* Кнопки запуска */
.run-actions {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.btn {
  padding: 14px 30px;
  font-size: 16px;
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

.btn-primary {
  background-color: #007bff;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background-color: #0056b3;
  transform: translateY(-2px);
}

.btn-secondary {
  background-color: #e9ecef;
  color: #333;
}

.btn-secondary:hover:not(:disabled) {
  background-color: #dde2e6;
  transform: translateY(-2px);
}

.btn-submit {
  background-color: #28a745;
  color: white;
}

.btn-submit:hover:not(:disabled) {
  background-color: #1e7e34;
  transform: translateY(-2px);
}

/* Панель сохранённой попытки */
.submission-panel {
  margin-top: 24px;
  padding: 16px;
  border-radius: 10px;
  border: 1px solid transparent;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.submission-panel.pending {
  background-color: #e7f1ff;
  border-color: #b6d4fe;
  color: #0b5ed7;
}

.submission-panel.success {
  background-color: #d4edda;
  border-color: #c3e6cb;
  color: #155724;
}

.submission-panel.failure {
  background-color: #f8d7da;
  border-color: #f5c6cb;
  color: #721c24;
}

.submission-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  flex-wrap: wrap;
}

.submission-status {
  font-size: 15px;
  font-weight: 600;
}

.submission-id {
  font-size: 11px;
  opacity: 0.8;
}

.submission-text {
  margin: 0;
  font-size: 14px;
}

.submission-error-text {
  margin: 0;
  padding: 8px 10px;
  background-color: rgba(255, 255, 255, 0.6);
  border-radius: 6px;
  font-size: 13px;
  white-space: pre-wrap;
  word-break: break-word;
}

.submission-meta {
  margin: 0;
  font-size: 12px;
  opacity: 0.8;
}

.submission-score {
  margin: 0;
  font-size: 14px;
  font-weight: 600;
}

.run-score {
  margin: 18px 0 0 0;
  font-size: 13px;
  color: #555;
  text-align: center;
}

.run-score strong {
  color: #0b5ed7;
}

.mono {
  font-family: 'Consolas', 'Monaco', monospace;
}

/* Баннеры результатов */
.result-banner {
  margin-top: 24px;
  padding: 16px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  gap: 14px;
  font-size: 15px;
}

.result-banner.error,
.result-banner.failure {
  background-color: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}

.result-banner.success {
  background-color: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}

.result-icon {
  font-size: 22px;
  font-weight: bold;
}

.result-subtext {
  margin: 4px 0 0 0;
  font-size: 13px;
  opacity: 0.9;
}

@media (max-width: 640px) {
  .params-grid {
    grid-template-columns: 1fr;
  }

  .code-runner-card {
    padding: 24px;
  }

  .run-actions {
    flex-direction: column;
  }

  .btn {
    width: 100%;
  }
}
</style>
