import { createApi } from "@reduxjs/toolkit/query/react";
import { baseQueryWithAuth } from "./baseQueryWithAuth";

export interface UserSummaryDto {
  id: string;
  email: string;
}

export const userApi = createApi({
  reducerPath: "userApi",
  baseQuery: baseQueryWithAuth,
  endpoints: (builder) => ({
    getStudents: builder.query<UserSummaryDto[], void>({
      query: () => "/users/students",
    }),
  }),
});

export const { useGetStudentsQuery } = userApi;
