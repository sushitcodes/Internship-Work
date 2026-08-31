export const HIDE_ACTIONS_WHEN_LOGGED_OUT = true;

export function canEdit(roles: string[]): boolean {
  return roles.includes("Staff") || roles.includes("Admin");
}

export function canDelete(roles: string[]): boolean {
  return roles.includes("Admin");
}
