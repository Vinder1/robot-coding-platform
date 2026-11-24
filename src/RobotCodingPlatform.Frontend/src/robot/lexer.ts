import { RobotSyntaxError } from './syntaxError';
import type { Token } from './types';

const TAB_WIDTH = 4;
const INT_MAX = 2_147_483_647;

const isLetter = (ch: string) => /\p{L}/u.test(ch);
const isLetterOrDigit = (ch: string) => /[\p{L}\p{Nd}]/u.test(ch);
const isDigit = (ch: string) => ch >= '0' && ch <= '9';

/**
 * Повторяет Lexer из RobotCodingPlatform.Core:
 * нижний регистр, комментарии `#`, отступы кратны 4,
 * Indent/Dedent/EndOfLine-маркеры.
 */
export function tokenize(text: string): Token[] {
  const normalized = text.toLowerCase();
  const tokens: Token[] = [];
  const indents: number[] = [0];
  const lines = normalized.split('\n');

  for (let i = 0; i < lines.length; i++) {
    const lineNumber = i + 1;
    let line = lines[i] ?? '';

    const commentIndex = line.indexOf('#');
    if (commentIndex >= 0) {
      line = line.slice(0, commentIndex);
    }

    if (line.trim() === '') {
      continue;
    }

    let position = 0;
    let indentWidth = 0;

    while (position < line.length) {
      const ch = line[position]!;
      if (ch === ' ') {
        indentWidth += 1;
      } else if (ch === '\t') {
        indentWidth += TAB_WIDTH;
      } else {
        break;
      }
      position++;
    }

    if (indentWidth % TAB_WIDTH !== 0) {
      throw new RobotSyntaxError('Incorrect indentation', lineNumber);
    }

    const currentIndent = indents[indents.length - 1]!;

    if (indentWidth > currentIndent) {
      indents.push(indentWidth);
      tokens.push({ kind: 'indent', line: lineNumber });
    } else if (indentWidth < currentIndent) {
      while (indents.length > 1 && indentWidth < indents[indents.length - 1]!) {
        indents.pop();
        tokens.push({ kind: 'dedent', line: lineNumber });
      }

      if (indentWidth !== indents[indents.length - 1]!) {
        throw new RobotSyntaxError('Incorrect indentation', lineNumber);
      }
    }

    while (position < line.length) {
      const current = line[position]!;

      if (/\s/.test(current)) {
        position++;
        continue;
      }

      const start = position;

      if (isLetter(current) || current === '_') {
        position++;
        while (position < line.length) {
          const next = line[position]!;
          if (!isLetterOrDigit(next) && next !== '_') break;
          position++;
        }

        tokens.push({ kind: 'command', text: line.slice(start, position), line: lineNumber });
      } else if (isDigit(current)) {
        position++;
        while (position < line.length && isDigit(line[position]!)) {
          position++;
        }

        const numericText = line.slice(start, position);
        const value = Number.parseInt(numericText, 10);

        if (!Number.isSafeInteger(value) || value > INT_MAX) {
          throw new RobotSyntaxError(`Incorrect number '${numericText}'`, lineNumber);
        }

        tokens.push({ kind: 'number', value, line: lineNumber });
      } else if (current === '(') {
        tokens.push({ kind: 'lparen', line: lineNumber });
        position++;
      } else if (current === ')') {
        tokens.push({ kind: 'rparen', line: lineNumber });
        position++;
      } else if (current === '<' || current === '>') {
        position++;
        if (position < line.length && line[position] === '=') {
          position++;
        }
        tokens.push({ kind: 'operator', text: line.slice(start, position), line: lineNumber });
      } else if (current === '=') {
        position++;
        if (position < line.length && line[position] === '=') {
          position++;
          tokens.push({ kind: 'operator', text: '==', line: lineNumber });
        } else {
          throw new RobotSyntaxError("Expected '==', got '='", lineNumber);
        }
      } else if (current === '!') {
        position++;
        if (position < line.length && line[position] === '=') {
          position++;
          tokens.push({ kind: 'operator', text: '!=', line: lineNumber });
        } else {
          throw new RobotSyntaxError("Expected '!=', got '!'", lineNumber);
        }
      } else {
        throw new RobotSyntaxError(`Unexpected symbol '${current}'`, lineNumber);
      }
    }

    tokens.push({ kind: 'eol', line: lineNumber });
  }

  while (indents.length > 1) {
    indents.pop();
    tokens.push({ kind: 'dedent', line: lines.length });
  }

  return tokens;
}
