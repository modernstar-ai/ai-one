import { useState, useEffect, useRef } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import { ScrollArea } from '@/components/ui/scroll-area';
import SimpleHeading from '@/components/Heading-Simple';
import MessageContent from '@/components/chat-page/message-content';
import { ChatMessageArea } from '@/components/chat-page/chat-message-area';
import {
  DeleteChatThreadFile,
  fetchChatThread,
  GetChatThreadFiles,
  GetChatThreadMessages,
  updateChatThread,
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
import { ChatThreadFile } from '@/types/ChatThread';
import ChatInput from '@/components/chat-page/chat-input';
import { Loader2 } from 'lucide-react';
import { useToast } from '@/hooks/use-toast';

const ChatPage = () => {
  const { threadId } = useParams();
  const navigate = useNavigate();

  if (!threadId) navigate('/');
  const [thread, setThread] = useState<ChatThread | undefined>(undefined);
  const [threadFiles, setThreadFiles] = useState<ChatThreadFile[] | undefined>(undefined);
  const [assistant, setAssistant] = useState<Assistant | undefined>(undefined);
  const { toast } = useToast();

  const [userInput, setUserInput] = useState<string>('');
  const prevMessagesRef = useRef<Message[] | undefined>(undefined);

  const { clearStream, setStream } = useStreamStore();

  const [isLoading, setIsLoading] = useState(true);
  const [swappingModels, setSwappingModels] = useState(false);
  const [isSending, setIsSending] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const userInputRef = useRef<HTMLTextAreaElement>(null);
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
      (e.target as any).value = null;
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

      const payload: ChatDto = {
        threadId: thread.id,
        userPrompt: userPrompt
      };

      const response = await chat(payload);

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
      setTimeout(() => userInputRef.current?.focus(), 0);
    }
  };

  if (isLoading || !thread || !prevMessagesRef.current) {
    return (
      <div className="h-screen flex justify-center items-center">
        <Loader2 className="animate-spin" />
      </div>
    );
  }

  return (
    <div className="flex h-[95vh] lg:h-screen bg-background text-foreground">
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

        <ChatInput
          handleFileUpload={handleFileUpload}
          handleFileDelete={handleFileDelete}
          thread={thread}
          threadFiles={threadFiles}
          assistant={assistant}
          isSending={swappingModels || isSending}
          userInputRef={userInputRef}
          setUserInput={setUserInput}
          disableSelect={swappingModels}
          handleModelChange={async (v) => {
            if (!v) return;
            setSwappingModels(true);
            try {
              await updateChatThread({ ...thread, modelOptions: { modelId: v } });
              toast({ value: 'Model Swapped' });
            } catch {
              toast({ variant: 'destructive', value: 'Error swapping models' });
            } finally {
              setSwappingModels(false);
            }
          }}
          defaultModel={thread.modelOptions?.modelId}
          handleSendMessage={handleSendMessage}
          handleKeyDown={handleKeyDown}
          settings={settings}
        />
      </div>
    </div>
  );
};

export default ChatPage;
