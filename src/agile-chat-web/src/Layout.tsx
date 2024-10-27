import { SidebarProvider } from "@/components/ui/sidebar"
import { LeftSidebar } from "./components/Left-Sidebar";

export default function Layout({ children }: { children: React.ReactNode }) {
  return (
    <SidebarProvider
      defaultOpen={false}
      style={{
        "--sidebar-width-icon": "3.5rem",
        "--sidebar-width": "20rem",
        "--sidebar-width-mobile": "20rem",        
      }}
    >
      <div className="flex h-screen w-full bg-background text-foreground">

        {/* Left Sidebar */}
        <div className="flex flex-col ">
          <LeftSidebar />
        </div>

        {/* Main Content Area */}
        <div className="flex-1 flex flex-col ">

          {/* Main Content Area */}
          <div className="flex-1 flex flex-col ">
            <main className="flex-1 p-2 w-full">
              {children}
            </main>
          </div>
        </div>
      </div>
    </SidebarProvider>
  );
}