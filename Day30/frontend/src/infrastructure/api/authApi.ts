import { createApi } from "@reduxjs/toolkit/query/react";
import { baseQueryWithAuth } from "./baseQueryWithAuth";
import { setCredentials, logout } from "../store/authSlice";

export interface AuthResponse {
  email: string;
  expiresAt: string;
  role: string;
}

export interface AuthRequest {
  email: string;
  password: string;
}

export const authApi = createApi({
  reducerPath: "authApi",
  baseQuery: baseQueryWithAuth, // CHANGE — was a plain fetchBaseQuery; now shares
  // the same credentials:"include" logic
  endpoints: (builder) => ({
    register: builder.mutation<AuthResponse, AuthRequest>({
      query: (body) => ({ url: "/auth/register", method: "POST", body }),
      onQueryStarted: async (_arg, { dispatch, queryFulfilled }) => {
        const { data } = await queryFulfilled;
        dispatch(setCredentials({ email: data.email, role: data.role }));
      },
    }),
    login: builder.mutation<AuthResponse, AuthRequest>({
      query: (body) => ({ url: "/auth/login", method: "POST", body }),
      onQueryStarted: async (_arg, { dispatch, queryFulfilled }) => {
        const { data } = await queryFulfilled;
        dispatch(setCredentials({ email: data.email, role: data.role }));
      },
    }),
    // ADD — a real server round-trip; logout is no longer purely local
    logoutUser: builder.mutation<void, void>({
      query: () => ({ url: "/auth/logout", method: "POST" }),
      onQueryStarted: async (_arg, { dispatch, queryFulfilled }) => {
        await queryFulfilled;
        dispatch(logout());
      },
    }),
    // ADD — called once on app load to check "is the cookie still valid"
    getMe: builder.query<{ email: string; role: string }, void>({
      query: () => "/auth/me",
      onQueryStarted: async (_arg, { dispatch, queryFulfilled }) => {
        try {
          const { data } = await queryFulfilled;
          dispatch(setCredentials({ email: data.email, role: data.role }));
        } catch {
          dispatch(logout()); // no valid cookie — stay logged out, no error shown
        }
      },
    }),
  }),
});

export const {
  useRegisterMutation,
  useLoginMutation,
  useLogoutUserMutation,
  useGetMeQuery,
} = authApi;
