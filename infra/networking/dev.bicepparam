using './main.bicep'

param environmentName = 'dev'
var sharedVariables = loadJsonContent('../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../tags.json')

param resourcePrefix = toLower('${projectName}-${environmentName}')

param vnetConfig = {
  name: toLower('${resourcePrefix}-vnet')
  addressPrefixes: '10.0.0.0/8'
  vmSubnet: {
    name: 'VmSubnet'
    addressPrefix: '10.3.1.0/24'
  }
  keyVaultSubnet: {
    name: 'KeyVaultSubnet'
    addressPrefix: '10.3.3.0/24'
  }
  storageSubnet: {
    name: 'StorageSubnet'
    addressPrefix: '10.3.4.0/24'
  }
  cosmosDbSubnet: {
    name: 'CosmosDbSubnet'
    addressPrefix: '10.3.5.0/24'
  }
  aiSearchSubnet: {
    name: 'AiSearchSubnet'
    addressPrefix: '10.3.6.0/24'
  }
  serviceBusSubnet: {
    name: 'ServiceBusSubnet'
    addressPrefix: '10.3.7.0/24'
  }
  appServiceSubnet: {
    name: 'AppServiceSubnet'
    addressPrefix: '10.3.8.0/24'
  }
  cognitiveServiceSubnet: {
    name: 'CognitiveServiceSubnet'
    addressPrefix: '10.3.9.0/24'
  }
  privateEndpointSubnet: {
    name: 'PrivateEndpointsSubnet'
    addressPrefix: '10.3.10.0/24'
  }
}

param nsgConfig = {
  vmNsgName: '${resourcePrefix}-vm-nsg'
  keyVaultNsgName: '${resourcePrefix}-keyvault-nsg'
  storageNsgName: '${resourcePrefix}-storage-nsg'
  cosmosDbNsgName: '${resourcePrefix}-cosmosdb-nsg'
  aiSearchNsgName: '${resourcePrefix}-aisearch-nsg'
  serviceBusNsgName: '${resourcePrefix}-servicebus-nsg'
  cognitiveServiceNsgName: '${resourcePrefix}-cognitive-nsg'
  appServiceNsgName: '${resourcePrefix}-appservice-nsg'
  allowedIpAddress: '' // Leave empty to allow all IPs, or specify a specific IP
}

// Log Analytics workspace configuration for diagnostics
param enableDiagnostics = false
param logAnalyticsWorkspaceResourceId = '/subscriptions/9221a966-ce17-4b76-a348-887f234a827a/resourcegroups/rg-practice-ai-aione-dev/providers/microsoft.operationalinsights/workspaces/ag-aione-dev-la'
