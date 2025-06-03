@description('Name of the Storage Account.')
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
param logAnalyticsWorkspaceResourceId string = ''

@description('Specifies whether network isolation is enabled. This will create a private endpoint for the Storage Account and link the private DNS zone.')
param networkIsolation bool = false

import { roleAssignmentType } from 'br/public:avm/utl/types/avm-common-types:0.5.1'
@description('Optional. Array of role assignments to create.')
param roleAssignments roleAssignmentType[] = []

@description('SKU for Storage Account')
param skuName string = 'Standard_LRS'

@description('Array of blob container names to be created')
param blobContainerCollection array = []

module blobPrivateDnsZone 'br/public:avm/res/network/private-dns-zone:0.7.0' = if (networkIsolation) {
  name: 'private-dns-blob-deployment'
  params: {
    name: 'privatelink.blob.${environment().suffixes.storage}'
    virtualNetworkLinks: [
      {
        virtualNetworkResourceId: virtualNetworkResourceId
      }
    ]
    tags: tags
  }
}

module filePrivateDnsZone 'br/public:avm/res/network/private-dns-zone:0.7.0' = if (networkIsolation) {
  name: 'private-dns-file-deployment'
  params: {
    name: 'privatelink.file.${environment().suffixes.storage}'
    virtualNetworkLinks: [
      {
        virtualNetworkResourceId: virtualNetworkResourceId
      }
    ]
    tags: tags
  }
}

module storageAccount 'br/public:avm/res/storage/storage-account:0.17.0' = {
  name: take('${take(toLower(name), 24)}-storage-account-deployment', 64)
  dependsOn: [blobPrivateDnsZone, filePrivateDnsZone]
  params: {
    name: name
    location: location
    tags: tags
    kind: 'StorageV2'
    skuName: skuName
    requireInfrastructureEncryption: false //TODO: Set to true
    publicNetworkAccess: networkIsolation ? 'Disabled' : 'Enabled'
    // accessTier: 'Hot'
    // allowBlobPublicAccess: false
    // allowSharedKeyAccess: false
    // allowCrossTenantReplication: false
    minimumTlsVersion: 'TLS1_2'
    networkAcls: {
      defaultAction: 'Allow'
    }
    supportsHttpsTrafficOnly: true
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
                  privateDnsZoneResourceId: blobPrivateDnsZone.outputs.resourceId
                }
              ]
            }
            service: 'blob'
            subnetResourceId: virtualNetworkSubnetResourceId
          }
          {
            privateDnsZoneGroup: {
              privateDnsZoneGroupConfigs: [
                {
                  privateDnsZoneResourceId: filePrivateDnsZone.outputs.resourceId
                }
              ]
            }
            service: 'file'
            subnetResourceId: virtualNetworkSubnetResourceId
          }
        ]
      : []
    roleAssignments: roleAssignments
    blobServices: {
      automaticSnapshotPolicyEnabled: true
      deleteRetentionPolicyDays: 9
      deleteRetentionPolicyEnabled: true
      containerDeleteRetentionPolicyDays: 10
      containerDeleteRetentionPolicyEnabled: true
      containers: [
        for container in blobContainerCollection: {
          name: container.name
          publicAccess: container.publicAccess
        }
      ]
    }
  }
}

output name string = storageAccount.outputs.name
output resourceId string = storageAccount.outputs.resourceId
