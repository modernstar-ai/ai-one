import { Markdown } from '@/components/markdown/markdown';
import { Citation, Message } from '@/types/ChatThread';
import { CitationSheet } from './citation-sheet';
import { ChatSearchResponse } from './chat-search-response';
import { useEffect, useRef } from 'react';
import { TempIdType } from '@/pages/Chat/utils';
import useStreamStore from '@/stores/stream-store';
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from '../ui/accordion';
import { DotIcon } from 'lucide-react';

interface MessageContentProps {
  message: Message;
  assistantId?: string;
}

const MessageContent = (props: MessageContentProps) => {
  const { message, assistantId } = props;
  const citations = (message.options.metadata.Citations as Citation[]) ?? undefined;
  const documents = (message.options.metadata.DocumentsRetrieved as Citation[]) ?? undefined;

  const contentOverride = useRef<string | undefined>(undefined);

  useEffect(() => {
    if (message.id === TempIdType.Assistant) {
      useStreamStore.setState({ callback: onStreamCallback });
    }
  }, [message.id]);

  const onStreamCallback = (content: string) => {
    contentOverride.current = (contentOverride.current ?? '') + content;
  };

  //  // Validate content
  // if (!content || typeof content !== "string") {
  //   console.error("Invalid message content:", content);
  //   return <></>;
  // }

  const scrollContainerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    // Scroll to the bottom of the container
    scrollContainerRef.current?.scrollIntoView();
  }, [message, contentOverride.current]); // This effect runs whenever 'items' changes

  const handleCitationClick = async () => {
    try {
      return <></>;
    } catch (error) {
      console.error('Error handling citation:', error);
      return <></>;
    }
  };

  if ((!message.content || message.content === '') && (!contentOverride.current || contentOverride.current === '')) {
    return (
      <div className="flex">
        <DotIcon className="m-0 p-0 animate-bounce delay-200" />
        <DotIcon className="m-0 p-0 animate-bounce delay-150" />
        <DotIcon className="m-0 p-0 animate-bounce delay-75" />
      </div>
    );
  }

  if (message.options.metadata.SearchProcess) {
    return <ChatSearchResponse message={message} assistantId={assistantId!} />;
  }

  return (
    <>
      <Markdown
        content={contentOverride.current ? contentOverride.current : message.content}
        onCitationClick={handleCitationClick}></Markdown>

      {/* Handle citations */}
      <Accordion
        type="single"
        collapsible
        className="m-0 p-0 not-prose"
        defaultValue={citations?.length ?? 0 > 0 ? 'item-1' : ''}>
        {citations && citations.length > 0 && (
          <AccordionItem value="item-1" className="m-0 p-0">
            <AccordionTrigger className="m-0 p-0">Citations</AccordionTrigger>
            <AccordionContent className="m-0 p-0">
              {citations && (
                <div className="citations mb-2">
                  {citations.map((citation, index) =>
                    citation.name && citation.url ? (
                      <CitationSheet citation={citation} key={message.id + index} assistantId={assistantId} />
                    ) : (
                      <li key={index}>Invalid citation data</li>
                    )
                  )}
                </div>
              )}
            </AccordionContent>
          </AccordionItem>
        )}

        {documents && documents.length > 0 && (
          <AccordionItem value="item-2" className="m-0 p-0">
            <AccordionTrigger className="m-0 p-0">Documents Retrieved</AccordionTrigger>
            <AccordionContent className="m-0 p-0">
              {documents && (
                <div className="citations mb-2">
                  {documents.map((document, index) =>
                    document.name && document.url ? (
                      <CitationSheet citation={document} key={message.id + index} assistantId={assistantId} />
                    ) : (
                      <li key={index}>Invalid citation data</li>
                    )
                  )}
                </div>
              )}
            </AccordionContent>
          </AccordionItem>
        )}
      </Accordion>

      <div ref={scrollContainerRef}></div>
    </>
  );
};

export default MessageContent;
