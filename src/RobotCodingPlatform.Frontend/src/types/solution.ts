export type SolutionAttemptStatus = 'Pending' | 'Succeeded' | 'Failed';

export interface SolutionAttemptSummary {
  id: string;
  challengeId: string;
  challengeName: string;
  userId: string;
  status: SolutionAttemptStatus;
  success: boolean | null;
  error: string | null;
  iterations: number | null;
  createdAt: string;
  completedAt: string | null;
}

export interface SolutionAttempt extends SolutionAttemptSummary {
  code: string;
}
