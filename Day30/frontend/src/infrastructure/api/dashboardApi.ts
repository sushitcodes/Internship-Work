import { Submission } from "../../domain/entities/Submission";
import { AttendanceRecordDto } from "./attendanceApi";
import { api } from "./api";
export interface StatusCount {
  status: string;
  count: number;
}
export interface DashboardSummary {
  totalStudents: number;
  totalStaff: number;
  totalSubmissions: number;
  todayAttendanceBreakdown: StatusCount[];
  recentSubmissions: Submission[];
}

export interface MyDashboard {
  mySubmissionsCount: number;
  myRecentSubmissions: Submission[];
  myAttendancePercentage: number;
  myRecentAttendance: AttendanceRecordDto[];
}

export const dashboardApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getDashboardSummary: builder.query<DashboardSummary, void>({
      query: () => "/dashboard/summary",
      providesTags: ["Dashboard"],
    }),
    getMyDashboard: builder.query<MyDashboard, void>({
      query: () => "/dashboard/me",
      providesTags: ["Dashboard"],
    }),
  }),
});

export const { useGetDashboardSummaryQuery, useGetMyDashboardQuery } =
  dashboardApi;
