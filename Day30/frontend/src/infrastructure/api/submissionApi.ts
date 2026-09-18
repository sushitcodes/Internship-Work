import { api } from "./api";
import { Submission, PagedResult } from "../../domain/entities/Submission";

export const submissionApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getSubmissions: builder.infiniteQuery<
      PagedResult<Submission>,
      { search?: string; pageSize: number },
      number
    >({
      infiniteQueryOptions: {
        initialPageParam: 1,
        getNextPageParam: (lastPage) =>
          lastPage.hasNextPage ? lastPage.page + 1 : undefined,
      },
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
      // Now that Dashboard lives in the SAME tag registry as dashboardApi's
      // queries, this line finally does what it always looked like it did.
      invalidatesTags: ["Submission", "Dashboard"],
    }),

    updateSubmission: builder.mutation<
      Submission,
      { id: string; formData: FormData }
    >({
      query: ({ id, formData }) => ({
        url: `/submissions/${id}`,
        method: "PUT",
        body: formData,
      }),
      invalidatesTags: ["Submission", "Dashboard"],
    }),

    deleteSubmission: builder.mutation<null, string>({
      query: (id) => ({ url: `/submissions/${id}`, method: "DELETE" }),
      invalidatesTags: ["Submission", "Dashboard"],
      transformResponse: (response: any) => response || null,
    }),
  }),
});

// Unchanged — every page importing these hooks from "./submissionApi"
// needs zero changes. injectEndpoints returns the same api object with
// these hooks merged in; the import path and hook names don't move.
export const {
  useGetSubmissionsInfiniteQuery,
  useGetSubmissionsCountQuery,
  useGetSubmissionByIdQuery,
  useSubmitFormMutation,
  useUpdateSubmissionMutation,
  useDeleteSubmissionMutation,
} = submissionApi;
