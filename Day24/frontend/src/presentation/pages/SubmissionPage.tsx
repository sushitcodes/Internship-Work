import { useParams, Link } from "react-router-dom";
import { useGetSubmissionByIdQuery } from "../../infrastructure/api/submissionApi";
const SubmissionPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();

  // skip: !id tells RTK Query "don't even attempt the fetch if id is missing."
  const {
    data: submission,
    isLoading,
    isError,
  } = useGetSubmissionByIdQuery(id!, {
    skip: !id,
  });

  if (isLoading) {
    return (
      <div className="max-w-2xl mx-auto mt-10 p-6 text-center text-gray-500">
        Loading...
      </div>
    );
  }

  if (isError || !submission) {
    return (
      <div className="max-w-2xl mx-auto mt-10 p-6 text-center text-red-500">
        Could not load this submission.
      </div>
    );
  }

  const apiOrigin = (import.meta.env.VITE_API_URL ?? "").replace(
    /\/api\/?$/,
    "",
  );
  const fileHref = `${apiOrigin}${submission.fileUrl}`;

  return (
    <div className="max-w-2xl mx-auto mt-10 p-6 bg-white rounded-lg shadow-md">
      <h2 className="text-2xl font-bold mb-6">Submission Details</h2>

      <div className="space-y-3 mb-6">
        <div>
          <span className="block text-sm text-gray-500">Full Name</span>
          <span className="text-gray-800 font-medium">
            {submission.fullName}
          </span>
        </div>
        <div>
          <span className="block text-sm text-gray-500">Email</span>
          <span className="text-gray-800 font-medium">{submission.email}</span>
        </div>
      </div>

      <h3 className="text-lg font-semibold text-gray-800 mb-3">Education</h3>
      <div className="space-y-3 mb-6">
        {submission.education.map((edu, index) => (
          <div
            key={index}
            className="border border-gray-200 rounded-md p-3 bg-gray-50"
          >
            <p className="font-medium text-gray-800">{edu.institution}</p>
            <p className="text-sm text-gray-600">
              {edu.degree} — {edu.year}
            </p>
          </div>
        ))}
      </div>

      <a
        href={fileHref}
        target="_blank"
        rel="noopener noreferrer"
        className="inline-block px-4 py-2 bg-blue-600 text-white rounded-md text-sm"
      >
        View / Download File
      </a>

      <div className="mt-6">
        <Link
          to="/submissions"
          className="text-blue-600 hover:underline text-sm"
        >
          View all submissions
        </Link>
      </div>
    </div>
  );
};

export default SubmissionPage;
