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
      <div className="flex h-screen w-full bg-background text-foreground border-blue-500 border-4">

        {/* Left Sidebar */}
        <div className="flex flex-col ">
          <LeftSidebar />
        </div>

        {/* Main Content Area */}
        <div className="flex-1 flex flex-col border-green-400 border-4">

          {/* Main Content Area */}
          <div className="flex-1 flex flex-col border-green-400 border-4">
            <main className="flex-1 p-4 border-red-500 border-8 w-full">
              {children}
            </main>
          </div>
        </div>
      </div>
    </SidebarProvider>
  );
}