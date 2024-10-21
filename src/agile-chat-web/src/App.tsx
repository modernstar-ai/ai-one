import { BrowserRouter as Router } from 'react-router-dom';
import AppRoutes from './routes';
import './App.css'
import { Toaster } from "@/components/ui/toaster"; 

function App() {
 
  return (
   
    <>
    <Toaster />
    <Router>
    <AppRoutes />
    </Router>
    </>

  )
}

export default App
