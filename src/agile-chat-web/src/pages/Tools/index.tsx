import { useState } from 'react'
import { Button } from "@/components/ui/button"
import ToolsComponent from "@/components/ToolsComponent"
import LeftMenu from '@/components/LeftMenu'
import SimpleHeading from '@/components/Heading-Simple';

const ToolsPage = () => {
  const [count, setCount] = useState(0)
  
  return (
    <div className="flex h-screen bg-background text-foreground">

      {/* Left Sidebar */}
      <LeftMenu />      

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