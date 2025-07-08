import { useState } from 'react';
import { toast } from './ui/use-toast';
import { useNavigate } from 'react-router-dom';
import { deleteIndex } from '@/services/indexes-service';
import { Index } from '@/models/indexmetadata';
import { Loader2, Info, Trash2, Pencil, BarChart, MoreVertical, Database, FileText } from 'lucide-react';
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
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger
} from '@/components/ui/dropdown-menu';
import { useIndexes } from '@/hooks/use-indexes';
import { PermissionHandler } from '@/authentication/permission-handler/permission-handler';
import { UserRole } from '@/authentication/user-roles';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from './ui/table';
import { EditIndexDialog } from '@/pages/Indexes/Edit/EditIndex';
import { Input } from './ui/input';
import { Badge } from './ui/badge';

const IndexerComponent: React.FC = () => {
  const { indexes, indexesLoading, refreshIndexes } = useIndexes();
  const [indexToDelete, setIndexToDelete] = useState<Index | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [indexEditing, setIndexEditing] = useState<Index | undefined>(undefined);
  const navigate = useNavigate();
  const [deleteText, setDeleteText] = useState<string>('');

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
        setDeleteText('');
      }
    }
  };

  // Mobile container card component
  const ContainerCard = ({ index }: { index: Index }) => (
    <Card className="mb-4">
      <CardContent className="p-4">
        <div className="flex items-start justify-between mb-3">
          <div className="flex items-start space-x-3 flex-1 min-w-0">
            <Database className="h-5 w-5 text-muted-foreground flex-shrink-0 mt-1" />
            <div className="flex-1 min-w-0">
              <h3 className="font-semibold text-base truncate">{index.name}</h3>
              <p className="text-sm text-muted-foreground mt-1">{index.description || 'No description available'}</p>
            </div>
          </div>

          <PermissionHandler roles={[UserRole.ContentManager]}>
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
                  <span className="sr-only">Open menu for {index.name}</span>
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent
                align="end"
                side="bottom"
                sideOffset={5}
                onCloseAutoFocus={(e) => e.preventDefault()}>
                <DropdownMenuItem onClick={() => handleViewStats(index.name)}>
                  <BarChart className="h-4 w-4 mr-2" />
                  View Stats
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => handleEditIndex(index)}>
                  <Pencil className="h-4 w-4 mr-2" />
                  Edit
                </DropdownMenuItem>
                <PermissionHandler roles={[UserRole.SystemAdmin]}>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem
                    onClick={() => handleDeleteIndex(index)}
                    className="text-destructive focus:text-destructive"
                    disabled={isDeleting}>
                    <Trash2 className="h-4 w-4 mr-2" />
                    Delete
                  </DropdownMenuItem>
                </PermissionHandler>
              </DropdownMenuContent>
            </DropdownMenu>
          </PermissionHandler>
        </div>

        <div className="space-y-3 text-sm">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-2">
              <FileText className="h-4 w-4 text-muted-foreground" />
              <span className="text-muted-foreground">Chunk Configuration:</span>
            </div>
            <Badge variant="secondary" className="text-xs">
              {index.chunkSize === 0 ? 2300 : index.chunkSize} / {index.chunkOverlap === 0 ? 25 : index.chunkOverlap}%
            </Badge>
          </div>
        </div>

        <div className="flex gap-2 mt-4">
          <Button size="sm" variant="outline" onClick={() => handleViewStats(index.name)} className="flex-1">
            <BarChart className="h-4 w-4 mr-2" />
            View Stats
          </Button>
          <PermissionHandler roles={[UserRole.ContentManager]}>
            <Button variant="outline" size="sm" onClick={() => handleEditIndex(index)}>
              <Pencil className="h-4 w-4 mr-2" />
              Edit
            </Button>
          </PermissionHandler>
        </div>
      </CardContent>
    </Card>
  );

  return (
    <div className="min-w-0">
      <Card>
        <CardHeader>
          <CardTitle className="text-xl md:text-2xl font-bold">Available Containers</CardTitle>
          <CardDescription>Manage and view all available containers in the system.</CardDescription>
        </CardHeader>
        <CardContent className="min-w-0">
          {indexes && indexes.length > 0 ? (
            <>
              {/* Desktop Table View */}
              <div className="hidden lg:block overflow-x-auto">
                <div className="min-w-full">
                  <Table aria-label="Available Containers">
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
                                          <p className="mt-2">
                                            Please Type "<strong>delete {indexToDelete?.name}</strong>" below to
                                            confirm:
                                          </p>
                                          <Input
                                            className="mt-2"
                                            autoFocus={true}
                                            onChange={(e) => setDeleteText(e.target.value)}
                                          />
                                        </AlertDialogDescription>
                                      </AlertDialogHeader>
                                      <AlertDialogFooter className="flex-col items-stretch sm:flex-row sm:justify-start sm:space-x-2">
                                        <AlertDialogAction
                                          onClick={confirmDelete}
                                          disabled={deleteText !== `delete ${indexToDelete?.name}` || isDeleting}>
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
                            {index.chunkOverlap === 0 ? 25 : index.chunkOverlap}%
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              </div>

              {/* Mobile Card View */}
              <div className="lg:hidden space-y-4">
                {indexes.map((index) => (
                  <ContainerCard key={index.id} index={index} />
                ))}
              </div>
            </>
          ) : (
            <div className="text-center py-8">
              <Info className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <p className="text-lg font-medium text-gray-900">No containers available</p>
              <p className="mt-1 text-sm text-gray-500">Get started by adding a new container to your system.</p>
            </div>
          )}
        </CardContent>

        {/* Delete Confirmation Dialog for Mobile */}
        <AlertDialog
          open={!!indexToDelete}
          onOpenChange={() => {
            setIndexToDelete(null);
            setDeleteText('');
          }}>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
              <AlertDialogDescription>
                This action cannot be undone. This will permanently delete the container "{indexToDelete?.name}" and
                remove it from our servers.
                <p className="mt-2">
                  Please Type "<strong>delete {indexToDelete?.name}</strong>" below to confirm:
                </p>
                <Input
                  className="mt-2"
                  autoFocus={true}
                  value={deleteText}
                  onChange={(e) => setDeleteText(e.target.value)}
                  placeholder={`delete ${indexToDelete?.name}`}
                />
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter className="flex-col items-stretch sm:flex-row sm:justify-start sm:space-x-2">
              <AlertDialogAction
                onClick={confirmDelete}
                disabled={deleteText !== `delete ${indexToDelete?.name}` || isDeleting}>
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

        <EditIndexDialog index={indexEditing} setIndexEditing={setIndexEditing} refreshIndexes={refreshIndexes} />
      </Card>
    </div>
  );
};

export default IndexerComponent;
