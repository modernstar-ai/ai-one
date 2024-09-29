public class Persona
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;
    public string Greeting { get; set; } = string.Empty;
    public string SystemMessage { get; set; } = string.Empty;
}