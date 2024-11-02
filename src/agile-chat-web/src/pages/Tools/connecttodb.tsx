import { useEffect, useState } from "react"
import { useSearchParams } from "react-router-dom"
import SidebarMenu from '@/components/Sidebar'
import SimpleHeading from '@/components/Heading-Simple'
import { Card, CardContent } from "@/components/ui/card"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import * as z from "zod"
import { Button } from "@/components/ui/button"
import { useToast } from "@/components/ui/use-toast"
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from "@/components/ui/form"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { createTool, fetchToolById, updateTool } from '@/services/toolservice'
import type { Tool } from '@/types/Tool'

//todo: replace with server side implementation
const generateGuid = () => {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
    const r = Math.random() * 16 | 0;
    const v = c === 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
};

// Define schema with Zod
const formSchema = z.object({
  name: z.string().min(1, { message: "Name is required" }),
  description: z.string().optional(),
  status: z.enum(['Active', 'Inactive', 'Deprecated'] as const),
  jsonTemplate: z.string().min(1, { message: "JSON Template is required" }),
  databaseDSN: z.string().min(1, { message: "Database DSN is required" }),
  databaseQuery: z.string().min(1, { message: "Database Query is required" }),
})

type FormValues = z.infer<typeof formSchema>

const ConnectToDB = () => {
  const [searchParams] = useSearchParams()
  const toolId = searchParams.get('id')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [toolDates, setToolDates] = useState({
    createddate: '',  // Changed to lowercase
    lastupdateddate: ''  // Changed to lowercase
  })
  const { toast } = useToast()

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      name: "",
      description: "",
      status: "Active" as const,
      jsonTemplate: "",
      databaseDSN: "",
      databaseQuery: ""
    },
  })

  // Fetch existing tool data if editing
  useEffect(() => {
    const loadTool = async () => {
      if (toolId) {
        const tool = await fetchToolById(toolId) as Tool
        if (tool) {
          form.reset({
            name: tool.name,
            description: tool.description || "",
            status: tool.status,
            jsonTemplate: tool.jsonTemplate,
            databaseDSN: tool.databaseDSN,
            databaseQuery: tool.databaseQuery,
          })
          // Store dates separately
          setToolDates({
            createddate: tool.createddate,
            lastupdateddate: tool.lastupdateddate
          })
        } else {
          toast({
            variant: "destructive",
            title: "Error",
            description: "Failed to load tool data",
          })
        }
      }
    }
    loadTool()
  }, [toolId, form, toast])

  // Form submit handler
  const onSubmit = async (values: FormValues) => {
    setIsSubmitting(true);
    try {
      const now = new Date().toISOString();
      toast({ title: "saving", description: "Saving..." });
      const toolData: Tool = {
        id: toolId || generateGuid(),
        name: values.name,
        type: 'Database',
        status: values.status,
        description: values.description,
        jsonTemplate: values.jsonTemplate,
        databaseDSN: values.databaseDSN,
        databaseQuery: values.databaseQuery,
        method: 'GET',
        api: '',
        createddate: toolId ? toolDates.createddate : now,
        lastupdateddate: now
      };

      if (toolId) {
        const result = await updateTool(toolData);
        if (result) {
          toast({
            title: "Success",
            description: "Tool updated successfully",            
          });
        } else {
          throw new Error("Failed to update tool");
        }
      } else {
        const result = await createTool(toolData);
        if (result) {
          toast({
            title: "Success",
            description: "Tool created successfully",
          });
          form.reset();
        } else {
          throw new Error("Failed to create tool");
        }
      }
    } catch (error) {
      toast({
        variant: "destructive",
        title: "Error",
        description: error instanceof Error ? error.message : "An error occurred",
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="flex h-screen bg-background text-foreground">
      <SidebarMenu />

      <div className="flex-1 flex flex-col">
        <SimpleHeading
          Title="Tools"
          Subtitle={toolId ? "Edit Database Tool" : "Create New Database Tool"}
          DocumentCount={0}
        />

        <div className="flex-1 p-4 overflow-auto">
          <main className="flex-1 space-y-6">
            <Card>
              <CardContent className="space-y-6">
                <Form {...form}>
                  <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8">
                    <FormField
                      control={form.control}
                      name="name"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Name</FormLabel>
                          <FormControl>
                            <Input {...field} placeholder="AdventureWorksCustomer" />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                    <FormField
                      control={form.control}
                      name="description"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Description</FormLabel>
                          <FormControl>
                            <Input {...field} placeholder="Look up record details based on name" />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                    <FormField
                      control={form.control}
                      name="status"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Status</FormLabel>
                          <Select onValueChange={field.onChange} defaultValue={field.value}>
                            <FormControl>
                              <SelectTrigger>
                                <SelectValue placeholder="Select status" />
                              </SelectTrigger>
                            </FormControl>
                            <SelectContent>
                              <SelectItem value="Active">Active</SelectItem>
                              <SelectItem value="Inactive">Inactive</SelectItem>
                              <SelectItem value="Deprecated">Deprecated</SelectItem>
                            </SelectContent>
                          </Select>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                    <FormField
                      control={form.control}
                      name="jsonTemplate"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>JSON Template</FormLabel>
                          <FormControl>
                            <Textarea
                              {...field}
                              placeholder={`{
  "type": "object",
  "properties": {
    "CustomerName": {
      "type": "string",
      "description": "Name of the customer to look up. e.g. John Smith"
    }
  },
  "required": ["CustomerName"]
}`}
                              className="font-mono h-[200px]"
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                    <FormField
                      control={form.control}
                      name="databaseDSN"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Database DSN</FormLabel>
                          <FormControl>
                            <Input {...field} placeholder="Enter your Database DSN" />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                    <FormField
                      control={form.control}
                      name="databaseQuery"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Database Query</FormLabel>
                          <FormControl>
                            <Input {...field} placeholder="Enter your database query" />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                    <div className="flex justify-between">
                      <Button type="submit" disabled={isSubmitting}>
                        {isSubmitting ? "Submitting..." : (toolId ? "Update" : "Create")}
                      </Button>
                      <Button
                        type="button"
                        variant="outline"
                        onClick={() => form.reset()}
                        disabled={isSubmitting}
                      >
                        Reset
                      </Button>
                    </div>
                  </form>
                </Form>
              </CardContent>
            </Card>
          </main>
        </div>
      </div>
    </div>
  )
}

export default ConnectToDB