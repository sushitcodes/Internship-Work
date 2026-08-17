import { BrowserRouter as Router, Routes, Route, Link } from "react-router-dom";
import FormPage from "./pages/FormPage";
import SubmissionPage from "./pages/SubmissionPage";

function App() {
  return (
    <Router>
      <div className="min-h-screen bg-gray-100">
        <nav className="flex gap-5 bg-white border-b border-gray-300 px-5 py-4 shadow-sm">
          <Link
            to="/"
            className="font-medium text-blue-600 hover:text-blue-800"
          >
            Form
          </Link>
          <Link
            to="/submissions"
            className="font-medium text-blue-600 hover:text-blue-800"
          >
            Submissions
          </Link>
        </nav>

        <div className="container mx-auto px-4">
          <Routes>
            <Route path="/" element={<FormPage />} />
            <Route path="/submissions" element={<SubmissionPage />} />
          </Routes>
        </div>
      </div>
    </Router>
  );
}

export default App;
