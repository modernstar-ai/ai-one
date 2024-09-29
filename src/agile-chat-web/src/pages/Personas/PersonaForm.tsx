import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Persona } from "@/types/Persona";

interface PersonaFormProps {
  initialPersona: Persona | null;
  onSubmit: (persona: Persona | Omit<Persona, "id">) => void;
}

const PersonaForm: React.FC<PersonaFormProps> = ({ initialPersona, onSubmit }) => {
  const [name, setName] = useState(initialPersona?.name || "");
  const [greeting, setGreeting] = useState(initialPersona?.greeting || "");
  const [systemmessage, setSystemmessage] = useState(initialPersona?.systemmessage || "");

  useEffect(() => {
    if (initialPersona) {
      setName(initialPersona.name);
      setGreeting(initialPersona.greeting);
      setSystemmessage(initialPersona.systemmessage);
    } else {
      setName("");
      setGreeting("");
      setSystemmessage("");
    }
  }, [initialPersona]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(initialPersona ? { ...initialPersona, name, greeting, systemmessage } : { name, greeting, systemmessage });
  };

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
  );
};

export default PersonaForm;
