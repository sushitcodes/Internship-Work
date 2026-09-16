import { useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import {
  useGetSubjectsByClassQuery,
  useCreateSubjectMutation,
  useDeleteSubjectMutation,
} from "../../infrastructure/api/gradeApi";
import { FormInput } from "../components/FormInput";
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
import { toast } from "sonner";

interface SubjectFormValues {
  name: string;
}

function ClassSubjectsPage() {
  // Non-null assertion is safe here — this page only renders behind
  // the /classes/:id/subjects route, so React Router guarantees the param.
  const { id: classRoomId } = useParams<{ id: string }>();
  const { data: subjects, isLoading } = useGetSubjectsByClassQuery(
    classRoomId!,
  );
  const [createSubject, { isLoading: isCreating }] = useCreateSubjectMutation();
  const [deleteSubject] = useDeleteSubjectMutation();
  const { register, handleSubmit, reset } = useForm<SubjectFormValues>();

  const onSubmit = async (data: SubjectFormValues) => {
    try {
      await createSubject({
        classRoomId: classRoomId!,
        name: data.name,
      }).unwrap();
      reset();
      toast.success("Subject added");
    } catch {
      toast.error("Failed to add subject");
    }
  };

  //delete taost
  const handleDelete = async (id: string) => {
    try {
      await deleteSubject({ id, classRoomId: classRoomId! }).unwrap();
      toast.success("Subject deleted");
    } catch (err: any) {
      toast.error(err?.data?.message ?? "Could not delete subject");
    }
  };
  return (
    <div className="max-w-2xl mx-auto mt-10 space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Add Subject</CardTitle>
        </CardHeader>
        <CardContent>
          <form
            onSubmit={handleSubmit(onSubmit)}
            className="flex gap-3 items-end"
          >
            <FormInput
              label="Subject Name"
              registration={register("name", { required: true })}
            />
            <Button type="submit" disabled={isCreating}>
              {isCreating ? "Adding..." : "Add"}
            </Button>
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Subjects</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading && (
            <p className="text-sm text-muted-foreground">Loading...</p>
          )}
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="bold ">Name</TableHead>
                <TableHead></TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {subjects?.map((s) => (
                <TableRow key={s.id}>
                  <TableCell>{s.name}</TableCell>
                  <TableCell>
                    <Button
                      className="text-sm text-red-500 "
                      variant="outline"
                      size="sm"
                      onClick={() => handleDelete(s.id)}
                    >
                      Delete
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
}

export default ClassSubjectsPage;
