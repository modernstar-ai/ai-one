@description('Name of the Service Bus')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('The SKU of the Service Bus namespace. Default is Standard.')
param sku string = 'Standard'

@description('Resource ID of the virtual network to link the private DNS zones.')
param virtualNetworkResourceId string = ''

@description('Resource ID of the subnet for the private endpoint.')
param virtualNetworkSubnetResourceId string = ''

@description('Resource ID of the Log Analytics workspace to use for diagnostic settings.')
param logAnalyticsWorkspaceResourceId string

@description('Specifies whether network isolation is enabled. This will create a private endpoint for the Service Bus and link the private DNS zone.')
param networkIsolation bool = false

@description('Optional. Array of role assignments to create.')
param roleAssignments roleAssignmentType[] = []

import { roleAssignmentType } from 'br/public:avm/utl/types/avm-common-types:0.5.1'

module privateDnsZone 'br/public:avm/res/network/private-dns-zone:0.7.0' = if (networkIsolation) {
  name: 'private-dns-servicebus-deployment'
  params: {
    name: 'privatelink.servicebus.windows.net'
    virtualNetworkLinks: [
      {
        virtualNetworkResourceId: virtualNetworkResourceId
      }
    ]
    tags: tags
  }
}

module serviceBus 'br/public:avm/res/service-bus/namespace:0.14.0' = {
  name: take('${take(toLower(name), 50)}-servicebus-deployment', 64)
  params: {
    name: name
    location: location
    tags: tags
    disableLocalAuth: false
    publicNetworkAccess: networkIsolation ? 'Disabled' : 'Enabled'
    skuObject: {
      name: sku
    }
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
  }
}

resource networkRuleSet 'Microsoft.ServiceBus/namespaces/networkRuleSets@2022-10-01-preview' = {
  name: '${name}/default'
  dependsOn: [
    serviceBus
  ]
  properties: {
    trustedServiceAccessEnabled: true
  }
}

output name string = serviceBus.outputs.name
output resourceId string = serviceBus.outputs.resourceId
