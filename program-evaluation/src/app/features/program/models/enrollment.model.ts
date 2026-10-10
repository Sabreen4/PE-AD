export interface Enrollment {
  id: string;
  programId: string;
  programName: string;
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  enrolledAt: string;
  isActive: boolean;
  completedAt: string | null;
  currentWeek?: number;
  createdAt: string;
  updatedAt: string;
}