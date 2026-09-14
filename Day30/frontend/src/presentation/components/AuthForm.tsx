import { useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate, Link } from "react-router-dom";
import { useLoginMutation } from "../../infrastructure/api/authApi";
import { FormInput } from "./FormInput";
import { PasswordInput } from "./PasswordInput";
import {
  emailValidation,
  loginPasswordValidation,
} from "../../application/validators/formValidators";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import { Paths } from "../../routes/paths";

interface AuthFormValues {
  email: string;
  password: string;
}

// (passwordValidation, used only by the register branch, is no longer imported.)
export function AuthForm() {
  const navigate = useNavigate();
  const [login, { isLoading }] = useLoginMutation();
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<AuthFormValues>();

  const onSubmit = async (data: AuthFormValues) => {
    setServerError(null);
    try {
      await login(data).unwrap();
      navigate(Paths.dashboard);
      toast.success("Logged in successfully.");
    } catch {
      setServerError("Invalid email or password.");
    }
  };

  return (
    <Card className="max-w-sm mx-auto mt-10">
      <CardHeader>
        <CardTitle className="text-center text-xl">Log In</CardTitle>
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
          <PasswordInput
            label="Password"
            registration={register("password", loginPasswordValidation)}
            error={errors.password}
          />
          {serverError && <p className="text-sm text-red-500">{serverError}</p>}

          <Button type="submit" disabled={isLoading} className="w-full">
            {isLoading ? "Logging in..." : "Log In"}
          </Button>
        </form>

        <div className="mt-4 text-center">
          <Link
            to={Paths.forgotPassword}
            className="text-sm underline text-red-500 hover:text-red-700"
          >
            Forgot password?
          </Link>
        </div>
      </CardContent>
    </Card>
  );
}
