import { useGetSubmissionsCountQuery } from "../../infrastructure/api/submissionApi";

const SubmissionCountBadge: React.FC = () => {
  // data is a plain number here (defaults to 0 while loading/undefined),
  // NOT an array — this hook hits /api/submissions/count, which returns
  // one integer, not a list of submissions.
  const { data: count = 0 } = useGetSubmissionsCountQuery();

  return (
    <span className="bg-blue-100 text-blue-800 text-xs font-semibold mr-2 px-2.5 py-0.5 rounded-full">
      {count} Submission{count !== 1 ? "s" : ""}
    </span>
  );
};

export default SubmissionCountBadge;
