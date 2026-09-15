import { useGetMyClassQuery } from "../../infrastructure/api/enrollmentApi";
import { useGetMyReportCardQuery } from "../../infrastructure/api/gradeApi";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Table,
  TableHeader,
  TableBody,
  TableHead,
  TableRow,
  TableCell,
} from "@/components/ui/table";

const ReportCardPage: React.FC = () => {
  // The unique index on Enrollment.StudentUserId means a student has exactly
  // one classroom — useGetMyClassQuery (already built for attendance/enrollment)
  // gives us the classRoomId without the student ever picking one.
  const { data: myClass, isLoading: isLoadingClass } = useGetMyClassQuery();

  const {
    data: reportCard,
    isLoading: isLoadingCard,
    isError,
  } = useGetMyReportCardQuery(myClass?.classRoomId ?? "", {
    skip: !myClass?.classRoomId,
  });

  if (isLoadingClass || isLoadingCard) {
    return (
      <p className="text-sm text-muted-foreground mt-10 text-center">
        Loading...
      </p>
    );
  }
  // Not enrolled yet

  if (!isLoadingClass && !myClass) {
    return (
      <p className="text-sm text-muted-foreground mt-10 text-center">
        You're not enrolled in a class yet.
      </p>
    );
  }
  if (isError) {
    return (
      <p className="text-sm text-muted-foreground mt-10 text-center">
        Could not load your report card.
      </p>
    );
  }

  return (
    <div className="max-w-2xl mx-auto mt-10 space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Report Card — {myClass?.classRoomName}</CardTitle>
        </CardHeader>
        <CardContent>
          {reportCard && reportCard.subjects.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              No subjects are assigned to your class yet.
            </p>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Subject</TableHead>
                  <TableHead>Marks</TableHead>
                  <TableHead>Remarks</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {reportCard?.subjects.map((s) => (
                  <TableRow key={s.subjectName}>
                    <TableCell>{s.subjectName}</TableCell>
                    <TableCell>
                      {s.marksObtained === null
                        ? "Not graded yet"
                        : `${s.marksObtained} / ${s.maxMarks}`}
                    </TableCell>
                    <TableCell>{s.remarks ?? "—"}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  );
};

export default ReportCardPage;
