// matching what the backend's UserProfileDto actually returns.
export interface UserProfile {
  userId: string;
  email: string;
  fullName: string;
  phone: string;
  avatarUrl?: string;
  memberNumber: number;
}
