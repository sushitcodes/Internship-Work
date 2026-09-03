import { Routes, Route } from "react-router-dom";
import FormPage from "./presentation/pages/FormPage";
import SubmissionPage from "./presentation/pages/SubmissionPage";
import SubmissionsListPage from "./presentation/pages/SubmissionsListPage";
import LoginPage from "./presentation/pages/LoginPage";
import RegisterPage from "./presentation/pages/RegisterPage";
import ProtectedRoute from "./presentation/components/ProtectedRoute";
import AppLayout from "./presentation/components/AppLayout"; // NEW — replaces Navbar import
import { useGetMeQuery } from "./infrastructure/api/authApi";
import ForgotPasswordPage from "./presentation/pages/ForgotPasswordPage";
import ResetPasswordPage from "./presentation/pages/ResetPasswordPage";
import ClassRoomsPage from "./presentation/pages/ClassRoomsPage";
import EnrollmentPage from "./presentation/pages/EnrollmentPage";
import MarkAttendancePage from "./presentation/pages/MarkAttendancePage";

function App() {
  useGetMeQuery();
  return (
    // Why no top-level div/Navbar anymore: auth pages (login/register) render
    // OUTSIDE the sidebar shell — there's no "app" to navigate yet at that point.
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route path="/reset-password" element={<ResetPasswordPage />} />

      {/* Everything below this line renders inside the sidebar shell.
          Your existing role-gating nesting is untouched — just wrapped one level deeper. */}
      <Route element={<AppLayout />}>
        <Route path="/" element={<SubmissionsListPage />} />
        <Route path="/submission/:id" element={<SubmissionPage />} />

        <Route element={<ProtectedRoute />}>
          <Route path="/formpage" element={<FormPage />} />
        </Route>

        <Route element={<ProtectedRoute allowedRoles={["Staff", "Admin"]} />}>
          <Route path="/submission/:id/edit" element={<FormPage />} />
        </Route>

        <Route element={<ProtectedRoute allowedRoles={["Admin"]} />}>
          <Route path="/classes" element={<ClassRoomsPage />} />
          <Route path="/classes/:id/enroll" element={<EnrollmentPage />} />
          <Route element={<ProtectedRoute allowedRoles={["Staff", "Admin"]} />}>
            <Route path="/attendance/mark" element={<MarkAttendancePage />} />
          </Route>
        </Route>
      </Route>
    </Routes>
  );
}

export default App;
