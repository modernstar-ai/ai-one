@description('Name of the AI Search resource.')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('SKU for Azure Search Service')
param skuName string = 'standard'

@description('SKU for semantic search')
@allowed(['disabled', 'free', 'standard'])
param semanticSearchSku string = 'free'

@description('Key Vault name for storing secrets related to AI Search.')
param keyVaultName string

@description('Key Vault secret name for AI Search API Key')
param searchServiceApiKeySecretName string = 'searchServiceApiKey'

resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' existing = {
  name: keyVaultName
}

resource searchService 'Microsoft.Search/searchServices@2024-06-01-preview' = {
  name: name
  location: location
  tags: tags
  properties: {
    partitionCount: 1
    publicNetworkAccess: 'enabled'
    replicaCount: 1
    semanticSearch: semanticSearchSku
  }
  sku: {
    name: skuName
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource searchServiceApiKey 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  name: searchServiceApiKeySecretName
  parent: keyVault
  properties: {
    value: 'test'
    //value: searchService.properties.primaryKey
    contentType: 'text/plain'
  }
}

output name string = searchService.name
output resourceId string = searchService.id
