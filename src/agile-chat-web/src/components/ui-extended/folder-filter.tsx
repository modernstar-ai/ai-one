/* eslint-disable @typescript-eslint/no-explicit-any */
import React, { useRef } from 'react';
import { PlusIcon, XIcon } from 'lucide-react';
import { Label } from '../ui/label';
import { Button } from '../ui/button';
import { Input } from '../ui/input';

interface IChatThreadSettingsProps {
  label?: string;
  className?: string;
  value?: string[];
  onChange?: (value: string[]) => void;
  onBlur?: () => void;
}

type FolderFiltersInputProps = IChatThreadSettingsProps;

export const FoldersFilterInput = React.forwardRef<
  HTMLInputElement,
  FolderFiltersInputProps
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
>((props, _ref) => {
  const inputRef = useRef<HTMLInputElement>(null);
  const { label, value = [], onChange, onBlur } = props;

  const addFolder = () => {
    onChange?.([...value, '']);
  };

  const removeFolder = (index: number) => {
    const newFolders = [...value];
    newFolders.splice(index, 1);
    onChange?.(newFolders);
  };

  const updateFolder = (index: number, newValue: string) => {
    const newFolders = [...value];
    newFolders[index] = newValue;
    onChange?.(newFolders);
  };

  return (
    <>
      <div className="w-full">
        <div className="flex items-center">
          {label && <Label>{label}</Label>}
          <Button onClick={addFolder} className="h-6 w-6 ms-3" size="icon">
            <PlusIcon />
          </Button>
        </div>

        {value.map((folder, index) => {
          return (
            <div key={'folder' + index} className="mt-2 flex items-center">
              <Input
                className="mx-2"
                value={folder}
                onChange={(e) => updateFolder(index, e.target.value)}
                placeholder="Enter folder name"
              />
              <Button onClick={() => removeFolder(index)} className="h-full p-1" size={'icon'}>
                <XIcon />
              </Button>
            </div>
          );
        })}
      </div>
      <Input type="hidden" ref={inputRef} value={JSON.stringify(value)} onChange={() => {}} onBlur={onBlur} />
    </>
  );
});
FoldersFilterInput.displayName = 'FoldersFilterInput';
