import { useState, useEffect, useRef } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { ScrollArea } from '@/components/ui/scroll-area';
import SimpleHeading from '@/components/Heading-Simple';
import MessageContent from '@/components/chat-page/message-content';
import { ChatMessageArea } from '@/components/chat-page/chat-message-area';
import {
  DeleteChatThreadFile,
  fetchChatThread,
  GetChatThreadFiles,
  GetChatThreadMessages,
  UploadChatThreadFile
} from '@/services/chatthreadservice';
import { ChatThread, Message, MessageType } from '@/types/ChatThread';
import { AxiosError } from 'axios';
import { chat } from '@/services/chat-completions-service';
import { ChatDto } from '@/types/ChatCompletions';
import { createTempMessage, ResponseType, updateMessages } from './utils';
import { Assistant } from '@/types/Assistant';
import { fetchAssistantById } from '@/services/assistantservice';
import useStreamStore from '@/stores/stream-store';
import { createParser, EventSourceMessage } from 'eventsource-parser';
import { useSettingsStore } from '@/stores/settings-store';
import { Paperclip, XIcon } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { ChatThreadFile } from '@/types/ChatThread';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger
} from '@/components/ui/dropdown-menu';
import { toast } from '@/components/ui/use-toast';

const ChatPage = () => {
  const { threadId } = useParams();
  const navigate = useNavigate();

  if (!threadId) navigate('/');
  const [thread, setThread] = useState<ChatThread | undefined>(undefined);
  const [threadFiles, setThreadFiles] = useState<ChatThreadFile[] | undefined>(undefined);
  const [assistant, setAssistant] = useState<Assistant | undefined>(undefined);

  const [userInput, setUserInput] = useState<string>('');
  const prevMessagesRef = useRef<Message[] | undefined>(undefined);

  const { clearStream, setStream } = useStreamStore();

  const [isLoading, setIsLoading] = useState(true);
  const [isSending, setIsSending] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const userInputRef = useRef<HTMLTextAreaElement>(null);
  const fileUploadRef = useRef<HTMLInputElement>(null);
  const { settings } = useSettingsStore();

  useEffect(() => {
    setError(null);
    setIsLoading(true);
    refreshThread().then((thread) => {
      if (thread?.assistantId) {
        refreshAssistant(thread.assistantId);
      }
      refreshThreadFiles();

      GetChatThreadMessages(thread!.id)
        .then((messages) => {
          //setMessagesDb(messages);
          prevMessagesRef.current = messages;
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

  const refreshThreadFiles = async () => {
    const files = await GetChatThreadFiles(threadId!);
    setThreadFiles(files);
  };

  const refreshAssistant = async (assistantId: string) => {
    const assistant = await fetchAssistantById(assistantId);
    if (!assistant) navigate('/');
    setAssistant(assistant!);
    return assistant;
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
    }
  };

  const onEvent = (event: EventSourceMessage) => {
    const responseType = event.event;

    if (responseType === ResponseType.Chat) {
      const chat = JSON.parse(event.data) as { content: string };
      setStream(chat.content);
    } else if (responseType === ResponseType.DbMessages) {
      const dbMsgs = JSON.parse(event.data) as Message[];
      let newMessages = [...prevMessagesRef.current!];
      dbMsgs.forEach((msg) => {
        newMessages = updateMessages(newMessages, msg);
      });
      prevMessagesRef.current = newMessages;
      clearStream();
    }
  };

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (!files || files.length === 0) return;

    try {
      setIsSending(true);
      const file = files[0];
      const formData = new FormData();
      formData.append('file', file);
      await UploadChatThreadFile(threadId!, formData);
      await refreshThreadFiles();
      toast({
        title: 'Success',
        description: <p>File {file.name} uploaded successfully</p>,
        variant: 'default'
      });
    } catch (e: any) {
      toast({
        title: 'Error',
        description: <p>{e.response.data ?? e.message}</p>,
        variant: 'destructive'
      });
    } finally {
      setIsSending(false);
      e.target.files = null;
    }
  };

  const handleFileDelete = async (fileId: string) => {
    try {
      setIsSending(true);
      await DeleteChatThreadFile(threadId!, fileId);
      await refreshThreadFiles();
    } catch (e: any) {
      toast({
        title: 'Error',
        description: <p>{e.response.data ?? e.message}</p>,
        variant: 'destructive'
      });
    } finally {
      setIsSending(false);
    }
  };

  const handleSendMessage = async () => {
    setError(null);
    if (!userInput.trim() || !thread) return;

    const userPrompt = userInput.trim();
    userInputRef.current!.value = '';

    const tempUserMessage: Message = createTempMessage(userPrompt, MessageType.User);
    const tempAssistantMessage: Message = createTempMessage('', MessageType.Assistant);

    const newMessages = [...prevMessagesRef.current!, tempUserMessage, tempAssistantMessage];
    // Save the previous messagesDb before the update
    prevMessagesRef.current = newMessages;

    try {
      setIsSending(true);

      const response = await chat({
        threadId: thread.id,
        userPrompt: userPrompt
      } as ChatDto);

      const parser = createParser({ onEvent });

      const reader = response.data.getReader();
      const decoder = new TextDecoder();
      let done = false;
      while (!done) {
        const { value, done: doneReading } = await reader.read();
        done = doneReading;

        const chunkValue = decoder.decode(value);
        parser.feed(chunkValue);
      }

      if (prevMessagesRef.current && prevMessagesRef.current!.length <= 2) {
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
      prevMessagesRef.current = prevMessagesRef.current.filter(
        (msg) => msg.id !== tempUserMessage.id && msg.id !== tempAssistantMessage.id
      );
      let errorMsg;
      try {
        errorMsg = JSON.parse(message).detail;
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
      } catch (err) {
        errorMsg = message;
      }

      setError(errorMsg);
    } finally {
      setIsSending(false);
    }
  };

  if (isLoading || !thread || !prevMessagesRef.current) {
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
              {prevMessagesRef.current?.map((message, index) => (
                <ChatMessageArea
                  message={message}
                  key={index}
                  userId={thread!.userId || ''} // Ensure username is never undefined
                  onCopy={() => {
                    navigator.clipboard.writeText(message.content);
                  }}>
                  <MessageContent message={message} />
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

          <div className="flex gap-2 items-center relative">
            <Button onClick={handleSendMessage} disabled={isSending} aria-label="Send Chat" accessKey="j">
              {isSending ? 'Sending...' : 'Send'}
            </Button>
            {/* UPLOAD FILE */}
            <div className="relative">
              <Button
                onClick={() => fileUploadRef.current?.click()}
                disabled={isSending}
                size={'icon'}
                variant={'outline'}
                title="Upload File">
                <Paperclip />
              </Button>
              <Input type="file" className="hidden" ref={fileUploadRef} onChange={handleFileUpload} />
              <DropdownMenu>
                <DropdownMenuTrigger disabled={isSending}>
                  <Badge className="absolute right-1.5 top-8 h-4 select-none">{threadFiles?.length ?? '...'}</Badge>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-56  dark text-white">
                  <DropdownMenuLabel ata-theme="dark">
                    <div className="flex flex-col space-y-1">
                      <p className="text-sm font-medium">Files Uploaded</p>
                    </div>
                  </DropdownMenuLabel>
                  <DropdownMenuSeparator />

                  {/* FILES LIST */}
                  {threadFiles?.map((file) => (
                    <div className="flex items-center">
                      <DropdownMenuItem disabled={true}>
                        <span className="truncate">{file.name}</span>
                      </DropdownMenuItem>
                      <Button
                        size={'icon'}
                        variant={'outline'}
                        className="w-6 h-6 ms-auto"
                        disabled={isSending}
                        title="Remove file"
                        onClick={() => handleFileDelete(file.id)}>
                        <XIcon />
                      </Button>
                    </div>
                  ))}

                  <DropdownMenuSeparator />
                </DropdownMenuContent>
              </DropdownMenu>
            </div>

            <p className="text-xs mx-auto">
              {settings?.aiDisclaimer && settings.aiDisclaimer != ''
                ? settings.aiDisclaimer
                : 'AI generated text can have mistakes. Check important info.'}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ChatPage;
