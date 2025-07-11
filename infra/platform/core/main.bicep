targetScope = 'resourceGroup'

@description('Primary location for all resources')
param location string = resourceGroup().location

@description('The name of the solution.')
@minLength(3)
@maxLength(12)
param projectName string

@description('The type of environment. e.g. local, dev, uat, prod.')
@minLength(1)
@maxLength(4)
param environmentName string

@description('Tags to apply to all resources.')
param tags object = {}

@description('Resource prefix for naming resources')
param resourcePrefix string = toLower('${projectName}-${environmentName}')

@description('Azure AD Client ID')
param azureClientId string

@description('Azure AD Tenant ID')
param azureTenantId string

@description('SKU for Storage Account')
param storageServiceSku string = 'Standard_LRS'

@description('App Service Plan name')
param appServicePlanName string = toLower('${resourcePrefix}-app')

@description('Storage account name')
param storageAccountName string = replace(('${projectName}${environmentName}sto'), '-', '')

@description('Key Vault name')
param keyVaultName string = toLower('${resourcePrefix}-kv')

@description('Log Analytics Workspace ID')
param logAnalyticsWorkspaceResourceId string

@description('Whether to enable network isolation for resources')
param networkIsolation bool = false

@description('SKU for Azure Search Service')
param searchServiceSkuName string = 'standard'

@description('SKU for semantic search')
@allowed(['disabled', 'free', 'standard'])
param semanticSearchSku string = 'free'

@description('Cosmos DB account name')
param cosmosDbAccountName string = toLower('${resourcePrefix}-cosmos')

@description('Azure Search Service name')
param searchServiceName string = toLower('${resourcePrefix}-search')

@description('SKU for Service Bus')
param serviceBusSku string = 'Standard'

@description('Service Bus namespace name')
param serviceBusName string = toLower('${resourcePrefix}-service-bus')

param virtualNetworkName string = toLower('${resourcePrefix}-vnet')
param keyVaultSubnetName string = 'KeyVaultSubnet'
param storageSubnetName string = 'StorageSubnet'
param cosmosDbSubnetName string = 'CosmosDbSubnet'
param aiSearchSubnetName string = 'AiSearchSubnet'
param serviceBusSubnetName string = 'ServiceBusSubnet'

resource vnet 'Microsoft.Network/virtualNetworks@2023-05-01' existing = if (networkIsolation) {
  name: virtualNetworkName
}

var virtualNetworkResourceId = networkIsolation ? vnet.id : ''
var keyVaultSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${keyVaultSubnetName}' : ''
var storageSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${storageSubnetName}' : ''
var cosmosDbSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${cosmosDbSubnetName}' : ''
var aiSearchSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${aiSearchSubnetName}' : ''
var serviceBusSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${serviceBusSubnetName}' : ''

module keyVaultModule '../../modules/keyVault.bicep' = {
  name: 'keyVaultModule'
  params: {
    name: keyVaultName
    location: location
    tags: tags
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceResourceId
    networkIsolation: networkIsolation
    virtualNetworkResourceId: virtualNetworkResourceId
    virtualNetworkSubnetResourceId: keyVaultSubnetResourceId
    secrets: [
      {
        name: 'AZURE-CLIENT-ID'
        contentType: 'text/plain'
        value: azureClientId
      }
      {
        name: 'AZURE-TENANT-ID'
        contentType: 'text/plain'
        value: azureTenantId
      }
    ]
  }
}

module storageModule '../../modules/storage.bicep' = {
  name: 'storageModule'
  params: {
    name: storageAccountName
    location: location
    tags: tags
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceResourceId
    networkIsolation: networkIsolation
    virtualNetworkResourceId: virtualNetworkResourceId
    virtualNetworkSubnetResourceId: storageSubnetResourceId
    skuName: storageServiceSku
  }
}

module appServicePlanModule '../../modules/appServicePlan.bicep' = {
  name: 'appServicePlanModule'
  params: {
    name: appServicePlanName
    location: location
    tags: tags
  }
}

module aiSearchService '../../modules/aiSearch.bicep' = {
  name: 'aiSearchService'
  params: {
    name: searchServiceName
    location: location
    tags: tags
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceResourceId
    networkIsolation: networkIsolation
    virtualNetworkResourceId: virtualNetworkResourceId
    virtualNetworkSubnetResourceId: aiSearchSubnetResourceId
    keyVaultName: keyVaultName
    searchServiceApiKeySecretName: 'AZURE-SEARCH-API-KEY'
    skuName: searchServiceSkuName
    semanticSearchSku: semanticSearchSku
  }
}

module cosmosDbAccountModule '../../modules/cosmosDb.bicep' = {
  name: 'cosmosDbAccount'
  params: {
    name: cosmosDbAccountName
    location: location
    tags: tags
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceResourceId
    networkIsolation: networkIsolation
    virtualNetworkResourceId: virtualNetworkResourceId
    virtualNetworkSubnetResourceId: cosmosDbSubnetResourceId
    keyVaultName: keyVaultName
    cosmosDbAccountApiSecretName: 'AZURE-COSMOSDB-KEY'
  }
}

module serviceBusModule '../../modules/serviceBus.bicep' = {
  name: 'serviceBusModule'
  params: {
    name: serviceBusName
    location: location
    tags: tags
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceResourceId
    networkIsolation: networkIsolation
    virtualNetworkResourceId: virtualNetworkResourceId
    virtualNetworkSubnetResourceId: serviceBusSubnetResourceId
    sku: serviceBusSku
  }
}

output keyVaultName string = keyVaultModule.outputs.name
output keyVaultId string = keyVaultModule.outputs.resourceId
output storageAccountName string = storageModule.outputs.name
output storageAccountId string = storageModule.outputs.resourceId
output appServicePlanName string = appServicePlanModule.outputs.name
output appServicePlanId string = appServicePlanModule.outputs.resourceId
output searchServiceName string = aiSearchService.outputs.name
output cosmosDbAccountName string = cosmosDbAccountModule.outputs.name
output cosmosDbAccountEndpoint string = cosmosDbAccountModule.outputs.endpoint
output serviceBusName string = serviceBusModule.outputs.name
