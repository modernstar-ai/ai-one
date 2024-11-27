targetScope = 'resourceGroup'

@minLength(1)
@maxLength(9)
@description('The name of the solution.')
param projectName string

@minLength(1)
@maxLength(4)
@description('The type of environment. e.g. local, dev, uat, prod.')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
//param location string
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

param openAISku string = 'S0'
param openAIApiVersion string = '2024-08-01-preview'
param chatGptDeploymentCapacity int = 8 //30
param chatGptDeploymentName string = 'gpt-4o'
param chatGptModelName string = 'gpt-4o'
param chatGptModelVersion string = '2024-05-13'
param embeddingDeploymentName string = 'embedding'
param embeddingDeploymentCapacity int = 120
param embeddingModelName string = 'text-embedding-ada-002'

// DALL-E v3 only supported in limited regions for now
@description('Location for the OpenAI DALL-E 3 instance resource group')
@allowed(['swedencentral', 'eastus', 'australiaeast'])
param dalleLocation string

param dalleDeploymentCapacity int = 1
param dalleDeploymentName string = 'dall-e-3'
param dalleModelName string = 'dall-e-3'
param dalleApiVersion string = '2023-12-01-preview'

param formRecognizerSkuName string = 'S0'
param searchServiceIndexName string = 'azure-chat'
param searchServiceSkuName string = 'standard'

// TODO: define good default Sku and settings for storage account
param storageServiceSku object = { name: 'Standard_LRS' }
param storageServiceImageContainerName string = 'images'

@description('Deployment Environment')
@allowed(['Development', 'Test', 'UAT', 'Production'])
param aspCoreEnvironment string = 'Development'

@description('AZURE_CLIENT_ID')
@secure()
param azureClientID string = ''

@description('AZURE_CLIENT_SECRET')
@secure()
param azureClientSecret string = ''

@description('AZURE_TENANT_ID')
@secure()
param azureTenantId string = ''

@description('The Azure Active Directory Application ID or URI to get the access token that will be included as the bearer token in delivery requests')
param azureADAppIdOrUri string = ''

//other
var tags = { 'azd-env-name': environmentName }

@description('UTS Role Endpoint')
param UtsRoleApiEndpoint string = ''

//Load tags from the file
// var tagsFilePath = './uts.tags.json'
// var tags = loadJsonContent(tagsFilePath)

module resources 'resources.bicep' = {
  name: 'all-resources'
  params: {
    projectName: projectName
    environmentName: environmentName
    tags: union(tags, { 'azd-env-name': environmentName })
    openai_api_version: openAIApiVersion
    openAiLocation: openAILocation
    openAiSkuName: openAISku
    chatGptDeploymentCapacity: chatGptDeploymentCapacity
    chatGptDeploymentName: chatGptDeploymentName
    chatGptModelName: chatGptModelName
    chatGptModelVersion: chatGptModelVersion
    embeddingDeploymentName: embeddingDeploymentName
    embeddingDeploymentCapacity: embeddingDeploymentCapacity
    embeddingModelName: embeddingModelName
    dalleLocation: dalleLocation
    dalleDeploymentCapacity: dalleDeploymentCapacity
    dalleDeploymentName: dalleDeploymentName
    dalleModelName: dalleModelName
    dalleApiVersion: dalleApiVersion
    formRecognizerSkuName: formRecognizerSkuName
    searchServiceIndexName: searchServiceIndexName
    searchServiceSkuName: searchServiceSkuName
    storageServiceSku: storageServiceSku
    storageServiceImageContainerName: storageServiceImageContainerName
    location: location
    aspCoreEnvironment: aspCoreEnvironment
    azureClientID: azureClientID
    azureClientSecret: azureClientSecret
    azureTenantId: azureTenantId
    azureADAppIdOrUri: azureADAppIdOrUri
    UtsRoleApiEndpoint: UtsRoleApiEndpoint
  }
}

output APP_URL string = resources.outputs.url
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
