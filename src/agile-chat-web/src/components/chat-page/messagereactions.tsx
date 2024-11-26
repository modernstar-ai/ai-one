"use client";

import React, { useState } from 'react';
import { ThumbsUp, ThumbsDown } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from "@/lib/utils";
import { 
  addLikeReaction, 
  removeLikeReaction,
  addDislikeReaction,
  removeDislikeReaction 
} from '@/services/chatthreadservice';

interface MessageReactionsProps {
  messageId: string;
  userId: string;
  initialLikes?: boolean;
  initialDislikes?: boolean;
  disabled?: boolean;
}

const MessageReactions: React.FC<MessageReactionsProps> = ({ 
  messageId, 
  userId,
  initialLikes = false,
  initialDislikes = false,
  disabled = false 
}) => {
  // Initialize state directly from props
  const [isLiked, setIsLiked] = useState<boolean>(initialLikes);
  const [isDisliked, setIsDisliked] = useState<boolean>(initialDislikes);
  const [isLoading, setIsLoading] = useState<boolean>(false);

  const handleLike = async (): Promise<void> => {
    if (isLoading || disabled) return;
    setIsLoading(true);

    try {
      if (isLiked) {
        // Remove like
        const success = await removeLikeReaction(messageId, userId);
        if (success) {
          setIsLiked(false);
        }
      } else {
        // Add like and remove dislike if present
        const success = await addLikeReaction(messageId, userId);
        if (success) {
          setIsLiked(true);
          if (isDisliked) {
            await removeDislikeReaction(messageId, userId);
            setIsDisliked(false);
          }
        }
      }
    } catch (error) {
      console.error('Error handling like:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDislike = async (): Promise<void> => {
    if (isLoading || disabled) return;
    setIsLoading(true);

    try {
      if (isDisliked) {
        // Remove dislike
        const success = await removeDislikeReaction(messageId, userId);
        if (success) {
          setIsDisliked(false);
        }
      } else {
        // Add dislike and remove like if present
        const success = await addDislikeReaction(messageId, userId);
        if (success) {
          setIsDisliked(true);
          if (isLiked) {
            await removeLikeReaction(messageId, userId);
            setIsLiked(false);
          }
        }
      }
    } catch (error) {
      console.error('Error handling dislike:', error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex items-center gap-1">
      <Button
        variant="ghost"
        size="icon"
        onClick={handleLike}
        disabled={isLoading || disabled}
        className={cn(
          "h-8 w-8",
          isLiked 
            ? "bg-gray-100 text-gray-950 hover:bg-gray-200 hover:text-gray-950" 
            : "text-gray-500 hover:text-gray-700 hover:bg-gray-100",
          (isLoading || disabled) && "opacity-50 cursor-not-allowed"
        )}
        title={isLiked ? 'Remove like' : 'Like'}
      >
        <ThumbsUp size={16} />
      </Button>
  
      <Button
        variant="ghost"
        size="icon"
        onClick={handleDislike}
        disabled={isLoading || disabled}
        className={cn(
          "h-8 w-8",
          isDisliked 
            ? "bg-gray-100 text-gray-950 hover:bg-gray-200 hover:text-gray-950" 
            : "text-gray-500 hover:text-gray-700 hover:bg-gray-100",
          (isLoading || disabled) && "opacity-50 cursor-not-allowed"
        )}
        title={isDisliked ? 'Remove dislike' : 'Dislike'}
      >
        <ThumbsDown size={16} />
      </Button>
    </div>
  );
};

export default MessageReactions;