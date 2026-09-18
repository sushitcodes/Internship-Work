import { useState, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  useGetSubmissionsInfiniteQuery,
  useDeleteSubmissionMutation,
} from "../../infrastructure/api/submissionApi";
import SubmissionCountBadge from "../components/SubmissionCountBadge";
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
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
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
import { Plus } from "lucide-react";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { getInitials } from "../../application/utils/getInitials";
import { useAppSelector } from "../../infrastructure/store/hooks";
import {
  HIDE_ACTIONS_WHEN_LOGGED_OUT,
  canEdit,
  canDelete,
} from "../config/authUiConfig";
import { resolveFileUrl } from "@/lib/resolveFileUrl";
import { Paths } from "@/routes/paths";
import { getModuleUrls } from "@/routes/getModuleUrls";

const SubmissionsListPage: React.FC = () => {
  const [searchInput, setSearchInput] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearch(searchInput), 400);
    return () => clearTimeout(timer);
  }, [searchInput]);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [deleteName, setDeleteName] = useState<string>("");
  const [pageIndex, setPageIndex] = useState(0);

  useEffect(() => {
    setPageIndex(0);
  }, [debouncedSearch]);
  const { data, fetchNextPage, isLoading, isError, isFetchingNextPage } =
    useGetSubmissionsInfiniteQuery({ search: debouncedSearch, pageSize: 10 });
  const [deleteSubmission, { isLoading: isDeleting }] =
    useDeleteSubmissionMutation();
  const roles = useAppSelector((state) => state.auth.roles);
  const pendingUploads = useAppSelector((state) =>
    Object.values(state.uploadProgress.byId),
  );
  const isLoggedIn = Boolean(useAppSelector((state) => state.auth.email));
  const navigate = useNavigate();
  const pages = data?.pages ?? [];
  const currentPage = pages[pageIndex];
  const submissions = currentPage?.items ?? [];
  // const submissions = pages.flatMap((page) => page.items);

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
    // Always safe with zero network cost: reaching pageIndex N always
    // means pages 0..N-1 were already fetched to get here.
    if (pageIndex > 0) setPageIndex((i) => i - 1);
  };

  const canGoNext =
    pageIndex < pages.length - 1 || Boolean(currentPage?.hasNextPage);
  const canGoPrev = pageIndex > 0;

  const handleDelete = async () => {
    if (!deleteId) return;
    try {
      await deleteSubmission(deleteId).unwrap();
      setDeleteId(null);
      setDeleteName("");
    } catch (err) {
      console.error("Failed to delete submission:", err);
      alert("Could not delete this submission. Please try again.");
    }
  };

  return (
    <Card className="max-w-3xl mx-auto mt-10">
      <CardHeader className="border-b">
        <div className="flex justify-between items-center">
          <CardTitle className="text-2xl font-bold">Student</CardTitle>
          <div className="flex items-center gap-4">
            <SubmissionCountBadge />

            {(isLoggedIn || !HIDE_ACTIONS_WHEN_LOGGED_OUT) &&
              (isLoggedIn ? (
                <Link to={Paths.submissionCreate}>
                  <Button className="gap-2">
                    <Plus className="h-4 w-4" />
                    New Submission
                  </Button>
                </Link>
              ) : (
                <Button className="gap-2" onClick={() => navigate(Paths.login)}>
                  <Plus className="h-4 w-4" />
                  New Submission
                </Button>
              ))}
          </div>
        </div>
      </CardHeader>
      <CardContent className="pt-6">
        <Input
          type="text"
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          placeholder="Search by name or email..."
          className="mb-6 placeholder:text-gray-350 placeholder:opacity-40"
        />
        {isLoading && (
          <div className="space-y-3">
            <div className="flex items-center space-x-4">
              <Skeleton className="h-12 w-full" />
            </div>
            <div className="flex items-center space-x-4">
              <Skeleton className="h-12 w-full" />
            </div>
            <div className="flex items-center space-x-4">
              <Skeleton className="h-12 w-full" />
            </div>
            <div className="flex items-center space-x-4">
              <Skeleton className="h-12 w-full" />
            </div>
            <div className="flex items-center space-x-4">
              <Skeleton className="h-12 w-full" />
            </div>
          </div>
        )}
        {!isLoading && isError && (
          <p className="text-red-500 text-center">
            Could not load submissions.
          </p>
        )}

        {(submissions.length > 0 || pendingUploads.length > 0) && (
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="py-2 pr-4">Name</TableHead>
                  <TableHead className="py-2"></TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {/* Pending uploads render FIRST, above real rows — this
                    is the actual feature: a live row that fills in as
                    the file goes out over the wire, then disappears
                    once invalidateTags brings in the real saved row. */}
                {pendingUploads.map((u) => (
                  <TableRow key={u.id} className="bg-muted/40">
                    <TableCell className="py-3 pr-4">
                      <div className="flex flex-col gap-1">
                        <span className="font-medium text-muted-foreground">
                          {u.fileName}
                        </span>
                        {u.status === "uploading" && (
                          <div className="h-1.5 w-40 rounded-full bg-gray-200 overflow-hidden">
                            <div
                              className="h-full bg-blue-500 transition-all duration-150"
                              style={{ width: `${u.progress}%` }}
                            />
                          </div>
                        )}
                        {u.status === "error" && (
                          <span className="text-xs text-red-500">
                            {u.errorMessage}
                          </span>
                        )}
                      </div>
                    </TableCell>
                    <TableCell className="py-3 text-xs text-muted-foreground">
                      {u.status === "uploading" ? `${u.progress}%` : "Failed"}
                    </TableCell>
                  </TableRow>
                ))}
                {/* // ...unchanged — everything below this stays exactly as it was */}
                {submissions.map((s) => (
                  <TableRow key={s.id}>
                    <TableCell className="py-3 pr-4">
                      <div className="flex items-center gap-3">
                        <Avatar className="h-8 w-8">
                          <AvatarImage
                            src={
                              resolveFileUrl(s.submitterAvatarUrl) ?? undefined
                            }
                            alt={s.fullName}
                          />
                          <AvatarFallback className="bg-blue-100 text-blue-800 text-xs">
                            {getInitials(s.fullName)}
                          </AvatarFallback>
                        </Avatar>
                        <span className="font-medium">{s.fullName}</span>
                      </div>
                    </TableCell>

                    <TableCell className="py-3">
                      <div className="flex items-center gap-2">
                        <Link
                          to={getModuleUrls("submissionDetail", { id: s.id })}
                        >
                          <Button variant="outline" size="sm">
                            View
                          </Button>
                        </Link>
                        {(canEdit(roles) || !HIDE_ACTIONS_WHEN_LOGGED_OUT) &&
                          (canEdit(roles) ? (
                            <Link
                              to={getModuleUrls("submissionEdit", { id: s.id })}
                            >
                              <Button variant="outline" size="sm">
                                Edit
                              </Button>
                            </Link>
                          ) : (
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => navigate(Paths.login)}
                            >
                              Edit
                            </Button>
                          ))}

                        {(canDelete(roles) || !HIDE_ACTIONS_WHEN_LOGGED_OUT) &&
                          (canDelete(roles) ? (
                            <AlertDialog>
                              <AlertDialogTrigger
                                render={
                                  <Button
                                    variant="outline"
                                    size="sm"
                                    className="text-red-500 hover:text-red-700 hover:bg-red-50"
                                    onClick={() => {
                                      setDeleteId(s.id);
                                      setDeleteName(s.fullName);
                                    }}
                                  >
                                    Delete
                                  </Button>
                                }
                              />
                              <AlertDialogContent>
                                <AlertDialogHeader>
                                  <AlertDialogTitle>
                                    Are you sure?
                                  </AlertDialogTitle>
                                  <AlertDialogDescription>
                                    This will permanently delete the submission
                                    from{" "}
                                    <span className="font-semibold">
                                      {deleteName}
                                    </span>
                                    . This action cannot be undone.
                                  </AlertDialogDescription>
                                </AlertDialogHeader>
                                <AlertDialogFooter>
                                  <AlertDialogCancel
                                    onClick={() => {
                                      setDeleteId(null);
                                      setDeleteName("");
                                    }}
                                  >
                                    Cancel
                                  </AlertDialogCancel>
                                  <AlertDialogAction
                                    onClick={handleDelete}
                                    disabled={isDeleting}
                                    className="bg-red-500 hover:bg-red-600"
                                  >
                                    {isDeleting ? "Deleting..." : "Delete"}
                                  </AlertDialogAction>
                                </AlertDialogFooter>
                              </AlertDialogContent>
                            </AlertDialog>
                          ) : (
                            <Button
                              variant="outline"
                              size="sm"
                              className="text-red-500 hover:text-red-700 hover:bg-red-50 border-red-200 hover:border-red-300"
                              onClick={() => navigate("/login")}
                            >
                              Delete
                            </Button>
                          ))}
                      </div>
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

            {Array.from({ length: pages.length }, (_, i) => (
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

export default SubmissionsListPage;
