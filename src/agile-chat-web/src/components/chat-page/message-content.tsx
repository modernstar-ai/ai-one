import { Markdown } from "@/components/markdown/markdown";
import React from "react";

interface MessageContentProps {
  message: {
    role: string;
    content: string;
    name: string;
    multiModalImage?: string;
  };
}

const MessageContent: React.FC<MessageContentProps> = ({ message }) => {
  if (message.role === "assistant" || message.role === "user") {
    return (
      <>
        <Markdown
          content={message.content}          
        ></Markdown>
        {message.multiModalImage && <img src={message.multiModalImage} />}
      </>
    );
  }

  return null;
};



export default MessageContent;