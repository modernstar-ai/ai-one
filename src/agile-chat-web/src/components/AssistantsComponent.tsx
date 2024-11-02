import React, { useEffect, useState } from 'react';
import { fetchAssistants, deleteAssistant } from '../services/assistantservice';
import { AssistantType, Assistant as BaseAssistant } from '../types/Assistant';
import { Button } from "@/components/ui/button";
import { useNavigate } from 'react-router-dom';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from "@/components/ui/card";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { Badge } from "@/components/ui/badge";
import { Database, Workflow, Globe, Pencil, Trash2, Loader2, Info } from 'lucide-react';
import { toast } from "@/components/ui/use-toast";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";

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
    async function getAssistants() {
      setIsLoading(true);
      try {
        const assistantsData = await fetchAssistants();
        if (assistantsData) {
          setAssistants(assistantsData);
        }
      } catch (error) {
        console.error('Failed to fetch assistants:', error);
        toast({
          title: "Error",
          description: "Failed to load assistants. Please try again later.",
          variant: "destructive",
        });
      } finally {
        setIsLoading(false);
      }
    }
    getAssistants();
  }, []);

  const handleEditAssistant = (id: string,type: string) => {

    navigate(`/assistant?id=${id}`);
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
            title: "Success",
            description: `Assistant "${assistantToDelete.name}" has been deleted.`,
          });
        } else {
          throw new Error('Delete operation failed');
        }
      } catch (error) {
        console.error('Failed to delete assistant:', error);
        toast({
          title: "Error",
          description: "Failed to delete the assistant. Please try again later.",
          variant: "destructive",
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
        return <Badge className="bg-green-100 text-green-800">Draft</Badge>;
      case 'published':
      case '1':
        return <Badge variant="secondary" className="bg-yellow-100 text-yellow-800">Published</Badge>;
      case 'archived':
      case '2':
        return <Badge variant="destructive" className="bg-red-100 text-red-800">Archived</Badge>;
      default:
        return <Badge variant="outline" className="bg-gray-100 text-gray-800">Unknown</Badge>;
    }
  };

  const getTypeIcon = (type: Assistant['type']) => {
    switch (type) {
      case AssistantType.Chat:
        return <Database className="h-4 w-4" aria-hidden="true" />;
      case AssistantType.Search:
        return <Workflow className="h-4 w-4" aria-hidden="true" />;
      // case 'ExternalAPI':
      //   return <Globe className="h-4 w-4" aria-hidden="true" />;
      default:
        return null;
    }
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
                  <TableHead className="w-[200px]">Type</TableHead>
                  <TableHead className="w-[500px]">Description</TableHead>
                 
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
                                onClick={() => handleEditAssistant(assistant.id,assistant.type)}
                              >
                                <Pencil className="h-4 w-4" />
                                <span className="sr-only">Edit {assistant.name}</span>
                              </Button>
                            </TooltipTrigger>
                            <TooltipContent>
                              <p>Edit {assistant.name}</p>
                            </TooltipContent>
                          </Tooltip>
                        </TooltipProvider>
                        <AlertDialog>
                          <TooltipProvider>
                            <Tooltip>
                              <TooltipTrigger asChild>
                                <AlertDialogTrigger asChild>
                                  <Button
                                    variant="outline"
                                    size="sm"
                                    className="h-8 w-8 p-0"
                                    onClick={() => handleDeleteAssistant(assistant)}
                                  >
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
                                This action cannot be undone. This will permanently delete the
                                assistant "{assistantToDelete?.name}" and remove it from our servers.
                              </AlertDialogDescription>
                            </AlertDialogHeader>
                            <AlertDialogFooter className="flex-col items-stretch sm:flex-row sm:justify-start sm:space-x-2">
                              <AlertDialogCancel className="mb-2 sm:mb-0">Cancel</AlertDialogCancel>
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
                            </AlertDialogFooter>
                          </AlertDialogContent>
                        </AlertDialog>
                      </div>
                    </TableCell>
                    <TableCell className="font-medium">{assistant.name}</TableCell>
                    <TableCell>{getStatusBadge(assistant.status)}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        {getTypeIcon(assistant.type)}
                        <span>{assistant.type}</span>
                      </div>
                    </TableCell>
                    {/* <TableCell className="font-mono text-xs">
                      <TooltipProvider>
                        <Tooltip>
                          <TooltipTrigger asChild>
                            <span className="cursor-help">{assistant.id}</span>
                          </TooltipTrigger>
                          <TooltipContent>
                            <p>Unique identifier for this assistant</p>
                          </TooltipContent>
                        </Tooltip>
                      </TooltipProvider>
                    </TableCell> */}
                  
                
                    <TableCell className="font-medium">{assistant.description}</TableCell>
                    
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