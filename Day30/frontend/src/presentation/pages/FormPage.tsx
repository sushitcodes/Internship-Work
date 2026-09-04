import { useEffect } from "react";
import { useForm, SubmitHandler } from "react-hook-form"; // ← Removed Controller
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
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { Label } from "@/components/ui/label";
import { FormInput } from "../components/FormInput"; // ← Import FormInput
import {
  SelectValue,
  SelectContent,
  SelectItem,
  SelectTrigger,
  Select,
} from "@/components/ui/select";
import { useGetClassRoomsQuery } from "@/infrastructure/api/classRoomApi";

interface FormValues {
  fullName: string;
  classRoomId: string;
  rollNo: number;
  file?: FileList;
}

const FormPage: React.FC = () => {
  const { id } = useParams<{ id?: string }>();
  const isEditMode = Boolean(id);
  const navigate = useNavigate();
  const { data: classRooms } = useGetClassRoomsQuery();

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
    setValue, //for select
    formState: { errors },
  } = useForm<FormValues>({
    defaultValues: {
      fullName: "",
      classRoomId: "",
      rollNo: 0,
    },
  });

  useEffect(() => {
    if (isEditMode && existingSubmission) {
      reset({
        fullName: existingSubmission.fullName,
        classRoomId: existingSubmission.classRoomId,
        rollNo: existingSubmission.rollNo,
      });
    }
  }, [isEditMode, existingSubmission, reset]);

  const onSubmit: SubmitHandler<FormValues> = async (data) => {
    // Log the data to debug
    console.log(
      "Submitting education data:",
      data.classRoomId,
      data.fullName,
      data.rollNo,
      data.file,
    );

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
      console.error("Failed to save submission:", err);
      alert("Could not save. Please try again.");
    }
  };

  if (isEditMode && isLoadingExisting) {
    return (
      <Card className="max-w-2xl mx-auto mt-10">
        <CardHeader>
          <CardTitle>
            {isEditMode ? "Edit Submission" : "Submit Form"}
          </CardTitle>
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
    if (existingSubmission.fileUrl.startsWith("http")) {
      return existingSubmission.fileUrl;
    }
    if (existingSubmission.fileUrl.startsWith("/")) {
      return `${apiOrigin}${existingSubmission.fileUrl}`;
    }
    return `${apiOrigin}/${existingSubmission.fileUrl}`;
  };

  const fileUrl = getFileUrl();

  return (
    <Card className="max-w-2xl mx-auto mt-10">
      <CardHeader>
        <CardTitle className="text-center text-xl">
          {isEditMode ? "Edit Submission" : "Submit Form"}
        </CardTitle>
      </CardHeader>
      <CardContent>
        <form
          onSubmit={handleSubmit(onSubmit)}
          noValidate
          className="space-y-6"
        >
          {/* Full Name */}
          <FormInput
            id="fullName"
            label="Full Name"
            registration={register("fullName", nameValidation)}
            error={errors.fullName}
          />

          {/* Class Selection - NEW */}
          <div className="space-y-2">
            <Label>Class</Label>
            <Select
              onValueChange={(v) => setValue("classRoomId", v ?? "")}
              defaultValue={existingSubmission?.classRoomId}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select a class" />
              </SelectTrigger>
              <SelectContent>
                {classRooms?.map((c) => (
                  <SelectItem key={c.id} value={c.id}>
                    {c.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {errors.classRoomId && (
              <p className="text-sm text-red-500">
                {errors.classRoomId.message}
              </p>
            )}
          </div>

          {/* Roll No - NEW */}
          <FormInput
            id="rollNo"
            label="Roll No"
            type="number"
            registration={register("rollNo", {
              required: "Roll number is required",
              valueAsNumber: true,
              min: { value: 1, message: "Roll number must be at least 1" },
            })}
            error={errors.rollNo}
          />

          {/* File Upload - Keep as is */}
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
                  className="text-shadow-black hover:underline font-medium"
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
          <Button type="submit" disabled={isSaving} className="w-full">
            {isSaving ? "Saving..." : isEditMode ? "Save Changes" : "Submit"}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
};

export default FormPage;
