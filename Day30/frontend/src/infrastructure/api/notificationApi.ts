import { api } from "./api";
import type { Notification } from "../../domain/entities/Notification";

export const notificationApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getNotifications: builder.query<
      Notification[],
      { unreadOnly?: boolean } | void
    >({
      query: (args) => {
        const params = new URLSearchParams();
        if (args?.unreadOnly) params.set("unreadOnly", "true");
        const qs = params.toString();
        return `/notifications${qs ? `?${qs}` : ""}`;
      },
      providesTags: ["Notification"],
    }),

    getUnreadCount: builder.query<number, void>({
      query: () => "/notifications/unread-count",
      providesTags: ["Notification"],
    }),

    markNotificationRead: builder.mutation<void, string>({
      query: (id) => ({ url: `/notifications/${id}/read`, method: "PATCH" }),
      invalidatesTags: ["Notification"],
    }),

    markAllNotificationsRead: builder.mutation<void, void>({
      query: () => ({ url: "/notifications/read-all", method: "POST" }),
      invalidatesTags: ["Notification"],
    }),

    broadcastNotification: builder.mutation<
      void,
      {
        title: string;
        body: string;
        link?: string;
        scope: "AllTeachers" | "AllStudents" | "Everyone";
      }
    >({
      query: (body) => ({
        url: "/notifications/broadcast",
        method: "POST",
        body: {
          Title: body.title,
          Body: body.body,
          Link: body.link ?? null,
          Scope: body.scope,
        },
      }),
    }),
  }),
});

export const {
  useGetNotificationsQuery,
  useGetUnreadCountQuery,
  useMarkNotificationReadMutation,
  useMarkAllNotificationsReadMutation,
  useBroadcastNotificationMutation,
} = notificationApi;
