import { SidebarProvider } from '@/components/ui/sidebar';
import { ResponsiveNavigation } from './components/Left-Sidebar';

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
            '--sidebar-width-mobile': '20rem'
          } as React.CSSProperties
        }>
        <ResponsiveNavigation />
        {/* Main Content Area */}
        <div className="min-h-[95vh] lg:h-screen w-full lg:mt-0 mt-[5vh]">{children}</div>
      </SidebarProvider>
    </div>
  );
}
