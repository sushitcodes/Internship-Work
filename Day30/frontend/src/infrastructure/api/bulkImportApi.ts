import { api } from "./api";

export interface BulkImportResult {
  success: boolean;
  importedCount: number;
  errors: string[];
}

export const bulkImportApi = api.injectEndpoints({
  endpoints: (builder) => ({
    bulkImportStudents: builder.mutation<
      BulkImportResult,
      { file: File; classRoomId: string }
    >({
      query: ({ file, classRoomId }) => {
        const formData = new FormData();
        formData.append("file", file);
        formData.append("classRoomId", classRoomId);
        return {
          url: "/bulkimport/students",
          method: "POST",
          body: formData,
        };
      },
      // Adding students affects the user list, the class roster, and the
      // classroom's student count. Invalidating those tags is what makes
      // the pages refetch without a full browser reload.
      invalidatesTags: ["UserProfile", "Enrollment", "ClassRoom"],
    }),
  }),
});

export const { useBulkImportStudentsMutation } = bulkImportApi;

/**
 * Downloads the sample template. Kept as a raw fetch because RTK Query's
 * caching layer doesn't fit blob responses — we just want the bytes.
 */
export async function downloadImportTemplate() {
  const apiOrigin = import.meta.env.VITE_API_URL ?? "";
  const res = await fetch(`${apiOrigin}/bulkimport/template`, {
    credentials: "include",
  });
  if (!res.ok) throw new Error("Could not download template.");

  const blob = await res.blob();
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = "Student_Import_Template.xlsx";
  document.body.appendChild(a);
  a.click();
  a.remove();
  window.URL.revokeObjectURL(url);
}
