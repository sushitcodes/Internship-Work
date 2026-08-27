import { Routes, Route } from "react-router-dom";
import FormPage from "./presentation/pages/FormPage";
import SubmissionPage from "./presentation/pages/SubmissionPage";
import SubmissionsListPage from "./presentation/pages/SubmissionsListPage";
import LoginPage from "./presentation/pages/LoginPage";
import RegisterPage from "./presentation/pages/RegisterPage";
import ProtectedRoute from "./presentation/components/ProtectedRoute";
import Navbar from "./presentation/components/Navbar";
import { useGetMeQuery } from "./infrastructure/api/authApi";
function App() {
  useGetMeQuery();
  return (
    <div className="min-h-screen bg-gray-100">
      <Navbar />
      <div className="py-8">
        <Routes>
          <Route path="/" element={<SubmissionsListPage />} />
          <Route path="/submission/:id" element={<SubmissionPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* Everything nested here requires a valid token */}
          <Route element={<ProtectedRoute />}>
            <Route path="/formpage" element={<FormPage />} />
            <Route path="/submission/:id/edit" element={<FormPage />} />
          </Route>
        </Routes>
      </div>
    </div>
  );
}

export default App;
