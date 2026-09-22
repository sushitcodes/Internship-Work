import React from "react";

export type StatusType =
  | "Present"
  | "Absent"
  | "Late"
  | "Excused"
  | "Unmarked"
  | string;

const STATUS_STYLES: Record<string, string> = {
  Present:
    "bg-emerald-500/10 text-emerald-700 dark:text-emerald-400 border-emerald-500/20",
  Absent: "bg-rose-500/10 text-rose-700 dark:text-rose-400 border-rose-500/20",
  Late: "bg-amber-500/10 text-amber-700 dark:text-amber-400 border-amber-500/20",
  Excused:
    "bg-indigo-500/10 text-indigo-700 dark:text-indigo-400 border-indigo-500/20",
  Unmarked: "bg-muted text-muted-foreground border-border",
};

export const StatusBadge: React.FC<{ status: StatusType }> = ({ status }) => {
  const badgeClass =
    STATUS_STYLES[status] || "bg-muted text-muted-foreground border-border";

  return (
    <span
      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${badgeClass} transition-colors`}
    >
      {status}
    </span>
  );
};
