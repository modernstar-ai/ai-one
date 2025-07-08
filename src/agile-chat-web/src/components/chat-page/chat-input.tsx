import { Paperclip, Send, XIcon } from 'lucide-react';
import { Textarea } from '@/components/ui/textarea';
import { Button } from '@/components/ui/button';

import { RefObject, ChangeEvent, KeyboardEvent, useRef } from 'react';
import { BaseSelect } from '../base/BaseSelect';
import { Assistant } from '@/types/Assistant';
import { Input } from '../ui/input';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger
} from '../ui/dropdown-menu';
import { Badge } from '../ui/badge';
import { ChatThread, ChatThreadFile } from '@/types/ChatThread';

interface ChatInputProps {
  handleFileUpload: (e: React.ChangeEvent<HTMLInputElement>) => Promise<void>;
  handleFileDelete: (fileId: string) => Promise<void>;
  threadFiles: ChatThreadFile[] | undefined;
  thread: ChatThread;
  userInputRef: RefObject<HTMLTextAreaElement>;
  assistant?: Assistant;
  setUserInput: (value: string) => void;
  handleKeyDown: (event: KeyboardEvent<HTMLTextAreaElement>) => void;
  handleSendMessage: () => void;
  handleModelChange: (model: string | null) => void;
  defaultModel?: string;
  isSending: boolean;
  settings?: {
    aiDisclaimer?: string;
  };
  disableSelect?: boolean;
}

export default function ChatInput({
  handleFileDelete,
  handleFileUpload,
  thread,
  threadFiles,
  userInputRef,
  setUserInput,
  handleKeyDown,
  handleSendMessage,
  isSending,
  settings,
  assistant,
  handleModelChange,
  defaultModel,
  disableSelect
}: ChatInputProps) {
  const fileUploadRef = useRef<HTMLInputElement>(null);

  return (
    <div className="p-4 border-t space-y-3">
      <Textarea
        ref={userInputRef}
        placeholder="Type your message here..."
        className="w-full"
        rows={4}
        onChange={(e: ChangeEvent<HTMLTextAreaElement>) => setUserInput(e.target.value)}
        onKeyDown={handleKeyDown}
        autoFocus
        aria-label="Chat Input"
        accessKey="i"
        disabled={disableSelect || isSending}
      />

      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
        <p className="text-xs text-center sm:text-right text-muted-foreground hidden lg:block">
          {settings?.aiDisclaimer?.trim()
            ? settings.aiDisclaimer
            : 'AI generated text can have mistakes. Check important info.'}
        </p>
        <div className="flex items-center gap-2">
          {/* UPLOAD FILE */}
          {(!thread.assistantId || assistant?.filterOptions.allowInThreadFileUploads) && (
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
                  {threadFiles?.map((file, index) => (
                    <div className="flex items-center" key={file.name + index}>
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
          )}
          {assistant?.modelOptions.allowModelSelection ? (
            <BaseSelect
              disabled={disableSelect}
              defaultValue={defaultModel || assistant.modelOptions.defaultModelId}
              placeholder="Select AI Model"
              onChange={handleModelChange}
              options={assistant.modelOptions.models
                .filter((model) => model.isSelected)
                .map((filteredModel) => ({
                  label: filteredModel.modelId,
                  value: filteredModel.modelId
                }))}
            />
          ) : null}
          <Button
            onClick={handleSendMessage}
            disabled={isSending || disableSelect}
            aria-label="Send Chat"
            accessKey="j">
            <Send className="w-4 h-4 mr-2" />
            Send
          </Button>
        </div>
        <p className="text-xs text-center sm:text-right text-muted-foreground lg:hidden">
          {settings?.aiDisclaimer?.trim()
            ? settings.aiDisclaimer
            : 'AI generated text can have mistakes. Check important info.'}
        </p>
      </div>
    </div>
  );
}
