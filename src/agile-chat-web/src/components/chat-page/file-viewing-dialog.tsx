import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { GenerateSharedLinkByUrl } from '@/services/files-service';
import { Citation } from '@/types/ChatThread';
import { Loader2Icon } from 'lucide-react';
import React, { useState } from 'react';

interface FileViewingDialogProps {
  citation: Citation;
  children?: React.ReactNode;
}
export function FileViewingDialog(props: FileViewingDialogProps) {
  const { citation, children } = props;
  const [file, setFile] = useState<string | undefined>(undefined);

  const loadFile = async (open: boolean) => {
    if (!file && open) {
      const response = await GenerateSharedLinkByUrl(citation.url);
      setFile(response);
    }
  };

  const isHtmlFile = (): boolean => {
    const extension = citation.url.split('.').pop();
    return extension === 'html' || extension === 'htm';
  };

  return (
    <Dialog onOpenChange={loadFile}>
      <DialogTrigger asChild>
        {children ? (
          <div>{children}</div>
        ) : (
          <Button variant="link" className="p-0 mt-2">
            <a>{citation.name}</a>
          </Button>
        )}
      </DialogTrigger>
      <DialogContent className="flex flex-col min-w-[90%] md:min-w-[50%] max-w-fit h-[90%]">
        <DialogHeader>
          <DialogTitle>{citation.name}</DialogTitle>
          <DialogDescription>File preview.</DialogDescription>
        </DialogHeader>
        <div className="flex flex-col grow min-h-0 w-full h-full justify-center items-center">
          {!file && <Loader2Icon className="animate-spin" />}
          {file && !isHtmlFile() && (
            <iframe
              className="w-full h-full"
              src={`https://docs.google.com/gview?url=${encodeURIComponent(file)}&embedded=true`}
            />
          )}
          {file && isHtmlFile() && <iframe className="w-full h-full" src={file} />}
        </div>
        <DialogFooter>
          <Button type="submit">Close</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
