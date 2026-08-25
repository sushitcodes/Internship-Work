import { configureStore } from "@reduxjs/toolkit";
import { submissionApi } from "../api/submissionApi";

export const store = configureStore({
  reducer: {
    [submissionApi.reducerPath]: submissionApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(submissionApi.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
export default store;
