@description('Name of the virtual network.')
param virtualNetworkName string

@description('Name of the subnet.')
param subnetName string

@description('Address prefix for the subnet.')
param addressPrefix string

@description('Resource ID of the network security group to associate with the subnet.')
param networkSecurityGroupId string = ''

@description('Service endpoints to enable on the subnet.')
param serviceEndpoints array = []

@description('Delegations for the subnet.')
param delegations array = []

@description('Enable or disable private endpoint network policies.')
param privateEndpointNetworkPolicies string = 'Enabled'

@description('Enable or disable private link service network policies.')
param privateLinkServiceNetworkPolicies string = 'Enabled'

resource vnet 'Microsoft.Network/virtualNetworks@2024-03-01' existing = {
  name: virtualNetworkName
  scope: resourceGroup()
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2022-11-01' = {
  name: subnetName
  parent: vnet
  properties: {
    addressPrefix: addressPrefix
    networkSecurityGroup: !empty(networkSecurityGroupId)
      ? {
          id: networkSecurityGroupId
        }
      : null
    serviceEndpoints: serviceEndpoints
    delegations: delegations
    privateEndpointNetworkPolicies: privateEndpointNetworkPolicies
    privateLinkServiceNetworkPolicies: privateLinkServiceNetworkPolicies
  }
}

output subnetId string = subnet.id
output subnetName string = subnetName
