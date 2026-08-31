import { useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate, Link } from "react-router-dom";
import { useForgotPasswordMutation } from "../../infrastructure/api/authApi";
import { FormInput } from "../components/FormInput";
import { emailValidation } from "../../application/validators/formValidators";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

interface ForgotPasswordValues {
  email: string;
}

const ForgotPasswordPage: React.FC = () => {
  const navigate = useNavigate();
  const [forgotPassword, { isLoading }] = useForgotPasswordMutation();
  const [submitted, setSubmitted] = useState(false);
  const {
    register,
    handleSubmit,
    getValues,
    formState: { errors },
  } = useForm<ForgotPasswordValues>();

  const onSubmit = async (data: ForgotPasswordValues) => {
    await forgotPassword(data);
    // Always show the same confirmation, regardless of what the backend
    // actually did internally — matches the "don't reveal which emails
    // are registered" principle from the backend itself.
    setSubmitted(true);
  };

  if (submitted) {
    return (
      <Card className="max-w-sm mx-auto mt-10">
        <CardHeader>
          <CardTitle className="text-center text-xl">
            Check your email
          </CardTitle>
        </CardHeader>
        <CardContent className="text-center space-y-4">
          <p className="text-sm text-muted-foreground">
            If an account exists for that email, a 6-digit code has been sent.
          </p>
          <Button
            className="w-full"
            onClick={() =>
              navigate(
                `/reset-password?email=${encodeURIComponent(getValues("email"))}`,
              )
            }
          >
            I have my code
          </Button>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="max-w-sm mx-auto mt-10">
      <CardHeader>
        <CardTitle className="text-center text-xl">Forgot Password</CardTitle>
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
          <Button type="submit" disabled={isLoading} className="w-full">
            {isLoading ? "Sending..." : "Send reset code"}
          </Button>
        </form>
        <p className="text-sm text-center mt-4 text-muted-foreground">
          Remembered it?{" "}
          <Link
            to="/login"
            className="underline text-red-500 hover:text-red-700"
          >
            Log In
          </Link>
        </p>
      </CardContent>
    </Card>
  );
};

export default ForgotPasswordPage;
