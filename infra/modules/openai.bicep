@description('Name of the Azure OpenAI resource.')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('SKU for Azure OpenAI resource')
param skuName string = 'S0'

@description('Resource ID of the Log Analytics workspace to use for diagnostic settings.')
param logAnalyticsWorkspaceResourceId string

@description('Specifies whether network isolation is enabled. This will create a private endpoint for the resource and link the private DNS zone.')
param networkIsolation bool = false

@description('Resource ID of the virtual network to link the private DNS zones.')
param virtualNetworkResourceId string = ''

@description('Resource ID of the subnet for the private endpoint.')
param virtualNetworkSubnetResourceId string = ''

import { roleAssignmentType } from 'br/public:avm/utl/types/avm-common-types:0.5.1'
@description('Optional. Array of role assignments to create.')
param roleAssignments roleAssignmentType[] = []

import { deploymentsType } from './customTypes.bicep'
@description('Optional. Array of deployments about cognitive service accounts to create.')
param deployments deploymentsType[] = []

module privateDnsZone 'br/public:avm/res/network/private-dns-zone:0.7.0' = if (networkIsolation) {
  name: 'private-dns-cognitiveservices-deployment'
  params: {
    name: 'privatelink.cognitiveservices.azure.com'
    virtualNetworkLinks: [
      {
        virtualNetworkResourceId: virtualNetworkResourceId
      }
    ]
    tags: tags
  }
}

module openAi 'br/public:avm/res/cognitive-services/account:0.10.2' = {
  name: take('${take(toLower(name), 24)}-openai-deployment', 64)
  dependsOn: [privateDnsZone]
  params: {
    name: name
    location: location
    tags: tags
    kind: 'OpenAI'
    sku: skuName
    disableLocalAuth: false
    customSubDomainName: name
    publicNetworkAccess: networkIsolation ? 'Disabled' : 'Enabled'
    managedIdentities: {
      systemAssigned: true
    }
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
            subnetResourceId: virtualNetworkSubnetResourceId
          }
        ]
      : []
    roleAssignments: roleAssignments
    deployments: deployments ?? []
  }
}

output name string = openAi.outputs.name
output resourceId string = openAi.outputs.resourceId
output endpoint string = openAi.outputs.endpoint
