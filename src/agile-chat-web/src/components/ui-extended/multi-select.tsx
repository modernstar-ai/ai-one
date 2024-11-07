import React, { useEffect } from 'react';
import { DropdownMenu, DropdownMenuCheckboxItem, DropdownMenuContent, DropdownMenuTrigger } from '../ui/dropdown-menu';
import { DropdownMenuProps } from '@radix-ui/react-dropdown-menu';
import { Badge } from '../ui/badge';
import { Loader2Icon } from 'lucide-react';

interface IChatThreadSettingsProps {
  label?: string;
  items?: readonly string[];
  selectedItems: string[];
  setSelectedItems: (items: string[]) => void;
  className?: string;
}

type MultiSelectInputInputProps = DropdownMenuProps & IChatThreadSettingsProps;

export const MultiSelectInput = React.forwardRef<
  HTMLInputElement,
  MultiSelectInputInputProps
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
>(({ label, items, selectedItems, setSelectedItems, className, ...props }, _ref) => {
  useEffect(() => {
    if (items) {
      const set = new Set(Array.from(selectedItems).filter((oldFolder) => items.includes(oldFolder)));
      setSelectedItems(Array.from(set));
    }
  }, [items]);

  const onCheckedChange = (folder: string) => {
    let newArr = Array.from(selectedItems);

    if (!newArr.includes(folder)) {
      newArr.push(folder);
    } else {
      newArr = newArr.filter((x) => x !== folder);
    }
    setSelectedItems(newArr);
  };

  return (
    <DropdownMenu {...props}>
      <DropdownMenuTrigger className={`border rounded-md ${className}`}>
        {selectedItems.length > 0 ? (
          <div className="flex w-full flex-wrap">
            {Array.from(selectedItems).map((item) => {
              return (
                <Badge variant="outline" key={item}>
                  {item}
                </Badge>
              );
            })}
          </div>
        ) : (
          <div className="flex w-full">
            <p className="p-2">Select {label ? label : 'items'}</p>
          </div>
        )}
      </DropdownMenuTrigger>
      <DropdownMenuContent side="bottom" style={{ width: 'var(--radix-dropdown-menu-trigger-width)' }}>
        {items === undefined || items === null ? (
          <div className="flex justify-center">
            <Loader2Icon className="animate-spin" size={24} />
          </div>
        ) : (
          items.map((item, index) => {
            return (
              <DropdownMenuCheckboxItem
                key={item + index}
                className="w-full min-w-0 select-none cursor-pointer"
                checked={selectedItems.includes(item)}
                onClick={(e) => {
                  e.preventDefault();
                  onCheckedChange(item);
                }}
              >
                {item}
              </DropdownMenuCheckboxItem>
            );
          })
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  );
});
MultiSelectInput.displayName = 'MultiSelectInput';
