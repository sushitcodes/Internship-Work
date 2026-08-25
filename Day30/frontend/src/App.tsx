import { Routes, Route } from "react-router-dom";
import FormPage from "./presentation/pages/FormPage";
import SubmissionPage from "./presentation/pages/SubmissionPage";
import SubmissionsListPage from "./presentation/pages/SubmissionsListPage";
// import EditSubmissionPage from "./presentation/pages/EditSubmissionPage";

function App() {
  return (
    <div className="min-h-screen bg-gray-100 py-8">
      <Routes>
        <Route path="/" element={<SubmissionsListPage />} />
        <Route path="/formpage" element={<FormPage />} />
        <Route path="/submission/:id" element={<SubmissionPage />} />
        {/* <Route path="/submission/:id/edit" element={<EditSubmissionPage />} /> */}
        <Route path="/submission/:id/edit" element={<FormPage />} />
      </Routes>
    </div>
  );
}

export default App;
