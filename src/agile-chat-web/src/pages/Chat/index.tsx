import { useState, useEffect, useMemo } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { ScrollArea } from '@/components/ui/scroll-area';
import SimpleHeading from '@/components/Heading-Simple';
import MessageContent from '@/components/chat-page/message-content';
import { ChatMessageArea } from '@/components/chat-page/chat-message-area';
import { fetchChatThread, GetChatThreadMessages } from '@/services/chatthreadservice';
import { ChatThread, Message, MessageType } from '@/types/ChatThread';
import { AxiosError } from 'axios';
import { chat } from '@/services/chat-completions-service';
import { ChatDto } from '@/types/ChatCompletions';
import { consumeChunks } from './utils';

interface MiniMessage {
  content: string;
  type: MessageType;
}

const ChatPage = () => {
  const { threadId } = useParams();
  const navigate = useNavigate();

  if (!threadId) navigate('/');
  const [thread, setThread] = useState<ChatThread | undefined>(undefined);

  const [inputValue, setInputValue] = useState('');
  const [messagesDb, setMessagesDb] = useState<Message[] | undefined>(undefined);
  const miniMessages = useMemo(() => {
    return (
      messagesDb?.map<MiniMessage>((message) => ({ content: message.content, type: message.type } as MiniMessage)) ?? []
    );
  }, [messagesDb]);
  const [messages, setMessages] = useState<MiniMessage[]>([]);
  useEffect(() => {
    setMessages(miniMessages);
  }, [miniMessages]);

  const [isLoading, setIsLoading] = useState(true);
  const [isSending, setIsSending] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setIsLoading(true);
    fetchChatThread(threadId!).then((thread) => {
      if (!thread) navigate('/error');
      setThread(thread!);
      GetChatThreadMessages(thread!.id)
        .then((messages) => {
          setMessagesDb(messages);
        })
        .finally(() => setIsLoading(false));
    });
  }, []);

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
    }
  };

  const handleSendMessage = async () => {
    if (!inputValue.trim() || !thread) return;

    const userPrompt = inputValue.trim();

    try {
      setIsSending(true);
      const messages = [...(miniMessages ?? []), { content: userPrompt, type: MessageType.User } as MiniMessage];
      setMessages(messages);

      // Clear input after storing message
      setInputValue('');

      const response = await chat({ threadId: thread.id, userPrompt: userPrompt } as ChatDto);

      // // Handle streaming response
      const stream = response.data as ReadableStream;
      const reader = stream.getReader();
      const decoder = new TextDecoder('utf-8');

      for await (const value of consumeChunks(reader, decoder)) {
        console.log(value);
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
      setInputValue('');
      setIsSending(false);
    }
  };

  if (isLoading) {
    return <div className="flex h-screen items-center justify-center">Loading...</div>;
  }

  return (
    <div className="flex h-screen bg-background text-foreground">
      <div className="flex-1 flex flex-col">
        <SimpleHeading
          Title={thread!.name ?? 'Chat'}
          Subtitle={'Why not have a chat'}
          threadId={thread!.id}
          DocumentCount={0}
        />

        {error && <div className="p-4 bg-red-100 text-red-700 rounded-md m-4">{error}</div>}

        <ScrollArea className="flex-1 p-4 space-y-4">
          {isLoading ? (
            <div className="flex justify-center items-center h-full">Loading messages...</div>
          ) : (
            messages?.map(
              (message, index) => (
                //message.sender !== 'system' && (
                <ChatMessageArea
                  key={index}
                  messageId={messagesDb![index].id}
                  userId={thread!.userId || ''} // Ensure username is never undefined
                  profileName={messagesDb![index].type === MessageType.User ? thread!.userId || 'User' : 'AI Assistant'}
                  role={messagesDb![index].type === MessageType.User ? 'user' : 'assistant'}
                  onCopy={() => {
                    navigator.clipboard.writeText(message.content);
                  }}
                  profilePicture={messagesDb![index].type === MessageType.User ? '' : '/agile.png'}
                  initialLikes={messagesDb![index].options.isLiked}
                  initialDislikes={messagesDb![index].options.isDisliked}
                >
                  <MessageContent
                    message={{
                      role: message.type,
                      content: message.content,
                      name: message.type === MessageType.User ? thread!.userId || 'User' : 'AI Assistant',
                      citations: undefined,
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

          <Button onClick={handleSendMessage} disabled={isSending} aria-label="Send Chat" accessKey="j">
            {isSending ? 'Sending...' : 'Send'}
          </Button>
        </div>
      </div>
    </div>
  );
};

export default ChatPage;
