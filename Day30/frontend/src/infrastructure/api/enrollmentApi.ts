import { createApi } from "@reduxjs/toolkit/query/react";
import { baseQueryWithAuth } from "./baseQueryWithAuth";

export interface EnrollmentDto {
  id: string;
  studentUserId: string;
  studentEmail: string;
  classRoomId: string;
  enrolledAt: string;
}

export interface CreateEnrollmentRequest {
  studentUserId: string;
  classRoomId: string;
}

export const enrollmentApi = createApi({
  reducerPath: "enrollmentApi",
  baseQuery: baseQueryWithAuth,
  tagTypes: ["Enrollment"],
  endpoints: (builder) => ({
    getEnrollmentsByClass: builder.query<EnrollmentDto[], string>({
      query: (classRoomId) => `/enrollments/class/${classRoomId}`,
      providesTags: ["Enrollment"],
    }),
    enrollStudent: builder.mutation<EnrollmentDto, CreateEnrollmentRequest>({
      query: (body) => ({ url: "/enrollments", method: "POST", body }),
      invalidatesTags: ["Enrollment"],
    }),
  }),
});

export const { useGetEnrollmentsByClassQuery, useEnrollStudentMutation } =
  enrollmentApi;
