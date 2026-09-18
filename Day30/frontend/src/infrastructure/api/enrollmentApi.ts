import { api } from "./api";
export interface EnrollmentDto {
  id: string;
  studentUserId: string;
  studentEmail: string;
  studentFullName: string;
  rollNo: number;
  classRoomId: string;
  classRoomName: string;
  enrolledAt: string;
}

export interface CreateEnrollmentRequest {
  studentUserId: string;
  classRoomId: string;
}
export const enrollmentApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getEnrollmentsByClass: builder.query<EnrollmentDto[], string>({
      query: (classRoomId) => `/enrollments/class/${classRoomId}`,
      providesTags: (_r, _e, classRoomId) => [
        { type: "Enrollment", id: classRoomId },
      ],
    }),
    enrollStudent: builder.mutation<EnrollmentDto, CreateEnrollmentRequest>({
      query: (body) => ({ url: "/enrollments", method: "POST", body }),
      invalidatesTags: (_r, _e, arg) => [
        { type: "Enrollment", id: arg.classRoomId },
        "ClassRoom",
      ],
    }),
    removeEnrollment: builder.mutation<
      void,
      { studentUserId: string; classRoomId: string }
    >({
      query: ({ studentUserId, classRoomId }) => ({
        url: `/enrollments/${studentUserId}/class/${classRoomId}`,
        method: "DELETE",
      }),
      invalidatesTags: (_r, _e, arg) => [
        { type: "Enrollment", id: arg.classRoomId },
      ],
    }),
    getMyClass: builder.query<EnrollmentDto, void>({
      query: () => "/enrollments/me/class",
      providesTags: ["Enrollment"],
    }),
  }),
});

export const {
  useGetEnrollmentsByClassQuery,
  useEnrollStudentMutation,
  useRemoveEnrollmentMutation,
  useGetMyClassQuery,
} = enrollmentApi;
