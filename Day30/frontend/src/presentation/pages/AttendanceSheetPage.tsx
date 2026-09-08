import { useState } from "react";
import { format, startOfMonth, endOfMonth, subMonths } from "date-fns";
import { useGetClassRoomsQuery } from "../../infrastructure/api/classRoomApi";
import {
  useGetAttendanceSheetQuery,
  downloadAttendanceSheet,
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
import { CalendarIcon, Download } from "lucide-react";
import { toast } from "sonner";

const STATUS_BADGE: Record<string, string> = {
  Present: "bg-green-100 text-green-800",
  Absent: "bg-red-100 text-red-800",
  Late: "bg-amber-100 text-amber-800",
  Excused: "bg-indigo-100 text-indigo-800",
  Unmarked: "bg-gray-100 text-gray-500",
};

const AttendanceSheetPage: React.FC = () => {
  const { data: classRooms } = useGetClassRoomsQuery();
  const [classRoomId, setClassRoomId] = useState("");

  const today = new Date();
  const [range, setRange] = useState<{ from: Date; to: Date }>({
    from: startOfMonth(today),
    to: today,
  });
  const startDate = format(range.from, "yyyy-MM-dd");
  const endDate = format(range.to, "yyyy-MM-dd");

  const {
    data: sheet,
    isLoading,
    isFetching,
  } = useGetAttendanceSheetQuery(
    { classRoomId, startDate, endDate },
    { skip: !classRoomId },
  );

  const setThisMonth = () => setRange({ from: startOfMonth(today), to: today });
  const setLastMonth = () => {
    const lastMonth = subMonths(today, 1);
    setRange({ from: startOfMonth(lastMonth), to: endOfMonth(lastMonth) });
  };

  const handleDownload = async () => {
    if (!classRoomId) return;
    try {
      await downloadAttendanceSheet(classRoomId, startDate, endDate);
    } catch {
      toast.error("Could not export the sheet. Please try again.");
    }
  };

  return (
    <div className="max-w-5xl mx-auto mt-10 space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Attendance Sheet</CardTitle>
        </CardHeader>
        <CardContent className="flex flex-wrap gap-3 items-end">
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

          <div className="flex gap-2">
            <Button variant="outline" size="sm" onClick={setThisMonth}>
              This Month
            </Button>
            <Button variant="outline" size="sm" onClick={setLastMonth}>
              Last Month
            </Button>
          </div>

          <div>
            <label className="text-sm font-medium mb-2 block">Date Range</label>
            <Popover>
              <PopoverTrigger
                render={
                  <Button variant="outline" className="justify-start">
                    <CalendarIcon className="mr-2 h-4 w-4" />
                    {format(range.from, "MMM d")} –{" "}
                    {format(range.to, "MMM d, yyyy")}
                  </Button>
                }
              />
              <PopoverContent className="p-0" align="start">
                <Calendar
                  mode="range"
                  selected={range}
                  onSelect={(r) =>
                    r?.from && r?.to && setRange({ from: r.from, to: r.to })
                  }
                />
              </PopoverContent>
            </Popover>
          </div>

          <Button
            onClick={handleDownload}
            disabled={!classRoomId || !sheet}
            className="gap-2"
          >
            <Download className="h-4 w-4" />
            Download Excel
          </Button>
        </CardContent>
      </Card>

      {classRoomId && (isLoading || isFetching) && (
        <p className="text-sm text-muted-foreground">Loading sheet...</p>
      )}

      {sheet && (
        <Card>
          <CardContent className="pt-6 overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="sticky left-0 bg-background">
                    Student
                  </TableHead>
                  {sheet.dates.map((d) => (
                    <TableHead
                      key={d}
                      className="text-center whitespace-nowrap"
                    >
                      {format(new Date(d), "MMM d")}
                    </TableHead>
                  ))}
                </TableRow>
              </TableHeader>
              <TableBody>
                {sheet.rows.map((row) => (
                  <TableRow key={row.enrollmentId}>
                    <TableCell className="sticky left-0 bg-background font-medium whitespace-nowrap">
                      {row.studentName}
                    </TableCell>
                    {sheet.dates.map((d) => {
                      const status = row.statusByDate[d] ?? "Unmarked";
                      return (
                        <TableCell key={d} className="text-center">
                          <span
                            className={`text-xs px-2 py-1 rounded ${STATUS_BADGE[status]}`}
                          >
                            {status[0]}
                          </span>
                        </TableCell>
                      );
                    })}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}
    </div>
  );
};

export default AttendanceSheetPage;
