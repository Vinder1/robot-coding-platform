export class RobotSyntaxError extends Error {
  constructor(message: string, line: number) {
    super(`${message} (line ${line})`);
    this.name = 'RobotSyntaxError';
  }
}
