import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export interface PendingUpload {
  id: string; // client-generated — the server hasn't assigned a real id yet
  fileName: string;
  progress: number; // 0-100
  status: "uploading" | "error";
  errorMessage?: string;
}

interface UploadProgressState {
  // Keyed by id, not an array, so any component can look up or remove
  // ONE specific upload without scanning the whole list every render.
  byId: Record<string, PendingUpload>;
}

const initialState: UploadProgressState = { byId: {} };

const uploadProgressSlice = createSlice({
  name: "uploadProgress",
  initialState,
  reducers: {
    startUpload: (
      state,
      action: PayloadAction<{ id: string; fileName: string }>,
    ) => {
      state.byId[action.payload.id] = {
        id: action.payload.id,
        fileName: action.payload.fileName,
        progress: 0,
        status: "uploading",
      };
    },
    updateUploadProgress: (
      state,
      action: PayloadAction<{ id: string; progress: number }>,
    ) => {
      const entry = state.byId[action.payload.id];
      if (entry) entry.progress = action.payload.progress;
    },
    uploadFailed: (
      state,
      action: PayloadAction<{ id: string; errorMessage: string }>,
    ) => {
      const entry = state.byId[action.payload.id];
      if (entry) {
        entry.status = "error";
        entry.errorMessage = action.payload.errorMessage;
      }
    },
    removeUpload: (state, action: PayloadAction<{ id: string }>) => {
      delete state.byId[action.payload.id];
    },
  },
});

export const { startUpload, updateUploadProgress, uploadFailed, removeUpload } =
  uploadProgressSlice.actions;
export default uploadProgressSlice.reducer;
