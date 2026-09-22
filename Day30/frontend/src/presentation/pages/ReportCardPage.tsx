import { useGetMyClassQuery } from "../../infrastructure/api/enrollmentApi";
import {
  useGetMyReportCardQuery,
  downloadMyReportCardPdf,
} from "../../infrastructure/api/gradeApi";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from "@/components/ui/card";
import {
  Table,
  TableHeader,
  TableBody,
  TableHead,
  TableRow,
  TableCell,
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useState } from "react";
import { PageHeader } from "../components/PageHeader";
import { Download } from "lucide-react";
import { toast } from "sonner";

function ReportCardPage() {
  // The unique index on Enrollment.StudentUserId means a student has exactly
  // one classroom — useGetMyClassQuery (already built for attendance/enrollment)
  // gives us the classRoomId without the student ever picking one.
  const { data: myClass, isLoading: isLoadingClass } = useGetMyClassQuery();
  const [isDownloading, setIsDownloading] = useState(false);
  const {
    data: reportCard,
    isLoading: isLoadingCard,
    isError,
  } = useGetMyReportCardQuery(myClass?.classRoomId ?? "", {
    skip: !myClass?.classRoomId,
  });

  const handleDownloadPdf = async () => {
    if (!myClass?.classRoomId) return;
    try {
      setIsDownloading(true);
      await downloadMyReportCardPdf(
        myClass.classRoomId,
        reportCard?.studentName,
      );
      toast.success("Report card PDF downloaded!");
    } catch {
      toast.error("Could not download report card. Please try again.");
    } finally {
      setIsDownloading(false);
    }
  };

  if (isLoadingClass || isLoadingCard) {
    return (
      <div className="max-w-4xl mx-auto space-y-6">
        <Skeleton className="h-10 w-64" />
        <Skeleton className="h-64 w-full rounded-xl" />
      </div>
    );
  }
  // Not enrolled yet

  if (!isLoadingClass && !myClass) {
    return (
      <div className="max-w-4xl mx-auto py-12 text-center">
        <p className="text-sm font-medium text-foreground">
          You are not enrolled in any class yet.
        </p>
        <p className="text-xs text-muted-foreground mt-1">
          Please contact an administrator to be enrolled.
        </p>
      </div>
    );
  }
  if (isError) {
    return (
      <div className="max-w-4xl mx-auto py-12 text-center text-destructive">
        Could not load your report card.
      </div>
    );
  }

  const gradedSubjects =
    reportCard?.subjects.filter((s) => s.marksObtained !== null) ?? [];
  const totalObtained = gradedSubjects.reduce(
    (acc, curr) => acc + (curr.marksObtained ?? 0),
    0,
  );
  const totalMax = gradedSubjects.reduce((acc, curr) => acc + curr.maxMarks, 0);
  const overallPercentage =
    totalMax > 0 ? Math.round((totalObtained * 100) / totalMax) : 0;

  return (
    <div className="max-w-4xl mx-auto space-y-6 pb-12">
      {/* 1. Page Header with PDF Download Button */}
      <PageHeader
        title="My Report Card"
        description={`Academic evaluation for ${myClass?.classRoomName}`}
      >
        <Button
          onClick={handleDownloadPdf}
          disabled={
            isDownloading || !reportCard || reportCard.subjects.length === 0
          }
          size="sm"
          className="gap-2 shadow-xs"
        >
          <Download className="h-4 w-4" />
          {isDownloading ? "Generating PDF..." : "Download Official PDF"}
        </Button>
      </PageHeader>
      {/* 2. Overview Summary Cards */}
      {reportCard && reportCard.subjects.length > 0 && (
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <Card className="border-border/70 shadow-xs">
            <CardContent className="p-4">
              <span className="text-xs text-muted-foreground uppercase font-semibold">
                Overall Score
              </span>
              <p className="text-2xl font-bold text-foreground mt-0.5">
                {totalObtained} / {totalMax}
              </p>
            </CardContent>
          </Card>
          <Card className="border-border/70 shadow-xs">
            <CardContent className="p-4">
              <span className="text-xs text-muted-foreground uppercase font-semibold">
                Percentage
              </span>
              <p className="text-2xl font-bold text-primary mt-0.5">
                {overallPercentage}%
              </p>
            </CardContent>
          </Card>
          <Card className="border-border/70 shadow-xs">
            <CardContent className="p-4">
              <span className="text-xs text-muted-foreground uppercase font-semibold">
                Graded Subjects
              </span>
              <p className="text-2xl font-bold text-emerald-600 dark:text-emerald-400 mt-0.5">
                {gradedSubjects.length} / {reportCard.subjects.length}
              </p>
            </CardContent>
          </Card>
        </div>
      )}
      {/* 3. Subjects Table */}
      <Card className="border-border/70 overflow-hidden shadow-xs">
        <CardHeader className="border-b bg-muted/20 pb-4">
          <CardTitle className="text-base font-semibold">
            Subject Breakdown
          </CardTitle>
          <CardDescription>
            Marks and teacher feedback for this term
          </CardDescription>
        </CardHeader>
        <CardContent className="p-0">
          {reportCard && reportCard.subjects.length === 0 ? (
            <p className="text-sm text-muted-foreground py-10 text-center">
              No subjects are assigned to your class yet.
            </p>
          ) : (
            <Table>
              <TableHeader className="bg-muted/40">
                <TableRow>
                  <TableHead>Subject</TableHead>
                  <TableHead>Marks Obtained</TableHead>
                  <TableHead>Remarks</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {reportCard?.subjects.map((s) => (
                  <TableRow key={s.subjectName} className="hover:bg-muted/30">
                    <TableCell className="font-semibold text-foreground">
                      {s.subjectName}
                    </TableCell>
                    <TableCell>
                      {s.marksObtained === null ? (
                        <span className="text-xs text-muted-foreground">
                          Not graded yet
                        </span>
                      ) : (
                        <span className="font-medium text-foreground">
                          {s.marksObtained} / {s.maxMarks}
                        </span>
                      )}
                    </TableCell>
                    <TableCell className="text-muted-foreground text-sm">
                      {s.remarks ?? "—"}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
export default ReportCardPage;
