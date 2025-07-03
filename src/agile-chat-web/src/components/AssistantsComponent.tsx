import React, { useEffect, useState } from 'react';
import { fetchAssistants, deleteAssistant } from '../services/assistantservice';
import { Assistant as BaseAssistant } from '../types/Assistant';
import { Button } from '@/components/ui/button';
import { useNavigate } from 'react-router-dom';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
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
import { Badge } from '@/components/ui/badge';
import { Pencil, Trash2, Loader2, Info, MessageSquare } from 'lucide-react';
import { toast } from '@/components/ui/use-toast';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';
import { PermissionHandler } from '@/authentication/permission-handler/permission-handler';
import { UserRole } from '@/authentication/user-roles';

type Assistant = Omit<BaseAssistant, 'status'> & {
  status: string | number;
};

const AssistantsComponent: React.FC = () => {
  const [assistants, setAssistants] = useState<Assistant[]>([]);
  const [assistantToDelete, setAssistantToDelete] = useState<Assistant | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isDeleting, setIsDeleting] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchData = async () => {
      setIsLoading(true);
      try {
        const assistants = await fetchAssistants();
        setAssistants(assistants ?? []);
      } catch (error) {
        console.error('Failed to fetch data:', error);
        toast({
          title: 'Error',
          description: 'Failed to load data. Please try again later.',
          variant: 'destructive'
        });
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, []);

  const handleEditAssistant = (id: string) => {
    navigate(`/assistant?id=${id}`);
  };

  const handleLaunchAssistant = (id: string) => {
    navigate(`/chat`, { state: { assistantId: id } });
  };

  const handleDeleteAssistant = async (assistant: Assistant) => {
    setAssistantToDelete(assistant);
  };

  const confirmDelete = async () => {
    if (assistantToDelete) {
      setIsDeleting(true);
      try {
        const success = await deleteAssistant(assistantToDelete.id);
        if (success) {
          setAssistants((prevAssistants) => prevAssistants.filter((t) => t.id !== assistantToDelete.id));
          toast({
            title: 'Success',
            description: `Assistant "${assistantToDelete.name}" has been deleted.`
          });
        } else {
          throw new Error('Delete operation failed');
        }
      } catch (error) {
        console.error('Failed to delete assistant:', error);
        toast({
          title: 'Error',
          description: 'Failed to delete the assistant. Please try again later.',
          variant: 'destructive'
        });
      } finally {
        setIsDeleting(false);
        setAssistantToDelete(null);
      }
    }
  };

  const getStatusBadge = (status: string | number) => {
    const statusString = String(status).toLowerCase();
    switch (statusString) {
      case 'draft':
      case '0':
        return <Badge className="bg-yellow-100 text-yellow-800 hover:bg-yellow-100">Draft</Badge>;
      case 'published':
      case '1':
        return <Badge className="bg-green-100 text-green-800 hover:bg-green-100">Published</Badge>;
      case 'archived':
      case '2':
        return <Badge className="bg-gray-100 text-gray-800 hover:bg-gray-100">Archived</Badge>;
      default:
        return <Badge className="bg-gray-100 text-gray-800">Unknown</Badge>;
    }
  };

  const getTypeIcon = () => {
    return <MessageSquare className="h-4 w-4" aria-hidden="true" />;
    // switch (type) {
    //   case AssistantType.Chat:
    //     return <MessageSquare className="h-4 w-4" aria-hidden="true" />;
    //   case AssistantType.Search:
    //     return <FileSearch className="h-4 w-4" aria-hidden="true" />;
    //   // case 'ExternalAPI':
    //   //   return <Globe className="h-4 w-4" aria-hidden="true" />;
    //   default:
    //     return null;
    // }
  };

  if (isLoading) {
    return (
      <Card>
        <CardContent className="flex items-center justify-center h-64">
          <Loader2 className="h-8 w-8 animate-spin" />
          <span className="sr-only">Loading assistants...</span>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-2xl font-bold">Available Assistants</CardTitle>
        <CardDescription>Manage and view all available assistants in the system.</CardDescription>
      </CardHeader>
      <CardContent>
        {assistants.length > 0 ? (
          <div className="overflow-x-auto">
            <Table aria-label="Available Assistants">
              <TableHeader>
                <TableRow>
                  <TableHead className="w-[100px]">Actions</TableHead>
                  <TableHead className="w-[250px]">Name</TableHead>
                  <TableHead className="w-[40px]">Status</TableHead>
                  <TableHead className="w-[200px]">Container</TableHead>
                  <TableHead className="w-[500px]">Description</TableHead>
                  <TableHead className="w-[500px]">Supported models</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {assistants.map((assistant) => (
                  <TableRow key={assistant.id}>
                    <TableCell>
                      <div className="flex items-start space-x-2">
                        <TooltipProvider>
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <Button
                                variant="outline"
                                size="sm"
                                className="h-8 w-8 p-0"
                                onClick={() => handleLaunchAssistant(assistant.id)}>
                                {getTypeIcon()}
                                <span className="sr-only">Chat with {assistant.name}</span>
                              </Button>
                            </TooltipTrigger>
                            <TooltipContent>
                              <p>Chat {assistant.name}</p>
                            </TooltipContent>
                          </Tooltip>
                        </TooltipProvider>
                        <PermissionHandler roles={[UserRole.ContentManager]} pac={assistant.accessControl}>
                          <TooltipProvider>
                            <Tooltip>
                              <TooltipTrigger asChild>
                                <Button
                                  variant="outline"
                                  size="sm"
                                  className="h-8 w-8 p-0"
                                  onClick={() => handleEditAssistant(assistant.id)}>
                                  <Pencil className="h-4 w-4" />
                                  <span className="sr-only">Edit {assistant.name}</span>
                                </Button>
                              </TooltipTrigger>
                              <TooltipContent>
                                <p>Edit {assistant.name}</p>
                              </TooltipContent>
                            </Tooltip>
                          </TooltipProvider>
                        </PermissionHandler>
                        <PermissionHandler roles={[UserRole.SystemAdmin]}>
                          <AlertDialog>
                            <TooltipProvider>
                              <Tooltip>
                                <TooltipTrigger asChild>
                                  <AlertDialogTrigger asChild>
                                    <Button
                                      variant="outline"
                                      size="sm"
                                      className="h-8 w-8 p-0"
                                      onClick={() => handleDeleteAssistant(assistant)}>
                                      <Trash2 className="h-4 w-4" />
                                      <span className="sr-only">Delete {assistant.name}</span>
                                    </Button>
                                  </AlertDialogTrigger>
                                </TooltipTrigger>
                                <TooltipContent>
                                  <p>Delete {assistant.name}</p>
                                </TooltipContent>
                              </Tooltip>
                            </TooltipProvider>
                            <AlertDialogContent>
                              <AlertDialogHeader>
                                <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
                                <AlertDialogDescription>
                                  This action cannot be undone. This will permanently delete the assistant "
                                  {assistantToDelete?.name}" and remove it from our servers.
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
                                    'Yes, delete assistant'
                                  )}
                                </AlertDialogAction>
                                <AlertDialogCancel className="mb-2 sm:mb-0">Cancel</AlertDialogCancel>
                              </AlertDialogFooter>
                            </AlertDialogContent>
                          </AlertDialog>
                        </PermissionHandler>
                      </div>
                    </TableCell>
                    <TableCell className="font-medium">{assistant.name}</TableCell>
                    <TableCell>{getStatusBadge(assistant.status)}</TableCell>
                    <TableCell>{assistant.filterOptions.indexName}</TableCell>
                    <TableCell className="font-medium">{assistant.description}</TableCell>
                    <TableCell className="font-medium">
                      {assistant.modelOptions.allowModelSelection
                        ? assistant.modelOptions.models
                            .filter((m) => m.isSelected)
                            .map((model) => model.modelId)
                            .join(', ')
                        : 'GPT-4o'}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        ) : (
          <div className="text-center py-8">
            <Info className="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <p className="text-lg font-medium text-gray-900">No assistants available</p>
            <p className="mt-1 text-sm text-gray-500">Get started by adding a new assistant to your system.</p>
          </div>
        )}
      </CardContent>
    </Card>
  );
};

export default AssistantsComponent;
