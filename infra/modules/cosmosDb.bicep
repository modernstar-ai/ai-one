@description('Name of the Cosmos DB Account.')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('Resource ID of the virtual network to link the private DNS zones.')
param virtualNetworkResourceId string = ''

@description('Resource ID of the subnet for the private endpoint.')
param virtualNetworkSubnetResourceId string = ''

@description('Resource ID of the Log Analytics workspace to use for diagnostic settings.')
param logAnalyticsWorkspaceResourceId string

@description('Specifies whether network isolation is enabled. This will create a private endpoint for the Cosmos DB Account and link the private DNS zone.')
param networkIsolation bool = false

import { roleAssignmentType } from 'br/public:avm/utl/types/avm-common-types:0.5.1'
@description('Optional. Array of role assignments to create.')
param roleAssignments roleAssignmentType[] = []

import { sqlDatabaseType } from 'customTypes.bicep'
@description('Optional. List of Cosmos DB databases to deploy.')
param databases sqlDatabaseType[]?

@description('Key Vault name for storing secrets related to Cosmos DB.')
param keyVaultName string

@description('Cosmos DD Account API Secret Name')
param cosmosDbAccountApiSecretName string

module privateDnsZone 'br/public:avm/res/network/private-dns-zone:0.7.0' = if (networkIsolation) {
  name: 'private-dns-cosmosdb-deployment'
  params: {
    name: 'privatelink.documents.azure.com'
    virtualNetworkLinks: [
      {
        virtualNetworkResourceId: virtualNetworkResourceId
      }
    ]
    tags: tags
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' existing = {
  name: keyVaultName
}

module cosmosDb 'br/public:avm/res/document-db/database-account:0.11.0' = {
  name: take('${toLower(name)}-cosmosdb-deployment', 64)
  params: {
    name: name
    location: location
    tags: tags
    databaseAccountOfferType: 'Standard'
    // automaticFailover: true
    diagnosticSettings: [
      {
        name: 'default'
        workspaceResourceId: logAnalyticsWorkspaceResourceId
      }
    ]
    disableKeyBasedMetadataWriteAccess: false
    disableLocalAuth: false
    minimumTlsVersion: 'Tls12'
    defaultConsistencyLevel: 'Session'
    networkRestrictions: {
      // networkAclBypass: 'None'
      publicNetworkAccess: networkIsolation ? 'Disabled' : 'Enabled'
    }
    privateEndpoints: networkIsolation
      ? [
          {
            privateDnsZoneGroup: {
              privateDnsZoneGroupConfigs: [
                {
                  privateDnsZoneResourceId: privateDnsZone.outputs.resourceId
                }
              ]
            }
            service: 'Sql'
            subnetResourceId: virtualNetworkSubnetResourceId
          }
        ]
      : []
    sqlDatabases: databases
    roleAssignments: roleAssignments
    secretsExportConfiguration: {
      keyVaultResourceId: keyVault.id
      primaryWriteKeySecretName: cosmosDbAccountApiSecretName
    }
  }
}

output resourceId string = cosmosDb.outputs.resourceId
output name string = cosmosDb.outputs.name
output endpoint string = cosmosDb.outputs.endpoint
