import { createApi } from "@reduxjs/toolkit/query/react";
import { baseQueryWithAuth } from "./baseQueryWithAuth";

export interface AttendanceRecordDto {
  id: string;
  enrollmentId: string;
  studentUserId: string;
  studentEmail: string;
  date: string;
  status: string;
}

export interface MarkAttendanceEntry {
  enrollmentId: string;
  status: string;
}

export interface MarkAttendanceRequest {
  classRoomId: string;
  date: string;
  entries: MarkAttendanceEntry[];
}

export const attendanceApi = createApi({
  reducerPath: "attendanceApi",
  baseQuery: baseQueryWithAuth,
  tagTypes: ["Attendance"],
  endpoints: (builder) => ({
    markAttendance: builder.mutation<void, MarkAttendanceRequest>({
      query: (body) => ({ url: "/attendance/mark", method: "POST", body }),
      invalidatesTags: ["Attendance"],
    }),
  }),
});

export const { useMarkAttendanceMutation } = attendanceApi;
