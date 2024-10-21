// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace SharedWebComponents.Models;

public record class SharedCultures
{
    [JsonPropertyName("translation")]
    public required IDictionary<string, AzureCulture> AvailableCultures { get; set; }
}
