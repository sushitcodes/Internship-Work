import { useGetSubmissionsCountQuery } from "../../infrastructure/api/submissionApi";
import { Badge } from "@/components/ui/badge";

const SubmissionCountBadge: React.FC = () => {
  // data is a plain number here (defaults to 0 while loading/undefined),
  // NOT an array — this hook hits /api/submissions/count, which returns
  // one integer, not a list of submissions.
  const { data: count = 0 } = useGetSubmissionsCountQuery();

  return (
    <Badge variant="secondary" className="text-xs font-semibold">
      {count} Submission{count !== 1 ? "s" : ""}
    </Badge>
  );
};
export default SubmissionCountBadge;
