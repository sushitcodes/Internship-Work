// src/pages/SubmissionPage.tsx
import React, { useEffect, useState } from "react";
import { useSelector, useDispatch } from "react-redux";
import type { RootState } from "../store";
import {
  clearSubmissions,
  deleteSubmission,
  setSubmissions,
} from "../store/actions/formActions";
import { useNavigate } from "react-router-dom";
import {
  getSubmissions,
  deleteSubmission as deleteFromBackend,
} from "../services/api";

const SubmissionPage: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deleting, setDeleting] = useState<string | null>(null); // Track which is being deleted

  const submissions = useSelector((state: RootState) => state.form.submissions);
  const dispatch = useDispatch();
  const navigate = useNavigate();

  //
  //  FETCH DATA FROM BACKEND WHEN PAGE LOADS
  //
  useEffect(() => {
    const fetchSubmissions = async () => {
      try {
        setLoading(true);
        setError(null);

        console.log("📥 Fetching submissions from database...");
        const data = await getSubmissions();
        console.log("✅ Data received from database:", data);

        // ✅ Store in Redux
        dispatch(setSubmissions(data));
      } catch (err: unknown) {
        console.error("❌ Error fetching submissions:", err);
        setError("Failed to load submissions. Please refresh the page.");
      } finally {
        setLoading(false);
      }
    };

    fetchSubmissions();
  }, [dispatch]);

  //  DELETE SINGLE SUBMISSION (Database + Redux)

  const handleDeleteSubmission = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this submission?")) {
      return;
    }

    try {
      setDeleting(id); // Show loading state on delete button
      console.log(` Deleting submission ${id} from database...`);

      // ✅ 1. DELETE FROM DATABASE FIRST
      await deleteFromBackend(id);
      console.log("✅ Deleted from database");

      // ✅ 2. THEN DELETE FROM REDUX
      dispatch(deleteSubmission(id));
      console.log("✅ Deleted from Redux");

      // ✅ 3. Show success message (optional)
      // You could add a toast notification here
    } catch (err: unknown) {
      console.error("❌ Error deleting submission:", err);
      alert("Failed to delete submission. Please try again.");
    } finally {
      setDeleting(null);
    }
  };

  // 🗑️ CLEAR ALL SUBMISSIONS

  const handleClearAll = async () => {
    if (
      !window.confirm(
        "Are you sure you want to delete ALL submissions? This cannot be undone!",
      )
    ) {
      return;
    }

    try {
      setLoading(true);
      console.log("🗑️ Deleting all submissions from database...");

      // ✅ 1. DELETE ALL FROM DATABASE
      // Option A: Delete one by one
      for (const submission of submissions) {
        await deleteFromBackend(submission.id);
      }

      // Option B: If you have a "delete all" endpoint, use that instead
      // await deleteAllSubmissions();

      console.log("✅ All submissions deleted from database");

      // ✅ 2. THEN CLEAR REDUX
      dispatch(clearSubmissions());
      console.log("✅ Cleared Redux");
    } catch (err: unknown) {
      console.error("❌ Error clearing submissions:", err);
      alert("Failed to clear submissions. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const handleGoBack = () => {
    navigate("/");
  };

  // 🎨 RENDER

  if (loading) {
    return (
      <div className="max-w-4xl mx-auto mt-10 p-6 text-center">
        <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        <p className="mt-4 text-gray-600">Loading submissions...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-4xl mx-auto mt-10 p-6">
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
          <p>{error}</p>
          <button
            onClick={() => window.location.reload()}
            className="mt-2 px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700"
          >
            Retry
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto mt-10 p-6">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-2xl font-bold">Submissions</h2>
        <div className="space-x-3">
          <button
            onClick={handleGoBack}
            className="px-4 py-2 bg-gray-600 hover:bg-gray-700 text-white rounded-md transition-colors"
          >
            ← Back to Form
          </button>
          {submissions.length > 0 && (
            <button
              onClick={handleClearAll}
              disabled={loading}
              className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-md transition-colors disabled:opacity-50"
            >
              {loading ? "Deleting..." : "Clear All"}
            </button>
          )}
        </div>
      </div>

      <p className="mb-4 text-gray-600">
        Total Submissions:{" "}
        <strong className="text-gray-800">{submissions.length}</strong>
      </p>

      {submissions.length === 0 ? (
        <div className="text-center py-12 bg-gray-50 rounded-lg">
          <p className="text-gray-600">No submissions yet.</p>
          <button
            onClick={() => navigate("/")}
            className="mt-4 px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
          >
            Go to Form
          </button>
        </div>
      ) : (
        <div>
          {submissions.map((submission) => (
            <div
              key={submission.id}
              className="border border-gray-200 p-4 mb-4 rounded-lg bg-white shadow-sm hover:shadow-md transition-shadow"
            >
              <div className="flex justify-between items-start">
                <div className="flex-1">
                  <p className="mb-1">
                    <strong className="text-gray-700">Name:</strong>
                    <span className="ml-2">{submission.name}</span>
                  </p>
                  <p className="mb-1">
                    <strong className="text-gray-700">Email:</strong>
                    <span className="ml-2">{submission.email}</span>
                  </p>
                  <p className="mb-3">
                    <strong className="text-gray-700">Message:</strong>
                    <span className="ml-2">{submission.message}</span>
                  </p>

                  {submission.education && submission.education.length > 0 && (
                    <div className="mt-3 pt-3 border-t border-gray-200">
                      <p className="font-semibold text-gray-700 mb-2">
                        Education:
                      </p>
                      <div className="space-y-2">
                        {submission.education.map((edu) => (
                          <div
                            key={edu.id}
                            className="bg-gray-50 p-3 rounded-md text-sm"
                          >
                            <span className="font-medium">{edu.degree}</span>
                            <span className="mx-2 text-gray-400">|</span>
                            <span>{edu.year}</span>
                            <span className="mx-2 text-gray-400">|</span>
                            <span>{edu.school}</span>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}

                  <p className="text-sm text-gray-500 mt-3">
                    <strong>Submitted:</strong>{" "}
                    {new Date(submission.submittedAt).toLocaleString()}
                  </p>
                </div>

                {/* ✅ DELETE BUTTON WITH LOADING STATE */}
                <button
                  onClick={() => handleDeleteSubmission(submission.id)}
                  disabled={deleting === submission.id}
                  className={`px-3 py-1 text-white rounded-md text-sm transition-colors ${
                    deleting === submission.id
                      ? "bg-gray-400 cursor-not-allowed"
                      : "bg-red-500 hover:bg-red-600"
                  }`}
                >
                  {deleting === submission.id ? "Deleting..." : "Delete"}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default SubmissionPage;
