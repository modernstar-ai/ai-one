@description('Name of the Key Vault.')
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

@description('Specifies whether network isolation is enabled. This will create a private endpoint for the Key Vault and link the private DNS zone.')
param networkIsolation bool = false

@description('Role assignments to be applied to the Key Vault.')
import { roleAssignmentType } from 'br/public:avm/utl/types/avm-common-types:0.5.1'
@description('Optional. Array of role assignments to create.')
param roleAssignments roleAssignmentType[] = []

@description('Optional. Array of secrets to create in the Key Vault.')
param secrets object[] = []

module privateDnsZone 'br/public:avm/res/network/private-dns-zone:0.7.0' = if (networkIsolation) {
  name: 'private-dns-keyvault-deployment'
  params: {
    name: 'privatelink.${toLower(environment().name) == 'azureusgovernment' ? 'vaultcore.usgovcloudapi.net' : 'vaultcore.azure.net'}'
    virtualNetworkLinks: [
      {
        virtualNetworkResourceId: virtualNetworkResourceId
      }
    ]
    tags: tags
  }
}

module keyvault 'br/public:avm/res/key-vault/vault:0.11.0' = {
  name: take('${take(toLower(name), 24)}-keyvault-deployment', 64)
  params: {
    name: name
    location: location
    tags: tags
    publicNetworkAccess: networkIsolation ? 'Disabled' : 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
    }
    enableVaultForDeployment: true
    enableVaultForDiskEncryption: true
    enableVaultForTemplateDeployment: true
    enablePurgeProtection: true
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    diagnosticSettings: [
      {
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
            service: 'vault'
            subnetResourceId: virtualNetworkSubnetResourceId
          }
        ]
      : []
    roleAssignments: roleAssignments
    secrets: secrets
  }
}

// module keyvaultSecret 'br/public:avm/res/key-vault/secret:0.11.0' = {
//   name: '${name}-secret'
//   params: {
//     name: 'example-secret'
//     value: 'example-value'
//     keyVaultResourceId: keyvault.outputs.resourceId
//     tags: tags
//   }
//   dependsOn: [
//     keyvault
//   ]

output resourceId string = keyvault.outputs.resourceId
output name string = keyvault.outputs.name
