import { api } from "./api";
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

// Every non-auth tag declared in api.ts — same list, one place, reused
// by both handlers below. If you add a new feature file with a new tag,
// add it here too.
const DATA_TAGS = [
  "Submission",
  "Dashboard",
  "Attendance",
  "ClassRoom",
  "Enrollment",
  "Subject",
  "Grade",
  "UserProfile",
  "Notification",
] as const;

export const authApi = api.injectEndpoints({
  endpoints: (builder) => ({
    login: builder.mutation<AuthResponse, AuthRequest>({
      query: (body) => ({ url: "/auth/login", method: "POST", body }),
      onQueryStarted: async (_arg, { dispatch, queryFulfilled }) => {
        const { data } = await queryFulfilled;
        dispatch(api.util.resetApiState());

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

    logoutUser: builder.mutation<void, void>({
      query: () => ({ url: "/auth/logout", method: "POST" }),
      onQueryStarted: async (_arg, { dispatch, queryFulfilled }) => {
        await queryFulfilled;
        dispatch(logout());

        // Explicitly wipe the own-profile cache entry so the next user
        // who logs in never briefly sees the previous user's avatar.
        // invalidateTags alone marks it stale but still serves cached
        // data until the component refetches — resetApiState is the
        // sledgehammer that clears every single cache entry at once.
        dispatch(api.util.resetApiState());
      },
    }),

    getMe: builder.query<{ email: string; roles: string[] }, void>({
      query: () => "/auth/me",
      onQueryStarted: async (_arg, { dispatch, queryFulfilled }) => {
        try {
          const { data } = await queryFulfilled;
          dispatch(setCredentials({ email: data.email, roles: data.roles }));
        } catch {
          dispatch(logout());
          dispatch(api.util.invalidateTags([...DATA_TAGS]));
        }
      },
    }),
  }),
});

export const {
  useLoginMutation,
  useLogoutUserMutation,
  useGetMeQuery,
  useForgotPasswordMutation,
  useResetPasswordMutation,
} = authApi;
