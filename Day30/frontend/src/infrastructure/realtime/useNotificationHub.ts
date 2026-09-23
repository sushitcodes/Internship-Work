import { useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import { toast } from "sonner";
import { api } from "../api/api";
import { useAppDispatch, useAppSelector } from "../store/hooks";

// Matches the pattern used by attendanceApi.ts and gradeApi.ts —
// strip "/api" from the base URL so we hit the hub at the server root.
const API_ORIGIN = (import.meta.env.VITE_API_URL ?? "").replace(
  /\/api\/?$/,
  "",
);
const HUB_URL = `${API_ORIGIN}/hubs/notifications`;

export function useNotificationHub() {
  const dispatch = useAppDispatch();
  const email = useAppSelector((s) => s.auth.email);

  useEffect(() => {
    if (!email) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, { withCredentials: true })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on("ReceiveNotification", (n) => {
      // Server is the source of truth. SignalR just says "something changed".
      dispatch(api.util.invalidateTags(["Notification"]));
      toast.info(n.title, { description: n.body });
    });

    // On reconnect, re-sync — messages sent while offline never arrived.
    connection.onreconnected(() => {
      dispatch(api.util.invalidateTags(["Notification"]));
    });

    connection
      .start()
      .catch((err) => console.warn("SignalR connection failed:", err));

    return () => {
      connection.stop();
    };
  }, [email, dispatch]);
}
