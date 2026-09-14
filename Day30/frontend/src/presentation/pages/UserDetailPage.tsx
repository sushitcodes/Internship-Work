import { useParams, Link } from "react-router-dom";
import { useGetUserProfileByIdQuery } from "../../infrastructure/api/userApi";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { getInitials } from "../../application/utils/getInitials";
import { resolveFileUrl } from "../../lib/resolveFileUrl";
import { Paths } from "../../routes/paths";

import { ArrowLeft } from "lucide-react";

const UserDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const {
    data: profile,
    isLoading,
    isError,
  } = useGetUserProfileByIdQuery(id!, {
    skip: !id,
  });

  if (isLoading) {
    return (
      <Card className="max-w-xl mx-auto mt-10">
        <CardHeader>
          <CardTitle>User Profile</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <Skeleton className="h-20 w-20 rounded-full" />
          <Skeleton className="h-6 w-48" />
          <Skeleton className="h-6 w-64" />
        </CardContent>
      </Card>
    );
  }

  if (isError || !profile) {
    return (
      <Card className="max-w-xl mx-auto mt-10">
        <CardContent className="pt-6">
          <p className="text-red-500 text-center">Could not load this user.</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="max-w-xl mx-auto mt-10">
      <CardHeader>
        <CardTitle>User Profile</CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="flex items-center gap-4">
          <Avatar className="h-20 w-20">
            <AvatarImage
              src={resolveFileUrl(profile.avatarUrl)}
              alt={profile.fullName}
            />
            <AvatarFallback className="text-lg">
              {getInitials(profile.fullName)}
            </AvatarFallback>
          </Avatar>
          <div>
            <p className="text-lg font-semibold">{profile.fullName}</p>
            <p className="text-sm text-muted-foreground">{profile.email}</p>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4 text-sm">
          <div>
            <span className="block text-muted-foreground">Phone</span>
            <span className="font-medium">
              {profile.phoneNumbers[0] || "—"}
            </span>{" "}
          </div>
          <div>
            <span className="block text-muted-foreground">Member No.</span>
            <span className="font-medium">
              {profile.memberNumber > 0 ? `#${profile.memberNumber}` : "—"}
            </span>
          </div>
        </div>

        {/* Read-only for now — editing another person's profile isn't built
            yet (flagged separately). This page only VIEWS, deliberately. */}

        <div className="pt-4 border-t">
          <Link
            to={Paths.users}
            className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors"
          >
            <ArrowLeft className="h-4 w-4" />
            Back to Users
          </Link>
        </div>
      </CardContent>
    </Card>
  );
};

export default UserDetailPage;
