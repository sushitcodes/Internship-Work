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
import { Toaster } from "@/components/ui/sonner";
import CreateUserPage from "./presentation/pages/CreateUserPage";
import DashboardPage from "./presentation/pages/DashboardPage";
import UserDetailPage from "./presentation/pages/UserDetailPage";
import AttendanceSheetPage from "./presentation/pages/AttendanceSheetPage";
import { Paths } from "../src/routes/paths";
import ClassSubjectPage from "./presentation/pages/ClassSubjectPage";
import EnterGradesPage from "./presentation/pages/EnterGradesPage";
import ReportCardPage from "./presentation/pages/ReportCardPage";
import AdminBroadcastPage from "./presentation/pages/AdminBroadcastPage";

function App() {
  useGetMeQuery();

  return (
    <>
      <Toaster richColors position="top-right" />
      <Routes>
        {/* Public routes - no sidebar */}
        <Route path={Paths.login} element={<LoginPage />} />
        <Route path={Paths.forgotPassword} element={<ForgotPasswordPage />} />
        <Route path={Paths.resetPassword} element={<ResetPasswordPage />} />

        {/* Protected routes with sidebar layout */}
        <Route element={<AppLayout />}>
          {/* Dashboard / Home */}
          <Route element={<ProtectedRoute />}>
            <Route path={Paths.dashboard} element={<DashboardPage />} />
            <Route path={Paths.submissions} element={<SubmissionsListPage />} />
            <Route path={Paths.submissionDetail} element={<SubmissionPage />} />

            {/* Protected routes - require authentication */}
            <Route path={Paths.submissionCreate} element={<FormPage />} />
            <Route path={Paths.profile} element={<MyProfilePage />} />
            <Route path={Paths.reportCard} element={<ReportCardPage />} />
          </Route>

          {/* Staff and Admin routes */}
          <Route element={<ProtectedRoute allowedRoles={["Staff", "Admin"]} />}>
            <Route path={Paths.submissionEdit} element={<FormPage />} />
            <Route
              path={Paths.markAttendance}
              element={<MarkAttendancePage />}
            />
            <Route
              path={Paths.attendanceSheet}
              element={<AttendanceSheetPage />}
            />
            <Route path={Paths.users} element={<UsersListPage />} />
            <Route path={Paths.userDetail} element={<UserDetailPage />} />

            <Route path={Paths.classSubjects} element={<ClassSubjectPage />} />
            <Route path={Paths.gradesEnter} element={<EnterGradesPage />} />
          </Route>

          {/* Admin only */}
          <Route element={<ProtectedRoute allowedRoles={["Admin"]} />}>
            <Route path={Paths.userCreate} element={<CreateUserPage />} />
            <Route path={Paths.classes} element={<ClassRoomsPage />} />
            <Route path={Paths.classEnroll} element={<EnrollmentPage />} />
            <Route path={Paths.broadcast} element={<AdminBroadcastPage />} />
          </Route>
        </Route>
      </Routes>
    </>
  );
}

export default App;
