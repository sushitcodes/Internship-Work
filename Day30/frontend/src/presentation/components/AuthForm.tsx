import { useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate, Link } from "react-router-dom";
import {
  useLoginMutation,
  useRegisterMutation,
} from "../../infrastructure/api/authApi";
import { FormInput } from "./FormInput";
import { PasswordInput } from "./PasswordInput";
import {
  emailValidation,
  passwordValidation,
  loginPasswordValidation,
} from "../../application/validators/formValidators";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

interface AuthFormValues {
  email: string;
  password: string;
}

interface AuthFormProps {
  mode: "login" | "register";
}

export function AuthForm({ mode }: AuthFormProps) {
  const isLogin = mode === "login";
  const navigate = useNavigate();
  const [login, { isLoading: isLoggingIn }] = useLoginMutation();
  const [registerUser, { isLoading: isRegistering }] = useRegisterMutation();
  const isLoading = isLogin ? isLoggingIn : isRegistering;
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<AuthFormValues>();

  const onSubmit = async (data: AuthFormValues) => {
    setServerError(null);
    try {
      if (isLogin) {
        await login(data).unwrap();
      } else {
        await registerUser(data).unwrap();
      }
      navigate("/");
    } catch (err: any) {
      setServerError(
        isLogin
          ? "Invalid email or password."
          : (err?.data ?? "Could not create account."),
      );
    }
  };

  return (
    <Card className="max-w-sm mx-auto mt-10">
      <CardHeader>
        <CardTitle className="text-center text-xl">
          {isLogin ? "Log In" : "Create Account"}
        </CardTitle>
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
            registration={register(
              "password",
              isLogin ? loginPasswordValidation : passwordValidation,
            )}
            error={errors.password}
          />
          {serverError && <p className="text-sm text-red-500">{serverError}</p>}
          <Button type="submit" disabled={isLoading} className="w-full">
            {isLoading
              ? isLogin
                ? "Logging in..."
                : "Creating account..."
              : isLogin
                ? "Log In"
                : "Register"}
          </Button>
        </form>
        <p className="text-sm text-center mt-4 text-muted-foreground">
          {isLogin ? (
            <>
              Don&apos;t have an account?{" "}
              <Link to="/register" className="underline">
                Register
              </Link>
            </>
          ) : (
            <>
              Already have an account?{" "}
              <Link to="/login" className="underline">
                Log In
              </Link>
            </>
          )}
        </p>
      </CardContent>
    </Card>
  );
}
