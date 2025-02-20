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

@description('APIM Azure OpenAI Endpoint')
param apimAiEndpointOverride string = ''
@description('APIM Azure OpenAI Embedding Endpoint')
param apimAiEmbeddingsEndpointOverride string = ''

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
var form_recognizer_name = toLower('${resourcePrefix}-form')
var service_bus_name = toLower('${resourcePrefix}-service-bus')
var service_bus_queue_name = toLower('${resourcePrefix}-folders-queue')

@description('ets options that control the availability of semantic search')
@allowed(['disabled', 'free', 'standard'])
param semanticSearchSku string = 'free'

//var clean_name = replace(replace('${resourcePrefix}', '-', ''), '_', '')
// var storage_prefix = take(clean_name, 13)

@description('The unique name of the Storage Account.')
var storage_name = toLower('${projectName}${environmentName}sto')

var keyVaultName = toLower('${resourcePrefix}-kv')

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

@description('Event Grid Subscription')
var EventGridSystemTopicSubName = toLower('${resourcePrefix}-folders-blobs-listener')

@description('Event Grid Name')
var eventGridName = toLower('${resourcePrefix}-blob-eg')

var databaseName = 'chat'
var historyContainerName = 'history'
var configContainerName = 'config'

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

      appSettings: concat(
        [
          {
            name: 'BlobStorage__Name'
            value: storage_name
          }
          {
            name: 'BlobStorage__Key'
            value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_STORAGE_ACCOUNT_KEY.name})'
          }
          {
            name: 'Audit__IncludePII'
            value: auditIncludePII
          }
          {
            name: 'AzureDocumentIntelligence__ApiKey'
            value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_DOCUMENT_INTELLIGENCE_KEY.name})'
          }
          {
            name: 'AzureDocumentIntelligence__Endpoint'
            value: 'https://${form_recognizer_name}.cognitiveservices.azure.com/'
          }
          {
            name: 'AzureServiceBus__ConnectionString'
            value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_SERVICE_BUS_CONNECTION_STRING.name})'
          }
          {
            name: 'AzureServiceBus__BlobQueueName'
            value: service_bus_queue_name
          }
          {
            name: 'AzureAd__ClientId'
            value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_CLIENT_ID.name})'
          }
          {
            name: 'AzureAd__TenantId'
            value: '@Microsoft.KeyVault(VaultName=${kv.name};SecretName=${kv::AZURE_TENANT_ID.name})'
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
            name: 'ASPNETCORE_ENVIRONMENT'
            value: aspCoreEnvironment
          }
          {
            name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
            value: 'false'
          }
        ],
        !empty(apimAiEndpointOverride)
          ? [
              {
                name: 'AzureOpenAi__Apim__Endpoint'
                value: apimAiEndpointOverride
              }
            ]
          : [],
        !empty(apimAiEmbeddingsEndpointOverride)
          ? [
              {
                name: 'AzureOpenAi__Apim__EmbeddingsEndpoint'
                value: apimAiEmbeddingsEndpointOverride
              }
            ]
          : [],
        empty(apimAiEndpointOverride)
          ? [
              {
                name: 'AzureOpenAi__Endpoint'
                value: azureopenai.properties.endpoint
              }
            ]
          : []
      )
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
resource kvFunctionAppPermissions 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (kvSetFunctionAppPermissions) {
  name: apiApp.name
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

  resource AZURE_OPENAI_API_KEY 'secrets' = if (empty(apimAiEndpointOverride)) {
    name: 'AZURE-OPENAI-API-KEY'
    properties: {
      contentType: 'text/plain'
      value: azureopenai.listKeys().key1
    }
  }

  resource AZURE_DOCUMENT_INTELLIGENCE_KEY 'secrets' = {
    name: 'AZURE-DOCUMENT-INTELLIGENCE-KEY'
    properties: {
      contentType: 'text/plain'
      value: formRecognizer.listKeys().key1
    }
  }

  resource AZURE_SERVICE_BUS_CONNECTION_STRING 'secrets' = {
    name: 'AZURE-SERVICE-BUS-CONNECTION-STRING'
    properties: {
      contentType: 'text/plain'
      value: listKeys('${serviceBus.id}/AuthorizationRules/RootManageSharedAccessKey', serviceBus.apiVersion).primaryConnectionString
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

  resource AZURE_CLIENT_ID 'secrets' = {
    name: 'AZURE-CLIENT-ID'
    properties: {
      contentType: 'text/plain'
      value: azureClientID
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

//old api Microsoft.Search/searchServices@2022-09-01
resource searchService 'Microsoft.Search/searchServices@2024-06-01-preview' = {
  name: search_name
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
  identity: { type: 'SystemAssigned' }
}

resource azureopenai 'Microsoft.CognitiveServices/accounts@2023-05-01' = if (empty(apimAiEndpointOverride)) {
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
  for deployment in llmDeployments: if (empty(apimAiEndpointOverride)) {
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

@description('Event Grid System Topic')
resource eventGridSystemTopic 'Microsoft.EventGrid/systemTopics@2024-06-01-preview' = {
  name: eventGridName
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
      endpointType: 'ServiceBusQueue'
      properties: {
        resourceId: serviceBus::queue.id
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

resource formRecognizer 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: form_recognizer_name
  location: location
  tags: tags
  kind: 'FormRecognizer'
  properties: {
    customSubDomainName: form_recognizer_name
    publicNetworkAccess: 'Enabled'
  }
  sku: {
    name: 'S0'
  }
}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  location: location
  name: service_bus_name

  resource queue 'queues' = {
    name: service_bus_queue_name
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
      maxDeliveryCount: 10
      status: 'Active'
      autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
      enablePartitioning: false
      enableExpress: false
    }
  }
}

output url string = 'https://${webApp.properties.defaultHostName}'
output api_url string = 'https://${apiApp.properties.defaultHostName}'
