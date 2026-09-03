import { useEffect } from "react";
import { useForm, SubmitHandler } from "react-hook-form";
import {
  useGetOwnProfileQuery,
  useUpdateOwnProfileMutation,
} from "../../infrastructure/api/userApi";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { FormInput } from "../components/FormInput";
import { getInitials } from "../../application/utils/getInitials";
import { resolveFileUrl } from "../../lib/resolveFileUrl";
import {
  nameValidation,
  phoneValidation,
} from "../../application/validators/formValidators";
import { toast } from "sonner";

interface ProfileFormValues {
  fullName: string;
  phone: string;
  avatar?: FileList;
}

const MyProfilePage: React.FC = () => {
  const { data: profile, isLoading } = useGetOwnProfileQuery();
  const [updateProfile, { isLoading: isSaving }] =
    useUpdateOwnProfileMutation();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ProfileFormValues>({
    defaultValues: { fullName: "", phone: "" },
  });

  // Same pattern as FormPage's edit-mode reset — populate the form only
  // once real data arrives, not on every render.
  useEffect(() => {
    if (profile) {
      reset({ fullName: profile.fullName, phone: profile.phone });
    }
  }, [profile, reset]);

  const onSubmit: SubmitHandler<ProfileFormValues> = async (data) => {
    const formData = new FormData();
    formData.append("fullName", data.fullName);
    formData.append("phone", data.phone);
    if (data.avatar && data.avatar.length > 0) {
      formData.append("avatar", data.avatar[0]);
    }

    try {
      await updateProfile(formData).unwrap();
      toast.success("Profile updated successfully.");
    } catch (err) {
      console.error("Failed to update profile:", err);
      toast.error("Could not save your profile. Please try again.");
    }
  };

  if (isLoading) {
    return (
      <Card className="max-w-xl mx-auto mt-10">
        <CardHeader>
          <CardTitle>My Profile</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <Skeleton className="h-20 w-20 rounded-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="max-w-xl mx-auto mt-10">
      <CardHeader>
        <CardTitle>My Profile</CardTitle>
      </CardHeader>
      <CardContent>
        <form
          onSubmit={handleSubmit(onSubmit)}
          noValidate
          className="space-y-6"
        >
          <div className="flex items-center gap-4">
            <Avatar className="h-20 w-20">
              <AvatarImage
                src={resolveFileUrl(profile?.avatarUrl)}
                alt={profile?.fullName}
              />{" "}
              <AvatarFallback className="text-lg">
                {profile ? getInitials(profile.fullName) : "?"}
              </AvatarFallback>
            </Avatar>
            <div>
              <Label htmlFor="avatar">Change photo</Label>
              <Input
                id="avatar"
                type="file"
                accept="image/*"
                {...register("avatar")}
              />
            </div>
          </div>

          {/* Email and member number are read-only display — not form fields,
              since neither can be edited from this endpoint (email is identity,
              MemberNumber is DB-assigned). Showing them as plain text avoids
              implying they're editable. */}
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div>
              <span className="block text-muted-foreground">Email</span>
              <span className="font-medium">{profile?.email}</span>
            </div>
            <div>
              <span className="block text-muted-foreground">Member No.</span>
              <span className="font-medium">#{profile?.memberNumber}</span>
            </div>
          </div>

          <FormInput
            id="fullName"
            label="Full Name"
            registration={register("fullName", nameValidation)}
            error={errors.fullName}
          />
          <FormInput
            id="phone"
            label="Phone"
            registration={register("phone", phoneValidation)}
            error={errors.phone}
          />

          <Button type="submit" disabled={isSaving} className="w-full">
            {isSaving ? "Saving..." : "Save Changes"}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
};

export default MyProfilePage;
