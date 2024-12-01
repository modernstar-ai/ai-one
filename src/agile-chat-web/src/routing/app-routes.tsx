import { Routes, Route } from 'react-router-dom';
import HomePage from '../pages/Home';
import ChatRouter from '../pages/Chat/ChatRouter';
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
import IndexesPage from '@/pages/Indexes/indexesPage';
import IndexForm from '@/pages/Indexes/indexForm';
import { ProtectedRoute } from './protected-route';
import { UserRole } from '@/authentication/user-roles';

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
          <ChatRouter />
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
        <ProtectedRoute role={UserRole.ContentManager}>
          <FileUploadPage />
        </ProtectedRoute>
      }
    />
    <Route
      path="/containers"
      element={
        <ProtectedRoute role={UserRole.ContentManager}>
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
        <ProtectedRoute role={UserRole.ContentManager}>
          <AssistantForm />
        </ProtectedRoute>
      }
    />
    <Route
      path="/files"
      element={
        <ProtectedRoute role={UserRole.ContentManager}>
          <FileListPage />
        </ProtectedRoute>
      }
    />

    <Route
      path="/tools"
      element={
        <ProtectedRoute role={UserRole.SystemAdmin}>
          <ToolsPage />
        </ProtectedRoute>
      }
    />
    <Route
      path="/indexes"
      element={
        <ProtectedRoute role={UserRole.ContentManager}>
          <IndexesPage />
        </ProtectedRoute>
      }
    />
    <Route
      path="/container-form"
      element={
        <ProtectedRoute role={UserRole.ContentManager}>
          <IndexForm />
        </ProtectedRoute>
      }
    />

    <Route
      path="/connecttodb"
      element={
        <ProtectedRoute role={UserRole.SystemAdmin}>
          <ConnectToDB />
        </ProtectedRoute>
      }
    />
    <Route
      path="/connecttologicapp"
      element={
        <ProtectedRoute role={UserRole.SystemAdmin}>
          <ConnectToLogicApp />
        </ProtectedRoute>
      }
    />
    <Route
      path="/connecttoapi"
      element={
        <ProtectedRoute role={UserRole.SystemAdmin}>
          <ConnectToApi />
        </ProtectedRoute>
      }
    />
  </Routes>
);

export default AppRoutes;
