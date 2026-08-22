import { useEffect } from "react";
import { useForm, useFieldArray, SubmitHandler } from "react-hook-form";
import { useNavigate, useParams, Link } from "react-router-dom";
import FormInput from "../components/FormInput";
import {
  nameValidation,
  emailValidation,
  phoneValidation,
  institutionValidation,
  degreeValidation,
  yearValidation,
} from "../../application/validators/formValidators";
import {
  useGetSubmissionByIdQuery,
  useUpdateSubmissionMutation,
} from "../../infrastructure/api/submissionApi";
import { EducationEntry } from "../../domain/entities/Submission";

interface FormValues {
  fullName: string;
  email: string;
  phone: string;
  education: EducationEntry[];
  file?: FileList; // optional — unlike FormPage, a new file isn't required here
}

const EditSubmissionPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  // Fetches the EXISTING submission — this is what makes this an "edit"
  // page rather than a blank form. skip: !id prevents firing a request
  // with an undefined id if this page somehow renders without one.
  const { data: submission, isLoading: isLoadingSubmission } =
    useGetSubmissionByIdQuery(id!, { skip: !id });

  const [updateSubmission, { isLoading: isSaving }] =
    useUpdateSubmissionMutation();

  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormValues>();

  const { fields, append, remove } = useFieldArray({
    control,
    name: "education",
  });

  // reset() pre-fills the form the MOMENT the fetched data arrives. This
  // can't be done via useForm's defaultValues directly, because on the
  // very first render, `submission` is still undefined — the data hasn't
  // come back from the network yet. This effect re-runs once it does.
  useEffect(() => {
    if (submission) {
      reset({
        fullName: submission.fullName,
        email: submission.email,
        phone: submission.phone,
        education: submission.education,
      });
    }
  }, [submission, reset]);

  const onSubmit: SubmitHandler<FormValues> = async (data) => {
    const formData = new FormData();
    formData.append("fullName", data.fullName);
    formData.append("email", data.email);
    formData.append("phone", data.phone);
    formData.append("education", JSON.stringify(data.education));

    // Only attach a file if the user actually picked a new one — this
    // mirrors the backend's UpdateSubmissionRequest.File being nullable.
    if (data.file && data.file.length > 0) {
      formData.append("file", data.file[0]);
    }

    try {
      await updateSubmission({ id: id!, formData }).unwrap();
      navigate(`/submission/${id}`);
    } catch (err) {
      console.error("Failed to update submission:", err);
      alert("Could not save changes. Please try again.");
    }
  };

  if (isLoadingSubmission) {
    return (
      <div className="max-w-2xl mx-auto mt-10 p-6 text-center text-gray-500">
        Loading...
      </div>
    );
  }

  if (!submission) {
    return (
      <div className="max-w-2xl mx-auto mt-10 p-6 text-center text-red-500">
        Submission not found.
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto mt-10 p-6 bg-white rounded-lg shadow-md">
      <h2 className="text-2xl font-bold mb-6">Edit Submission</h2>

      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <FormInput
          label="Full Name"
          registration={register("fullName", nameValidation)}
          error={errors.fullName}
        />
        <FormInput
          label="Email"
          registration={register("email", emailValidation)}
          error={errors.email}
        />
        <FormInput
          label="Phone"
          registration={register("phone", phoneValidation)}
          error={errors.phone}
        />

        <div className="mb-6">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-xl font-semibold text-gray-800">Education</h3>
            <button
              type="button"
              onClick={() =>
                append({
                  institution: "",
                  degree: "",
                  year: undefined as unknown as number,
                })
              }
              className="px-4 py-2 bg-black hover:bg-gray-800 text-white rounded-md text-sm"
            >
              + Add Education
            </button>
          </div>

          {fields.map((field, index) => (
            <div
              key={field.id}
              className="border border-gray-200 rounded-lg p-4 mb-4 bg-gray-50"
            >
              <div className="flex justify-between items-start mb-3">
                <h4 className="text-sm font-medium text-gray-600">
                  Education {index + 1}
                </h4>
                {fields.length > 1 && (
                  <button
                    type="button"
                    onClick={() => remove(index)}
                    className="text-red-500 hover:text-red-700 text-sm font-medium"
                  >
                    Remove
                  </button>
                )}
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                <FormInput
                  label="Institution"
                  registration={register(
                    `education.${index}.institution`,
                    institutionValidation,
                  )}
                  error={errors.education?.[index]?.institution}
                />
                <FormInput
                  label="Degree"
                  registration={register(
                    `education.${index}.degree`,
                    degreeValidation,
                  )}
                  error={errors.education?.[index]?.degree}
                />
                <FormInput
                  label="Year"
                  type="number"
                  registration={register(`education.${index}.year`, {
                    ...yearValidation,
                    valueAsNumber: true,
                  })}
                  error={errors.education?.[index]?.year}
                />
              </div>
            </div>
          ))}
        </div>

        <div className="mb-6">
          <label className="block text-gray-700 font-medium mb-2">
            Replace file (optional — leave empty to keep current file)
          </label>
          <input
            type="file"
            {...register("file")}
            className="w-full px-4 py-2 border border-gray-300 rounded-md text-sm"
          />
        </div>

        <button
          type="submit"
          disabled={isSaving}
          className={`w-full text-white font-semibold py-2 px-4 rounded-md ${
            isSaving
              ? "bg-gray-400 cursor-not-allowed"
              : "bg-blue-600 hover:bg-blue-700"
          }`}
        >
          {isSaving ? "Saving..." : "Save Changes"}
        </button>
      </form>

      <div className="mt-4">
        <Link
          to={`/submission/${id}`}
          className="text-blue-600 hover:underline text-sm"
        >
          ← Cancel and go back
        </Link>
      </div>
    </div>
  );
};

export default EditSubmissionPage;
