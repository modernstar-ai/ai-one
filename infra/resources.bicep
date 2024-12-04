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

@description('Enable PII Auditing')
@allowed(['true', 'false'])
param auditIncludePII string = 'true'

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

@description('Admin email addresses array')
param AdminEmailAddresses array = [
  'adam-stephensen@agile-analytics.com.au'
]

// param AzureCosmosdbDbName string = 'chat'
// param AzureCosmosdbDatabaseName string = 'chat'
// param AzureCosmosdbContainerName string = 'history'
// param AzureCosmosdbConfigContainerName string = 'config'
// param AzureCosmosdbToolsContainerName string = 'tools'
// param AzureCosmosdbFilesContainerName string = 'fileUploads'
// param AzureCosmosdbAuditsContainerName string = 'audits'

// param AzureSearchIndexNameRag string = 'rag_index'
// param MaxUploadDocumentSize string = '20000000'
// param AzureStorageFoldersContainerName string = 'folders'

param searchServiceSkuName string = 'standard'
// param searchServiceIndexName string = 'azure-chat'

param storageServiceSku object
param storageServiceImageContainerName string

var openai_name = toLower('${resourcePrefix}-aillm')
//var openai_dalle_name = toLower('${resourcePrefix}-aidalle')

// @description('Cosmos DB Chat threads container name')
// param azureCosmosDbChatThreadsName string = 'history'

var cosmos_name = toLower('${resourcePrefix}-cosmos')
var search_name = toLower('${resourcePrefix}-search')

//var clean_name = replace(replace('${resourcePrefix}', '-', ''), '_', '')
// var storage_prefix = take(clean_name, 13)

@description('The unique name of the Storage Account.')
param storage_name string = toLower('stg${uniqueString(resourceGroup().id)}')

//var keyVaultName = toLower('${resourcePrefix}-kv')

@description('The unique name of the Key Vault.')
param keyVaultName string = toLower('kv-${uniqueString(resourceGroup().id)}')

var la_workspace_name = toLower('${resourcePrefix}-la')
var diagnostic_setting_name = 'AppServiceConsoleLogs'

var keyVaultSecretsOfficerRole = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
)

@description('Conditionally deploy key vault Api app permissions')
param kvSetFunctionAppPermissions bool = false

var validStorageServiceImageContainerName = toLower(replace(storageServiceImageContainerName, '-', ''))

//Event Grid config
@description('The container name where files will be stored for folder search')
param storageServiceFoldersContainerName string = 'index-content'

@description('The Azure Active Directory Application ID or URI to get the access token that will be included as the bearer token in delivery requests')
@secure()
param azureADAppIdOrUri string = ''

@description('Event Grid System Topic Name')
var EventGridSystemTopicName = toLower('${resourcePrefix}-folders-listener')

@description('Event Grid Subscription')
var EventGridSystemTopicSubName = toLower('${resourcePrefix}-folders-blobs-listener')

@description('Conditionally deploy event Grid')
param deployEventGrid bool = false

var databaseName = 'chat'
var historyContainerName = 'history'
var configContainerName = 'config'

@description('UTS Role Endpoint')
param UtsRoleApiEndpoint string = ''

@description('UTS Subject Query API Key')
@secure()
param UtsXApiKey string = ''

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
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'

      connectionStrings: [
        {
          connectionString: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_STORAGE_ACCOUNT_CONNECTION.name})'
          name: 'BlobStorage'
          type: 'Custom'
        }

        {
          connectionString: applicationInsights.properties.ConnectionString
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          type: 'Custom'
        }
      ]

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
          name: 'Audit__IncludePII'
          value: auditIncludePII
        }
        {
          name: 'AzureAd__ClientId'
          value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_CLIENT_ID.name})'
        }
        {
          name: 'AzureAd__ClientSecret'
          value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_CLIENT_SECRET.name})'
        }
        {
          name: 'AzureAd__TenantId'
          value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_TENANT_ID.name})'
        }
        {
          name: 'AzureAiServicesKey'
          value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_AI_SERVICES_KEY.name})'
        }
        {
          name: 'AzureOpenAi__Endpoint'
          value: azureopenai.properties.endpoint
        }
        {
          name: 'AzureOpenAi__ApiKey'
          value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_OPENAI_API_KEY.name})'
        }
        {
          name: 'AzureOpenAi__ApiVersion'
          value: openai_api_version
        }
        {
          name: 'AzureOpenAi__InstanceName'
          value: openai_name
        }
        {
          name: 'AzureOpenAi__DeploymentName'
          value: chatGptDeploymentName
        }
        {
          name: 'AzureOpenAi__EmbeddingsDeploymentName'
          value: embeddingDeploymentName
        }
        {
          name: 'AzureOpenAi__EmbeddingsModelName'
          value: embeddingModelName
        }
        {
          name: 'AzureSearch__Endpoint'
          value: 'https://${search_name}.search.windows.net'
        }
        {
          name: 'AzureSearch__ApiKey'
          value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_SEARCH_API_KEY.name})'
        }
        {
          name: 'CosmosDb__Endpoint'
          value: cosmosDbAccount.properties.documentEndpoint
        }
        {
          name: 'CosmosDb__Key'
          value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_COSMOSDB_KEY.name})'
        }
        {
          name: 'AdminEmailAddresses'
          value: join(AdminEmailAddresses, ',')
        }
        {
          name: 'UtsRoleApiEndpoint'
          value: UtsRoleApiEndpoint
        }
        {
          name: 'UtsXApiKey'
          value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::UTS_XAPI_KEY.name})'
        }
        // {
        //   name: 'AZURE_COSMOSDB_URI'
        //   value: cosmosDbAccount.properties.documentEndpoint
        // }
        // {
        //   name: 'AZURE_COSMOSDB_KEY'
        //   value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_COSMOSDB_KEY.name})'
        // }

        // {
        //   name: 'AZURE_STORAGE_FOLDERS_CONTAINER_NAME'
        //   value: AzureStorageFoldersContainerName
        // }
        // {
        //   name: 'MAX_UPLOAD_DOCUMENT_SIZE'
        //   value: MaxUploadDocumentSize
        // }
        // {
        //   name: 'AZURE_SEARCH_INDEX_NAME_RAG'
        //   value: AzureSearchIndexNameRag
        // }
        // {
        //   name: 'AZURE_COSMOSDB_DB_NAME'
        //   value: AzureCosmosdbDbName
        // }
        // {
        //   name: 'AZURE_COSMOSDB_DATABASE_NAME'
        //   value: AzureCosmosdbDatabaseName
        // }
        // {
        //   name: 'AZURE_COSMOSDB_CONTAINER_NAME'
        //   value: AzureCosmosdbContainerName
        // }
        // {
        //   name: 'AZURE_COSMOSDB_CONFIG_CONTAINER_NAME'
        //   value: AzureCosmosdbConfigContainerName
        // }
        // {
        //   name: 'AZURE_COSMOSDB_TOOLS_CONTAINER_NAME'
        //   value: AzureCosmosdbToolsContainerName
        // }
        // {
        //   name: 'AZURE_COSMOSDB_FILES_CONTAINER_NAME'
        //   value: AzureCosmosdbFilesContainerName
        // }
        // {
        //   name: 'AZURE_COSMOSDB_AUDIT_CONTAINER_NAME'
        //   value: AzureCosmosdbAuditsContainerName
        // }        
        {
          name: 'ALLOWED_ORIGINS'
          value: 'https://${webApp.properties.defaultHostName}'
        }
        // {
        //   name: 'AZURE_COSMOSDB_CHAT_THREADS_CONTAINER_NAME'
        //   value: azureCosmosDbChatThreadsName
        // }       
        // {
        //   name: 'AZURE_KEY_VAULT_NAME'
        //   value: keyVaultName
        // }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: aspCoreEnvironment
        }
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'false'
        }
        // {
        //   name: 'AZURE_SEARCH_NAME'
        //   value: search_name
        // }
        // {
        //   name: 'AZURE_SEARCH_INDEX_NAME'
        //   value: searchServiceIndexName
        // }        
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

@description('The name of the Role Assignment - from Guid.')
param roleAssignmentName string = newGuid()

resource kvFunctionAppPermissions 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (kvSetFunctionAppPermissions) {
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

  resource UTS_XAPI_KEY 'secrets' = {
    name: 'UTS-XAPI-KEY'
    properties: {
      contentType: 'text/plain'
      value: UtsXApiKey
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
    publicAccess: 'None' //'Blob'
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
resource eventGridSystemTopic 'Microsoft.EventGrid/systemTopics@2024-06-01-preview' = if (deployEventGrid) {
  name: EventGridSystemTopicName
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    source: storage.id
    //source: '/subscriptions/${subscription().subscriptionId}/resourceGroups/${resourceGroup().name}/providers/Microsoft.Storage/storageAccounts/${storage_name}'
    topicType: 'Microsoft.Storage.StorageAccounts'
  }
}

@description('Event Grid Subscription')
resource eventGrid 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2024-06-01-preview' = if (deployEventGrid) {
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
  dependsOn: [
    apiApp
  ]
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
