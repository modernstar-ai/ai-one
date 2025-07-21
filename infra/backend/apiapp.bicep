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

@description('Azure AD Tenant ID')
param azureTenantId string

@description('Resource prefix for naming resources')
param resourcePrefix string = toLower('${projectName}-${environmentName}')

@description('API App name')
param apiAppName string = toLower('${resourcePrefix}-apiapp')

@description('App Service Plan name')
param appServicePlanName string

@description('Application Insights name')
param applicationInsightsName string = toLower('${resourcePrefix}-apiapp')

@description('Log Analytics Workspace ID')
param logAnalyticsWorkspaceResourceId string

@description('Key Vault name')
param keyVaultName string

@description('Storage Account name (for existing resource)')
param storageAccountName string

@description('Document Intelligence Service name')
param documentIntelligenceServiceName string

@description('Document Intelligence Service endpoint')
param documentIntelligenceEndpoint string

@description('OpenAI resource name. If using Foundry, this will be the account name.')
param openAiName string

@description('OpenAI API endpoint')
param openAiEndpoint string

@description('OpenAI API version')
param openAiApiVersion string

@description('APIM Azure OpenAI Endpoint')
param apimAiEndpointOverride string = ''

@description('APIM Azure OpenAI Embedding Endpoint')
param apimAiEmbeddingsEndpointOverride string = ''

@description('ChatGPT deployment name')
param chatGptDeploymentName string = 'gpt-4o'

@description('Embedding deployment name')
param embeddingDeploymentName string = 'embedding'

@description('Embedding model name')
param embeddingModelName string = 'text-embedding-3-small'

@description('Azure Search Service name')
param searchServiceName string

@description('Service Bus namespace name')
param serviceBusName string

@description('Service Bus queue name')
param serviceBusQueueName string = toLower('${resourcePrefix}-folders-queue')

@description('Maximum message size for Service Bus queue in KB')
param maxServiceBusQueueMessageSizeInKilobytes int = 256

@description('Cosmos DB Account name')
param cosmosDbAccountName string

@description('Cosmos DB Account endpoint (document endpoint)')
param cosmosDbAccountEndpoint string

param agileChatDatabaseName string = 'AgileChat'

@description('Allowed origins for CORS')
param allowedOrigins string[] = []

@description('Enable PII Auditing')
@allowed(['true', 'false'])
param auditIncludePII string = 'true'

@description('Deployment Environment')
@allowed(['Development', 'Test', 'UAT', 'Production'])
param aspCoreEnvironment string

param storageServiceFoldersContainerName string = 'index-content'

@description('Event Grid system topic name')
param eventGridName string = toLower('${resourcePrefix}-blob-eg')

param allowModelSelection bool = true

param defaultTextModelId string = 'gpt-4o'

param eventGridSystemTopicSubName string = toLower('${resourcePrefix}-folders-blobs-listener')

@description('Whether to enable network isolation for resources')
param networkIsolation bool = false

@description('Azure Virtual Network name')
param virtualNetworkName string = toLower('${resourcePrefix}-vnet')

@description('App Service subnet name')
param appServiceSubnetName string = 'AppServiceSubnet'

param deployRoleAssignments bool = true

var blobContainersArray = loadJsonContent('../blob-storage-containers.json')
var blobContainers = [
  for name in blobContainersArray: {
    name: toLower(name)
    publicAccess: 'None'
  }
]

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}

resource appServicePlan 'Microsoft.Web/serverfarms@2021-02-01' existing = {
  name: appServicePlanName
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' existing = {
  name: serviceBusName
}

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' existing = {
  name: cosmosDbAccountName
}

resource apiAppManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-${apiAppName}'
  location: location
}

resource vnet 'Microsoft.Network/virtualNetworks@2023-05-01' existing = if (networkIsolation) {
  name: virtualNetworkName
}

var virtualNetworkResourceId = networkIsolation ? vnet.id : ''
var appServiceSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${appServiceSubnetName}' : ''

module apiAppModule '../modules/site.bicep' = {
  name: 'apiAppModule'
  dependsOn: [
    serviceBusQueue
  ]
  params: {
    name: apiAppName
    location: location
    tags: union(tags, { 'azd-service-name': 'agilechat-api' })
    serverFarmResourceId: appServicePlan.id
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceResourceId
    userAssignedIdentityId: apiAppManagedIdentity.id
    networkIsolation: networkIsolation
    virtualNetworkResourceId: virtualNetworkResourceId
    virtualNetworkSubnetResourceId: appServiceSubnetResourceId
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
        allowedOrigins: allowedOrigins
        supportCredentials: true
      }
      defaultDocuments: [
        'string'
      ]
    }
    appSettings: concat(
      [
        {
          name: 'BlobStorage__AccountName'
          value: storageAccountName
        }
        {
          name: 'Audit__IncludePII'
          value: auditIncludePII
        }
        {
          name: 'AzureDocumentIntelligence__Endpoint'
          value: documentIntelligenceEndpoint
        }
        {
          name: 'AzureServiceBus__BlobQueueName'
          value: serviceBusQueueName
        }
        {
          name: 'AzureServiceBus__Namespace'
          value: serviceBusName
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: apiAppManagedIdentity.properties.clientId
        }
        {
          name: 'AZURE_TENANT_ID'
          value: azureTenantId
        }
        {
          name: 'AzureAd__ClientId'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=AZURE-CLIENT-ID)'
        }
        {
          name: 'AzureAd__TenantId'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=AZURE-TENANT-ID)'
        }
        {
          name: 'CosmosDb__Endpoint'
          value: cosmosDbAccountEndpoint
        }
        {
          name: 'CosmosDb__DatabaseName'
          value: agileChatDatabaseName
        }
        {
          name: 'CosmosDb__Key'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=AZURE-COSMOSDB-KEY)'
        }
        {
          name: 'ApplicationInsights__InstrumentationKey'
          value: applicationInsights.properties.InstrumentationKey
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
      [
        {
          name: 'AzureOpenAi__ApiVersion'
          value: openAiApiVersion
        }
        {
          name: 'AzureOpenAi__Endpoint'
          value: openAiEndpoint
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
          name: 'ModelSelectionFeatureEnabled'
          value: allowModelSelection
        }
        {
          name: 'DefaultTextModelId'
          value: defaultTextModelId
        }
        {
          name: 'AzureSearch__Endpoint'
          value: 'https://${searchServiceName}.search.windows.net'
        }
        {
          name: 'AzureSearch__ApiKey'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=AZURE-SEARCH-API-KEY)'
        }
      ]
    )
  }
}

resource cosmosDbDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-04-15' = {
  name: agileChatDatabaseName
  dependsOn: [
    cosmosDbAccount
  ]
  location: location
  tags: tags
  parent: cosmosDbAccount
  properties: {
    resource: {
      id: agileChatDatabaseName
    }
    options: {}
  }
}

resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2024-01-01' = {
  name: serviceBusQueueName
  parent: serviceBusNamespace
  properties: {
    maxMessageSizeInKilobytes: maxServiceBusQueueMessageSizeInKilobytes
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
    autoDeleteOnIdle: 'PT5M'
    enablePartitioning: false
    enableExpress: false
  }
}

resource eventGridSystemTopic 'Microsoft.EventGrid/systemTopics@2024-06-01-preview' = {
  name: eventGridName
  dependsOn: [
    serviceBusQueue
  ]
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

resource eventGrid 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2024-06-01-preview' = {
  name: eventGridSystemTopicSubName
  parent: eventGridSystemTopic
  properties: {
    deliveryWithResourceIdentity: {
      destination: {
        endpointType: 'ServiceBusQueue'
        properties: {
          resourceId: serviceBusQueue.id
        }
      }
      identity: {
        type: 'SystemAssigned'
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
    apiAppModule
  ]
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspaceResourceId
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts/blobServices@2024-01-01' existing = {
  name: 'default'
  parent: storage
}

resource blobContainersResource 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = [
  for container in blobContainers: {
    name: container.name
    parent: storageAccount
    properties: {
      publicAccess: container.publicAccess
    }
  }
]

module apiAppRoleAssignments './roleAssignment.bicep' = if (deployRoleAssignments) {
  name: 'apiAppRoleAssignments'
  params: {
    principalId: apiAppManagedIdentity.properties.principalId
    openAiResourceId: openAiName
    storageResourceId: storageAccountName
    documentIntelligenceResourceId: documentIntelligenceServiceName
    serviceBusResourceId: serviceBusName
    keyVaultResourceId: keyVaultName
    eventGridSystemTopicPrincipalId: eventGridSystemTopic.identity.principalId
  }
}

output apiAppName string = apiAppModule.outputs.name
output apiAppDefaultHostName string = apiAppModule.outputs.defaultHostName
output apiAppManagedIdentityId string = apiAppManagedIdentity.id
