import { CosmosFile } from '@/models/filemetadata';
import { Card, CardContent } from './ui/card';
import { Checkbox } from './ui/checkbox';
import { File, Folder, MoreVertical, Trash2 } from 'lucide-react';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger
} from './ui/dropdown-menu';
import { Button } from './ui/button';
import { Badge } from './ui/badge';
import { FileEditDialog } from '@/pages/FileUpload/Edit';

interface Props {
  file: CosmosFile;
  selectedFiles: string[];
  toggleFileSelection: (id: string) => void;
  simplifyContentType: (v: string) => string;
  formatBytesToKB: (v: number) => string;
  handleDeleteSingleFile: (id: string) => Promise<void>;
  refresh: () => Promise<void>;
  disabled: boolean;
}

export const FileCard = (props: Props) => {
  const {
    file,
    selectedFiles,
    toggleFileSelection,
    simplifyContentType,
    formatBytesToKB,
    disabled,
    handleDeleteSingleFile,
    refresh
  } = props;

  return (
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
                    handleRefresh={refresh}
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
                disabled={disabled}>
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
};
