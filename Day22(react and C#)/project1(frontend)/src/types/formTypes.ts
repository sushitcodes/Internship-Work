export interface Education {
  id: string;
  degree: string;
  year: string;
  school: string;
}

export interface FormData {
  id: string;
  name: string;
  email: string;
  education: Education[];
  message: string;
  submittedAt: string;
}

export interface FormState {
  submissions: FormData[];
}

// Action Types
export const ADD_SUBMISSION = "ADD_SUBMISSION";
export const CLEAR_SUBMISSIONS = "CLEAR_SUBMISSIONS";
export const DELETE_SUBMISSION = "DELETE_SUBMISSION";
export const SET_SUBMISSIONS = "SET_SUBMISSIONS";

//  Action Interfaces
export interface AddSubmissionAction {
  type: typeof ADD_SUBMISSION;
  payload: Omit<FormData, "id" | "submittedAt">;
  [key: string]: unknown;
}
export interface ClearSubmissionsAction {
  type: typeof CLEAR_SUBMISSIONS;
  [key: string]: unknown;
}

export interface DeleteSubmissionAction {
  type: typeof DELETE_SUBMISSION;
  payload: string;
  [key: string]: unknown;
}
export interface SetSubmissionsAction {
  type: typeof SET_SUBMISSIONS;
  payload: FormData[];
  [key: string]: unknown;
}

//  Union Type - Must include all three!
export type FormActionTypes =
  | AddSubmissionAction
  | ClearSubmissionsAction
  | DeleteSubmissionAction
  | SetSubmissionsAction; // ← Must include this!
