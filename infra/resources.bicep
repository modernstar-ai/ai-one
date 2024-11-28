//parms
param projectName string = 'agilechat'
param environmentName string = 'dev'
param location string = resourceGroup().location
param tags object = {}

//calculate names
var resourcePrefix = toLower('${projectName}-${environmentName}')
var appservice_name = toLower('${resourcePrefix}-app')
var webapp_name = toLower('${resourcePrefix}-webapp')
var apiapp_name = toLower('${resourcePrefix}-apiapp')
var applicationInsightsName = toLower('${resourcePrefix}-apiapp')

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

param openai_api_version string

param openAiLocation string
param openAiSkuName string
param chatGptDeploymentCapacity int
param chatGptDeploymentName string
param chatGptModelName string
param chatGptModelVersion string
param embeddingDeploymentName string
param embeddingDeploymentCapacity int
param embeddingModelName string

//param OpenaiApiEmbeddingsModelName string = 'text-embedding-ada-002'
param AdminEmailAddress string = 'adam-stephensen@agile-analytics.com.au'
param AzureCosmosdbDbName string = 'chat'
param AzureCosmosdbDatabaseName string = 'chat'
param AzureCosmosdbContainerName string = 'history'
param AzureCosmosdbConfigContainerName string = 'config'
param AzureCosmosdbToolsContainerName string = 'tools'
param AzureCosmosdbFilesContainerName string = 'fileUploads'
param AzureSearchIndexNameRag string = 'rag_index'
param MaxUploadDocumentSize string = '20000000'
param AzureStorageFoldersContainerName string = 'folders'

// param dalleLocation string
// param dalleDeploymentCapacity int
// param dalleDeploymentName string
// param dalleModelName string
// param dalleApiVersion string

// param speechServiceSkuName string = 'S0'

// param formRecognizerSkuName string = 'S0'

param searchServiceSkuName string = 'standard'
param searchServiceIndexName string = 'azure-chat'

param storageServiceSku object
param storageServiceImageContainerName string

var openai_name = toLower('${resourcePrefix}-aillm')
//var openai_dalle_name = toLower('${resourcePrefix}-aidalle')

@description('Cosmos DB Chat threads container name')
param azureCosmosDbChatThreadsName string = 'history'

// var form_recognizer_name = toLower('${resourcePrefix}-form')
// var speech_service_name = toLower('${resourcePrefix}-speech')
var cosmos_name = toLower('${resourcePrefix}-cosmos')
var search_name = toLower('${resourcePrefix}-search')
// storage name must be < 24 chars, alphanumeric only. 'sto' is 3 and resourceToken is 13
var clean_name = replace(replace('${resourcePrefix}', '-', ''), '_', '')
var storage_prefix = take(clean_name, 13)
var storage_name = toLower('${storage_prefix}sto')
// keyvault name must be less than 24 chars - token is 13
//var kv_prefix = take(projectName, 7)
//var keyVaultName = toLower('${kv_prefix}${environmentName}-kv')
//todo: apply toLower to resourcePrefix and ensure it is small enough to fit all resources.
var keyVaultName = toLower('${resourcePrefix}-kv')
var la_workspace_name = toLower('${resourcePrefix}-la')
var diagnostic_setting_name = 'AppServiceConsoleLogs'

var keyVaultSecretsOfficerRole = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
)

var validStorageServiceImageContainerName = toLower(replace(storageServiceImageContainerName, '-', ''))

//Event Grid config
@description('The container name where files will be stored for folder search')
param storageServiceFoldersContainerName string = 'index-content'

@description('The Azure Active Directory Application ID or URI to get the access token that will be included as the bearer token in delivery requests')
param azureADAppIdOrUri string = ''

@description('Event Grid System Topic Name')
var EventGridSystemTopicName = toLower('${resourcePrefix}-folders-listener')

@description('Event Grid Subscription')
var EventGridSystemTopicSubName = toLower('${resourcePrefix}-folders-blobs-listener')

var databaseName = 'chat'
var historyContainerName = 'history'
var configContainerName = 'config'

@description('UTS Role Endpoint')
param UtsRoleApiEndpoint string = ''

@description('AI Services  Name')
var aiServices_name = toLower('${projectName}${environmentName}-ai-services')

var llmDeployments = [
  {
    name: chatGptDeploymentName
    model: {
      format: 'OpenAI'
      name: chatGptModelName
      version: chatGptModelVersion
    }
    sku: {
      name: 'GlobalStandard'
      capacity: chatGptDeploymentCapacity
    }
  }
  {
    name: embeddingDeploymentName
    model: {
      format: 'OpenAI'
      name: embeddingModelName
      version: '2'
    }
    capacity: embeddingDeploymentCapacity
  }
]

/* **************************************************** */

resource appServicePlan 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: appservice_name
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

resource webApp 'Microsoft.Web/sites@2020-06-01' = {
  name: webapp_name
  location: location
  tags: union(tags, { 'azd-service-name': 'agilechat-web' })
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      linuxFxVersion: 'node|18-lts'
      alwaysOn: true
      appCommandLine: 'npx serve -s dist'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'

      appSettings: [
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'false'
        }
        {
          name: 'ALLOWED_ORIGINS'
          value: 'https://${apiapp_name}.azurewebsites.net'
        }
      ]
    }
  }
  identity: { type: 'SystemAssigned' }

  resource configLogs 'config' = {
    name: 'logs'
    properties: {
      applicationLogs: { fileSystem: { level: 'Verbose' } }
      detailedErrorMessages: { enabled: true }
      failedRequestsTracing: { enabled: true }
      httpLogs: { fileSystem: { enabled: true, retentionInDays: 1, retentionInMb: 35 } }
    }
  }
}

resource apiApp 'Microsoft.Web/sites@2020-06-01' = {
  name: apiapp_name
  location: location
  tags: union(tags, { 'azd-service-name': 'agilechat-api' })
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
      //appCommandLine: 'dotnet agile-chat-api.dll'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'

      cors: {
        allowedOrigins: [
          'https://${webApp.properties.defaultHostName}'
        ]
        supportCredentials: true
      }
      defaultDocuments: [
        'string'
      ]

      appSettings: [
        {
          name: 'UTS_ROLE_API_ENDPOINT'
          value: UtsRoleApiEndpoint
        }
        {
          name: 'AZURE_STORAGE_FOLDERS_CONTAINER_NAME'
          value: AzureStorageFoldersContainerName
        }
        {
          name: 'AZURE_STORAGE_ACCOUNT_CONNECTION'
          value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_STORAGE_ACCOUNT_CONNECTION.name})'
        }
        {
          name: 'MAX_UPLOAD_DOCUMENT_SIZE'
          value: MaxUploadDocumentSize
        }
        {
          name: 'AZURE_SEARCH_ENDPOINT'
          value: 'https://${search_name}.search.windows.net'
        }
        {
          name: 'AZURE_SEARCH_INDEX_NAME_RAG'
          value: AzureSearchIndexNameRag
        }
        // {
        //   name: 'AZURE_OPENAI_API_EMBEDDINGS_DEPLOYMENT_NAME'
        //   value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_OPENAI_API_EMBEDDINGS_DEPLOYMENT_NAME.name})'
        // }
        {
          name: 'ADMIN_EMAIL_ADDRESS'
          value: AdminEmailAddress
        }
        {
          name: 'AZURE_COSMOSDB_DB_NAME'
          value: AzureCosmosdbDbName
        }
        {
          name: 'AZURE_COSMOSDB_DATABASE_NAME'
          value: AzureCosmosdbDatabaseName
        }
        {
          name: 'AZURE_COSMOSDB_CONTAINER_NAME'
          value: AzureCosmosdbContainerName
        }
        {
          name: 'AZURE_COSMOSDB_CONFIG_CONTAINER_NAME'
          value: AzureCosmosdbConfigContainerName
        }
        {
          name: 'AZURE_COSMOSDB_TOOLS_CONTAINER_NAME'
          value: AzureCosmosdbToolsContainerName
        }
        {
          name: 'AZURE_COSMOSDB_FILES_CONTAINER_NAME'
          value: AzureCosmosdbFilesContainerName
        }
        {
          name: 'AZURE_AI_SERVICES_KEY'
          value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_AI_SERVICES_KEY.name})'
        }
        {
          name: 'ALLOWED_ORIGINS'
          value: 'https://${webApp.properties.defaultHostName}'
        }
        {
          name: 'AZURE_COSMOSDB_CHAT_THREADS_CONTAINER_NAME'
          value: azureCosmosDbChatThreadsName
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsights.properties.InstrumentationKey
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_CLIENT_ID.name})'
        }
        {
          name: 'AZURE_CLIENT_SECRET'
          value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_CLIENT_SECRET.name})'
        }
        {
          name: 'AZURE_TENANT_ID'
          value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_TENANT_ID.name})'
        }
        {
          name: 'AZURE_KEY_VAULT_NAME'
          value: keyVaultName
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: aspCoreEnvironment
        }
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'false'
        }
        {
          name: 'AZURE_OPENAI_API_KEY'
          value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_OPENAI_API_KEY.name})'
        }
        {
          name: 'AZURE_OPENAI_API_INSTANCE_NAME'
          value: openai_name
        }
        {
          name: 'AZURE_OPENAI_API_DEPLOYMENT_NAME'
          value: chatGptDeploymentName
        }
        {
          name: 'AZURE_OPENAI_API_EMBEDDINGS_DEPLOYMENT_NAME'
          value: embeddingDeploymentName
        }
        {
          name: 'AZURE_OPENAI_API_VERSION'
          value: openai_api_version
        }
        {
          name: 'AZURE_OPENAI_ENDPOINT'
          value: azureopenai.properties.endpoint
        }
        {
          name: 'AZURE_COSMOSDB_URI'
          value: cosmosDbAccount.properties.documentEndpoint
        }
        {
          name: 'AZURE_COSMOSDB_KEY'
          value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_COSMOSDB_KEY.name})'
        }
        {
          name: 'AZURE_SEARCH_API_KEY'
          value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_SEARCH_API_KEY.name})'
        }
        {
          name: 'AZURE_SEARCH_NAME'
          value: search_name
        }
        {
          name: 'AZURE_SEARCH_INDEX_NAME'
          value: searchServiceIndexName
        }
        {
          name: 'AZURE_STORAGE_ACCOUNT_NAME'
          value: storage_name
        }
        {
          name: 'AZURE_STORAGE_ACCOUNT_KEY'
          value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_STORAGE_ACCOUNT_KEY.name})'
        }
      ]
    }
  }
  identity: { type: 'SystemAssigned' }
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: la_workspace_name
  tags: tags
  location: location
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

resource webDiagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: diagnostic_setting_name
  scope: webApp
  properties: {
    workspaceId: logAnalyticsWorkspace.id
    logs: [
      {
        category: 'AppServiceConsoleLogs'
        enabled: true
      }
    ]
    metrics: []
  }
}

//**************************************************************************
//Add Role Assignment for web app to Key vault

@description('The name of the Role Assignment - from Guid.')
param roleAssignmentName string = newGuid()

resource kvFunctionAppPermissions 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: roleAssignmentName
  scope: kv
  properties: {
    principalId: apiApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: keyVaultSecretsOfficerRole
  }
}

//**************************************************************************

resource kv 'Microsoft.KeyVault/vaults@2021-06-01-preview' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enabledForDeployment: false
    enabledForDiskEncryption: true
    enabledForTemplateDeployment: false
  }

  resource AZURE_OPENAI_API_KEY 'secrets' = {
    name: 'AZURE-OPENAI-API-KEY'
    properties: {
      contentType: 'text/plain'
      value: azureopenai.listKeys().key1
    }
  }

  resource AZURE_COSMOSDB_KEY 'secrets' = {
    name: 'AZURE-COSMOSDB-KEY'
    properties: {
      contentType: 'text/plain'
      value: cosmosDbAccount.listKeys().secondaryMasterKey
    }
  }

  resource AZURE_SEARCH_API_KEY 'secrets' = {
    name: 'AZURE-SEARCH-API-KEY'
    properties: {
      contentType: 'text/plain'
      value: searchService.listAdminKeys().secondaryKey
    }
  }

  resource AZURE_STORAGE_ACCOUNT_KEY 'secrets' = {
    name: 'AZURE-STORAGE-ACCOUNT-KEY'
    properties: {
      contentType: 'text/plain'
      value: storage.listKeys().keys[0].value
    }
  }

  resource AZURE_STORAGE_ACCOUNT_CONNECTION 'secrets' = {
    name: 'AZURE-STORAGE-ACCOUNT-CONNECTION'
    properties: {
      contentType: 'text/plain'
      value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};AccountKey=${listKeys(storage.id, '2023-01-01').keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
    }
  }

  resource AZURE_AI_SERVICES_KEY 'secrets' = {
    name: 'AZURE-AI-SERVICES-KEY'
    properties: {
      contentType: 'text/plain'
      value: aiServices.listKeys().key1
    }
  }

  resource AZURE_CLIENT_ID 'secrets' = {
    name: 'AZURE-CLIENT-ID'
    properties: {
      contentType: 'text/plain'
      value: azureClientID
    }
  }

  resource AZURE_CLIENT_SECRET 'secrets' = {
    name: 'AZURE-CLIENT-SECRET'
    properties: {
      contentType: 'text/plain'
      value: azureClientSecret
    }
  }

  resource AZURE_TENANT_ID 'secrets' = {
    name: 'AZURE-TENANT-ID'
    properties: {
      contentType: 'text/plain'
      value: azureTenantId
    }
  }
}

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: cosmos_name
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

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2022-05-15' = {
  name: databaseName
  parent: cosmosDbAccount
  properties: {
    resource: {
      id: databaseName
    }
  }
}

resource historyContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = {
  name: historyContainerName
  parent: database
  properties: {
    resource: {
      id: historyContainerName
      partitionKey: {
        paths: [
          '/userId'
        ]
        kind: 'Hash'
      }
    }
  }
}

resource configContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = {
  name: configContainerName
  parent: database
  properties: {
    resource: {
      id: configContainerName
      partitionKey: {
        paths: [
          '/userId'
        ]
        kind: 'Hash'
      }
    }
  }
}

resource searchService 'Microsoft.Search/searchServices@2022-09-01' = {
  name: search_name
  location: location
  tags: tags
  properties: {
    partitionCount: 1
    publicNetworkAccess: 'enabled'
    replicaCount: 1
  }
  sku: {
    name: searchServiceSkuName
  }
  identity: { type: 'SystemAssigned' }
}

resource azureopenai 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: openai_name
  location: openAiLocation
  tags: tags
  kind: 'OpenAI'
  properties: {
    customSubDomainName: openai_name
    publicNetworkAccess: 'Enabled'
  }
  sku: {
    name: openAiSkuName
  }
}

@batchSize(1)
resource llmdeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = [
  for deployment in llmDeployments: {
    parent: azureopenai
    name: deployment.name
    properties: {
      model: deployment.model
      #disable-next-line use-safe-access BCP053
      raiPolicyName: contains(deployment, 'raiPolicyName') ? deployment.?raiPolicyName : null
    }
    #disable-next-line use-safe-access
    sku: contains(deployment, 'sku')
      ? deployment.sku
      : {
          name: 'Standard'
          capacity: deployment.capacity
        }
  }
]

// ChatGptDeployment
// resource chatGptDeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
//   name: chatGptDeploymentName
//   sku: {
//     name: 'GlobalStandard'
//     capacity: chatGptDeploymentCapacity
//   }
//   parent: azureopenai
//   properties: {
//     model: {
//       format: 'OpenAI'
//       name: chatGptModelName
//       version: chatGptModelVersion
//     }
//   }
// }

// embeddingDeployment
// resource embeddingDeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
//   name: embeddingModelName
//   parent: azureopenai
//   sku: {
//     name: 'Standard'
//     capacity: embeddingDeploymentCapacity
//   }
//   properties: {
//     model: {
//       format: 'OpenAI'
//       name: embeddingModelName
//       version: '2'
//     }
//   }
// }

@description('Storage Account')
resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storage_name
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
    publicAccess: 'Blob'
  }
}

@description('Role Assignment for search to access blob storage')
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, searchService.id, 'blob-reader')
  scope: storage
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1'
    )
    principalId: searchService.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

@description('Event Grid System Topic')
resource eventGridSystemTopic 'Microsoft.EventGrid/systemTopics@2024-06-01-preview' = {
  name: EventGridSystemTopicName
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    source: storage.id
    topicType: 'Microsoft.Storage.StorageAccounts'
  }
}

@description('Event Grid Subscription')
resource eventGrid 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2024-06-01-preview' = {
  name: EventGridSystemTopicSubName
  parent: eventGridSystemTopic
  properties: {
    destination: {
      endpointType: 'WebHook'
      properties: {
        azureActiveDirectoryApplicationIdOrUri: azureADAppIdOrUri
        azureActiveDirectoryTenantId: azureTenantId
        endpointUrl: 'https://${apiApp.properties.defaultHostName}/api/file/webhook'
        maxEventsPerBatch: 1
        preferredBatchSizeInKilobytes: 64
      }
    }
    filter: {
      includedEventTypes: [
        'Microsoft.Storage.BlobCreated'
        'Microsoft.Storage.BlobDeleted'
      ]
      isSubjectCaseSensitive: false
      enableAdvancedFilteringOnArrays: true
      subjectBeginsWith: '/blobServices/default/containers/${storageServiceFoldersContainerName}/'
    }
    eventDeliverySchema: 'EventGridSchema'
    retryPolicy: {
      maxDeliveryAttempts: 30
      eventTimeToLiveInMinutes: 1440
    }
  }
  // dependsOn: [
  //   apiApp
  // ]
}

@description('AI Cognitive Services')
resource aiServices 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: aiServices_name
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  sku: {
    name: 'S0'
  }
  kind: 'CognitiveServices'
  properties: {
    disableLocalAuth: false
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
    }
  }
}

output url string = 'https://${webApp.properties.defaultHostName}'
output api_url string = 'https://${apiApp.properties.defaultHostName}'
