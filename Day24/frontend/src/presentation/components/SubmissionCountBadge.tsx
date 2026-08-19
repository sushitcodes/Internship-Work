import { useGetSubmissionsQuery } from "../../infrastructure/api/submissionApi";
const SubmissionCountBadge: React.FC = () => {
  const { data: submissions = [] } = useGetSubmissionsQuery();

  return (
    <span className="bg-blue-100 text-blue-800 text-xs font-semibold mr-2 px-2.5 py-0.5 rounded-full">
      {submissions.length} Submission{submissions.length !== 1 ? "s" : ""}
    </span>
  );
};
export default SubmissionCountBadge;
