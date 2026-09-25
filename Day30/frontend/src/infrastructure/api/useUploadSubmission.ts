import { useCallback } from "react";
import axios, { AxiosError } from "axios";
import { useAppDispatch } from "../store/hooks";
import { api } from "./api";
import {
  startUpload,
  updateUploadProgress,
  removeUpload,
} from "../store/uploadProgressSlice";
import type { Submission } from "../../domain/entities/Submission";

export function useUploadSubmission() {
  const dispatch = useAppDispatch();

  const runUpload = useCallback(
    (
      method: "POST" | "PUT",
      url: string,
      formData: FormData,
      fileName: string,
      // NEW — called synchronously with the id, BEFORE the network
      // request goes out. This is what lets a caller (FormPage) start
      // watching this exact upload's progress from the same Redux
      // slice the list page already reads, no second source of truth.
      onStart?: (id: string) => void,
    ): Promise<Submission> => {
      const tempId = crypto.randomUUID();
      dispatch(startUpload({ id: tempId, fileName }));
      onStart?.(tempId);

      return axios
        .request<Submission>({
          method,
          url: `${import.meta.env.VITE_API_URL ?? ""}${url}`,
          data: formData,
          withCredentials: true,
          onUploadProgress: (progressEvent) => {
            const percent = Math.round((progressEvent.progress ?? 0) * 100);
            dispatch(updateUploadProgress({ id: tempId, progress: percent }));
          },
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
            "Upload failed put file less than 10mb. Please try again.";
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
