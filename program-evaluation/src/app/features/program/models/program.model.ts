export interface Program {
  id: string;
  name: string;
  code: string;
  description: string | null;
  evaluatorId?: number;
  durationWeeks?: number;
  startDate: string;
  endDate: string;
  organizationId: string;
  organizationName: string;
  isActive: boolean;
  totalWeeks: number;
  createdAt: string;
  updatedAt: string;
}
