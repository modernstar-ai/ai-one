using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Agile.Framework.Common.Dtos;

public class QueryDto
{
    [DefaultValue(null)]
    public string? Search { get; set; }
    [DefaultValue(null)]
    public string? OrderBy { get; set; }
    [DefaultValue(null)]
    public OrderType? OrderType { get; set; }
    
    [DefaultValue(null)] 
    public int? Page { get; set; }
    [DefaultValue(null)]
    public int? PageSize { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderType
{
    ASC,
    DESC
}