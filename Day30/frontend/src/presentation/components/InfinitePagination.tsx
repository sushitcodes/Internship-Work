import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/components/ui/pagination";

interface InfinitePaginationProps {
  pageCount: number;
  pageIndex: number;
  onPageIndexChange: (index: number) => void;
  onNext: () => void;
  onPrev: () => void;
  canGoNext: boolean;
  canGoPrev: boolean;
  isFetchingNextPage?: boolean;
  className?: string;
}

export function InfinitePagination({
  pageCount,
  pageIndex,
  onPageIndexChange,
  onNext,
  onPrev,
  canGoNext,
  canGoPrev,
  isFetchingNextPage,
  className = "mt-4",
}: InfinitePaginationProps) {
  return (
    <Pagination className={className}>
      <PaginationContent>
        <PaginationItem>
          <PaginationPrevious
            onClick={onPrev}
            className={
              !canGoPrev ? "pointer-events-none opacity-50" : "cursor-pointer"
            }
          />
        </PaginationItem>
        {Array.from({ length: pageCount }, (_, i) => (
          <PaginationItem key={i}>
            <PaginationLink
              isActive={i === pageIndex}
              onClick={() => onPageIndexChange(i)}
            >
              {i + 1}
            </PaginationLink>
          </PaginationItem>
        ))}
        <PaginationItem>
          <PaginationNext
            onClick={onNext}
            className={
              !canGoNext || isFetchingNextPage
                ? "pointer-events-none opacity-50"
                : "cursor-pointer"
            }
          />
        </PaginationItem>
      </PaginationContent>
    </Pagination>
  );
}
