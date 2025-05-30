@description('Key Vault name')
param keyVaultName string

@description('Azure region for resource deployment')
param location string

@description('Resource tags')
param tags object

@description('List of secrets to create in Key Vault')
param keyVaultSecrets array

resource kv 'Microsoft.KeyVault/vaults@2024-11-01' = {
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
}

// Create secrets dynamically from the array
resource keyVaultSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = [for secret in keyVaultSecrets: {
  parent: kv
  name: secret.name
  properties: {
    value: secret.value
    contentType: secret.contentType
  }
}]

output keyVaultName string = kv.name
output keyVaultId string = kv.id
