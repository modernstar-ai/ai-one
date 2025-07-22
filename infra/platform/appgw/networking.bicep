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

@description('Address prefix for the Application Gateway subnet.')
param appGwSubnetAddressPrefix string = '10.3.11.0/24'

var appGwNsgName = 'appgw-nsg'

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

output nsgId string = appGwNsg.outputs.nsgId
output nsgName string = appGwNsg.outputs.nsgName
output subnetId string = appGwSubnet.outputs.subnetId
output subnetName string = virtualNetworkSubnetName
