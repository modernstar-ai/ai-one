import { Citation, Message, SearchProcess } from '@/types/ChatThread';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Tabs } from '@radix-ui/react-tabs';
import { TabsContent, TabsList, TabsTrigger } from '../ui/tabs';
import { CitationSheet } from './citation-sheet';
import { Book, Lightbulb, MessageCircleMore, View } from 'lucide-react';
import { useState } from 'react';
import { getCitationChunkById } from '@/services/ai-search-service';
import { Button } from '../ui/button';
import { Textarea } from '../ui/textarea';
import { Separator } from '../ui/separator';
import { FileViewingDialog } from './file-viewing-dialog';

interface ChatSearchResponseProps {
  message: Message;
  assistantId: string;
}

export const ChatSearchResponse = (props: ChatSearchResponseProps) => {
  const { message, assistantId } = props;
  const searchProcess = message.options.metadata.SearchProcess as SearchProcess;
  searchProcess.Citations = (message.options.metadata.Citations as Citation[]) ?? [];
  const [chunks, setChunks] = useState<string[] | undefined>(undefined);

  const onOpenSupportingContent = async () => {
    if (chunks === undefined) {
      if (searchProcess.Citations.length === 0) {
        setChunks([]);
      }

      const chunkReqs: Promise<string>[] = [];
      searchProcess.Citations.forEach((citation) => {
        chunkReqs.push(getCitationChunkById(assistantId, citation.id));
      });

      const chunks = await Promise.all(chunkReqs);
      setChunks(chunks);
    }
  };

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
        <TabsTrigger value="supporting content" onClick={onOpenSupportingContent}>
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
          <CardContent className="space-y-2 max-h-[20rem] overflow-auto">
            {((message.options.metadata.Citations as Citation[]) ?? []).map((citation, index) =>
              citation.name && citation.url && chunks ? (
                <div>
                  <div className="flex items-center gap-2">
                    <span className="font-bold mt-2">{index + 1})&nbsp;&nbsp;</span>
                    <FileViewingDialog citation={citation} />
                    <View className="mt-2" />
                  </div>

                  <p>{chunks[index]}</p>
                  <Separator className="my-2" />
                </div>
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
