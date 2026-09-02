import { useState, useEffect } from "react";
import { format } from "date-fns";
import { useGetClassRoomsQuery } from "../../infrastructure/api/classRoomApi";
import { useGetEnrollmentsByClassQuery } from "../../infrastructure/api/enrollmentApi";
import { useMarkAttendanceMutation } from "../../infrastructure/api/attendanceApi";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import {
  Popover,
  PopoverTrigger,
  PopoverContent,
} from "@/components/ui/popover";
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";
import {
  Table,
  TableHeader,
  TableBody,
  TableHead,
  TableRow,
  TableCell,
} from "@/components/ui/table";
import { CalendarIcon } from "lucide-react";

const STATUS_OPTIONS = ["Present", "Absent", "Late", "Excused"];

const MarkAttendancePage: React.FC = () => {
  const { data: classRooms } = useGetClassRoomsQuery();
  const [classRoomId, setClassRoomId] = useState<string>("");
  const [date, setDate] = useState<Date | undefined>(new Date());

  const { data: enrollments } = useGetEnrollmentsByClassQuery(classRoomId, {
    skip: !classRoomId,
  });

  // enrollmentId -> status, defaulted to "Present" whenever the class changes
  const [statuses, setStatuses] = useState<Record<string, string>>({});
  useEffect(() => {
    if (enrollments) {
      const defaults: Record<string, string> = {};
      enrollments.forEach((e) => {
        defaults[e.id] = "Present";
      });
      setStatuses(defaults);
    }
  }, [enrollments]);

  const [markAttendance, { isLoading }] = useMarkAttendanceMutation();
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const handleSubmit = async () => {
    if (!classRoomId || !date || !enrollments) return;
    setError(null);
    setSuccess(false);
    try {
      await markAttendance({
        classRoomId,
        date: format(date, "yyyy-MM-dd"),
        entries: enrollments.map((e) => ({
          enrollmentId: e.id,
          status: statuses[e.id] ?? "Present",
        })),
      }).unwrap();
      setSuccess(true);
    } catch (err: any) {
      setError(err?.data ?? "Could not mark attendance.");
    }
  };

  return (
    <div className="max-w-2xl mx-auto mt-10 space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Mark Attendance</CardTitle>
        </CardHeader>
        <CardContent className="flex gap-3 items-end">
          <div>
            <label className="text-sm font-medium mb-2 block">Class</label>
            <Select
              onValueChange={(value) => setClassRoomId(value ?? "")}
              value={classRoomId}
            >
              <SelectTrigger className="w-48">
                <SelectValue placeholder="Select a class" />
              </SelectTrigger>
              <SelectContent>
                {classRooms?.map((c) => (
                  <SelectItem key={c.id} value={c.id}>
                    {c.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div>
            <label className="text-sm font-medium mb-2 block">Date</label>
            <Popover>
              <PopoverTrigger
                render={
                  <Button variant="outline" className="w-40 justify-start">
                    <CalendarIcon className="mr-2 h-4 w-4" />
                    {date ? format(date, "PP") : "Pick a date"}
                  </Button>
                }
              />
              <PopoverContent className="p-0" align="start">
                <Calendar mode="single" selected={date} onSelect={setDate} />
              </PopoverContent>
            </Popover>
          </div>
        </CardContent>
      </Card>

      {classRoomId && enrollments && enrollments.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Students</CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Email</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {enrollments.map((e) => (
                  <TableRow key={e.id}>
                    <TableCell>{e.studentEmail}</TableCell>
                    <TableCell>
                      <Select
                        value={statuses[e.id] ?? "Present"}
                        onValueChange={(val) =>
                          setStatuses((prev) => ({
                            ...prev,
                            [e.id]: val ?? "Present",
                          }))
                        }
                      >
                        <SelectTrigger className="w-32">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          {STATUS_OPTIONS.map((s) => (
                            <SelectItem key={s} value={s}>
                              {s}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>

            {error && <p className="text-sm text-red-500 mt-3">{error}</p>}
            {success && (
              <p className="text-sm text-green-600 mt-3">Attendance saved.</p>
            )}

            <Button
              onClick={handleSubmit}
              disabled={isLoading}
              className="mt-4 w-full"
            >
              {isLoading ? "Saving..." : "Save Attendance"}
            </Button>
          </CardContent>
        </Card>
      )}
    </div>
  );
};

export default MarkAttendancePage;
