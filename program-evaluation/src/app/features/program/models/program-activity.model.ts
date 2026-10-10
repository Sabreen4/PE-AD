export interface ProgramActivity {
  id: string;
  programWeekId: string;
  name: string;
  description: string | null;
  activityType: string;
  dueDate: string | null;
  maxScore: number | null;
  isMandatory: boolean;
  createdAt: string;
  updatedAt: string;
}