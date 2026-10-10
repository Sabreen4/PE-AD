export interface EmployeeAssignment {
  id: string;
  templateId: string;
  templateName: string;
  employeeId: string;
  employeeName: string;
  assignedDate: string;
  dueDate: string;
  status: string;
  score: number | null;
  submittedAt: string | null;
  createdAt: string;
  updatedAt: string;
}