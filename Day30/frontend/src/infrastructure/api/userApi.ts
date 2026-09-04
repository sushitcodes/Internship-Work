import { createApi } from "@reduxjs/toolkit/query/react";
import { baseQueryWithAuth } from "./baseQueryWithAuth";
import { UserProfile } from "../../domain/entities/UserProfile";
import { PagedResult } from "../../domain/entities/Submission";

export interface UserSummaryDto {
  id: string;
  email: string;
}
export interface CreateUserRequest {
  email: string;
  temporaryPassword: string;
  roles: string[];
}

export interface CreatedUser {
  id: string;
  email: string;
  roles: string[];
}
export const userApi = createApi({
  reducerPath: "userApi",
  baseQuery: baseQueryWithAuth,
  tagTypes: ["UserProfile"], // new — getStudents doesn't need tags, profile endpoints do
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
      invalidatesTags: ["UserProfile"], // so UsersListPage refetches and shows the new person
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
      invalidatesTags: ["UserProfile"],
    }),

    searchUsers: builder.infiniteQuery<
      PagedResult<UserProfile>,
      { search?: string; pageSize: number },
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
} = userApi;
