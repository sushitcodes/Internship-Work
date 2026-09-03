// Shared by FormPage/SubmissionPage (submission files) and MyProfilePage
// (avatars) — anywhere the backend returns a relative file path that needs
// to become an absolute URL the browser can actually fetch.
export function resolveFileUrl(path?: string | null): string | undefined {
  if (!path) return undefined;
  if (path.startsWith("http")) return path; // already absolute, leave as-is

  const apiOrigin = (import.meta.env.VITE_API_URL ?? "").replace(
    /\/api\/?$/,
    "",
  );
  return path.startsWith("/") ? `${apiOrigin}${path}` : `${apiOrigin}/${path}`;
}
