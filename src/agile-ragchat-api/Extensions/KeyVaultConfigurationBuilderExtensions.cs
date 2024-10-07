// Copyright (c) Microsoft. All rights reserved.

namespace MinimalApi.Extensions;

internal static class KeyVaultConfigurationBuilderExtensions
{
    internal static IConfigurationBuilder ConfigureAzureKeyVault(this IConfigurationBuilder builder)
    {
        var keyVaultEndpointValue = Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_ENDPOINT");
        var azureKeyVaultEndpoint = keyVaultEndpointValue ?? throw new InvalidOperationException("Azure Key Vault endpoint is not set.");
        ArgumentNullException.ThrowIfNullOrEmpty(azureKeyVaultEndpoint);
        var defaultCredential = new DefaultAzureCredential();

        //todo:adam - removed to resolve error
        //builder.AddAzureKeyVault(
        //    new Uri(azureKeyVaultEndpoint), defaultCredential);

        return builder;
    }
}
