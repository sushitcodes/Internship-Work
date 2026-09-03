const ROLE_DISPLAY_NAMES: Record<string, string> = {
  Staff: "Teacher",
  Admin: "Admin",
  Student: "Student",
};

export function displayRoleName(role: string): string {
  return ROLE_DISPLAY_NAMES[role] ?? role;
}
