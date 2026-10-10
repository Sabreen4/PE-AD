export interface Employee {
  id: string;
  userId: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  organizationId: string;
  organizationName: string;
  isActive: boolean;
  enrolledProgramIds: string[];
  createdAt: string;
  updatedAt: string;
}
