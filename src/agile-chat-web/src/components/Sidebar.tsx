'use client'

import { useState } from 'react'
import { Button } from "@/components/ui/button"
import { Home, MessageCircleMore, FileBox, User, VenetianMask, LogOut, Columns2  } from 'lucide-react'
import { Link } from 'react-router-dom';
import {
    Popover,    
    PopoverContent,
    PopoverTrigger,
  } from "@/components/ui/popover"

export default function Sidebar() {
  const [isSidebarExpanded, setIsSidebarExpanded] = useState(false)

  const toggleSidebar = () => {
    setIsSidebarExpanded(!isSidebarExpanded)
  }

  return (
    <div className="flex h-full">
      {/* Sidebar container */}
      {/* <div className="bg-black w-16 flex flex-col items-center py-4"> */}
      <div className="w-16 bg-primary text-primary-foreground flex flex-col items-center py-4 space-y-4">
        {/* Top icons */}
        <div className="space-y-4">
          <Link to="/" aria-label="Home" accessKey="h"><Button variant="ghost" size="icon" tabIndex={-1} aria-label="Home Button"><Home className="h-6 w-6" /></Button></Link>
          {/* <Home className="text-gray-400 w-6 h-6" /> */}
          </div>
          <button onClick={toggleSidebar} aria-label="Toggle Sidebar" accessKey="t">
            <Columns2 className="w-6 h-6" />
          </button>
        
        {/* Middle icons, flex-grow ensures they are in the center */}
        <div className="flex-grow flex flex-col justify-center space-y-4">
        <Link to="/chat" aria-label="Home" accessKey="c"><Button variant="ghost" size="icon" tabIndex={-1} aria-label="Chat Button"><MessageCircleMore className="h-6 w-6" /></Button></Link>
        <Link to="/filelist" aria-label="Home" accessKey="f"><Button variant="ghost" size="icon" tabIndex={-1} aria-label="File Button"> <FileBox className=" w-6 h-6" /></Button></Link>
        <Link to="/assistantlist" aria-label="Home" accessKey="p"><Button variant="ghost" size="icon" tabIndex={-1} aria-label="Chatbot Button"><VenetianMask className="h-6 w-6" /></Button></Link>
        </div>
        {/* Bottom icon */}
        {/* <Link to="/"><Button variant="ghost" size="icon"><User className="h-6 w-6" /></Button></Link> */}
        <Popover>
          <PopoverTrigger asChild>
            <Button variant="ghost" size="icon" aria-label="User Details">
              <User className="h-6 w-6" />
            </Button>
          </PopoverTrigger>
          <PopoverContent className="w-60">
            <div className="space-y-2">
              <h4 className="font-medium leading-none">Username</h4>
              <p className="text-sm text-muted-foreground">test@student.test.edu.au</p>
              <Button variant="outline" className="w-full justify-start" aria-label="Logout Button">
                <LogOut className="mr-2 h-4 w-4" />
                Logout
              </Button>
            </div>
          </PopoverContent>
        </Popover>
      </div>
      {/* Sidebar content */}
      {isSidebarExpanded && (
        <div className="bg-gray-100 w-64 p-4 overflow-y-auto">
          <div className="flex items-center mb-6">
            <div className="text-3xl font-bold mr-2">UTS</div>
          </div>
          <div className="text-sm font-semibold mb-2">Your AI Assistants</div>
          <div className="space-y-2">
            <div>Foundations of Nursing...</div>
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
         <Button variant="secondary" className="mt-4 px-0">Clear History</Button>
        </div>
      )}
    </div>
  )
}
