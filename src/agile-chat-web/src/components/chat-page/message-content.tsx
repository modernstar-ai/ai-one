import { Markdown } from '@/components/markdown/markdown';
import { Citation, Message } from '@/types/ChatThread';
import { CitationSheet } from './citation-sheet';
import { ChatSearchResponse } from './chat-search-response';

interface MessageContentProps {
  message: Message;
  assistantId?: string;
}

const MessageContent = (props: MessageContentProps) => {
  const { message, assistantId } = props;

  //  // Validate content
  // if (!content || typeof content !== "string") {
  //   console.error("Invalid message content:", content);
  //   return <></>;
  // }

  const handleCitationClick = async () => {
    try {
      return <></>;
    } catch (error) {
      console.error('Error handling citation:', error);
      return <></>;
    }
  };

  if (message.options.metadata.SearchProcess) {
    return <ChatSearchResponse message={message} assistantId={assistantId!} />;
  }

  return (
    <>
      <Markdown content={message.content} onCitationClick={handleCitationClick}></Markdown>

      {/* Handle citations */}
      {message.options.metadata.Citations && (
        <div className="citations">
          <h5 className="font-bold">Citations:</h5>
          {(message.options.metadata.Citations as Citation[]).map((citation, index) =>
            citation.name && citation.url ? (
              <CitationSheet index={index + 1} citation={citation} key={message.id + index} assistantId={assistantId} />
            ) : (
              <li key={index}>Invalid citation data</li>
            )
          )}
        </div>
      )}
    </>
  );
};

export default MessageContent;
