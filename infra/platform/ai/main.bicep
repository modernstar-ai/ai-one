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

@description('Document Intelligence service name')
param documentIntelligenceServiceName string = toLower('${resourcePrefix}-docintel')

@description('OpenAI resource name')
param openAiName string = toLower('${resourcePrefix}-aillm')

@description('AI Services resource name if deploying AI Foundry resources')
param aiFoundryServicesName string = toLower('${resourcePrefix}-foundry')

@description('Flag to control deployment of AI Foundry resources')
param deployAIFoundryResources bool = true

@description('AI Foundry Project name')
param aiFoundryProjectName string = toLower('${resourcePrefix}-prj')

@description('Azure OpenAI resource location')
param openAILocation string

@description('SKU for Azure OpenAI resource')
param openAiSkuName string = 'S0'

@description('Whether to enable network isolation for resources')
param networkIsolation bool = false

param virtualNetworkName string = toLower('${resourcePrefix}-vnet')
param openAiSubnetName string = 'OpenAiSubnet'
param cognitiveServiceSubnetName string = 'CognitiveServiceSubnet'

@description('Flag to control deployment of OpenAI models')
param deployOpenAiModels bool = false

@description('Log Analytics Workspace ID')
param logAnalyticsWorkspaceResourceId string

@description('Cosmos DB account name from data deployment')
param cosmosDbAccountName string = toLower('${resourcePrefix}-cosmos')

@description('Search service name from data deployment')
param searchServiceName string = toLower('${resourcePrefix}-search')

@description('Storage account name from core deployment')
param storageAccountName string = replace(('${projectName}${environmentName}sto'), '-', '')

var openAiModelsArray = loadJsonContent('../../openai-models.json')

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

resource vnet 'Microsoft.Network/virtualNetworks@2023-05-01' existing = if (networkIsolation) {
  name: virtualNetworkName
}

var virtualNetworkResourceId = networkIsolation ? vnet.id : ''
var openAiSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${openAiSubnetName}' : ''
var cognitiveServiceSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${cognitiveServiceSubnetName}' : ''

module documentIntelligenceModule '../../modules/documentIntelligence.bicep' = if (!deployAIFoundryResources) {
  name: 'documentIntelligenceModule'
  params: {
    name: documentIntelligenceServiceName
    location: location
    tags: tags
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceResourceId
    virtualNetworkSubnetResourceId: cognitiveServiceSubnetResourceId
    networkIsolation: networkIsolation
    virtualNetworkResourceId: virtualNetworkResourceId
  }
}

module openAiModule '../../modules/openai.bicep' = if (!deployAIFoundryResources) {
  name: 'openAiModule'
  params: {
    name: openAiName
    location: openAILocation
    tags: tags
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceResourceId
    networkIsolation: networkIsolation
    virtualNetworkResourceId: virtualNetworkResourceId
    virtualNetworkSubnetResourceId: openAiSubnetResourceId
    skuName: openAiSkuName
    deployments: deployOpenAiModels ? openAiSampleModels : []
  }
}

module aiFoundryProject '../../modules/aifoundryProject.bicep' = if (deployAIFoundryResources) {
  name: aiFoundryProjectName
  params: {
    name: aiFoundryProjectName
    location: location
    tags: tags
    aiFoundryServicesName: cognitiveServices.outputs.aiFoundryServicesName
    cosmosDbEnabled: true
    searchEnabled: true
    cosmosDBname: cosmosDbAccountName
    searchServiceName: searchServiceName
    storageName: storageAccountName
  }
}

module cognitiveServices '../../modules/cognitive-services/main.bicep' = if (deployAIFoundryResources) {
  name: '${projectName}-cognitive-services-deployment'
  params: {
    location: location
    tags: tags
    aiFoundryServicesName: aiFoundryServicesName
    aiServiceLocation: openAILocation
    resourcePrefix: resourcePrefix
    networkIsolation: networkIsolation
    documentIntelligenceServiceEnabled: true
    documentIntelligenceServiceName: documentIntelligenceServiceName
    languageEnabled: false
    visionEnabled: false
    contentSafetyEnabled: false
    speechEnabled: false
    virtualNetworkResourceId: virtualNetworkResourceId
    virtualNetworkSubnetResourceId: cognitiveServiceSubnetResourceId
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceResourceId
  }
}

module modelDeployments '../../modules/modelDeployments.bicep' = if (deployOpenAiModels) {
  name: 'modelDeployments'
  dependsOn: [
    cognitiveServices
  ]
  params: {
    aiFoundryServicesName: cognitiveServices.outputs.aiFoundryServicesName
    aiModelDeployments: openAiSampleModels
  }
}

output openAiName string = deployAIFoundryResources
  ? cognitiveServices.outputs.aiFoundryServicesName
  : openAiModule.outputs.name
output openAiEndpoint string = deployAIFoundryResources
  ? cognitiveServices.outputs.aiServicesEndpoint
  : openAiModule.outputs.endpoint
