import { BrowserRouter as Router } from 'react-router-dom';
import AppRoutes from './routes';
import './App.css'
import { Toaster } from "@/components/ui/toaster"; 
import { ErrorBoundary } from './error-handling/ErrorBoundary';

function App() {
 
  return (
  <ErrorBoundary>
    <>
    <Toaster />
    <Router>
    <AppRoutes />
    </Router>
    </>
   </ErrorBoundary>
  
  )
}

export default App




