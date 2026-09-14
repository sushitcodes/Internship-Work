export const Paths = {
  // Public

  login: "/login",
  forgotPassword: "/forgot-password",
  resetPassword: "/reset-password",

  // Protected — any logged-in user

  dashboard: "/",

  submissions: "/submissions",
  submissionDetail: "/submission/:id",
  submissionEdit: "/submission/:id/edit",
  submissionCreate: "/formpage",

  profile: "/profile",

  // Staff or Admin

  markAttendance: "/attendance/mark",
  attendanceSheet: "/attendance/sheet",

  users: "/users",
  userDetail: "/users/:id",

  // Admin only

  userCreate: "/users/create",

  classes: "/classes",
  classEnroll: "/classes/:id/enroll",

  enrollments: "/enrollments",
} as const;

export type PathKey = keyof typeof Paths;
