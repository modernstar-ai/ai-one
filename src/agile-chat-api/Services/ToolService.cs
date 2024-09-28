public interface IToolService
{
    IEnumerable<Tool> GetAll();
    Tool? GetById(Guid id);
    void Create(Tool tool);
    void Update(Guid id, Tool tool);
    void Delete(Guid id);
}
public class ToolService : IToolService
{
    private readonly List<Tool> _tools = new();

    public ToolService()
    {
        _tools.Add(new Tool { Id = Guid.NewGuid(), Name = "Sample Tool 1", Description = "This is a sample tool" });
        _tools.Add(new Tool { Id = Guid.NewGuid(), Name = "Sample Tool 2", Description = "This is a sample tool" });
    }
    public IEnumerable<Tool> GetAll()
    {
        return _tools;
    }

    public Tool? GetById(Guid id)
    {
        return _tools.FirstOrDefault(t => t.Id == id);
    }

    public void Create(Tool tool)
    {
        _tools.Add(tool);
    }

    public void Update(Guid id, Tool tool)
    {
        var existingTool = GetById(id);
        if (existingTool != null)
        {
            existingTool.Name = tool.Name;
            existingTool.Description = tool.Description;
        }
    }

    public void Delete(Guid id)
    {
        var tool = GetById(id);
        if (tool != null)
        {
            _tools.Remove(tool);
        }
    }
}
