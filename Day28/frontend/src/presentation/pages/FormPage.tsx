// presentation/pages/FormPage.tsx

import { useForm, useFieldArray, SubmitHandler } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import FormInput from "../components/FormInput";
import {
  nameValidation,
  emailValidation,
  phoneValidation,
  institutionValidation,
  degreeValidation,
  yearValidation,
  fileValidation,
} from "../../application/validators/formValidators";
import { useSubmitFormMutation } from "../../infrastructure/api/submissionApi";
import { EducationEntry } from "../../domain/entities/Submission";

interface FormValues {
  fullName: string;
  email: string;
  phone: string;
  education: EducationEntry[];
  file: FileList;
}

const FormPage: React.FC = () => {
  const navigate = useNavigate();

  // useSubmitFormMutation gives us:
  //   submitForm  -> the function that actually fires the request
  //   { isLoading } -> automatically tracked loading state (no useState needed!)
  // This replaces both submissionService.submit() AND your old isSubmitting
  // tracking — RTK Query does both jobs in one hook.
  const [submitForm, { isLoading }] = useSubmitFormMutation();

  const {
    register,
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

  const onSubmit: SubmitHandler<FormValues> = async (data) => {
    const formData = new FormData();
    formData.append("fullName", data.fullName);
    formData.append("email", data.email);
    formData.append("phone", data.phone);
    formData.append("education", JSON.stringify(data.education));
    formData.append("file", data.file[0]);

    try {
      const result = await submitForm(formData).unwrap();
      reset();
      navigate(`/submission/${result.id}`);
    } catch (error) {
      console.error("Error submitting form:", error);
      alert("Failed to save data. Please try again.");
    }
  };

  return (
    <div className="max-w-2xl mx-auto mt-10 p-6 bg-white rounded-lg shadow-md">
      <h2 className="text-2xl font-bold mb-6">Submit Form</h2>

      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <FormInput
          label="Full Name"
          placeholder="Enter your full name"
          registration={register("fullName", nameValidation)}
          error={errors.fullName}
        />

        <FormInput
          label="Email"
          placeholder="Enter your email"
          registration={register("email", emailValidation)}
          error={errors.email}
        />

        <FormInput
          label="Phone"
          placeholder="Enter your phone number"
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
                  placeholder="e.g., Tribhuvan University"
                  registration={register(
                    `education.${index}.institution`,
                    institutionValidation,
                  )}
                  error={errors.education?.[index]?.institution}
                />
                <FormInput
                  label="Degree"
                  placeholder="e.g., Bachelor of Science"
                  registration={register(
                    `education.${index}.degree`,
                    degreeValidation,
                  )}
                  error={errors.education?.[index]?.degree}
                />
                <FormInput
                  label="Year"
                  type="number"
                  placeholder="e.g., 2024"
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
            Attachment (Certificate)
          </label>
          <input
            type="file"
            {...register("file", fileValidation)}
            className="w-full px-4 py-2 border rounded-md text-sm"
          />
          {errors.file && (
            <span className="text-red-500 text-sm mt-1 block">
              {errors.file.message}
            </span>
          )}
        </div>

        <button
          type="submit"
          disabled={isLoading}
          className={`w-full text-white font-semibold py-2 px-4 rounded-md ${
            isLoading
              ? "bg-gray-400 cursor-not-allowed"
              : "bg-blue-600 hover:bg-blue-700"
          }`}
        >
          {isLoading ? "Submitting..." : "Submit"}
        </button>
      </form>
    </div>
  );
};

export default FormPage;
