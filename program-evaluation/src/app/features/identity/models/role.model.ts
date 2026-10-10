export interface Role {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
  featureIds: string[];
  createdAt: string;
  updatedAt: string;
}
