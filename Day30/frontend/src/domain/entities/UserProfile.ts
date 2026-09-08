// matching what the backend's UserProfileDto actually returns.
export interface UserProfile {
  userId: string;
  email: string;
  fullName: string;
  address: string;
  gender?: string | null;
  phoneNumbers: string[];
  avatarUrl?: string | null;
  memberNumber: number;
  isActive: boolean;
}
