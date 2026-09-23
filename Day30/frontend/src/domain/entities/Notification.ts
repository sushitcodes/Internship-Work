// Mirrors the server's NotificationDto. Keep the field names in sync.
export interface Notification {
  id: string;
  title: string;
  body: string;
  link?: string | null;
  kind: "submission" | "grade" | "generic";
  isRead: boolean;
  createdAt: string;
}
