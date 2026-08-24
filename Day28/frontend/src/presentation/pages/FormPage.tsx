import { useEffect } from "react";
import {
  useForm,
  useFieldArray,
  SubmitHandler,
  Controller,
} from "react-hook-form";
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
    control,
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

    // If fileUrl already starts with http, use it as is
    if (existingSubmission.fileUrl.startsWith("http")) {
      return existingSubmission.fileUrl;
    }

    // If fileUrl starts with /, just append to apiOrigin
    if (existingSubmission.fileUrl.startsWith("/")) {
      return `${apiOrigin}${existingSubmission.fileUrl}`;
    }

    // Otherwise, add a slash between
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
          <div className="space-y-2">
            <Label htmlFor="fullName">Full Name</Label>
            <Controller
              name="fullName"
              control={control}
              rules={nameValidation}
              render={({ field }) => (
                <Input
                  id="fullName"
                  placeholder="Enter your full name"
                  value={field.value || ""}
                  onChange={field.onChange}
                  onBlur={field.onBlur}
                  ref={field.ref}
                />
              )}
            />
            {errors.fullName && (
              <p className="text-sm text-red-500">{errors.fullName.message}</p>
            )}
          </div>

          {/* Email */}
          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Controller
              name="email"
              control={control}
              rules={emailValidation}
              render={({ field }) => (
                <Input
                  id="email"
                  type="email"
                  placeholder="Enter your email"
                  value={field.value || ""}
                  onChange={field.onChange}
                  onBlur={field.onBlur}
                  ref={field.ref}
                />
              )}
            />
            {errors.email && (
              <p className="text-sm text-red-500">{errors.email.message}</p>
            )}
          </div>

          {/* Phone */}
          <div className="space-y-2">
            <Label htmlFor="phone">Phone</Label>
            <Controller
              name="phone"
              control={control}
              rules={phoneValidation}
              render={({ field }) => (
                <Input
                  id="phone"
                  placeholder="Enter your phone number"
                  value={field.value || ""}
                  onChange={field.onChange}
                  onBlur={field.onBlur}
                  ref={field.ref}
                />
              )}
            />
            {errors.phone && (
              <p className="text-sm text-red-500">{errors.phone.message}</p>
            )}
          </div>

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
                  {/* Institution */}
                  <div className="space-y-2">
                    <Label htmlFor={`education.${index}.institution`}>
                      Institution
                    </Label>
                    <Controller
                      name={`education.${index}.institution`}
                      control={control}
                      rules={institutionValidation}
                      render={({ field }) => (
                        <Input
                          id={`education.${index}.institution`}
                          placeholder="Institution name"
                          value={field.value || ""}
                          onChange={field.onChange}
                          onBlur={field.onBlur}
                          ref={field.ref}
                        />
                      )}
                    />
                    {errors.education?.[index]?.institution && (
                      <p className="text-sm text-red-500">
                        {errors.education[index].institution?.message}
                      </p>
                    )}
                  </div>

                  {/* Degree */}
                  <div className="space-y-2">
                    <Label htmlFor={`education.${index}.degree`}>Degree</Label>
                    <Controller
                      name={`education.${index}.degree`}
                      control={control}
                      rules={degreeValidation}
                      render={({ field }) => (
                        <Input
                          id={`education.${index}.degree`}
                          placeholder="Degree"
                          value={field.value || ""}
                          onChange={field.onChange}
                          onBlur={field.onBlur}
                          ref={field.ref}
                        />
                      )}
                    />
                    {errors.education?.[index]?.degree && (
                      <p className="text-sm text-red-500">
                        {errors.education[index].degree?.message}
                      </p>
                    )}
                  </div>

                  {/* Year */}
                  <div className="space-y-2">
                    <Label htmlFor={`education.${index}.year`}>Year</Label>
                    <Controller
                      name={`education.${index}.year`}
                      control={control}
                      rules={yearValidation}
                      render={({ field }) => (
                        <Input
                          id={`education.${index}.year`}
                          type="number"
                          placeholder="Year"
                          value={field.value || ""}
                          onChange={(e) => {
                            const value = e.target.value;
                            field.onChange(value ? parseInt(value) : undefined);
                          }}
                          onBlur={field.onBlur}
                          ref={field.ref}
                        />
                      )}
                    />
                    {errors.education?.[index]?.year && (
                      <p className="text-sm text-red-500">
                        {errors.education[index].year?.message}
                      </p>
                    )}
                  </div>
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

            <Controller
              name="file"
              control={control}
              rules={isEditMode ? {} : fileValidation}
              render={({ field: { onChange, value, ...field } }) => (
                <Input
                  id="file"
                  type="file"
                  onChange={(e) => {
                    const files = e.target.files;
                    onChange(files);
                  }}
                  {...field}
                />
              )}
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
