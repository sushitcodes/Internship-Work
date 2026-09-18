import { api } from "./api";
export interface ClassRoomDto {
  id: string;
  name: string;
  academicYear: number;
  studentCount: number;
  classTeacherUserId: string | null;
  classTeacherName: string | null;
}

export interface CreateClassRoomRequest {
  name: string;
  academicYear: number;
}

export const classRoomApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getClassRooms: builder.query<ClassRoomDto[], void>({
      query: () => "/classrooms",
      providesTags: ["ClassRoom"],
    }),
    createClassRoom: builder.mutation<ClassRoomDto, CreateClassRoomRequest>({
      query: (body) => ({ url: "/classrooms", method: "POST", body }),
      invalidatesTags: ["ClassRoom"],
    }),
    assignClassTeacher: builder.mutation<
      void,
      { classRoomId: string; teacherUserId: string | null }
    >({
      query: ({ classRoomId, teacherUserId }) => ({
        url: `/classrooms/${classRoomId}/class-teacher`,
        method: "PUT",
        body: { teacherUserId },
      }),
      invalidatesTags: ["ClassRoom"],
    }),
  }),
});

export const {
  useGetClassRoomsQuery,
  useCreateClassRoomMutation,
  useAssignClassTeacherMutation,
} = classRoomApi;
