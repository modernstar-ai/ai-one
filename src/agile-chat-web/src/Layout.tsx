import { SidebarProvider } from '@/components/ui/sidebar';
import { LeftSidebar } from './components/Left-Sidebar';

export default function Layout({ children }: { children: React.ReactNode }) {
  return (
    <div className="h-screen w-full bg-background text-foreground">
      {/* Left Sidebar */}
      <SidebarProvider
        className="h-screen"
        defaultOpen={false}
        style={
          {
            '--sidebar-width-icon': '3.5rem',
            '--sidebar-width': '20rem',
            '--sidebar-width-mobile': '20rem',
          } as React.CSSProperties
        }
      >
        <LeftSidebar />
        {/* Main Content Area */}
        <div className="h-screen w-full">{children}</div>
      </SidebarProvider>
    </div>
  );
}
