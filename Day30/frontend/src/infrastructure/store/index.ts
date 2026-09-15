import { configureStore } from "@reduxjs/toolkit";
import { submissionApi } from "../api/submissionApi";
import { authApi } from "../api/authApi";
import authReducer from "./authSlice";
import { classRoomApi } from "../api/classRoomApi";
import { enrollmentApi } from "../api/enrollmentApi";
import { userApi } from "../api/userApi";
import { attendanceApi } from "../api/attendanceApi";
import { dashboardApi } from "../api/dashboardApi";
import { gradeApi } from "../api/gradeApi";
export const store = configureStore({
  reducer: {
    [submissionApi.reducerPath]: submissionApi.reducer,

    [authApi.reducerPath]: authApi.reducer,
    [classRoomApi.reducerPath]: classRoomApi.reducer,
    [enrollmentApi.reducerPath]: enrollmentApi.reducer,
    [userApi.reducerPath]: userApi.reducer,
    [attendanceApi.reducerPath]: attendanceApi.reducer,
    [dashboardApi.reducerPath]: dashboardApi.reducer,
    [gradeApi.reducerPath]: gradeApi.reducer,
    auth: authReducer,
  },

  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(
      submissionApi.middleware,
      authApi.middleware,
      classRoomApi.middleware,
      enrollmentApi.middleware,
      userApi.middleware,
      attendanceApi.middleware,
      dashboardApi.middleware,
      gradeApi.middleware,
    ),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
export default store;
