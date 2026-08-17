import {
  ADD_SUBMISSION,
  CLEAR_SUBMISSIONS,
  DELETE_SUBMISSION,
  SET_SUBMISSIONS,
} from "../../types/formTypes";

import type {
  AddSubmissionAction,
  ClearSubmissionsAction,
  DeleteSubmissionAction,
  SetSubmissionsAction,
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

export const setSubmissions = (
  submissions: FormData[],
): SetSubmissionsAction => ({
  type: SET_SUBMISSIONS,
  payload: submissions,
});
