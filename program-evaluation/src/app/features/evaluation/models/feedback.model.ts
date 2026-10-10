export interface Feedback {
  id: string;
  employeeId: string;
  employeeName: string;
  providerId: string;
  providerName: string;
  type: string;
  content: string;
  rating: number | null;
  isAnonymous: boolean;
  submittedAt: string;
  createdAt: string;
  updatedAt: string;
}
