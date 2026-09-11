import { Routes, Route } from "react-router-dom";
import FormPage from "./presentation/pages/FormPage";
import SubmissionPage from "./presentation/pages/SubmissionPage";
import SubmissionsListPage from "./presentation/pages/SubmissionsListPage";
import LoginPage from "./presentation/pages/LoginPage";
import ProtectedRoute from "./presentation/components/ProtectedRoute";
import AppLayout from "./presentation/components/AppLayout";
import { useGetMeQuery } from "./infrastructure/api/authApi";
import ForgotPasswordPage from "./presentation/pages/ForgotPasswordPage";
import ResetPasswordPage from "./presentation/pages/ResetPasswordPage";
import ClassRoomsPage from "./presentation/pages/ClassRoomsPage";
import EnrollmentPage from "./presentation/pages/EnrollmentPage";
import MarkAttendancePage from "./presentation/pages/MarkAttendancePage";
import UsersListPage from "./presentation/pages/UserListPage";
import MyProfilePage from "./presentation/pages/MyProfilePage";
import { Toaster } from "@/components/ui/sonner"; // shadcn's sonner wrapper, not the raw "sonner" package
import CreateUserPage from "./presentation/pages/CreateUserPage";
import DashboardPage from "./presentation/pages/DashboardPage";
import UserDetailPage from "./presentation/pages/UserDetailPage";
import AttendanceSheetPage from "./presentation/pages/AttendanceSheetPage";

// Import any missing pages for the navigation
// import CoursesPage from "./presentation/pages/CoursesPage";
// import AttendanceReportsPage from "./presentation/pages/AttendanceReportsPage";
// import AttendanceAnalyticsPage from "./presentation/pages/AttendanceAnalyticsPage";
// import UsersPage from "./presentation/pages/UsersPage";
// import SettingsPage from "./presentation/pages/SettingsPage";

function App() {
  useGetMeQuery();

  return (
    <>
      <Toaster richColors position="top-right" />
      <Routes>
        {/* Public routes - no sidebar */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />

        {/* Protected routes with sidebar layout */}
        <Route element={<AppLayout />}>
          {/* Dashboard / Home */}
          <Route element={<ProtectedRoute />}>
            <Route path="/" element={<DashboardPage />} />
            <Route path="/submissions" element={<SubmissionsListPage />} />
            <Route path="/submission/:id" element={<SubmissionPage />} />

            {/* Protected routes - require authentication */}
            <Route path="/formpage" element={<FormPage />} />
            <Route path="/profile" element={<MyProfilePage />} />
          </Route>

          {/* Staff and Admin routes */}
          <Route element={<ProtectedRoute allowedRoles={["Staff", "Admin"]} />}>
            <Route path="/submission/:id/edit" element={<FormPage />} />
            <Route path="/attendance/mark" element={<MarkAttendancePage />} />
            <Route path="/attendance/sheet" element={<AttendanceSheetPage />} />
            <Route path="/users" element={<UsersListPage />} />
            <Route path="/users/:id" element={<UserDetailPage />} />
          </Route>

          {/* Attendance section */}
          <Route element={<ProtectedRoute allowedRoles={["Admin"]} />}>
            <Route path="/users/create" element={<CreateUserPage />} />
            <Route path="/classes" element={<ClassRoomsPage />} />
            <Route path="/classes/:id/enroll" element={<EnrollmentPage />} />
            <Route path="/enrollments" element={<EnrollmentPage />} />
          </Route>
        </Route>
      </Routes>
    </>
  );
}

export default App;
