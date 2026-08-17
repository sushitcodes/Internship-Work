import React from "react";
import { useForm, useFieldArray } from "react-hook-form";
import { useDispatch } from "react-redux";
import { useNavigate } from "react-router-dom";
import { addSubmission } from "../store/actions/formActions";
import { submitFormData } from "../services/api";
// Define the form data type
interface FormInputs {
  name: string;
  email: string;
  message: string;
  education: {
    degree: string;
    year: string;
    school: string;
  }[];
}

const FormPage: React.FC = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  //  Initialize React Hook Form
  const {
    register,
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
  } = useForm<FormInputs>({
    defaultValues: {
      name: "",
      email: "",
      message: "",
      education: [], // Start with one empty education
    },
  });

  //  useFieldArray for dynamic education fields
  const { fields, append, remove } = useFieldArray({
    control,
    name: "education",
  });

  //  Form submission handler
  const onSubmit = async (data: FormInputs) => {
    try {
      // Convert education entries to include IDs
      const educationWithIds = data.education.map((edu) => ({
        ...edu,
        id: Date.now().toString() + Math.random().toString(36).substring(2, 5),
      }));

      // Prepare data for Redux
      const formData = {
        name: data.name,
        email: data.email,
        message: data.message,
        education: educationWithIds,
      };
      console.log("📤 Sending data to backend:", formData);
      const savedData = await submitFormData(formData);
      console.log("✅ Data saved to backend:", savedData); // Dispatch to Redux
      dispatch(addSubmission(formData));

      // Navigate to submissions page
      navigate("/submissions");

      // Reset form
      reset();
    } catch (error) {
      console.error("❌ Error submitting form:", error);
      alert("Failed to save data. Please try again.");
    }
  };

  //  Add new education field
  const handleAddEducation = () => {
    append({ degree: "", year: "", school: "" });
  };

  //  Remove education field
  const handleRemoveEducation = (index: number) => {
    if (fields.length > 1) {
      remove(index);
    } else {
      alert("You must have at least one education entry.");
    }
  };

  return (
    <div className="max-w-2xl mx-auto mt-10 p-6 bg-white rounded-lg shadow-md">
      <h2 className="text-2xl font-bold mb-6">Submit Form</h2>

      <form onSubmit={handleSubmit(onSubmit)}>
        {/* NAME FIELD */}
        <div className="mb-4">
          <label className="block text-gray-700 font-medium mb-2">Name</label>
          <input
            {...register("name", {
              required: "Name is required",
              pattern: {
                value: /^[A-Za-z]+$/i,
                message: "Name can only contain letters",
              },
              maxLength: { value: 20, message: "Max length is 20" },
              minLength: { value: 5, message: "Min length is 5" },
            })}
            className={`w-full px-4 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 ${
              errors.name ? "border-red-500" : "border-gray-300"
            }`}
            placeholder="Enter your name"
          />
          {errors.name && (
            <span className="text-red-500 text-sm mt-1">
              {errors.name.message}
            </span>
          )}
        </div>
        {/* EMAIL FIELD */}
        <div className="mb-4">
          <label className="block text-gray-700 font-medium mb-2">Email</label>
          <input
            {...register("email", {
              required: "Email is required",
              pattern: {
                value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                message: "Invalid email address",
              },
            })}
            className={`w-full px-4 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 ${
              errors.email ? "border-red-500" : "border-gray-300"
            }`}
            placeholder="Enter your email"
          />
          {errors.email && (
            <span className="text-red-500 text-sm mt-1">
              {errors.email.message}
            </span>
          )}
        </div>
        {/* MESSAGE FIELD */}
        <div className="mb-6">
          <label className="block text-gray-700 font-medium mb-2">
            Message
          </label>
          <textarea
            {...register("message", { required: "Message is required" })}
            rows={4}
            className={`w-full px-4 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 resize-vertical ${
              errors.message ? "border-red-500" : "border-gray-300"
            }`}
            placeholder="Enter your message"
          />
          {errors.message && (
            <span className="text-red-500 text-sm mt-1">
              {errors.message.message}
            </span>
          )}
        </div>
        {/* EDUCATION SECTION */}
        <div className="mb-6">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-xl font-semibold text-gray-800">Education</h3>
            <button
              type="button"
              onClick={handleAddEducation}
              className="px-4 py-2 bg-black hover:bg-gray-800 text-white rounded-md transition-colors text-sm"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="1.5"
                strokeLinecap="round"
                strokeLinejoin="round"
              >
                <line x1="12" y1="5" x2="12" y2="19" />
                <line x1="5" y1="12" x2="19" y2="12" />
              </svg>{" "}
              Add Education
            </button>
          </div>

          {/* Education Fields */}
          {fields.map((field, index) => (
            <div
              key={field.id}
              className="border border-gray-200 rounded-lg p-4 mb-4 bg-gray-50 relative"
            >
              <div className="flex justify-between items-start mb-3">
                <h4 className="text-sm font-medium text-gray-600">Education</h4>
                <button
                  type="button"
                  onClick={() => handleRemoveEducation(index)}
                  className="text-red-500 hover:text-red-700 text-sm font-medium"
                >
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="24"
                    height="24"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="1.5"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  >
                    <polyline points="3 6 5 6 21 6" />
                    <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                    <line x1="10" y1="11" x2="10" y2="17" />
                    <line x1="14" y1="11" x2="14" y2="17" />
                  </svg>{" "}
                  Remove
                </button>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                {/* Degree */}
                <div>
                  <label className="block text-sm text-gray-600 mb-1">
                    Degree Type
                  </label>
                  <input
                    {...register(`education.${index}.degree`, {
                      required: "Degree is required",
                    })}
                    className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm ${
                      errors.education?.[index]?.degree
                        ? "border-red-500"
                        : "border-gray-300"
                    }`}
                    placeholder="e.g., Bachelor of Science"
                  />
                  {errors.education?.[index]?.degree && (
                    <span className="text-red-500 text-xs mt-1">
                      {errors.education[index]?.degree?.message}
                    </span>
                  )}
                </div>

                {/* Year */}
                <div>
                  <label className="block text-sm text-gray-600 mb-1">
                    Completed Year
                  </label>
                  <input
                    {...register(`education.${index}.year`, {
                      required: "Year is required",
                      pattern: {
                        value: /^[0-9]{4}$/,
                        message: "Enter a valid year (e.g., 2020)",
                      },
                    })}
                    className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm ${
                      errors.education?.[index]?.year
                        ? "border-red-500"
                        : "border-gray-300"
                    }`}
                    placeholder="e.g., 2020"
                  />
                  {errors.education?.[index]?.year && (
                    <span className="text-red-500 text-xs mt-1">
                      {errors.education[index]?.year?.message}
                    </span>
                  )}
                </div>

                {/* School */}
                <div>
                  <label className="block text-sm text-gray-600 mb-1">
                    School Name
                  </label>
                  <input
                    {...register(`education.${index}.school`, {
                      required: "School name is required",
                    })}
                    className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm ${
                      errors.education?.[index]?.school
                        ? "border-red-500"
                        : "border-gray-300"
                    }`}
                    placeholder="e.g., Harvard University"
                  />
                  {errors.education?.[index]?.school && (
                    <span className="text-red-500 text-xs mt-1">
                      {errors.education[index]?.school?.message}
                    </span>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
        {/* SUBMIT BUTTON */}
        <button
          type="submit"
          disabled={isSubmitting}
          className={`w-full text-white font-semibold py-2 px-4 rounded-md transition-colors duration-300 ${
            isSubmitting
              ? "bg-gray-400 cursor-not-allowed"
              : "bg-blue-600 hover:bg-blue-700"
          }`}
        >
          {isSubmitting ? "Submitting..." : "Submit"}
        </button>
      </form>
    </div>
  );
};

export default FormPage;
