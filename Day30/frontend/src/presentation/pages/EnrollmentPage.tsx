import { useState, useMemo } from "react";
import { useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import {
  useGetEnrollmentsByClassQuery,
  useEnrollStudentMutation,
  useRemoveEnrollmentMutation,
} from "../../infrastructure/api/enrollmentApi";
import { useGetStudentsQuery } from "../../infrastructure/api/userApi";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import {
  Table,
  TableHeader,
  TableBody,
  TableHead,
  TableRow,
  TableCell,
} from "@/components/ui/table";
import { PageHeader } from "../components/PageHeader";
import { getInitials } from "../../application/utils/getInitials";
import { ChevronsUpDown, Check, Search, UserPlus, Trash2 } from "lucide-react";

interface EnrollFormValues {
  studentUserId: string;
}

function EnrollmentPage() {
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
  const [openCombobox, setOpenCombobox] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");

  const { handleSubmit, reset, setValue, watch } = useForm<EnrollFormValues>({
    defaultValues: { studentUserId: "" },
  });

  const selectedUserId = watch("studentUserId");

  // Exclude students who are already enrolled in this class
  const enrolledHereIds = useMemo(
    () => new Set(enrollments?.map((e) => e.studentUserId) ?? []),
    [enrollments],
  );

  const availableStudents = useMemo(
    () => (students ?? []).filter((s) => !enrolledHereIds.has(s.id)),
    [students, enrolledHereIds],
  );

  // Filter students based on search input
  const filteredStudents = useMemo(() => {
    if (!searchQuery.trim()) return availableStudents;
    const q = searchQuery.toLowerCase();
    return availableStudents.filter(
      (s) =>
        s.fullName?.toLowerCase().includes(q) ||
        s.email?.toLowerCase().includes(q),
    );
  }, [availableStudents, searchQuery]);

  const selectedStudent = useMemo(
    () => students?.find((s) => s.id === selectedUserId),
    [students, selectedUserId],
  );

  const onSubmit = async () => {
    if (!selectedUserId) return;
    setError(null);
    try {
      await enrollStudent({
        studentUserId: selectedUserId,
        classRoomId: classRoomId!,
      }).unwrap();
      reset({ studentUserId: "" });
      setSearchQuery("");
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
    <div className="max-w-4xl mx-auto space-y-6">
      {/* 1. Unified Page Header */}
      <PageHeader
        title="Class Enrollment"
        description="Search and enroll available students into this classroom roster."
      />

      {/* 2. Enrollment Form with Searchable Combobox */}
      <Card className="border-border/70 shadow-xs">
        <CardHeader className="border-b bg-muted/20 pb-4">
          <CardTitle className="text-base font-semibold">
            Enroll a New Student
          </CardTitle>
          <CardDescription>
            Search by student full name or email address
          </CardDescription>
        </CardHeader>
        <CardContent className="pt-6">
          <form
            onSubmit={handleSubmit(onSubmit)}
            className="flex flex-col sm:flex-row gap-3 items-stretch sm:items-center"
          >
            {/* Searchable Student Picker */}
            <Popover open={openCombobox} onOpenChange={setOpenCombobox}>
              <PopoverTrigger
                render={
                  <Button
                    type="button"
                    variant="outline"
                    role="combobox"
                    aria-expanded={openCombobox}
                    className="flex-1 justify-between h-auto py-2 px-3 text-left hover:bg-background"
                  >
                    {selectedStudent ? (
                      <div className="flex items-center gap-3 min-w-0">
                        <Avatar className="h-7 w-7 text-xs shrink-0">
                          <AvatarFallback className="bg-primary/10 text-primary font-semibold">
                            {getInitials(
                              selectedStudent.fullName || selectedStudent.email,
                            )}
                          </AvatarFallback>
                        </Avatar>
                        <div className="min-w-0 truncate text-left">
                          <p className="text-sm font-medium text-foreground truncate">
                            {selectedStudent.fullName || "Unnamed Student"}
                          </p>
                          <p className="text-xs text-muted-foreground truncate">
                            {selectedStudent.email}
                          </p>
                        </div>
                      </div>
                    ) : (
                      <span className="text-muted-foreground text-sm flex items-center gap-2">
                        <Search className="h-4 w-4 text-muted-foreground/70" />
                        {isLoadingStudents
                          ? "Loading students..."
                          : "Select or search student..."}
                      </span>
                    )}
                    <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                  </Button>
                }
              />

              <PopoverContent className="w-90 sm:w-105 p-2" align="start">
                {/* Search Input */}
                <div className="flex items-center gap-2 border-b pb-2 px-1">
                  <Search className="h-4 w-4 text-muted-foreground shrink-0" />
                  <Input
                    placeholder="Search by name or email..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="h-8 text-sm border-0 focus-visible:ring-0 focus-visible:ring-offset-0 px-1"
                    autoFocus
                  />
                </div>

                {/* Students List */}
                <div className="max-h-60 overflow-y-auto pt-2 space-y-1">
                  {filteredStudents.length > 0 ? (
                    filteredStudents.map((s) => {
                      const isSelected = s.id === selectedUserId;
                      return (
                        <button
                          key={s.id}
                          type="button"
                          onClick={() => {
                            setValue("studentUserId", s.id);
                            setOpenCombobox(false);
                          }}
                          className={`w-full flex items-center justify-between p-2 rounded-lg text-left transition-colors ${
                            isSelected
                              ? "bg-primary/10 text-primary"
                              : "hover:bg-muted"
                          }`}
                        >
                          <div className="flex items-center gap-3 min-w-0">
                            <Avatar className="h-7 w-7 text-xs shrink-0">
                              <AvatarFallback className="bg-muted text-foreground">
                                {getInitials(s.fullName || s.email)}
                              </AvatarFallback>
                            </Avatar>
                            <div className="min-w-0 truncate">
                              <p className="text-sm font-medium truncate text-foreground">
                                {s.fullName || "Unnamed"}
                              </p>
                              <p className="text-xs text-muted-foreground truncate">
                                {s.email}
                              </p>
                            </div>
                          </div>
                          {isSelected && (
                            <Check className="h-4 w-4 text-primary shrink-0 ml-2" />
                          )}
                        </button>
                      );
                    })
                  ) : (
                    <div className="py-6 text-center text-xs text-muted-foreground">
                      {availableStudents.length === 0
                        ? "All students are already enrolled."
                        : "No students match your search."}
                    </div>
                  )}
                </div>
              </PopoverContent>
            </Popover>
            {/* Enroll Button */}
            <Button
              type="submit"
              disabled={isEnrolling || !selectedUserId}
              className="gap-1.5 shrink-0 shadow-xs h-10 px-5"
            >
              <UserPlus className="h-4 w-4" />
              {isEnrolling ? "Enrolling..." : "Enroll Student"}
            </Button>
          </form>

          {error && (
            <p className="text-sm text-destructive mt-3 bg-destructive/10 p-2.5 rounded-lg">
              {error}
            </p>
          )}
        </CardContent>
      </Card>

      {/* 3. Enrolled Students Table */}
      <Card className="border-border/70 overflow-hidden shadow-xs">
        <CardHeader className="border-b bg-muted/20 pb-4">
          <CardTitle className="text-base font-semibold">
            Enrolled Students ({enrollments?.length ?? 0})
          </CardTitle>
          <CardDescription>
            Students currently assigned to this classroom
          </CardDescription>
        </CardHeader>
        <CardContent className="p-0">
          {isLoading ? (
            <div className="p-6 text-sm text-muted-foreground text-center">
              Loading enrolled students...
            </div>
          ) : enrollments && enrollments.length > 0 ? (
            <Table>
              <TableHeader className="bg-muted/40">
                <TableRow>
                  <TableHead className="w-20">Roll No</TableHead>
                  <TableHead>Student</TableHead>
                  <TableHead>Class</TableHead>
                  <TableHead>Enrolled Date</TableHead>
                  <TableHead className="text-right">Action</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {enrollments.map((e) => (
                  <TableRow key={e.id} className="hover:bg-muted/30">
                    <TableCell className="font-semibold text-foreground">
                      #{e.rollNo}
                    </TableCell>
                    <TableCell>
                      <span className="font-medium text-foreground block">
                        {e.studentFullName}
                      </span>
                    </TableCell>
                    <TableCell>
                      <span className="text-xs px-2 py-0.5 rounded-md bg-secondary text-secondary-foreground font-medium">
                        {e.classRoomName}
                      </span>
                    </TableCell>
                    <TableCell className="text-muted-foreground text-sm">
                      {new Date(e.enrolledAt).toLocaleDateString()}
                    </TableCell>
                    <TableCell className="text-right">
                      <Button
                        variant="ghost"
                        size="sm"
                        disabled={isRemoving}
                        onClick={() => onRemove(e.studentUserId)}
                        className="text-destructive hover:text-destructive hover:bg-destructive/10 gap-1.5"
                      >
                        <Trash2 className="h-4 w-4" />
                        Remove
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <div className="py-12 text-center">
              <p className="text-sm font-medium text-foreground">
                No students enrolled yet
              </p>
              <p className="text-xs text-muted-foreground mt-0.5">
                Use the search box above to enroll students into this class.
              </p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

export default EnrollmentPage;
