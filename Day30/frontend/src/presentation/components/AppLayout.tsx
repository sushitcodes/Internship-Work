import { Outlet } from "react-router-dom";
import {
  SidebarProvider,
  SidebarInset,
  SidebarTrigger,
} from "@/components/ui/sidebar";
import { AppSidebar } from "./AppSidebar";

// Why this exists: a layout ROUTE (rendered via <Outlet/>) instead of wrapping
// every page manually. React Router renders this once, and swaps only the
// <Outlet/> content when you navigate — the sidebar never remounts or flickers.
export default function AppLayout() {
  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <header className="flex h-12 items-center gap-2 border-b px-3">
          <SidebarTrigger />
        </header>
        <div className="p-4">
          <Outlet />
        </div>
      </SidebarInset>
    </SidebarProvider>
  );
}
