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
import { useState, useEffect } from 'react';
import { Check, CopyIcon, Loader2Icon } from 'lucide-react';
import { FilePreviewType, useSettingsStore } from '@/stores/settings-store';
import { getSettings } from '@/services/settings-service';
import { getDownloadFileUrl } from '@/services/files-service';

interface CitationSheetProps {
  citation: Citation;
  index: number;
}
export function CitationSheet(props: CitationSheetProps) {
  const { citation, index } = props;
  const [chunk, setChunk] = useState<string | undefined>(undefined);
  const [copied, setCopied] = useState(false);

  const onOpen = async () => {
    if (!chunk) {
      setChunk(citation.content);
    }
  };
  const { settings } = useSettingsStore();

  useEffect(() => {
    if (!settings) {
      getSettings();
      return;
    }
    
  }, [settings]);

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
          <Label htmlFor="path" className="mb-2">
            Folder Path
          </Label>
          <div className="flex w-full items-center gap-2">
            <Input
              readOnly={true}
              id="name"
              value={decodeURI(new URL(citation.url).pathname)
                .replace(/\/index-content\/[^/]+\//, '')
                .replace(/\/thread-content\/[^/]+\//, '')}
              className=""
            />
            <Button
              size={'icon'}
              variant={'outline'}
              title="Copy"
              onClick={() => {
                navigator.clipboard.writeText(
                  decodeURI(new URL(citation.url).pathname)
                    .replace(/\/index-content\/[^/]+\//, '')
                    .replace(/\/thread-content\/[^/]+\//, '')
                );
                setCopied(true);
                setTimeout(() => setCopied(false), 1000);
              }}>
              {copied ? <Check className={`text-green-500 animate-pulse`} /> : <CopyIcon />}
            </Button>
          </div>
        </div>
        {settings?.filePreviewType === FilePreviewType.Preview && (
          <div className="flex flex-col items-start">
            <Label htmlFor="link">Link</Label>
            <FileViewingDialog citation={citation} />
          </div>
        )}

        {settings?.filePreviewType === FilePreviewType.Download && (
          <div className="flex flex-col items-start mt-4">
            <Label htmlFor="download">Download File</Label>
            <Button
              variant="link"
              className="p-0 mt-2"
              asChild
            >
              <a
                href={getDownloadFileUrl(citation.url)}
                target="_blank"
                rel="noopener noreferrer"
                download // This hints to browsers to download, but actual download depends on backend Content-Disposition headers
              >
                {citation.name}
              </a>
            </Button>
          </div>
        )}

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
