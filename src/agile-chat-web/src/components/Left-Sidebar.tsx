import { Sidebar, SidebarContent, SidebarTrigger } from '@/components/ui/sidebar';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';
import { Button } from '@/components/ui/button';
import {
  Home,
  MessageCircleMore,
  FileBox,
  User,
  VenetianMask,
  LogOut,
  MessageSquareCode,
  Wrench,
  Database,
} from 'lucide-react';
import { Link } from 'react-router-dom';
import { useAuth } from '@/services/auth-helpers';

export function LeftSidebar() {
  const { instance, accounts, isLoggedIn, name, username } = useAuth();

  return (
    <Sidebar variant="inset" collapsible="icon" className="p-0">
      <SidebarContent>
        <div className="flex h-screen w-full  bg-background text-foreground border-red-500">
          <div
            id="left-icon-menu"
            className="bg-primary text-primary-foreground flex flex-col items-center py-4 space-y-4 h-full  border-orange-500 border-0"
          >
            <div className="flex h-full border-green-500 border-1">
              {/* Sidebar container */}
              {/* <div className="bg-black w-16 flex flex-col items-center py-4"> */}
              <div className="w-16 bg-primary text-primary-foreground flex flex-col items-center py-4 space-y-4">
                {/* Top icons */}
                <div className="space-y-4">
                  <Link to="/" aria-label="Home" accessKey="h">
                    <Button variant="ghost" size="icon" tabIndex={-1} aria-label="Home Button">
                      <Home className="h-6 w-6" />
                    </Button>
                  </Link>
                  {/* <Home className="text-gray-400 w-6 h-6" /> */}
                </div>
                <SidebarTrigger />

                {/* Middle icons, flex-grow ensures they are in the center */}
                <div className="flex-grow flex flex-col justify-center space-y-4">
                  <Link to="/chat" aria-label="Home" accessKey="c">
                    <Button variant="ghost" size="icon" tabIndex={-1} aria-label="Chat Button">
                      <MessageCircleMore className="h-6 w-6" />
                    </Button>
                  </Link>
                  <Link aria-label="Chat over data" to="/ragchat" accessKey="r">
                    <Button variant="ghost" size="icon" tabIndex={-1} aria-label="Chat over Data Button">
                      <MessageSquareCode className="h-6 w-6" />
                    </Button>
                  </Link>
                  <Link to="/files" aria-label="Home" accessKey="f">
                    <Button variant="ghost" size="icon" tabIndex={-1} aria-label="File Button">
                      {' '}
                      <FileBox className=" w-6 h-6" />
                    </Button>
                  </Link>
                  <Link to="/indexes" aria-label="Indexes" accessKey="i">
                    <Button variant="ghost" size="icon" tabIndex={-1} aria-label="Chatbot Button">
                      <Database className="h-6 w-6" />
                    </Button>
                  </Link>
                  <Link to="/assistants" aria-label="Home" accessKey="p">
                    <Button variant="ghost" size="icon" tabIndex={-1} aria-label="Chatbot Button">
                      <VenetianMask className="h-6 w-6" />
                    </Button>
                  </Link>
                  <Link to="/indexes" aria-label="Indexes" accessKey="i">
                    <Button variant="ghost" size="icon" tabIndex={-1} aria-label="Chatbot Button">
                      <Database className="h-6 w-6" />
                    </Button>
                  </Link>
                  <Link to="/tools" aria-label="Tools" accessKey="l">
                    <Button variant="ghost" size="icon" tabIndex={-1} aria-label="Tools Page Button">
                      <Wrench className="h-6 w-6" />
                    </Button>
                  </Link>
                </div>

                {isLoggedIn && (
                  <Popover>
                    <PopoverTrigger asChild>
                      <Button variant="ghost" size="icon" aria-label="User Details">
                        <User className="h-6 w-6" />
                      </Button>
                    </PopoverTrigger>
                    <PopoverContent className="w-60">
                      <div className="space-y-2">
                        <h4 className="font-medium leading-none">{name}</h4>
                        <p className="text-xs text-muted-foreground">{username}</p>
                        <Button
                          variant="outline"
                          className="w-full justify-start"
                          aria-label="Logout Button"
                          onClick={() => {
                            instance.logoutRedirect({ account: accounts[0] }).then(() => {
                              instance.logoutPopup({ account: accounts[0] });
                            });
                          }}
                        >
                          <LogOut className="mr-2 h-4 w-4" />
                          Logout
                        </Button>
                      </div>
                    </PopoverContent>
                  </Popover>
                )}
              </div>
            </div>
          </div>
          {isLoggedIn && (
            <div
              id="left-sidebar-content"
              className="flex flex-col bg-gray-100 p-4 overflow-y-auto border-yellow-500 border-0"
            >
              <div className="bg-gray-100 p-4 overflow-y-auto">
                <div className="flex items-center mb-6">
                  <div className="text-3xl font-bold mr-2">[logo]</div>
                </div>
                <div className="text-sm font-semibold mb-2">Your AI Assistants</div>

                <div className="space-y-2">
                  <div>Foundations of...</div>
                  <div>Health and Society</div>
                  <div>Clinical Practice 2A</div>
                  <div>Clinical Practice 3A</div>
                </div>

                <div className="mt-6 text-sm font-semibold">History</div>
                <div className="space-y-2 mt-2">
                  <div>What is the role....</div>
                  <div>History 2</div>
                  <div>History 3</div>
                  <div>History 4</div>
                  <div>History 5</div>
                </div>
                {/* Left-aligned Clear History button */}
                <Button variant="secondary" className="mt-4 px-0">
                  Clear History
                </Button>
              </div>
            </div>
          )}
        </div>
      </SidebarContent>
    </Sidebar>
  );
}
