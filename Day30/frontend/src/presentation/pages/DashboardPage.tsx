import React from "react";
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
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import { useGetMyClassQuery } from "../../infrastructure/api/enrollmentApi";
import { getModuleUrls } from "@/routes/getModuleUrls";
import { Paths } from "../../routes/paths";
import {
  Users,
  GraduationCap,
  FileText,
  CalendarCheck,
  Award,
  ArrowUpRight,
  ClipboardCheck,
  BookOpen,
  Calendar,
  Sparkles,
  Inbox,
} from "lucide-react";

// Color mapping for attendance statuses
const STATUS_CONFIG: Record<
  string,
  { fill: string; badgeClass: string; label: string }
> = {
  Present: {
    fill: "#10b981",
    badgeClass:
      "bg-emerald-500/10 text-emerald-700 dark:text-emerald-400 border-emerald-500/20",
    label: "Present",
  },
  Absent: {
    fill: "#f43f5e",
    badgeClass:
      "bg-rose-500/10 text-rose-700 dark:text-rose-400 border-rose-500/20",
    label: "Absent",
  },
  Late: {
    fill: "#f59e0b",
    badgeClass:
      "bg-amber-500/10 text-amber-700 dark:text-amber-400 border-amber-500/20",
    label: "Late",
  },
  Excused: {
    fill: "#6366f1",
    badgeClass:
      "bg-indigo-500/10 text-indigo-700 dark:text-indigo-400 border-indigo-500/20",
    label: "Excused",
  },
};

// Reusable Metric Stat Card
interface StatCardProps {
  label: string;
  value: string | number;
  icon: React.ElementType;
  description?: string;
  iconBg?: string;
  iconColor?: string;
}

const StatCard: React.FC<StatCardProps> = ({
  label,
  value,
  icon: Icon,
  description,
  iconBg = "bg-primary/10",
  iconColor = "text-primary",
}) => (
  <Card className="border-border/70 shadow-xs transition-all hover:shadow-sm">
    <CardContent className="p-5">
      <div className="flex items-center justify-between">
        <div className="space-y-1">
          <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider">
            {label}
          </p>
          <div className="text-2xl font-bold tracking-tight text-foreground">
            {value}
          </div>
          {description && (
            <p className="text-xs text-muted-foreground">{description}</p>
          )}
        </div>
        <div className={`p-3 rounded-xl ${iconBg} ${iconColor} shrink-0`}>
          <Icon className="h-5 w-5" />
        </div>
      </div>
    </CardContent>
  </Card>
);

// Attendance Status Pill Badge
const StatusBadge: React.FC<{ status: string }> = ({ status }) => {
  const config = STATUS_CONFIG[status] || {
    badgeClass: "bg-muted text-muted-foreground border-border",
    label: status,
  };
  return (
    <span
      className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium border ${config.badgeClass}`}
    >
      {config.label}
    </span>
  );
};

// Minimalist Empty State Component
const EmptyState: React.FC<{ message: string; submessage?: string }> = ({
  message,
  submessage,
}) => (
  <div className="flex flex-col items-center justify-center py-10 text-center">
    <div className="w-12 h-12 rounded-full bg-muted/60 flex items-center justify-center text-muted-foreground mb-3">
      <Inbox className="w-5 h-5 opacity-70" />
    </div>
    <p className="text-sm font-medium text-foreground">{message}</p>
    {submessage && (
      <p className="text-xs text-muted-foreground mt-0.5">{submessage}</p>
    )}
  </div>
);

const DashboardPage: React.FC = () => {
  const roles = useAppSelector((state) => state.auth.roles);
  const email = useAppSelector((state) => state.auth.email);
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

  const { data: myClass } = useGetMyClassQuery(undefined, {
    skip: isStaffOrAdmin,
  });

  // Time-based greeting helper
  const hour = new Date().getHours();
  const greeting =
    hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";

  const todayFormatted = new Intl.DateTimeFormat("en-US", {
    weekday: "long",
    month: "short",
    day: "numeric",
  }).format(new Date());

  // -------------------------------------------------------------
  // STAFF & ADMIN DASHBOARD
  // -------------------------------------------------------------
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
      <div className="max-w-6xl mx-auto space-y-6 pb-12 pt-2">
        {/* Header with Quick Actions */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b pb-5">
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-2xl font-bold tracking-tight">
                {greeting}, {email?.split("@")[0] || "User"}
              </h1>
              <span className="inline-flex items-center gap-1 rounded-full bg-primary/10 px-2 py-0.5 text-xs font-semibold text-primary">
                <Sparkles className="h-3 w-3" />
                {roles.includes("Admin") ? "Admin" : "Staff"}
              </span>
            </div>
            <p className="text-sm text-muted-foreground mt-0.5 flex items-center gap-1.5">
              <Calendar className="h-3.5 w-3.5" />
              {todayFormatted} · Campus Overview
            </p>
          </div>

          <div className="flex items-center gap-2.5">
            <Link to={Paths.markAttendance}>
              <Button size="sm" className="gap-1.5 shadow-xs">
                <ClipboardCheck className="h-4 w-4" />
                Mark Attendance
              </Button>
            </Link>
            <Link to={Paths.submissions}>
              <Button size="sm" variant="outline" className="gap-1.5">
                <FileText className="h-4 w-4" />
                Submissions
              </Button>
            </Link>
          </div>
        </div>

        {/* Top 4 Stat Cards */}
        {isLoadingSummary ? (
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
            {[...Array(4)].map((_, i) => (
              <Skeleton key={i} className="h-28 w-full rounded-xl" />
            ))}
          </div>
        ) : (
          summary && (
            <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
              <StatCard
                label="Students"
                value={summary.totalStudents}
                icon={GraduationCap}
                description="Total enrolled"
                iconBg="bg-blue-500/10"
                iconColor="text-blue-600 dark:text-blue-400"
              />
              <StatCard
                label="Staff Members"
                value={summary.totalStaff}
                icon={Users}
                description="Active educators"
                iconBg="bg-indigo-500/10"
                iconColor="text-indigo-600 dark:text-indigo-400"
              />
              <StatCard
                label="Submissions"
                value={summary.totalSubmissions}
                icon={FileText}
                description="Submitted tasks"
                iconBg="bg-violet-500/10"
                iconColor="text-violet-600 dark:text-violet-400"
              />
              <StatCard
                label="Today's Attendance"
                value={totalMarkedToday === 0 ? "—" : `${presentPercentage}%`}
                icon={CalendarCheck}
                description={
                  totalMarkedToday === 0
                    ? "Pending marking"
                    : `${presentToday}/${totalMarkedToday} present`
                }
                iconBg="bg-emerald-500/10"
                iconColor="text-emerald-600 dark:text-emerald-400"
              />
            </div>
          )
        )}

        {/* Charts Row */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
          {/* Students vs Staff Bar Chart */}
          <Card className="border-border/70">
            <CardHeader className="pb-2">
              <CardTitle className="text-base font-semibold">
                User Breakdown
              </CardTitle>
              <CardDescription>
                Comparison of enrolled students vs staff
              </CardDescription>
            </CardHeader>
            <CardContent>
              <ResponsiveContainer width="100%" height={220}>
                <BarChart
                  data={[
                    { name: "Students", count: summary?.totalStudents ?? 0 },
                    { name: "Staff", count: summary?.totalStaff ?? 0 },
                  ]}
                  margin={{ top: 10, right: 10, left: -20, bottom: 0 }}
                >
                  <CartesianGrid
                    strokeDasharray="3 3"
                    className="stroke-border/40"
                    vertical={false}
                  />
                  <XAxis
                    dataKey="name"
                    tickLine={false}
                    axisLine={false}
                    className="text-xs text-muted-foreground"
                  />
                  <YAxis
                    allowDecimals={false}
                    tickLine={false}
                    axisLine={false}
                    className="text-xs text-muted-foreground"
                  />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: "var(--card)",
                      borderRadius: "8px",
                      borderColor: "var(--border)",
                      fontSize: "12px",
                    }}
                  />
                  <Bar
                    dataKey="count"
                    fill="var(--primary)"
                    radius={[6, 6, 0, 0]}
                    maxBarSize={50}
                  />
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>

          {/* Today's Attendance Donut Chart */}
          <Card className="border-border/70">
            <CardHeader className="pb-2">
              <CardTitle className="text-base font-semibold">
                Today's Attendance
              </CardTitle>
              <CardDescription>
                Live distribution by status for today
              </CardDescription>
            </CardHeader>
            <CardContent>
              {summary?.todayAttendanceBreakdown.length ? (
                <ResponsiveContainer width="100%" height={220}>
                  <PieChart>
                    <Pie
                      data={summary.todayAttendanceBreakdown}
                      dataKey="count"
                      nameKey="status"
                      cx="50%"
                      cy="50%"
                      innerRadius={55}
                      outerRadius={80}
                      paddingAngle={4}
                    >
                      {summary.todayAttendanceBreakdown.map((entry) => (
                        <Cell
                          key={entry.status}
                          fill={STATUS_CONFIG[entry.status]?.fill ?? "#94a3b8"}
                          stroke="none"
                        />
                      ))}
                    </Pie>
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "var(--card)",
                        borderRadius: "8px",
                        borderColor: "var(--border)",
                        fontSize: "12px",
                      }}
                    />
                    <Legend
                      verticalAlign="bottom"
                      height={36}
                      iconType="circle"
                      iconSize={8}
                    />
                  </PieChart>
                </ResponsiveContainer>
              ) : (
                <EmptyState
                  message="No attendance marked yet today"
                  submessage="Attendance records will be visualized here as soon as staff marks them."
                />
              )}
            </CardContent>
          </Card>
        </div>

        {/* Recent Submissions List */}
        <Card className="border-border/70">
          <CardHeader className="flex flex-row items-center justify-between pb-3">
            <div>
              <CardTitle className="text-base font-semibold">
                Recent Submissions
              </CardTitle>
              <CardDescription>
                Latest student work delivered across all classes
              </CardDescription>
            </div>
            <Link
              to={Paths.submissions}
              className="text-xs font-medium text-primary hover:underline flex items-center gap-1"
            >
              View all <ArrowUpRight className="h-3 w-3" />
            </Link>
          </CardHeader>
          <CardContent>
            {summary?.recentSubmissions.length ? (
              <div className="divide-y divide-border/60">
                {summary.recentSubmissions.map((s) => (
                  <Link
                    key={s.id}
                    to={getModuleUrls("submissionDetail", { id: s.id })}
                    className="flex items-center justify-between py-3 px-2 -mx-2 rounded-lg hover:bg-muted/50 transition-colors group"
                  >
                    <div className="flex items-center gap-3 min-w-0">
                      <div className="w-8 h-8 rounded-lg bg-primary/10 text-primary flex items-center justify-center shrink-0">
                        <FileText className="h-4 w-4" />
                      </div>
                      <div className="truncate">
                        <p className="text-sm font-medium text-foreground group-hover:text-primary transition-colors truncate">
                          {s.fullName}
                        </p>
                        <p className="text-xs text-muted-foreground truncate">
                          Roll No: {s.rollNo || "—"}
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3 shrink-0">
                      <span className="text-xs px-2 py-0.5 rounded-md bg-secondary text-secondary-foreground font-medium">
                        {s.classRoomName || "Unassigned"}
                      </span>
                      <ArrowUpRight className="h-4 w-4 text-muted-foreground opacity-0 group-hover:opacity-100 transition-opacity" />
                    </div>
                  </Link>
                ))}
              </div>
            ) : (
              <EmptyState
                message="No submissions yet"
                submessage="Student submissions will appear here once received."
              />
            )}
          </CardContent>
        </Card>
      </div>
    );
  }

  // -------------------------------------------------------------
  // STUDENT DASHBOARD
  // -------------------------------------------------------------
  return (
    <div className="max-w-4xl mx-auto space-y-6 pb-12 pt-2">
      {/* Student Greeting */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b pb-5">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">
            {greeting}, {email?.split("@")[0]}
          </h1>
          <p className="text-sm text-muted-foreground mt-0.5 flex items-center gap-1.5">
            <Calendar className="h-3.5 w-3.5" />
            {todayFormatted} · Student Portal
          </p>
        </div>

        <div className="flex items-center gap-2.5">
          <Link to={Paths.submissionCreate}>
            <Button size="sm" className="gap-1.5 shadow-xs">
              <FileText className="h-4 w-4" />
              New Submission
            </Button>
          </Link>
          <Link to={Paths.reportCard}>
            <Button size="sm" variant="outline" className="gap-1.5">
              <Award className="h-4 w-4" />
              Report Card
            </Button>
          </Link>
        </div>
      </div>

      {/* Student Metrics */}
      {isLoadingMine ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Skeleton className="h-28 w-full rounded-xl" />
          <Skeleton className="h-28 w-full rounded-xl" />
        </div>
      ) : (
        mine && (
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <StatCard
              label="My Submissions"
              value={mine.mySubmissionsCount}
              icon={FileText}
              description="Completed assignments"
              iconBg="bg-primary/10"
              iconColor="text-primary"
            />
            <StatCard
              label="Attendance Rate"
              value={`${mine.myAttendancePercentage}%`}
              icon={CalendarCheck}
              description={
                mine.myAttendancePercentage >= 75
                  ? "✓ Satisfactory attendance"
                  : "⚠ Needs improvement"
              }
              iconBg={
                mine.myAttendancePercentage >= 75
                  ? "bg-emerald-500/10"
                  : "bg-amber-500/10"
              }
              iconColor={
                mine.myAttendancePercentage >= 75
                  ? "text-emerald-600 dark:text-emerald-400"
                  : "text-amber-600 dark:text-amber-400"
              }
            />
          </div>
        )
      )}

      {/* Enrolled Classroom Card */}
      <Card className="border-border/70">
        <CardHeader className="pb-3">
          <div className="flex items-center gap-2">
            <div className="p-2 rounded-lg bg-primary/10 text-primary">
              <BookOpen className="h-4 w-4" />
            </div>
            <div>
              <CardTitle className="text-base font-semibold">
                My Class
              </CardTitle>
              <CardDescription>
                Current academic enrollment details
              </CardDescription>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {myClass ? (
            <div className="flex flex-wrap items-center justify-between gap-4 p-4 rounded-xl bg-muted/40 border">
              <div>
                <p className="text-xs uppercase tracking-wider text-muted-foreground font-semibold">
                  Assigned Class
                </p>
                <p className="text-lg font-bold text-foreground mt-0.5">
                  {myClass.classRoomName || "—"}
                </p>
              </div>

              <div className="flex items-center gap-6">
                <div>
                  <p className="text-xs text-muted-foreground">Roll Number</p>
                  <p className="text-sm font-semibold">{myClass.rollNo}</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Enrolled Date</p>
                  <p className="text-sm font-semibold">
                    {new Date(myClass.enrolledAt).toLocaleDateString()}
                  </p>
                </div>
              </div>
            </div>
          ) : (
            <EmptyState
              message="Not enrolled in any class yet"
              submessage="Please contact your school administrator to be assigned to a class."
            />
          )}
        </CardContent>
      </Card>

      {/* Two Column Section: Recent Attendance & Recent Submissions */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
        {/* Attendance Log */}
        <Card className="border-border/70">
          <CardHeader className="pb-3">
            <CardTitle className="text-base font-semibold">
              Recent Attendance
            </CardTitle>
            <CardDescription>Your latest daily records</CardDescription>
          </CardHeader>
          <CardContent>
            {mine?.myRecentAttendance.length ? (
              <div className="divide-y divide-border/60">
                {mine.myRecentAttendance.map((a) => (
                  <div
                    key={a.id}
                    className="flex items-center justify-between py-2.5"
                  >
                    <span className="text-sm font-medium text-foreground">
                      {a.date}
                    </span>
                    <StatusBadge status={a.status} />
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState message="No attendance records recorded yet." />
            )}
          </CardContent>
        </Card>

        {/* My Recent Submissions */}
        <Card className="border-border/70">
          <CardHeader className="flex flex-row items-center justify-between pb-3">
            <div>
              <CardTitle className="text-base font-semibold">
                My Submissions
              </CardTitle>
              <CardDescription>Recent files you turned in</CardDescription>
            </div>
            <Link
              to={Paths.submissions}
              className="text-xs font-medium text-primary hover:underline flex items-center gap-1"
            >
              All <ArrowUpRight className="h-3 w-3" />
            </Link>
          </CardHeader>
          <CardContent>
            {mine?.myRecentSubmissions &&
            mine.myRecentSubmissions.length > 0 ? (
              <div className="divide-y divide-border/60">
                {mine.myRecentSubmissions.map((s) => (
                  <Link
                    key={s.id}
                    to={getModuleUrls("submissionDetail", { id: s.id })}
                    className="flex items-center justify-between py-2.5 px-1.5 -mx-1.5 rounded-lg hover:bg-muted/50 transition-colors group"
                  >
                    <div className="flex items-center gap-2.5 truncate">
                      <FileText className="h-4 w-4 text-primary shrink-0" />
                      <span className="text-sm font-medium text-foreground group-hover:text-primary transition-colors truncate">
                        {s.fullName}
                      </span>
                    </div>
                    <ArrowUpRight className="h-4 w-4 text-muted-foreground opacity-0 group-hover:opacity-100 transition-opacity shrink-0" />
                  </Link>
                ))}
              </div>
            ) : (
              <EmptyState
                message="No submissions yet"
                submessage="Click 'New Submission' above to submit your first work."
              />
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
};

export default DashboardPage;
