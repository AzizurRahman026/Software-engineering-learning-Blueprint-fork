export enum NotificationType {
  Info = 0,
  Warning = 1,
  Success = 2,
  Error = 3
}

export interface NotificationDto {
  message: string;
  type: NotificationType;
  timestamp: string;
}
