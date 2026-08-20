import { Link } from "react-router-dom";
import {
  useGetSubmissionsQuery,
  useDeleteSubmissionMutation,
} from "../../infrastructure/api/submissionApi";
import SubmissionCountBadge from "../components/SubmissionCountBadge";
const SubmissionsListPage: React.FC = () => {
  const {
    data: submissions = [],
    isLoading,
    isError,
  } = useGetSubmissionsQuery();
  const [deleteSubmission, { isLoading: isDeleting }] =
    useDeleteSubmissionMutation();
  const handleDelete = async (id: string, name: string) => {
    const confirmed = window.confirm(
      `Delete submission from ${name}? This cannot be undone.`,
    );
    if (!confirmed) return;

    try {
      await deleteSubmission(id).unwrap();
      // No manual state update needed — invalidatesTags in submissionApi.ts
      // already told RTK Query to refetch the list automatically.
    } catch (err) {
      console.error("Failed to delete submission:", err);
      alert("Could not delete this submission. Please try again.");
    }
  };

  return (
    <div className="max-w-3xl mx-auto mt-10 p-6 bg-white rounded-lg shadow-md">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-2xl font-bold">Student</h2>
        <SubmissionCountBadge />

        <Link to="/" className="text-blue-600 hover:underline text-sm">
          + New Submission
        </Link>
      </div>

      {isLoading && <p className="text-gray-500 text-center">Loading...</p>}
      {isError && (
        <p className="text-red-500 text-center">Could not load submissions.</p>
      )}
      {!isLoading && submissions.length === 0 && (
        <p className="text-gray-500 text-center">No submissions yet.</p>
      )}

      {submissions.length > 0 && (
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="border-b border-gray-200 text-sm text-gray-500">
              <th className="py-2 pr-4">Name</th>
              <th className="py-2 pr-4">Email</th>
              <th className="py-2"></th>
            </tr>
          </thead>
          <tbody>
            {submissions.map((s) => (
              <tr
                key={s.id}
                className="border-b border-gray-100 hover:bg-gray-50"
              >
                <td className="py-3 pr-4 font-medium text-gray-800">
                  {s.fullName}
                </td>
                <td className="py-3 pr-4 text-gray-600">{s.email}</td>
                <td className="py-3">
                  <Link
                    to={`/submission/${s.id}`}
                    className="text-blue-600 hover:underline text-sm"
                  >
                    View →
                  </Link>
                </td>
                <td className="py-3">
                  <button
                    onClick={() => handleDelete(s.id, s.fullName)}
                    disabled={isDeleting}
                    className="text-red-500 hover:text-red-700 text-sm font-medium disabled:opacity-50"
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default SubmissionsListPage;
