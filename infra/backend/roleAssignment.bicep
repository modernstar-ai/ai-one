param principalId string
param eventGridSystemTopicPrincipalId string

// Resource IDs for role assignment targets
param openAiResourceId string
param storageResourceId string
param documentIntelligenceResourceId string
param serviceBusResourceId string
param keyVaultResourceId string
param aiFoundryProjectName string = ''
param aiFoundryAccountName string = ''

// Role Definition IDs (public, not secrets)
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

// Azure AI User Role
resource azureAiUserRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '53ca6127-db72-4b80-b1b0-d745d6d5456d'
  scope: subscription()
}

resource openAiResource 'Microsoft.CognitiveServices/accounts@2023-05-01' existing = {
  name: openAiResourceId
}

resource documentIntelligenceResource 'Microsoft.CognitiveServices/accounts@2023-05-01' existing = {
  name: documentIntelligenceResourceId
}

resource serviceBusResource 'Microsoft.ServiceBus/namespaces@2024-01-01' existing = {
  name: serviceBusResourceId
}

resource storageResource 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageResourceId
}

resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' existing = {
  name: keyVaultResourceId
}

resource aiFoundryAccount 'Microsoft.CognitiveServices/accounts@2025-04-01-preview' existing = if (!empty(aiFoundryProjectName)) {
  name: aiFoundryAccountName
}

resource aiFoundryProject 'Microsoft.CognitiveServices/accounts/projects@2025-04-01-preview' existing = if (!empty(aiFoundryProjectName)) {
  name: aiFoundryProjectName
  parent: aiFoundryAccount
}

resource apiAppOpenAiRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, principalId, openAiResourceId, openAiUserRole.id)
  scope: openAiResource
  properties: {
    roleDefinitionId: openAiUserRole.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

resource apiAppBlobStorageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, principalId, storageResourceId, blobDataContributorRole.id)
  scope: storageResource
  properties: {
    roleDefinitionId: blobDataContributorRole.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

resource apiAppDocIntelligenceRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, principalId, documentIntelligenceResourceId, cognitiveServicesUserRole.id)
  scope: documentIntelligenceResource
  properties: {
    roleDefinitionId: cognitiveServicesUserRole.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

resource apiAppServiceBusReceiverRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, principalId, serviceBusResourceId, serviceBusDataReceiverRole.id)
  scope: serviceBusResource
  properties: {
    roleDefinitionId: serviceBusDataReceiverRole.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

resource apiAppServiceBusSenderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, principalId, serviceBusResourceId, serviceBusDataSenderRole.id)
  scope: serviceBusResource
  properties: {
    roleDefinitionId: serviceBusDataSenderRole.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

resource keyVaultResourceIdRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, principalId, keyVaultSecretsUserRole.id)
  scope: keyVault
  properties: {
    roleDefinitionId: keyVaultSecretsUserRole.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

resource eventGridSystemTopicServiceBusSenderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, eventGridSystemTopicPrincipalId, serviceBusDataSenderRole.id)
  scope: serviceBusResource
  properties: {
    roleDefinitionId: serviceBusDataSenderRole.id
    principalId: eventGridSystemTopicPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource apiAppAiFoundryProjectRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(aiFoundryProjectName)) {
  name: guid(resourceGroup().id, principalId, aiFoundryProjectName, azureAiUserRole.id)
  scope: aiFoundryProject
  properties: {
    roleDefinitionId: azureAiUserRole.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
