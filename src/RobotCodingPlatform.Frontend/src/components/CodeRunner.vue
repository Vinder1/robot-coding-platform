<template>
  <div class="code-runner-card" v-if="authStore.isLoggedIn">
    <div class="card-header">
      <h2>Запуск кода</h2>
    </div>

    <form @submit.prevent="runCode" class="runner-form">
      <!-- Параметры поля и цели -->
      <div class="params-grid">
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

      <!-- Список препятствий -->
      <div class="obstacles-section">
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
          spellcheck="false" placeholder="// Введите ваш код здесь..." required
          @keydown.tab.prevent="handleTab"></textarea>
      </div>

      <!-- Кнопка отправки -->
      <button type="submit" class="btn btn-primary" :disabled="isLoading">
        {{ isLoading ? 'Выполнение...' : 'Запустить код' }}
      </button>
    </form>

    <!-- Вывод ошибки запроса -->
    <div v-if="errorMessage" class="result-banner error">
      <strong>Ошибка:</strong> {{ errorMessage }}
    </div>

    <!-- Вывод результата RunCodeResult -->
    <div v-if="runResult !== null" class="result-banner" :class="runResult.success ? 'success' : 'failure'">
      <span class="result-icon">{{ runResult.success ? '✓' : '✕' }}</span>
      <div>
        <strong>{{ runResult.success ? 'Успех!' : 'Задача не решена' }}</strong>
        <p class="result-subtext">
          {{
            runResult.success
              ? 'Код успешно выполнен, цель достигнута.'
              : 'Код отработал, но цель не была достигнута (Success: false).'
          }}
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { nextTick, ref } from 'vue';
import axios from 'axios';
import { api } from '@/api/api';
import { useAuthStore } from '@/stores/authStore';

const authStore = useAuthStore();

// Типы, соответствующие C# DTO
interface Point {
  x: number;
  y: number;
}

interface RunCodeParams {
  height: number;
  width: number;
  goal: Point;
  obstacles: Point[];
  code: string;
}

interface RunCodeResult {
  success: boolean;
}

const codeTextarea = ref<HTMLTextAreaElement | null>(null);
const isLoading = ref(false);
const errorMessage = ref('');
const runResult = ref<RunCodeResult | null>(null);

const form = ref<RunCodeParams>({
  width: 10,
  height: 10,
  goal: { x: 5, y: 5 },
  obstacles: [
    { x: 2, y: 2 },
    { x: 3, y: 4 },
  ],
  code: '',
});

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

async function runCode() {
  isLoading.value = true;
  errorMessage.value = '';
  runResult.value = null;

  try {
    // Проверьте baseURL в вашем '@/api/api':
    // если baseURL уже '/api', используйте '/code/run', иначе '/api/code/run'
    const response = await api.post<RunCodeResult>('/code/run', form.value);

    // Учитываем возможный PascalCase (Success) из C#
    const raw = response.data;
    runResult.value = {
      success: Boolean(raw.success),
    };
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 401) {
        errorMessage.value = 'Требуется авторизация. Пожалуйста, войдите в систему.';
      } else {
        errorMessage.value =
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
    isLoading.value = false;
  }
}
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

.runner-form {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

/* Сетка параметров поля и цели */
.params-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
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

/* Кнопка запуска */
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
}
</style>
