import React from "react";
import { useSelector, useDispatch } from "react-redux";
import type { RootState } from "../store";
import {
  clearSubmissions,
  deleteSubmission,
} from "../store/actions/formActions"; // ← Import both
import { useNavigate } from "react-router-dom";

const SubmissionPage: React.FC = () => {
  const submissions = useSelector((state: RootState) => state.form.submissions);
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const handleClearAll = () => {
    if (window.confirm("Are you sure you want to clear all submissions?")) {
      dispatch(clearSubmissions());
    }
  };

  const handleDeleteSubmission = (id: string) => {
    if (window.confirm("Are you sure you want to delete this submission?")) {
      dispatch(deleteSubmission(id));
    }
  };

  const handleGoBack = () => {
    navigate("/");
  };

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
              className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-md transition-colors"
            >
              Clear All
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
                  <p className="mb-1">
                    <strong className="text-gray-700">Message:</strong>
                    <span className="ml-2">{submission.message}</span>
                  </p>

                  {/* NEW: Education Section */}
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

                  <p className="text-sm text-gray-500 mt-2">
                    <strong>Submitted:</strong>{" "}
                    {new Date(submission.submittedAt).toLocaleString()}
                  </p>
                  <p className="mb-1">
                    <strong className="text-gray-700">Id:</strong>
                    <span className="ml-2">{submission.id}</span>
                  </p>
                </div>

                <button
                  onClick={() => handleDeleteSubmission(submission.id)}
                  className="px-3 py-1 bg-red-500 hover:bg-red-600 text-white rounded-md text-sm transition-colors"
                >
                  Delete
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
