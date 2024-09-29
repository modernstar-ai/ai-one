import React, { useState } from 'react'
import { Button } from "@/components/ui/button"
import { Textarea } from "@/components/ui/textarea"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Menu, Book, Paperclip } from 'lucide-react'
import LeftMenu from '@/components/Menu-Left';

const ChatPage = () => {
  
  const [isHistoryOpen, setIsHistoryOpen] = useState(false);
     
  return (
    <div className="flex h-screen bg-background text-foreground">
      
      {/* Left Sidebar */}
      <LeftMenu isHistoryOpen={isHistoryOpen} setIsHistoryOpen={setIsHistoryOpen} />   

      {/* Search History Panel */}
      {isHistoryOpen && (
        <div className="w-64 bg-secondary p-4 overflow-auto">
          <h2 className="text-lg font-semibold mb-4">Search History</h2>
          <ScrollArea className="h-[calc(100vh-2rem)]">
            {/* Add your search history items here */}
            <div className="space-y-2">
              <div className="p-2 hover:bg-accent rounded">Previous search 1</div>
              <div className="p-2 hover:bg-accent rounded">Previous search 2</div>
              {/* ... more items ... */}
            </div>
          </ScrollArea>
        </div>
      )}

      {/* Main Chat Area */}
      <div className="flex-1 flex flex-col">
        {/* Header */}
        <div className="bg-muted p-4 flex justify-between items-center">
          <div>
            <h1 className="text-2xl font-bold">Detailed Image Descriptions</h1>
            <p className="text-sm text-muted-foreground">Subtitle goes here</p>
          </div>
          <div className="flex space-x-2">
            <Button variant="outline" size="icon"><Menu className="h-4 w-4" /></Button>
            <Button variant="outline" size="icon">0</Button>
          </div>
        </div>

        {/* Chat Messages */}
        <ScrollArea className="flex-1 p-4">
          {/* Add your chat messages here */}
        </ScrollArea>

        {/* Input Area */}
        <div className="p-4 border-t">
          <Textarea 
            placeholder="Type your message here..." 
            className="w-full mb-2"
            rows={4}
          />
          <div className="flex justify-between items-center">
            <div className="flex space-x-2">            
              <Button variant="outline" size="icon"><Paperclip className="h-4 w-4"  /></Button>
              <Button variant="outline" size="icon"><Book className="h-4 w-4"  /></Button>
            </div>
            <Button><Book size="icon" className="h-4 w-4 mr-2"/>Send </Button>
          </div>
        </div>
      </div>
    </div>
 
);
};

export default ChatPage;
