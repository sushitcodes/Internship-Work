import { useEffect } from "react";
import { useForm, useFieldArray, SubmitHandler } from "react-hook-form"; // ← Removed Controller
import { useNavigate, useParams } from "react-router-dom";
import {
  nameValidation,
  emailValidation,
  phoneValidation,
  institutionValidation,
  degreeValidation,
  yearValidation,
  fileValidation,
} from "../../application/validators/formValidators";
import {
  useGetSubmissionByIdQuery,
  useSubmitFormMutation,
  useUpdateSubmissionMutation,
} from "../../infrastructure/api/submissionApi";
import { EducationEntry } from "../../domain/entities/Submission";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { Label } from "@/components/ui/label";
import { FormInput } from "../components/FormInput"; // ← Import FormInput

interface FormValues {
  fullName: string;
  email: string;
  phone: string;
  education: EducationEntry[];
  file?: FileList;
}

const FormPage: React.FC = () => {
  const { id } = useParams<{ id?: string }>();
  const isEditMode = Boolean(id);
  const navigate = useNavigate();

  const { data: existingSubmission, isLoading: isLoadingExisting } =
    useGetSubmissionByIdQuery(id!, { skip: !isEditMode });

  const [submitForm, { isLoading: isCreating }] = useSubmitFormMutation();
  const [updateSubmission, { isLoading: isUpdating }] =
    useUpdateSubmissionMutation();
  const isSaving = isCreating || isUpdating;

  const {
    register, // ← Added register
    control, // ← Keep for useFieldArray
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormValues>({
    defaultValues: {
      fullName: "",
      email: "",
      phone: "",
      education: [
        { institution: "", degree: "", year: undefined as unknown as number },
      ],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control,
    name: "education",
  });

  useEffect(() => {
    if (isEditMode && existingSubmission) {
      reset({
        fullName: existingSubmission.fullName,
        email: existingSubmission.email,
        phone: existingSubmission.phone,
        education: existingSubmission.education,
      });
    }
  }, [isEditMode, existingSubmission, reset]);

  const onSubmit: SubmitHandler<FormValues> = async (data) => {
    // Log the data to debug
    console.log("Submitting education data:", data.education);

    const formData = new FormData();
    formData.append("fullName", data.fullName);
    formData.append("email", data.email);
    formData.append("phone", data.phone);
    formData.append("education", JSON.stringify(data.education));
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
          {/* ✅ Using FormInput with register */}
          <FormInput
            id="fullName"
            label="Full Name"
            placeholder="Enter your full name"
            registration={register("fullName", nameValidation)}
            error={errors.fullName}
          />

          <FormInput
            id="email"
            label="Email"
            type="email"
            placeholder="Enter your email"
            registration={register("email", emailValidation)}
            error={errors.email}
          />

          <FormInput
            id="phone"
            label="Phone"
            placeholder="Enter your phone number"
            registration={register("phone", phoneValidation)}
            error={errors.phone}
          />

          {/* Education Section */}
          <div className="space-y-4">
            <div className="flex justify-between items-center">
              <h3 className="text-lg font-semibold">Education</h3>
              <Button
                type="button"
                variant="default"
                onClick={() =>
                  append({
                    institution: "",
                    degree: "",
                    year: undefined as unknown as number,
                  })
                }
              >
                + Add Education
              </Button>
            </div>

            {fields.map((field, index) => (
              <div key={field.id} className="border rounded-lg p-4 space-y-4">
                <div className="flex justify-between items-center">
                  <span className="text-sm font-medium text-muted-foreground">
                    Education {index + 1}
                  </span>
                  {fields.length > 1 && (
                    <Button
                      type="button"
                      variant="destructive"
                      size="sm"
                      onClick={() => remove(index)}
                    >
                      Remove
                    </Button>
                  )}
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <FormInput
                    id={`education.${index}.institution`}
                    label="Institution"
                    placeholder="Institution name"
                    registration={register(
                      `education.${index}.institution`,
                      institutionValidation,
                    )}
                    error={errors.education?.[index]?.institution}
                  />

                  <FormInput
                    id={`education.${index}.degree`}
                    label="Degree"
                    placeholder="Degree"
                    registration={register(
                      `education.${index}.degree`,
                      degreeValidation,
                    )}
                    error={errors.education?.[index]?.degree}
                  />

                  <FormInput
                    id={`education.${index}.year`}
                    label="Year"
                    type="number"
                    placeholder="Year"
                    registration={register(`education.${index}.year`, {
                      ...yearValidation,
                      setValueAs: (v) => {
                        if (!v) return undefined;
                        const num = parseInt(v);
                        return isNaN(num) ? undefined : num;
                      },
                    })}
                    error={errors.education?.[index]?.year}
                  />
                </div>
              </div>
            ))}
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
