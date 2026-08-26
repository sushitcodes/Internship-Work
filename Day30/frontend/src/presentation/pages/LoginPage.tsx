import { useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate, Link } from "react-router-dom";
import { useLoginMutation } from "../../infrastructure/api/authApi";
import { FormInput } from "../components/FormInput";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import {
  emailValidation,
  loginPasswordValidation,
} from "../../application/validators/formValidators";
interface LoginFormValues {
  email: string;
  password: string;
}
const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const [login, { isLoading }] = useLoginMutation();
  const [serverError, setServerError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>();
  const onSubmit = async (data: LoginFormValues) => {
    setServerError(null);
    try {
      await login(data).unwrap();
      navigate("/"); // token is now in Redux (authApi's onQueryStarted already stored it)
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
          <FormInput
            label="Password"
            type="password"
            registration={register("password", loginPasswordValidation)}
            error={errors.password}
          />
          {serverError && <p className="text-sm text-red-500">{serverError}</p>}
          <Button type="submit" disabled={isLoading} className="w-full">
            {isLoading ? "Logging in..." : "Log In"}
          </Button>
        </form>
        <p className="text-sm text-center mt-4 text-muted-foreground">
          Don&apos;t have an account?{" "}
          <Link to="/register" className="underline">
            Register
          </Link>
        </p>
      </CardContent>
    </Card>
  );
};

export default LoginPage;
