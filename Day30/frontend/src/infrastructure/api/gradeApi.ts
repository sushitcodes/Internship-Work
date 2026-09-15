import { createApi } from "@reduxjs/toolkit/query/react";
import { baseQueryWithAuth } from "./baseQueryWithAuth";

export interface SubjectDto {
  id: string;
  classRoomId: string;
  name: string;
}

export interface CreateSubjectRequest {
  classRoomId: string;
  name: string;
}

export interface GradeRosterEntry {
  enrollmentId: string;
  studentUserId: string;
  studentName: string;
  gradeId: string | null;
  marksObtained: number | null;
  maxMarks: number;
  remarks: string | null;
}

export interface SubmitGradeEntry {
  enrollmentId: string;
  marksObtained: number;
  maxMarks: number;
  remarks?: string;
}

export interface SubmitGradesRequest {
  subjectId: string;
  entries: SubmitGradeEntry[];
}

export interface SubjectGrade {
  subjectName: string;
  marksObtained: number | null;
  maxMarks: number;
  remarks: string | null;
}

export interface StudentReportCard {
  studentUserId: string;
  studentName: string;
  subjects: SubjectGrade[];
}

export const gradeApi = createApi({
  reducerPath: "gradeApi",
  baseQuery: baseQueryWithAuth,
  tagTypes: ["Subject", "Grade"],
  endpoints: (builder) => ({
    getSubjectsByClass: builder.query<SubjectDto[], string>({
      query: (classRoomId) => `/subjects/class/${classRoomId}`,
      providesTags: (_r, _e, classRoomId) => [
        { type: "Subject", id: classRoomId },
      ],
    }),
    createSubject: builder.mutation<SubjectDto, CreateSubjectRequest>({
      query: (body) => ({ url: "/subjects", method: "POST", body }),
      invalidatesTags: (_r, _e, arg) => [
        { type: "Subject", id: arg.classRoomId },
      ],
    }),
    deleteSubject: builder.mutation<void, { id: string; classRoomId: string }>({
      query: ({ id }) => ({ url: `/subjects/${id}`, method: "DELETE" }),
      invalidatesTags: (_r, _e, arg) => [
        { type: "Subject", id: arg.classRoomId },
      ],
    }),
    getGradeRoster: builder.query<GradeRosterEntry[], string>({
      query: (subjectId) => `/grades/roster/${subjectId}`,
      providesTags: ["Grade"],
    }),
    submitGrades: builder.mutation<void, SubmitGradesRequest>({
      query: (body) => ({ url: "/grades/submit", method: "POST", body }),
      invalidatesTags: ["Grade"],
    }),
    getMyReportCard: builder.query<StudentReportCard, string>({
      query: (classRoomId) => `/grades/report-card/me/${classRoomId}`,
      providesTags: ["Grade"],
    }),
  }),
});

export const {
  useGetSubjectsByClassQuery,
  useCreateSubjectMutation,
  useDeleteSubjectMutation,
  useGetGradeRosterQuery,
  useSubmitGradesMutation,
  useGetMyReportCardQuery,
} = gradeApi;
