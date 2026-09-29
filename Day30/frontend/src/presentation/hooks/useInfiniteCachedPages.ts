import { useEffect, useState } from "react";

type PageLike = { hasNextPage?: boolean };

/**
 * Cached infinite-query paging: keep already-fetched pages and only
 * call fetchNextPage when the user walks past the last loaded page.
 */
export function useInfiniteCachedPages<TPage extends PageLike>(
  pages: TPage[],
  fetchNextPage: () => Promise<unknown>,
  resetKey: string,
) {
  const [pageIndex, setPageIndex] = useState(0);

  useEffect(() => {
    setPageIndex(0);
  }, [resetKey]);

  const currentPage = pages[pageIndex];

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

  return {
    pageIndex,
    setPageIndex,
    currentPage,
    handleNext,
    handlePrev,
    canGoNext,
    canGoPrev,
  };
}
