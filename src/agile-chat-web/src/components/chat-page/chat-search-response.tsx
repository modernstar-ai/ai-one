import { Citation, Message, SearchProcess } from '@/types/ChatThread';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Tabs } from '@radix-ui/react-tabs';
import { TabsContent, TabsList, TabsTrigger } from '../ui/tabs';
import { Book, Lightbulb, Loader2Icon, MessageCircleMore, View } from 'lucide-react';
import { useState } from 'react';
import { Separator } from '../ui/separator';
import { FileViewingDialog } from './file-viewing-dialog';
import { Badge } from '../ui/badge';

interface ChatSearchResponseProps {
  message: Message;
}

export const ChatSearchResponse = (props: ChatSearchResponseProps) => {
  const { message } = props;
  const searchProcess = message.options.SearchProcess as SearchProcess;
  searchProcess.Citations = (message.options.Citations as Citation[]) ?? [];
  const [chunks, setChunks] = useState<string[] | undefined>(undefined);

  const onOpenSupportingContent = async () => {
    if (chunks === undefined) {
      if (searchProcess.Citations.length === 0) {
        setChunks([]);
      }

      setChunks(searchProcess.Citations.map((citation) => citation.content));
    }
  };

  const citationBadge = (citation: Citation, index: number): JSX.Element => {
    return (
      <Badge className={`cursor-pointer hover:underline mr-2`}>
        {index + 1}. {citation.name}
      </Badge>
    );
  };

  return (
    <Tabs
      defaultValue="answer"
      onValueChange={(val) => {
        if (val === 'search results' && !chunks) onOpenSupportingContent();
      }}>
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
        <TabsTrigger value="search results">
          <span className="flex items-center justify-center gap-2">
            <Book size={18} /> Search Results
          </span>
        </TabsTrigger>
      </TabsList>
      <TabsContent value="answer">
        <Card>
          <CardHeader className="m-0 p-1">
            <CardTitle className="m-2">AI Assistant</CardTitle>
            <CardDescription className="m-2">Response summary</CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
            <div className="flex flex-wrap">
              {(message.options.Citations as Citation[])?.map((citation, index) => {
                const elemenet = citationBadge(citation, index);
                return (
                  <FileViewingDialog key={index} citation={citation}>
                    {elemenet}
                  </FileViewingDialog>
                );
              })}
            </div>
            <div className="space-y-1">
              <p>{message.content}</p>
            </div>
          </CardContent>
        </Card>
      </TabsContent>
      <TabsContent value="thought process">
        <Card>
          <CardHeader className="m-0 p-1">
            <CardTitle className="m-2">Thought Process</CardTitle>
            <CardDescription className="m-2">The thought process the assistant went through</CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
            <div className="space-y-1">
              <p>{searchProcess.thoughtProcess}</p>
            </div>
          </CardContent>
        </Card>
      </TabsContent>
      <TabsContent value="search results">
        <Card>
          <CardHeader className="m-0 p-1">
            <CardTitle className="m-2">Search Results</CardTitle>
            <CardDescription className="m-2">Citations and references here.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-2 max-h-[20rem] overflow-auto">
            {!chunks && <Loader2Icon className="animate-spin" size={24} />}
            {((message.options.Citations as Citation[]) ?? []).map(
              (citation, index) =>
                citation.name &&
                citation.url &&
                chunks && (
                  <div key={'citation' + index}>
                    <div className="flex items-center gap-2">
                      <span className="font-bold mt-2">{index + 1})&nbsp;&nbsp;</span>
                      <FileViewingDialog citation={citation} />
                      <View className="mt-2" />
                    </div>

                    <p>{chunks[index]}</p>
                    <Separator className="my-2" />
                  </div>
                )
            )}
          </CardContent>
        </Card>
      </TabsContent>
    </Tabs>
  );
};
