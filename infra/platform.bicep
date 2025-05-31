@description('Azure region for resource deployment')
param location string

@description('Resource tags')
param tags object

@description('Azure AD Client ID')
param azureClientId string

@description('Azure AD Tenant ID')
param azureTenantId string

@minLength(1)
@maxLength(12)
@description('The name of the solution.')
param projectName string

@minLength(1)
@maxLength(4)
@description('The type of environment. e.g. local, dev, uat, prod.')
param environmentName string

@description('Resource prefix for naming resources')
param resourcePrefix string = toLower('${projectName}-${environmentName}')

@description('SKU for Storage Account')
param storageServiceSku object = {
  name: 'Standard_LRS'
}

@description('SKU for Azure Search Service')
param searchServiceSkuName string = 'standard'

@description('SKU for semantic search')
@allowed(['disabled', 'free', 'standard'])
param semanticSearchSku string = 'free'

@description('App Service Plan name')
param appServicePlanName string = toLower('${resourcePrefix}-app')

@description('Storage account name')
param storageAccountName string = replace(('${projectName}${environmentName}sto'), '-', '')

@description('Key Vault name')
param keyVaultName string = toLower('${resourcePrefix}2-kv')

@description('Azure Container Registry name')
param acrName string = toLower(replace('${resourcePrefix}acr', '-', ''))

@description('Log Analytics Workspace name')
param logAnalyticsWorkspaceName string = toLower('${resourcePrefix}-la')

@description('Cosmos DB account name')
param cosmosDbAccountName string = toLower('${resourcePrefix}-cosmos')

@description('Form Recognizer name')
param formRecognizerName string = toLower('${resourcePrefix}-form')

@description('Azure Search Service name')
param searchServiceName string = toLower('${resourcePrefix}-search')

@description('Service Bus namespace name')
param serviceBusName string = toLower('${resourcePrefix}-service-bus')

@description('Service Bus queue name')
param serviceBusQueueName string = toLower('${resourcePrefix}-folders-queue')

@description('Whether to deploy Azure OpenAI resources')
param deployAzueOpenAi bool = true

@description('OpenAI resource name')
param openAiName string = toLower('${resourcePrefix}-aillm')

@description('Azure OpenAI resource location')
param openAiLocation string

@description('SKU for Azure OpenAI resource')
param openAiSkuName string = 'S0'

@description('Cosmos DB custom role definition name')
param cosmosDbAccountDataPlaneCustomRoleName string = 'Custom Cosmos DB for NoSQL Data Plane Contributor'

@description('Database name for AgileChat')
param agileChatDatabaseName string = 'AgileChat'

var blobContainersArray = loadJsonContent('./blob-storage-containers.json')
var openAiSampleModelsArray = loadJsonContent('./openai-models.json')

var blobContainers = [
  for name in blobContainersArray: {
    name: toLower(replace(name, '-', ''))
    publicAccess: 'None'
  }
]

var openAiSampleModels = [
  for record in openAiSampleModelsArray: {
    name: record.name
    model: {
      name: record.model.name
      version: record.model.version
      format: record.model.format
    }
    sku: {
      name: record.sku.name
      capacity: record.sku.capacity
    }
  }
]

// @description('The optional APIM Gateway URL to override the azure open AI instance')
// param apimAiEndpointOverride string = ''
// @description('The optional APIM Gateway URL to override the azure open AI embedding instance')
// param apimAiEmbeddingsEndpointOverride string = '' 

module logAnalyticsWorkspaceModule './modules/logAnalyticsWorkspace.bicep' = {
  name: 'logAnalyticsWorkspaceModule'
  params: {
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceName
    location: location
    tags: tags
  }
}

module keyVaultModule './modules/keyVault.bicep' = {
  name: 'keyVaultModule'
  params: {
    name: keyVaultName
    location: location
    tags: tags
    logWorkspaceName: logAnalyticsWorkspaceName
    userObjectId: ''
    keyVaultSecrets: [
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

module acrModule './modules/acr.bicep' = {
  name: 'acrModule'
  params: {
    name: acrName
    location: location
    tags: tags
    logWorkspaceName: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceName
  }
}

module storageModule './modules/storage.bicep' = {
  name: 'storageModule'
  params: {
    name: storageAccountName
    location: location
    tags: tags
    logWorkspaceName: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceName
    skuName: storageServiceSku
    blobContainerCollection: blobContainers
  }
}

module aiSearchService './modules/aiSearch.bicep' = {
  name: 'aiSearchService'
  params: {
    name: searchServiceName
    location: location
    tags: tags
    keyVaultName: keyVaultModule.outputs.name
    searchServiceApiKeySecretName: 'AZURE-SEARCH-API-KEY' //TODO: Remove this secret after refactoring the API to use managed identity
    skuName: searchServiceSkuName
    semanticSearchSku: semanticSearchSku
  }
}

module appServicePlanModule './modules/appServicePlan.bicep' = {
  name: 'appServicePlanModule'
  params: {
    name: appServicePlanName
    location: location
    tags: tags
  }
}

module cosmosDbAccountModule './modules/cosmosDb.bicep' = {
  name: 'cosmosDbAccount'
  params: {
    name: cosmosDbAccountName
    location: location
    tags: tags
    keyVaultName: keyVaultModule.outputs.name
    cosmosDbAccountApiSecretName: 'AZURE-COSMOSDB-KEY' //TODO: Remove this secret after refactoring the API to use managed identity
    cosmosDbAccountDataPlaneCustomRoleName: cosmosDbAccountDataPlaneCustomRoleName
    databases: [
      agileChatDatabaseName
    ]
  }
}

module documentIntelligenceModule './modules/documentIntelligence.bicep' = {
  name: 'documentIntelligenceModule'
  params: {
    name: formRecognizerName
    location: location
    tags: tags
  }
}

module serviceBusModule './modules/serviceBus.bicep' = {
  name: 'serviceBusModule'
  params: {
    name: serviceBusName
    location: location
    tags: tags
    serviceBusQueueName: serviceBusQueueName
  }
}

module openAiModule './modules/openai.bicep' = {
  name: 'openAiModule'
  params: {
    name: openAiName
    location: openAiLocation
    tags: tags
    skuName: openAiSkuName
    deployments: deployAzueOpenAi ? openAiSampleModels : []
  }
}

output logAnalyticsWorkspaceName string = logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceName
output logAnalyticsWorkspaceId string = logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceId
output searchServiceName string = aiSearchService.outputs.name
output keyVaultName string = keyVaultModule.outputs.name
output keyVaultId string = keyVaultModule.outputs.resourceId
output storageAccountName string = storageModule.outputs.name
output storageAccountId string = storageModule.outputs.resourceId
output appServicePlanName string = appServicePlanModule.outputs.name
output appServicePlanId string = appServicePlanModule.outputs.resourceId
output cosmosDbAccountName string = cosmosDbAccountModule.outputs.name
output cosmosDbAccountEndpoint string = cosmosDbAccountModule.outputs.endpoint
output formRecognizerName string = documentIntelligenceModule.outputs.name
output serviceBusName string = serviceBusModule.outputs.name
output serviceBusQueueName string = serviceBusModule.outputs.serviceBusQueueName
output openAiName string = openAiModule.outputs.name
output openAiEndpoint string = openAiModule.outputs.endpoint
output cosmosDbAccountDataPlaneCustomRoleId string = cosmosDbAccountModule.outputs.cosmosDbAccountDataPlaneCustomRoleId
