import { Outlet } from "react-router-dom";
import {
  SidebarProvider,
  SidebarInset,
  SidebarTrigger,
} from "@/components/ui/sidebar";
import { AppSidebar } from "./AppSidebar";
import { UserMenu } from "./UserMenu";

// No manual localStorage/cookie handling here — SidebarProvider already
// persists open/collapsed state on its own (via a cookie) as soon as you
// use SidebarTrigger. Adding a second localStorage key on top of that was
// the duplicate-source-of-truth bug from the last version.
export default function AppLayout() {
  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <header className="flex h-12 items-center justify-between gap-2 border-b px-3">
          <SidebarTrigger />
          <UserMenu />
        </header>
        <div className="p-4">
          <Outlet />
        </div>
      </SidebarInset>
    </SidebarProvider>
  );
}
