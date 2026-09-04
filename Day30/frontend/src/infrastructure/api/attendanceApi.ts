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
export interface AttendanceRosterEntry {
  enrollmentId: string;
  studentUserId: string;
  studentEmail: string;
  attendanceRecordId: string | null;
  status: string; // "Present" | "Absent" | "Late" | "Excused" | "Unmarked"
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
    getRoster: builder.query<
      AttendanceRosterEntry[],
      { classRoomId: string; date: string }
    >({
      query: ({ classRoomId, date }) =>
        `/attendance/roster/${classRoomId}?date=${date}`,
      providesTags: ["Attendance"],
    }),
  }),
});

export const { useMarkAttendanceMutation, useGetRosterQuery } = attendanceApi;
