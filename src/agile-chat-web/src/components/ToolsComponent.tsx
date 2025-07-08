import React, { useEffect, useState } from 'react';
import { fetchTools, deleteTool } from '../services/toolservice';
import { Tool as BaseTool } from '../types/Tool';
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

type Tool = Omit<BaseTool, 'status'> & {
  status: string | number;
};

const ToolsComponent: React.FC = () => {
  const [tools, setTools] = useState<Tool[]>([]);
  const [toolToDelete, setToolToDelete] = useState<Tool | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isDeleting, setIsDeleting] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    async function getTools() {
      setIsLoading(true);
      try {
        const toolsData = await fetchTools();
        if (toolsData) {
          setTools(toolsData);
        }
      } catch (error) {
        console.error('Failed to fetch tools:', error);
        toast({
          title: "Error",
          description: "Failed to load tools. Please try again later.",
          variant: "destructive",
        });
      } finally {
        setIsLoading(false);
      }
    }
    getTools();
  }, []);

  const handleEditTool = (id: string,type: string) => {

    if(type == "Database")
    {
      navigate(`/connecttodb?id=${id}`);
    }
    else  if(type == "ExternalAPI")
    {
      navigate(`/connecttoapi?id=${id}`);
    }
    else  if(type == "LogicApp")
    {
      navigate(`/connecttologicapp?id=${id}`);
    }
  };

  const handleDeleteTool = async (tool: Tool) => {
    setToolToDelete(tool);
  };

  const confirmDelete = async () => {
    if (toolToDelete) {
      setIsDeleting(true);
      try {
        const success = await deleteTool(toolToDelete.id);
        if (success) {
          setTools((prevTools) => prevTools.filter((t) => t.id !== toolToDelete.id));
          toast({
            title: "Success",
            description: `Tool "${toolToDelete.name}" has been deleted.`,
          });
        } else {
          throw new Error('Delete operation failed');
        }
      } catch (error) {
        console.error('Failed to delete tool:', error);
        toast({
          title: "Error",
          description: "Failed to delete the tool. Please try again later.",
          variant: "destructive",
        });
      } finally {
        setIsDeleting(false);
        setToolToDelete(null);
      }
    }
  };

  const getStatusBadge = (status: string | number) => {
    const statusString = String(status).toLowerCase();
    switch (statusString) {
      case 'active':
      case '0':
        return <Badge className="bg-green-100 text-green-800">Active</Badge>;
      case 'inactive':
      case '1':
        return <Badge variant="secondary" className="bg-yellow-100 text-yellow-800">Inactive</Badge>;
      case 'deprecated':
      case '2':
        return <Badge variant="destructive" className="bg-red-100 text-red-800">Deprecated</Badge>;
      default:
        return <Badge variant="outline" className="bg-gray-100 text-gray-800">Unknown</Badge>;
    }
  };

  const getTypeIcon = (type: Tool['type']) => {
    switch (type) {
      case 'Database':
        return <Database className="h-4 w-4" aria-hidden="true" />;
      case 'LogicApp':
        return <Workflow className="h-4 w-4" aria-hidden="true" />;
      case 'ExternalAPI':
        return <Globe className="h-4 w-4" aria-hidden="true" />;
      default:
        return null;
    }
  };

  if (isLoading) {
    return (
      <Card>
        <CardContent className="flex items-center justify-center h-64">
          <Loader2 className="h-8 w-8 animate-spin" />
          <span className="sr-only">Loading tools...</span>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-2xl font-bold">Available Tools</CardTitle>
        <CardDescription>Manage and view all available tools in the system.</CardDescription>
      </CardHeader>
      <CardContent>
        {tools.length > 0 ? (
          <div className="overflow-x-auto">
            <Table aria-label="Available Tools">
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
                {tools.map((tool) => (
                  <TableRow key={tool.id}>
                    <TableCell>
                      <div className="flex items-start space-x-2">
                        <TooltipProvider>
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <Button
                                variant="outline"
                                size="sm"
                                className="h-8 w-8 p-0"
                                onClick={() => handleEditTool(tool.id,tool.type)}
                              >
                                <Pencil className="h-4 w-4" />
                                <span className="sr-only">Edit {tool.name}</span>
                              </Button>
                            </TooltipTrigger>
                            <TooltipContent>
                              <p>Edit {tool.name}</p>
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
                                    onClick={() => handleDeleteTool(tool)}
                                  >
                                    <Trash2 className="h-4 w-4" />
                                    <span className="sr-only">Delete {tool.name}</span>
                                  </Button>
                                </AlertDialogTrigger>
                              </TooltipTrigger>
                              <TooltipContent>
                                <p>Delete {tool.name}</p>
                              </TooltipContent>
                            </Tooltip>
                          </TooltipProvider>
                          <AlertDialogContent>
                            <AlertDialogHeader>
                              <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
                              <AlertDialogDescription>
                                This action cannot be undone. This will permanently delete the
                                tool "{toolToDelete?.name}" and remove it from our servers.
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
                                  'Yes, delete tool'
                                )}
                              </AlertDialogAction>
                            </AlertDialogFooter>
                          </AlertDialogContent>
                        </AlertDialog>
                      </div>
                    </TableCell>
                    <TableCell className="font-medium">{tool.name}</TableCell>
                    <TableCell>{getStatusBadge(tool.status)}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        {getTypeIcon(tool.type)}
                        <span>{tool.type}</span>
                      </div>
                    </TableCell>
                    {/* <TableCell className="font-mono text-xs">
                      <TooltipProvider>
                        <Tooltip>
                          <TooltipTrigger asChild>
                            <span className="cursor-help">{tool.id}</span>
                          </TooltipTrigger>
                          <TooltipContent>
                            <p>Unique identifier for this tool</p>
                          </TooltipContent>
                        </Tooltip>
                      </TooltipProvider>
                    </TableCell> */}
                  
                
                    <TableCell className="font-medium">{tool.description}</TableCell>
                    
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        ) : (
          <div className="text-center py-8">
            <Info className="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <p className="text-lg font-medium text-gray-900">No tools available</p>
            <p className="mt-1 text-sm text-gray-500">Get started by adding a new tool to your system.</p>
          </div>
        )}
      </CardContent>
    </Card>
  );
};

export default ToolsComponent;