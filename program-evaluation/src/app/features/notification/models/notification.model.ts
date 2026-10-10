export interface NotificationDto {
  id: number;
  title: string;
  message: string;
  channel: string;
  isRead: boolean;
  createdAt: string;
}

export interface PagedNotificationResponse {
  items: NotificationDto[];
  pageNumber: number;
  pageSize: number;
  totalRecords: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
