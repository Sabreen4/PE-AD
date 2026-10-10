export interface UserFeature {
  id: string;
  userId: string;
  featureId: string;
  canView: boolean;
  canCreate: boolean;
  canEdit: boolean;
  canDelete: boolean;
}
