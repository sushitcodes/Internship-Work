import { useState, useEffect } from "react";
import { format } from "date-fns";
import { useGetClassRoomsQuery } from "../../infrastructure/api/classRoomApi";
import {
  useGetRosterQuery,
  useMarkAttendanceMutation,
} from "../../infrastructure/api/attendanceApi";
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
import { toast } from "sonner";

const STATUS_OPTIONS = ["Present", "Absent", "Late", "Excused"];

const MarkAttendancePage: React.FC = () => {
  const { data: classRooms } = useGetClassRoomsQuery();
  const [classRoomId, setClassRoomId] = useState<string>("");
  const [date, setDate] = useState<Date>(new Date());
  const dateStr = format(date, "yyyy-MM-dd");

  // Roster IS the source of truth now — includes "Unmarked" for anyone
  // not yet touched today, and the REAL saved status for anyone who is.
  const { data: roster, isLoading: isLoadingRoster } = useGetRosterQuery(
    { classRoomId, date: dateStr },
    { skip: !classRoomId },
  );

  // enrollmentId -> status the STAFF has changed in this session.
  // Populated FROM the roster's real data, never a blind "Present" default.
  const [statuses, setStatuses] = useState<Record<string, string>>({});
  useEffect(() => {
    if (roster) {
      const fromServer: Record<string, string> = {};
      roster.forEach((r) => {
        fromServer[r.enrollmentId] = r.status;
      });
      setStatuses(fromServer);
    }
  }, [roster]);

  const [markAttendance, { isLoading: isSaving }] = useMarkAttendanceMutation();

  const handleSubmit = async () => {
    if (!classRoomId || !roster) return;

    // Only send entries that are a REAL status — "Unmarked" rows the staff
    // never touched are excluded entirely, so they can't be accidentally
    // overwritten just by loading the page and hitting Save.
    const entries = roster
      .map((r) => ({
        enrollmentId: r.enrollmentId,
        status: statuses[r.enrollmentId],
      }))
      .filter((e) => e.status && e.status !== "Unmarked");

    if (entries.length === 0) {
      toast.error("Mark at least one student before saving.");
      return;
    }

    try {
      await markAttendance({ classRoomId, date: dateStr, entries }).unwrap();
      toast.success("Attendance saved.");
    } catch (err) {
      console.error("Failed to save attendance:", err);
      toast.error("Could not save attendance. Please try again.");
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
              onValueChange={(v) => setClassRoomId(v ?? "")}
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
                    {format(date, "PP")}
                  </Button>
                }
              />
              <PopoverContent className="p-0" align="start">
                <Calendar
                  mode="single"
                  selected={date}
                  onSelect={(d) => d && setDate(d)}
                />
              </PopoverContent>
            </Popover>
          </div>
        </CardContent>
      </Card>

      {classRoomId && isLoadingRoster && (
        <p className="text-sm text-muted-foreground">Loading roster...</p>
      )}

      {classRoomId && roster && roster.length > 0 && (
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
                {roster.map((r) => (
                  <TableRow key={r.enrollmentId}>
                    <TableCell>{r.studentEmail}</TableCell>
                    <TableCell>
                      <Select
                        value={statuses[r.enrollmentId] ?? "Unmarked"}
                        onValueChange={(val) =>
                          setStatuses((prev) => ({
                            ...prev,
                            [r.enrollmentId]: val ?? "Unmarked",
                          }))
                        }
                      >
                        <SelectTrigger className="w-36">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          {/* "Unmarked" shown only as the CURRENT state, not
                              something staff picks deliberately to revert to */}
                          <SelectItem value="Unmarked" disabled>
                            Unmarked
                          </SelectItem>
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

            <Button
              onClick={handleSubmit}
              disabled={isSaving}
              className="mt-4 w-full"
            >
              {isSaving ? "Saving..." : "Save Attendance"}
            </Button>
          </CardContent>
        </Card>
      )}
    </div>
  );
};

export default MarkAttendancePage;
