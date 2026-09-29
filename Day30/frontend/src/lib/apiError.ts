/** Readable message from an RTK Query / ASP.NET error body. */
export function extractErrorMessage(
  err: unknown,
  fallback = "Could not save. Please try again.",
): string {
  if (!err || typeof err !== "object") return fallback;
  const e = err as Record<string, unknown>;

  if ("data" in e && e.data && typeof e.data === "object") {
    const d = e.data as Record<string, unknown>;

    if (d.errors && typeof d.errors === "object") {
      const errorEntries = Object.entries(d.errors as Record<string, string[]>);
      if (errorEntries.length > 0) {
        return errorEntries
          .map(([field, msgs]) => `${field}: ${msgs.join(", ")}`)
          .join(" | ");
      }
    }

    if (typeof d.message === "string") return d.message;
    if (typeof d.title === "string") return d.title;
  }
  if (typeof e.data === "string") return e.data;
  if (typeof e.message === "string") return e.message;
  return fallback;
}
