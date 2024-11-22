import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { ScrollArea } from '@/components/ui/scroll-area';
import SimpleHeading from '@/components/Heading-Simple';
import { getApiUri } from '@/services/uri-helpers';
import axios from '@/error-handling/axiosSetup';
import MessageContent from '@/components/chat-page/message-content';
import { ChatMessageArea } from '@/components/chat-page/chat-message-area';
import { useAuth } from '@/services/auth-helpers';
import { createChatThread, GetChatThreadMessages } from '@/services/chatthreadservice';
import { fetchAssistantById } from '@/services/assistantservice';
import { Message } from '@/types/ChatThread';
import { Assistant } from '@/types/Assistant';
import { AxiosError } from 'axios';

const ChatPage = () => {
  const { '*': chatThreadId } = useParams();
  const urlParams = new URLSearchParams(window.location.search);
  const assistantId = urlParams.get('assistantId');
  const navigate = useNavigate();
  const { username } = useAuth();

  const [inputValue, setInputValue] = useState('');
  const [messages, setMessages] = useState<Message[]>([]);
  const [assistant, setAssistant] = useState<Assistant | null>(null);

  const [isLoading, setIsLoading] = useState(true);
  const [isMessagesLoading, setIsMessagesLoading] = useState(false);
  const [isStreaming, setIsStreaming] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Separate assistant fetching logic
  const fetchAssistant = async (id: string) => {
    try {
      const fetchedAssistant = await fetchAssistantById(id);
      if (fetchedAssistant) {
        setAssistant(fetchedAssistant);
        console.log('Assistant loaded:', fetchedAssistant);
        return fetchedAssistant;
      }
      throw new Error('Assistant not found');
    } catch (err) {
      console.error('Error fetching assistant:', err);
      setError('Failed to load assistant');
      return null;
    }
  };

  // Initialize chat thread or load existing thread
  useEffect(() => {
    let isMounted = true;

    const initializeChatThread = async () => {
      try {
        setIsLoading(true);
        let currentAssistant = null;

        // Step 1: Fetch assistant if ID is provided
        if (assistantId && isMounted) {
          currentAssistant = await fetchAssistant(assistantId);
          console.log('Chat - currentAssistant:', currentAssistant);
          if (!currentAssistant) {
            throw new Error('Failed to load assistant');
          }
        }

        // Step 2: Handle chat thread initialization when no chatThreadId is provided
        if (!chatThreadId && isMounted) {
          const newThread = await createChatThread({
            name: 'New Chat',
            userId: username,
            personaMessage: currentAssistant?.systemMessage || '',
            personaMessageTitle: currentAssistant?.name || '',
          });
          console.log('Chat - newThread:', newThread);

          if (!newThread) {
            throw new Error('Failed to create new chat thread');
          }

          const newUrl = assistantId ? `/chat/${newThread.id}?assistantId=${assistantId}` : `/chat/${newThread.id}`;
          console.log('Chat - newUrl to redirect to:', newUrl);

          navigate(newUrl, { replace: true });
        }
      } catch (err) {
        if (isMounted) {
          console.error('Chat initialization error:', err);
          setError(err instanceof Error ? err.message : 'An error occurred while initializing chat');
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    initializeChatThread();

    return () => {
      isMounted = false;
      setAssistant(null);
    };
  }, [assistantId, username, navigate]);

  // Effect to handle message fetching after chatThreadId is updated
  useEffect(() => {
    const fetchMessages = async () => {
      if (chatThreadId) {
        setIsMessagesLoading(true);
        try {
          const getMessages = await GetChatThreadMessages(username, chatThreadId, assistant);
          console.log('Chat - getMessages after new thread:', getMessages);
          setMessages(getMessages);
        } catch (err) {
          console.error('Error fetching messages:', err);
          setError('Failed to load messages');
        } finally {
          setIsMessagesLoading(false);
        }
      }
    };

    fetchMessages();
  }, [chatThreadId, username, assistant]);

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
    }
  };

  const handleSendMessage = async () => {
    if (!inputValue.trim() || !chatThreadId) return;

    const newMessage: Message = {
      id: crypto.randomUUID(),
      createdAt: new Date(),
      type: 'text',
      isDeleted: false,
      content: inputValue,
      name: username,
      role: 'user',
      threadId: chatThreadId,
      userId: username,
      multiModalImage: '',
      sender: 'user',
    };

    // Add user message to chat
    setMessages((prev) => [...prev, newMessage]);
    setIsStreaming(true);

    try {
      // Use new /chat endpoint
      const apiUrl = getApiUri('chat', {
        threadId: chatThreadId,
        ...(assistantId && { assistantId }),
      });

      console.log('Chat apiUrl: ', apiUrl);

      // Create message history format
      const messageHistory = messages.map((msg) => ({
        text: msg.content,
        role: msg.role,
      }));

      // Add current message
      messageHistory.push({
        text: inputValue,
        role: 'user',
      });

      // Clear input after storing message
      setInputValue('');

      const response = await axios.post(apiUrl, messageHistory, {
        headers: {
          Accept: 'text/plain',
          'Content-Type': 'application/json',
        },
        responseType: 'stream',
        adapter: 'fetch',
      });

      // Create placeholder for bot response
      const botMessage: Message = {
        id: crypto.randomUUID(),
        createdAt: new Date(),
        type: 'text',
        isDeleted: false,
        content: '',
        name: 'Assistant',
        role: 'assistant',
        threadId: chatThreadId,
        userId: username,
        multiModalImage: '',
        sender: 'assistant',
      };
      setMessages((prev) => [...prev, botMessage]);

      // Handle streaming response
      const stream: ReadableStream = response.data;
      const reader = stream.getReader();
      const decoder = new TextDecoder('utf-8');

      while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        const chunk = decoder.decode(value, { stream: true });

        setMessages((prev) => {
          const lastMessage = prev[prev.length - 1];
          const updatedMessage = {
            ...lastMessage,
            content: lastMessage.content + chunk,
          };
          return [...prev.slice(0, prev.length - 1), updatedMessage];
        });
      }
    } catch (err) {
      let message = 'failed to send message';
      const axiosErr = err as AxiosError;
      const stream = axiosErr.response?.data as ReadableStream;
      if (stream) {
        const read = await stream.getReader().read();
        message = new TextDecoder('utf-8').decode(read.value).replace(/^"(.*)"$/, '$1');
      }
      console.error('Error sending message:', err);
      setError(message);
    } finally {
      setIsStreaming(false);
      setInputValue('');
    }
  };

  if (isLoading) {
    return <div className="flex h-screen items-center justify-center">Loading...</div>;
  }

  return (
    <div className="flex h-screen bg-background text-foreground">
      <div className="flex-1 flex flex-col">
      <SimpleHeading
        Title={assistant ? assistant.name : 'Chat'}
        Subtitle={assistant ? assistant.description : 'Why not have a chat'}
        DocumentCount={messages.length}
        threadId={chatThreadId}
      />

        {error && <div className="p-4 bg-red-100 text-red-700 rounded-md m-4">{error}</div>}

        <ScrollArea className="flex-1 p-4 space-y-4">
          {isMessagesLoading ? (
            <div className="flex justify-center items-center h-full">Loading messages...</div>
          ) : (
            messages.map(
              (message, index) => (
                //message.sender !== 'system' && (
                <ChatMessageArea
                  key={index}
                  profileName={message.sender === 'user' ? username || 'User' : assistant?.name || 'AI Assistant'}
                  role={message.sender === 'user' ? 'user' : 'assistant'}
                  onCopy={() => {
                    navigator.clipboard.writeText(message.content);
                  }}
                  profilePicture={message.sender === 'user' ? '' : '/agile.png'}
                >
                  <MessageContent
                    message={{
                      role: message.sender === 'user' ? 'user' : 'assistant',
                      content: message.content,
                      name: message.sender,
                    }}
                  />
                </ChatMessageArea>
              )
              //)
            )
          )}
        </ScrollArea>

        <div className="p-4 border-t">
          <Textarea
            placeholder="Type your message here..."
            className="w-full mb-2"
            rows={4}
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyDown={handleKeyDown}
            autoFocus
            aria-label="Chat Input"
            accessKey="i"
          />

          <Button
            onClick={handleSendMessage}
            disabled={isStreaming || !chatThreadId || isMessagesLoading}
            aria-label="Send Chat"
            accessKey="j"
          >
            {isStreaming ? 'Sending...' : 'Send'}
          </Button>
        </div>
      </div>
    </div>
  );
};

export default ChatPage;
