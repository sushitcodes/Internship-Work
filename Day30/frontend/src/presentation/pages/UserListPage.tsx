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

// Same pageIndex/debounced-search/infinite-query wiring as SubmissionsListPage —
// deliberately kept identical rather than inventing a new pagination approach.
const UsersListPage: React.FC = () => {
  const [searchInput, setSearchInput] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearch(searchInput), 400);
    return () => clearTimeout(timer);
  }, [searchInput]);

  const [pageIndex, setPageIndex] = useState(0);
  useEffect(() => setPageIndex(0), [debouncedSearch]);

  const { data, fetchNextPage, isLoading, isError, isFetchingNextPage } =
    useSearchUsersInfiniteQuery({ search: debouncedSearch, pageSize: 10 });

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
        <CardTitle className="text-2xl font-bold">Users</CardTitle>
      </CardHeader>
      <CardContent className="pt-6">
        <Input
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          placeholder="Search by name or email..."
          className="mb-6"
        />

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
                      #{u.memberNumber}
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
