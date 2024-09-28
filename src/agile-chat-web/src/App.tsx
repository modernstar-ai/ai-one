import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import { Button } from "@/components/ui/button"
import ToolsComponent from  "@/components/ToolsComponent"
import LeftIconMenu from "@/components/LeftIconMenu"

import './App.css'

function App() {
  const [count, setCount] = useState(0)

  return (
    <>
    <div className="flex">
      <LeftIconMenu />
      <main className="flex-1">
        {/* Your main content goes here */}
      <div>
        <a href="https://vitejs.dev" target="_blank">
          <img src={viteLogo} className="logo" alt="Vite logo" />
        </a>
        <a href="https://react.dev" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
      <h1>Vite + React</h1>
      <div className="card">
        <button onClick={() => setCount((count) => count + 1)}>
          Default button - count is {count}
        </button>
        <Button onClick={() => setCount((count) => count + 1)}>
          Accessible Button - Count is {count}
        </Button>
        <p>
          Edit <code>src/App.tsx</code> and save to test HMR
        </p>
      </div>
      <ToolsComponent />
      <p className="read-the-docs">
        Click on the Vite and React logos to learn more
      </p>
      </main>
    </div>
    </>
  )
}

export default App
