import { RobotSyntaxError } from './syntaxError';
import type {
  CommandToken,
  Condition,
  Node,
  Token,
} from './types';

const KEYWORDS = new Set(['if', 'else', 'while', 'and', 'or', 'not', 'true', 'false']);

function tokenToString(token: Token): string {
  switch (token.kind) {
    case 'command':
      return `Command '${token.text}'`;
    case 'number':
      return `Number '${token.value}'`;
    case 'operator':
      return `Operator '${token.text}'`;
    default:
      return token.kind;
  }
}

/**
 * Повторяет Parser из RobotCodingPlatform.Core:
 * команды с числовыми аргументами, if/else/while,
 * условия and/or/not и датчики.
 */
export class Parser {
  private position = 0;

  constructor(private readonly tokens: Token[]) {}

  static parse(tokens: Token[]): Node[] {
    const parser = new Parser(tokens);
    const result = parser.parseBlock();

    if (parser.position < tokens.length) {
      const token = tokens[parser.position]!;
      throw new RobotSyntaxError(`Unexpected token '${tokenToString(token)}'`, token.line);
    }

    return result;
  }

  private parseBlock(): Node[] {
    const result: Node[] = [];

    while (this.position < this.tokens.length) {
      const token = this.tokens[this.position]!;

      if (token.kind === 'command') {
        this.position++;
        result.push(this.parseCommandNode(token));
        continue;
      }

      if (token.kind === 'indent') {
        if (result.length === 0) {
          throw new RobotSyntaxError('Unexpected indent', token.line);
        }

        const parent = result[result.length - 1]!;

        if (parent.type !== 'if' && parent.type !== 'while') {
          throw new RobotSyntaxError('This statement cannot have a nested block', token.line);
        }

        this.position++;
        const children = this.parseBlock();
        parent.children = children;

        const dedent = this.tokens[this.position];
        if (!dedent || dedent.kind !== 'dedent') {
          throw new RobotSyntaxError('Block is not closed', token.line);
        }
        this.position++;

        if (parent.type === 'if') {
          const next = this.tokens[this.position];
          if (next && next.kind === 'command' && next.text === 'else') {
            this.position++;

            const eol = this.tokens[this.position];
            if (!eol || eol.kind !== 'eol') {
              throw new RobotSyntaxError("Expected end of line after 'else'", next.line);
            }
            this.position++;

            const indent = this.tokens[this.position];
            if (!indent || indent.kind !== 'indent') {
              throw new RobotSyntaxError("'else' requires an indented block", next.line);
            }
            this.position++;

            parent.elseChildren = this.parseBlock();

            const elseDedent = this.tokens[this.position];
            if (!elseDedent || elseDedent.kind !== 'dedent') {
              throw new RobotSyntaxError("'else' block is not closed", next.line);
            }
            this.position++;
          }
        }

        continue;
      }

      if (token.kind === 'dedent') {
        return result;
      }

      if (token.kind === 'number') {
        throw new RobotSyntaxError(`Unexpected number '${token.value}'`, token.line);
      }

      if (token.kind === 'operator') {
        throw new RobotSyntaxError(`Unexpected operator '${token.text}'`, token.line);
      }

      throw new RobotSyntaxError(`Unexpected '${token.kind}'`, token.line);
    }

    return result;
  }

  private parseCommandNode(keyword: CommandToken): Node {
    const name = keyword.text;
    const line = keyword.line;
    const isConditional = name === 'if' || name === 'while';

    if (!isConditional && KEYWORDS.has(name)) {
      throw new RobotSyntaxError(`'${name}' cannot be used as a command`, line);
    }

    const condition = isConditional ? this.parseCondition() : null;
    const args: number[] = [];
    let endOfLine = false;

    while (!endOfLine && this.position < this.tokens.length) {
      const token = this.tokens[this.position]!;

      if (token.kind === 'number') {
        if (isConditional) {
          throw new RobotSyntaxError(
            `Unexpected argument '${token.value}' after condition`,
            token.line,
          );
        }
        args.push(token.value);
        this.position++;
      } else if (token.kind === 'eol') {
        endOfLine = true;
        this.position++;
      } else if (token.kind === 'command') {
        throw new RobotSyntaxError(`Expected end of line, got '${token.text}'`, token.line);
      } else if (token.kind === 'operator') {
        throw new RobotSyntaxError(`Unexpected operator '${token.text}'`, token.line);
      } else {
        throw new RobotSyntaxError(`Unexpected '${token.kind}'`, token.line);
      }
    }

    if (!endOfLine) {
      throw new RobotSyntaxError('Expected end of line, got end of input', line);
    }

    if (isConditional) {
      const next = this.tokens[this.position];
      if (!next || next.kind !== 'indent') {
        throw new RobotSyntaxError(`'${name}' requires an indented block`, line);
      }
    }

    if (name === 'if') {
      return { type: 'if', condition: condition!, children: [], elseChildren: null, line };
    }

    if (name === 'while') {
      return { type: 'while', condition: condition!, children: [], line };
    }

    return { type: 'action', name, args, line };
  }

  private parseCondition(): Condition {
    return this.parseOr();
  }

  private parseOr(): Condition {
    let left = this.parseAnd();

    while (this.position < this.tokens.length) {
      const token = this.tokens[this.position]!;
      if (token.kind !== 'command' || token.text !== 'or') break;

      this.position++;
      left = { type: 'or', left, right: this.parseAnd() };
    }

    return left;
  }

  private parseAnd(): Condition {
    let left = this.parseNot();

    while (this.position < this.tokens.length) {
      const token = this.tokens[this.position]!;
      if (token.kind !== 'command' || token.text !== 'and') break;

      this.position++;
      left = { type: 'and', left, right: this.parseNot() };
    }

    return left;
  }

  private parseNot(): Condition {
    const token = this.tokens[this.position];

    if (token && token.kind === 'command' && token.text === 'not') {
      this.position++;
      return { type: 'not', inner: this.parseNot() };
    }

    return this.parsePrimary();
  }

  private parsePrimary(): Condition {
    if (this.position >= this.tokens.length) {
      const line = this.tokens.length > 0 ? this.tokens[this.tokens.length - 1]!.line : 1;
      throw new RobotSyntaxError('Expected condition, got end of input', line);
    }

    const token = this.tokens[this.position]!;

    if (token.kind === 'lparen') {
      this.position++;
      const inner = this.parseCondition();

      const closing = this.tokens[this.position];
      if (!closing || closing.kind !== 'rparen') {
        throw new RobotSyntaxError("Expected ')'", closing?.line ?? token.line);
      }
      this.position++;
      return inner;
    }

    if (token.kind !== 'command') {
      throw new RobotSyntaxError(
        `Expected condition, got '${tokenToString(token)}'`,
        token.line,
      );
    }

    if (token.text === 'true' || token.text === 'false') {
      this.position++;
      return { type: 'bool', value: token.text === 'true' };
    }

    if (KEYWORDS.has(token.text)) {
      throw new RobotSyntaxError(`Unexpected keyword '${token.text}'`, token.line);
    }

    this.position++;
    return { type: 'sensor', name: token.text };
  }
}

export function parse(tokens: Token[]): Node[] {
  return Parser.parse(tokens);
}
