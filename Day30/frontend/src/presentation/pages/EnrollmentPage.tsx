import { useState } from "react";
import { useParams } from "react-router-dom";
import { useForm, Controller } from "react-hook-form";
import {
  useGetEnrollmentsByClassQuery,
  useEnrollStudentMutation,
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
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";

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
  const [error, setError] = useState<string | null>(null);
  const { control, handleSubmit, reset } = useForm<EnrollFormValues>();

  const onSubmit = async (data: EnrollFormValues) => {
    setError(null);
    try {
      await enrollStudent({
        studentUserId: data.studentUserId,
        classRoomId: classRoomId!,
      }).unwrap();
      reset();
    } catch (err: any) {
      setError(err?.data ?? "Could not enroll student.");
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
            <Controller
              name="studentUserId"
              control={control}
              rules={{ required: true }}
              render={({ field }) => (
                <Select onValueChange={field.onChange} value={field.value}>
                  <SelectTrigger className="w-60">
                    <SelectValue
                      placeholder={
                        isLoadingStudents ? "Loading..." : "Select a student"
                      }
                    />
                  </SelectTrigger>
                  <SelectContent>
                    {students?.map((s) => (
                      <SelectItem key={s.id} value={s.id}>
                        {s.email}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
            <Button type="submit" disabled={isEnrolling}>
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
                <TableHead>Email</TableHead>
                <TableHead>Enrolled</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {enrollments?.map((e) => (
                <TableRow key={e.id}>
                  <TableCell>{e.studentEmail}</TableCell>
                  <TableCell>
                    {new Date(e.enrolledAt).toLocaleDateString()}
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
