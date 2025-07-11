@description('Azure region for resource deployment')
param location string

@description('Resource tags')
param tags object

@minLength(1)
@maxLength(12)
@description('The name of the solution.')
param projectName string

@minLength(1)
@maxLength(4)
@description('The type of environment. e.g. local, dev, uat, prod.')
param environmentName string

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

@description('Log Analytics Workspace name')
param logAnalyticsWorkspaceName string

@description('Key Vault name')
param keyVaultName string

@description('Storage account name')
param storageName string

@description('Storage Account name (for existing resource)')
param storageAccountName string

@description('Document Intelligence Service name')
param documentIntelligenceServiceName string

@description('Document Intelligence Service endpoint')
param documentIntelligenceEndpoint string

@description('OpenAI resource name')
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

@description('Cosmos DB Account name')
param cosmosDbAccountName string

@description('Cosmos DB Account endpoint (document endpoint)')
param cosmosDbAccountEndpoint string

param agileChatDatabaseName string = 'AgileChat'

@description('Allowed origins for CORS')
param allowedOrigins string[] = []

@description('Admin email addresses')
param adminEmailAddresses array

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

var blobContainersArray = loadJsonContent('../blob-storage-containers.json')
var blobContainers = [
  for name in blobContainersArray: {
    name: toLower(name)
    publicAccess: 'None'
  }
]

resource azureopenai 'Microsoft.CognitiveServices/accounts@2023-05-01' existing = {
  name: openAiName
}

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}

resource documentIntelligence 'Microsoft.CognitiveServices/accounts@2023-05-01' existing = {
  name: documentIntelligenceServiceName
}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2024-01-01' existing = {
  name: serviceBusName
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' existing = {
  name: logAnalyticsWorkspaceName
}

resource appServicePlan 'Microsoft.Web/serverfarms@2021-02-01' existing = {
  name: appServicePlanName
}

resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' existing = {
  name: keyVaultName
}

// Key Vault Secrets User Role
resource keyVaultSecretsUserRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '4633458b-17de-408a-b874-0445c86b69e6'
  scope: subscription()
}

// Cognitive Services OpenAI User Role
resource openAiUserRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
  scope: subscription()
}

// Cognitive Services User Role for Document Intelligence
resource cognitiveServicesUserRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: 'a97b65f3-24c7-4388-baec-2e87135dc908'
  scope: subscription()
}

// Storage Blob Data Contributor Role
resource blobDataContributorRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
  scope: subscription()
}

// Azure Service Bus Data Receiver Role
resource serviceBusDataReceiverRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'
  scope: subscription()
}

// Azure Service Bus Data Sender Role
resource serviceBusDataSenderRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
  scope: subscription()
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
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspace.id
    userAssignedIdentityId: apiAppManagedIdentity.id
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
          value: storageName
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
          name: 'KeyVault__Name'
          value: keyVaultName
        }
        {
          name: 'KeyVault__ClientId'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=AZURE-CLIENT-ID)'
        }
        {
          name: 'KeyVault__TenantId'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=AZURE-TENANT-ID)'
        }
        {
          name: 'ApplicationInsights__InstrumentationKey'
          value: applicationInsights.properties.InstrumentationKey
        }
        {
          name: 'AdminEmailAddresses'
          value: join(adminEmailAddresses, ',')
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
          name: 'AllowModelSelection'
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
    destination: {
      endpointType: 'ServiceBusQueue'
      properties: {
        resourceId: serviceBusQueue.id
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
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

resource apiAppOpenAiRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, apiAppManagedIdentity.id, azureopenai.id, openAiUserRole.id)
  scope: azureopenai
  properties: {
    roleDefinitionId: openAiUserRole.id
    principalId: apiAppManagedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource apiAppBlobStorageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, apiAppManagedIdentity.id, storage.id, blobDataContributorRole.id)
  scope: storage
  properties: {
    roleDefinitionId: blobDataContributorRole.id
    principalId: apiAppManagedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// resource apiAppCosmosDbContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
//   name: guid(resourceGroup().id, apiAppManagedIdentity.id, cosmosDbAccount.id, cosmosDbContributorRole.id)
//   scope: cosmosDbAccount
//   properties: {
//     roleDefinitionId: cosmosDbContributorRole.id
//     principalId: apiAppManagedIdentity.properties.principalId
//     principalType: 'ServicePrincipal'
//   }
// }

resource apiAppDocIntelligenceRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, apiAppManagedIdentity.id, documentIntelligence.id, cognitiveServicesUserRole.id)
  scope: documentIntelligence
  properties: {
    roleDefinitionId: cognitiveServicesUserRole.id
    principalId: apiAppManagedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource apiAppServiceBusReceiverRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, apiAppManagedIdentity.id, serviceBus.id, serviceBusDataReceiverRole.id)
  scope: serviceBus
  properties: {
    roleDefinitionId: serviceBusDataReceiverRole.id
    principalId: apiAppManagedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource apiAppServiceBusSenderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, apiAppManagedIdentity.id, serviceBus.id, serviceBusDataSenderRole.id)
  scope: serviceBus
  properties: {
    roleDefinitionId: serviceBusDataSenderRole.id
    principalId: apiAppManagedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

module appServiceSecretsUserRoleAssignmentModule '../modules/keyvaultRoleAssignment.bicep' = {
  name: guid(resourceGroup().id, apiAppManagedIdentity.id, keyVault.id, keyVaultSecretsUserRole.id)
  params: {
    roleDefinitionId: keyVaultSecretsUserRole.id
    principalId: apiAppManagedIdentity.properties.principalId
    keyVaultName: keyVaultName
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

output apiAppDefaultHostName string = apiAppModule.outputs.defaultHostName
output apiAppManagedIdentityId string = apiAppManagedIdentity.id
