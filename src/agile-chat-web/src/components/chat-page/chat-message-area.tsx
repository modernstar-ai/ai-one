'use client';

import { cn } from '@/lib/utils';
import {
  CheckIcon,
  /*ClipboardIcon,*/
  Copy,
  PocketKnife,
  UserCircle,
} from 'lucide-react';
import { useEffect, useState } from 'react';
import { Avatar, AvatarImage } from '@/components/common/avatar';
import { Button } from '@/components/common/button';
import MessageReactions from '@/components/chat-page/messagereactions';
import React from 'react';
import { Message, MessageType } from '@/types/ChatThread';

interface ChatMessageAreaProps {
  children?: React.ReactNode;
  message: Message;
  onCopy: () => void;
  userId: string;
}

export const ChatMessageArea = React.forwardRef<HTMLDivElement, ChatMessageAreaProps>(
  ({ children, message, onCopy, userId }, ref) => {
    const [isIconChecked, setIsIconChecked] = useState<boolean>(false);

    const handleButtonClick = (): void => {
      onCopy();
      setIsIconChecked(true);
    };

    useEffect(() => {
      const timeout = setTimeout(() => {
        setIsIconChecked(false);
      }, 2000);
      return () => clearTimeout(timeout);
    }, [isIconChecked]);

    const renderProfile = (): JSX.Element | null => {
      switch (message.messageType) {
        case MessageType.Assistant:
          return (
            <Avatar className="h-5 w-5">
              <AvatarImage src="/agile.png" />
            </Avatar>
          );
        case MessageType.User:
          return <UserCircle size={20} strokeWidth={1.4} className="text-muted-foreground" />;
        default:
          return <PocketKnife size={20} strokeWidth={1.4} className="text-muted-foreground" />;
      }
    };

    return (
      <div className="flex flex-col" ref={ref}>
        <div className="h-6 flex items-center">
          <div className="flex gap-2">
            {renderProfile()}
            <div className={cn('text-primary capitalize items-center flex text-sm')}>
              {message.messageType === MessageType.User ? userId : 'AI Assistant'}
            </div>
          </div>
          <div className=" h-7 flex items-center justify-between">
            <div>
              <Button
                variant={'ghost'}
                size={'sm'}
                title="Copy text"
                className="justify-right flex"
                onClick={handleButtonClick}
              >
                {isIconChecked ? <CheckIcon size={16} /> : <Copy size={16} />}
              </Button>
            </div>
          </div>
        </div>
        <div className="flex flex-col flex-1 px-10">
          <div className="prose prose-slate dark:prose-invert whitespace-break-spaces prose-p:leading-relaxed prose-pre:p-0 max-w-none">
            {children}
          </div>
          <div className="flex justify-start items-center gap-1 mt-2">
            {/* <Button
            variant="ghost"
            size="icon"
            title="Copy text"
            className="h-8 w-8 text-muted-foreground hover:text-foreground"
            onClick={handleButtonClick}
          >
            {isIconChecked ? (
              <CheckIcon size={16} />
            ) : (
              <ClipboardIcon size={16} />
            )}
          </Button> */}
            <MessageReactions message={message} />
          </div>
        </div>
      </div>
    );
  }
);
