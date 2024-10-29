import { Routes, Route } from 'react-router-dom';
import HomePage from './pages/Home';
import ChatPage from './pages/Chat';
import RagChatPage from './pages/RagChat/index-using-post';
import ProfileForm from './pages/Personas';
import ToolsPage from './pages/tools';
import FileUploadPage from './pages/FileUpload';
import AiAssistantPage from './pages/AiAssistant';
import FileListPage from './pages/FileUpload/filelist';
import AssistantListPage from './pages/AiAssistant/assistantlist';
import ConnectToDB from './pages/tools/connecttodb';
import ConnectToLogicApp from './pages/tools/connecttologicapp';
import ConnectToApi from './pages/tools/connecttoapi';


const AppRoutes = () => (
  <Routes>
    <Route path="/" element={<HomePage />} />
    <Route path="/chat" element={<ChatPage />} />
    <Route path="/ragchat" element={<RagChatPage />} />
    <Route path="/fileupload" element={<FileUploadPage />} />
    <Route path="/aiassistant" element={<AiAssistantPage />} />
    <Route path="/filelist" element={<FileListPage />} />
    <Route path="/assistantlist" element={<AssistantListPage />} />
    <Route path="/personas" element={<ProfileForm />} />
    <Route path="/tools" element={<ToolsPage />} />
    <Route path="/connecttodb" element={<ConnectToDB />} />
    <Route path="/connecttologicapp" element={<ConnectToLogicApp />} />
    <Route path="/connecttoapi" element={<ConnectToApi />} />
    
    
  </Routes>
);

export default AppRoutes;
