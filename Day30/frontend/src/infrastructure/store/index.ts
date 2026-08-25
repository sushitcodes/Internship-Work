import { configureStore } from "@reduxjs/toolkit";
import { submissionApi } from "../api/submissionApi";
import { authApi } from "../api/authApi";
import authReducer from "./authSlice";
export const store = configureStore({
  reducer: {
    [submissionApi.reducerPath]: submissionApi.reducer,
    [authApi.reducerPath]: authApi.reducer,
    auth: authReducer,
  },

  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(submissionApi.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
export default store;
