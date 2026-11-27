import type { Robot } from './robot';
import type { Condition, Node } from './types';

/**
 * Повторяет Interpreter из RobotCodingPlatform.Core.
 */
export class Interpreter {
  private iterations = 0;

  constructor(
    private readonly robot: Robot,
    private readonly maxIterations = 10_000,
  ) {}

  get iterationCount(): number {
    return this.iterations;
  }

  interpret(nodes: readonly Node[]): void {
    for (const node of nodes) {
      this.executeNode(node);
    }
  }

  private executeNode(node: Node): void {
    this.bumpIterations();

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
          this.bumpIterations();
          this.interpret(node.children);
        }
        break;
    }
  }

  private bumpIterations(): void {
    this.iterations++;

    if (this.iterations > this.maxIterations) {
      throw new Error(`Execution limit exceeded: more than ${this.maxIterations} iterations.`);
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
