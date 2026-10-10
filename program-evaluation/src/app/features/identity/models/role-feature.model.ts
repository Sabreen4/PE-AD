export interface RoleFeature {
  id: string;
  roleId: string;
  featureId: string;
  canView: boolean;
  canCreate: boolean;
  canEdit: boolean;
  canDelete: boolean;
}
