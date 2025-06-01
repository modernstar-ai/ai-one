targetScope = 'resourceGroup'

@description('The name of the solution.')
@minLength(1)
@maxLength(12)
param projectName string

@description('The type of environment. e.g. local, dev, uat, prod.')
@minLength(1)
@maxLength(4)
param environmentName string

@description('Primary location for all resources')
param location string = resourceGroup().location

// azure open ai -- regions currently support gpt-4o global-standard
@description('Location for the OpenAI resource group')
@allowed([
  'australiaeast'
  'brazilsouth'
  'canadaeast'
  'eastus'
  'eastus2'
  'francecentral'
  'germanywestcentral'
  'japaneast'
  'koreacentral'
  'northcentralus'
  'norwayeast'
  'polandcentral'
  'spaincentral'
  'southafricanorth'
  'southcentralus'
  'southindia'
  'swedencentral'
  'switzerlandnorth'
  'uksouth'
  'westeurope'
  'westus'
  'westus3'
])
@metadata({
  azd: {
    type: 'location'
  }
})
param openAILocation string

@description('Resource prefix for naming resources')
param resourcePrefix string = toLower('${projectName}-${environmentName}')

@description('Deployment Environment')
@allowed(['Development', 'Test', 'UAT', 'Production'])
param aspCoreEnvironment string = 'Development'

@description('ets options that control the availability of semantic search')
@allowed(['disabled', 'free', 'standard'])
param semanticSearchSku string = 'standard'

@description('AZURE_CLIENT_ID')
param azureClientID string

@description('AZURE_TENANT_ID')
param azureTenantId string

@description('API App name')
param apiAppName string = toLower('${resourcePrefix}-apiapp')

@description('APIM Azure OpenAI Endpoint')
param apimAiEndpointOverride string = ''

@description('APIM Azure OpenAI Embedding Endpoint')
param apimAiEmbeddingsEndpointOverride string = ''

@description('Shared variables pattern for loading tags')
var tagsFilePath = './tags.json'
var tags = loadJsonContent(tagsFilePath)

@description('Admin email addresses array')
param AdminEmailAddresses array = [
  'adam-stephensen@agile-analytics.com.au'
]

@description('SKU for Azure OpenAI resource')
param openAISku string = 'S0'

@description('API version for Azure OpenAI')
param openAIApiVersion string = '2024-08-01-preview'

@description('Database name for AgileChat')
param agileChatDatabaseName string = 'AgileChat'

var deployAzueOpenAi = (!empty(apimAiEndpointOverride) && empty(apimAiEmbeddingsEndpointOverride)) || (empty(apimAiEndpointOverride) && !empty(apimAiEmbeddingsEndpointOverride)) || (empty(apimAiEndpointOverride) && empty(apimAiEmbeddingsEndpointOverride))

module platform 'platform.bicep' = {
  name: 'platform'
  params: {
    projectName: projectName
    environmentName: environmentName
    location: location
    tags: tags
    resourcePrefix: resourcePrefix
    semanticSearchSku: semanticSearchSku
    azureClientId: azureClientID
    azureTenantId: azureTenantId
    openAiLocation: openAILocation
    openAiSkuName: openAISku
    deployAzueOpenAi: deployAzueOpenAi
    agileChatDatabaseName: agileChatDatabaseName
  }
}

module webApp 'webapp.bicep' = {
  name: 'webapp'
  params: {
    projectName: projectName
    environmentName: environmentName
    location: location
    tags: tags
    resourcePrefix: resourcePrefix
    appServicePlanName: platform.outputs.appServicePlanName
    apiAppName: apiAppName
    logAnalyticsWorkspaceName: platform.outputs.logAnalyticsWorkspaceName
  }
}

module apiApp 'apiapp.bicep' = {
  name: 'apiapp'
  params: {
    projectName: projectName
    environmentName: environmentName
    location: location
    tags: tags
    resourcePrefix: resourcePrefix
    apiAppName: apiAppName
    aspCoreEnvironment: aspCoreEnvironment
    azureTenantId: azureTenantId
    webAppDefaultHostName: webApp.outputs.webAppDefaultHostName
    storageName: platform.outputs.storageAccountName
    formRecognizerName: platform.outputs.formRecognizerName
    serviceBusQueueName: platform.outputs.serviceBusQueueName
    serviceBusName: platform.outputs.serviceBusName
    searchServiceName: platform.outputs.searchServiceName
    keyVaultName: platform.outputs.keyVaultName
    appServicePlanName: platform.outputs.appServicePlanName
    logAnalyticsWorkspaceName: platform.outputs.logAnalyticsWorkspaceName
    storageAccountName: platform.outputs.storageAccountName
    cosmosDbAccountName: platform.outputs.cosmosDbAccountName
    cosmosDbAccountEndpoint: platform.outputs.cosmosDbAccountEndpoint
    auditIncludePII: 'true'
    openAiApiVersion: openAIApiVersion
    openAiName: platform.outputs.openAiName
    apimAiEndpointOverride: apimAiEndpointOverride
    apimAiEmbeddingsEndpointOverride: apimAiEmbeddingsEndpointOverride
    adminEmailAddresses: AdminEmailAddresses
    cosmosDbAccountDataPlaneCustomRoleId: platform.outputs.cosmosDbAccountDataPlaneCustomRoleId
    agileChatDatabaseName: agileChatDatabaseName
  }
}

output url string = 'https://${webApp.outputs.webAppDefaultHostName}'
output api_url string = 'https://${apiApp.outputs.apiAppDefaultHostName}'
