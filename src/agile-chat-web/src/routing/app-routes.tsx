import { Routes, Route } from 'react-router-dom';
import HomePage from '../pages/Home';
import ChatPage from '../pages/Chat';
import RagChatPage from '../pages/RagChat/index-using-post';
import ProfileForm from '../pages/Personas';
import ToolsPage from '../pages/tools';
import FileUploadPage from '../pages/FileUpload';
import AiAssistantPage from '../pages/AiAssistant';
import FileListPage from '../pages/FileUpload/filelist';
import AssistantListPage from '../pages/AiAssistant/assistantlist';
import ConnectToDB from '../pages/tools/connecttodb';
import ConnectToLogicApp from '../pages/tools/connecttologicapp';
import ConnectToApi from '../pages/tools/connecttoapi';
import LoginPage from '../pages/Login/login';
import { ProtectedRoute } from './protected-route';

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
      path="/aiassistant"
      element={
        <ProtectedRoute>
          <AiAssistantPage />
        </ProtectedRoute>
      }
    />
    <Route
      path="/filelist"
      element={
        <ProtectedRoute>
          <FileListPage />
        </ProtectedRoute>
      }
    />
    <Route
      path="/assistantlist"
      element={
        <ProtectedRoute>
          <AssistantListPage />
        </ProtectedRoute>
      }
    />
    <Route
      path="/personas"
      element={
        <ProtectedRoute>
          <ProfileForm />
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
