import { Link, useLocation, useNavigate } from "react-router-dom";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem,
  SidebarHeader,
  SidebarFooter,
  useSidebar,
} from "@/components/ui/sidebar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Button } from "@/components/ui/button";
import {
  ClipboardCheck,
  LogOut,
  ChevronDown,
  ChevronRight,
  UserCircle,
  Users,
  GraduationCap,
  LayoutDashboard,
  FileText,
  Award,
  BookOpenCheck,
} from "lucide-react";
import { useAppSelector } from "../../infrastructure/store/hooks";
import { useLogoutUserMutation } from "../../infrastructure/api/authApi";
import { useState } from "react";
import { toast } from "sonner";
import { Paths } from "../../routes/paths";
// Kept for when a section genuinely has multiple real children (Phase 1+).
// Today nothing does yet, so no item below actually uses this — but the
// rendering code supports it so we don't have to rewrite this file again
// when "Enrollments" or "Reports" become real pages.
interface NavSubItem {
  to: string;
  label: string;
}

interface NavItem {
  to?: string;
  label: string;
  icon: React.ElementType;
  show: boolean;
  subItems?: NavSubItem[];
}

export function AppSidebar() {
  const email = useAppSelector((state) => state.auth.email);
  const roles = useAppSelector((state) => state.auth.roles);
  const [logoutUser] = useLogoutUserMutation();
  const navigate = useNavigate();
  const location = useLocation();

  // Single source of truth for collapsed/expanded — this comes straight from
  // SidebarProvider, which already tracks the real breakpoint and persists
  // the choice in a cookie. We do NOT keep a second windowWidth/localStorage
  // copy of this — that was the bug in the last version (two systems could
  // disagree about whether the sidebar is collapsed).
  const { state, isMobile } = useSidebar();
  const isIconMode = state === "collapsed" && !isMobile;

  // Per-item expand/collapse for sub-menus, only relevant in expanded mode.
  // This is legitimately separate from `state` above — it's "which section
  // is open," not "is the sidebar itself collapsed" — so it's fine to be
  // its own local state instead of reusing the library's.
  const [openSections, setOpenSections] = useState<Set<string>>(new Set());
  const toggleSection = (label: string) => {
    setOpenSections((prev) => {
      const next = new Set(prev);
      next.has(label) ? next.delete(label) : next.add(label);
      return next;
    });
  };

  const handleLogout = async () => {
    await logoutUser();
    navigate(Paths.login);
    toast.success("Logged out.");
  };

  // Only routes that exist in App.tsx today. Add more here the same day
  // the real page + route lands — never point a nav link at a dead route.
  const navItems: NavItem[] = [
    {
      to: Paths.dashboard,
      label: "Dashboard",
      icon: LayoutDashboard,
      show: true,
    },
    { to: Paths.submissions, label: "Submissions", icon: FileText, show: true },
    {
      to: Paths.classes,
      label: "Classes",
      icon: GraduationCap,
      show: roles.includes("Admin"),
    },
    {
      label: "Attendance",
      icon: ClipboardCheck,
      show: roles.includes("Staff") || roles.includes("Admin"),
      subItems: [
        { to: Paths.markAttendance, label: "Mark Attendance" },
        { to: Paths.attendanceSheet, label: "Attendance Sheet" },
      ],
    },
    { to: Paths.profile, label: "My Profile", icon: UserCircle, show: !!email },
    {
      to: Paths.users,
      label: "Users",
      icon: Users,
      show: roles.includes("Staff") || roles.includes("Admin"),
    },
    {
      to: Paths.reportCard,
      label: "My Report Card",
      icon: Award,
      show: !!email,
    },
    {
      label: "Grades",
      icon: BookOpenCheck,
      show: roles.includes("Staff") || roles.includes("Admin"),
      subItems: [{ to: Paths.gradesEnter, label: "Enter Grades" }],
    },
  ];

  const visibleItems = navItems.filter((item) => item.show);

  const isActive = (item: NavItem) =>
    (item.to && location.pathname === item.to) ||
    (item.subItems?.some((s) => location.pathname === s.to) ?? false);

  // One function that renders a sub-item list, reused by both the expanded
  // in-place view and the collapsed flyout — instead of three copy-pasted
  // blocks that can drift out of sync.
  const renderSubItems = (subItems: NavSubItem[]) => (
    <SidebarMenuSub>
      {subItems.map((sub) => (
        <SidebarMenuSubItem key={sub.to}>
          <SidebarMenuSubButton
            render={<Link to={sub.to} />}
            isActive={location.pathname === sub.to}
          >
            {sub.label}
          </SidebarMenuSubButton>
        </SidebarMenuSubItem>
      ))}
    </SidebarMenuSub>
  );

  return (
    <Sidebar collapsible="icon">
      <SidebarHeader>
        <Link
          to={Paths.dashboard}
          className="font-semibold text-lg px-2 py-1 flex items-center gap-2"
        >
          {state === "collapsed" ? (
            // Show only icon when collapsed
            <span className="text-xl">📚</span>
          ) : (
            // Show full text when expanded
            <span>Student Submission</span>
          )}
        </Link>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>Menu</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {visibleItems.map((item) => {
                const hasSubItems = !!item.subItems?.length;

                if (!hasSubItems) {
                  return (
                    <SidebarMenuItem key={item.label}>
                      <SidebarMenuButton
                        render={<Link to={item.to!} />}
                        tooltip={item.label}
                        isActive={isActive(item)}
                      >
                        <item.icon />
                        <span>{item.label}</span>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                  );
                }

                // Has sub-items, sidebar is full-width: click to expand in place.
                if (!isIconMode) {
                  const isOpen = openSections.has(item.label);
                  return (
                    <SidebarMenuItem key={item.label}>
                      <SidebarMenuButton
                        isActive={isActive(item)}
                        onClick={() => toggleSection(item.label)}
                      >
                        <item.icon />
                        <span>{item.label}</span>
                        <span className="ml-auto">
                          {isOpen ? (
                            <ChevronDown className="h-4 w-4" />
                          ) : (
                            <ChevronRight className="h-4 w-4" />
                          )}
                        </span>
                      </SidebarMenuButton>
                      {isOpen && renderSubItems(item.subItems!)}
                    </SidebarMenuItem>
                  );
                }

                // Has sub-items, sidebar is icon-only: Popover flyout on click.
                // (Popover instead of raw CSS hover — no flicker on diagonal
                // mouse movement, and it's keyboard-accessible for free.)
                return (
                  <SidebarMenuItem key={item.label}>
                    <Popover>
                      <PopoverTrigger
                        render={
                          <SidebarMenuButton
                            tooltip={item.label}
                            isActive={isActive(item)}
                          >
                            <item.icon />
                          </SidebarMenuButton>
                        }
                      />
                      <PopoverContent side="right" className="p-1 w-48">
                        {renderSubItems(item.subItems!)}
                      </PopoverContent>
                    </Popover>
                  </SidebarMenuItem>
                );
              })}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter>
        {email ? (
          <div className="flex flex-col gap-2 px-2 py-1">
            <Button
              variant="outline"
              size={isIconMode ? "icon" : "sm"}
              onClick={handleLogout}
            >
              <LogOut className="h-4 w-4" />
              {!isIconMode && <span className="ml-1">Log Out</span>}
            </Button>
          </div>
        ) : (
          <div className="flex flex-col gap-2 px-2 py-1">
            <Link to={Paths.login}>
              <Button
                variant="outline"
                size={isIconMode ? "icon" : "sm"}
                className="w-full"
              >
                {isIconMode ? "🔑" : "Log In"}
              </Button>
            </Link>
          </div>
        )}
      </SidebarFooter>
    </Sidebar>
  );
}
