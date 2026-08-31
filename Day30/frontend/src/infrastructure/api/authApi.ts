import { createApi } from "@reduxjs/toolkit/query/react";
import { baseQueryWithAuth } from "./baseQueryWithAuth";
import { setCredentials, logout } from "../store/authSlice";

export interface AuthResponse {
  email: string;
  expiresAt: string;
  roles: string[];
}

export interface AuthRequest {
  email: string;
  password: string;
}
export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  code: string;
  newPassword: string;
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
        dispatch(setCredentials({ email: data.email, roles: data.roles }));
      },
    }),
    login: builder.mutation<AuthResponse, AuthRequest>({
      query: (body) => ({ url: "/auth/login", method: "POST", body }),
      onQueryStarted: async (_arg, { dispatch, queryFulfilled }) => {
        const { data } = await queryFulfilled;
        dispatch(setCredentials({ email: data.email, roles: data.roles }));
      },
    }),
    forgotPassword: builder.mutation<
      { message: string },
      ForgotPasswordRequest
    >({
      query: (body) => ({ url: "/auth/forgot-password", method: "POST", body }),
    }),
    resetPassword: builder.mutation<{ message: string }, ResetPasswordRequest>({
      query: (body) => ({ url: "/auth/reset-password", method: "POST", body }),
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
    getMe: builder.query<{ email: string; roles: string[] }, void>({
      query: () => "/auth/me",
      onQueryStarted: async (_arg, { dispatch, queryFulfilled }) => {
        try {
          const { data } = await queryFulfilled;
          dispatch(setCredentials({ email: data.email, roles: data.roles }));
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
  useForgotPasswordMutation,
  useResetPasswordMutation,
} = authApi;
