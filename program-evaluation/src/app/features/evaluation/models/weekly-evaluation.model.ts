export interface WeeklyEvaluation {
  id: string;
  employeeId: string;
  employeeName: string;
  weekStart: string;
  weekEnd: string;
  score: number;
  maxScore: number;
  criteria: WeeklyEvaluationCriterion[];
  comments: string | null;
  evaluatedBy: string;
  evaluatedAt: string;
  createdAt: string;
  updatedAt: string;
}

export interface WeeklyEvaluationCriterion {
  name: string;
  score: number;
  maxScore: number;
  comment: string | null;
}
