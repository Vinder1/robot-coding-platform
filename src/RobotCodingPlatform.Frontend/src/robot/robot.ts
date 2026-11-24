import type { GameMap } from './gameMap';
import type { Direction } from './types';

export interface RobotStepInfo {
  x: number;
  y: number;
  direction: Direction;
  action: string;
}

export type StepListener = (info: RobotStepInfo) => void;

function delta(direction: Direction): readonly [number, number] {
  switch (direction) {
    case 0: // North
      return [0, 1];
    case 1: // East
      return [1, 0];
    case 2: // South
      return [0, -1];
    case 3: // West
      return [-1, 0];
  }
}

/**
 * Повторяет Robot из RobotCodingPlatform.Core.
 * Начальное направление — North (0), Y растёт вверх.
 * Каждый шаг/поворот дополнительно отдаётся в onStep для анимации.
 */
export class Robot {
  x: number;
  y: number;
  direction: Direction = 0;

  constructor(
    private readonly map: GameMap,
    x: number,
    y: number,
    private readonly onStep?: StepListener,
  ) {
    this.x = x;
    this.y = y;
  }

  get isGoalReached(): boolean {
    return this.x === this.map.goal.x && this.y === this.map.goal.y;
  }

  executeCommand(name: string, args: readonly number[]): void {
    switch (name.toLowerCase()) {
      case 'move': {
        const steps = args.length > 0 ? args[0]! : 1;
        this.move(steps);
        break;
      }

      case 'turn_left':
        this.turn(-1, 'turn_left');
        break;

      case 'turn_right':
        this.turn(1, 'turn_right');
        break;

      default:
        throw new Error(`Unknown command '${name}'`);
    }
  }

  getSensor(name: string): boolean {
    switch (name.toLowerCase()) {
      case 'wall_forward':
        return this.isWallAtRelative(0);
      case 'wall_left':
        return this.isWallAtRelative(-1);
      case 'wall_right':
        return this.isWallAtRelative(1);
      default:
        throw new Error(`Unknown sensor '${name}'`);
    }
  }

  private move(steps: number): void {
    for (let i = 0; i < steps; i++) {
      const [dx, dy] = delta(this.direction);
      const nextX = this.x + dx;
      const nextY = this.y + dy;

      if (!this.map.isPassable(nextX, nextY)) {
        throw new Error(`Robot crashed into a wall or obstacle at (${nextX}, ${nextY})!`);
      }

      this.x = nextX;
      this.y = nextY;
      this.onStep?.({ x: this.x, y: this.y, direction: this.direction, action: 'move' });
    }
  }

  private turn(deltaDirection: number, action: string): void {
    this.direction = (((this.direction + deltaDirection + 4) % 4) as Direction);
    this.onStep?.({ x: this.x, y: this.y, direction: this.direction, action });
  }

  private isWallAtRelative(relativeOffset: number): boolean {
    const targetDirection = (((this.direction + relativeOffset + 4) % 4) as Direction);
    const [dx, dy] = delta(targetDirection);
    return !this.map.isPassable(this.x + dx, this.y + dy);
  }
}
