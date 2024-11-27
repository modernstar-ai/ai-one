"use client";

import { cn } from "@/lib/utils";
import {
  CheckIcon,
  /*ClipboardIcon,*/
  Copy,
  PocketKnife,
  UserCircle,
} from "lucide-react";
import { useEffect, useState } from "react";
import { Avatar, AvatarImage } from "@/components/common/avatar";
import { Button } from "@/components/common/button";
import MessageReactions from "@/components/chat-page/messagereactions";

interface ChatMessageAreaProps {
  children?: React.ReactNode;
  profilePicture?: string | null;
  profileName?: string;
  role: "function" | "user" | "assistant" | "system" | "tool";
  onCopy: () => void;
  messageId: string;
  userId: string;
  initialLikes?: boolean;
  initialDislikes?: boolean;
}

export const ChatMessageArea: React.FC<ChatMessageAreaProps> = ({
  children,
  profilePicture,
  profileName,
  role,
  onCopy,
  messageId,
  userId,
  initialLikes = false,
  initialDislikes = false,
}) => {
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
    switch (role) {
      case "assistant":
      case "user":
        if (profilePicture) {
          return (
            <Avatar className="h-5 w-5">
              <AvatarImage src={profilePicture} />
            </Avatar>
          );
        }
        return (
          <UserCircle
            size={20}
            strokeWidth={1.4}
            className="text-muted-foreground"
          />
        );
      case "tool":
      case "function":
        return (
          <PocketKnife
            size={20}
            strokeWidth={1.4}
            className="text-muted-foreground"
          />
        );
      default:
        return null;
    }
  };

  return (
    <div className="flex flex-col">
      <div className="h-6 flex items-center">
        <div className="flex gap-2">
          {renderProfile()}
          <div
            className={cn(
              "text-primary capitalize items-center flex text-sm",
              role === "function" || role === "tool"
                ? "text-muted-foreground text-xs"
                : ""
            )}
          >
            {profileName}
          </div>
        </div>
        <div className=" h-7 flex items-center justify-between">
          <div>
            <Button
              variant={"ghost"}
              size={"sm"}
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
          <MessageReactions
            messageId={messageId}
            userId={userId}
            initialLikes={initialLikes}
            initialDislikes={initialDislikes}
          />
        </div>
      </div>
    </div>
  );
};
