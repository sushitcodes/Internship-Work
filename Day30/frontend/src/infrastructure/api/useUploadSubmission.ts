import { useCallback } from "react";
import axios, { AxiosError } from "axios";
import { useAppDispatch } from "../store/hooks";
import { api } from "./api";
import {
  startUpload,
  updateUploadProgress,
  uploadFailed,
  removeUpload,
} from "../store/uploadProgressSlice";
import type { Submission } from "../../domain/entities/Submission";
import { refreshSession } from "./baseQueryWithAuth";
export function useUploadSubmission() {
  const dispatch = useAppDispatch();

  const runUpload = useCallback(
    (
      method: "POST" | "PUT",
      url: string,
      formData: FormData,
      fileName: string,
      onStart?: (id: string) => void,
    ): Promise<Submission> => {
      const tempId = crypto.randomUUID();
      dispatch(startUpload({ id: tempId, fileName }));
      onStart?.(tempId);

      const send = () =>
        axios.request<Submission>({
          method,
          url: `${import.meta.env.VITE_API_URL ?? ""}${url}`,
          data: formData,
          withCredentials: true,
          onUploadProgress: (progressEvent) => {
            const percent = Math.round((progressEvent.progress ?? 0) * 100);
            dispatch(updateUploadProgress({ id: tempId, progress: percent }));
          },
        });

      return send()
        .catch(async (err: AxiosError) => {
          if (err.response?.status === 401 && (await refreshSession()))
            return send();
          throw err;
        })
        .then((response) => {
          dispatch(api.util.invalidateTags(["Submission", "Dashboard"]));
          dispatch(removeUpload({ id: tempId }));
          return response.data;
        })
        .catch((err: AxiosError<{ message?: string; title?: string }>) => {
          const message =
            err.response?.data?.message ??
            err.response?.data?.title ??
            (typeof err.response?.data === "string"
              ? err.response.data
              : null) ??
            "Upload failed. Use a PDF, JPG or PNG under 5 MB and try again.";

          // Mark the row as failed so the list shows "Failed" and the reason,
          // then remove it by itself after a few seconds.
          dispatch(uploadFailed({ id: tempId, errorMessage: message }));
          setTimeout(() => dispatch(removeUpload({ id: tempId })), 8000);

          throw new Error(message);
        });
    },
    [dispatch],
  );

  const uploadCreate = useCallback(
    (formData: FormData, fileName: string, onStart?: (id: string) => void) =>
      runUpload("POST", "/submissions", formData, fileName, onStart),
    [runUpload],
  );

  const uploadUpdate = useCallback(
    (
      id: string,
      formData: FormData,
      fileName: string,
      onStart?: (id: string) => void,
    ) => runUpload("PUT", `/submissions/${id}`, formData, fileName, onStart),
    [runUpload],
  );

  return { uploadCreate, uploadUpdate };
}
