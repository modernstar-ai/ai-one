import { useState,useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { ScrollArea } from '@/components/ui/scroll-area';
import SimpleHeading from '@/components/Heading-Simple';
import { getApiUri } from '@/services/uri-helpers';
import axios from '@/error-handling/axiosSetup';
import { fetchAssistantById } from '@/services/assistantservice';
import { useParams } from 'react-router-dom';

const ChatPage = () => {
  const [messages, setMessages] = useState<{ text: string; sender: string }[]>([]);
  const [inputValue, setInputValue] = useState('');
  const [isStreaming, setIsStreaming] = useState(false);
  const [assistant, setAssistant] = useState<any>(null);
  const { id } = useParams<{ id: string }>();
  
  useEffect(() => {
    const fetchData = async () => {
        try {
            const fetchedAssistant = await fetchAssistantById(id);
            setAssistant(fetchedAssistant);
            console.log('chat assistant:', assistant);
            if (fetchedAssistant) {
                console.log('ChatRouter assistant.Type:', fetchedAssistant.type);
                const newMessages = [...messages, 
                  { text: fetchedAssistant.systemMessage, sender: 'system' },
                  { text: fetchedAssistant.greeting, sender: 'assistant' }];
                setMessages(newMessages);
            } else {
                console.log('chat assistant not found:', id);                
            }
        } catch (error) {
            console.error('Error fetching chat assistant:', error);                
        }
    };

    fetchData();
}, [id]);


  const handleSendMessage = async () => {
    if (inputValue.trim()) {
      // Add the user's message to the chat
      const newMessages = [...messages, { text: inputValue, sender: 'user' }];
      setMessages(newMessages);
      setIsStreaming(true); // Set the streaming flag to true

      console.log('messages:', messages);
      
      // Establish an SSE connection for the bot's response
      const apiUrl = getApiUri('chatcompletions', { prompt: encodeURIComponent(inputValue) });

      axios
        .get(apiUrl, {
          headers: {
            Accept: 'text/plain',
          },
          responseType: 'stream',
          adapter: 'fetch',
        })
        .then(async (resp) => {
          const stream: ReadableStream = resp.data;

          const reader = stream.getReader();
          const decoder = new TextDecoder('utf-8');
          while (true) {
            const { done, value } = await reader.read();
            const partialChunk = decoder.decode(value, { stream: true });

            // Update the last bot message with the streamed data
            setMessages((prevMessages) => {
              const lastMessage = prevMessages[prevMessages.length - 1];
              const updatedBotMessage = { ...lastMessage, text: lastMessage.text + partialChunk };
              return [...prevMessages.slice(0, prevMessages.length - 1), updatedBotMessage];
            });

            if (done) {
              break;
            }
          }
        })
        .finally(() => setIsStreaming(false));

      // Add the bot's message placeholder to the chat
      setMessages((prevMessages) => [
        ...prevMessages,
        { text: '', sender: 'bot' }, // Start with an empty message for the bot
      ]);

      // Clear the input field after sending the message
      setInputValue('');
    }
  };

  return (
    <div className="flex h-screen bg-background text-foreground">

      {/* Main Chat Area */}
      <div className="flex-1 flex flex-col">
        {/* Header */}
        <SimpleHeading 
          Title={assistant ? assistant.name : "Chat"}
          Subtitle={assistant ? assistant.description : ""}
          DocumentCount={0} />

        <ScrollArea className="flex-1 p-4 space-y-4">
          {messages.map((message, index) => (
            message.sender !== 'system' && (
            <div
              key={index}
              className={`w-full my-4 p-4 flex ${message.sender === 'user' ? 'justify-end' : 'justify-start'}`}
            >
              <div
                className={`p-6 rounded-lg`}
                style={{
                  backgroundColor: message.sender === 'user' ? 'var(--chat-user-bg-color)' : 'var(--chat-bot-bg-color)',
                  color: message.sender === 'user' ? 'var(--chat-user-text-color)' : 'var(--chat-bot-text-color)',
                  width: '60%',
                }}
              >
                {message.text}
              </div>
            </div>
            )
          ))}
        </ScrollArea>
        {/* Input Area */}
        <div className="p-4 border-t">
          <Textarea
            placeholder="Type your message here..."
            className="w-full mb-2"
            rows={4}
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            autoFocus
            aria-label="Chat Input"
            accessKey="i"
          />
          <Button onClick={handleSendMessage} disabled={isStreaming} aria-label="Send Chat" accessKey="j">
            Send
          </Button>
        </div>
      </div>
    </div>
  );
};

export default ChatPage;
