import { Citation, Message, SearchProcess } from '@/types/ChatThread';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Tabs } from '@radix-ui/react-tabs';
import { TabsContent, TabsList, TabsTrigger } from '../ui/tabs';
import { CitationSheet } from './citation-sheet';
import { Book, Lightbulb, MessageCircleMore } from 'lucide-react';

interface ChatSearchResponseProps {
  message: Message;
  assistantId: string;
}

export const ChatSearchResponse = (props: ChatSearchResponseProps) => {
  const { message, assistantId } = props;
  const searchProcess = message.options.metadata.SearchProcess as SearchProcess;
  searchProcess.Citations = (message.options.metadata.Citations as Citation[]) ?? [];

  return (
    <Tabs defaultValue="answer">
      <TabsList className="grid w-full grid-cols-3">
        <TabsTrigger value="answer">
          <span className="flex items-center justify-center gap-2">
            <MessageCircleMore size={18} /> Answer
          </span>
        </TabsTrigger>
        <TabsTrigger value="thought process">
          <span className="flex items-center justify-center gap-2">
            <Lightbulb size={18} /> Thought Process
          </span>
        </TabsTrigger>
        <TabsTrigger value="supporting content">
          <span className="flex items-center justify-center gap-2">
            <Book size={18} /> Supporting Content
          </span>
        </TabsTrigger>
      </TabsList>
      <TabsContent value="answer">
        <Card>
          <CardHeader>
            <CardTitle>AI Assistant</CardTitle>
            <CardDescription>Response summary</CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
            <div className="space-y-1">
              <p>{message.content}</p>
            </div>
          </CardContent>
        </Card>
      </TabsContent>
      <TabsContent value="thought process">
        <Card>
          <CardHeader>
            <CardTitle>Thought Process</CardTitle>
            <CardDescription>The thought process the assistant went through</CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
            <div className="space-y-1">
              <p>{searchProcess.thoughtProcess}</p>
            </div>
          </CardContent>
        </Card>
      </TabsContent>
      <TabsContent value="supporting content">
        <Card>
          <CardHeader>
            <CardTitle>Supporting Content</CardTitle>
            <CardDescription>Citations and references here.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
            {((message.options.metadata.Citations as Citation[]) ?? []).map((citation, index) =>
              citation.name && citation.url ? (
                <CitationSheet
                  index={index + 1}
                  citation={citation}
                  key={message.id + index}
                  assistantId={assistantId}
                />
              ) : (
                <li key={index}>Invalid citation data</li>
              )
            )}
          </CardContent>
        </Card>
      </TabsContent>
    </Tabs>
  );
};
