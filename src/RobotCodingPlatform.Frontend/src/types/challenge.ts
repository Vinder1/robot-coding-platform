export interface ChallengeObstacle {
  x: number;
  y: number;
}

export interface ChallengeSummary {
  id: string;
  name: string;
  width: number;
  height: number;
  startX: number;
  startY: number;
  goalX: number;
  goalY: number;
  obstacleCount: number;
  createdAt: string;
}

export interface Challenge extends Omit<ChallengeSummary, 'obstacleCount'> {
  description: string;
  obstacles: ChallengeObstacle[];
}
