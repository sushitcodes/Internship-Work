import { useParams, Link } from "react-router-dom";
import { useGetSubmissionByIdQuery } from "../../infrastructure/api/submissionApi";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import { ArrowLeft } from "lucide-react";

const SubmissionPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();

  const {
    data: submission,
    isLoading,
    isError,
  } = useGetSubmissionByIdQuery(id!, {
    skip: !id,
  });

  if (isLoading) {
    return (
      <Card className="max-w-2xl mx-auto mt-10">
        <CardHeader>
          <CardTitle>Submission Details</CardTitle>
        </CardHeader>
        <CardContent className="space-y-6">
          <Skeleton className="h-6 w-48" />
          <Skeleton className="h-6 w-64" />
          <Skeleton className="h-6 w-32" />
          <Skeleton className="h-24 w-full" />
          <Skeleton className="h-10 w-32" />
        </CardContent>
      </Card>
    );
  }

  if (isError || !submission) {
    return (
      <Card className="max-w-2xl mx-auto mt-10">
        <CardContent className="pt-6">
          <p className="text-red-500 text-center">
            Could not load this submission.
          </p>
        </CardContent>
      </Card>
    );
  }

  const apiOrigin = (import.meta.env.VITE_API_URL ?? "").replace(
    /\/api\/?$/,
    "",
  );
  const fileHref = `${apiOrigin}${submission.fileUrl}`;

  return (
    <Card className="max-w-2xl mx-auto mt-10">
      <CardHeader>
        <CardTitle>Submission Details</CardTitle>
      </CardHeader>

      <CardContent className="space-y-6">
        {/* Personal Information */}
        <div className="space-y-3">
          <h3 className="text-sm font-medium text-gray-500">
            Personal Information
          </h3>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <span className="block text-sm text-gray-500">Full Name</span>
              <span className="text-gray-800 font-medium">
                {submission.fullName}
              </span>
            </div>
            <div>
              <span className="block text-sm text-gray-500">Email</span>
              <span className="text-gray-800 font-medium">
                {submission.email}
              </span>
            </div>
            <div>
              <span className="block text-sm text-gray-500">Phone</span>
              <span className="text-gray-800 font-medium">
                {submission.phone}
              </span>
            </div>
          </div>
        </div>

        {/* Education */}
        <div className="space-y-3">
          <h3 className="text-sm font-medium text-gray-500">Education</h3>
          <div className="space-y-3">
            {submission.education.map((edu, index) => (
              <div key={index} className="border rounded-md p-3 bg-gray-50">
                <p className="font-medium text-gray-800">{edu.institution}</p>
                <p className="text-sm text-gray-600">
                  {edu.degree} — {edu.year}
                </p>
              </div>
            ))}
          </div>
        </div>

        {/* Actions */}
        <div className="flex flex-wrap gap-3 pt-2">
          {/* ✅ Fixed: Using Button with onClick instead of asChild */}
          <Button onClick={() => window.open(fileHref, "_blank")}>
            View / Download File
          </Button>
          <Link to={`/submission/${id}/edit`}>
            <Button variant="outline">Edit</Button>
          </Link>
        </div>

        {/* Back Link */}
        <div className="pt-4 border-t">
          <Link
            to="/"
            className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors"
          >
            <ArrowLeft className="h-4 w-4" />
            View all submissions
          </Link>
        </div>
      </CardContent>
    </Card>
  );
};

export default SubmissionPage;
