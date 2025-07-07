import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import { DataTable } from '@/components/ui/data-table';
import { Card, CardContent } from '@/components/ui/card';
import { Loader2Icon, RefreshCw, Trash2, Search, MoreVertical, File, Folder } from 'lucide-react';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useFetchFiles } from '@/hooks/use-files';
import { deleteFiles } from '@/services/files-service';
import SimpleHeading from '@/components/Heading-Simple';
import { Badge } from '@/components/ui/badge';
import { FileEditDialog } from './Edit';
import { CosmosFile } from '@/models/filemetadata';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger
} from '@/components/ui/dropdown-menu';
import { Input } from '@/components/ui/input';

export default function FileList() {
  const { files, refetch, loading, queryDto, setQueryDto } = useFetchFiles();
  const [selectedFiles, setSelectedFiles] = useState<string[]>([]);
  const [isProcessing, setIsProcessing] = useState<boolean>(false);
  const [currentPage, setCurrentPage] = useState<number>(0);
  const [searchValue, setSearchValue] = useState<string>('');

  const totalPages = files?.totalCount ? Math.ceil(files.totalCount / files.pageSize) : 1;

  // Handle page change
  const handlePageChange = (page: number) => {
    console.log('page', page);
    setCurrentPage(page);
    setQueryDto({ ...queryDto, page, pageSize: files.pageSize });
  };

  // Function to toggle the selection of files
  const toggleFileSelection = (fileId: string) => {
    setSelectedFiles((prev) => (prev.includes(fileId) ? prev.filter((id) => id !== fileId) : [...prev, fileId]));
  };

  // Size conversion in KB
  function formatBytesToKB(bytes: number): string {
    if (bytes === 0) return '0 KB';
    const kilobytes = bytes / 1024;
    return `${kilobytes.toFixed(2)} KB`;
  }

  // Simplified Content Type
  function simplifyContentType(contentType: string): string {
    const mimeTypeMappings: Record<string, string> = {
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document': 'Word Document',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': 'Excel',
      'application/pdf': 'PDF',
      'text/plain': 'Text',
      'application/msword': 'Word',
      'application/vnd.ms-excel': 'Excel',
      'application/vnd.openxmlformats-officedocument.presentationml.presentation': 'PowerPoint'
    };
    return mimeTypeMappings[contentType] || contentType;
  }

  // Handle Delete Files
  const handleDeleteSelected = async () => {
    if (selectedFiles.length === 0) {
      alert('No files selected for deletion.');
      return;
    }
    setIsProcessing(true);
    try {
      const deletions: Promise<void>[] = [];
      selectedFiles.forEach((fileId) => deletions.push(deleteFiles(fileId)));
      await Promise.all(deletions);
      alert('Selected files deleted successfully.');
      setSelectedFiles([]);
      await refetch();
    } catch (error) {
      console.error('Error deleting files:', error);
      alert('An error occurred while deleting files.');
    } finally {
      handleRefresh();
      setIsProcessing(false);
    }
  };

  // Handle Delete Single File (Mobile)
  const handleDeleteSingleFile = async (fileId: string) => {
    setIsProcessing(true);
    try {
      await deleteFiles(fileId);
      alert('File deleted successfully.');
      await refetch();
    } catch (error) {
      console.error('Error deleting file:', error);
      alert('An error occurred while deleting the file.');
    } finally {
      setIsProcessing(false);
    }
  };

  // Handle Refresh Files
  const handleRefresh = async () => {
    setIsProcessing(true);
    try {
      await refetch();
    } catch (error) {
      console.error('Error refreshing files:', error);
      alert('An error occurred while refreshing files.');
    } finally {
      setIsProcessing(false);
    }
  };

  const getPaginationButtons = () => {
    const paginationButtons = [];
    paginationButtons.push(0);
    if (currentPage > 3) paginationButtons.push('...');
    const start = Math.max(1, currentPage - 2);
    const end = Math.min(totalPages - 1, currentPage + 2);
    for (let i = start; i < end; i++) {
      paginationButtons.push(i);
    }
    if (currentPage < totalPages - 3) paginationButtons.push('...');
    if (totalPages > 1) paginationButtons.push(totalPages - 1);
    return paginationButtons;
  };

  // Desktop table columns
  const columns = [
    {
      accessorKey: 'checkbox',
      header: () => (
        <Checkbox
          checked={files?.items && selectedFiles.length === files.items.length && files.items.length > 0}
          onCheckedChange={(checked) => {
            if (checked) {
              setSelectedFiles(files?.items.map((file) => file.id) || []);
            } else {
              setSelectedFiles([]);
            }
          }}
          aria-label="Select all files"
        />
      ),
      cell: ({ row }: { row: any }) => (
        <Checkbox
          checked={selectedFiles.includes(row.original.id)}
          onCheckedChange={() => toggleFileSelection(row.original.id)}
          aria-label={`Select file ${row.original.name}`}
        />
      )
    },
    {
      accessorKey: 'name',
      header: 'File Name',
      cell: ({ row }: { row: any }) => row.original.name
    },
    {
      accessorKey: 'contentType',
      header: 'Content Type',
      cell: ({ row }: { row: any }) => simplifyContentType(row.original.contentType || 'unknown')
    },
    {
      accessorKey: 'size',
      header: 'Size',
      cell: ({ row }: { row: any }) => formatBytesToKB(row.original.size)
    },
    {
      accessorKey: 'indexName',
      header: 'Container',
      cell: ({ row }: { row: any }) => row.original.indexName
    },
    {
      accessorKey: 'folderName',
      header: 'Folder',
      cell: ({ row }: { row: any }) => row.original.folderName
    },
    {
      accessorKey: 'tags',
      header: 'Tags',
      cell: ({ row }: { row: any }) => (
        <div className="flex flex-col items-start gap-2 max-h-24 overflow-y-scroll max-w-40 overflow-x-auto">
          {(row.original.tags as string[])?.map((tag, index) => (
            <Badge className="text-nowrap" key={tag + index}>
              {tag}
            </Badge>
          ))}
        </div>
      )
    },
    {
      accessorKey: 'status',
      header: 'Status',
      cell: ({ row }: { row: any }) => (
        <div className="flex">
          <Badge>{row.original.status}</Badge>
        </div>
      )
    },
    {
      accessorKey: '',
      header: 'Update',
      cell: ({ row }: { row: any }) => (
        <div className="flex items-center justify-center">
          <FileEditDialog file={row.original as CosmosFile} handleRefresh={handleRefresh} />
        </div>
      )
    }
  ];

  // Handle search submit
  const handleGlobalSearchSubmit = (e: React.KeyboardEvent<HTMLInputElement> | React.FocusEvent<HTMLInputElement>) => {
    if ('key' in e && e.key !== 'Enter') return;
    const target = e.target as HTMLInputElement;
    const searchValue = target.value.trim();
    setQueryDto({ ...queryDto, page: 0, search: searchValue });
    setCurrentPage(0);
  };

  // Mobile file card component
  const FileCard = ({ file }: { file: CosmosFile }) => (
    <Card className="mb-4">
      <CardContent className="p-4">
        <div className="flex items-start justify-between mb-3">
          <div className="flex items-start space-x-3 flex-1 min-w-0">
            <Checkbox
              checked={selectedFiles.includes(file.id)}
              onCheckedChange={() => toggleFileSelection(file.id)}
              aria-label={`Select file ${file.name}`}
            />
            <div className="flex-1 min-w-0">
              <div className="flex items-center space-x-2 mb-1">
                <File className="h-4 w-4 text-muted-foreground flex-shrink-0" />
                <h3 className="font-medium text-sm truncate">{file.name}</h3>
              </div>
              <div className="flex items-center space-x-2 text-xs text-muted-foreground">
                <span>{simplifyContentType(file.contentType || 'unknown')}</span>
                <span>•</span>
                <span>{formatBytesToKB(file.size)}</span>
              </div>
            </div>
          </div>

          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                variant="ghost"
                size="sm"
                className="h-8 w-8 p-0"
                onClick={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                }}
                onTouchStart={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                }}>
                <MoreVertical className="h-4 w-4" />
                <span className="sr-only">Open menu for {file.name}</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" side="bottom" sideOffset={5} onCloseAutoFocus={(e) => e.preventDefault()}>
              <DropdownMenuItem asChild onSelect={(e) => e.preventDefault()}>
                <div className="w-full">
                  <FileEditDialog
                    file={file}
                    handleRefresh={handleRefresh}
                    customTrigger={<div className="w-full h-full">Edit</div>}
                  />
                </div>
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                onClick={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                  handleDeleteSingleFile(file.id);
                }}
                onSelect={() => handleDeleteSingleFile(file.id)}
                className="text-destructive focus:text-destructive"
                disabled={isProcessing}>
                <Trash2 className="h-4 w-4 mr-2" />
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>

        <div className="space-y-2 text-xs">
          <div className="flex items-center space-x-2">
            <Folder className="h-3 w-3 text-muted-foreground" />
            <span className="text-muted-foreground">Container:</span>
            <span>{file.indexName}</span>
          </div>

          {file.folderName && (
            <div className="flex items-center space-x-2">
              <Folder className="h-3 w-3 text-muted-foreground" />
              <span className="text-muted-foreground">Folder:</span>
              <span>{file.folderName}</span>
            </div>
          )}

          <div className="flex items-center space-x-2">
            <span className="text-muted-foreground">Status:</span>
            <Badge variant="secondary" className="text-xs">
              {file.status}
            </Badge>
          </div>

          {file.tags && file.tags.length > 0 && (
            <div className="pt-2">
              <div className="text-muted-foreground mb-1">Tags:</div>
              <div className="flex flex-wrap gap-1">
                {file.tags.map((tag, index) => (
                  <Badge key={tag + index} variant="outline" className="text-xs">
                    {tag}
                  </Badge>
                ))}
              </div>
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );

  // Bulk actions bar for mobile
  const MobileBulkActions = () => {
    if (selectedFiles.length === 0) return null;

    return (
      <div className="fixed bottom-0 left-0 right-0 bg-background border-t p-4 lg:hidden">
        <div className="flex items-center justify-between">
          <span className="text-sm text-muted-foreground">
            {selectedFiles.length} file{selectedFiles.length !== 1 ? 's' : ''} selected
          </span>
          <div className="flex space-x-2">
            <Button variant="outline" size="sm" onClick={() => setSelectedFiles([])}>
              Clear
            </Button>
            <Button variant="destructive" size="sm" onClick={handleDeleteSelected} disabled={isProcessing}>
              {isProcessing ? (
                <Loader2Icon className="h-4 w-4 animate-spin mr-2" />
              ) : (
                <Trash2 className="h-4 w-4 mr-2" />
              )}
              Delete
            </Button>
          </div>
        </div>
      </div>
    );
  };

  return (
    <div className="flex min-h-screen bg-background text-foreground overflow-hidden">
      <div className="flex-1 flex flex-col lg:max-h-screen lg:overflow-y-auto min-w-0">
        <SimpleHeading Title="Your Files" Subtitle="Manage your uploaded files" DocumentCount={0} />

        <div className="flex-1 p-4 pb-20 lg:pb-4 min-w-0">
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-4 space-y-2 sm:space-y-0 min-w-0">
            <Link to="/fileupload" aria-label="Add New File" accessKey="n">
              <Button tabIndex={-1} aria-label="Add New File Button" className="w-full sm:w-auto">
                Add New
              </Button>
            </Link>

            <div className="hidden lg:flex space-x-2">
              <Button
                variant="outline"
                size="icon"
                aria-label="Refresh"
                onClick={handleRefresh}
                disabled={loading || isProcessing}>
                <RefreshCw className="h-4 w-4" />
              </Button>
              <Button
                variant="outline"
                size="icon"
                aria-label="Trash"
                onClick={handleDeleteSelected}
                disabled={selectedFiles.length === 0}>
                <Trash2 className="h-4 w-4" />
              </Button>
            </div>

            <div className="flex lg:hidden space-x-2 w-full sm:w-auto">
              <Button
                variant="outline"
                size="sm"
                onClick={handleRefresh}
                disabled={loading || isProcessing}
                className="flex-1 sm:flex-none">
                <RefreshCw className="h-4 w-4 mr-2" />
                Refresh
              </Button>
              {selectedFiles.length > 0 && (
                <Button variant="destructive" size="sm" onClick={handleDeleteSelected} className="flex-1 sm:flex-none">
                  <Trash2 className="h-4 w-4 mr-2" />
                  Delete ({selectedFiles.length})
                </Button>
              )}
            </div>
          </div>

          <div className="mb-4 min-w-0">
            <div className="relative max-w-full">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                type="text"
                placeholder="Search files..."
                className="pl-10 w-full max-w-sm"
                value={searchValue}
                onChange={(e) => setSearchValue(e.target.value)}
                onKeyDown={handleGlobalSearchSubmit}
                onBlur={handleGlobalSearchSubmit}
              />
            </div>
          </div>

          {loading ? (
            <div className="flex justify-center items-center h-32">
              <Loader2Icon className="animate-spin h-8 w-8" />
            </div>
          ) : (
            <>
              <div className="hidden lg:block overflow-x-auto">
                <div className="min-w-full">
                  <DataTable columns={columns} data={files.items} />
                </div>
              </div>

              <div className="lg:hidden">
                {files.items && files.items.length > 0 ? (
                  files.items.map((file) => <FileCard key={file.id} file={file} />)
                ) : (
                  <div className="text-center py-8">
                    <File className="h-12 w-12 text-gray-400 mx-auto mb-4" />
                    <p className="text-lg font-medium text-gray-900">No files found</p>
                    <p className="mt-1 text-sm text-gray-500">Upload your first file to get started.</p>
                  </div>
                )}
              </div>
            </>
          )}

          {files.items && files.items.length > 0 && (
            <div className="flex justify-center mt-6">
              <div className="flex space-x-1 overflow-x-auto pb-2">
                {getPaginationButtons().map((page, index) =>
                  typeof page === 'number' ? (
                    <Button
                      key={index}
                      variant={currentPage === page ? 'default' : 'outline'}
                      size="sm"
                      onClick={() => handlePageChange(page)}
                      disabled={currentPage === page}
                      className="min-w-[2.5rem]">
                      {page + 1}
                    </Button>
                  ) : (
                    <span key={index} className="px-2 py-2 text-sm text-muted-foreground">
                      ...
                    </span>
                  )
                )}
              </div>
            </div>
          )}
        </div>

        <MobileBulkActions />
      </div>
    </div>
  );
}
