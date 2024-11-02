'use client'

import { useState } from 'react'
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Slider } from "@/components/ui/slider"
import { Label } from "@/components/ui/label"
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { ChevronsUpDown } from "lucide-react"
import SidebarMenu from '@/components/Sidebar'

const folders = [
  { id: "folder1", name: "Folder 1" },
  { id: "folder2", name: "Folder 2" },
  { id: "folder3", name: "Folder 3" },
]

export default function AssistantForm() {
  const [name, setName] = useState('')
  const [greeting, setGreeting] = useState('')
  const [systemMessage, setSystemMessage] = useState('')
  const [group, setGroup] = useState('')
  const [selectedFolders, setSelectedFolders] = useState<string[]>([])
  const [temperature, setTemperature] = useState(0.7)
  const formatValue = (val: number) => val.toFixed(1)

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    // Handle form submission
    console.log({ name, greeting, systemMessage, group, selectedFolders, temperature })
  }

  const toggleFolder = (folderId: string) => {
    setSelectedFolders((current) =>
      current.includes(folderId)
        ? current.filter((id) => id !== folderId)
        : [...current, folderId]
    )
  }
  const [documentLimit, setDocumentLimit] = useState<number | "">(5)
  const [isDocumentLimitValid, setIsDocumentLimitValid] = useState(true)

  const handleDocumentLimitChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.target.value
    const numValue = parseInt(value, 10)

    if (value === "") {
      setDocumentLimit("")
      setIsDocumentLimitValid(true)
    } else if (!isNaN(numValue) && numValue >= 0 && numValue <= 1000) {
      setDocumentLimit(numValue)
      setIsDocumentLimitValid(true)
    } else {
      setIsDocumentLimitValid(false)
    }
  }


  return (
    <div className="flex h-screen bg-background">
      <SidebarMenu />
      <div className="flex-1 p-8 overflow-y-auto">
        <h1 className="text-3xl font-bold mb-6">Create a new AI Assistant</h1>
        <form onSubmit={handleSubmit} className="space-y-6 max-w-2xl">
          <div className="space-y-2">
            <Label htmlFor="name">Name</Label>
            <Input
              id="name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="greeting">Greeting</Label>
            <Textarea
              id="greeting"
              value={greeting}
              onChange={(e) => setGreeting(e.target.value)}
              required
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="systemMessage">System Message</Label>
            <Textarea
              id="systemMessage"
              value={systemMessage}
              onChange={(e) => setSystemMessage(e.target.value)}
              required
            />
          </div>
          <div className="space-y-2">
            <Label>Group</Label>
            <Input
              id="group"
              value={name}
              onChange={(e) => setGroup(e.target.value)}
              required
            />
          </div>
          <div className="space-y-2">
            <Label>Folder</Label>
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline" className="w-full justify-between">
                  {selectedFolders.length > 0
                    ? `${selectedFolders.length} folder${selectedFolders.length > 1 ? 's' : ''} selected`
                    : "Select folders"}
                  <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent className="w-56" side="bottom" align="start" sideOffset={4}>
                <DropdownMenuLabel>Select Folders</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {folders.map((folder) => (
                  <DropdownMenuCheckboxItem
                    key={folder.id}
                    checked={selectedFolders.includes(folder.id)}
                    onCheckedChange={() => toggleFolder(folder.id)}
                  >
                    {folder.name}
                  </DropdownMenuCheckboxItem>
                ))}
              </DropdownMenuContent>
            </DropdownMenu>
          </div>

          <div className="space-y-2">
            <Label htmlFor="documentLimit">Document Limit</Label>
            <Input
              type="number"
              id="documentLimit"
              placeholder="Enter a number between 0 and 1000"
              value={documentLimit}
              onChange={handleDocumentLimitChange}
              min={0}
              max={1000}
              className={!isDocumentLimitValid ? "border-red-500" : ""}
            />
            {!isDocumentLimitValid && (
              <p className="text-sm text-red-500">Please enter a number between 0 and 1000.</p>
            )}
          </div>
          <div className="space-y-2">
            <Label htmlFor="temperature" id="temperature-label">Temperature: from most accurate (0) to most creative (2). Current value: </Label>
            <span>{formatValue(temperature)}</span>
            <div className="flex items-center space-x-4">
              <span>0</span>
              <Slider
                id="temperature"
                value={[temperature]}
                onValueChange={([value]: [number]) => setTemperature(value)}
                min={0}
                max={2}
                step={0.1}
                className="flex-1"
                aria-label="temperature-label"
              />
              <span>2</span>
            </div>
          </div>
          <Button type="submit">Save</Button>
        </form>
      </div>
    </div>
  )
}