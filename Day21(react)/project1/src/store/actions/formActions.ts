import {
  ADD_SUBMISSION,
  CLEAR_SUBMISSIONS,
  DELETE_SUBMISSION, // ← Must import this!
} from "../../types/formTypes";

import type {
  AddSubmissionAction,
  ClearSubmissionsAction,
  DeleteSubmissionAction, // ← Must import this!
  FormData,
} from "../../types/formTypes";

export const addSubmission = (
  formData: Omit<FormData, "id" | "submittedAt">,
): AddSubmissionAction => ({
  type: ADD_SUBMISSION,
  payload: formData,
});

export const clearSubmissions = (): ClearSubmissionsAction => ({
  type: CLEAR_SUBMISSIONS,
});

export const deleteSubmission = (id: string): DeleteSubmissionAction => ({
  type: DELETE_SUBMISSION,
  payload: id,
});
