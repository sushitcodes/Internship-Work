import { useEffect } from "react";
import { useForm, useFieldArray, SubmitHandler } from "react-hook-form";
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
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";
import { getInitials } from "../../application/utils/getInitials";
import { resolveFileUrl } from "../../lib/resolveFileUrl";
import { toast } from "sonner";
import { X, Plus } from "lucide-react";

interface ProfileFormValues {
  address: string;
  gender: string;
  phoneNumbers: { value: string }[]; // useFieldArray needs objects, not raw strings
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
    control,
    watch,
    setValue,
    // formState: { errors },
  } = useForm<ProfileFormValues>({
    defaultValues: { address: "", gender: "", phoneNumbers: [] },
  });
  const gender = watch("gender");
  const { fields, append, remove } = useFieldArray({
    control,
    name: "phoneNumbers" as never,
  });

  useEffect(() => {
    if (profile) {
      reset({
        address: profile.address,
        gender: profile.gender ?? "",
        phoneNumbers: profile.phoneNumbers.map((p) => ({ value: p })),
      });
    }
  }, [profile, reset]);

  const onSubmit: SubmitHandler<ProfileFormValues> = async (data) => {
    const formData = new FormData();
    formData.append("address", data.address);
    if (data.gender) formData.append("gender", data.gender);
    // Same key repeated for each number — ASP.NET Core model binding
    // collects repeated form keys into the List<string> automatically.
    data.phoneNumbers.forEach((p) => {
      if (p.value.trim()) formData.append("phoneNumbers", p.value.trim());
    });
    if (data.avatar?.length) formData.append("avatar", data.avatar[0]);

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
              />
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

          {/* Full Name — READ-ONLY display now, same treatment as Email/Member No.
              No <Input>, no register() — there's no form field for it to submit. */}
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div>
              <span className="block text-muted-foreground">Full Name</span>
              <span className="font-medium">{profile?.fullName}</span>
            </div>
            <div>
              <span className="block text-muted-foreground">Email</span>
              <span className="font-medium">{profile?.email}</span>
            </div>
            <div>
              <span className="block text-muted-foreground">Member No.</span>
              <span className="font-medium">
                {profile && profile.memberNumber > 0
                  ? `#${profile.memberNumber}`
                  : "—"}
              </span>
            </div>
          </div>

          <div className="space-y-2">
            <Label>Gender</Label>
            <Select
              value={gender}
              onValueChange={(v) => setValue("gender", v ?? "")}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select gender" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Male">Male</SelectItem>
                <SelectItem value="Female">Female</SelectItem>
                <SelectItem value="Other">Other</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label htmlFor="address">Address</Label>
            <Input id="address" {...register("address")} />
          </div>

          <div className="space-y-2">
            <Label>Phone Numbers</Label>
            {fields.map((field, index) => (
              <div key={field.id} className="flex gap-2">
                <Input
                  {...register(`phoneNumbers.${index}.value` as const)}
                  placeholder="Phone number"
                />
                <Button
                  type="button"
                  variant="outline"
                  size="icon"
                  onClick={() => remove(index)}
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>
            ))}
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => append({ value: "" })}
            >
              <Plus className="h-4 w-4 mr-1" /> Add Phone Number
            </Button>
          </div>

          <Button type="submit" disabled={isSaving} className="w-full">
            {isSaving ? "Saving..." : "Save Changes"}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
};

export default MyProfilePage;
