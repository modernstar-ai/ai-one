import * as React from 'react';
import Dropzone, { FileRejection } from 'react-dropzone';
import { Cross2Icon, UploadIcon } from '@radix-ui/react-icons';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import { ScrollArea } from '@/components/ui/scroll-area';
import { SparklesIcon, FileSpreadsheetIcon, FileTextIcon, FileIcon, GlobeIcon, MailIcon } from 'lucide-react';
import { useToast } from '@/components/ui/use-toast';
import axios from '@/error-handling/axiosSetup';

function getApiUrl(endpoint: string): string {
  const rootApiUrl = import.meta.env.VITE_AGILECHAT_API_URL as string;
  return `${rootApiUrl}/${endpoint}`;
}

export default function Component() {
  const { toast } = useToast(); // Initialize the Shadcn toast

  const uploadFiles = async () => {
    if (files.length === 0) {
      toast({
        title: 'Error',
        description: 'No files selected for upload.',
        variant: 'destructive',
      });
      return;
    }

    const formData = new FormData();
    files.forEach((file) => {
      formData.append('files', file);
    });

    try {
      const apiUrl = getApiUrl('upload');
      console.log(apiUrl);
      const response = await axios.post(apiUrl, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      console.log(response);
      if (response.status != 200) {
        toast({
          title: 'Error',
          description: 'File upload failed.',
          variant: 'destructive',
        });
      }
      toast({
        title: 'Success',
        description: 'Files uploaded successfully!',
        variant: 'default',
      });

      setFiles([]); // Clear files after upload
    } catch (error) {
      // Check if the error is an instance of Error
      console.log(error);
      if (error instanceof Error) {
        toast({
          title: 'Error',
          description: error.message,
          variant: 'destructive',
        });
      } else {
        toast({
          title: 'Error',
          description: 'An unknown error occurred during upload.',
          variant: 'destructive',
        });
      }
    }
  };

  const [files, setFiles] = React.useState<File[]>([]);
  const [progresses] = React.useState<Record<string, number>>({});

  const maxFileCount = 5; // Maximum number of files allowed
  const maxSize = 1024 * 1024 * 2; // 2MB

  const onDrop = (acceptedFiles: File[], rejectedFiles: FileRejection[]) => {
    if (files.length + acceptedFiles.length > maxFileCount) {
      toast({
        title: 'Error',
        description: `Cannot upload more than ${maxFileCount} files`,
        variant: 'destructive',
      });
      return;
    }

    const newFiles = acceptedFiles.map((file) => file); // Just keep the accepted files

    setFiles([...files, ...newFiles]);

    if (rejectedFiles.length > 0) {
      rejectedFiles.forEach(({ file }) => {
        toast({
          title: 'Error',
          description: `File ${file.name} was rejected`, // Use backticks for template literal
          variant: 'destructive',
        });
      });
    }
  };

  const onRemove = (index: number) => {
    const newFiles = files.filter((_, i) => i !== index);
    setFiles(newFiles);
  };

  React.useEffect(() => {
    return () => {
      files.forEach((file) => {
        if ('preview' in file && typeof file.preview === 'string') {
          URL.revokeObjectURL(file.preview);
        }
      });
    };
  }, [files]);

  return (
    <div className="flex h-screen bg-white-100">

      {/* Main Content */}
      <main className="flex-1 p-8" role="main">
        <header className="mb-8 text-center" role="banner">
          <SparklesIcon className="mx-auto mb-4 h-12 w-12 text-primary" />
          <h1 className="text-3xl font-bold">Supported File Types</h1>
          <p className="mt-2 text-gray-600">UTS AI Assistant currently supports the following file types</p>
        </header>

        {/* File Types */}
        <div className="mb-8 grid grid-cols-5 gap-4 text-center">
          {[
            { icon: FileTextIcon, title: 'Data', description: 'xml, json, csv, txt' },
            { icon: FileSpreadsheetIcon, title: 'Productivity Software', description: 'ppt, docx, xlsx' },
            { icon: FileIcon, title: 'PDF', description: 'pdf' },
            { icon: GlobeIcon, title: 'Web', description: 'html, html' },
            { icon: MailIcon, title: 'Email', description: 'email & msg' },
          ].map((item, index) => (
            <div key={index} className="flex flex-col items-center">
              <item.icon className="mb-2 h-12 w-12 text-primary" />
              <h2 className="font-semibold">{item.title}</h2>
              <p className="text-sm text-gray-600">{item.description}</p>
            </div>
          ))}
        </div>

        {/* Dropdowns */}
        <div className="flex justify-center w-full">
          <div className="mb-8 grid grid-cols-2 gap-4 w-1/2">
            <Select>
              <SelectTrigger aria-label="Select Group">
                <SelectValue placeholder="Select Group" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="group1">Group 1</SelectItem>
                <SelectItem value="group2">Group 2</SelectItem>
                <SelectItem value="group3">Group 3</SelectItem>
              </SelectContent>
            </Select>
            <Select>
              <SelectTrigger aria-label="Select Folder">
                <SelectValue placeholder="Select Folder" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="folder1">Folder 1</SelectItem>
                <SelectItem value="folder2">Folder 2</SelectItem>
                <SelectItem value="folder3">Folder 3</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>

        {/* File Upload Area */}
        <div className="flex justify-center w-full">
          <Dropzone
            onDrop={onDrop}
            accept={{}} // Allow all file types
            maxSize={maxSize}
            maxFiles={maxFileCount}
          >
            {({
              getRootProps,
              getInputProps,
              isDragActive,
            }: {
              getRootProps: () => React.HTMLProps<HTMLDivElement>;
              getInputProps: () => React.InputHTMLAttributes<HTMLInputElement>;
              isDragActive: boolean;
            }) => (
              <div
                {...getRootProps()}
                className={`group relative grid text-center h-52 w-1/2 cursor-pointer place-items-center rounded-lg border-2 border-dashed border-muted-foreground/25 px-5 py-2.5 text-center transition hover:bg-muted/25 ${
                  isDragActive ? 'border-muted-foreground/50' : ''
                }`}
              >
                <input {...getInputProps()} aria-labelledby="dropzone-label" />
                <span id="dropzone-label" className="sr-only">
                  Upload files
                </span>
                {isDragActive ? (
                  <div className="flex flex-col items-center justify-center gap-4 sm:px-5">
                    <div className="rounded-full border border-dashed p-3">
                      <UploadIcon className="size-7 text-muted-foreground" aria-hidden="true" />
                    </div>
                    <p className="font-medium text-muted-foreground">Drop the files here</p>
                  </div>
                ) : (
                  <div className="flex flex-col items-center justify-center gap-4 sm:px-5">
                    <div className="rounded-full border border-dashed p-3">
                      <UploadIcon className="size-7 text-muted-foreground" aria-hidden="true" />
                    </div>
                    <p className="font-medium text-muted-foreground">
                      Drag 'n' drop files here, or click to select files
                    </p>
                    <p className="text-sm text-muted-foreground">
                      You can upload up to {maxFileCount} files (up to {Math.round(maxSize / (1024 * 1024))}MB each)
                    </p>
                  </div>
                )}
              </div>
            )}
          </Dropzone>
        </div>
        <div className="flex justify-center mt-4">
          <Button size="lg" className="grid text-center" onClick={uploadFiles} aria-label="Add Files Button">
            Upload Files
          </Button>
        </div>

        {files.length > 0 && (
          <ScrollArea className="mt-4 h-fit w-full px-3">
            <div className="flex max-h-48 flex-col gap-4">
              {files.map((file, index) => (
                <FileCard key={index} file={file} onRemove={() => onRemove(index)} progress={progresses[file.name]} />
              ))}
            </div>
          </ScrollArea>
        )}
      </main>
    </div>
  );
}

function FileCard({ file, progress, onRemove }: { file: File; progress?: number; onRemove: () => void }) {
  return (
    <div className="relative flex items-center gap-2.5">
      <div className="flex flex-1 gap-2.5">
        <div className="flex w-full flex-col gap-2">
          <div className="flex flex-col gap-px">
            <p className="line-clamp-1 text-sm font-medium text-foreground/80">{file.name}</p>
            <p className="text-xs text-muted-foreground">
              {/* {formatBytes(file.size)} Optional: You can add formatBytes function if needed */}
            </p>
          </div>
          {progress ? <Progress value={progress} /> : null}
        </div>
      </div>
      <div className="flex items-center gap-2">
        <Button
          aria-label="Remove File Button"
          type="button"
          variant="outline"
          size="icon"
          className="size-7"
          onClick={onRemove}
        >
          <Cross2Icon className="size-4" aria-hidden="true" />
          <span className="sr-only">Remove file</span>
        </Button>
      </div>
    </div>
  );
}
