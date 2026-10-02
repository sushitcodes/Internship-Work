import { useForm } from "react-hook-form";
import {
  useGetClassRoomsQuery,
  useCreateClassRoomMutation,
  useAssignClassTeacherMutation,
  useDeleteClassRoomMutation,
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
import { PageHeader } from "../components/PageHeader";
import { extractErrorMessage } from "@/lib/apiError";
import { useState } from "react";
import { ConfirmDialog } from "@/presentation/components/ConfirmDialog";

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
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [deleteName, setDeleteName] = useState("");
  const [deleteClassRoom, { isLoading: isDeleting }] =
    useDeleteClassRoomMutation();
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
    try {
      const created = await createClassRoom({
        ...data,
        academicYear: Number(data.academicYear),
      }).unwrap();
      toast.success(
        created.wasRestored
          ? `Class "${created.name}" was restored with its previous subjects.`
          : "Class created",
      );
      reset();
    } catch (err) {
      const e = err as { data?: { message?: string } | string };
      toast.error(
        (typeof e.data === "object" ? e.data?.message : e.data) ??
          "Could not create the class.",
      );
    }
  };
  const handleDelete = async () => {
    if (!deleteId) return;
    try {
      await deleteClassRoom(deleteId).unwrap();
      toast.success("Class deleted");
      setDeleteId(null);
      setDeleteName("");
    } catch (err) {
      toast.error(extractErrorMessage(err, "Could not delete the class."));
    }
  };

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <PageHeader
        title="Classrooms"
        description="Create academic classes and manage student enrollments."
      />
      <Card className="border-border/70 shadow-xs">
        <CardHeader className="border-b bg-muted/20 pb-4">
          <CardTitle className="text-base font-semibold">
            Create New Class
          </CardTitle>
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
                    <ConfirmDialog
                      trigger={
                        <Button
                          variant="outline"
                          size="sm"
                          className="text-red-500 hover:text-red-700 hover:bg-red-50"
                          disabled={c.studentCount > 0}
                          title={
                            c.studentCount > 0
                              ? "Remove all students first"
                              : "Delete class"
                          }
                          onClick={() => {
                            setDeleteId(c.id);
                            setDeleteName(c.name);
                          }}
                        >
                          Delete
                        </Button>
                      }
                      title="Are you sure?"
                      description={
                        <>
                          This will delete the class{" "}
                          <span className="font-semibold">{deleteName}</span>.
                          You can bring it back later by creating a class with
                          the same name and year.
                        </>
                      }
                      confirmLabel={isDeleting ? "Deleting..." : "Delete"}
                      confirmDisabled={isDeleting}
                      confirmClassName="bg-red-500 hover:bg-red-600"
                      onConfirm={handleDelete}
                      onCancel={() => {
                        setDeleteId(null);
                        setDeleteName("");
                      }}
                    />
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
