import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { ScrollArea } from "@/components/ui/scroll-area";
import { PlusCircle, Trash2 } from "lucide-react";
import LeftMenu from '@/components/LeftMenu'
import SimpleHeading from '@/components/Heading-Simple';
import { useEffect, useState } from "react";
import { fetchPersonas, addPersona, updatePersona, deletePersona } from "@/services/personaservice";
import PersonaForm from '@/pages/Personas/PersonaForm';
import { Persona } from "@/types/Persona";

export default function PersonaManager() {
  const [personas, setPersonas] = useState<Persona[]>([]);
  const [selectedPersona, setSelectedPersona] = useState<Persona | null>(null);
  const [isAddingNew, setIsAddingNew] = useState(false);
  

  useEffect(() => {
    async function loadPersonas() {
      const personasData = await fetchPersonas();
      if (personasData) {
        setPersonas(personasData);
      }
    }
    loadPersonas();
  }, []);

  const handleAddPersona = async (newPersona: Omit<Persona, "id">) => {
    const persona = await addPersona(newPersona);
    if (persona) {
      setPersonas([...personas, persona]);
      setSelectedPersona(persona);
    }
    setIsAddingNew(false);
  };

  const handleUpdatePersona = async (updatedPersona: Persona) => {
    const updated = await updatePersona(updatedPersona);
    if (updated) {
      setPersonas(personas.map(p => p.id === updated.id ? updated : p));
      setSelectedPersona(updated);
    }
  };

  const handleDeletePersona = async (id: string) => {
    const success = await deletePersona(id);
    if (success) {
      setPersonas(personas.filter(p => p.id !== id));
      if (selectedPersona?.id === id) {
        setSelectedPersona(null);
      }
    }
  };

  return (
    <div className="flex h-screen bg-background text-foreground">
      {/* Left Sidebar */}
      <LeftMenu />

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col">
        {/* Header */}
        <SimpleHeading Title="Personas" Subtitle="Manage your personas" DocumentCount={personas.length} />

        {/* Main Content */}
        <div className="flex-1 p-4 flex">
          {/* Personas List */}
          <div className="w-1/4 pr-4 border-r">
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

          {/* Persona Form */}
          <div className="w-3/4 pl-4">
            {(selectedPersona || isAddingNew) && (
              <Card>
                <CardHeader>
                  <CardTitle>{isAddingNew ? "Add New Persona" : "Edit Persona"}</CardTitle>
                </CardHeader>
                <CardContent>
                  <PersonaForm
                    initialPersona={isAddingNew ? null : selectedPersona}
                    onSubmit={async (persona) => {
                      if (isAddingNew) {
                        await handleAddPersona(persona as Omit<Persona, "id">);
                      } else {
                        await handleUpdatePersona(persona as Persona);
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
      </div>
    </div>
  );
}
