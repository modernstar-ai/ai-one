@description('Azure region for resource deployment')
param location string

@description('Virtual network name')
param virtualNetworkName string

@description('Virtual network address prefixes')
param virtualNetworkAddressPrefixes string = '10.0.0.0/8'

@description('VM subnet configuration')
param vmSubnet object = {
  name: 'VmSubnet'
  addressPrefix: '10.3.1.0/24'
}

@description('Key Vault subnet configuration')
param keyVaultSubnet object = {
  name: 'KeyVaultSubnet'
  addressPrefix: '10.3.3.0/24'
}

@description('Storage subnet configuration')
param storageSubnet object = {
  name: 'StorageSubnet'
  addressPrefix: '10.3.4.0/24'
}

@description('Cosmos DB subnet configuration')
param cosmosDbSubnet object = {
  name: 'CosmosDbSubnet'
  addressPrefix: '10.3.5.0/24'
}

@description('AI Search subnet configuration')
param aiSearchSubnet object = {
  name: 'AiSearchSubnet'
  addressPrefix: '10.3.6.0/24'
}

@description('Service Bus subnet configuration')
param serviceBusSubnet object = {
  name: 'ServiceBusSubnet'
  addressPrefix: '10.3.7.0/24'
}

@description('Cognitive Service subnet configuration')
param cognitiveServiceSubnet object = {
  name: 'CognitiveServiceSubnet'
  addressPrefix: '10.3.9.0/24'
}

@description('App Service subnet configuration')
param appServiceSubnet object = {
  name: 'AppServiceSubnet'
  addressPrefix: '10.3.8.0/24'
}

@description('App Service subnet V2 configuration')
param appServiceSubnetV2 object = {
  name: 'AppServiceSubnetV2'
  addressPrefix: '10.3.10.0/24'
}

param appGatewaySubnet object = {
  name: 'AppGatewaySubnet'
  addressPrefix: '10.3.11.0/24'
}

@description('VM subnet NSG ID')
param vmSubnetNsgId string

@description('Key Vault subnet NSG ID')
param keyVaultSubnetNsgId string

@description('Storage subnet NSG ID')
param storageSubnetNsgId string

@description('Cosmos DB subnet NSG ID')
param cosmosDbSubnetNsgId string

@description('AI Search subnet NSG ID')
param aiSearchSubnetNsgId string

@description('Service Bus subnet NSG ID')
param serviceBusSubnetNsgId string

@description('Cognitive Service subnet NSG ID')
param cognitiveServiceSubnetNsgId string

@description('App Service subnet NSG ID')
param appServiceSubnetNsgId string = 'AppServiceSubnetNsg'

@description('App Service subnet V2 NSG ID')
param appServiceSubnetV2NsgId string = 'AppServiceSubnetV2Nsg'

@description('App Gateway subnet configuration')
param appGatewaySubnetNsgId string = 'AppGatewaySubnetNsg'

@description('Resource tags')
param tags object = {}

@description('Enable private endpoint network policies')
param enablePrivateEndpointNetworkPolicies bool = false

@description('Enable private link service network policies')
param enablePrivateLinkServiceNetworkPolicies bool = false

// Virtual Network
resource vnet 'Microsoft.Network/virtualNetworks@2024-03-01' = {
  name: virtualNetworkName
  location: location
  tags: tags
  properties: {
    addressSpace: {
      addressPrefixes: [
        virtualNetworkAddressPrefixes
      ]
    }
    subnets: [
      {
        name: vmSubnet.name
        properties: {
          addressPrefix: vmSubnet.addressPrefix
          networkSecurityGroup: {
            id: vmSubnetNsgId
          }
          privateEndpointNetworkPolicies: enablePrivateEndpointNetworkPolicies ? 'Enabled' : 'Disabled'
          privateLinkServiceNetworkPolicies: enablePrivateLinkServiceNetworkPolicies ? 'Enabled' : 'Disabled'
        }
      }
      {
        name: keyVaultSubnet.name
        properties: {
          addressPrefix: keyVaultSubnet.addressPrefix
          networkSecurityGroup: {
            id: keyVaultSubnetNsgId
          }
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Disabled'
        }
      }
      {
        name: storageSubnet.name
        properties: {
          addressPrefix: storageSubnet.addressPrefix
          networkSecurityGroup: {
            id: storageSubnetNsgId
          }
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Disabled'
        }
      }
      {
        name: cosmosDbSubnet.name
        properties: {
          addressPrefix: cosmosDbSubnet.addressPrefix
          networkSecurityGroup: {
            id: cosmosDbSubnetNsgId
          }
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Disabled'
        }
      }
      {
        name: aiSearchSubnet.name
        properties: {
          addressPrefix: aiSearchSubnet.addressPrefix
          networkSecurityGroup: {
            id: aiSearchSubnetNsgId
          }
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Disabled'
        }
      }
      {
        name: serviceBusSubnet.name
        properties: {
          addressPrefix: serviceBusSubnet.addressPrefix
          networkSecurityGroup: {
            id: serviceBusSubnetNsgId
          }
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Disabled'
        }
      }
      {
        name: cognitiveServiceSubnet.name
        properties: {
          addressPrefix: cognitiveServiceSubnet.addressPrefix
          networkSecurityGroup: {
            id: cognitiveServiceSubnetNsgId
          }
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Disabled'
        }
      }
      {
        name: appServiceSubnet.name
        properties: {
          addressPrefix: appServiceSubnet.addressPrefix
          networkSecurityGroup: {
            id: appServiceSubnetNsgId
          }
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Disabled'
          delegations: [
            {
              name: 'MicrosoftWebServerFarmsDelegation'
              properties: {
                serviceName: 'Microsoft.Web/serverFarms'
              }
            }
          ]
        }
      }
      {
        name: appServiceSubnetV2.name
        properties: {
          addressPrefix: appServiceSubnetV2.addressPrefix
          networkSecurityGroup: {
            id: appServiceSubnetV2NsgId
          }
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Disabled'
          // No delegation by default, add if needed
        }
      }
      {
        name: appGatewaySubnet.name
        properties: {
          addressPrefix: appGatewaySubnet.addressPrefix
          networkSecurityGroup: {
            id: appGatewaySubnetNsgId
          }
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Disabled'
        }
      }
    ]
  }
}

// Outputs
output virtualNetworkId string = vnet.id
output virtualNetworkName string = vnet.name
output vmSubnetId string = resourceId('Microsoft.Network/virtualNetworks/subnets', vnet.name, vmSubnet.name)
output vmSubnetName string = vmSubnet.name
output addressSpace array = vnet.properties.addressSpace.addressPrefixes

// Platform subnet outputs
output keyVaultSubnetId string = resourceId('Microsoft.Network/virtualNetworks/subnets', vnet.name, keyVaultSubnet.name)
output storageSubnetId string = resourceId('Microsoft.Network/virtualNetworks/subnets', vnet.name, storageSubnet.name)
output cosmosDbSubnetId string = resourceId('Microsoft.Network/virtualNetworks/subnets', vnet.name, cosmosDbSubnet.name)
output aiSearchSubnetId string = resourceId('Microsoft.Network/virtualNetworks/subnets', vnet.name, aiSearchSubnet.name)
output serviceBusSubnetId string = resourceId(
  'Microsoft.Network/virtualNetworks/subnets',
  vnet.name,
  serviceBusSubnet.name
)
output cognitiveServiceSubnetId string = resourceId(
  'Microsoft.Network/virtualNetworks/subnets',
  vnet.name,
  cognitiveServiceSubnet.name
)
output appServiceSubnetId string = resourceId(
  'Microsoft.Network/virtualNetworks/subnets',
  vnet.name,
  appServiceSubnet.name
)
output appServiceSubnetV2Id string = resourceId(
  'Microsoft.Network/virtualNetworks/subnets',
  vnet.name,
  appServiceSubnetV2.name
)

output appGatewaySubnetId string = resourceId(
  'Microsoft.Network/virtualNetworks/subnets',
  vnet.name,
  appGatewaySubnet.name
)

// Platform subnet name outputs
output keyVaultSubnetName string = keyVaultSubnet.name
output storageSubnetName string = storageSubnet.name
output cosmosDbSubnetName string = cosmosDbSubnet.name
output aiSearchSubnetName string = aiSearchSubnet.name
output serviceBusSubnetName string = serviceBusSubnet.name
output cognitiveServiceSubnetName string = cognitiveServiceSubnet.name
output appServiceSubnetName string = appServiceSubnet.name
output appServiceSubnetV2Name string = appServiceSubnetV2.name
output appGatewaySubnetName string = appGatewaySubnet.name
