<template>
  <div class="robot-arena">
    <div class="arena-header">
      <div class="arena-heading">
        <span class="arena-title">Поле {{ width }} × {{ height }}</span>
        <span class="arena-step">Шаг {{ frameIndex }} из {{ lastIndex }}</span>
      </div>

      <div class="arena-controls">
        <button
          type="button"
          class="arena-btn"
          :disabled="frames.length <= 1"
          @click="togglePlayback"
        >
          {{ playButtonLabel }}
        </button>
        <button
          type="button"
          class="arena-btn"
          :disabled="frames.length <= 1"
          @click="restart"
        >
          Сначала
        </button>
        <select v-model.number="speedMs" class="arena-speed" aria-label="Скорость анимации">
          <option :value="650">Медленно</option>
          <option :value="300">Обычно</option>
          <option :value="120">Быстро</option>
        </select>
      </div>
    </div>

    <div class="arena-viewport">
      <div class="arena-grid" :style="gridStyle">
        <div
          v-for="cell in cells"
          :key="cell.key"
          class="arena-cell"
          :class="cellClass(cell)"
          :title="`(${cell.x}, ${cell.y})`"
        ></div>

        <div class="robot" :style="robotStyle" :title="robotTitle">
          <span class="robot-arrow"></span>
        </div>
      </div>
    </div>

    <div class="arena-legend">
      <span><i class="swatch start"></i> старт</span>
      <span><i class="swatch goal"></i> цель</span>
      <span><i class="swatch obstacle"></i> препятствие</span>
      <span><i class="swatch visited"></i> путь</span>
    </div>

    <div class="arena-status" :class="statusType">
      <span class="status-dot"></span>
      <span class="status-text">{{ statusText }}</span>
      <span v-if="frames.length > 0" class="status-meta">
        ({{ currentFrame.x }}, {{ currentFrame.y }}) · {{ directionName }} · {{ actionLabel }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onUnmounted, ref, watch } from 'vue';
import type { Point, RobotFrame } from '@/robot/types';

interface ArenaCell {
  x: number;
  y: number;
  key: string;
  isObstacle: boolean;
  isGoal: boolean;
  isStart: boolean;
}

const props = defineProps<{
  width: number;
  height: number;
  start: Point;
  goal: Point;
  obstacles: Point[];
  frames: RobotFrame[];
  success: boolean;
  error: string | null;
}>();

const frameIndex = ref(0);
const isPlaying = ref(false);
const speedMs = ref(300);
const visited = ref<Set<string>>(new Set());

let timer: ReturnType<typeof setTimeout> | null = null;

const lastIndex = computed(() => Math.max(0, props.frames.length - 1));

const obstacleKeys = computed(() => new Set(props.obstacles.map((o) => `${o.x}:${o.y}`)));
const goalKey = computed(() => `${props.goal.x}:${props.goal.y}`);
const startKey = computed(() => `${props.start.x}:${props.start.y}`);

const cells = computed<ArenaCell[]>(() => {
  const result: ArenaCell[] = [];

  for (let row = 0; row < props.height; row++) {
    for (let x = 0; x < props.width; x++) {
      // Y растёт вверх, поэтому верхняя строка — максимальный Y.
      const y = props.height - 1 - row;
      const key = `${x}:${y}`;

      result.push({
        x,
        y,
        key,
        isObstacle: obstacleKeys.value.has(key),
        isGoal: key === goalKey.value,
        isStart: key === startKey.value && key !== goalKey.value,
      });
    }
  }

  return result;
});

const gridStyle = computed(() => ({
  gridTemplateColumns: `repeat(${props.width}, minmax(0, 1fr))`,
  gridTemplateRows: `repeat(${props.height}, minmax(0, 1fr))`,
  aspectRatio: `${props.width} / ${props.height}`,
}));

const currentFrame = computed<RobotFrame>(() => {
  const frame = props.frames[frameIndex.value];
  return (
    frame ?? {
      x: props.start.x,
      y: props.start.y,
      direction: 0,
      action: 'start',
    }
  );
});

const robotStyle = computed(() => ({
  left: `${((currentFrame.value.x + 0.5) / props.width) * 100}%`,
  top: `${((props.height - 1 - currentFrame.value.y + 0.5) / props.height) * 100}%`,
  width: `${(0.72 / props.width) * 100}%`,
  transform: `translate(-50%, -50%) rotate(${currentFrame.value.direction * 90}deg)`,
  transitionDuration: `${Math.min(speedMs.value, 300)}ms`,
}));

const robotTitle = computed(
  () =>
    `Робот: (${currentFrame.value.x}, ${currentFrame.value.y}), ${directionName.value}`,
);

const directionName = computed(() => {
  const names = ['север', 'восток', 'юг', 'запад'] as const;
  return names[currentFrame.value.direction] ?? '';
});

const actionLabel = computed(() => {
  switch (currentFrame.value.action) {
    case 'start':
      return 'старт';
    case 'move':
      return 'движение';
    case 'turn_left':
      return 'поворот налево';
    case 'turn_right':
      return 'поворот направо';
    default:
      return currentFrame.value.action;
  }
});

const isFinished = computed(() => !isPlaying.value && frameIndex.value >= lastIndex.value);

const statusType = computed<'error' | 'success' | 'failure' | 'running' | 'paused'>(() => {
  if (props.error) return 'error';
  if (isPlaying.value) return 'running';
  if (!isFinished.value) return 'paused';
  return props.success ? 'success' : 'failure';
});

const statusText = computed(() => {
  if (props.error) return props.error;
  if (isPlaying.value) return 'Выполняется...';
  if (!isFinished.value) return 'Пауза';
  return props.success ? 'Робот достиг цели!' : 'Робот не достиг цели.';
});

const playButtonLabel = computed(() => {
  if (isPlaying.value) return 'Пауза';
  if (frameIndex.value >= lastIndex.value) return 'Ещё раз';
  return 'Продолжить';
});

function cellClass(cell: ArenaCell) {
  return {
    obstacle: cell.isObstacle,
    goal: cell.isGoal,
    start: cell.isStart,
    visited: visited.value.has(cell.key) && !cell.isObstacle,
  };
}

function clearTimer() {
  if (timer !== null) {
    clearTimeout(timer);
    timer = null;
  }
}

function scheduleNext() {
  clearTimer();

  timer = setTimeout(() => {
    timer = null;

    if (!isPlaying.value) return;

    if (frameIndex.value < lastIndex.value) {
      frameIndex.value += 1;
      scheduleNext();
    } else {
      isPlaying.value = false;
    }
  }, speedMs.value);
}

function rebuildVisited(index: number) {
  const next = new Set<string>();

  for (let i = 0; i <= index; i++) {
    const frame = props.frames[i];
    if (frame) next.add(`${frame.x}:${frame.y}`);
  }

  visited.value = next;
}

function startPlayback() {
  isPlaying.value = true;
  scheduleNext();
}

function togglePlayback() {
  if (props.frames.length <= 1) return;

  if (isPlaying.value) {
    isPlaying.value = false;
    clearTimer();
    return;
  }

  if (frameIndex.value >= lastIndex.value) {
    frameIndex.value = 0;
    rebuildVisited(0);
  }

  startPlayback();
}

function restart() {
  if (props.frames.length <= 1) return;

  clearTimer();
  isPlaying.value = false;
  frameIndex.value = 0;
  rebuildVisited(0);
  startPlayback();
}

// Новый запуск — сбрасываем анимацию и сразу проигрываем.
watch(
  () => props.frames,
  (frames) => {
    clearTimer();
    isPlaying.value = false;
    frameIndex.value = 0;
    rebuildVisited(0);

    if (frames.length > 1) {
      startPlayback();
    }
  },
  { immediate: true },
);

watch(frameIndex, (index) => {
  const frame = props.frames[index];
  if (!frame) return;

  const key = `${frame.x}:${frame.y}`;
  if (!visited.value.has(key)) {
    visited.value.add(key);
  }
});

watch(speedMs, () => {
  if (isPlaying.value) {
    scheduleNext();
  }
});

onUnmounted(clearTimer);
</script>

<style scoped>
.robot-arena {
  margin-top: 24px;
  padding: 20px;
  background-color: #f8f9fa;
  border: 1px solid #e9ecef;
  border-radius: 14px;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.arena-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
}

.arena-heading {
  display: flex;
  align-items: baseline;
  gap: 10px;
  flex-wrap: wrap;
}

.arena-title {
  font-size: 14px;
  font-weight: 600;
  color: #333;
}

.arena-step {
  font-size: 12px;
  color: #777;
}

.arena-controls {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.arena-btn {
  padding: 6px 12px;
  font-size: 13px;
  font-weight: 500;
  border: none;
  border-radius: 7px;
  background-color: #e9ecef;
  color: #333;
  cursor: pointer;
  transition: background-color 0.15s ease;
}

.arena-btn:hover:not(:disabled) {
  background-color: #dde2e6;
}

.arena-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.arena-speed {
  padding: 6px 8px;
  font-size: 13px;
  border: 1px solid #dee2e6;
  border-radius: 7px;
  background-color: white;
  color: #333;
}

.arena-viewport {
  overflow-x: auto;
  padding: 2px;
}

.arena-grid {
  position: relative;
  display: grid;
  gap: 1px;
  width: 100%;
  max-width: 640px;
  margin: 0 auto;
  background-color: #ced4da;
  border: 1px solid #ced4da;
  border-radius: 8px;
  overflow: hidden;
}

.arena-cell {
  position: relative;
  min-width: 0;
  min-height: 0;
  background-color: #ffffff;
}

.arena-cell.visited {
  background-color: #e3f2fd;
}

.arena-cell.obstacle {
  background-color: #495057;
}

.arena-cell.goal {
  background-color: #d0e7ff;
}

.arena-cell.goal::after {
  content: '';
  position: absolute;
  inset: 22%;
  border-radius: 50%;
  background-color: #007bff;
}

.arena-cell.start {
  background-color: #e6f7ea;
}

.arena-cell.start::after {
  content: '';
  position: absolute;
  inset: 28%;
  border-radius: 50%;
  border: 2px solid #28a745;
  box-sizing: border-box;
}

.robot {
  position: absolute;
  aspect-ratio: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  background: radial-gradient(circle at 35% 30%, #ffc266, #ff9800 65%, #ef6c00);
  border: 1px solid #e65100;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.35);
  z-index: 2;
  pointer-events: none;
  transition-property: left, top, transform;
  transition-timing-function: ease;
}

.robot-arrow {
  width: 42%;
  height: 52%;
  background-color: #ffffff;
  clip-path: polygon(50% 0%, 100% 100%, 50% 74%, 0% 100%);
  filter: drop-shadow(0 1px 1px rgba(0, 0, 0, 0.35));
}

.arena-legend {
  display: flex;
  flex-wrap: wrap;
  gap: 14px;
  font-size: 12px;
  color: #666;
}

.arena-legend span {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.swatch {
  display: inline-block;
  width: 12px;
  height: 12px;
  border-radius: 3px;
  border: 1px solid #ced4da;
}

.swatch.start {
  background-color: #e6f7ea;
  border-color: #28a745;
}

.swatch.goal {
  background-color: #007bff;
  border-color: #007bff;
}

.swatch.obstacle {
  background-color: #495057;
  border-color: #495057;
}

.swatch.visited {
  background-color: #e3f2fd;
  border-color: #90caf9;
}

.arena-status {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 12px;
  border-radius: 9px;
  font-size: 14px;
  flex-wrap: wrap;
}

.status-dot {
  width: 9px;
  height: 9px;
  border-radius: 50%;
  background-color: currentColor;
  flex-shrink: 0;
}

.status-text {
  font-weight: 600;
}

.status-meta {
  font-size: 12px;
  opacity: 0.8;
}

.arena-status.running {
  background-color: #e7f1ff;
  color: #0b5ed7;
}

.arena-status.success {
  background-color: #d4edda;
  color: #155724;
}

.arena-status.failure,
.arena-status.error {
  background-color: #f8d7da;
  color: #721c24;
}

.arena-status.paused {
  background-color: #eef1f5;
  color: #555;
}

@media (max-width: 640px) {
  .robot-arena {
    padding: 14px;
  }

  .arena-header {
    align-items: flex-start;
  }
}
</style>
