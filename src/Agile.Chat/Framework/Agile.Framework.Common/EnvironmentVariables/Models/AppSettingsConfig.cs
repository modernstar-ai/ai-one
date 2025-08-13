namespace Agile.Framework.Common.EnvironmentVariables.Models;

using System.Text.Json.Serialization;
using static Agile.Framework.Common.EnvironmentVariables.Constants;

public class AppSettingsConfig
{
    public string AppName { get; set; }
    public string AiDisclaimer { get; set; }
    public string LogoUrl { get; set; }
    public string FaviconUrl { get; set; }
    public bool ModelSelectionFeatureEnabled { get; set; } = true;
    public bool AllowModelSelectionDefaultValue { get; set; } = true;
    public string DefaultTextModelId { get; set; } = TextModels.Gpt4o;
    public FilePreviewType FilePreviewType { get; set; } = FilePreviewType.Preview;
    public bool BingDeployed { get; set; } = false;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FilePreviewType
{
    None,
    Preview,
    Download
}