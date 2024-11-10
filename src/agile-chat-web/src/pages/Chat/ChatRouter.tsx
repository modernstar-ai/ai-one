import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import ChatPage from '../Chat/index';
import RagChatPage from '../RagChat/index-using-post';
import { fetchAssistantById } from '@/services/assistantservice';

const ChatRouter: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState<React.ReactNode | null>(null);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const assistant = await fetchAssistantById(id);                
                if (assistant) {                        
                    if (assistant.type == 'Chat') {
                        setPage(<ChatPage id={id} />);
                    } else if (assistant.type == 'Search') {
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

        if(id) {            
            fetchData();
        } else {            
            setPage(<ChatPage />);            
        }

    }, [id, navigate]);

    if (loading) {
        return <div>Loading...</div>;
    }

    return <>{page}</>;
};

export default ChatRouter;