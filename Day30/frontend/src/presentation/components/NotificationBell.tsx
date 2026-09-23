import { useState } from "react";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Button } from "@/components/ui/button";
import { Bell, FileText, Award } from "lucide-react";
import { useNavigate } from "react-router-dom";
import {
  useGetNotificationsQuery,
  useGetUnreadCountQuery,
  useMarkNotificationReadMutation,
  useMarkAllNotificationsReadMutation,
} from "../../infrastructure/api/notificationApi";

export function NotificationBell() {
  const [open, setOpen] = useState(false);
  const navigate = useNavigate();

  const { data: unreadCount = 0 } = useGetUnreadCountQuery();
  const { data: notifications = [], isLoading } = useGetNotificationsQuery();
  const [markRead] = useMarkNotificationReadMutation();
  const [markAllRead] = useMarkAllNotificationsReadMutation();

  const handleClick = async (id: string, link?: string | null) => {
    await markRead(id);
    setOpen(false);
    if (link) navigate(link);
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
        <div className="flex items-center justify-between p-3 border-b bg-muted/20">
          <div className="flex items-center gap-2">
            <h3 className="font-semibold text-sm">Notifications</h3>
            {unreadCount > 0 && (
              <span className="text-xs px-2 py-0.5 rounded-full bg-primary/10 text-primary font-medium">
                {unreadCount} new
              </span>
            )}
          </div>
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
        </div>

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
                className={`w-full text-left p-3 flex items-start gap-3 transition-colors ${
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
