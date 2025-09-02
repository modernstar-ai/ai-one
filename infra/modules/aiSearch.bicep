@description('Name of the AI Search resource.')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('SKU for Azure Search Service')
param skuName string = 'standard'

@description('SKU for semantic search')
@allowed(['disabled', 'free', 'standard'])
param semanticSearchSku string = 'free'

@description('Resource ID of the virtual network to link the private DNS zones.')
param virtualNetworkResourceId string = ''

@description('Resource ID of the subnet for the private endpoint.')
param virtualNetworkSubnetResourceId string = ''

@description('Resource ID of the Log Analytics workspace to use for diagnostic settings.')
param logAnalyticsWorkspaceResourceId string

@description('Specifies whether network isolation is enabled. This will create a private endpoint for the AI Search resource and link the private DNS zone.')
param networkIsolation bool = false

import { roleAssignmentType } from 'br/public:avm/utl/types/avm-common-types:0.5.1'
@description('Optional. Array of role assignments to create.')
param roleAssignments roleAssignmentType[] = []

@description('Key Vault name for storing secrets related to AI Search.')
param keyVaultName string

@description('Key Vault secret name for AI Search API Key')
param searchServiceApiKeySecretName string = 'searchServiceApiKey'

resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' existing = {
  name: keyVaultName
}

module privateDnsZone 'br/public:avm/res/network/private-dns-zone:0.7.0' = if (networkIsolation) {
  name: 'private-dns-search-deployment'
  params: {
    name: 'privatelink.search.windows.net'
    virtualNetworkLinks: [
      {
        virtualNetworkResourceId: virtualNetworkResourceId
      }
    ]
    tags: tags
  }
}

module aiSearch 'br/public:avm/res/search/search-service:0.10.0' = {
  name: take('${take(toLower(name), 60)}-search-services-deployment', 64)
  dependsOn: [privateDnsZone] // required due to optional flags that could change dependency
  params: {
    name: name
    location: location
    tags: tags
    // cmkEnforcement: 'Enabled'
    disableLocalAuth: false
    managedIdentities: {
      systemAssigned: true
    }
    publicNetworkAccess: networkIsolation ? 'Disabled' : 'Enabled'
    sku: skuName
    partitionCount: 1
    replicaCount: 1
    roleAssignments: roleAssignments
    diagnosticSettings: [
      {
        name: 'default'
        workspaceResourceId: logAnalyticsWorkspaceResourceId
      }
    ]
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
            subnetResourceId: virtualNetworkSubnetResourceId
          }
        ]
      : []
    semanticSearch: semanticSearchSku
    secretsExportConfiguration: {
      keyVaultResourceId: keyVault.id
      primaryAdminKeyName: searchServiceApiKeySecretName
    }
  }
}

output resourceId string = aiSearch.outputs.resourceId
output name string = aiSearch.outputs.name
output endpoint string = aiSearch.outputs.endpoint
