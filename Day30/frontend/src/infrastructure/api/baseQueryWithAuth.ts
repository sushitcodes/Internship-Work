import { fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import type {
  BaseQueryFn,
  FetchArgs,
  FetchBaseQueryError,
} from "@reduxjs/toolkit/query/react";
import { logout } from "../store/authSlice";
import { Mutex } from "async-mutex";

const rawBaseQuery = fetchBaseQuery({
  baseUrl: import.meta.env.VITE_API_URL,
  credentials: "include",
});

// Same signature as fetchBaseQuery, so any API slice can drop this in
// as a direct replacement.
const mutex = new Mutex();

export const baseQueryWithAuth: BaseQueryFn<
  string | FetchArgs,
  unknown,
  FetchBaseQueryError
> = async (args, api, extraOptions) => {
  await mutex.waitForUnlock();
  let result = await rawBaseQuery(args, api, extraOptions);

  if (result.error?.status === 401) {
    // Only the FIRST request to hit this point actually performs the
    // refresh; others will simply wait via mutex.waitForUnlock() above,
    // on their own retry, once the lock is released.
    if (!mutex.isLocked()) {
      const release = await mutex.acquire();
      try {
        const refreshResult = await rawBaseQuery(
          { url: "/auth/refresh", method: "POST" },
          api,
          extraOptions,
        );

        if (refreshResult.data) {
          // Refresh succeeded — the server has already set new cookies.
          // Retry the ORIGINAL request; it will succeed now.
          result = await rawBaseQuery(args, api, extraOptions);
        } else {
          // Refresh itself failed — refresh token expired, revoked, or
          // reuse was detected. There's no way to recover silently.
          api.dispatch(logout());
        }
      } finally {
        release();
      }
    } else {
      // Someone else is already refreshing — wait for them to finish,
      // then just retry this request with the (now fresh) cookies.
      await mutex.waitForUnlock();
      result = await rawBaseQuery(args, api, extraOptions);
    }
  }

  return result;
};
