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

@description('Resource prefix for naming resources')
param resourcePrefix string = toLower('${projectName}-${environmentName}')

@description('Azure AD Client ID')
param azureClientId string

@description('Azure AD Tenant ID')
param azureTenantId string

@description('SKU for Storage Account')
param storageServiceSku string = 'Standard_LRS'

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
param keyVaultName string = toLower('${resourcePrefix}-kv')

@description('Azure Container Registry name')
param acrName string = toLower(replace('${resourcePrefix}acr', '-', ''))

@description('Log Analytics Workspace name')
param logAnalyticsWorkspaceName string = toLower('${resourcePrefix}-la')

@description('Application Insights name')
param applicationInsightsName string = toLower('${resourcePrefix}-platform')

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspaceModule.outputs.resourceId
  }
}

@description('Cosmos DB account name')
param cosmosDbAccountName string = toLower('${resourcePrefix}-cosmos')

@description('Form Recognizer name')
param formRecognizerName string = toLower('${resourcePrefix}-form')

@description('Azure Search Service name')
param searchServiceName string = toLower('${resourcePrefix}-search')

@description('Service Bus namespace name')
param serviceBusName string = toLower('${resourcePrefix}-service-bus')

@description('Whether to deploy Azure OpenAI resources')
param deployAzureOpenAi bool = true

@description('OpenAI resource name')
param openAiName string = toLower('${resourcePrefix}-aillm')

@description('Flag to control deployment of AI Foundry resources')
param deployAIFoundryResources bool = true

@description('Name of the AI Foundry Account.')
param aiFoundryAccountName string = toLower('${resourcePrefix}-foundry')

@description('AI Foundry Project name')
param aiFoundryProjectName string = toLower('${resourcePrefix}-prj')

@description('Azure OpenAI resource location')
param openAILocation string

@description('SKU for Azure OpenAI resource')
param openAiSkuName string = 'S0'

@description('Whether to enable network isolation for resources')
param networkIsolation bool = false

param virtualNetworkName string = toLower('${resourcePrefix}-vnet')

param keyVaultSubnetName string = 'VmSubnet'
param storageSubnetName string = 'VmSubnet'
param cosmosDbSubnetName string = 'VmSubnet'
param aiSearchSubnetName string = 'VmSubnet'
param serviceBusSubnetName string = 'VmSubnet'
param formRecognizerSubnetName string = 'VmSubnet'
param acrSubnetName string = 'VmSubnet'
param openAiSubnetName string = 'VmSubnet'
param cognitiveServiceSubnetName string = 'VmSubnet'

@description('Shared variables pattern for loading tags')
var tagsFilePath = '../tags.json'
var tags = loadJsonContent(tagsFilePath)

@description('Flag to control deployment of OpenAI models')
param deployOpenAiModels bool = false

var openAiModelsArray = loadJsonContent('../openai-models.json')

#disable-next-line no-unused-vars
var openAiSampleModels = [
  for record in openAiModelsArray: {
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

resource vnet 'Microsoft.Network/virtualNetworks@2023-05-01' existing = if (networkIsolation) {
  name: virtualNetworkName
}

var virtualNetworkResourceId = networkIsolation ? vnet.id : ''
var keyVaultSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${keyVaultSubnetName}' : ''
var storageSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${storageSubnetName}' : ''
var cosmosDbSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${cosmosDbSubnetName}' : ''
var aiSearchSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${aiSearchSubnetName}' : ''
var serviceBusSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${serviceBusSubnetName}' : ''
var formRecognizerSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${formRecognizerSubnetName}' : ''
var acrSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${acrSubnetName}' : ''
var openAiSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${openAiSubnetName}' : ''
var cognitiveServiceSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${cognitiveServiceSubnetName}' : ''

module logAnalyticsWorkspaceModule '../modules/logAnalyticsWorkspace.bicep' = {
  name: 'logAnalyticsWorkspaceModule'
  params: {
    name: logAnalyticsWorkspaceName
    location: location
    tags: tags
  }
}

module keyVaultModule '../modules/keyVault.bicep' = {
  name: 'keyVaultModule'
  params: {
    name: keyVaultName
    location: location
    tags: tags
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceModule.outputs.resourceId
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

module acrModule '../modules/acr.bicep' = {
  name: 'acrModule'
  params: {
    name: acrName
    location: location
    tags: tags
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceModule.outputs.resourceId
    networkIsolation: networkIsolation
    virtualNetworkResourceId: virtualNetworkResourceId
    virtualNetworkSubnetResourceId: acrSubnetResourceId
  }
}

module storageModule '../modules/storage.bicep' = {
  name: 'storageModule'
  params: {
    name: storageAccountName
    location: location
    tags: tags
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceModule.outputs.resourceId
    networkIsolation: networkIsolation
    virtualNetworkResourceId: virtualNetworkResourceId
    virtualNetworkSubnetResourceId: storageSubnetResourceId
    skuName: storageServiceSku
  }
}

module aiSearchService '../modules/aiSearch.bicep' = {
  name: 'aiSearchService'
  params: {
    name: searchServiceName
    location: location
    tags: tags
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceModule.outputs.resourceId
    networkIsolation: networkIsolation
    virtualNetworkResourceId: virtualNetworkResourceId
    virtualNetworkSubnetResourceId: aiSearchSubnetResourceId
    keyVaultName: keyVaultModule.outputs.name
    searchServiceApiKeySecretName: 'AZURE-SEARCH-API-KEY' //TODO: Remove this secret after refactoring the API to use managed identity
    skuName: searchServiceSkuName
    semanticSearchSku: semanticSearchSku
  }
}

module appServicePlanModule '../modules/appServicePlan.bicep' = {
  name: 'appServicePlanModule'
  params: {
    name: appServicePlanName
    location: location
    tags: tags
  }
}

module cosmosDbAccountModule '../modules/cosmosDb.bicep' = {
  name: 'cosmosDbAccount'
  params: {
    name: cosmosDbAccountName
    location: location
    tags: tags
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceModule.outputs.resourceId
    networkIsolation: networkIsolation
    virtualNetworkResourceId: virtualNetworkResourceId
    virtualNetworkSubnetResourceId: cosmosDbSubnetResourceId
    keyVaultName: keyVaultModule.outputs.name
    cosmosDbAccountApiSecretName: 'AZURE-COSMOSDB-KEY' //TODO: Remove this secret after refactoring the API to use managed identity
  }
}

module documentIntelligenceModule '../modules/documentIntelligence.bicep' = {
  name: 'documentIntelligenceModule'
  params: {
    name: formRecognizerName
    location: location
    tags: tags
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceModule.outputs.resourceId
    virtualNetworkSubnetResourceId: formRecognizerSubnetResourceId
    networkIsolation: networkIsolation
    virtualNetworkResourceId: virtualNetworkResourceId
  }
}

module serviceBusModule '../modules/serviceBus.bicep' = {
  name: 'serviceBusModule'
  params: {
    name: serviceBusName
    location: location
    tags: tags
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceModule.outputs.resourceId
    networkIsolation: networkIsolation
    virtualNetworkResourceId: virtualNetworkResourceId
    virtualNetworkSubnetResourceId: serviceBusSubnetResourceId
  }
}

module openAiModule '../modules/openai.bicep' = if (deployAzureOpenAi) {
  name: 'openAiModule'
  params: {
    name: openAiName
    location: openAILocation
    tags: tags
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceModule.outputs.resourceId
    networkIsolation: networkIsolation
    virtualNetworkResourceId: virtualNetworkResourceId
    virtualNetworkSubnetResourceId: openAiSubnetResourceId
    skuName: openAiSkuName
    deployments: deployOpenAiModels ? openAiSampleModels : []
  }
}

module aiFoundryProject '../modules/aifoundryProject.bicep' = if (deployAIFoundryResources) {
  name: aiFoundryProjectName
  params: {
    name: aiFoundryProjectName
    location: location
    tags: tags
    aiFoundryAccountName: aiFoundryAccountName
    cosmosDbEnabled: true
    searchEnabled: true
    cosmosDBname: cosmosDbAccountName
    searchServiceName: aiSearchService.outputs.name
    storageName: storageModule.outputs.name
  }
}

module cognitiveServices '../modules/cognitive-services/main.bicep' = if (deployAIFoundryResources) {
  name: '${projectName}-cognitive-services-deployment'
  dependsOn: [
    aiFoundryProject
  ]
  params: {
    location: location
    tags: tags
    aiServicesName: openAiName
    aiServiceLocation: openAILocation
    resourcePrefix: resourcePrefix
    networkIsolation: networkIsolation
    languageEnabled: false
    visionEnabled: false
    contentSafetyEnabled: false
    speechEnabled: false
    // networkAcls: networkAcls
    virtualNetworkResourceId: virtualNetworkResourceId
    virtualNetworkSubnetResourceId: cognitiveServiceSubnetResourceId
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceModule.outputs.resourceId
  }
}

module modelDeployments '../modules/modelDeployments.bicep' = if (deployOpenAiModels) {
  name: 'modelDeployments'
  dependsOn: [
    cognitiveServices
  ]
  params: {
    aiFoundryAccountName: aiFoundryAccountName
    aiModelDeployments: openAiSampleModels
  }
}

output logAnalyticsWorkspaceName string = logAnalyticsWorkspaceModule.outputs.name
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
output openAiName string = deployAIFoundryResources
  ? cognitiveServices.outputs.aiServicesName
  : openAiModule.outputs.name
output openAiEndpoint string = deployAIFoundryResources
  ? cognitiveServices.outputs.aiServicesEndpoint
  : openAiModule.outputs.endpoint
