import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import {
  useSearchUsersInfiniteQuery,
  useUpdateUserNameMutation,
  useSetUserActiveStatusMutation,
} from "../../infrastructure/api/userApi";
import {
  Table,
  TableHeader,
  TableBody,
  TableHead,
  TableRow,
  TableCell,
} from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/components/ui/pagination";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { getInitials } from "../../application/utils/getInitials";
import { resolveFileUrl } from "../../lib/resolveFileUrl";
import { Button } from "@/components/ui/button";
import { Plus, Check, X, Pencil } from "lucide-react";
import { useAppSelector } from "@/infrastructure/store/hooks";
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { Badge } from "@/components/ui/badge";
import { displayRoleName } from "../../lib/roleDisplay";
import { toast } from "sonner";
import { Paths } from "@/routes/paths";
import { PageHeader } from "../components/PageHeader";
import { useDebounce } from "../hooks/useDebounce";
const ROLE_OPTIONS = ["Student", "Staff", "Admin"] as const;

function UsersListPage() {
  const roles = useAppSelector((state) => state.auth.roles);
  const [searchInput, setSearchInput] = useState("");
  const [rollNoInput, setRollNoInput] = useState("");
  const [roleFilter, setRoleFilter] = useState<string | undefined>();
  const debouncedSearch = useDebounce(searchInput, 400);
  const debouncedRollRaw = useDebounce(rollNoInput, 400);
  const debouncedRollNo = debouncedRollRaw
    ? parseInt(debouncedRollRaw, 10)
    : undefined;
  const [pageIndex, setPageIndex] = useState(0);
  useEffect(
    () => setPageIndex(0),
    [debouncedSearch, debouncedRollNo, roleFilter],
  );

  const { data, fetchNextPage, isLoading, isError, isFetchingNextPage } =
    useSearchUsersInfiniteQuery({
      search: debouncedSearch,
      rollNo: debouncedRollNo,
      role: roleFilter,
      pageSize: 10,
    });

  const [updateName, { isLoading: isSavingName }] = useUpdateUserNameMutation();
  const [setActiveStatus] = useSetUserActiveStatusMutation();

  // Inline-edit state — which row (by userId) is currently being edited,
  // and the in-progress value. Only ONE row editable at a time by design;
  // starting a new edit implicitly cancels any other in-flight one.
  const [editingUserId, setEditingUserId] = useState<string | null>(null);
  const [editValue, setEditValue] = useState("");

  const startEdit = (userId: string, currentName: string) => {
    setEditingUserId(userId);
    setEditValue(currentName);
  };
  const cancelEdit = () => {
    setEditingUserId(null);
    setEditValue("");
  };
  const saveEdit = async (userId: string) => {
    if (!editValue.trim()) {
      toast.error("Name cannot be empty.");
      return;
    }
    try {
      await updateName({ id: userId, fullName: editValue.trim() }).unwrap();
      toast.success("Name updated.");
      setEditingUserId(null);
    } catch (err) {
      console.error("Failed to update name:", err);
      toast.error("Could not update name.");
    }
  };

  const handleToggleActive = async (
    userId: string,
    currentlyActive: boolean,
  ) => {
    try {
      await setActiveStatus({
        id: userId,
        isActive: !currentlyActive,
      }).unwrap();
      toast.success(
        currentlyActive ? "User deactivated." : "User reactivated.",
      );
    } catch (err) {
      console.error("Failed to update status:", err);
      toast.error("Could not update status.");
    }
  };

  const pages = data?.pages ?? [];
  const currentPage = pages[pageIndex];
  const users = currentPage?.items ?? [];

  const handleNext = async () => {
    if (pageIndex < pages.length - 1) {
      setPageIndex((i) => i + 1);
      return;
    }
    if (currentPage?.hasNextPage) {
      await fetchNextPage();
      setPageIndex((i) => i + 1);
    }
  };
  const handlePrev = () => {
    if (pageIndex > 0) setPageIndex((i) => i - 1);
  };
  const canGoNext =
    pageIndex < pages.length - 1 || Boolean(currentPage?.hasNextPage);
  const canGoPrev = pageIndex > 0;

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      {/* 1. Page Header with Title & Action Button */}
      <PageHeader
        title="Users Management"
        description="View, search, and manage student, staff, and admin accounts."
      >
        {roles.includes("Admin") && (
          <Link to={Paths.userCreate}>
            <Button size="sm" className="gap-2 shadow-xs">
              <Plus className="h-4 w-4" />
              Add User
            </Button>
          </Link>
        )}
      </PageHeader>

      {/* 2. Card with Filters & Table */}
      <Card className="border-border/70 overflow-hidden shadow-xs">
        <CardHeader className="border-b bg-muted/20 p-4">
          <div className="flex flex-wrap gap-3">
            <Input
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              placeholder="Search by name or email..."
              className="flex-1 min-w-55"
            />
            <Input
              type="number"
              value={rollNoInput}
              onChange={(e) => setRollNoInput(e.target.value)}
              placeholder="Roll No"
              className="w-28"
            />
            <Select
              value={roleFilter ?? "all"}
              onValueChange={(v) =>
                setRoleFilter(v === "all" || v == null ? undefined : v)
              }
            >
              <SelectTrigger className="w-36">
                <SelectValue placeholder="All Roles" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Roles</SelectItem>
                {ROLE_OPTIONS.map((r) => (
                  <SelectItem key={r} value={r}>
                    {displayRoleName(r)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </CardHeader>

        <CardContent className="p-0">
          {isLoading && (
            <div className="space-y-3">
              {[...Array(5)].map((_, i) => (
                <Skeleton key={i} className="h-12 w-full" />
              ))}
            </div>
          )}
          {!isLoading && isError && (
            <p className="text-red-500 text-center">Could not load users.</p>
          )}

          {users.length > 0 && (
            <div className="rounded-md border">
              <Table>
                <TableHeader className="bg-muted/40">
                  <TableRow>
                    <TableHead>Member #</TableHead>
                    <TableHead>Name</TableHead>
                    <TableHead>Email</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {users.map((u) => {
                    const isEditing = editingUserId === u.userId;
                    return (
                      <TableRow
                        key={u.userId}
                        className={!u.isActive ? "opacity-60" : ""}
                      >
                        <TableCell className="text-muted-foreground">
                          {u.memberNumber > 0 ? `#${u.memberNumber}` : "—"}
                        </TableCell>

                        <TableCell>
                          {isEditing ? (
                            <div className="flex items-center gap-2">
                              <Input
                                value={editValue}
                                onChange={(e) => setEditValue(e.target.value)}
                                className="h-8 w-40"
                                autoFocus
                              />
                              <Button
                                size="icon"
                                variant="ghost"
                                className="h-8 w-8"
                                onClick={() => saveEdit(u.userId)}
                                disabled={isSavingName}
                              >
                                <Check className="h-4 w-4 text-green-600" />
                              </Button>
                              <Button
                                size="icon"
                                variant="ghost"
                                className="h-8 w-8"
                                onClick={cancelEdit}
                              >
                                <X className="h-4 w-4 text-red-500" />
                              </Button>
                            </div>
                          ) : (
                            <div className="flex items-center gap-3">
                              <Avatar className="h-8 w-8">
                                <AvatarImage
                                  src={resolveFileUrl(u.avatarUrl)}
                                  alt={u.fullName}
                                />
                                <AvatarFallback className="text-xs">
                                  {getInitials(u.fullName)}
                                </AvatarFallback>
                              </Avatar>
                              <span className="font-medium">{u.fullName}</span>
                            </div>
                          )}
                        </TableCell>

                        <TableCell className="text-gray-600">
                          {u.email}
                        </TableCell>

                        <TableCell>
                          <Badge variant={u.isActive ? "default" : "secondary"}>
                            {u.isActive ? "Active" : "Deactivated"}
                          </Badge>
                        </TableCell>

                        <TableCell>
                          <div className="flex items-center gap-2">
                            <Link to={`${Paths.users}/${u.userId}`}>
                              <Button variant="outline" size="sm">
                                View
                              </Button>
                            </Link>

                            {roles.includes("Admin") && !isEditing && (
                              <Button
                                variant="outline"
                                size="sm"
                                onClick={() => startEdit(u.userId, u.fullName)}
                              >
                                <Pencil className="h-3 w-3" />
                              </Button>
                            )}

                            {roles.includes("Admin") && (
                              <AlertDialog>
                                <AlertDialogTrigger
                                  render={
                                    <Button
                                      variant="outline"
                                      size="sm"
                                      className={
                                        u.isActive
                                          ? "text-red-500 hover:text-red-700 hover:bg-red-50"
                                          : "text-green-600 hover:text-green-700 hover:bg-green-50"
                                      }
                                    >
                                      {u.isActive ? "Deactivate" : "Activate"}
                                    </Button>
                                  }
                                />
                                <AlertDialogContent>
                                  <AlertDialogHeader>
                                    <AlertDialogTitle>
                                      {u.isActive
                                        ? "Deactivate this user?"
                                        : "Reactivate this user?"}
                                    </AlertDialogTitle>
                                    <AlertDialogDescription>
                                      {u.isActive ? (
                                        <>
                                          <span className="font-semibold">
                                            {u.fullName}
                                          </span>{" "}
                                          will be blocked from logging in and
                                          signed out of any active session.
                                          Their submissions and attendance
                                          history stay intact and can be
                                          restored anytime.
                                        </>
                                      ) : (
                                        <>
                                          <span className="font-semibold">
                                            {u.fullName}
                                          </span>{" "}
                                          will be able to log in again.
                                        </>
                                      )}
                                    </AlertDialogDescription>
                                  </AlertDialogHeader>
                                  <AlertDialogFooter>
                                    <AlertDialogCancel>
                                      Cancel
                                    </AlertDialogCancel>
                                    <AlertDialogAction
                                      onClick={() =>
                                        handleToggleActive(u.userId, u.isActive)
                                      }
                                      className={
                                        u.isActive
                                          ? "bg-red-500 hover:bg-red-600"
                                          : ""
                                      }
                                    >
                                      {u.isActive ? "Deactivate" : "Activate"}
                                    </AlertDialogAction>
                                  </AlertDialogFooter>
                                </AlertDialogContent>
                              </AlertDialog>
                            )}
                          </div>
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </div>
          )}

          <Pagination className="mt-4">
            <PaginationContent>
              <PaginationItem>
                <PaginationPrevious
                  onClick={handlePrev}
                  className={
                    !canGoPrev
                      ? "pointer-events-none opacity-50"
                      : "cursor-pointer"
                  }
                />
              </PaginationItem>
              {pages.map((_, i) => (
                <PaginationItem key={i}>
                  <PaginationLink
                    isActive={i === pageIndex}
                    onClick={() => setPageIndex(i)}
                  >
                    {i + 1}
                  </PaginationLink>
                </PaginationItem>
              ))}
              <PaginationItem>
                <PaginationNext
                  onClick={handleNext}
                  className={
                    !canGoNext || isFetchingNextPage
                      ? "pointer-events-none opacity-50"
                      : "cursor-pointer"
                  }
                />
              </PaginationItem>
            </PaginationContent>
          </Pagination>
        </CardContent>
      </Card>
    </div>
  );
}

export default UsersListPage;
