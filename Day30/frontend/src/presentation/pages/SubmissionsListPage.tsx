import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  useGetSubmissionsInfiniteQuery,
  useDeleteSubmissionMutation,
} from "@/infrastructure/api/submissionApi";
import {
  Table,
  TableHeader,
  TableBody,
  TableHead,
  TableRow,
  TableCell,
} from "@/components/ui/table";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";
import { useAppSelector } from "@/infrastructure/store/hooks";
import {
  HIDE_ACTIONS_WHEN_LOGGED_OUT,
  canEdit,
  canDelete,
} from "@/presentation/config/authUiConfig";
import { extractErrorMessage } from "@/lib/apiError";
import { Paths } from "@/routes/paths";
import { getModuleUrls } from "@/routes/getModuleUrls";
import { PageHeader } from "@/presentation/components/PageHeader";
import { UserAvatar } from "@/presentation/components/UserAvatar";
import { TableSkeleton } from "@/presentation/components/TableSkeleton";
import { ConfirmDialog } from "@/presentation/components/ConfirmDialog";
import { InfinitePagination } from "@/presentation/components/InfinitePagination";
import { SearchInput } from "@/presentation/components/SearchInput";
import { useDebounce } from "@/presentation/hooks/useDebounce";
import { useInfiniteCachedPages } from "@/presentation/hooks/useInfiniteCachedPages";
import { toast } from "sonner";

const SubmissionsListPage: React.FC = () => {
  const [searchInput, setSearchInput] = useState("");
  const debouncedSearch = useDebounce(searchInput, 400);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [deleteName, setDeleteName] = useState<string>("");

  const { data, fetchNextPage, isLoading, isError, isFetchingNextPage } =
    useGetSubmissionsInfiniteQuery({ search: debouncedSearch, pageSize: 10 });
  const [deleteSubmission, { isLoading: isDeleting }] =
    useDeleteSubmissionMutation();
  const roles = useAppSelector((state) => state.auth.roles);
  const pendingUploads = useAppSelector((state) =>
    Object.values(state.uploadProgress.byId),
  );
  const navigate = useNavigate();
  const pages = data?.pages ?? [];
  const {
    pageIndex,
    setPageIndex,
    currentPage,
    handleNext,
    handlePrev,
    canGoNext,
    canGoPrev,
  } = useInfiniteCachedPages(pages, fetchNextPage, debouncedSearch);
  const submissions = currentPage?.items ?? [];

  const handleDelete = async () => {
    if (!deleteId) return;
    try {
      await deleteSubmission(deleteId).unwrap();
      setDeleteId(null);
      setDeleteName("");
    } catch (err) {
      console.error("Failed to delete submission:", err);
      toast.error(
        extractErrorMessage(err, "Could not delete this submission. Please try again."),
      );
    }
  };

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      <PageHeader
        title="Student Submissions"
        description="Browse and review submitted assignments across all classes."
      >
        <Link to={Paths.submissionCreate}>
          <Button size="sm" className="gap-1.5 shadow-xs">
            <Plus className="h-4 w-4" />
            New Submission
          </Button>
        </Link>
      </PageHeader>

      <Card className="border-border/70 overflow-hidden shadow-xs">
        <CardContent className="pt-6">
          <SearchInput
            value={searchInput}
            onChange={setSearchInput}
            placeholder="Search by name or email..."
            className="mb-6 placeholder:text-gray-350 placeholder:opacity-40"
          />
          {isLoading && <TableSkeleton />}
          {!isLoading && isError && (
            <p className="text-red-500 text-center">
              Could not load submissions.
            </p>
          )}

          {(submissions.length > 0 || pendingUploads.length > 0) && (
            <div className="rounded-md border">
              <Table>
                <TableHeader className="bg-muted/40">
                  <TableRow>
                    <TableHead className="py-2 pr-4">Name</TableHead>
                    <TableHead className="py-2"></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
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
                  {submissions.map((s) => (
                    <TableRow key={s.id}>
                      <TableCell className="py-3 pr-4">
                        <div className="flex items-center gap-3">
                          <UserAvatar
                            name={s.fullName}
                            src={s.submitterAvatarUrl}
                            fallbackClassName="bg-blue-100 text-blue-800"
                          />
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
                                to={getModuleUrls("submissionEdit", {
                                  id: s.id,
                                })}
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

                          {(canDelete(roles) ||
                            !HIDE_ACTIONS_WHEN_LOGGED_OUT) &&
                            (canDelete(roles) ? (
                              <ConfirmDialog
                                trigger={
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
                                title="Are you sure?"
                                description={
                                  <>
                                    This will permanently delete the submission
                                    from{" "}
                                    <span className="font-semibold">
                                      {deleteName}
                                    </span>
                                    . This action cannot be undone.
                                  </>
                                }
                                confirmLabel={
                                  isDeleting ? "Deleting..." : "Delete"
                                }
                                confirmDisabled={isDeleting}
                                confirmClassName="bg-red-500 hover:bg-red-600"
                                onConfirm={handleDelete}
                                onCancel={() => {
                                  setDeleteId(null);
                                  setDeleteName("");
                                }}
                              />
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
          <InfinitePagination
            pageCount={pages.length}
            pageIndex={pageIndex}
            onPageIndexChange={setPageIndex}
            onNext={handleNext}
            onPrev={handlePrev}
            canGoNext={canGoNext}
            canGoPrev={canGoPrev}
            isFetchingNextPage={isFetchingNextPage}
          />
        </CardContent>
      </Card>
    </div>
  );
};

export default SubmissionsListPage;
