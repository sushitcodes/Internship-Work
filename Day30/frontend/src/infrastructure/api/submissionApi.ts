import { createApi } from "@reduxjs/toolkit/query/react";
import { Submission, PagedResult } from "../../domain/entities/Submission";
import { baseQueryWithAuth } from "./baseQueryWithAuth";

// The argument type for the infinite query. It's an OBJECT-shaped queryArg
// (just the search string here) — RTK Query treats each distinct search
// term as its OWN separate accumulating page sequence. Search for "john",
// then clear it, then search "john" again -> the earlier pages are still
// cached, no new network request needed.

export const submissionApi = createApi({
  reducerPath: "submissionApi",

  baseQuery: baseQueryWithAuth,

  tagTypes: ["Submission", "Dashboard"],
  refetchOnMountOrArgChange: 30,

  endpoints: (builder) => ({
    // 3 generics, in order:
    //   1. PagedResult<Submission> -> shape of ONE page's data
    //   2. string | undefined      -> the queryArg (search term)
    //   3. number                  -> the pageParam RTK Query tracks internally
    getSubmissions: builder.infiniteQuery<
      PagedResult<Submission>,
      { search?: string; pageSize: number },
      number
    >({
      infiniteQueryOptions: {
        initialPageParam: 1,
        // After each fetch, RTK Query calls this to decide "does a next
        // page exist, and what number is it?" We just read the flag our
        // OWN backend already computed (hasNextPage), instead of
        // re-deriving pagination logic on the frontend too.
        getNextPageParam: (lastPage) =>
          lastPage.hasNextPage ? lastPage.page + 1 : undefined,
      },
      // queryArg = search term (from the hook call), pageParam = which
      // page number RTK Query wants fetched next.
      query: ({ queryArg, pageParam }) => {
        const params = new URLSearchParams({
          page: String(pageParam),
          pageSize: String(queryArg.pageSize),
        });
        if (queryArg.search) params.set("search", queryArg.search);
        return `/submissions?${params.toString()}`;
      },
      providesTags: ["Submission"],
    }),

    getSubmissionsCount: builder.query<number, void>({
      query: () => "/submissions/count",
      providesTags: ["Submission"],
    }),

    getSubmissionById: builder.query<Submission, string>({
      query: (id) => `/submissions/${id}`,
      providesTags: ["Submission"],
    }),

    submitForm: builder.mutation<Submission, FormData>({
      query: (formData) => ({
        url: "/submissions",
        method: "POST",
        body: formData,
      }),
      invalidatesTags: ["Submission", "Dashboard"],
    }),

    // New: for the Edit feature. Takes both the id (which submission to
    // update) and the FormData (the new values) — bundled into one object
    // since RTK Query mutations only accept a single argument.
    updateSubmission: builder.mutation<
      Submission,
      { id: string; formData: FormData }
    >({
      query: ({ id, formData }) => ({
        url: `/submissions/${id}`,
        method: "PUT",
        body: formData,
      }),
      // Same handshake as submitForm — after a successful edit, any
      // component showing this submission (list, detail page) auto-refreshes.
      invalidatesTags: ["Submission", "Dashboard"],
    }),

    deleteSubmission: builder.mutation<null, string>({
      query: (id) => ({
        url: `/submissions/${id}`,
        method: "DELETE",
      }),
      invalidatesTags: ["Submission", "Dashboard"],
      transformResponse: (response: any) => {
        return response || null;
      },
    }),
  }),
});

// Auto-generated hook names, one per endpoint above. useGetInfiniteSubmissionsQuery
// is the "composed" hook — it bundles subscription (triggering fetches,
// exposing fetchNextPage) and state-reading (data, isLoading) in one call,
// the same pattern every other useXQuery hook here already follows.
export const {
  useGetSubmissionsInfiniteQuery,
  useGetSubmissionsCountQuery,
  useGetSubmissionByIdQuery,
  useSubmitFormMutation,
  useUpdateSubmissionMutation,
  useDeleteSubmissionMutation,
} = submissionApi;
