// App.tsx
import { Routes, Route } from "react-router-dom";
import FormPage from "./presentation/pages/FormPage";
import SubmissionPage from "./presentation/pages/SubmissionPage";
import SubmissionsListPage from "./presentation/pages/SubmissionsListPage";
import LoginPage from "./presentation/pages/LoginPage";
import RegisterPage from "./presentation/pages/RegisterPage";
import ProtectedRoute from "./presentation/components/ProtectedRoute";
import AppLayout from "./presentation/components/AppLayout";
import { useGetMeQuery } from "./infrastructure/api/authApi";
import ForgotPasswordPage from "./presentation/pages/ForgotPasswordPage";
import ResetPasswordPage from "./presentation/pages/ResetPasswordPage";
import ClassRoomsPage from "./presentation/pages/ClassRoomsPage";
import EnrollmentPage from "./presentation/pages/EnrollmentPage";
import MarkAttendancePage from "./presentation/pages/MarkAttendancePage";
// Import any missing pages for the navigation
// import CoursesPage from "./presentation/pages/CoursesPage";
// import AttendanceReportsPage from "./presentation/pages/AttendanceReportsPage";
// import AttendanceAnalyticsPage from "./presentation/pages/AttendanceAnalyticsPage";
// import UsersPage from "./presentation/pages/UsersPage";
// import SettingsPage from "./presentation/pages/SettingsPage";

function App() {
  useGetMeQuery();

  return (
    <Routes>
      {/* Public routes - no sidebar */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route path="/reset-password" element={<ResetPasswordPage />} />

      {/* Protected routes with sidebar layout */}
      <Route element={<AppLayout />}>
        {/* Dashboard / Home */}
        <Route path="/" element={<SubmissionsListPage />} />
        <Route path="/submission/:id" element={<SubmissionPage />} />

        {/* Protected routes - require authentication */}
        <Route element={<ProtectedRoute />}>
          <Route path="/formpage" element={<FormPage />} />
        </Route>

        {/* Staff and Admin routes */}
        <Route element={<ProtectedRoute allowedRoles={["Staff", "Admin"]} />}>
          <Route path="/submission/:id/edit" element={<FormPage />} />

          {/* Attendance section */}
          <Route path="/attendance/mark" element={<MarkAttendancePage />} />
          {/* <Route
            path="/attendance/reports"
            element={<AttendanceReportsPage />}
          />
          <Route
            path="/attendance/analytics"
            element={<AttendanceAnalyticsPage />}
          /> */}
        </Route>

        {/* Admin only routes */}
        <Route element={<ProtectedRoute allowedRoles={["Admin"]} />}>
          {/* Academics section */}
          <Route path="/classes" element={<ClassRoomsPage />} />
          <Route path="/classes/:id/enroll" element={<EnrollmentPage />} />
          <Route path="/enrollments" element={<EnrollmentPage />} />
          {/* <Route path="/courses" element={<CoursesPage />} /> */}

          {/* Administration section */}
          {/* <Route path="/users" element={<UsersPage />} />
          <Route path="/settings" element={<SettingsPage />} /> */}
        </Route>
      </Route>
    </Routes>
  );
}

export default App;
