import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { RefreshCw, Trash2 } from "lucide-react"
import { useState } from "react"
import SidebarMenu from '@/components/Sidebar'
import { Link } from 'react-router-dom'

const assistants = [
  { id: 1, name: "Foundations of Nursing Practice 1 A - Content Chatbot", group: "Completed", folder: "upload/temp2", submittedOn: "2024-08-10 00:15:30" },
  { id: 2, name: "Nurse Content AI Assistant", group: "Completed", folder: "upload/temp2", submittedOn: "2024-08-10 00:15:30" },
  { id: 3, name: "Clinical Practice 2A Chatbot", group: "Completed", folder: "upload/temp2", submittedOn: "2024-08-10 00:15:30" },
  { id: 4, name: "Clinical Practice 3A Chatbot", group: "Completed", folder: "upload/temp2", submittedOn: "2024-08-10 00:15:30" },
  { id: 5, name: "Health & Society Chatbot", group: "Completed", folder: "upload/temp2", submittedOn: "2024-08-10 00:15:30" },
  { id: 6, name: "Health & Society Content Chatbot", group: "Completed", folder: "upload/temp2", submittedOn: "2024-08-10 00:15:30" },
]

export default function AssistantList() {
  const [selectedAssistants, setSelectedAssistants] = useState<number[]>([])

  const toggleAssistantSelection = (assistantId: number) => {
    setSelectedAssistants(prev => 
      prev.includes(assistantId) 
        ? prev.filter(id => id !== assistantId)
        : [...prev, assistantId]
    )
  }

  return (
    <div className="flex h-screen bg-background">
      <SidebarMenu />
      <div className="flex-1 p-8 overflow-y-auto">
        <h1 className="text-3xl font-bold mb-6">AI Assistants</h1>

        <div className="flex space-x-4 mb-4">
          <Select>
            <SelectTrigger className="w-[180px]" aria-label="Select Group">
              <SelectValue placeholder="Select Group" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="group1">Group 1</SelectItem>
              <SelectItem value="group2">Group 2</SelectItem>
              <SelectItem value="group3">Group 3</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div className="flex justify-between items-center mb-4">
          <Link to="/AiAssistant" aria-label="Create New Assistant" accessKey="n">
            <Button tabIndex={-1} aria-label="Create New Assistant Button">Create New</Button>
          </Link>
          <div className="space-x-2">
            <Button variant="outline" size="icon" aria-label="Refresh">
              <RefreshCw className="h-4 w-4" />
            </Button>
            <Button variant="outline" size="icon" aria-label="Delete">
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>
        </div>

        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-[50px]" aria-label="Select row">
                <span className="sr-only">Select</span>
              </TableHead>
              <TableHead>Name</TableHead>
              <TableHead>Group</TableHead>
              <TableHead>Folder</TableHead>
              <TableHead>Submitted On</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {assistants.map((assistant) => (
              <TableRow key={assistant.id}>
                <TableCell>
                  <Checkbox
                    checked={selectedAssistants.includes(assistant.id)}
                    onCheckedChange={() => toggleAssistantSelection(assistant.id)}
                    aria-label={`Select assistant ${assistant.name}`}
                  />
                </TableCell>
                <TableCell>{assistant.name}</TableCell>
                <TableCell>{assistant.group}</TableCell>
                <TableCell>{assistant.folder}</TableCell>
                <TableCell>{assistant.submittedOn}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </div>
  )
}