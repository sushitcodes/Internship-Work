import { refreshSession } from "./baseQueryWithAuth";

/** fetch() with cookies, and one automatic refresh + retry on 401. */
export async function authFetch(url: string, init: RequestInit = {}) {
  const options: RequestInit = { ...init, credentials: "include" };
  let res = await fetch(url, options);
  if (res.status === 401 && (await refreshSession())) {
    res = await fetch(url, options);
  }
  return res;
}
