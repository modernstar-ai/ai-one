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

const folders = [
  { id: "folder1", name: "Folder 1" },
  { id: "folder2", name: "Folder 2" },
  { id: "folder3", name: "Folder 3" },
]

export default function CreateAIAssistant() {
  const [name, setName] = useState('')
  const [greeting, setGreeting] = useState('')
  const [systemMessage, setSystemMessage] = useState('')
  const [group, setGroup] = useState('')
  const [selectedFolders, setSelectedFolders] = useState<string[]>([])
  const [temperature, setTemperature] = useState(0.5)

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

  return (
    <div className="flex h-screen bg-background">
      
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
            <Select value={group} onValueChange={setGroup}>
              <SelectTrigger aria-label="Select Group">
                <SelectValue placeholder="Select Group" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="group1">Group 1</SelectItem>
                <SelectItem value="group2">Group 2</SelectItem>
                <SelectItem value="group3">Group 3</SelectItem>
              </SelectContent>
            </Select>
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
            <Label id="temperature-label">Temperature (min accurate - max creative)</Label>
            <div className="flex items-center space-x-4">
              <span>0</span>
              <Slider
                value={[temperature]}
                onValueChange={([value]: [number]) => setTemperature(value)}
                max={1}
                step={0.1}
                className="flex-1" 
                aria-label="temperature-label"
              />
              <span>1</span>
            </div>
          </div>
          <Button type="submit">Save</Button>
        </form>
      </div>
    </div>
  )
}