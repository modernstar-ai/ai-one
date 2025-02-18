import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Sheet,
  SheetClose,
  SheetContent,
  SheetDescription,
  SheetFooter,
  SheetHeader,
  SheetTitle,
  SheetTrigger
} from '@/components/ui/sheet';
import { Citation } from '@/types/ChatThread';
import { Tooltip, TooltipContent, TooltipTrigger } from '../ui/tooltip';
import { Textarea } from '../ui/textarea';
import { FileViewingDialog } from './file-viewing-dialog';
import { useState } from 'react';
import { Loader2Icon } from 'lucide-react';
import { getCitationChunkById } from '@/services/chatthreadservice';

interface CitationSheetProps {
  citation: Citation;
  index: number;
}
export function CitationSheet(props: CitationSheetProps) {
  const { citation, index } = props;
  const [chunk, setChunk] = useState<string | undefined>(undefined);

  const onOpen = async () => {
    if (!chunk) {
      const chunk = await getCitationChunkById(citation.id);
      setChunk(chunk);
    }
  };

  return (
    <Sheet onOpenChange={onOpen}>
      <Tooltip>
        <TooltipTrigger asChild>
          <SheetTrigger asChild>
            <Button variant="outline" className="mr-2">
              {index + 1}
            </Button>
          </SheetTrigger>
        </TooltipTrigger>
        <TooltipContent>
          <p>{citation.name}</p>
        </TooltipContent>
      </Tooltip>
      <SheetContent className="flex flex-col h-full">
        <SheetHeader>
          <SheetTitle>Citation</SheetTitle>
          <SheetDescription>Source of information</SheetDescription>
        </SheetHeader>
        <div className="flex flex-col items-start">
          <Label htmlFor="name">Name</Label>
          <Input readOnly={true} id="name" value={citation.name} className="mt-2" />
        </div>
        <div className="flex flex-col items-start">
          <Label htmlFor="link">Link</Label>
          <FileViewingDialog citation={citation} />
        </div>
        <div className="flex flex-col grow min-h-0">
          <Label htmlFor="content">Content</Label>
          {chunk ? (
            <Textarea className="mt-2 h-full" readOnly={true} value={chunk}></Textarea>
          ) : (
            <Loader2Icon className="flex self-center animate-spin" />
          )}
        </div>

        <SheetFooter>
          <SheetClose asChild>
            <Button type="submit">Close</Button>
          </SheetClose>
        </SheetFooter>
      </SheetContent>
    </Sheet>
  );
}
