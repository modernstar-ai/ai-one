@description('Name of the Azure Container Registry instance.')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('The name of the workload\'s existing Log Analytics workspace.')
param logWorkspaceName string
resource logWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: logWorkspaceName
}

resource acrResource 'Microsoft.ContainerRegistry/registries@2024-11-01-preview' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: 'Standard'
  }
  properties: {
    // adminUserEnabled: false
    // dataEndpointEnabled: false
    // networkRuleBypassOptions: 'AzureServices' // This allows support for ACR tasks to push the build image and bypass network restrictions - https://learn.microsoft.com/en-us/azure/container-registry/allow-access-trusted-services#trusted-services-workflow
    // networkRuleSet: {
    //   defaultAction: 'Deny'
    //   ipRules: []
    // }
    // policies: {
    //   exportPolicy: {
    //     status: 'disabled'
    //   }
    //   azureADAuthenticationAsArmPolicy: {
    //     status: 'disabled'
    //   }
    // }
    // publicNetworkAccess: 'Disabled'
    // zoneRedundancy: 'Enabled'
    // metadataSearch: 'Disabled'
  }
}

@description('Diagnostic settings for the Azure Container Registry instance.')
resource acrResourceDiagSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'default'
  scope: acrResource
  properties: {
    workspaceId: logWorkspace.id
    logs: [
      {
        category: 'ContainerRegistryRepositoryEvents'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'ContainerRegistryLoginEvents'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
    logAnalyticsDestinationType: null
  }
}

output acrName string = acrResource.name
