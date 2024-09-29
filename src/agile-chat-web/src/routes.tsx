import React from 'react';
import { Routes, Route } from 'react-router-dom';
import HomePage from './pages/Home';
import ChatPage from './pages/Chat';
import PersonasPage from './pages/Personas';
import ToolsPage from './pages/Tools';

const AppRoutes = () => (
  <Routes>
    <Route path="/" element={<HomePage />} />
    <Route path="/chat" element={<ChatPage />} />
    <Route path="/personas" element={<PersonasPage />} />
    <Route path="/tools" element={<ToolsPage />} />
  </Routes>
);

export default AppRoutes;
