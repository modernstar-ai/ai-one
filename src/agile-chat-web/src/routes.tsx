import { Routes, Route } from 'react-router-dom';
import HomePage from './pages/Home';
import ChatPage from './pages/Chat';
import ProfileForm from './pages/Personas';
import ToolsPage from './pages/Tools';

const AppRoutes = () => (
  <Routes>
    <Route path="/" element={<HomePage />} />
    <Route path="/chat" element={<ChatPage />} />
    <Route path="/personas" element={<ProfileForm />} />
    <Route path="/tools" element={<ToolsPage />} />
  </Routes>
);

export default AppRoutes;
