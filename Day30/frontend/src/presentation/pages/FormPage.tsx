import { useEffect } from "react";
import { useForm, SubmitHandler } from "react-hook-form";
import { useNavigate, useParams } from "react-router-dom";
import {
  nameValidation,
  fileValidation,
} from "../../application/validators/formValidators";
import {
  useGetSubmissionByIdQuery,
  useSubmitFormMutation,
  useUpdateSubmissionMutation,
} from "../../infrastructure/api/submissionApi";
import { useGetOwnProfileQuery } from "../../infrastructure/api/userApi";
import { useAppSelector } from "../../infrastructure/store/hooks";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { Label } from "@/components/ui/label";
import { useGetClassRoomsQuery } from "@/infrastructure/api/classRoomApi";
import { toast } from "sonner";
import { IdSelect } from "../components/IdSelect";
import { useGetMyClassQuery } from "../../infrastructure/api/enrollmentApi";

interface FormValues {
  fullName: string;
  classRoomId: string;
  rollNo: number;
  file?: FileList;
}

// Extract a readable message from an RTK Query error object.
// The backend returns either a plain string body or a { message } / { title } JSON object.
function extractErrorMessage(err: unknown): string {
  if (!err || typeof err !== "object")
    return "Could not save. Please try again.";
  const e = err as Record<string, unknown>;

  // RTK Query wraps fetch errors as { status, data }
  if ("data" in e) {
    const data = e.data;
    if (typeof data === "string" && data.length > 0) return data;
    if (data && typeof data === "object") {
      const d = data as Record<string, unknown>;
      if (typeof d.message === "string") return d.message;
      if (typeof d.title === "string") return d.title;
    }
  }
  if (typeof e.message === "string") return e.message;
  return "Could not save. Please try again.";
}

const FormPage: React.FC = () => {
  const { id } = useParams<{ id?: string }>();
  const isEditMode = Boolean(id);
  const navigate = useNavigate();

  const roles = useAppSelector((state) => state.auth.roles);
  const isStaffOrAdmin = roles.includes("Staff") || roles.includes("Admin");

  const { data: classRooms } = useGetClassRoomsQuery();
  const { data: ownProfile } = useGetOwnProfileQuery();
  const { data: myClass, isLoading: isLoadingMyClass } = useGetMyClassQuery(
    undefined,
    {
      skip: isStaffOrAdmin || isEditMode, // students only, create mode only
    },
  );
  const { data: existingSubmission, isLoading: isLoadingExisting } =
    useGetSubmissionByIdQuery(id!, { skip: !isEditMode });

  const [submitForm, { isLoading: isCreating }] = useSubmitFormMutation();
  const [updateSubmission, { isLoading: isUpdating }] =
    useUpdateSubmissionMutation();
  const isSaving = isCreating || isUpdating;

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors },
  } = useForm<FormValues>({
    defaultValues: {
      fullName: "",
      classRoomId: "",
      rollNo: 0,
    },
  });

  // Register classRoomId so validation fires on submit
  register("classRoomId", { required: "Please select a class" });
  const classRoomIdValue = watch("classRoomId");

  // Edit mode: restore existing submission values
  useEffect(() => {
    if (isEditMode && existingSubmission) {
      reset({
        fullName: existingSubmission.fullName,
        classRoomId: existingSubmission.classRoomId,
        rollNo: existingSubmission.rollNo,
      });
      setValue("classRoomId", existingSubmission.classRoomId);
    }
  }, [isEditMode, existingSubmission, reset, setValue]);

  // Create mode: auto-fill Roll No from the logged-in student's own profile.
  // Staff/Admin can type any number; students always submit their own.
  useEffect(() => {
    if (isEditMode) return;
    if (ownProfile) {
      if (ownProfile.fullName) setValue("fullName", ownProfile.fullName);
      if (ownProfile.memberNumber) setValue("rollNo", ownProfile.memberNumber);
    }
    if (myClass) {
      setValue("classRoomId", myClass.classRoomId, { shouldValidate: true });
    }
  }, [isEditMode, ownProfile, myClass, setValue]);

  const onSubmit: SubmitHandler<FormValues> = async (data) => {
    const formData = new FormData();
    formData.append("fullName", data.fullName);
    formData.append("classRoomId", data.classRoomId);
    formData.append("rollNo", data.rollNo.toString());
    if (data.file && data.file.length > 0) {
      formData.append("file", data.file[0]);
    }

    try {
      if (isEditMode) {
        await updateSubmission({ id: id!, formData }).unwrap();
        navigate(`/submission/${id}`);
      } else {
        const result = await submitForm(formData).unwrap();
        reset();
        navigate(`/submission/${result.id}`);
      }
    } catch (err) {
      // Show the backend's actual error message (e.g. "No student found with
      // roll number X") so the user knows exactly what to fix.
      const message = extractErrorMessage(err);
      toast.error(message);
      console.error("Failed to save submission:", err);
    }
  };

  if (isEditMode && isLoadingExisting) {
    return (
      <Card className="max-w-2xl mx-auto mt-10">
        <CardHeader>
          <CardTitle>Edit Submission</CardTitle>
        </CardHeader>
        <CardContent className="space-y-6">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-32 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-12 w-full" />
        </CardContent>
      </Card>
    );
  }

  const getFileUrl = () => {
    if (!existingSubmission?.fileUrl) return null;
    const apiOrigin = (import.meta.env.VITE_API_URL ?? "").replace(
      /\/api\/?$/,
      "",
    );
    if (existingSubmission.fileUrl.startsWith("http"))
      return existingSubmission.fileUrl;
    if (existingSubmission.fileUrl.startsWith("/"))
      return `${apiOrigin}${existingSubmission.fileUrl}`;
    return `${apiOrigin}/${existingSubmission.fileUrl}`;
  };

  const fileUrl = getFileUrl();

  // If the student has no profile yet, warn them before they try to submit
  // and hit the backend validation.
  const studentHasNoProfile = !isStaffOrAdmin && !ownProfile?.memberNumber;
  const studentHasNoClass =
    !isStaffOrAdmin && !isEditMode && !isLoadingMyClass && !myClass;
  const cannotSubmit = isSaving || studentHasNoProfile || studentHasNoClass;

  return (
    <Card className="max-w-2xl mx-auto mt-10">
      <CardHeader>
        <CardTitle className="text-center text-xl">
          {isEditMode ? "Edit Submission" : "Submit Form"}
        </CardTitle>
      </CardHeader>
      <CardContent>
        {studentHasNoProfile && (
          <div className="mb-4 rounded-md border border-yellow-300 bg-yellow-50 p-3 text-sm text-yellow-800">
            Your profile does not have a roll number yet. Please ask an
            administrator to set up your profile before submitting.
          </div>
        )}
        {studentHasNoClass && (
          <div className="mb-4 rounded-md border border-yellow-300 bg-yellow-50 p-3 text-sm text-yellow-800">
            You are not enrolled in any class. Contact an admin before
            submitting.
          </div>
        )}

        <form
          onSubmit={handleSubmit(onSubmit)}
          noValidate
          className="space-y-6"
        >
          {/* Full Name — read-only for students, editable for Staff/Admin */}
          <div className="space-y-2">
            <Label htmlFor="fullName">Full Name</Label>
            <Input
              id="fullName"
              readOnly={!isStaffOrAdmin}
              className={!isStaffOrAdmin ? "bg-muted cursor-not-allowed" : ""}
              {...register("fullName", nameValidation)}
            />
            {errors.fullName && (
              <p className="text-sm text-red-500">{errors.fullName.message}</p>
            )}
            {!isStaffOrAdmin && (
              <p className="text-xs text-muted-foreground">
                From your profile. Contact an admin to change it.
              </p>
            )}
          </div>

          {/* Class Selection */}
          {/* Class — dropdown for Staff/Admin, read-only display for students */}
          <div className="space-y-2">
            <Label>Class</Label>

            {isStaffOrAdmin ? (
              <IdSelect
                options={
                  classRooms?.map((c) => ({
                    id: c.id,
                    label: c.name,
                  })) || []
                }
                value={
                  classRoomIdValue || existingSubmission?.classRoomId || ""
                }
                onValueChange={(v) =>
                  setValue("classRoomId", v ?? "", { shouldValidate: true })
                }
                placeholder="Select a class"
                className={errors.classRoomId ? "border-red-500" : ""}
              />
            ) : (
              <>
                <Input
                  value={
                    myClass?.classRoomName ??
                    existingSubmission?.classRoomName ??
                    "Not enrolled in any class"
                  }
                  readOnly
                  className="bg-muted cursor-not-allowed"
                />

                {/* Hidden registered field so react-hook-form still submits classRoomId */}
                <input
                  type="hidden"
                  {...register("classRoomId", {
                    required: "You are not enrolled in any class",
                  })}
                />
              </>
            )}

            {errors.classRoomId && (
              <p className="text-sm text-red-500">
                {errors.classRoomId.message}
              </p>
            )}
            {!isStaffOrAdmin && myClass && (
              <p className="text-xs text-muted-foreground">
                Based on your enrollment. Contact an admin to change classes.
              </p>
            )}
          </div>

          {/* Roll No — read-only for students (auto-filled from their profile),
              editable for Staff/Admin who may submit on behalf of any student */}
          <div className="space-y-2">
            <Label htmlFor="rollNo">
              Roll No
              {!isStaffOrAdmin && ownProfile?.memberNumber && (
                <span className="ml-2 text-xs text-muted-foreground">
                  (your member number)
                </span>
              )}
            </Label>
            <Input
              id="rollNo"
              type="number"
              readOnly={!isStaffOrAdmin}
              className={!isStaffOrAdmin ? "bg-muted cursor-not-allowed" : ""}
              {...register("rollNo", {
                required: "Roll number is required",
                valueAsNumber: true,
                min: { value: 1, message: "Roll number must be at least 1" },
              })}
            />
            {errors.rollNo && (
              <p className="text-sm text-red-500">{errors.rollNo.message}</p>
            )}
          </div>

          {/* File Upload */}
          <div className="space-y-2">
            <Label htmlFor="file">
              {isEditMode
                ? "Replace file (optional — leave empty to keep current)"
                : "Attachment (Certificate)"}
            </Label>

            {isEditMode && fileUrl && (
              <div className="text-sm">
                <span className="text-muted-foreground">Current file: </span>
                <a
                  href={fileUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="hover:underline font-medium"
                >
                  📎 View current file
                </a>
              </div>
            )}

            <Input
              id="file"
              type="file"
              {...register("file", isEditMode ? {} : fileValidation)}
            />
            {errors.file && (
              <p className="text-sm text-red-500">{errors.file.message}</p>
            )}
            {isEditMode && (
              <p className="text-xs text-muted-foreground">
                Leave empty to keep the current file
              </p>
            )}
          </div>

          {/* Submit Button */}
          <Button type="submit" disabled={cannotSubmit} className="w-full">
            {isSaving ? "Saving..." : isEditMode ? "Save Changes" : "Submit"}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
};

export default FormPage;
