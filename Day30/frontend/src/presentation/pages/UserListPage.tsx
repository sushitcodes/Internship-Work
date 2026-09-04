import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { useSearchUsersInfiniteQuery } from "../../infrastructure/api/userApi";
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
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { getInitials } from "../../application/utils/getInitials";
import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";
import { useAppSelector } from "@/infrastructure/store/hooks";
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";
import { displayRoleName } from "../../lib/roleDisplay";

const ROLE_OPTIONS = ["Student", "Staff", "Admin"] as const;

const UsersListPage: React.FC = () => {
  const roles = useAppSelector((state) => state.auth.roles);

  // --- Filter inputs (raw, as typed) ---
  const [searchInput, setSearchInput] = useState("");
  const [rollNoInput, setRollNoInput] = useState("");
  const [roleFilter, setRoleFilter] = useState<string | undefined>();

  // --- Debounced versions (what actually gets sent to the API) ---
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [debouncedRollNo, setDebouncedRollNo] = useState<number | undefined>();

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearch(searchInput), 400);
    return () => clearTimeout(timer);
  }, [searchInput]);

  useEffect(() => {
    const timer = setTimeout(() => {
      const parsed = parseInt(rollNoInput);
      setDebouncedRollNo(isNaN(parsed) ? undefined : parsed);
    }, 400);
    return () => clearTimeout(timer);
  }, [rollNoInput]);

  const [pageIndex, setPageIndex] = useState(0);
  // Reset to page 0 whenever ANY filter changes — now safe, since all
  // three debounced values are declared above this point.
  useEffect(() => {
    setPageIndex(0);
  }, [debouncedSearch, debouncedRollNo, roleFilter]);

  // ONE call, not two — this was duplicated before.
  const { data, fetchNextPage, isLoading, isError, isFetchingNextPage } =
    useSearchUsersInfiniteQuery({
      search: debouncedSearch,
      rollNo: debouncedRollNo,
      role: roleFilter,
      pageSize: 10,
    });

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
    <Card className="max-w-3xl mx-auto mt-10">
      <CardHeader className="border-b">
        <div className="flex justify-between items-center mb-4">
          <CardTitle className="text-2xl font-bold">Users</CardTitle>
          {roles.includes("Admin") && (
            <Link to="/users/create">
              <Button className="gap-2">
                <Plus className="h-4 w-4" />
                Add User
              </Button>
            </Link>
          )}
        </div>

        {/* ONE filter row — the duplicate search box from CardContent is gone. */}
        <div className="flex gap-3">
          <Input
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            placeholder="Search by name or email..."
            className="flex-1"
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

      <CardContent className="pt-6">
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
              <TableHeader>
                <TableRow>
                  <TableHead>Member #</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead></TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {users.map((u) => (
                  <TableRow key={u.userId}>
                    <TableCell className="text-muted-foreground">
                      {u.memberNumber > 0 ? `#${u.memberNumber}` : "—"}
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-3">
                        <Avatar className="h-8 w-8">
                          <AvatarImage src={u.avatarUrl} alt={u.fullName} />
                          <AvatarFallback className="text-xs">
                            {getInitials(u.fullName)}
                          </AvatarFallback>
                        </Avatar>
                        <span className="font-medium">{u.fullName}</span>
                      </div>
                    </TableCell>
                    <TableCell className="text-gray-600">{u.email}</TableCell>
                    <TableCell>
                      <Link to={`/users/${u.userId}`}>
                        <span className="text-sm underline">View</span>
                      </Link>
                    </TableCell>
                  </TableRow>
                ))}
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
  );
};

export default UsersListPage;
