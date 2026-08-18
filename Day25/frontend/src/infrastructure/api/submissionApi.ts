import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { Submission } from "../../domain/entities/Submission";

// createApi is RTK Query's core function. You describe WHAT endpoints exist
export const submissionApi = createApi({
  reducerPath: "submissionApi",
  baseQuery: fetchBaseQuery({ baseUrl: import.meta.env.VITE_API_URL }),

  // "tagTypes" power RTK Query's caching/invalidation system — explained
  tagTypes: ["Submission"],

  endpoints: (builder) => ({
    // --- QUERY: for GET requests (reading data) ---
    getSubmissions: builder.query<Submission[], void>({
      query: () => "/submissions",
      // This endpoint "provides" the Submission tag — meaning RTK Query
      // knows this data should refresh whenever something "invalidates" it.
      providesTags: ["Submission"],
    }),

    getSubmissionById: builder.query<Submission, string>({
      query: (id) => `/submissions/${id}`,
      providesTags: ["Submission"],
    }),

    // --- MUTATION: for POST/PUT/DELETE requests (writing data) ---
    submitForm: builder.mutation<Submission, FormData>({
      query: (formData) => ({
        url: "/submissions",
        method: "POST",
        body: formData,
      }),

      invalidatesTags: ["Submission"],
    }),
  }),
});

// RTK Query auto-generates these hook names from your endpoint names above
// (getSubmissions -> useGetSubmissionsQuery, etc.) — you just import and use them.
export const {
  useGetSubmissionsQuery,
  useGetSubmissionByIdQuery,
  useSubmitFormMutation,
} = submissionApi;
