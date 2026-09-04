import { Link } from "react-router-dom";

import { useAppSelector } from "../../infrastructure/store/hooks";

import {
  useGetDashboardSummaryQuery,
  useGetMyDashboardQuery,
} from "../../infrastructure/api/dashboardApi";

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

const DashboardPage: React.FC = () => {
  const roles = useAppSelector((state) => state.auth.roles);

  const isStaffOrAdmin = roles.includes("Staff") || roles.includes("Admin");

  // Only ONE of these two queries actually runs, based on role — skip

  // avoids firing a request that would 403 anyway (Student hitting

  // /dashboard/summary) or wastefully double-fetching.

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
                value={`${summary.todayAttendancePercentage}%`}
              />
            </div>
          )
        )}

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

                  <span className="text-muted-foreground">{s.email}</span>
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
