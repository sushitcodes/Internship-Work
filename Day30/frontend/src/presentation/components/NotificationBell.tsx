import { useState } from "react";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Button } from "@/components/ui/button";
import { Bell, FileText, Award, Trash2, X } from "lucide-react";
import { useNavigate } from "react-router-dom";
import {
  useGetNotificationsQuery,
  useGetUnreadCountQuery,
  useMarkNotificationReadMutation,
  useMarkAllNotificationsReadMutation,
  useDeleteNotificationMutation,
  useClearAllNotificationsMutation,
} from "../../infrastructure/api/notificationApi";

export function NotificationBell() {
  const [open, setOpen] = useState(false);
  const navigate = useNavigate();

  const { data: unreadCount = 0 } = useGetUnreadCountQuery();
  const { data: notifications = [], isLoading } = useGetNotificationsQuery();
  const [markRead] = useMarkNotificationReadMutation();
  const [markAllRead] = useMarkAllNotificationsReadMutation();
  const [deleteOne] = useDeleteNotificationMutation();
  const [clearAll] = useClearAllNotificationsMutation();

  const handleClick = async (id: string, link?: string | null) => {
    await markRead(id);
    setOpen(false);
    if (link) navigate(link);
  };

  const handleDismiss = async (e: React.MouseEvent, id: string) => {
    e.stopPropagation(); // don't let the row's onClick (navigate) fire
    await deleteOne(id);
  };

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger
        render={
          <button
            aria-label="Open notifications"
            className="relative rounded-full p-2 text-foreground hover:bg-accent transition-colors"
          >
            <Bell className="h-5 w-5" />
            {unreadCount > 0 && (
              <span className="absolute top-1 right-1 flex h-4 w-4 items-center justify-center rounded-full bg-rose-500 text-[10px] font-bold text-white">
                {unreadCount > 9 ? "9+" : unreadCount}
              </span>
            )}
          </button>
        }
      />
      <PopoverContent align="end" className="w-80 sm:w-96 p-0 shadow-lg">
        {/* Header */}
        <div className="flex items-center justify-between p-3 border-b bg-muted/20">
          <div className="flex items-center gap-2">
            <h3 className="font-semibold text-sm">Notifications</h3>
            {unreadCount > 0 && (
              <span className="text-xs px-2 py-0.5 rounded-full bg-primary/10 text-primary font-medium">
                {unreadCount} new
              </span>
            )}
          </div>

          <div className="flex items-center gap-1">
            {unreadCount > 0 && (
              <Button
                variant="ghost"
                size="sm"
                onClick={() => markAllRead()}
                className="h-7 text-xs px-2"
              >
                Mark all read
              </Button>
            )}
            {notifications.length > 0 && (
              <Button
                variant="ghost"
                size="sm"
                onClick={() => clearAll()}
                className="h-7 w-7 p-0 text-muted-foreground hover:text-destructive"
                title="Clear all notifications"
              >
                <Trash2 className="h-3.5 w-3.5" />
              </Button>
            )}
          </div>
        </div>

        {/* List */}
        <div className="max-h-80 overflow-y-auto divide-y divide-border/60">
          {isLoading ? (
            <div className="py-8 text-center text-xs text-muted-foreground">
              Loading…
            </div>
          ) : notifications.length > 0 ? (
            notifications.map((n) => (
              <button
                key={n.id}
                onClick={() => handleClick(n.id, n.link)}
                className={`relative w-full text-left p-3 pr-10 flex items-start gap-3 transition-colors ${
                  !n.isRead ? "bg-primary/5" : "hover:bg-muted/40"
                }`}
              >
                <div
                  className={`p-2 rounded-lg shrink-0 mt-0.5 ${
                    n.kind === "submission"
                      ? "bg-blue-500/10 text-blue-600 dark:text-blue-400"
                      : n.kind === "grade"
                        ? "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400"
                        : "bg-muted text-muted-foreground"
                  }`}
                >
                  {n.kind === "submission" ? (
                    <FileText className="h-4 w-4" />
                  ) : n.kind === "grade" ? (
                    <Award className="h-4 w-4" />
                  ) : (
                    <Bell className="h-4 w-4" />
                  )}
                </div>

                <div className="flex-1 min-w-0">
                  <p className="text-xs font-semibold text-foreground truncate">
                    {n.title}
                  </p>
                  <p className="text-xs text-muted-foreground mt-0.5 leading-snug">
                    {n.body}
                  </p>
                  <span className="text-[10px] text-muted-foreground/80 mt-1 block">
                    {new Date(n.createdAt).toLocaleString([], {
                      hour: "2-digit",
                      minute: "2-digit",
                      day: "2-digit",
                      month: "short",
                    })}
                  </span>
                </div>

                {!n.isRead && (
                  <span className="w-2 h-2 rounded-full bg-primary shrink-0 mt-1.5" />
                )}

                {/* Per-row dismiss — always visible */}
                <span
                  role="button"
                  tabIndex={0}
                  aria-label="Dismiss notification"
                  onClick={(e) => handleDismiss(e, n.id)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" || e.key === " ") {
                      e.preventDefault();
                      handleDismiss(e as unknown as React.MouseEvent, n.id);
                    }
                  }}
                  className="absolute top-1/2 -translate-y-1/2 right-2 p-1.5 rounded-md text-muted-foreground/60 hover:bg-muted hover:text-destructive transition-colors cursor-pointer"
                >
                  <X className="h-3.5 w-3.5" />
                </span>
              </button>
            ))
          ) : (
            <div className="py-8 text-center text-xs text-muted-foreground">
              No notifications yet.
            </div>
          )}
        </div>
      </PopoverContent>
    </Popover>
  );
}
