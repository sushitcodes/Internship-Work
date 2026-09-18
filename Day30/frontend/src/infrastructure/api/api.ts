import { createApi } from "@reduxjs/toolkit/query/react";
import { baseQueryWithAuth } from "./baseQueryWithAuth";

// The ONE RTK Query instance for the whole app. Every feature file
// (submissionApi, authApi, etc.) attaches its endpoints to THIS object
// via injectEndpoints — they don't call createApi() themselves anymore.
// One reducerPath, one middleware, one shared tag registry.
export const api = createApi({
  reducerPath: "api",
  baseQuery: baseQueryWithAuth,

  // The union of every tagType previously scattered across 7 files.
  // Declaring them here, once, is what lets a tag invalidated in one
  // feature file actually reach providesTags declared in another.
  tagTypes: [
    "Submission",
    "Dashboard",
    "Attendance",
    "ClassRoom",
    "Enrollment",
    "Subject",
    "Grade",
    "UserProfile",
  ],

  // No endpoints here — every one gets added via injectEndpoints below.
  endpoints: () => ({}),
});
