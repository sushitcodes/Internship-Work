import { useEffect, useRef } from "react";
import { useDispatch, useSelector } from "react-redux";
import type { RootState, AppDispatch } from "../store";
import { setCredentials, logout } from "../store/authSlice";

const REFRESH_BEFORE_EXPIRY_MS = 2 * 60 * 1000; // refresh 2 minutes before expiry

/**
 * Schedules a silent token refresh 2 minutes before the current access token expires.
 * This prevents the 401 → refresh → retry cycle that causes TaskCanceledException
 * on the backend when the original request gets cancelled mid-flight.
 *
 * Place this hook once at the app root (App.tsx).
 */
export function useProactiveTokenRefresh() {
  const dispatch = useDispatch<AppDispatch>();
  const expiresAt = useSelector((state: RootState) => state.auth.expiresAt);
  const email = useSelector((state: RootState) => state.auth.email);
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    // Clear any existing timer whenever expiry changes
    if (timerRef.current !== null) {
      clearTimeout(timerRef.current);
      timerRef.current = null;
    }

    if (!email || !expiresAt) return;

    const expiryMs = new Date(expiresAt).getTime();
    const nowMs = Date.now();
    const delay = expiryMs - nowMs - REFRESH_BEFORE_EXPIRY_MS;

    if (delay <= 0) {
      // Token already expired or about to — refresh immediately
      void refreshNow(dispatch);
      return;
    }

    timerRef.current = setTimeout(() => {
      void refreshNow(dispatch);
    }, delay);

    return () => {
      if (timerRef.current !== null) {
        clearTimeout(timerRef.current);
      }
    };
  }, [expiresAt, email, dispatch]);
}

async function refreshNow(dispatch: AppDispatch) {
  try {
    const res = await fetch(
      `${import.meta.env.VITE_API_URL ?? ""}/auth/refresh`,
      { method: "POST", credentials: "include" }
    );

    if (res.ok) {
      const data = await res.json() as { email: string; roles: string[]; expiresAt: string };
      dispatch(setCredentials({ email: data.email, roles: data.roles, expiresAt: data.expiresAt }));
    } else {
      // Refresh token also expired — log the user out cleanly
      dispatch(logout());
    }
  } catch {
    // Network error — don't log out, the reactive 401 handler will deal with it
  }
}
