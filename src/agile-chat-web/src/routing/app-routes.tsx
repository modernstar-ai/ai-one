import { Routes, Route } from 'react-router-dom';
import HomePage from '../pages/Home';
import ChatPage from '../pages/Chat';
import RagChatPage from '../pages/RagChat/index-using-post';
import AssistantsPage from '../pages/Assistant';
import AssistantForm from '../pages/Assistant/AssistantForm';
import FileUploadPage from '../pages/FileUpload';
import FileListPage from '../pages/FileUpload/filelist';
import ToolsPage from '../pages/tools/index';
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
