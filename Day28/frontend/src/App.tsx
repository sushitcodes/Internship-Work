import { Routes, Route } from "react-router-dom";
import FormPage from "./presentation/pages/FormPage";
import SubmissionPage from "./presentation/pages/SubmissionPage";
import SubmissionsListPage from "./presentation/pages/SubmissionsListPage";

function App() {
  return (
    <div className="min-h-screen bg-gray-100 py-8">
      <Routes>
        <Route path="/" element={<SubmissionsListPage />} />

        <Route path="/FormPage" element={<FormPage />} />
        <Route path="/submissions" element={<SubmissionsListPage />} />
        <Route path="/submission/:id" element={<SubmissionPage />} />
      </Routes>
    </div>
  );
}

export default App;
