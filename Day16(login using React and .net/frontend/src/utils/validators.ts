export type PasswordValidationResult = {
  isValid: boolean;
  message: string;
};

export function validatePassword(password: string): PasswordValidationResult {
  if (password.length < 8) {
    return {
      isValid: false,
      message: "Password must be at least 8 characters.",
    };
  }

  if (password.length > 100) {
    return {
      isValid: false,
      message: "Password must be under 100 characters.",
    };
  }

  if (!/[A-Z]/.test(password)) {
    return {
      isValid: false,
      message: "Password must include at least one uppercase letter.",
    };
  }

  if (!/[a-z]/.test(password)) {
    return {
      isValid: false,
      message: "Password must include at least one lowercase letter.",
    };
  }

  if (!/[0-9]/.test(password)) {
    return {
      isValid: false,
      message: "Password must include at least one number.",
    };
  }

  if (!/[!@#$%^&*]/.test(password)) {
    return {
      isValid: false,
      message:
        "Password must include at least one special character (!@#$%^&*).",
    };
  }

  return { isValid: true, message: "" };
}
