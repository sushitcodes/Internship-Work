import { useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate, useSearchParams, Link } from "react-router-dom";
import { useResetPasswordMutation } from "../../infrastructure/api/authApi";
import { FormInput } from "../components/FormInput";
import { PasswordInput } from "../components/PasswordInput";
import {
  emailValidation,
  passwordValidation,
} from "../../application/validators/formValidators";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

interface ResetPasswordValues {
  email: string;
  code: string;
  newPassword: string;
}

const ResetPasswordPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [resetPassword, { isLoading }] = useResetPasswordMutation();
  const [serverError, setServerError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ResetPasswordValues>({
    defaultValues: { email: searchParams.get("email") ?? "" },
  });

  const onSubmit = async (data: ResetPasswordValues) => {
    setServerError(null);
    try {
      await resetPassword(data).unwrap();
      navigate("/login");
    } catch {
      setServerError("Invalid or expired code. Please try again.");
    }
  };

  return (
    <Card className="max-w-sm mx-auto mt-10">
      <CardHeader>
        <CardTitle className="text-center text-xl">Reset Password</CardTitle>
      </CardHeader>
      <CardContent>
        <form
          onSubmit={handleSubmit(onSubmit)}
          className="space-y-4"
          noValidate
        >
          <FormInput
            label="Email"
            type="email"
            registration={register("email", emailValidation)}
            error={errors.email}
          />
          <FormInput
            label="6-digit code"
            registration={register("code", {
              required: "Code is required",
              pattern: { value: /^\d{6}$/, message: "Enter the 6-digit code" },
            })}
            error={errors.code}
          />
          <PasswordInput
            label="New password"
            registration={register("newPassword", passwordValidation)}
            error={errors.newPassword}
          />
          {serverError && <p className="text-sm text-red-500">{serverError}</p>}
          <Button type="submit" disabled={isLoading} className="w-full">
            {isLoading ? "Resetting..." : "Reset Password"}
          </Button>
        </form>
        <p className="text-sm text-center mt-4 text-muted-foreground">
          <Link
            to="/forgot-password"
            className="underline text-red-500 hover:text-red-700"
          >
            Request a new code
          </Link>
        </p>
      </CardContent>
    </Card>
  );
};

export default ResetPasswordPage;
