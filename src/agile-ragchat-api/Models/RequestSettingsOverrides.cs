// Copyright (c) Microsoft. All rights reserved.

using Shared.Models;

namespace SharedWebComponents.Models;

public record RequestSettingsOverrides
{
    public Approach Approach { get; set; }
    public RequestOverrides Overrides { get; set; } = new();
}
