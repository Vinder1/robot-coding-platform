import type { Robot } from './robot';
import type { Condition, Node } from './types';

/**
 * Повторяет Interpreter из RobotCodingPlatform.Core.
 * Счётчик итераций общий на весь запуск, лимит — 1000.
 */
export class Interpreter {
  private iterations = 0;

  constructor(
    private readonly robot: Robot,
    private readonly maxIterations = 1000,
  ) {}

  interpret(nodes: readonly Node[]): void {
    for (const node of nodes) {
      this.executeNode(node);
    }
  }

  private executeNode(node: Node): void {
    this.iterations++;
    if (this.iterations >= this.maxIterations) {
      throw new Error('Max iterations reached');
    }

    switch (node.type) {
      case 'action':
        this.robot.executeCommand(node.name, node.args);
        break;

      case 'if':
        if (this.evaluate(node.condition)) {
          this.interpret(node.children);
        } else if (node.elseChildren) {
          this.interpret(node.elseChildren);
        }
        break;

      case 'while':
        while (this.evaluate(node.condition)) {
          this.interpret(node.children);
        }
        break;
    }
  }

  private evaluate(condition: Condition): boolean {
    switch (condition.type) {
      case 'bool':
        return condition.value;
      case 'sensor':
        return this.robot.getSensor(condition.name);
      case 'and':
        return this.evaluate(condition.left) && this.evaluate(condition.right);
      case 'or':
        return this.evaluate(condition.left) || this.evaluate(condition.right);
      case 'not':
        return !this.evaluate(condition.inner);
    }
  }
}
