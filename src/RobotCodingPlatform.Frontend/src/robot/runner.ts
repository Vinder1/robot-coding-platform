import { GameMap } from './gameMap';
import { Interpreter } from './interpreter';
import { tokenize } from './lexer';
import { parse } from './parser';
import { Robot } from './robot';
import type { RobotFrame, RobotMapConfig, RobotRunResult } from './types';

/** Защита браузера от бесконечной анимации (`move 1000000` и т.п.). */
const MAX_FRAMES = 10_000;

/**
 * Полностью клиентское исполнение программы робота.
 * Возвращает кадры для анимации, признак достижения цели и текст ошибки.
 */
export function runRobotProgram(code: string, config: RobotMapConfig): RobotRunResult {
  const frames: RobotFrame[] = [
    {
      x: config.start.x,
      y: config.start.y,
      direction: 0,
      action: 'start',
    },
  ];

  try {
    const tokens = tokenize(code);
    const ast = parse(tokens);

    const map = new GameMap(config.width, config.height, config.goal, config.obstacles);

    const robot = new Robot(map, config.start.x, config.start.y, (step) => {
      if (frames.length >= MAX_FRAMES) {
        throw new Error(`Слишком много шагов (лимит ${MAX_FRAMES})`);
      }

      frames.push({
        x: step.x,
        y: step.y,
        direction: step.direction,
        action: step.action,
      });
    });

    const interpreter = new Interpreter(robot);
    interpreter.interpret(ast);

    return {
      frames,
      success: robot.isGoalReached,
      error: null,
    };
  } catch (error) {
    return {
      frames,
      success: false,
      error: error instanceof Error ? error.message : String(error),
    };
  }
}
