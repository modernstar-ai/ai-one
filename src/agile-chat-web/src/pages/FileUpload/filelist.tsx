import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import { DataTable } from '@/components/ui/data-table';
import { RefreshCw, Trash2 } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useFetchFiles } from '@/hooks/use-files';
import { CosmosFile } from '@/models/filemetadata';
import { deleteFiles } from '@/services/files-service';
import SimpleHeading from '@/components/Heading-Simple';
import { PagedResultsDto } from '@/models/pagedResultsDto';

export default function FileList() {
  // Using the custom hook to fetch files
  
  const { files, refetch, loading, queryDto, setQueryDto } = useFetchFiles();
  const [selectedFiles, setSelectedFiles] = useState<string[]>([]);
  const [isProcessing, setIsProcessing] = useState<boolean>(false); // Processing state for delete/refresh
  const [sortedFiles, setSortedFiles] = useState<PagedResultsDto<CosmosFile>>();
  const [currentPage, setCurrentPage] = useState<number>(0); // Track the current page

  const totalPages = files?.totalCount ? Math.ceil(files.totalCount / files.pageSize) : 1;

  // Handle page change
  const handlePageChange = (page: number) => {
    console.log("page", page);
    setCurrentPage(page);
    setQueryDto({ ...queryDto, page, pageSize: files.pageSize });
  };

  // Function to toggle the selection of files
  const toggleFileSelection = (fileId: string) => {
    setSelectedFiles((prev) => (prev.includes(fileId) ? prev.filter((id) => id !== fileId) : [...prev, fileId]));
  };

  // To remove extensions from File names
  function removeFileExtension(fileName: string): string {
    return fileName.replace(/\.[^/.]+$/, '');
  }

  // Size conversion in KB
  function formatBytesToKB(bytes: number): string {
    if (bytes === 0) return '0 KB';
    const kilobytes = bytes / 1024;
    return `${kilobytes.toFixed(2)} KB`; // Round to 2 decimal places for better readability
  }

  // Simplified Content Type
  function simplifyContentType(contentType: string): string {
    const mimeTypeMappings: Record<string, string> = {
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document': 'application/document',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': 'application/xlsx',
      'application/pdf': 'application/pdf',
      'text/plain': 'text/txt',
      'application/msword': 'application/msword',
      'application/vnd.ms-excel': 'application/xls',
      'application/vnd.openxmlformats-officedocument.presentationml.presentation': 'application/ppt',
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
      // Sending delete request to the server
      await Promise.all(deletions);
      // Update the UI after successful deletion
      alert('Selected files deleted successfully.');
      // Clear selected files
      setSelectedFiles([]);
      // Re-fetch files to update UI
      await refetch();
    } catch (error) {
      console.error('Error deleting files:', error);
      alert('An error occurred while deleting files.');
    } finally {
      handleRefresh();
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

  // data-table
  const columns = [
    {
      accessorKey: 'checkbox',
      header: '',
      cell: ({ row }: { row: any }) => (
        <Checkbox
          checked={selectedFiles.includes(row.original.id)}
          onCheckedChange={() => toggleFileSelection(row.original.id)}
          aria-label={`Select file ${row.original.name}`}
        />
      ),
    },
    { 
      accessorKey: 'name', 
      header: 'File Name',
      cell: ({ row }: { row: any }) => removeFileExtension(row.original.name),
    },
    {
      accessorKey: 'contentType',
      header: 'Content Type',
      cell: ({ row }: { row: any }) => simplifyContentType(row.original.contentType || 'unknown'),
    },
    {
      accessorKey: 'size',
      header: 'Size',
      cell: ({ row }: { row: any }) => formatBytesToKB(row.original.size),
    },
    {
      accessorKey: 'createdDate',
      header: 'Submitted On',
    },
    { 
      accessorKey: 'indexName', 
      header: 'Container',
      cell: ({ row }: { row: any }) => row.original.indexName,
    },
  ];


// Add this handler for "Enter" key press or focus loss
const handleGlobalSearchSubmit = (e: React.KeyboardEvent<HTMLInputElement> | React.FocusEvent<HTMLInputElement>) => {
  if ('key' in e && e.key !== 'Enter') return; // Proceed only if "Enter" is pressed

  const target = e.target as HTMLInputElement; 
  const searchValue = target.value.trim(); // Get the trimmed value from the input
  
  setQueryDto({...queryDto, page: 0, search:searchValue});
  setCurrentPage(0);
};

  // Fetch sorted data on every `files` change
  useEffect(() => {
    if (files?.items) {
      setSortedFiles(files); // Update sorted files when new files are fetched
    }
  }, [files]);

  return (
    <div className="flex h-screen bg-background text-foreground">
      <div className="flex-1  flex flex-col">
        <SimpleHeading Title="Your Files" Subtitle="Manage your uploaded files" DocumentCount={0} />
        <div className="flex-1 p-4 overflow-auto">

          <div className="flex justify-between items-center mb-4">
            <Link to="/fileupload" aria-label="Add New File" accessKey="n">
              <Button tabIndex={-1} aria-label="Add New File Button">
                Add New
              </Button>
            </Link>

            <div className="space-x-2">
              <Button
                variant="outline"
                size="icon"
                aria-label="Refresh"
                onClick={handleRefresh}
                disabled={loading || isProcessing}
              >
                <RefreshCw className="h-4 w-4" />
              </Button>
              <Button
                variant="outline"
                size="icon"
                aria-label="Trash"
                onClick={handleDeleteSelected}
                disabled={selectedFiles.length === 0}
              >
                <Trash2 className="h-4 w-4" />
              </Button>
            </div>
          </div>
          <div className="flex justify-between items-center mb-4">
            <input
              type="text"
              placeholder="Search files..."
              className="border rounded p-2 text-sm w-[300px]"
              onKeyDown={handleGlobalSearchSubmit}
              onBlur={handleGlobalSearchSubmit} 
            />
          </div>
          <DataTable columns={columns}  data={sortedFiles?.items || []} />

           {/* Pagination */}
           <div className="flex mt-4 space-x-2">
            {Array.from({ length: totalPages }, (_, index) => (
              <Button
                key={index}
                variant={currentPage === index ? "link" : "outline"}
                onClick={() => handlePageChange(index)}
                disabled={currentPage === index}
              >
                {index + 1}
              </Button>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
