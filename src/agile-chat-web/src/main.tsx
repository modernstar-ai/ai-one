import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App.tsx'
import Layout from './Layout';
import './global.css'
import { BrowserRouter as Router } from 'react-router-dom';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Router>
    <Layout>
      <App />
    </Layout>
    </Router>
  </StrictMode>,
)
