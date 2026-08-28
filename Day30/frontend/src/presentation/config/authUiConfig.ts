export const HIDE_ACTIONS_WHEN_LOGGED_OUT = true;

export function canEdit(role: string | null): boolean {
  return role === "Staff" || role === "Admin";
}

export function canDelete(role: string | null): boolean {
  return role === "Admin";
}
