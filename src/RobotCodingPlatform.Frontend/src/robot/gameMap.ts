import type { Point } from './types';

/**
 * Повторяет GameMap из RobotCodingPlatform.Core:
 * клетка проходима, если она внутри поля и не занята препятствием.
 */
export class GameMap {
  private readonly obstacleKeys: Set<string>;

  constructor(
    readonly width: number,
    readonly height: number,
    readonly goal: Point,
    obstacles: readonly Point[] = [],
  ) {
    this.obstacleKeys = new Set(obstacles.map((o) => `${o.x}:${o.y}`));
  }

  isPassable(x: number, y: number): boolean {
    if (x < 0 || x >= this.width || y < 0 || y >= this.height) {
      return false;
    }

    return !this.obstacleKeys.has(`${x}:${y}`);
  }
}
