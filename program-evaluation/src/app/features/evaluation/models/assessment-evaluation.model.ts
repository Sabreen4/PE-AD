export interface AssessmentEvaluation {
  id: string;
  employeeId: string;
  employeeName: string;
  assessmentType: string;
  score: number;
  maxScore: number;
  weight: number;
  conductedAt: string;
  evaluatedBy: string;
  comments: string | null;
  createdAt: string;
  updatedAt: string;
}
