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
import { useEffect, useState } from "react";
import { Sun, Moon } from "lucide-react";

export function UserMenu() {
  const email = useAppSelector((state) => state.auth.email);
  const navigate = useNavigate();
  const [logoutUser] = useLogoutUserMutation();
  const [dark, setDark] = useState(() => {
    const saved = localStorage.getItem("theme");
    if (saved) return saved === "dark";
    return window.matchMedia("(prefers-color-scheme: dark)").matches;
  }); // Skip the query entirely when logged out — no point calling
  // /users/me/profile with no valid session, it'd just 401.

  useEffect(() => {
    document.documentElement.classList.toggle("dark", dark);
    localStorage.setItem("theme", dark ? "dark" : "light");
  }, [dark]);
  const { data: profile } = useGetOwnProfileQuery(undefined, {
    skip: !email,
    refetchOnMountOrArgChange: true,
  });

  if (!email) return null; // header shows nothing for logged-out visitors

  const handleLogout = async () => {
    await logoutUser();
    toast.success("Logged out.");
    navigate(Paths.login);
  };
  // const toggleTheme = () => {
  //   const next = !dark;
  //   setDark(next);
  //   console.log("toggled, now dark =", next);
  // };

  return (
    <>
      <div className="flex flex-end gap-4">
        <button
          onClick={() => setDark((d) => !d)}
          aria-label="Toggle theme"
          className="rounded-md p-2 text-foreground hover:bg-accent"
        >
          {dark ? <Moon /> : <Sun />}
        </button>

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
            <DropdownMenuItem
              render={<Link to={Paths.profile}>My Profile</Link>}
            />
            <DropdownMenuItem onClick={handleLogout}>Log Out</DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </>
  );
}
