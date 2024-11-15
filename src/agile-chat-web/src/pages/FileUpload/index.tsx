import * as React from 'react';
import Dropzone, { FileRejection } from 'react-dropzone';
import { Cross2Icon, UploadIcon } from '@radix-ui/react-icons';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import { ScrollArea } from '@/components/ui/scroll-area';
import { SparklesIcon, FileSpreadsheetIcon, FileTextIcon, FileIcon, GlobeIcon, MailIcon } from 'lucide-react';
import { useToast } from '@/components/ui/use-toast';
import { useFolders } from '@/hooks/use-folders';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import * as z from 'zod';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { uploadFiles } from '@/services/files-service';
import { useIndexes } from '@/hooks/use-indexes';

const maxFileCount = 5; // Maximum number of files allowed
const maxSize = 26214400; // 25MB

const formSchema = z.object({
  index: z.string().min(1, { message: 'Container is required' }),
  folder: z.string().optional(),
  files: z
    .array(z.instanceof(File))
    .refine((files) => files.length > 0, { message: 'No files selected' })
    .refine((files) => files.length <= maxFileCount, { message: 'Maximum of 5 files can be uploaded at a time' }),
});
type FormValues = z.infer<typeof formSchema>;

export default function FileUploadComponent() {
  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      index: '',
      folder: '',
      files: [],
    },
  });

  const { toast } = useToast(); // Initialize the Shadcn toast
  const [progresses] = useState<Record<string, number>>({});
  const navigate = useNavigate();
  const { folders } = useFolders();
  const { indexes } = useIndexes();

  const onSubmit = async (values: FormValues) => {
    const formData = new FormData();
    formData.append('index', values.index);
    formData.append('folder', values.folder ?? '');
    values.files.forEach((file) => formData.append('files', file));

    await uploadFiles(formData);
    navigate('/files');
  };

  const onDrop = (acceptedFiles: File[], rejectedFiles: FileRejection[]) => {
    form.setValue('files', [...form.getValues().files, ...acceptedFiles]);
    rejectedFiles.forEach((rejected) => {
      toast({
        variant: 'destructive',
        title: 'Error',
        description: rejected.errors[0].message,
      });
    });
  };

  const onRemove = (index: number) => {
    const newArr = [...form.getValues().files];
    newArr.splice(index, 1);
    form.setValue('files', newArr);
  };

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
        <Form {...form}>
          <FormField
            control={form.control}
            name="index"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Container</FormLabel>
                <FormControl>
                  <Select onValueChange={(value) => field.onChange(value)} value={field.value}>
                    <SelectTrigger aria-label="Select Folder">
                      <SelectValue placeholder="Select Folder" />
                    </SelectTrigger>
                    <SelectContent>
                      {indexes &&
                        indexes.map((index) => (
                          <SelectItem key={index.id} value={index.name}>
                            {index.name}
                          </SelectItem>
                        ))}
                    </SelectContent>
                  </Select>
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="folder"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Folder</FormLabel>
                <FormControl>
                  <Select onValueChange={(value) => field.onChange(value)} value={field.value}>
                    <SelectTrigger aria-label="Select Folder">
                      <SelectValue placeholder="Select Folder" />
                    </SelectTrigger>
                    <SelectContent>
                      {folders &&
                        folders.map((folder, index) => (
                          <SelectItem key={folder + index} value={folder}>
                            {folder}
                          </SelectItem>
                        ))}
                    </SelectContent>
                  </Select>
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="files"
            render={({ field }) => {
              return (
                <FormItem>
                  <FormLabel>Files</FormLabel>
                  <FormControl>
                    <div className="w-full flex flex-col justify-center items-center">
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
                                  You can upload up to {maxFileCount} files (up to {Math.round(maxSize / (1024 * 1024))}
                                  MB each)
                                </p>
                              </div>
                            )}
                          </div>
                        )}
                      </Dropzone>
                      {field.value.length > 0 && (
                        <ScrollArea className="mt-4 h-fit w-[50%] px-3">
                          <div className="flex max-h-48 flex-col gap-4">
                            {field.value.map((file, index) => (
                              <FileCard
                                key={index}
                                file={file}
                                onRemove={() => onRemove(index)}
                                progress={progresses[file.name]}
                              />
                            ))}
                          </div>
                        </ScrollArea>
                      )}
                    </div>
                  </FormControl>
                  <FormMessage />
                </FormItem>
              );
            }}
          />
          {/* File Upload Area */}
          <div className="flex mt-2 justify-center">
            <Button type="submit" disabled={form.formState.isSubmitting} onClick={form.handleSubmit(onSubmit)}>
              {form.formState.isSubmitting ? 'Uploading...' : 'Upload'}
            </Button>
          </div>
        </Form>
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
