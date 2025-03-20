import { useState } from 'react';
import { toast } from './ui/use-toast';
import { useNavigate } from 'react-router-dom';
import { deleteIndex } from '@/services/indexes-service';
import { Index } from '@/models/indexmetadata';
import { Loader2, Info, Trash2, Pencil, BarChart } from 'lucide-react';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from './ui/button';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger
} from '@/components/ui/alert-dialog';
import { useIndexes } from '@/hooks/use-indexes';
import { PermissionHandler } from '@/authentication/permission-handler/permission-handler';
import { UserRole } from '@/authentication/user-roles';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from './ui/table';
import { EditIndexDialog } from '@/pages/Indexes/Edit/EditIndex';

const IndexerComponent: React.FC = () => {
  const { indexes, indexesLoading, refreshIndexes } = useIndexes();
  const [indexToDelete, setIndexToDelete] = useState<Index | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [indexEditing, setIndexEditing] = useState<Index | undefined>(undefined);
  const navigate = useNavigate();

  if (indexesLoading) {
    return (
      <Card>
        <CardContent className="flex items-center justify-center h-64">
          <Loader2 className="h-8 w-8 animate-spin" />
          <span className="sr-only">Loading indexes...</span>
        </CardContent>
      </Card>
    );
  }

  const handleDeleteIndex = async (index: Index) => {
    setIndexToDelete(index);
  };

  const handleEditIndex = async (index: Index) => {
    setIndexEditing(index);
  };

  const handleViewStats = (indexName: string) => {
    navigate(`/index-report?indexname=${encodeURIComponent(indexName)}`);
  };

  const confirmDelete = async () => {
    if (indexToDelete) {
      setIsDeleting(true);
      try {
        const success = await deleteIndex(indexToDelete.id);
        if (success) {
          refreshIndexes();
          toast({
            title: 'Success',
            description: `Index "${indexToDelete.name}" has been deleted.`
          });
        } else {
          throw new Error('Delete operation failed');
        }
      } catch (error) {
        console.error('Failed to delete index:', error);
        toast({
          title: 'Error',
          description: 'Failed to delete the index. Please try again later.',
          variant: 'destructive'
        });
      } finally {
        setIsDeleting(false);
        setIndexToDelete(null);
      }
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-2xl font-bold">Available Containers</CardTitle>
        <CardDescription>Manage and view all available containers in the system.</CardDescription>
      </CardHeader>
      <CardContent>
        {indexes && indexes.length > 0 ? (
          <div className="overflow-x-auto">
            <Table aria-label="Available Assistants">
              <TableHeader>
                <TableRow>
                  <PermissionHandler roles={[UserRole.ContentManager]}>
                    <TableHead className="w-[100px]">Actions</TableHead>
                  </PermissionHandler>
                  <TableHead className="w-[250px]">Name</TableHead>
                  <TableHead className="w-[500px]">Description</TableHead>
                  <TableHead className="w-[200px]">Chunk Size/Overlap (%)</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {indexes.map((index) => (
                  <TableRow key={index.id}>
                    <PermissionHandler roles={[UserRole.ContentManager]}>
                      <TableCell>
                        <div className="flex items-start space-x-2">
                          <TooltipProvider>
                            <Tooltip>
                              <TooltipTrigger asChild>
                                <Button
                                  variant="outline"
                                  size="sm"
                                  className="h-8 w-8 p-0"
                                  disabled={isDeleting}
                                  onClick={() => handleEditIndex(index)}>
                                  <Pencil className="h-4 w-4" />
                                  <span className="sr-only">Edit {index.name}</span>
                                </Button>
                              </TooltipTrigger>
                              <TooltipContent>
                                <p>Edit {index.name}</p>
                              </TooltipContent>
                            </Tooltip>
                          </TooltipProvider>

                          <PermissionHandler roles={[UserRole.SystemAdmin]}>
                            <AlertDialog>
                              <TooltipProvider>
                                <Tooltip>
                                  <TooltipTrigger asChild>
                                    <AlertDialogTrigger asChild>
                                      <Button
                                        variant="outline"
                                        size="sm"
                                        disabled={isDeleting}
                                        className="h-8 w-8 p-0"
                                        onClick={() => handleDeleteIndex(index)}>
                                        <Trash2 className="h-4 w-4" />
                                        <span className="sr-only">Delete {index.name}</span>
                                      </Button>
                                    </AlertDialogTrigger>
                                  </TooltipTrigger>
                                  <TooltipContent>
                                    <p>Delete {index.name}</p>
                                  </TooltipContent>
                                </Tooltip>
                              </TooltipProvider>
                              <AlertDialogContent>
                                <AlertDialogHeader>
                                  <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
                                  <AlertDialogDescription>
                                    This action cannot be undone. This will permanently delete the container "
                                    {indexToDelete?.name}" and remove it from our servers.
                                  </AlertDialogDescription>
                                </AlertDialogHeader>
                                <AlertDialogFooter className="flex-col items-stretch sm:flex-row sm:justify-start sm:space-x-2">
                                  <AlertDialogAction onClick={confirmDelete} disabled={isDeleting}>
                                    {isDeleting ? (
                                      <>
                                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                        Deleting...
                                      </>
                                    ) : (
                                      'Yes, delete container'
                                    )}
                                  </AlertDialogAction>
                                  <AlertDialogCancel className="mb-2 sm:mb-0">Cancel</AlertDialogCancel>
                                </AlertDialogFooter>
                              </AlertDialogContent>
                            </AlertDialog>
                          </PermissionHandler>

                          <TooltipProvider>
                            <Tooltip>
                              <TooltipTrigger asChild>
                                <Button
                                  variant="outline"
                                  size="sm"
                                  className="h-8 w-8 p-0"
                                  onClick={() => handleViewStats(index.name)}>
                                  <BarChart className="h-4 w-4" />
                                  <span className="sr-only">View Stats for {index.name}</span>
                                </Button>
                              </TooltipTrigger>
                              <TooltipContent>
                                <p>View Stats for {index.name}</p>
                              </TooltipContent>
                            </Tooltip>
                          </TooltipProvider>
                        </div>
                      </TableCell>
                    </PermissionHandler>
                    <TableCell className="font-medium">{index.name}</TableCell>
                    <TableCell className="font-medium">{index.description ? index.description : 'N/A'}</TableCell>
                    <TableCell className="font-medium">
                      {index.chunkSize === 0 ? 2300 : index.chunkSize}/
                      {index.chunkOverlap === 0 ? 25 : index.chunkOverlap}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        ) : (
          <div className="text-center py-8">
            <Info className="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <p className="text-lg font-medium text-gray-900">No containers available</p>
            <p className="mt-1 text-sm text-gray-500">Get started by adding a new container to your system.</p>
          </div>
        )}
      </CardContent>
      <EditIndexDialog index={indexEditing} setIndexEditing={setIndexEditing} refreshIndexes={refreshIndexes} />
    </Card>
  );
};
export default IndexerComponent;
