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
export interface AttendanceSheetRow {
  enrollmentId: string;
  studentName: string;
  statusByDate: Record<string, string>;
}
export interface AttendanceSheet {
  dates: string[];
  rows: AttendanceSheetRow[];
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
    getAttendanceSheet: builder.query<
      AttendanceSheet,
      { classRoomId: string; startDate: string; endDate: string }
    >({
      query: ({ classRoomId, startDate, endDate }) =>
        `/attendance/sheet/${classRoomId}?startDate=${startDate}&endDate=${endDate}`,
      providesTags: ["Attendance"],
    }),
  }),
});

export const {
  useMarkAttendanceMutation,
  useGetRosterQuery,
  useGetAttendanceSheetQuery,
} = attendanceApi;

export async function downloadAttendanceSheet(
  classRoomId: string,
  startDate: string,
  endDate: string,
) {
  const apiOrigin = import.meta.env.VITE_API_URL ?? "";
  const res = await fetch(
    `${apiOrigin}/attendance/sheet/${classRoomId}/export?startDate=${startDate}&endDate=${endDate}`,
    { credentials: "include" }, // same cookie-based auth as everything else
  );
  if (!res.ok) throw new Error("Export failed");

  const blob = await res.blob();
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = `attendance_${startDate}_to_${endDate}.xlsx`;
  a.click();
  window.URL.revokeObjectURL(url); // free the blob URL once the download's triggered
}
