import { Routes, Route } from 'react-router-dom';
import HomePage from '../pages/Home';
import ChatRouter from '../pages/Chat/ChatRouter';
import ChatPage from '../pages/Chat';
import RagChatPage from '../pages/RagChat/index-using-post';
import AssistantsPage from '../pages/Assistant';
import AssistantForm from '../pages/Assistant/AssistantForm';
import FileUploadPage from '../pages/FileUpload';
import FileListPage from '../pages/FileUpload/filelist';
import ToolsPage from '../pages/Tools/index';
import ConnectToDB from '../pages/Tools/connecttodb';
import ConnectToLogicApp from '../pages/Tools/connecttologicapp';
import ConnectToApi from '../pages/Tools/connecttoapi';
import LoginPage from '../pages/Login/login';

import { ProtectedRoute } from './protected-route';
import IndexesPage from '@/pages/Indexes';

const AppRoutes = () => (
  <Routes>
    <Route
      path="/"
      element={
        <ProtectedRoute>
          <HomePage />
        </ProtectedRoute>
      }
    />
    <Route path="/login" element={<LoginPage />} />
    <Route
      path="/chatrouter/:id"
      element={
        <ProtectedRoute>
          <ChatRouter />
        </ProtectedRoute>
      }
    />
    <Route
      path="/chat"
      element={
        <ProtectedRoute>
          <ChatPage />
        </ProtectedRoute>
      }
    />
    <Route
      path="/ragchat"
      element={
        <ProtectedRoute>
          <RagChatPage />
        </ProtectedRoute>
      }
    />
    <Route
      path="/fileupload"
      element={
        <ProtectedRoute>
          <FileUploadPage />
        </ProtectedRoute>
      }
    />
    <Route
      path="/indexes"
      element={
        <ProtectedRoute>
          <IndexesPage />
        </ProtectedRoute>
      }
    />
    <Route
      path="/assistants"
      element={
        <ProtectedRoute>
          <AssistantsPage />
        </ProtectedRoute>
      }
    />
    <Route
      path="/assistant"
      element={
        <ProtectedRoute>
          <AssistantForm />
        </ProtectedRoute>
      }
    />
    <Route
      path="/files"
      element={
        <ProtectedRoute>
          <FileListPage />
        </ProtectedRoute>
      }
    />

    <Route
      path="/tools"
      element={
        <ProtectedRoute>
          <ToolsPage />
        </ProtectedRoute>
      }
    />
    <Route
      path="/connecttodb"
      element={
        <ProtectedRoute>
          <ConnectToDB />
        </ProtectedRoute>
      }
    />
    <Route
      path="/connecttologicapp"
      element={
        <ProtectedRoute>
          <ConnectToLogicApp />
        </ProtectedRoute>
      }
    />
    <Route
      path="/connecttoapi"
      element={
        <ProtectedRoute>
          <ConnectToApi />
        </ProtectedRoute>
      }
    />
  </Routes>
);

export default AppRoutes;
