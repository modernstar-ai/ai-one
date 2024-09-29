
using System;
using System.Collections.Generic;
using System.Linq;


public interface IPersonaService
{
    IEnumerable<Persona> GetAll();
    Persona? GetById(Guid id);
    void Create(Persona persona);
    void Update(Guid id, Persona persona);
    void Delete(Guid id);
}



public class PersonaService : IPersonaService
{
    private readonly List<Persona> _personas = new();

    public PersonaService()
    {
        _personas.Add(new Persona { Id = Guid.NewGuid(), Name = "New User", Greeting = "Welcome to our platform!", SystemMessage = "You are a helpful assistant welcoming new users." });
        _personas.Add(new Persona { Id = Guid.NewGuid(), Name = "Support Agent", Greeting = "How can I assist you today?", SystemMessage = "You are a knowledgeable support agent helping users with their queries." });
    }

    public IEnumerable<Persona> GetAll()
    {
        return _personas;
    }

    public Persona? GetById(Guid id)
    {
        return _personas.FirstOrDefault(p => p.Id == id);
    }

    public void Create(Persona persona)
    {
        persona.Id = Guid.NewGuid();
        _personas.Add(persona);
    }

    public void Update(Guid id, Persona updatedPersona)
    {
        var persona = _personas.FirstOrDefault(p => p.Id == id);
        if (persona != null)
        {
            persona.Name = updatedPersona.Name;
            persona.Greeting = updatedPersona.Greeting;
            persona.SystemMessage = updatedPersona.SystemMessage;
        }
    }

    public void Delete(Guid id)
    {
        var persona = _personas.FirstOrDefault(p => p.Id == id);
        if (persona != null)
        {
            _personas.Remove(persona);
        }
    }
}