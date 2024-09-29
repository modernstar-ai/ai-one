import { useState } from 'react'
import { Button } from "@/components/ui/button"
import ToolsComponent from "@/components/ToolsComponent"
import LeftIconMenu from "@/components/Menu-LeftIconMenu"


const ToolsPage = () => {
  const [count, setCount] = useState(0)

  return (
    <>
      <div className="flex">
        <LeftIconMenu />
        <main className="flex-1">
          {/* Your main content goes here */}
          <h1>Vite + React</h1>
          
          <div className="card">
            <button onClick={() => setCount((count) => count + 1)}>
              Default button - count is {count}
            </button>
            <Button onClick={() => setCount((count) => count + 1)}>
              Accessible Button - Count is {count}
            </Button>            
          </div>
          <ToolsComponent />          
        </main>
      </div>
    </>
  )
}

export default ToolsPage