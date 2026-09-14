import { useForm, SubmitHandler } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { useCreateUserMutation } from "../../infrastructure/api/userApi";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { FormInput } from "../components/FormInput";
import { emailValidation } from "../../application/validators/formValidators";
import { toast } from "sonner";
import { displayRoleName } from "../../lib/roleDisplay";
import { Paths } from "@/routes/paths";

// Matches the backend UserRole enum's real names exactly — sent as-is,
// only the LABEL shown to the Admin goes through displayRoleName.
const AVAILABLE_ROLES = ["Student", "Staff", "Admin"] as const;

interface FormValues {
  email: string;
  temporaryPassword: string;
  roles: string[];
}

const CreateUserPage: React.FC = () => {
  const navigate = useNavigate();
  const [createUser, { isLoading }] = useCreateUserMutation();

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors },
  } = useForm<FormValues>({ defaultValues: { roles: [] } });

  const selectedRoles = watch("roles");

  const toggleRole = (role: string) => {
    const next = selectedRoles.includes(role)
      ? selectedRoles.filter((r) => r !== role)
      : [...selectedRoles, role];
    setValue("roles", next);
  };

  const onSubmit: SubmitHandler<FormValues> = async (data) => {
    if (data.roles.length === 0) {
      toast.error("Select at least one role.");
      return;
    }
    try {
      await createUser({
        email: data.email,
        temporaryPassword: data.temporaryPassword,
        roles: data.roles,
      }).unwrap();
      toast.success("User created.");
      navigate(Paths.users);
    } catch (err) {
      console.error("Failed to create user:", err);
      toast.error(
        "Could not create user. Try with a different email address may already be in use.",
      );
    }
  };

  return (
    <Card className="max-w-md mx-auto mt-10">
      <CardHeader>
        <CardTitle>Add User</CardTitle>
      </CardHeader>
      <CardContent>
        <form
          onSubmit={handleSubmit(onSubmit)}
          noValidate
          className="space-y-6"
        >
          <FormInput
            id="email"
            label="Email"
            type="email"
            registration={register("email", emailValidation)}
            error={errors.email}
          />

          {/* No confirm-password field, no strength meter — this is a
              TEMPORARY password Admin sets and (presumably) relays directly
              to the person; the person can change it once logged in via
              MyProfilePage, so the same UX rigor as a real signup form isn't
              needed here. */}
          <FormInput
            id="temporaryPassword"
            label="Temporary Password"
            type="text"
            registration={register("temporaryPassword", {
              required: "Required",
            })}
            error={errors.temporaryPassword}
          />

          <div className="space-y-2">
            <Label>Roles</Label>
            <div className="flex flex-col gap-2">
              {AVAILABLE_ROLES.map((role) => (
                <label key={role} className="flex items-center gap-2 text-sm">
                  <Checkbox
                    checked={selectedRoles.includes(role)}
                    onCheckedChange={() => toggleRole(role)}
                  />
                  {displayRoleName(role)}
                </label>
              ))}
            </div>
          </div>

          <Button type="submit" disabled={isLoading} className="w-full">
            {isLoading ? "Creating..." : "Create User"}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
};

export default CreateUserPage;
