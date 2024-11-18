import React, { useEffect, useState } from 'react';
import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import ChatPage from '../Chat/index';
import RagChatPage from '../RagChat/index-using-post';
import { fetchAssistantById } from '@/services/assistantservice';

const ChatRouter: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [_searchParams, setSearchParams] = useSearchParams();
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState<React.ReactNode | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const assistant = await fetchAssistantById(id!);
        if (assistant) {
          if (assistant.type === 'Chat') {
            // Add assistantId to query parameters
            setSearchParams((prev) => {
              const newParams = new URLSearchParams(prev);
              newParams.set('assistantId', id!);
              return newParams;
            });
            setPage(<ChatPage />);
          } else if (assistant.type === 'Search') {
            setPage(<RagChatPage id={id} />);
          } else {
            console.log('ChatRouter unknown assistant type:', assistant.type);
            navigate('/not-found');
          }
        } else {
          console.log('ChatRouter assistant not found:', id);
          navigate('/not-found');
        }
      } catch (error) {
        console.error('Error fetching assistant:', error);
        navigate('/error');
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      fetchData();
    } else {
      setPage(<ChatPage />);
    }
  }, [id, navigate, setSearchParams]);

  if (loading) {
    return <div>Loading...</div>;
  }

  return <>{page}</>;
};

export default ChatRouter;
