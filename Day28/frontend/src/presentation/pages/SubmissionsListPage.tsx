import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import {
  useGetSubmissionsInfiniteQuery,
  useDeleteSubmissionMutation,
} from "../../infrastructure/api/submissionApi";
import SubmissionCountBadge from "../components/SubmissionCountBadge";
const SubmissionsListPage: React.FC = () => {
  const [searchInput, setSearchInput] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearch(searchInput), 400);
    return () => clearTimeout(timer);
  }, [searchInput]);
  const [pageIndex, setPageIndex] = useState(0);

  useEffect(() => {
    setPageIndex(0);
  }, [debouncedSearch]);
  const { data, fetchNextPage, isLoading, isError, isFetchingNextPage } =
    useGetSubmissionsInfiniteQuery(debouncedSearch || undefined);
  const [deleteSubmission, { isLoading: isDeleting }] =
    useDeleteSubmissionMutation();

  const pages = data?.pages ?? [];
  const currentPage = pages[pageIndex];
  // const submissions = currentPage?.items ?? [];
  const submissions = pages.flatMap((page) => page.items);

  const handleNext = async () => {
    if (pageIndex < pages.length - 1) {
      setPageIndex((i) => i + 1);
      return;
    }
    if (currentPage?.hasNextPage) {
      await fetchNextPage();
      setPageIndex((i) => i + 1);
    }
  };

  const handlePrev = () => {
    // Always safe with zero network cost: reaching pageIndex N always
    // means pages 0..N-1 were already fetched to get here.
    if (pageIndex > 0) setPageIndex((i) => i - 1);
  };

  const canGoNext =
    pageIndex < pages.length - 1 || Boolean(currentPage?.hasNextPage);
  const canGoPrev = pageIndex > 0;

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

      <input
        type="text"
        value={searchInput}
        onChange={(e) => setSearchInput(e.target.value)}
        placeholder="Search by name or email..."
        className="w-full px-4 py-2 border border-gray-300 rounded-md mb-6 focus:outline-none focus:ring-2 focus:ring-blue-500"
      />

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

      <div className="flex justify-between items-center">
        <button
          onClick={handlePrev}
          disabled={!canGoPrev}
          className="px-4 py-2 text-sm border rounded-md disabled:opacity-40"
        >
          ← Previous
        </button>
        <span className="text-sm text-gray-500">Page {pageIndex + 1}</span>
        <button
          onClick={handleNext}
          disabled={!canGoNext || isFetchingNextPage}
          className="px-4 py-2 text-sm border rounded-md disabled:opacity-40"
        >
          {isFetchingNextPage ? "Loading..." : "Next →"}
        </button>
      </div>
    </div>
  );
};

export default SubmissionsListPage;
