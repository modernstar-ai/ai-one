import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { ScrollArea } from "@/components/ui/scroll-area"
import { PlusCircle, Trash2 } from "lucide-react"

interface Persona {
  id: string
  name: string
  greeting: string
  systemmessage: string
}

export default function PersonaManager() {
  const [personas, setPersonas] = useState<Persona[]>([
    { id: "1", name: "New User", greeting: "Welcome to our platform!", systemmessage: "You are a helpful assistant welcoming new users." },
    { id: "2", name: "Support Agent", greeting: "How can I assist you today?", systemmessage: "You are a knowledgeable support agent helping users with their queries." },
  ])
  const [selectedPersona, setSelectedPersona] = useState<Persona | null>(null)
  const [isAddingNew, setIsAddingNew] = useState(false)

  const handleAddPersona = (newPersona: Omit<Persona, "id">) => {
    const persona = { ...newPersona, id: crypto.randomUUID() }
    setPersonas([...personas, persona])
    setSelectedPersona(persona)
    setIsAddingNew(false)
  }

  const handleUpdatePersona = (updatedPersona: Persona) => {
    setPersonas(personas.map(p => p.id === updatedPersona.id ? updatedPersona : p))
    setSelectedPersona(updatedPersona)
  }

  const handleDeletePersona = (id: string) => {
    setPersonas(personas.filter(p => p.id !== id))
    if (selectedPersona?.id === id) {
      setSelectedPersona(null)
    }
  }

  return (
    <div className="container mx-auto p-4 h-screen flex">
      
      <div className="w-1/4 pr-4 border-r">
        <h2 className="text-xl font-bold mb-4">Personas</h2>
        <ScrollArea className="h-[calc(100vh-8rem)]">
          <div className="space-y-2">
            {personas.map(persona => (
              <Button
                key={persona.id}
                variant={selectedPersona?.id === persona.id ? "secondary" : "ghost"}
                className="w-full justify-start"
                onClick={() => setSelectedPersona(persona)}
              >
                {persona.name}
              </Button>
            ))}
          </div>
        </ScrollArea>
        <Button className="w-full mt-4" onClick={() => { setIsAddingNew(true); setSelectedPersona(null); }}>
          <PlusCircle className="mr-2 h-4 w-4" /> Add New Persona
        </Button>
      </div>
      <div className="w-3/4 pl-4">
        {(selectedPersona || isAddingNew) && (
          <Card>
            <CardHeader>
              <CardTitle>{isAddingNew ? "Add New Persona" : "Edit Persona"}</CardTitle>
            </CardHeader>
            <CardContent>
              <PersonaForm
                initialPersona={isAddingNew ? null : selectedPersona}
                onSubmit={(persona) => {
                  if (isAddingNew) {
                    handleAddPersona(persona as Omit<Persona, "id">);
                  } else {
                    handleUpdatePersona(persona as Persona);
                  }
                }}
              />
            </CardContent>
            {!isAddingNew && (
              <CardFooter className="justify-between">
                <Button variant="destructive" onClick={() => handleDeletePersona(selectedPersona!.id)}>
                  <Trash2 className="mr-2 h-4 w-4" /> Delete Persona
                </Button>
              </CardFooter>
            )}
          </Card>
        )}
        {!selectedPersona && !isAddingNew && (
          <div className="h-full flex items-center justify-center">
            <p className="text-muted-foreground">Select a persona to edit or create a new one.</p>
          </div>
        )}
      </div>
    </div>
  )
}

interface PersonaFormProps {
  initialPersona: Persona | null
  onSubmit: (persona: Persona | Omit<Persona, "id">) => void
}

function PersonaForm({ initialPersona, onSubmit }: PersonaFormProps) {
  const [name, setName] = useState(initialPersona?.name || "")
  const [greeting, setGreeting] = useState(initialPersona?.greeting || "")
  const [systemmessage, setSystemmessage] = useState(initialPersona?.systemmessage || "")

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    onSubmit(initialPersona ? { ...initialPersona, name, greeting, systemmessage } : { name, greeting, systemmessage })
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
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
        <Input
          id="greeting"
          value={greeting}
          onChange={(e) => setGreeting(e.target.value)}
          required
        />
      </div>
      <div className="space-y-2">
        <Label htmlFor="systemmessage">System Message</Label>
        <Textarea
          id="systemmessage"
          value={systemmessage}
          onChange={(e) => setSystemmessage(e.target.value)}
          className="min-h-[200px]"
          required
        />
      </div>
      <Button type="submit">{initialPersona ? "Update Persona" : "Add Persona"}</Button>
    </form>
  )
}