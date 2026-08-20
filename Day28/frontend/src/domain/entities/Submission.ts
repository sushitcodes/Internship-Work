// Pure business types. No imports from React, Redux, or Axios —
// this file would survive a full framework rewrite untouched.

export interface EducationEntry {
  institution: string;
  degree: string;
  year: number;
}

export interface Submission {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  education: EducationEntry[];
  fileUrl: string;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
