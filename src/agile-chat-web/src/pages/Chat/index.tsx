import { useState, useEffect, useRef } from 'react';
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
import { consumeChunks, createTempMessage, ResponseType, updateMessages } from './utils';
import { flushSync } from 'react-dom';
import { Assistant } from '@/types/Assistant';
import { fetchAssistantById } from '@/services/assistantservice';

const ChatPage = () => {
  const { threadId } = useParams();
  const navigate = useNavigate();

  if (!threadId) navigate('/');
  const [thread, setThread] = useState<ChatThread | undefined>(undefined);
  const [assistant, setAssistant] = useState<Assistant | undefined>(undefined);

  const [userInput, setUserInput] = useState<string>('');
  const [messagesDb, setMessagesDb] = useState<Message[] | undefined>(undefined);
  const [isLoading, setIsLoading] = useState(true);
  const [isSending, setIsSending] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const userInputRef = useRef<HTMLTextAreaElement>(null);
  const scrollContainerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    // Scroll to the bottom of the container
    scrollContainerRef.current?.scrollIntoView();
  }, [messagesDb]); // This effect runs whenever 'items' changes

  useEffect(() => {
    setIsLoading(true);
    refreshThread().then((thread) => {
      if (thread?.assistantId) {
        fetchAssistantById(thread.assistantId).then((assistant) => {
          setAssistant(assistant ?? undefined);
        });
      }
      GetChatThreadMessages(thread!.id)
        .then((messages) => {
          setMessagesDb(messages);
        })
        .finally(() => setIsLoading(false));
    });
  }, [threadId]);

  const refreshThread = async () => {
    const thread = await fetchChatThread(threadId!);
    if (!thread) navigate('/');
    setThread(thread!);
    return thread;
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
    }
  };

  const handleSendMessage = async () => {
    if (!userInput.trim() || !thread) return;

    const userPrompt = userInput.trim();
    userInputRef.current!.value = '';

    const tempUserMessage: Message = createTempMessage(userPrompt, MessageType.User);
    let newMessages = [...messagesDb!, tempUserMessage];
    setMessagesDb(newMessages);

    try {
      setIsSending(true);

      const response = await chat({
        threadId: thread.id,
        userPrompt: userPrompt,
      } as ChatDto);

      // // Handle streaming response
      const stream = response.data as ReadableStream;
      const reader = stream.getReader();
      const decoder = new TextDecoder('utf-8');
      let assistantResponse = '';
      newMessages = [...newMessages, createTempMessage(assistantResponse, MessageType.Assistant)];

      for await (const value of consumeChunks(reader, decoder)) {
        if (value[ResponseType.Chat]) {
          assistantResponse = assistantResponse + value[ResponseType.Chat];
          const assistantMessage: Message = createTempMessage(assistantResponse, MessageType.Assistant);
          newMessages = updateMessages(newMessages, assistantMessage);
          flushSync(() => setMessagesDb(newMessages));
        } else if (value[ResponseType.DbMessages]) {
          const dbMsgs = value[ResponseType.DbMessages] as Message[];
          dbMsgs.forEach((msg) => {
            newMessages = updateMessages(newMessages, msg);
            flushSync(() => setMessagesDb(newMessages));
          });
        }
      }

      if (messagesDb && messagesDb.length <= 2) {
        await refreshThread();
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
      setIsSending(false);
    }
  };

  if (isLoading || !thread || !messagesDb) {
    return <div className="flex h-screen items-center justify-center">Loading...</div>;
  }

  return (
    <div className="flex h-screen bg-background text-foreground">
      <div className="flex-1 flex flex-col">
        <SimpleHeading
          Title={thread!.name ?? 'Chat'}
          Subtitle={assistant ? assistant.name : 'Why not have a chat'}
          threadId={thread!.id}
          DocumentCount={0}
        />

        {error && <div className="p-4 bg-red-100 text-red-700 rounded-md m-4">{error}</div>}

        <ScrollArea className="flex-1 p-4 space-y-4">
          {isLoading ? (
            <div className="flex justify-center items-center h-full">Loading messages...</div>
          ) : (
            <>
              {messagesDb?.map((message, index) => (
                <ChatMessageArea
                  ref={scrollContainerRef}
                  message={message}
                  key={index}
                  userId={thread!.userId || ''} // Ensure username is never undefined
                  onCopy={() => {
                    navigator.clipboard.writeText(message.content);
                  }}
                >
                  <MessageContent message={message} assistantId={thread.assistantId} />
                </ChatMessageArea>
              ))}
            </>
          )}
        </ScrollArea>

        <div className="p-4 border-t">
          <Textarea
            ref={userInputRef}
            placeholder="Type your message here..."
            className="w-full mb-2"
            rows={4}
            onChange={(e) => setUserInput(e.target.value)}
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
