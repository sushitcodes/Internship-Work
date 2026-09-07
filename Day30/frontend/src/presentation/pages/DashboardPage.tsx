import { Link } from "react-router-dom";
import { useAppSelector } from "../../infrastructure/store/hooks";
import {
  useGetDashboardSummaryQuery,
  useGetMyDashboardQuery,
} from "../../infrastructure/api/dashboardApi";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Legend,
} from "recharts";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

const StatCard: React.FC<{ label: string; value: string | number }> = ({
  label,
  value,
}) => (
  <Card>
    <CardContent className="pt-6">
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className="text-3xl font-bold">{value}</p>
    </CardContent>
  </Card>
);

const STATUS_COLORS: Record<string, string> = {
  Present: "#22c55e",
  Absent: "#ef4444",
  Late: "#f59e0b",
  Excused: "#6366f1",
};

const DashboardPage: React.FC = () => {
  const roles = useAppSelector((state) => state.auth.roles);
  const isStaffOrAdmin = roles.includes("Staff") || roles.includes("Admin");

  const { data: summary, isLoading: isLoadingSummary } =
    useGetDashboardSummaryQuery(undefined, {
      skip: !isStaffOrAdmin,
    });
  const { data: mine, isLoading: isLoadingMine } = useGetMyDashboardQuery(
    undefined,
    {
      skip: isStaffOrAdmin,
    },
  );

  if (isStaffOrAdmin) {
    const totalMarkedToday =
      summary?.todayAttendanceBreakdown.reduce(
        (acc, curr) => acc + curr.count,
        0,
      ) ?? 0;
    const presentToday =
      summary?.todayAttendanceBreakdown.find((b) => b.status === "Present")
        ?.count ?? 0;
    const presentPercentage =
      totalMarkedToday === 0
        ? 0
        : Math.round((100 * presentToday) / totalMarkedToday);

    return (
      <div className="max-w-4xl mx-auto mt-10 space-y-6">
        <h1 className="text-2xl font-bold">Dashboard</h1>

        {isLoadingSummary ? (
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {[...Array(4)].map((_, i) => (
              <Skeleton key={i} className="h-24 w-full" />
            ))}
          </div>
        ) : (
          summary && (
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <StatCard label="Total Students" value={summary.totalStudents} />
              <StatCard label="Total Staff" value={summary.totalStaff} />
              <StatCard
                label="Total Submissions"
                value={summary.totalSubmissions}
              />
              <StatCard
                label="Today's Attendance"
                value={totalMarkedToday === 0 ? "—" : `${presentPercentage}%`}
              />
            </div>
          )
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Students vs Staff</CardTitle>
            </CardHeader>
            <CardContent>
              <ResponsiveContainer width="100%" height={200}>
                <BarChart
                  data={[
                    { name: "Students", count: summary?.totalStudents ?? 0 },
                    { name: "Staff", count: summary?.totalStaff ?? 0 },
                  ]}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis allowDecimals={false} />
                  <Tooltip />
                  <Bar dataKey="count" fill="#6366f1" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-base">Today's Attendance</CardTitle>
            </CardHeader>
            <CardContent>
              {summary?.todayAttendanceBreakdown.length ? (
                <ResponsiveContainer width="100%" height={200}>
                  <PieChart>
                    <Pie
                      data={summary.todayAttendanceBreakdown}
                      dataKey="count"
                      nameKey="status"
                      cx="50%"
                      cy="50%"
                      outerRadius={70}
                      // recharts' label callback exposes `name`/`value` — the
                      // GENERIC prop names it renders with, not "status"/"count"
                      // (those are just the data keys WE chose when feeding it
                      // data). Typed `any` here since recharts' own type for
                      // this callback doesn't know our data's shape.
                      // label={(entry: any) => `${entry.name}: ${entry.value}`}
                    >
                      {summary.todayAttendanceBreakdown.map((entry) => (
                        <Cell
                          key={entry.status}
                          fill={STATUS_COLORS[entry.status] ?? "#94a3b8"}
                        />
                      ))}
                    </Pie>
                    <Tooltip />
                    <Legend />
                  </PieChart>
                </ResponsiveContainer>
              ) : (
                <p className="text-sm text-muted-foreground text-center py-16">
                  No attendance marked yet today.
                </p>
              )}
            </CardContent>
          </Card>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Recent Submissions</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            {summary?.recentSubmissions.length ? (
              summary.recentSubmissions.map((s) => (
                <Link
                  key={s.id}
                  to={`/submission/${s.id}`}
                  className="flex justify-between text-sm py-2 border-b last:border-0 hover:bg-muted/50 px-2 -mx-2 rounded"
                >
                  <span className="font-medium">{s.fullName}</span>
                  <span className="text-muted-foreground">
                    {s.classRoomName}
                  </span>
                </Link>
              ))
            ) : (
              <p className="text-sm text-muted-foreground">
                No submissions yet.
              </p>
            )}
          </CardContent>
        </Card>
      </div>
    );
  }

  // Student view
  return (
    <div className="max-w-2xl mx-auto mt-10 space-y-6">
      <h1 className="text-2xl font-bold">My Dashboard</h1>

      {isLoadingMine ? (
        <div className="grid grid-cols-2 gap-4">
          <Skeleton className="h-24 w-full" />
          <Skeleton className="h-24 w-full" />
        </div>
      ) : (
        mine && (
          <div className="grid grid-cols-2 gap-4">
            <StatCard label="My Submissions" value={mine.mySubmissionsCount} />
            <StatCard
              label="My Attendance Rate"
              value={`${mine.myAttendancePercentage}%`}
            />
          </div>
        )
      )}

      <Card>
        <CardHeader>
          <CardTitle>My Recent Attendance</CardTitle>
        </CardHeader>
        <CardContent className="space-y-2">
          {mine?.myRecentAttendance.length ? (
            mine.myRecentAttendance.map((a) => (
              <div
                key={a.id}
                className="flex justify-between text-sm py-2 border-b last:border-0"
              >
                <span>{a.date}</span>
                <span className="font-medium">{a.status}</span>
              </div>
            ))
          ) : (
            <p className="text-sm text-muted-foreground">
              No attendance records yet.
            </p>
          )}
        </CardContent>
      </Card>
    </div>
  );
};

export default DashboardPage;
