using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;


public static class PersonaEndpoints
{
    public static void MapPersonaEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/personas", (IPersonaService personaService) =>
        {
            var personas = personaService.GetAll();
            return Results.Ok(personas);
        });

        app.MapGet("/personas/{id:guid}", (Guid id, IPersonaService personaService) =>
        {
            var persona = personaService.GetById(id);
            return persona != null ? Results.Ok(persona) : Results.NotFound();
        });

        app.MapPost("/personas", (Persona persona, IPersonaService personaService) =>
        {
            personaService.Create(persona);
            return Results.Created($"/personas/{persona.Id}", persona);
        });

        app.MapPut("/personas/{id:guid}", (Guid id, Persona updatedPersona, IPersonaService personaService) =>
        {
            var existingPersona = personaService.GetById(id);
            if (existingPersona is null)
            {
                return Results.NotFound();
            }

            personaService.Update(id, updatedPersona);
            return Results.NoContent();
        });

        app.MapDelete("/personas/{id:guid}", (Guid id, IPersonaService personaService) =>
        {
            var persona = personaService.GetById(id);
            if (persona is null)
            {
                return Results.NotFound();
            }

            personaService.Delete(id);
            return Results.NoContent();
        });
    }
}