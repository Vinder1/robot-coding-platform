<template>
  <div class="challenge-create-card">
    <div class="card-header">
      <h2>Создание challenge</h2>
      <span class="endpoint-badge">POST api/challenges</span>
    </div>

    <div v-if="errorMessages.length > 0" class="error-message">
      <div v-for="(message, index) in errorMessages" :key="index">{{ message }}</div>
    </div>

    <div v-if="successMessage" class="success-message">{{ successMessage }}</div>

    <form class="challenge-form" @submit.prevent="submit">
      <div class="form-field">
        <label for="challenge-name">Название</label>
        <input
          id="challenge-name"
          v-model.trim="form.name"
          type="text"
          maxlength="100"
          placeholder="Например, Первый лабиринт"
          :disabled="isSubmitting"
        />
      </div>

      <div class="form-field">
        <label for="challenge-description">Описание</label>
        <textarea
          id="challenge-description"
          v-model="form.description"
          rows="6"
          maxlength="10000"
          placeholder="Большой текст с описанием задания..."
          :disabled="isSubmitting"
        ></textarea>
        <span class="field-hint">{{ form.description.length }} / 10000</span>
      </div>

      <div class="form-grid">
        <div class="form-field">
          <label for="challenge-width">Ширина поля</label>
          <input
            id="challenge-width"
            v-model.number="form.width"
            type="number"
            min="1"
            max="100"
            :disabled="isSubmitting"
          />
        </div>
        <div class="form-field">
          <label for="challenge-height">Высота поля</label>
          <input
            id="challenge-height"
            v-model.number="form.height"
            type="number"
            min="1"
            max="100"
            :disabled="isSubmitting"
          />
        </div>
      </div>

      <fieldset class="points-group" :disabled="isSubmitting">
        <legend>Стартовая точка</legend>
        <div class="form-grid">
          <div class="form-field">
            <label for="challenge-start-x">X</label>
            <input id="challenge-start-x" v-model.number="form.startX" type="number" min="0" />
          </div>
          <div class="form-field">
            <label for="challenge-start-y">Y</label>
            <input id="challenge-start-y" v-model.number="form.startY" type="number" min="0" />
          </div>
        </div>
      </fieldset>

      <fieldset class="points-group" :disabled="isSubmitting">
        <legend>Цель</legend>
        <div class="form-grid">
          <div class="form-field">
            <label for="challenge-goal-x">X</label>
            <input id="challenge-goal-x" v-model.number="form.goalX" type="number" min="0" />
          </div>
          <div class="form-field">
            <label for="challenge-goal-y">Y</label>
            <input id="challenge-goal-y" v-model.number="form.goalY" type="number" min="0" />
          </div>
        </div>
      </fieldset>

      <div class="obstacles-section">
        <div class="obstacles-header">
          <span class="section-title">Препятствия ({{ form.obstacles.length }})</span>
          <button
            type="button"
            class="btn btn-small"
            :disabled="isSubmitting"
            @click="form.obstacles = []"
          >
            Очистить
          </button>
        </div>

        <div class="obstacle-add-row">
          <input
            v-model.number="obstacleDraft.x"
            type="number"
            min="0"
            placeholder="X"
            :disabled="isSubmitting"
          />
          <input
            v-model.number="obstacleDraft.y"
            type="number"
            min="0"
            placeholder="Y"
            :disabled="isSubmitting"
          />
          <button
            type="button"
            class="btn btn-secondary"
            :disabled="isSubmitting"
            @click="addObstacle"
          >
            Добавить
          </button>
        </div>

        <div v-if="form.obstacles.length === 0" class="no-obstacles">Препятствий пока нет.</div>
        <div v-else class="obstacles-list">
          <button
            v-for="(obstacle, index) in form.obstacles"
            :key="`${obstacle.x}:${obstacle.y}`"
            type="button"
            class="obstacle-chip"
            :title="'Удалить препятствие'"
            :disabled="isSubmitting"
            @click="removeObstacle(index)"
          >
            ({{ obstacle.x }}, {{ obstacle.y }}) ×
          </button>
        </div>

        <p class="field-hint">
          Кликните по клетке на карте ниже, чтобы поставить или убрать препятствие.
        </p>

        <div class="map-preview-wrapper">
          <div
            class="map-preview"
            :style="{
              gridTemplateColumns: `repeat(${safeWidth}, minmax(0, 1fr))`,
              width: `${previewWidthPx}px`
            }"
          >
            <button
              v-for="cell in cells"
              :key="`${cell.x}:${cell.y}`"
              type="button"
              class="map-cell"
              :class="cellClass(cell)"
              :title="`(${cell.x}, ${cell.y})`"
              :disabled="isSubmitting"
              @click="toggleObstacle(cell.x, cell.y)"
            ></button>
          </div>
        </div>
      </div>

      <div class="form-actions">
        <button type="submit" class="btn btn-primary" :disabled="isSubmitting">
          {{ isSubmitting ? 'Создание...' : 'Создать challenge' }}
        </button>
        <button type="button" class="btn btn-secondary" :disabled="isSubmitting" @click="resetForm()">
          Сбросить
        </button>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import axios from 'axios';
import { api } from '@/api/api';

interface ChallengeObstacle {
  x: number;
  y: number;
}

interface ChallengeDto {
  id: string;
  name: string;
}

const emit = defineEmits<{
  created: [id: string];
}>();

const MAX_SIZE = 100;
const MAX_PREVIEW_PX = 560;
const CELL_GAP_PX = 2;

const form = reactive({
  name: '',
  description: '',
  width: 10,
  height: 10,
  startX: 0,
  startY: 0,
  goalX: 9,
  goalY: 9,
  obstacles: [] as ChallengeObstacle[]
});

const obstacleDraft = reactive({ x: 0, y: 0 });
const isSubmitting = ref(false);
const errorMessages = ref<string[]>([]);
const successMessage = ref('');

const safeWidth = computed(() => clampDimension(form.width));
const safeHeight = computed(() => clampDimension(form.height));

const previewWidthPx = computed(() => {
  const maxCell = Math.floor((MAX_PREVIEW_PX - CELL_GAP_PX * (safeWidth.value - 1)) / safeWidth.value);
  const cell = Math.max(6, Math.min(28, maxCell));
  return cell * safeWidth.value + CELL_GAP_PX * (safeWidth.value - 1);
});

const cells = computed(() => {
  const result: ChallengeObstacle[] = [];
  for (let y = 0; y < safeHeight.value; y++) {
    for (let x = 0; x < safeWidth.value; x++) {
      result.push({ x, y });
    }
  }
  return result;
});

function clampDimension(value: unknown): number {
  const parsed = Number(value);
  if (!Number.isFinite(parsed)) return 1;
  return Math.min(MAX_SIZE, Math.max(1, Math.trunc(parsed)));
}

function isStart(x: number, y: number): boolean {
  return form.startX === x && form.startY === y;
}

function isGoal(x: number, y: number): boolean {
  return form.goalX === x && form.goalY === y;
}

function isObstacle(x: number, y: number): boolean {
  return form.obstacles.some((obstacle) => obstacle.x === x && obstacle.y === y);
}

function cellClass(cell: ChallengeObstacle) {
  return {
    start: isStart(cell.x, cell.y),
    goal: isGoal(cell.x, cell.y),
    obstacle: isObstacle(cell.x, cell.y)
  };
}

function toggleObstacle(x: number, y: number) {
  if (isStart(x, y) || isGoal(x, y)) return;

  const index = form.obstacles.findIndex((obstacle) => obstacle.x === x && obstacle.y === y);
  if (index >= 0) {
    form.obstacles.splice(index, 1);
  } else {
    form.obstacles.push({ x, y });
  }
}

function removeObstacle(index: number) {
  form.obstacles.splice(index, 1);
}

function addObstacle() {
  errorMessages.value = [];

  const x = Number(obstacleDraft.x);
  const y = Number(obstacleDraft.y);

  if (!Number.isInteger(x) || !Number.isInteger(y)) {
    errorMessages.value = ['Координаты препятствия должны быть целыми числами.'];
    return;
  }

  if (x < 0 || x >= safeWidth.value || y < 0 || y >= safeHeight.value) {
    errorMessages.value = ['Препятствие должно находиться внутри карты.'];
    return;
  }

  if (isStart(x, y)) {
    errorMessages.value = ['Стартовая точка не может совпадать с препятствием.'];
    return;
  }

  if (isGoal(x, y)) {
    errorMessages.value = ['Цель не может совпадать с препятствием.'];
    return;
  }

  if (isObstacle(x, y)) {
    errorMessages.value = ['Такое препятствие уже добавлено.'];
    return;
  }

  form.obstacles.push({ x, y });
}

async function submit() {
  errorMessages.value = [];
  successMessage.value = '';

  const clientErrors = validateForm();
  if (clientErrors.length > 0) {
    errorMessages.value = clientErrors;
    return;
  }

  isSubmitting.value = true;

  try {
    const { data } = await api.post<ChallengeDto>('/challenges', {
      name: form.name.trim(),
      description: form.description.trim(),
      width: form.width,
      height: form.height,
      startX: form.startX,
      startY: form.startY,
      goalX: form.goalX,
      goalY: form.goalY,
      obstacles: form.obstacles.map((obstacle) => ({ x: obstacle.x, y: obstacle.y }))
    });

    successMessage.value = `Challenge «${data.name}» создан.`;
    resetForm(false);
    emit('created', data.id);
  } catch (error) {
    errorMessages.value = describeErrors(error);
  } finally {
    isSubmitting.value = false;
  }
}

function validateForm(): string[] {
  const errors: string[] = [];

  if (!form.name.trim()) errors.push('Название обязательно.');
  if (!form.description.trim()) errors.push('Описание обязательно.');

  const width = Number(form.width);
  const height = Number(form.height);
  if (!Number.isInteger(width) || width < 1 || width > MAX_SIZE) {
    errors.push(`Ширина должна быть от 1 до ${MAX_SIZE}.`);
  }
  if (!Number.isInteger(height) || height < 1 || height > MAX_SIZE) {
    errors.push(`Высота должна быть от 1 до ${MAX_SIZE}.`);
  }

  if (Number.isInteger(width) && Number.isInteger(height) && errors.length === 0) {
    if (!Number.isInteger(form.startX) || form.startX < 0 || form.startX >= width) {
      errors.push('Стартовая точка должна находиться внутри карты.');
    }
    if (!Number.isInteger(form.startY) || form.startY < 0 || form.startY >= height) {
      errors.push('Стартовая точка должна находиться внутри карты.');
    }
    if (!Number.isInteger(form.goalX) || form.goalX < 0 || form.goalX >= width) {
      errors.push('Цель должна находиться внутри карты.');
    }
    if (!Number.isInteger(form.goalY) || form.goalY < 0 || form.goalY >= height) {
      errors.push('Цель должна находиться внутри карты.');
    }
    if (form.startX === form.goalX && form.startY === form.goalY) {
      errors.push('Цель не может совпадать со стартовой точкой.');
    }
    if (form.obstacles.some((o) => o.x === form.startX && o.y === form.startY)) {
      errors.push('Стартовая точка не может совпадать с препятствием.');
    }
    if (form.obstacles.some((o) => o.x === form.goalX && o.y === form.goalY)) {
      errors.push('Цель не может совпадать с препятствием.');
    }
  }

  return [...new Set(errors)];
}

function describeErrors(error: unknown): string[] {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as
      | { errors?: unknown; message?: string; error?: string }
      | undefined;

    if (Array.isArray(data?.errors)) {
      return data.errors.map((item) => String(item));
    }
    if (typeof data?.errors === 'object' && data.errors !== null) {
      return Object.values(data.errors as Record<string, unknown>)
        .flat()
        .map((item) => String(item));
    }
    if (data?.message) return [data.message];
    if (data?.error) return [data.error];

    if (error.response?.status === 401) return ['Требуется авторизация администратора.'];
    if (error.response?.status === 403) return ['Недостаточно прав для создания challenge.'];

    return ['Не удалось создать challenge.'];
  }

  if (error instanceof Error) return [error.message];
  return ['Не удалось создать challenge.'];
}

function resetForm(clearMessages = true) {
  form.name = '';
  form.description = '';
  form.width = 10;
  form.height = 10;
  form.startX = 0;
  form.startY = 0;
  form.goalX = 9;
  form.goalY = 9;
  form.obstacles = [];
  obstacleDraft.x = 0;
  obstacleDraft.y = 0;

  if (clearMessages) {
    errorMessages.value = [];
    successMessage.value = '';
  }
}

// Убираем препятствия, которые вышли за пределы карты после изменения размера,
// и подтягиваем старт/цель внутрь поля.
watch(
  () => [safeWidth.value, safeHeight.value] as const,
  ([width, height]) => {
    form.obstacles = form.obstacles.filter((o) => o.x < width && o.y < height);

    if (form.startX >= width) form.startX = width - 1;
    if (form.startY >= height) form.startY = height - 1;
    if (form.goalX >= width) form.goalX = width - 1;
    if (form.goalY >= height) form.goalY = height - 1;
  }
);
</script>

<style scoped>
.challenge-create-card {
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

/* Форма */
.challenge-form {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 14px;
}

.form-field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.form-field label {
  font-size: 13px;
  font-weight: 600;
  color: #555;
}

.form-field input,
.form-field textarea,
.obstacle-add-row input {
  padding: 10px 12px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 14px;
  font-family: inherit;
  transition: border-color 0.2s ease;
  box-sizing: border-box;
  width: 100%;
}

.form-field textarea {
  resize: vertical;
  min-height: 120px;
  line-height: 1.5;
}

.form-field input:focus,
.form-field textarea:focus,
.obstacle-add-row input:focus {
  outline: none;
  border-color: #007bff;
}

.field-hint {
  margin: 0;
  font-size: 12px;
  color: #888;
}

.points-group {
  border: 1px solid #e9ecef;
  border-radius: 10px;
  padding: 14px 16px 16px;
  margin: 0;
}

.points-group legend {
  padding: 0 6px;
  font-size: 13px;
  font-weight: 600;
  color: #555;
}

/* Препятствия */
.obstacles-section {
  display: flex;
  flex-direction: column;
  gap: 12px;
  border: 1px solid #e9ecef;
  border-radius: 10px;
  padding: 16px;
}

.obstacles-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
}

.section-title {
  font-size: 13px;
  font-weight: 600;
  color: #555;
}

.obstacle-add-row {
  display: flex;
  gap: 10px;
  align-items: center;
}

.obstacle-add-row input {
  width: 90px;
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
  cursor: pointer;
  transition: all 0.15s ease;
}

.obstacle-chip:hover:not(:disabled) {
  border-color: #dc3545;
  color: #dc3545;
}

/* Карта */
.map-preview-wrapper {
  overflow-x: auto;
  padding: 4px 0;
}

.map-preview {
  display: grid;
  gap: 2px;
  background-color: #f1f3f5;
  padding: 2px;
  border-radius: 8px;
  max-width: 100%;
}

.map-cell {
  aspect-ratio: 1;
  border: none;
  border-radius: 2px;
  background-color: white;
  cursor: pointer;
  padding: 0;
  transition: background-color 0.12s ease;
}

.map-cell:hover:not(:disabled) {
  background-color: #dbe9ff;
}

.map-cell.obstacle {
  background-color: #495057;
}

.map-cell.start {
  background-color: #28a745;
  cursor: not-allowed;
}

.map-cell.goal {
  background-color: #007bff;
  cursor: not-allowed;
}

/* Кнопки */
.form-actions {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  padding-top: 8px;
  border-top: 2px solid #f0f0f0;
}

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
  background-color: #0069d9;
}

.btn-secondary {
  background-color: #e9ecef;
  color: #333;
}

.btn-secondary:hover:not(:disabled) {
  background-color: #dde2e6;
}

.btn-small {
  padding: 6px 12px;
  font-size: 13px;
  background-color: #e9ecef;
  color: #333;
}

.btn-small:hover:not(:disabled) {
  background-color: #dde2e6;
}

/* Сообщения */
.error-message {
  margin-bottom: 20px;
  padding: 12px;
  background-color: #f8d7da;
  color: #721c24;
  border-radius: 8px;
  font-size: 14px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.success-message {
  margin-bottom: 20px;
  padding: 12px;
  background-color: #d4edda;
  color: #155724;
  border-radius: 8px;
  font-size: 14px;
}

@media (max-width: 640px) {
  .challenge-create-card {
    padding: 24px;
  }

  .form-grid {
    grid-template-columns: 1fr;
  }

  .form-actions {
    flex-direction: column;
  }

  .btn {
    width: 100%;
  }
}
</style>
