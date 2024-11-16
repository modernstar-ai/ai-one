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
import { useAuth } from "@/services/auth-helpers";
import { createChatThread, fetchChatsbythreadid} from '@/services/chatthreadservice';
import { fetchAssistantById } from '@/services/assistantservice';
import { Message } from '@/types/ChatThread';
import { Assistant } from '@/types/Assistant';

const ChatPage = () => {
  const { "*": chatThreadId } = useParams();
  const urlParams = new URLSearchParams(window.location.search);
  const assistantId = urlParams.get('assistantId');
  const navigate = useNavigate();
  const { username } = useAuth();
  
  const [messages, setMessages] = useState<Message[]>([]);
  const [inputValue, setInputValue] = useState('');
  const [isStreaming, setIsStreaming] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [assistant, setAssistant] = useState<Assistant | null>(null);

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
    const initializeChatThread = async () => {
      try {
        setIsLoading(true);
        let currentAssistant = null;
        
        // Step 1: Fetch assistant if ID is provided
        if (assistantId) {
          currentAssistant = await fetchAssistant(assistantId);
          if (!currentAssistant) {
            throw new Error('Failed to load assistant');
          }
        }

        // Step 2: Handle chat thread initialization
        if (!chatThreadId) {
          // Create new chat thread
          const newThread = await createChatThread({
            name: "New Chat",
            userId: username,
            personaMessage: currentAssistant?.systemMessage || "",
            personaMessageTitle: currentAssistant?.name || ""
          });
          
          if (newThread) {
            const newUrl = assistantId 
              ? `/chat/${newThread.id}?assistantId=${assistantId}`
              : `/chat/${newThread.id}`;
            navigate(newUrl, { replace: true });
            
            // Initialize messages for new thread with assistant
            if (currentAssistant) {
              const initialMessages: Message[] = [
                {
                  id: crypto.randomUUID(),
                  createdAt: new Date(),
                  type: 'text',
                  isDeleted: false,
                  content: currentAssistant.systemMessage,
                  name: 'System',
                  role: 'system',
                  threadId: newThread.id,
                  userId: username,
                  multiModalImage: '',
                  sender: 'system'
                },
                {
                  id: crypto.randomUUID(),
                  createdAt: new Date(),
                  type: 'text',
                  isDeleted: false,
                  content: currentAssistant.greeting,
                  name: currentAssistant.name,
                  role: 'assistant',
                  threadId: newThread.id,
                  userId: username,
                  multiModalImage: '',
                  sender: 'assistant'
                }
              ];          
              setMessages(initialMessages);              
            }
          } else {
            throw new Error('Failed to create new chat thread');
          }
        } else {
          // Load existing chat thread messages
          const existingMessages = await fetchChatsbythreadid(chatThreadId);
          if (existingMessages) {
            // If we have an assistant, prepend system and greeting messages
            if (currentAssistant && existingMessages.length === 0) {
              const initialMessages: Message[] = [
                {
                  id: crypto.randomUUID(),
                  createdAt: new Date(),
                  type: 'text',
                  isDeleted: false,
                  content: currentAssistant.systemMessage,
                  name: 'System',
                  role: 'system',
                  threadId: chatThreadId,
                  userId: username,
                  multiModalImage: '',
                  sender: 'system'
                },
                {
                  id: crypto.randomUUID(),
                  createdAt: new Date(),
                  type: 'text',
                  isDeleted: false,
                  content: currentAssistant.greeting,
                  name: currentAssistant.name,
                  role: 'assistant',
                  threadId: chatThreadId,
                  userId: username,
                  multiModalImage: '',
                  sender: 'assistant'
                }
              ];
              setMessages([...initialMessages, ...existingMessages]);
            } else {
              setMessages(existingMessages);
            }
          } else {
            throw new Error('Failed to load chat messages');
          }
        }
      } catch (err) {
        console.error('Chat initialization error:', err);
        setError(err instanceof Error ? err.message : 'An error occurred while initializing chat');
      } finally {
        setIsLoading(false);
      }
    };

    initializeChatThread();

    setAssistant(null); // Clear assistant on unmount

  }, [chatThreadId, assistantId, username, navigate]);

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
      sender: 'user'
    };

    // Add user message to chat
    setMessages(prev => [...prev, newMessage]);
    setIsStreaming(true);

    try {
      // Use new /chat endpoint
      const apiUrl = getApiUri('chat', {
        threadId: chatThreadId,
        ...(assistantId && { assistantId })
      });

      console.log('Chat apiUrl: ', apiUrl);

      // Create message history format
      const messageHistory = messages.map(msg => ({
        text: msg.content,
        role: msg.role
      }));

      // Add current message
      messageHistory.push({
        text: inputValue,
        role: 'user'
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
        sender : 'assistant'
      };
      setMessages(prev => [...prev, botMessage]);

      // Handle streaming response
      const stream: ReadableStream = response.data;
      const reader = stream.getReader();
      const decoder = new TextDecoder('utf-8');

      while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        const chunk = decoder.decode(value, { stream: true });
        
        setMessages(prev => {
          const lastMessage = prev[prev.length - 1];
          const updatedMessage = {
            ...lastMessage,
            content: lastMessage.content + chunk
          };
          return [...prev.slice(0, prev.length - 1), updatedMessage];
        });
      }
    } catch (err) {
      console.error('Error sending message:', err);
      setError('Failed to send message');
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
          Title={assistant ? assistant.name : "Chat"}
          Subtitle={assistant ? assistant.description : "Why not have a chat"}
          DocumentCount={messages.length} 
        />

        {error && (
          <div className="p-4 bg-red-100 text-red-700 rounded-md m-4">
            {error}
          </div>
        )}

<ScrollArea className="flex-1 p-4 space-y-4">
  {messages.map((message, index) =>
    message.sender !== 'system' && (
      
        <ChatMessageArea
          key={index}
          profileName={message.sender === "user" ? (username || "User") : (assistant?.name || "AI Assistant")}
          role={message.sender === "user" ? "user" : "assistant"}
          onCopy={() => {
            navigator.clipboard.writeText(message.content);
          }}
          profilePicture={
            message.sender === "user" 
              ? ""
              : "/agile.png"
          }
        >  
          <MessageContent 
            message={{
              role: message.sender === "user" ? "user" : "assistant",
              content: message.content,
              name: message.sender 
            }}
          />
        </ChatMessageArea>
      
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
            disabled={isStreaming || !chatThreadId} 
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