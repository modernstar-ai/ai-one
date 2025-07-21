@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('Name of the virtual network where the Application Gateway subnet will be created.')
@minLength(1)
param virtualNetworkName string

@description('Resource ID of the subnet for the private endpoint.')
param virtualNetworkSubnetName string = 'AppGatewaySubnet'

@description('Resource ID of the Log Analytics workspace to use for diagnostic settings.')
param logAnalyticsWorkspaceResourceId string

@description('Enable diagnostic settings')
param enableDiagnostics bool = true

var appGwNsgName = 'appgw-nsg'
// Create subnet for Application Gateway
var appGwSubnetAddressPrefix = '10.3.12.0/24'

// Create NSG for Application Gateway subnet
module appGwNsg '../../modules/networking/nsg.bicep' = {
  name: 'appGwNsg'
  params: {
    location: location
    nsgName: appGwNsgName
    tags: tags
    securityRules: [
      // {
      //   name: 'Allow-GatewayManager'
      //   properties: {
      //     priority: 100
      //     protocol: '*'
      //     access: 'Allow'
      //     direction: 'Inbound'
      //     sourceAddressPrefix: 'GatewayManager'
      //     sourcePortRange: '*'
      //     destinationAddressPrefix: '*'
      //     destinationPortRange: '65200-65535'
      //   }
      // }
      // {
      //   name: 'Allow-AzureLoadBalancer'
      //   properties: {
      //     priority: 110
      //     protocol: '*'
      //     access: 'Allow'
      //     direction: 'Inbound'
      //     sourceAddressPrefix: 'AzureLoadBalancer'
      //     sourcePortRange: '*'
      //     destinationAddressPrefix: '*'
      //     destinationPortRange: '*'
      //   }
      // }
      // {
      //   name: 'Allow-Internet-In'
      //   properties: {
      //     priority: 120
      //     protocol: '*'
      //     access: 'Allow'
      //     direction: 'Inbound'
      //     sourceAddressPrefix: 'Internet'
      //     sourcePortRange: '*'
      //     destinationAddressPrefix: '*'
      //     destinationPortRanges: ['80', '443']
      //   }
      // }
    ]
  }
}

// Add diagnostic settings for the NSG
module appGwNsgDiagnostics '../../modules/networking/diagnostic-settings.bicep' = if (enableDiagnostics) {
  name: 'appGwNsgDiagnostics'
  params: {
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceResourceId
    targetResourceName: appGwNsg.outputs.nsgName
    resourceType: 'nsg'
  }
}

module appGwSubnet '../../modules/networking/subnet.bicep' = {
  name: 'appGwSubnet'
  params: {
    virtualNetworkName: virtualNetworkName
    subnetName: virtualNetworkSubnetName
    addressPrefix: appGwSubnetAddressPrefix
    networkSecurityGroupId: appGwNsg.outputs.nsgId
    privateEndpointNetworkPolicies: 'Disabled'
    privateLinkServiceNetworkPolicies: 'Disabled'
  }
}

output appGwNsgId string = appGwNsg.outputs.nsgId
output appGwNsgName string = appGwNsg.outputs.nsgName
output appGwSubnetId string = appGwSubnet.outputs.subnetId
output appGwSubnetName string = virtualNetworkSubnetName
