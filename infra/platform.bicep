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

@description('The container name where files will be stored for folder search')
param storageServiceFoldersContainerName string = 'index-content'

@description('The container name for images')
param storageServiceImageContainerName string = 'images'

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

@description('Capacity for ChatGPT deployment')
param chatGptDeploymentCapacity int = 8

@description('ChatGPT deployment name')
param chatGptDeploymentName string = 'gpt-4o'

@description('ChatGPT model name')
param chatGptModelName string = 'gpt-4o'

@description('ChatGPT model version')
param chatGptModelVersion string = '2024-05-13'

@description('Embedding deployment name')
param embeddingDeploymentName string = 'embedding'

@description('Capacity for embedding deployment')
param embeddingDeploymentCapacity int = 350

@description('Embedding model name')
param embeddingModelName string = 'text-embedding-3-small'

@description('Cosmos DB custom role definition name')
param cosmosDbAccountDataPlaneCustomRoleName string = 'Custom Cosmos DB for NoSQL Data Plane Contributor'

@description('Database name for AgileChat')
param agileChatDatabaseName string = 'AgileChat'

// @description('The optional APIM Gateway URL to override the azure open AI instance')
// param apimAiEndpointOverride string = ''
// @description('The optional APIM Gateway URL to override the azure open AI embedding instance')
// param apimAiEmbeddingsEndpointOverride string = ''

var validStorageServiceImageContainerName = toLower(replace(storageServiceImageContainerName, '-', ''))

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsWorkspaceName
  tags: tags
  location: location
}

resource searchService 'Microsoft.Search/searchServices@2024-06-01-preview' = {
  name: searchServiceName
  location: location
  tags: tags
  properties: {
    partitionCount: 1
    publicNetworkAccess: 'enabled'
    replicaCount: 1
    semanticSearch: semanticSearchSku
  }
  sku: {
    name: searchServiceSkuName
  }
  identity: {
    type: 'SystemAssigned'
  }
}

// resource kv 'Microsoft.KeyVault/vaults@2024-11-01' = {
//   name: keyVaultName
//   location: location
//   tags: tags
//   properties: {
//     sku: {
//       family: 'A'
//       name: 'standard'
//     }
//     tenantId: subscription().tenantId
//     enableRbacAuthorization: true
//     enabledForDeployment: false
//     enabledForDiskEncryption: true
//     enabledForTemplateDeployment: false
//   }

//   resource AZURE_SEARCH_API_KEY 'secrets' = {
//     name: 'AZURE-SEARCH-API-KEY'
//     properties: {
//       contentType: 'text/plain'
//       value: searchService.listAdminKeys().secondaryKey
//     }
//   }

//   resource AZURE_CLIENT_ID 'secrets' = {
//     name: 'AZURE-CLIENT-ID'
//     properties: {
//       contentType: 'text/plain'
//       value: azureClientId
//     }
//   }

//   resource AZURE_TENANT_ID 'secrets' = {
//     name: 'AZURE-TENANT-ID'
//     properties: {
//       contentType: 'text/plain'
//       value: azureTenantId
//     }
//   }

//   resource AZURE_COSMOSDB_KEY 'secrets' = {
//     name: 'AZURE-COSMOSDB-KEY'
//     properties: {
//       contentType: 'text/plain'
//       value: cosmosDbAccount.listKeys().secondaryMasterKey
//     }
//   }
// }

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
        name: 'AZURE-SEARCH-API-KEY'
        contentType: 'text/plain'
        value: listAdminKeys(searchServiceName, '2024-06-01-preview').secondaryKey
      }
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
      {
        //TODO: Remove this secret after refactoring the API to use managed identity
        name: 'AZURE-COSMOSDB-KEY'
        contentType: 'text/plain'
        value: listKeys(cosmosDbAccountName, '2023-04-15').secondaryMasterKey
      }
    ]
  }
}


resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: storageServiceSku
}

resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storage
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

resource imagesContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobServices
  name: validStorageServiceImageContainerName
}

resource indexContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobServices
  name: storageServiceFoldersContainerName
  properties: {
    publicAccess: 'None'
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  properties: {
    reserved: true
  }
  sku: {
    name: 'P0v3'
    tier: 'Premium0V3'
    size: 'P0v3'
    family: 'Pv3'
    capacity: 1
  }
  kind: 'linux'
}

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: cosmosDbAccountName
  location: location
  tags: tags
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: location
        failoverPriority: 0
      }
    ]
  }
}

resource formRecognizer 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: formRecognizerName
  location: location
  tags: tags
  kind: 'FormRecognizer'
  properties: {
    customSubDomainName: formRecognizerName
    publicNetworkAccess: 'Enabled'
  }
  sku: {
    name: 'S0'
  }
}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  location: location
  name: serviceBusName

  resource queue 'queues' = {
    name: serviceBusQueueName
    properties: {
      maxMessageSizeInKilobytes: 256
      lockDuration: 'PT5M'
      maxSizeInMegabytes: 5120
      requiresDuplicateDetection: false
      requiresSession: false
      defaultMessageTimeToLive: 'P14D'
      deadLetteringOnMessageExpiration: true
      enableBatchedOperations: true
      duplicateDetectionHistoryTimeWindow: 'PT10M'
      maxDeliveryCount: 5
      status: 'Active'
      autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
      enablePartitioning: false
      enableExpress: false
    }
  }
}

resource azureopenai 'Microsoft.CognitiveServices/accounts@2023-05-01' = if (deployAzueOpenAi) {
  name: openAiName
  location: openAiLocation
  tags: tags
  kind: 'OpenAI'
  properties: {
    customSubDomainName: openAiName
    publicNetworkAccess: 'Enabled'
  }
  sku: {
    name: openAiSkuName
  }
}

resource gptllmdeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = if (deployAzueOpenAi) {
  parent: azureopenai
  name: chatGptDeploymentName
  properties: {
    model: {
      format: 'OpenAI'
      name: chatGptModelName
      version: chatGptModelVersion
    }
  }
  #disable-next-line use-safe-access
  sku: {
    name: 'GlobalStandard'
    capacity: chatGptDeploymentCapacity
  }
}

resource embeddingsllmdeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = if (deployAzueOpenAi) {
  parent: azureopenai
  dependsOn: [gptllmdeployment]
  name: embeddingDeploymentName
  properties: {
    model: {
      format: 'OpenAI'
      name: embeddingModelName
      version: '1'
    }
  }
  sku: {
    name: 'Standard'
    capacity: embeddingDeploymentCapacity
  }
}

// Cosmos DB Custom Role Definition
//https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/how-to-grant-data-plane-access?tabs=built-in-definition%2Ccsharp&pivots=azure-interface-bicep
resource cosmosDbAccountDataPlaneCustomRole 'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions@2024-05-15' = {
  name: guid(resourceGroup().id, cosmosDbAccount.id, cosmosDbAccountDataPlaneCustomRoleName)
  parent: cosmosDbAccount
  properties: {
    roleName: cosmosDbAccountDataPlaneCustomRoleName
    type: 'CustomRole'
    assignableScopes: [
      cosmosDbAccount.id
    ]
    permissions: [
      {
        dataActions: [
          'Microsoft.DocumentDB/databaseAccounts/readMetadata'
          'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/*'
          'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/*'
        ]
      }
    ]
  }
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2022-05-15' = {
  name: agileChatDatabaseName
  parent: cosmosDbAccount
  properties: {
    resource: {
      id: agileChatDatabaseName
    }
  }
}

output logAnalyticsWorkspaceName string = logAnalyticsWorkspace.name
output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.id
output searchServiceName string = searchService.name
output searchServiceId string = searchService.id
output keyVaultName string = kv.name
output keyVaultId string = kv.id
output storageAccountName string = storage.name
output storageAccountId string = storage.id
output blobServicesId string = blobServices.id
output imagesContainerId string = imagesContainer.id
output indexContainerId string = indexContainer.id
output appServicePlanName string = appServicePlan.name
output appServicePlanId string = appServicePlan.id
output cosmosDbAccountName string = cosmosDbAccount.name
output cosmosDbAccountId string = cosmosDbAccount.id
output cosmosDbAccountEndpoint string = cosmosDbAccount.properties.documentEndpoint
output formRecognizerName string = formRecognizer.name
output formRecognizerId string = formRecognizer.id
output serviceBusName string = serviceBus.name
output serviceBusId string = serviceBus.id
output serviceBusQueueName string = serviceBus::queue.name
output serviceBusQueueId string = serviceBus::queue.id
output storageServiceFoldersContainerName string = indexContainer.name
output storageServiceImageContainerName string = imagesContainer.name
output openAiName string = azureopenai.name
output openAiEndpoint string = azureopenai.properties.endpoint
output cosmosDbAccountDataPlaneCustomRoleId string = cosmosDbAccountDataPlaneCustomRole.id
output agileChatDatabaseName string = database.name
