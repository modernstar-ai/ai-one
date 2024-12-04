'use client';

import React, { useState } from 'react';
import { ThumbsUp, ThumbsDown } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { Message, MessageOptions } from '@/types/ChatThread';
import { updateReaction } from '@/services/chatthreadservice';

interface MessageReactionsProps {
  message: Message;
}

const MessageReactions: React.FC<MessageReactionsProps> = ({ message }) => {
  // Initialize state directly from props
  const [isLoading, setIsLoading] = useState<boolean>(false);

  const updateMessage = async (options: MessageOptions): Promise<void> => {
    if (isLoading) return;
    setIsLoading(true);

    try {
      await updateReaction(message.id, options);
      message.options = options;
    } catch (error) {
      console.error('Error handling like:', error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex items-center gap-1">
      <Button
        variant="ghost"
        size="icon"
        onClick={() => updateMessage({ ...message.options, isLiked: !message.options.isLiked })}
        disabled={isLoading}
        className={cn(
          'h-8 w-8',
          message.options.isLiked
            ? 'bg-gray-100 text-gray-950 hover:bg-gray-200 hover:text-gray-950'
            : 'text-gray-500 hover:text-gray-700 hover:bg-gray-100',
          isLoading && 'opacity-50 cursor-not-allowed'
        )}
        title={message.options.isLiked ? 'Remove like' : 'Like'}
      >
        <ThumbsUp size={16} />
      </Button>

      <Button
        variant="ghost"
        size="icon"
        onClick={() => updateMessage({ ...message.options, isDisliked: !message.options.isDisliked })}
        disabled={isLoading}
        className={cn(
          'h-8 w-8',
          message.options.isDisliked
            ? 'bg-gray-100 text-gray-950 hover:bg-gray-200 hover:text-gray-950'
            : 'text-gray-500 hover:text-gray-700 hover:bg-gray-100',
          isLoading && 'opacity-50 cursor-not-allowed'
        )}
        title={message.options.isDisliked ? 'Remove dislike' : 'Dislike'}
      >
        <ThumbsDown size={16} />
      </Button>
    </div>
  );
};

export default MessageReactions;
