import React, { useState } from "react";
import { useParams, Link } from "react-router-dom";
import {
  useGetUserProfileByIdQuery,
  useUpdateUserNameMutation,
  useSetUserActiveStatusMutation,
} from "../../infrastructure/api/userApi";
import { useAppSelector } from "../../infrastructure/store/hooks";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { PageHeader } from "../components/PageHeader";
import { getInitials } from "../../application/utils/getInitials";
import { resolveFileUrl } from "../../lib/resolveFileUrl";
import { Paths } from "../../routes/paths";
import { toast } from "sonner";
import {
  ArrowLeft,
  Mail,
  Phone,
  MapPin,
  User,
  Copy,
  Check,
  Pencil,
  X,
  ShieldAlert,
} from "lucide-react";

const UserDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const roles = useAppSelector((state) => state.auth.roles);
  const isAdmin = roles.includes("Admin");
  const isStaffOrAdmin = roles.includes("Staff") || isAdmin;

  const {
    data: profile,
    isLoading,
    isError,
  } = useGetUserProfileByIdQuery(id!, { skip: !id });

  const [updateName, { isLoading: isSavingName }] = useUpdateUserNameMutation();
  const [setActiveStatus, { isLoading: isUpdatingStatus }] =
    useSetUserActiveStatusMutation();

  // Inline editing for user name
  const [isEditingName, setIsEditingName] = useState(false);
  const [nameInput, setNameInput] = useState("");
  const [copiedField, setCopiedField] = useState<string | null>(null);

  const handleCopy = (text: string, field: string) => {
    navigator.clipboard.writeText(text);
    setCopiedField(field);
    toast.success(`Copied ${field} to clipboard.`);
    setTimeout(() => setCopiedField(null), 2000);
  };

  const handleSaveName = async () => {
    if (!nameInput.trim()) {
      toast.error("Name cannot be empty.");
      return;
    }
    try {
      await updateName({ id: id!, fullName: nameInput.trim() }).unwrap();
      toast.success("Name updated successfully.");
      setIsEditingName(false);
    } catch {
      toast.error("Could not update name.");
    }
  };

  const handleToggleStatus = async () => {
    if (!profile) return;
    try {
      await setActiveStatus({
        id: profile.userId,
        isActive: !profile.isActive,
      }).unwrap();
      toast.success(
        profile.isActive ? "User has been deactivated." : "User reactivated.",
      );
    } catch {
      toast.error("Could not update user status.");
    }
  };

  if (isLoading) {
    return (
      <div className="max-w-4xl mx-auto space-y-6">
        <Skeleton className="h-10 w-48" />
        <Card className="p-6">
          <div className="flex items-center gap-4">
            <Skeleton className="h-20 w-20 rounded-full" />
            <div className="space-y-2">
              <Skeleton className="h-6 w-48" />
              <Skeleton className="h-4 w-32" />
            </div>
          </div>
        </Card>
      </div>
    );
  }

  if (isError || !profile) {
    return (
      <div className="max-w-4xl mx-auto py-12 text-center">
        <div className="w-12 h-12 rounded-full bg-destructive/10 text-destructive flex items-center justify-center mx-auto mb-3">
          <ShieldAlert className="w-6 h-6" />
        </div>
        <h2 className="text-lg font-bold text-foreground">User Not Found</h2>
        <p className="text-sm text-muted-foreground mt-1">
          The requested user profile does not exist or has been removed.
        </p>
        <Link to={Paths.users} className="inline-block mt-4">
          <Button variant="outline" size="sm">
            <ArrowLeft className="h-4 w-4 mr-1" />
            Back to Users
          </Button>
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto space-y-6 pb-12">
      {/* 1. Header with Back Button */}
      <PageHeader
        title="User Profile"
        description="Detailed personal, contact, and account information."
      >
        <Link to={Paths.users}>
          <Button variant="outline" size="sm" className="gap-2">
            <ArrowLeft className="h-4 w-4" />
            Back to Users
          </Button>
        </Link>
      </PageHeader>

      {/* 2. Hero Profile Card */}
      <Card className="border-border/70 overflow-hidden shadow-xs">
        <CardContent className="p-6">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-6">
            <div className="flex items-center gap-5">
              <div className="relative">
                <Avatar className="h-20 w-20 border-2 border-background shadow-xs">
                  <AvatarImage
                    src={resolveFileUrl(profile.avatarUrl)}
                    alt={profile.fullName}
                  />
                  <AvatarFallback className="text-xl font-bold bg-primary/10 text-primary">
                    {getInitials(profile.fullName)}
                  </AvatarFallback>
                </Avatar>
                {/* Status Dot */}
                <span
                  className={`absolute bottom-0 right-0 w-5 h-5 rounded-full border-2 border-background flex items-center justify-center ${
                    profile.isActive ? "bg-emerald-500" : "bg-rose-500"
                  }`}
                  title={profile.isActive ? "Active Account" : "Deactivated"}
                />
              </div>

              <div className="space-y-1">
                {/* Full Name / Inline Edit */}
                {isEditingName ? (
                  <div className="flex items-center gap-2">
                    <Input
                      value={nameInput}
                      onChange={(e) => setNameInput(e.target.value)}
                      className="h-8 text-base font-semibold w-60"
                      autoFocus
                    />
                    <Button
                      size="sm"
                      onClick={handleSaveName}
                      disabled={isSavingName}
                      className="h-8 px-2"
                    >
                      <Check className="h-4 w-4" />
                    </Button>
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => setIsEditingName(false)}
                      className="h-8 px-2"
                    >
                      <X className="h-4 w-4" />
                    </Button>
                  </div>
                ) : (
                  <div className="flex items-center gap-2">
                    <h2 className="text-xl font-bold text-foreground">
                      {profile.fullName}
                    </h2>
                    {isStaffOrAdmin && (
                      <button
                        onClick={() => {
                          setNameInput(profile.fullName);
                          setIsEditingName(true);
                        }}
                        className="text-muted-foreground hover:text-foreground transition-colors p-1"
                        title="Edit Full Name"
                      >
                        <Pencil className="h-3.5 w-3.5" />
                      </button>
                    )}
                  </div>
                )}

                <p className="text-sm text-muted-foreground flex items-center gap-1.5">
                  <Mail className="h-3.5 w-3.5" />
                  {profile.email}
                </p>

                <div className="flex items-center gap-2 pt-1">
                  {profile.memberNumber > 0 && (
                    <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-semibold bg-secondary text-secondary-foreground border">
                      Roll #{profile.memberNumber}
                    </span>
                  )}
                  <span
                    className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium border ${
                      profile.isActive
                        ? "bg-emerald-500/10 text-emerald-700 dark:text-emerald-400 border-emerald-500/20"
                        : "bg-rose-500/10 text-rose-700 dark:text-rose-400 border-rose-500/20"
                    }`}
                  >
                    {profile.isActive ? "Active Account" : "Deactivated"}
                  </span>
                </div>
              </div>
            </div>

            {/* Quick Actions (Admin Toggle Status) using render prop */}
            {isAdmin && (
              <div className="flex sm:flex-col items-end gap-2 shrink-0">
                <AlertDialog>
                  <AlertDialogTrigger
                    render={
                      <Button
                        variant={profile.isActive ? "destructive" : "outline"}
                        size="sm"
                        disabled={isUpdatingStatus}
                      >
                        {profile.isActive ? "Deactivate User" : "Activate User"}
                      </Button>
                    }
                  />
                  <AlertDialogContent>
                    <AlertDialogHeader>
                      <AlertDialogTitle>
                        {profile.isActive
                          ? "Deactivate this user?"
                          : "Activate this user?"}
                      </AlertDialogTitle>
                      <AlertDialogDescription>
                        {profile.isActive
                          ? "Deactivating this account will immediately revoke all active sessions and prevent the user from logging in."
                          : "Reactivating this account will allow the user to log in and use the portal again."}
                      </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                      <AlertDialogCancel>Cancel</AlertDialogCancel>
                      <AlertDialogAction onClick={handleToggleStatus}>
                        Confirm
                      </AlertDialogAction>
                    </AlertDialogFooter>
                  </AlertDialogContent>
                </AlertDialog>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* 3. Detailed Information Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
        {/* Personal Details */}
        <Card className="border-border/70 shadow-xs">
          <CardHeader className="pb-3 border-b bg-muted/20">
            <CardTitle className="text-base font-semibold flex items-center gap-2">
              <User className="h-4 w-4 text-primary" />
              Personal Information
            </CardTitle>
          </CardHeader>
          <CardContent className="pt-4 space-y-4 text-sm">
            <div>
              <span className="text-xs text-muted-foreground uppercase tracking-wider block font-medium">
                Full Name
              </span>
              <span className="font-medium text-foreground">
                {profile.fullName || "—"}
              </span>
            </div>

            <div>
              <span className="text-xs text-muted-foreground uppercase tracking-wider block font-medium">
                Gender
              </span>
              <span className="font-medium text-foreground">
                {profile.gender || "Not specified"}
              </span>
            </div>

            <div>
              <span className="text-xs text-muted-foreground uppercase tracking-wider block font-medium">
                Roll / Member Number
              </span>
              <span className="font-medium text-foreground">
                {profile.memberNumber > 0 ? `#${profile.memberNumber}` : "—"}
              </span>
            </div>
          </CardContent>
        </Card>

        {/* Contact & Location Details */}
        <Card className="border-border/70 shadow-xs">
          <CardHeader className="pb-3 border-b bg-muted/20">
            <CardTitle className="text-base font-semibold flex items-center gap-2">
              <Phone className="h-4 w-4 text-primary" />
              Contact & Location
            </CardTitle>
          </CardHeader>
          <CardContent className="pt-4 space-y-4 text-sm">
            {/* Email */}
            <div>
              <span className="text-xs text-muted-foreground uppercase tracking-wider block font-medium">
                Email Address
              </span>
              <div className="flex items-center gap-2 mt-0.5">
                <a
                  href={`mailto:${profile.email}`}
                  className="font-medium text-primary hover:underline truncate"
                >
                  {profile.email}
                </a>
                <button
                  onClick={() => handleCopy(profile.email, "email")}
                  className="text-muted-foreground hover:text-foreground p-1 transition-colors"
                  title="Copy Email"
                >
                  {copiedField === "email" ? (
                    <Check className="h-3.5 w-3.5 text-emerald-500" />
                  ) : (
                    <Copy className="h-3.5 w-3.5" />
                  )}
                </button>
              </div>
            </div>

            {/* Phone Numbers */}
            <div>
              <span className="text-xs text-muted-foreground uppercase tracking-wider block font-medium">
                Phone Number(s)
              </span>
              {profile.phoneNumbers && profile.phoneNumbers.length > 0 ? (
                <div className="flex flex-wrap gap-2 mt-1">
                  {profile.phoneNumbers.map((phone, idx) => (
                    <a
                      key={idx}
                      href={`tel:${phone}`}
                      className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md bg-secondary text-secondary-foreground hover:bg-secondary/80 font-medium text-xs border"
                    >
                      <Phone className="h-3 w-3 text-muted-foreground" />
                      {phone}
                    </a>
                  ))}
                </div>
              ) : (
                <span className="text-muted-foreground">
                  No phone numbers registered
                </span>
              )}
            </div>

            {/* Address */}
            <div>
              <span className="text-xs text-muted-foreground uppercase tracking-wider block font-medium">
                Residential Address
              </span>
              <div className="flex items-start gap-1.5 mt-0.5">
                <MapPin className="h-4 w-4 text-muted-foreground shrink-0 mt-0.5" />
                <span className="font-medium text-foreground">
                  {profile.address || "No address provided"}
                </span>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
};

export default UserDetailPage;
