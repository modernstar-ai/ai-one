import { useState } from 'react'
import { Button } from "@/components/ui/button"
import ToolsComponent from "@/components/ToolsComponent"

import { ScrollArea } from "@/components/ui/scroll-area"
import LeftMenu from '@/components/Menu-Left';
import SimpleHeading from '@/components/Heading-Simple';

const ToolsPage = () => {
  const [count, setCount] = useState(0)
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
        <SimpleHeading Title="Tools" Subtitle='Configure your tools' DocumentCount={0} />

        <div className="flex-1 p-4">

          <main className="flex-1">
            {/* Your main content goes here */}
            <h1 className="text-xl mv-2" >Vite + React</h1>
            <div className="card">
              <Button onClick={() => setCount((count) => count + 1)}>
                Accessible Button - Count is {count}
              </Button>
            </div>
            <ToolsComponent />
          </main>
        </div>

      </div>


    </div>
  )
}

export default ToolsPage