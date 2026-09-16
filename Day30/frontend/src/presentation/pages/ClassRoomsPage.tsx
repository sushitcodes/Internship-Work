import { useForm } from "react-hook-form";
import {
  useGetClassRoomsQuery,
  useCreateClassRoomMutation,
  useAssignClassTeacherMutation,
} from "../../infrastructure/api/classRoomApi";
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
import { Link } from "react-router-dom";
import { getModuleUrls } from "@/routes/getModuleUrls";
import { toast } from "sonner";
import { useSearchUsersInfiniteQuery } from "@/infrastructure/api/userApi";
import { IdSelect } from "../components/IdSelect";

interface ClassRoomFormValues {
  name: string;
  academicYear: number;
}

function ClassRoomsPage() {
  const UNASSIGNED = "unassigned";
  const { data: classRooms, isLoading } = useGetClassRoomsQuery();
  const [createClassRoom, { isLoading: isCreating }] =
    useCreateClassRoomMutation();
  const { register, handleSubmit, reset } = useForm<ClassRoomFormValues>();
  const { data: staffPages } = useSearchUsersInfiniteQuery({
    role: "Staff",
    pageSize: 100,
  });
  const staffOptions = [
    { id: UNASSIGNED, label: "Unassigned" }, // ADD — first in the list
    ...(staffPages?.pages[0]?.items.map((u) => ({
      id: u.userId,
      label: u.fullName,
    })) ?? []),
  ];
  const [assignClassTeacher] = useAssignClassTeacherMutation();

  const handleAssign = async (classRoomId: string, selectedId: string) => {
    const teacherUserId = selectedId === UNASSIGNED ? null : selectedId;
    try {
      await assignClassTeacher({ classRoomId, teacherUserId }).unwrap();
      toast.success("Teacher assigned");
    } catch {
      toast.error("Failed to assign teacher:");
    }
  };

  const onSubmit = async (data: ClassRoomFormValues) => {
    await createClassRoom({ ...data, academicYear: Number(data.academicYear) });
    reset();
  };

  return (
    <div className="max-w-3xl mx-auto mt-10 space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Create Class</CardTitle>
        </CardHeader>
        <CardContent>
          <form
            onSubmit={handleSubmit(onSubmit)}
            className="flex gap-3 items-end"
          >
            <FormInput
              label="Class Name"
              registration={register("name", { required: true })}
            />
            <FormInput
              label="Academic Year"
              type="number"
              registration={register("academicYear", {
                required: true,
                valueAsNumber: true,
              })}
            />
            <Button type="submit" disabled={isCreating}>
              {isCreating ? "Creating..." : "Create"}
            </Button>
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Classes</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading && <p>Loading...</p>}
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Name</TableHead>
                <TableHead>Year</TableHead>
                <TableHead>Students</TableHead>
                <TableHead>Class Teacher</TableHead>

                <TableHead></TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {classRooms?.map((c) => (
                <TableRow key={c.id}>
                  <TableCell>{c.name}</TableCell>
                  <TableCell>{c.academicYear}</TableCell>
                  <TableCell>{c.studentCount}</TableCell>
                  <TableCell>
                    <IdSelect
                      options={staffOptions}
                      value={c.classTeacherUserId ?? UNASSIGNED}
                      onValueChange={(v) => handleAssign(c.id, v)}
                      placeholder="Unassigned"
                      className="w-44"
                    />
                  </TableCell>

                  <TableCell>
                    <Link to={getModuleUrls("classEnroll", { id: c.id })}>
                      <Button variant="outline" size="sm">
                        Manage Enrollment
                      </Button>
                    </Link>
                    <Link to={getModuleUrls("classSubjects", { id: c.id })}>
                      <Button variant="outline" size="sm">
                        Manage Subjects
                      </Button>
                    </Link>
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

export default ClassRoomsPage;
