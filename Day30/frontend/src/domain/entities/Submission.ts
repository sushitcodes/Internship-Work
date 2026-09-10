// Pure business types. No imports from React, Redux, or Axios —
// this file would survive a full framework rewrite untouched.

// export interface EducationEntry {
//   institution: string;
//   degree: string;
//   year: number;
// }

export interface Submission {
  id: string;
  fullName: string;
  classRoomId: string;
  classRoomName: string;
  rollNo: number;
  fileUrl: string;
  createdAt: string;
  submitterAvatarUrl: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
