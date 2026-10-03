// application/validators/formValidators.ts
// Validation rules are business rules -> they live in Application,
// not in Infrastructure (axios) or Presentation (React components).

export const nameValidation = {
  required: "Name is required",
  minLength: { value: 2, message: "Name must be at least 2 characters" },
  maxLength: { value: 50, message: "Name must be under 50 characters" },
};

export const emailValidation = {
  required: "Email is required",
  pattern: {
    value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
    message: "Enter a valid email address",
  },
};

// Frontend file check is a UX nicety only. The REAL validation
// (size, type, virus scan) happens on the backend. Never trust the browser.
const ALLOWED_EXTENSIONS = ["pdf", "jpg", "jpeg", "png"];
const MAX_FILE_BYTES = 5 * 1024 * 1024; // same 5 MB limit as the server

export const fileValidation = {
  required: "Please attach a file",
  validate: (files?: FileList) => {
    const file = files?.[0];
    if (!file) return true; // "required" already handles the empty case
    const ext = file.name.split(".").pop()?.toLowerCase() ?? "";
    if (!ALLOWED_EXTENSIONS.includes(ext))
      return "Only PDF, JPG or PNG files are allowed.";
    if (file.size > MAX_FILE_BYTES) return "File must be 5 MB or smaller.";
    return true;
  },
};

export const passwordValidation = {
  required: "Password is required",
  minLength: { value: 8, message: "Password must be at least 8 characters" },
};

export const loginPasswordValidation = {
  required: "Password is required",
};
