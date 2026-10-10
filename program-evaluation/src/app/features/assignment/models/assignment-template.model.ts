export interface AssignmentTemplate {
  id: string;
  name: string;
  description: string | null;
  programWeekId: string;
  completionDays: number;
  dueDate: string | null;
  maxScore: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}