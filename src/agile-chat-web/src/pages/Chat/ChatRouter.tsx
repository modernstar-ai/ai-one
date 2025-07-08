import React, { useEffect } from 'react';
import { useNavigate, useSearchParams, useLocation } from 'react-router-dom';
import { fetchAssistantById } from '@/services/assistantservice';
import { createChatThread } from '@/services/chatthreadservice';
import { Loader2 } from 'lucide-react';

const ChatRouter: React.FC = () => {
  const { state } = useLocation();
  const navigate = useNavigate();
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [_searchParams, setSearchParams] = useSearchParams();

  useEffect(() => {
    const fetchData = async () => {
      try {
        const assistant = state.assistantId ? await fetchAssistantById(state.assistantId) : undefined;
        const thread = await createChatThread({
          name: assistant ? `${assistant.name}` : 'New Chat',
          assistantId: assistant?.id
        });

        if (thread) {
          navigate(`/chat/${thread.id}`);
        } else navigate('/');
      } catch (error) {
        console.error('Error fetching assistant:', error);
        navigate('/');
      }
    };

    fetchData();
  }, [navigate, setSearchParams]);

  return (
    <div className="h-screen flex justify-center items-center">
      <Loader2 className="animate-spin" />
    </div>
  );
};

export default ChatRouter;
