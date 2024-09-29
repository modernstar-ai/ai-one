// white background - attempt to look like azure chat
// not currently used
import React from 'react'
import { 
  PanelLeftClose, 
  Home, 
  MessageCircle, 
  Book, 
  UserCircle,
  FileText,
  VenetianMask,
  Scissors
} from 'lucide-react'
import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip"

interface NavItemProps {
  icon: React.ReactNode
  label: string
}

function NavItem({ icon, label }: NavItemProps) {
  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <Button variant="ghost" size="icon" className="h-12 w-12">
          {icon}
          <span className="sr-only">{label}</span>
        </Button>
      </TooltipTrigger>
      <TooltipContent side="right">
        {label}
      </TooltipContent>
    </Tooltip>
  )
}

export default function LeftIconMenu({ className }: { className?: string }) {
  return (
    <TooltipProvider>
      <aside className={cn("flex h-screen w-12 flex-col items-center justify-between border-r bg-background py-4", className)}>
        <div className="flex flex-col items-center space-y-4">
          <NavItem icon={<PanelLeftClose className="h-5 w-5" />} label="Toggle Panel" />
          <NavItem icon={<Home className="h-5 w-5" />} label="Home" />
        </div>
        <div className="flex flex-col items-center space-y-4">
          <NavItem icon={<MessageCircle className="h-5 w-5" />} label="Messages" />
          <NavItem icon={<VenetianMask  className="h-5 w-5" />} label="Venetian Mask" />
          <NavItem icon={<Scissors className="h-5 w-5" />} label="Pocket Knife" />
          <NavItem icon={<Book className="h-5 w-5" />} label="Book" />
        </div>
        <div className="flex flex-col items-center space-y-4">
          <NavItem icon={<FileText className="h-5 w-5" />} label="Sheet" />
          <NavItem icon={<UserCircle className="h-5 w-5" />} label="User Profile" />
        </div>
      </aside>
    </TooltipProvider>
  )
}