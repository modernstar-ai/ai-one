import { Markdown } from "@/components/markdown/markdown";
import React from "react";

interface Citation {
  fileName: string;
  fileUrl: string;
}

interface MessageContentProps {
  message: {
    role: string;
    content: string;
    name: string;
    citations?: Citation[]; 
    multiModalImage?: string;
  };
}

const MessageContent: React.FC<MessageContentProps> = ({ message }) => {
  const { role, content, citations, multiModalImage } = message;

  //  // Validate content
  // if (!content || typeof content !== "string") {
  //   console.error("Invalid message content:", content);
  //   return <></>;
  // }

  const handleCitationClick = async () => {
    try {
      return <></>;
    } catch (error) {
      console.error("Error handling citation:", error);
      return <></>; 
    }
  };

  if (role === "assistant" || role === "user") {
    return (
      <>
        <Markdown
          content={content}  onCitationClick={handleCitationClick}          
        ></Markdown>

        {/* Handle citations */}
        {citations === undefined ? (
          <p></p>
        ) : citations.length > 0 ? (
          <div className="citations">
            <h5>Citations:</h5>
            <ul>
              {citations.map((citation, index) =>
                citation.fileName && citation.fileUrl ? (
                  <li key={index}>
                    <a
                      href={citation.fileUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-blue-600 hover:underline"
                    >
                      {citation.fileName}
                    </a>
                  </li>
                ) : (
                  <li key={index}>Invalid citation data</li>
                )
              )}
            </ul>
          </div>
        ) : (
          <p></p>
        )}

        {multiModalImage && <img src={multiModalImage} />}
      </>
    );
  }

  return null;
};



export default MessageContent;