@description('Name of the Cosmos DB account.')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('Optional. List of Cosmos DB databases to deploy.')
param databases array = []

@description('Key Vault name for storing secrets related to Cosmos DB.')
param keyVaultName string

@description('Cosmos DD Account API Secret Name')
param cosmosDbAccountApiSecretName string

@description('Cosmos DB custom role definition name')
param cosmosDbAccountDataPlaneCustomRoleName string = 'Custom Cosmos DB for NoSQL Data Plane Contributor'

resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' existing = {
  name: keyVaultName
}

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: name
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

resource sqlDatabases 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2022-05-15' = [
  for dbName in databases: {
    name: dbName
    parent: cosmosDbAccount
    properties: {
      resource: {
        id: dbName
      }
    }
  }
]

resource cosmosDbAccountApiSecret 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  name: cosmosDbAccountApiSecretName
  parent: keyVault
  properties: {
    value: 'test'//listKeys(cosmosDbAccount.name, '2023-04-15').primaryMasterKey
  }
}

output name string = cosmosDbAccount.name
output endpoint string = cosmosDbAccount.properties.documentEndpoint
output cosmosDbAccountDataPlaneCustomRoleId string = cosmosDbAccountDataPlaneCustomRole.id
