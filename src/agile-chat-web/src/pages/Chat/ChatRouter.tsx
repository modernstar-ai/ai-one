import React, { useEffect, useState } from 'react';
import { useNavigate, useSearchParams, useLocation } from 'react-router-dom';
import ChatPage from '../Chat/index';
import { fetchAssistantById } from '@/services/assistantservice';
import { createChatThread } from '@/services/chatthreadservice';
import { ChatThreadFilterOptions, ChatThreadPromptOptions } from '@/types/ChatThread';

const ChatRouter: React.FC = () => {
  const { state } = useLocation();
  const navigate = useNavigate();
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [_searchParams, setSearchParams] = useSearchParams();
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState<React.ReactNode | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const assistant = state.assistantId ? await fetchAssistantById(state.assistantId) : undefined;
        const thread = await createChatThread({
          name: assistant ? `New Chat - ${assistant.name}` : undefined,
          assistantId: assistant?.id,
        });

        if (thread) setPage(<ChatPage thread={thread} />);
        else navigate('/error');
      } catch (error) {
        console.error('Error fetching assistant:', error);
        navigate('/error');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [navigate, setSearchParams]);

  if (loading) {
    return <div>Loading...</div>;
  }

  return <>{page}</>;
};

export default ChatRouter;
