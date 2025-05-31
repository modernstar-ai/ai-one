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

resource aiSearch 'Microsoft.Search/searchServices@2024-06-01-preview' = {
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

output name string = aiSearch.name
output resourceId string = aiSearch.id
