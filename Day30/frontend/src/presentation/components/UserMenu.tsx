import { Link, useNavigate } from "react-router-dom";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Paths } from "../../routes/paths";

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { useAppSelector } from "../../infrastructure/store/hooks";
import { useLogoutUserMutation } from "../../infrastructure/api/authApi";
import { useGetOwnProfileQuery } from "../../infrastructure/api/userApi";
import { getInitials } from "../../application/utils/getInitials";
import { resolveFileUrl } from "../../lib/resolveFileUrl";
import { toast } from "sonner";

export function UserMenu() {
  const email = useAppSelector((state) => state.auth.email);
  const navigate = useNavigate();
  const [logoutUser] = useLogoutUserMutation();

  // Skip the query entirely when logged out — no point calling
  // /users/me/profile with no valid session, it'd just 401.
  const { data: profile } = useGetOwnProfileQuery(undefined, { skip: !email });

  if (!email) return null; // header shows nothing for logged-out visitors

  const handleLogout = async () => {
    await logoutUser();
    toast.success("Logged out.");
    navigate(Paths.login);
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        render={
          <button className="rounded-full">
            <Avatar className="h-8 w-8 cursor-pointer">
              <AvatarImage
                src={resolveFileUrl(profile?.avatarUrl)}
                alt={profile?.fullName}
              />
              <AvatarFallback className="text-xs">
                {profile ? getInitials(profile.fullName) : "?"}
              </AvatarFallback>
            </Avatar>
          </button>
        }
      />
      <DropdownMenuContent align="end" className="w-56">
        {/* Plain div instead of DropdownMenuLabel — sidesteps the Menu.Group
      requirement entirely instead of adding a wrapper we don't otherwise need. */}
        <div className="px-2 py-1.5 flex flex-col">
          <span className="font-medium text-sm">
            {profile?.fullName ?? email}
          </span>
          <span className="text-xs text-muted-foreground">{email}</span>
        </div>
        <DropdownMenuSeparator />
        <DropdownMenuItem render={<Link to={Paths.profile}>My Profile</Link>} />
        <DropdownMenuItem onClick={handleLogout}>Log Out</DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
