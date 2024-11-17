// import { MultiSelectInput } from '@/components/ui-extended/multi-select';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
// import { useFolders } from '@/hooks/use-folders';
import { RefreshCw, Trash2 } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useFetchFiles } from '@/hooks/use-files';
import { FileMetadata } from '@/models/filemetadata';
import { deleteFiles } from '@/services/files-service';
import SimpleHeading from '@/components/Heading-Simple';

export default function FileList() {
  // Using the custom hook to fetch files
  const { files, refetch, loading } = useFetchFiles();
  const [selectedFiles, setSelectedFiles] = useState<string[]>([]);
  const [isProcessing, setIsProcessing] = useState<boolean>(false); // Processing state for delete/refresh
  const [sortedFiles, setSortedFiles] = useState<FileMetadata[]>([]);
  // const { folders } = useFolders();
  // const [selectedFolders, setSelectedFolders] = useState<string[]>([]);

  // Sorting logic
  useEffect(() => {
    const sorted = [...files].sort((a, b) => {
      const folderA = a.folder ?? ''; // Use empty string if folder is undefined
      const folderB = b.folder ?? '';
      const fileNameA = a.fileName ?? ''; // Use empty string if fileName is undefined
      const fileNameB = b.fileName ?? '';

      if (folderA === folderB) {
        return fileNameA.localeCompare(fileNameB);
      }
      return folderA.localeCompare(folderB);
    });
    setSortedFiles(sorted);
  }, [files]);

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
      // Sending delete request to the server
      await deleteFiles(selectedFiles);
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

  return (
    <div className="flex h-screen bg-background text-foreground">
      <div className="flex-1  flex flex-col">
       
        <SimpleHeading Title="Your Files" Subtitle="Manage your uploaded files" DocumentCount={0} />
        <div className="flex-1 p-4 overflow-auto">
          
        {/* <div className="flex space-x-4 mb-4">
          <MultiSelectInput
            className="w-[30%] max-w-[500px]"
            label="Folders"
            items={folders}
            selectedItems={selectedFolders}
            onChange={setSelectedFolders}
          />
        </div> */}

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

        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-[50px]" aria-label="Select row">
                <span className="sr-only">Select</span>
              </TableHead>
              <TableHead>FileName</TableHead>
              <TableHead>ContentType</TableHead>
              <TableHead>Size</TableHead>
              <TableHead>Submitted On</TableHead>
              <TableHead>Folder</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {sortedFiles.map((file) => (
              <TableRow key={file.id}>
                <TableCell>
                  <Checkbox
                    checked={selectedFiles.includes(file.id)}
                    onCheckedChange={() => toggleFileSelection(file.id)}
                    aria-label={`Select file ${file.fileName}`}
                  />
                </TableCell>
                <TableCell>{removeFileExtension(file.fileName)}</TableCell>
                <TableCell>{simplifyContentType(file.contentType || 'unknown')}</TableCell>
                <TableCell>{formatBytesToKB(file.size)}</TableCell>
                <TableCell>{file.submittedOn}</TableCell>
                <TableCell>{file.folder}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
        </div>
      </div>
    </div>
  );
}
