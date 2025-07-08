import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger
} from '@/components/ui/dialog';
import { getDownloadFileUrl } from '@/services/files-service';
import { Citation } from '@/types/ChatThread';
import Markdoc from '@markdoc/markdoc';
import axios from 'axios';
import { Loader2Icon } from 'lucide-react';
import React, { useEffect, useState } from 'react';

interface FileViewingDialogProps {
  citation: Citation;
  children?: React.ReactNode;
}
export function FileViewingDialog(props: FileViewingDialogProps) {
  const { citation, children } = props;
  const extension = citation.url.split('.').pop()!;

  const [file] = useState<string | undefined>(getDownloadFileUrl(citation.url));
  const [fileContent, setFileContent] = useState<string | undefined>(undefined);

  useEffect(() => {
    if (fileContent || !file) return;

    if (['json', 'txt', 'xml'].includes(extension)) {
      getText().then((text) => setFileContent(text));
    } else if (extension === 'md') {
      getMarkdownFromText().then((md) => setFileContent(md));
    }
  }, [extension, file]);

  const getText = async () => {
    const text = await axios.get<string>(file!, { responseType: 'text' });
    return text.data.toString();
  };

  const getMarkdownFromText = async () => {
    const text = await getText();
    const ast = Markdoc.parse(text);
    const content = Markdoc.transform(ast);

    return Markdoc.renderers.html(content);
  };

  return (
    <Dialog>
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
          {!file ? (
            <Loader2Icon className="animate-spin" />
          ) : ['docx', 'xlsx', 'pptx'].includes(extension) ? (
            // Treat other Office formats like "xlsx" for the Office Online Viewer
            <iframe
              title="Source File"
              src={
                'https://view.officeapps.live.com/op/view.aspx?src=' + encodeURIComponent(file) + '&action=embedview'
              }
              width="100%"
              height="100%"
            />
          ) : extension === 'pdf' ? (
            // Use object tag for PDFs because iframe does not support page numbers
            <iframe title="Source File" src={file} width="100%" height="100%" />
          ) : extension === 'md' ? (
            // Render Markdown content using react-markdown
            <div className="flex w-full h-full overflow-auto prose">
              <div className="flex w-full h-full" dangerouslySetInnerHTML={{ __html: fileContent! }} />
            </div>
          ) : ['json', 'txt', 'xml'].includes(extension) ? (
            // Render plain text content
            <div className="w-full h-full overflow-auto">
              <pre>{fileContent}</pre>
            </div>
          ) : ['bmp', 'jpg', 'jpeg', 'png', 'tiff'].includes(extension) ? (
            <img src={file} className="w-fit h-fit overflow-auto" />
          ) : (
            // Default to iframe for other file types.
            <iframe title="Source File" src={file} width="100%" height="100%" />
          )}
        </div>
        <DialogFooter>
          <Button type="submit">Close</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
