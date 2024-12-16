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
    [DefaultValue(0)] 
    public int Page { get; set; } = 0;
    [DefaultValue(10)]
    public int PageSize { get; set; } = 10;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderType
{
    ASC,
    DESC
}