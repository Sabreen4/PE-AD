export interface Submission {
  id: string;
  assignmentId: string;
  employeeId: string;
  employeeName: string;
  submittedAt: string;
  content: string;
  attachments: string[];
  score: number | null;
  feedback: string | null;
  gradedBy: string | null;
  gradedAt: string | null;
  createdAt: string;
  updatedAt: string;
}