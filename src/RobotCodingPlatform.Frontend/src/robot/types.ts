export interface Point {
  x: number;
  y: number;
}

/* ---------- Токены ---------- */

export interface BaseToken {
  line: number;
}

export interface CommandToken extends BaseToken {
  kind: 'command';
  text: string;
}

export interface NumberToken extends BaseToken {
  kind: 'number';
  value: number;
}

export interface OperatorToken extends BaseToken {
  kind: 'operator';
  text: string;
}

export interface MarkerToken extends BaseToken {
  kind: 'lparen' | 'rparen' | 'eol' | 'indent' | 'dedent';
}

export type Token = CommandToken | NumberToken | OperatorToken | MarkerToken;

/* ---------- AST ---------- */

export type Condition =
  | { type: 'sensor'; name: string }
  | { type: 'bool'; value: boolean }
  | { type: 'and'; left: Condition; right: Condition }
  | { type: 'or'; left: Condition; right: Condition }
  | { type: 'not'; inner: Condition };

export interface ActionCommandNode {
  type: 'action';
  name: string;
  args: number[];
  line: number;
}

export interface IfStatementNode {
  type: 'if';
  condition: Condition;
  children: Node[];
  elseChildren: Node[] | null;
  line: number;
}

export interface WhileStatementNode {
  type: 'while';
  condition: Condition;
  children: Node[];
  line: number;
}

export type Node = ActionCommandNode | IfStatementNode | WhileStatementNode;

/* ---------- Исполнение ---------- */

export type Direction = 0 | 1 | 2 | 3; // North, East, South, West

export interface RobotFrame {
  x: number;
  y: number;
  direction: Direction;
  action: string;
}

export interface RobotMapConfig {
  width: number;
  height: number;
  start: Point;
  goal: Point;
  obstacles: Point[];
}

export interface RobotRunResult {
  frames: RobotFrame[];
  success: boolean;
  error: string | null;
  iterations: number;
}
