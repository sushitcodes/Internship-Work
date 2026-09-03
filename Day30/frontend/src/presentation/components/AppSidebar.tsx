import { Link, useNavigate } from "react-router-dom";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarFooter,
  SidebarHeader,
} from "@/components/ui/sidebar";
import { Button } from "@/components/ui/button";
import { Home, GraduationCap, ClipboardCheck, LogOut } from "lucide-react";
import { useAppSelector } from "../../infrastructure/store/hooks";
import { useLogoutUserMutation } from "../../infrastructure/api/authApi";
export function AppSidebar() {
  const email = useAppSelector((state) => state.auth.email);
  const roles = useAppSelector((state) => state.auth.roles);
  const [logoutUser] = useLogoutUserMutation();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logoutUser();
    navigate("/login");
  };
  // one place to add/remove a link, and it's trivial to loop + filter by role.
  const navItems = [
    { to: "/", label: "Submissions", icon: Home, show: true },
    {
      to: "/classes",
      label: "Classes",
      icon: GraduationCap,
      show: roles.includes("Admin"),
    },
    {
      to: "/attendance/mark",
      label: "Attendance",
      icon: ClipboardCheck,
      show: roles.includes("Staff") || roles.includes("Admin"),
    },
  ];

  return (
    <Sidebar collapsible="icon">
      <SidebarHeader>
        <Link to="/" className="font-semibold text-lg px-2 py-1">
          Student Submission
        </Link>
      </SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>Menu</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {navItems
                .filter((item) => item.show)
                .map((item) => (
                  <SidebarMenuItem key={item.to}>
                    render={<Link to={item.to} />}
                    <SidebarMenuButton
                      render={<Link to={item.to} />}
                      tooltip={item.label}
                    >
                      <Link to={item.to}>
                        <item.icon />
                        <span>{item.label}</span>
                      </Link>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter>
        {email ? (
          <div className="flex flex-col gap-2 px-2 py-1">
            <span className="text-sm text-muted-foreground truncate">
              {email}
            </span>
            <Button variant="outline" size="sm" onClick={handleLogout}>
              <LogOut className="mr-1 h-4 w-4" /> Log Out
            </Button>
          </div>
        ) : (
          <div className="flex flex-col gap-2 px-2 py-1">
            <Link to="/login">
              <Button variant="outline" size="sm" className="w-full">
                Log In
              </Button>
            </Link>
            <Link to="/register">
              <Button size="sm" className="w-full">
                Register
              </Button>
            </Link>
          </div>
        )}
      </SidebarFooter>
    </Sidebar>
  );
}
