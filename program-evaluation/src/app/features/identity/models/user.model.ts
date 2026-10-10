export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  organizationId: string;
  organizationName: string;
  roles: string[];
  role: string;
  phone?: string | null;
  createdAt: string;
  updatedAt: string;
}
