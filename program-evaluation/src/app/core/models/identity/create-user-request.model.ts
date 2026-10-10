export interface CreateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  organizationId: number;
  roleType: string;
  gender: string;
}
