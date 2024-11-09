import React, { useEffect, useState } from "react";
import { Label } from "@/components/ui/label";
import { DropdownMenu, DropdownMenuCheckboxItem, DropdownMenuContent, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { LoadingIndicator } from "@/components/ui/loading";
import { DropdownMenuProps } from "@radix-ui/react-dropdown-menu";
import { Badge } from "@/components/ui/badge";
import { Tool } from "@/types/Tool";
import { ReloadIcon } from "@radix-ui/react-icons";

interface IMultiToolSelectorSettingsProps {
  label?: string;
  tools?: Tool[]; // Update to accept tools instead of folders
  selectedToolIds: Set<string>; // Updated to hold tool IDs instead of names
  setSelectedToolIds: (toolIds: Set<string>) => void; // Updated to work with tool IDs
}

type MultiToolNumberInputProps = DropdownMenuProps & IMultiToolSelectorSettingsProps;

export const MultiToolSettingsDropdownInput = React.forwardRef<HTMLDivElement, MultiToolNumberInputProps>(
({ label, tools, selectedToolIds, setSelectedToolIds, ...props }, ref) => {
  // Set tools in state to update dynamically
  const [currentTools, setCurrentTools] = useState<Tool[]>(tools || []);

  useEffect(() => {
    if (tools) {
      setSelectedToolIds(
        new Set(
          Array.from(selectedToolIds).filter((oldToolId) =>
            tools.some((tool) => tool.id === oldToolId)
          )
        )
      );
      setCurrentTools(tools); // Initialize the current tools with the provided tools
    }
  }, [tools, selectedToolIds, setSelectedToolIds]);

  // Function to handle checked changes
  const onCheckedChange = (toolId: string) => {
    const newSelectedToolIds = new Set(selectedToolIds);
    if (selectedToolIds.has(toolId)) {
      newSelectedToolIds.delete(toolId);
    } else {
      newSelectedToolIds.add(toolId);
    }
    setSelectedToolIds(newSelectedToolIds);
  };

   // Function to handle refreshing the selection
   const refreshSelection = () => {
    setSelectedToolIds(new Set()); // Clear the selection
  };

  return (
    <div className="grow" ref={ref}>
      {label && <Label>{label}</Label>}
      <div className="flex items-center w-full mt-1 border rounded-md">
        {/* Dropdown Trigger Section */}
        <DropdownMenu {...props}>
          <DropdownMenuTrigger className="flex-grow p-2 focus:outline-none">
            {selectedToolIds.size > 0 ? (
              <div className="flex w-full flex-wrap">
                {Array.from(selectedToolIds).map((toolId) => {
                  const tool = currentTools.find((tool) => tool.id === toolId);
                  return (
                    <Badge variant="outline" key={toolId} className="mr-1 mb-1">
                      {tool?.name ?? "Unknown Tool"}
                    </Badge>
                  );
                })}
              </div>
            ) : (
              <div className="flex w-full">
                <p className="p-2 text-sm text-gray-800 font-normal">
                  Select the required tools
                </p>
              </div>
            )}
          </DropdownMenuTrigger>

          {/* Dropdown Content Section */}
          <DropdownMenuContent
            side="bottom"
            style={{ width: "var(--radix-dropdown-menu-trigger-width)" }}
          >
            {!currentTools ? (
              <div className="flex justify-center">
                <LoadingIndicator isLoading={true} />
              </div>
            ) : (
              currentTools.map((tool) => (
                <DropdownMenuCheckboxItem
                  key={tool.id}
                  className="w-full min-w-0 select-none cursor-pointer text-xs"
                  checked={selectedToolIds.has(tool.id)}
                  onCheckedChange={() => onCheckedChange(tool.id)}>
                  {tool.name}
                </DropdownMenuCheckboxItem>
              ))
            )}
          </DropdownMenuContent>
        </DropdownMenu>

        {/* Refresh Icon Button */}
        <button
            type="button"
            className="ml-2 p-2 rounded-md text-gray-500 hover:bg-gray-100 focus:outline-none"
            onClick={refreshSelection}
            aria-label="Refresh selection">
            <ReloadIcon width={20} height={20} />
          </button>
      </div>
    </div>
  );
}
);

MultiToolSettingsDropdownInput.displayName = "MultiToolSettingsDropdownInput";