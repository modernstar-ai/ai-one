@description('Name of the AI Foundry Project.')
param name string

@description('Specifies the location for all the Azure resources. Defaults to the location of the resource group.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('Name of the AI Foundry Account.')
param aiFoundryAccountName string

@description('AI Foundry SKU.')
param aiFoundrySku string = 'S0'

@description('Name of the customers existing CosmosDB Resource')
param cosmosDBname string

@description('Whether to include Cosmos DB in the deployment.')
param cosmosDbEnabled bool

@description('Name of the customers existing Azure Storage Account')
param storageName string

@description('Whether to include Azure AI Search in the deployment.')
param searchEnabled bool

@description('Azure Search Service Name')
param searchServiceName string

@description('Name of the first project')
param defaultProjectName string = name
param defaultProjectDisplayName string = name

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageName
}

resource aiSearchService 'Microsoft.Search/searchServices@2024-06-01-preview' existing = if (searchEnabled) {
  name: searchServiceName
}

resource cosmosDBAccount 'Microsoft.DocumentDB/databaseAccounts@2025-05-01-preview' existing = if (cosmosDbEnabled) {
  name: cosmosDBname
}

resource foundryAccount 'Microsoft.CognitiveServices/accounts@2025-04-01-preview' = {
  name: aiFoundryAccountName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  sku: {
    name: aiFoundrySku
  }
  kind: 'AIServices'
  properties: {
    allowProjectManagement: true
    customSubDomainName: aiFoundryAccountName
    disableLocalAuth: false
    publicNetworkAccess: 'Enabled'
  }
}

resource project 'Microsoft.CognitiveServices/accounts/projects@2025-04-01-preview' = {
  name: defaultProjectName
  parent: foundryAccount
  tags: tags
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    displayName: defaultProjectDisplayName
  }
}

resource project_connection_azure_storage 'Microsoft.CognitiveServices/accounts/projects/connections@2025-04-01-preview' = {
  name: storageName
  parent: project
  properties: {
    category: 'AzureBlob'
    target: storageAccount.properties.primaryEndpoints.blob
    authType: 'AAD'
    metadata: {
      ApiType: 'Azure'
      ResourceId: storageAccount.id
      location: storageAccount.location
      accountName: storageAccount.name
      containerName: '${name}proj'
    }
  }
}

resource project_connection_azureai_search 'Microsoft.CognitiveServices/accounts/projects/connections@2025-04-01-preview' = if (searchEnabled) {
  name: searchEnabled ? aiSearchService.name : ''
  parent: project
  properties: {
    category: 'CognitiveSearch'
    target: searchEnabled ? 'https://${aiSearchService.name}.search.windows.net/' : ''
    authType: 'AAD'
    isSharedToAll: true
    metadata: {
      ApiType: 'Azure'
      ResourceId: searchEnabled ? aiSearchService.id : ''
      location: searchEnabled ? aiSearchService.location : ''
    }
  }
}

resource project_connection_cosmosdb 'Microsoft.CognitiveServices/accounts/projects/connections@2025-04-01-preview' = if (cosmosDbEnabled) {
  name: cosmosDBname
  parent: project
  properties: {
    category: 'CosmosDB'
    target: cosmosDbEnabled ? cosmosDBAccount.properties.documentEndpoint : ''
    authType: 'AAD'
    metadata: {
      ApiType: 'Azure'
      ResourceId: cosmosDbEnabled ? cosmosDBAccount.id : ''
      location: cosmosDbEnabled ? cosmosDBAccount.location : ''
    }
  }
}

output projectId string = project.id
output projectName string = project.name
