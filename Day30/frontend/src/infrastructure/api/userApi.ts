import { UserProfile } from "../../domain/entities/UserProfile";
import { PagedResult } from "../../domain/entities/Submission";
import { api } from "./api";
export interface UserSummaryDto {
  id: string;
  email: string;
  fullName: string;
}
export interface CreateUserRequest {
  email: string;
  temporaryPassword: string;
  roles: string[];
}

export interface CreatedUser {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
}

export const userApi = api.injectEndpoints({
  // new — getStudents doesn't need tags, profile endpoints do
  endpoints: (builder) => ({
    getStudents: builder.query<UserSummaryDto[], void>({
      query: () => "/users/students",
    }),
    createUser: builder.mutation<CreatedUser, CreateUserRequest>({
      query: (body) => ({
        url: "/users",
        method: "POST",
        body,
      }),
      invalidatesTags: ["UserProfile", "Submission"], // so UsersListPage refetches and shows the new person
    }),
    getOwnProfile: builder.query<UserProfile, void>({
      query: () => "/users/me/profile",
      providesTags: ["UserProfile"],
    }),

    updateOwnProfile: builder.mutation<UserProfile, FormData>({
      query: (formData) => ({
        url: "/users/me/profile",
        method: "PUT",
        body: formData,
      }),
      invalidatesTags: ["UserProfile", "Submission"], // so UsersListPage refetches and shows the new person
    }),
    updateUserName: builder.mutation<
      UserProfile,
      { id: string; fullName: string }
    >({
      query: ({ id, fullName }) => ({
        url: `/users/${id}/name`,
        method: "PATCH",
        body: { fullName },
      }),
      invalidatesTags: ["UserProfile", "Submission"],
    }),

    setUserActiveStatus: builder.mutation<
      void,
      { id: string; isActive: boolean }
    >({
      query: ({ id, isActive }) => ({
        url: `/users/${id}/status`,
        method: "PATCH",
        body: { isActive },
      }),
      invalidatesTags: ["UserProfile", "Submission"],
    }),

    searchUsers: builder.infiniteQuery<
      PagedResult<UserProfile>,
      { search?: string; rollNo?: number; role?: string; pageSize: number },
      number
    >({
      infiniteQueryOptions: {
        initialPageParam: 1,
        getNextPageParam: (lastPage) =>
          lastPage.hasNextPage ? lastPage.page + 1 : undefined,
      },
      query: ({ queryArg, pageParam }) => {
        const params = new URLSearchParams({
          page: String(pageParam),
          pageSize: String(queryArg.pageSize),
        });
        if (queryArg.search) params.set("search", queryArg.search);
        if (queryArg.rollNo) params.set("rollNo", String(queryArg.rollNo));
        if (queryArg.role) params.set("role", queryArg.role);
        return `/users?${params.toString()}`;
      },
      providesTags: ["UserProfile"],
    }),
    getUserProfileById: builder.query<UserProfile, string>({
      query: (id) => `/users/${id}/profile`,
      providesTags: ["UserProfile"],
    }),
  }),
});

export const {
  useGetStudentsQuery,
  useGetOwnProfileQuery,
  useUpdateOwnProfileMutation,
  useSearchUsersInfiniteQuery,
  useGetUserProfileByIdQuery,
  useCreateUserMutation,
  useSetUserActiveStatusMutation,
  useUpdateUserNameMutation,
} = userApi;
