import { Send } from 'lucide-react';
import { Textarea } from '@/components/ui/textarea';
import { Button } from '@/components/ui/button';

import { RefObject, ChangeEvent, KeyboardEvent } from 'react';
import { BaseSelect } from '../base/BaseSelect';
import { Assistant } from '@/types/Assistant';

interface ChatInputProps {
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
        <p className="text-xs text-center sm:text-right text-muted-foreground">
          {settings?.aiDisclaimer?.trim()
            ? settings.aiDisclaimer
            : 'AI generated text can have mistakes. Check important info.'}
        </p>
        <div className="flex items-center gap-2">
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
      </div>
    </div>
  );
}
