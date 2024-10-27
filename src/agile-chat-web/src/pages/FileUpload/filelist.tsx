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
import  LeftMenu from '@/components/LeftMenu'
import { Link } from 'react-router-dom';

const files = [
  { id: 1, name: "Foundations_of_Nursing_Practice.pdf", state: "Completed", folder: "upload/temp2", submittedOn: "2024-08-10 00:15:30" },
  { id: 2, name: "Foundations_of_Nursing_Practice_Lec1.ppt", state: "Completed", folder: "upload/temp2", submittedOn: "2024-08-10 00:15:30" },
  { id: 3, name: "Foundations_of_Nursing_Practice_Assessment1.pdf", state: "Completed", folder: "upload/temp2", submittedOn: "2024-08-10 00:15:30" },
  { id: 4, name: "Health_and_Society_Lec1.pdf", state: "Completed", folder: "upload/temp2", submittedOn: "2024-08-10 00:15:30" },
  { id: 5, name: "Health_and_Society_Lec1.pdf", state: "Completed", folder: "upload/temp2", submittedOn: "2024-08-10 00:15:30" },
  { id: 6, name: "Health_and_Society_GroupAssessment.pdf", state: "Completed", folder: "upload/temp2", submittedOn: "2024-08-10 00:15:30" },
]

export default function FileList() {
  const [selectedFiles, setSelectedFiles] = useState<number[]>([])

  const toggleFileSelection = (fileId: number) => {
    setSelectedFiles(prev => 
      prev.includes(fileId) 
        ? prev.filter(id => id !== fileId)
        : [...prev, fileId]
    )
  }

  return (

      <div className="flex h-screen bg-background">
        
        <div className="flex-1 p-8 overflow-y-auto">
          <h1 className="text-3xl font-bold mb-6">Your Files</h1>

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

            <Select>
              <SelectTrigger className="w-[180px]" aria-label="Select Folder">
                <SelectValue placeholder="Select Folder" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="folder1">Folder 1</SelectItem>
                <SelectItem value="folder2">Folder 2</SelectItem>
                <SelectItem value="folder3">Folder 3</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="flex justify-between items-center mb-4">
          <Link to="/fileupload" aria-label="Add New File" accessKey="n"><Button  tabIndex={-1} aria-label="Add New File Button">Add New</Button></Link>
          <div className="space-x-2">
              <Button variant="outline" size="icon" aria-label="Refresh">
                <RefreshCw className="h-4 w-4" />
              </Button>
              <Button variant="outline" size="icon" aria-label="Trash">
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
                <TableHead>State</TableHead>
                <TableHead>Folder</TableHead>
                <TableHead>Submitted On</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {files.map((file) => (
                <TableRow key={file.id}>
                  <TableCell>
                    <Checkbox
                      checked={selectedFiles.includes(file.id)}
                      onCheckedChange={() => toggleFileSelection(file.id)}
                      aria-label={`Select file ${file.name}`}
                    />
                  </TableCell>
                  <TableCell>{file.name}</TableCell>
                  <TableCell>{file.state}</TableCell>
                  <TableCell>{file.folder}</TableCell>
                  <TableCell>{file.submittedOn}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      </div>
  )
}

