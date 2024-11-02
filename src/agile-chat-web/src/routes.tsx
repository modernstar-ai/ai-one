import { Routes, Route } from 'react-router-dom';
import HomePage from './pages/Home';
import ChatPage from './pages/Chat';
import RagChatPage from './pages/RagChat/index-using-post';
import ProfileForm from './pages/Personas';
import AssistantPage from './pages/Assistant';
import AssistantForm from './pages/Assistant/AssistantForm';
import FileUploadPage from './pages/FileUpload';
import FileListPage from './pages/FileUpload/filelist';
import ToolsPage from './pages/tools';
import ConnectToDB from './pages/tools/connecttodb';
import ConnectToLogicApp from './pages/tools/connecttologicapp';
import ConnectToApi from './pages/tools/connecttoapi';


const AppRoutes = () => (
  <Routes>
    <Route path="/" element={<HomePage />} />
    <Route path="/chat" element={<ChatPage />} />
    <Route path="/ragchat" element={<RagChatPage />} />

    <Route path="/assistants" element={<AssistantPage />} />
    <Route path="/assistant" element={<AssistantForm />} />

    <Route path="/files" element={<FileListPage />} />
    <Route path="/fileupload" element={<FileUploadPage />} />

    <Route path="/personas" element={<ProfileForm />} />
    
    <Route path="/tools" element={<ToolsPage />} />
    <Route path="/connecttodb" element={<ConnectToDB />} />
    <Route path="/connecttologicapp" element={<ConnectToLogicApp />} />
    <Route path="/connecttoapi" element={<ConnectToApi />} />
    
    
  </Routes>
);

export default AppRoutes;
