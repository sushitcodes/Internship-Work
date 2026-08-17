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

export const phoneValidation = {
  required: "Phone number is required",
  pattern: {
    value: /^[0-9]{10}$/,
    message: "Phone number must be 10 digits",
  },
};

export const institutionValidation = {
  required: "Institution name is required",
};

export const degreeValidation = {
  required: "Degree is required",
};

export const yearValidation = {
  required: "Year is required",
  min: { value: 1950, message: "Enter a valid year" },
  max: { value: new Date().getFullYear(), message: "Year cannot be in the future" },
};

// Frontend file check is a UX nicety only — the REAL validation
// (size, type, virus scan) happens on the backend. Never trust the browser.
export const fileValidation = {
  required: "Please attach a file",
};
