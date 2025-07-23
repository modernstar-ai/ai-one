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
  cognitiveServiceSubnet: {
    name: 'CognitiveServiceSubnet'
    addressPrefix: '10.3.9.0/24'
  }
  appServiceSubnet: {
    name: 'AppServiceSubnet'
    addressPrefix: '10.3.8.0/24'
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
param logAnalyticsWorkspaceResourceId = '/subscriptions/a8d44b38-6c3d-459c-b6c5-8bc173941b44/resourcegroups/ms-rg-aione_dev1-ae/providers/microsoft.operationalinsights/workspaces/ms-aione-dev-la'
