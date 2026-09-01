import { configureStore } from "@reduxjs/toolkit";
import { submissionApi } from "../api/submissionApi";
import { authApi } from "../api/authApi";
import authReducer from "./authSlice";
import { classRoomApi } from "../api/classRoomApi";
import { enrollmentApi } from "../api/enrollmentApi";
import { userApi } from "../api/userApi";

export const store = configureStore({
  reducer: {
    [submissionApi.reducerPath]: submissionApi.reducer,

    [authApi.reducerPath]: authApi.reducer,
    [classRoomApi.reducerPath]: classRoomApi.reducer,
    [enrollmentApi.reducerPath]: enrollmentApi.reducer,
    [userApi.reducerPath]: userApi.reducer,
    auth: authReducer,
  },

  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(
      submissionApi.middleware,
      authApi.middleware,
      classRoomApi.middleware,
      enrollmentApi.middleware,
      userApi.middleware,
    ),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
export default store;
