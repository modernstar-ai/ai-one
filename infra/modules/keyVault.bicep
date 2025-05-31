@description('Name of the Key Vault.')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('Resource ID of the Log Analytics workspace to use for diagnostic settings.')
param logAnalyticsWorkspaceResourceId string

@description('List of secrets to create in Key Vault')
param keyVaultSecrets array

@description('Specifies the object id of a Microsoft Entra ID user. In general, this the object id of the system administrator who deploys the Azure resources. This defaults to the deploying user.')
param userObjectId string

resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(userObjectId)) {
  name: guid(keyvault.id, userObjectId, 'Key Vault Secrets User')
  scope: keyvault
  properties: {
    principalId: userObjectId
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '4633458b-17de-408a-b874-0445c86b69e6'
    ) // Key Vault Secrets User
    principalType: 'User'
  }
}

resource keyvault 'Microsoft.KeyVault/vaults@2024-11-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enabledForDeployment: true
    enabledForDiskEncryption: true
    enabledForTemplateDeployment: false
    enableSoftDelete: true
    softDeleteRetentionInDays: 3
  }
}

resource keyVaultSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = [
  for secret in keyVaultSecrets: {
    parent: keyvault
    name: secret.name
    properties: {
      value: secret.value
      contentType: secret.contentType
    }
  }
]

resource keyVaultDiagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = if (!empty(logAnalyticsWorkspaceResourceId)) {
  name: '${keyvault.name}-diagnostic'
  scope: keyvault
  properties: {
    workspaceId: logAnalyticsWorkspaceResourceId
    logs: [
      // {
      //   category: 'AuditEvent'
      //   enabled: true
      //   retentionPolicy: {
      //     enabled: false
      //     days: 0
      //   }
      // }
    ]
    metrics: [
      // {
      //   category: 'AllMetrics'
      //   enabled: true
      //   retentionPolicy: {
      //     enabled: false
      //     days: 0
      //   }
      // }
    ]
  }
}

output resourceId string = keyvault.id
output name string = keyvault.name
