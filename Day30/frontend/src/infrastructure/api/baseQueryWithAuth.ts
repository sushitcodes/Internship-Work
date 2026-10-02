import { fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import type {
  BaseQueryFn,
  FetchArgs,
  FetchBaseQueryError,
} from "@reduxjs/toolkit/query/react";
import { logout } from "../store/authSlice";
import { Mutex } from "async-mutex";

const API_URL = import.meta.env.VITE_API_URL ?? "";

const rawBaseQuery = fetchBaseQuery({
  baseUrl: API_URL,
  credentials: "include",
});

// Serialises refreshes inside this tab.
const mutex = new Mutex();

/**
 * Asks the server for fresh cookies. Returns true on success.
 * navigator.locks makes this single file ACROSS TABS (async-mutex only covers one tab),
 * so two tabs never present the same refresh token at the same moment.
 */
export async function refreshSession(): Promise<boolean> {
  const doRefresh = async () => {
    const res = await fetch(`${API_URL}/auth/refresh`, {
      method: "POST",
      credentials: "include",
    });
    return res.ok;
  };
  if (typeof navigator !== "undefined" && "locks" in navigator) {
    return navigator.locks.request("auth-refresh", doRefresh);
  }
  return doRefresh();
}

// A 401 from these means "wrong credentials / bad code", not "your session expired".
const NO_REFRESH_PREFIXES = [
  "/auth/login",
  "/auth/refresh",
  "/auth/logout",
  "/auth/forgot-password",
  "/auth/reset-password",
];
const isAuthCall = (args: string | FetchArgs) => {
  const url = typeof args === "string" ? args : args.url;
  return NO_REFRESH_PREFIXES.some((p) => url.startsWith(p));
};

export const baseQueryWithAuth: BaseQueryFn<
  string | FetchArgs,
  unknown,
  FetchBaseQueryError
> = async (args, api, extraOptions) => {
  await mutex.waitForUnlock();
  let result = await rawBaseQuery(args, api, extraOptions);

  if (result.error?.status === 401 && !isAuthCall(args)) {
    if (!mutex.isLocked()) {
      const release = await mutex.acquire();
      try {
        if (await refreshSession()) {
          result = await rawBaseQuery(args, api, extraOptions); // retry once
        } else {
          api.dispatch(logout()); // refresh token expired, revoked or reused
        }
      } finally {
        release();
      }
    } else {
      // Someone else is refreshing: wait, then retry with the fresh cookies.
      await mutex.waitForUnlock();
      result = await rawBaseQuery(args, api, extraOptions);
    }
  }

  return result;
};
