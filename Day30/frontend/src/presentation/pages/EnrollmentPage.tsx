import { useState } from "react";
import { useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import {
  useGetEnrollmentsByClassQuery,
  useEnrollStudentMutation,
  useRemoveEnrollmentMutation,
} from "../../infrastructure/api/enrollmentApi";
import { useGetStudentsQuery } from "../../infrastructure/api/userApi";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableHeader,
  TableBody,
  TableHead,
  TableRow,
  TableCell,
} from "@/components/ui/table";
import { IdSelect } from "../components/IdSelect";

interface EnrollFormValues {
  studentUserId: string;
}

const EnrollmentPage: React.FC = () => {
  const { id: classRoomId } = useParams<{ id: string }>();
  const { data: enrollments, isLoading } = useGetEnrollmentsByClassQuery(
    classRoomId!,
    { skip: !classRoomId },
  );
  const { data: students, isLoading: isLoadingStudents } =
    useGetStudentsQuery();
  const [enrollStudent, { isLoading: isEnrolling }] =
    useEnrollStudentMutation();
  const [removeEnrollment, { isLoading: isRemoving }] =
    useRemoveEnrollmentMutation();
  const [error, setError] = useState<string | null>(null);

  // ← Changed: Removed Controller, added setValue and watch
  const { handleSubmit, reset, setValue, watch } = useForm<EnrollFormValues>({
    defaultValues: { studentUserId: "" },
  });

  const studentUserId = watch("studentUserId");

  // Hide students already enrolled in THIS class from the dropdown.
  // (Backend still blocks the "enrolled elsewhere" case with a clear error.)
  const enrolledHereIds = new Set(
    enrollments?.map((e) => e.studentUserId) ?? [],
  );
  const availableStudents = (students ?? []).filter(
    (s) => !enrolledHereIds.has(s.id),
  );

  const onSubmit = async () => {
    // ← Changed: no longer needs data parameter
    setError(null);
    try {
      await enrollStudent({
        studentUserId: studentUserId, // ← Use the watched value
        classRoomId: classRoomId!,
      }).unwrap();
      reset();
    } catch (err: any) {
      setError(err?.data ?? "Could not enroll student.");
    }
  };
  const onRemove = async (studentUserId: string) => {
    setError(null);
    try {
      await removeEnrollment({
        studentUserId,
        classRoomId: classRoomId!,
      }).unwrap();
    } catch (err: any) {
      setError(err?.data ?? "Could not remove student.");
    }
  };

  return (
    <div className="max-w-2xl mx-auto mt-10 space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Enroll Student</CardTitle>
        </CardHeader>
        <CardContent>
          <form
            onSubmit={handleSubmit(onSubmit)}
            className="flex gap-3 items-end"
          >
            <IdSelect
              options={availableStudents.map((s) => ({
                id: s.id,
                label: `${s.email} - ${s.fullName}`,
              }))}
              value={studentUserId}
              onValueChange={(v) => setValue("studentUserId", v ?? "")}
              placeholder={
                isLoadingStudents ? "Loading..." : "Select a student"
              }
              className="w-60"
            />
            <Button type="submit" disabled={isEnrolling || !studentUserId}>
              {isEnrolling ? "Enrolling..." : "Enroll"}
            </Button>
          </form>
          {error && <p className="text-sm text-red-500 mt-2">{error}</p>}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Enrolled Students</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading && <p>Loading...</p>}
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Roll No</TableHead>
                <TableHead>Name</TableHead>
                <TableHead>Class</TableHead>
                <TableHead>Enrolled</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {enrollments?.map((e) => (
                <TableRow key={e.id}>
                  <TableCell>{e.rollNo}</TableCell>
                  <TableCell>{e.studentFullName}</TableCell>
                  <TableCell>{e.classRoomName}</TableCell>
                  <TableCell>
                    {new Date(e.enrolledAt).toLocaleDateString()}
                  </TableCell>
                  <TableCell className="text-right">
                    <Button
                      variant="destructive"
                      size="sm"
                      disabled={isRemoving}
                      onClick={() => onRemove(e.studentUserId)}
                    >
                      Remove
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
};
export default EnrollmentPage;
