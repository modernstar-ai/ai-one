// Copyright (c) Microsoft. All rights reserved.

namespace MinimalApi.Extensions;

internal static class ConfigurationExtensions
{
    private static string? GetEnvVar(string key) => Environment.GetEnvironmentVariable(key);

    internal static string GetStorageAccountEndpoint(this IConfiguration config)
    {
        var endpoint = GetEnvVar("AZURE_STORAGE_ACCOUNT_ENDPOINT");
        ArgumentNullException.ThrowIfNullOrEmpty(endpoint);

        return endpoint;
    }

    internal static string ToCitationBaseUrl(this IConfiguration config)
    {
        var endpoint = config.GetStorageAccountEndpoint();

        var builder = new UriBuilder(endpoint)
        {
            Path = GetEnvVar("AZURE_STORAGE_ACCOUNT_CONTAINER")
        };

        return builder.Uri.AbsoluteUri;
    }
}
