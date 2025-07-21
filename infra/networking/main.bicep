@description('Azure region for resource deployment')
param location string

@minLength(3)
@maxLength(12)
@description('The name of the solution.')
param projectName string

@minLength(1)
@maxLength(4)
@description('The type of environment. e.g. local, dev, uat, prod.')
param environmentName string

@description('Tags to apply to all resources.')
param tags object = {}

@description('Resource prefix for naming resources')
param resourcePrefix string = toLower('${projectName}-${environmentName}')

@description('Virtual network configuration')
param vnetConfig object = {
  name: toLower('${resourcePrefix}-vnet')
  addressPrefixes: '10.0.0.0/8'
  vmSubnet: {
    name: 'VmSubnet'
    addressPrefix: '10.3.1.0/24'
  }
  keyVaultSubnet: {
    name: 'KeyVaultSubnet'
    addressPrefix: '10.3.3.0/24'
  }
  storageSubnet: {
    name: 'StorageSubnet'
    addressPrefix: '10.3.4.0/24'
  }
  cosmosDbSubnet: {
    name: 'CosmosDbSubnet'
    addressPrefix: '10.3.5.0/24'
  }
  aiSearchSubnet: {
    name: 'AiSearchSubnet'
    addressPrefix: '10.3.6.0/24'
  }
  serviceBusSubnet: {
    name: 'ServiceBusSubnet'
    addressPrefix: '10.3.7.0/24'
  }
  cognitiveServiceSubnet: {
    name: 'CognitiveServiceSubnet'
    addressPrefix: '10.3.9.0/24'
  }
  appServiceSubnet: {
    name: 'AppServiceSubnet'
    addressPrefix: '10.3.8.0/24'
    delegation: {
      name: 'Microsoft.Web/serverFarms'
      serviceName: 'Microsoft.Web/serverFarms'
    }
  }
  privateEndpointsSubnet: {
    name: 'PrivateEndpointsSubnet'
    addressPrefix: '10.3.10.0/24'
    delegation: {
      name: 'Microsoft.Web/serverFarms'
      serviceName: 'Microsoft.Web/serverFarms'
    }
  }
  appGatewaySubnet: {
    name: 'AppGatewaySubnet'
    addressPrefix: '10.3.11.0/24'
  }
}

@description('Network Security Group configuration')
param nsgConfig object = {
  vmNsgName: toLower('${resourcePrefix}-vm-nsg')
  keyVaultNsgName: toLower('${resourcePrefix}-keyvault-nsg')
  storageNsgName: toLower('${resourcePrefix}-storage-nsg')
  cosmosDbNsgName: toLower('${resourcePrefix}-cosmosdb-nsg')
  aiSearchNsgName: toLower('${resourcePrefix}-aisearch-nsg')
  serviceBusNsgName: toLower('${resourcePrefix}-servicebus-nsg')
  cognitiveServiceNsgName: toLower('${resourcePrefix}-cognitive-nsg')
  appServiceNsgName: toLower('${resourcePrefix}-appservice-nsg')
  appGatewayNsgName: toLower('${resourcePrefix}-appgw-nsg')
  eventGridNsgName: toLower('${resourcePrefix}-eventgrid-nsg')
  allowedIpAddress: ''
}

@description('Enable diagnostic settings')
param enableDiagnostics bool = true

@description('Log Analytics Workspace ID')
param logAnalyticsWorkspaceResourceId string = ''

module vmNsg '../modules/networking/nsg.bicep' = {
  name: 'vmNsg'
  params: {
    location: location
    nsgName: nsgConfig.vmNsgName
    securityRules: []
    tags: tags
  }
}

module keyVaultNsg '../modules/networking/nsg.bicep' = {
  name: 'keyVaultNsg'
  params: {
    location: location
    nsgName: nsgConfig.keyVaultNsgName
    securityRules: []
    tags: tags
  }
}

module storageNsg '../modules/networking/nsg.bicep' = {
  name: 'storageNsg'
  params: {
    location: location
    nsgName: nsgConfig.storageNsgName
    securityRules: []
    tags: tags
  }
}

module cosmosDbNsg '../modules/networking/nsg.bicep' = {
  name: 'cosmosDbNsg'
  params: {
    location: location
    nsgName: nsgConfig.cosmosDbNsgName
    securityRules: []
    tags: tags
  }
}

module aiSearchNsg '../modules/networking/nsg.bicep' = {
  name: 'aiSearchNsg'
  params: {
    location: location
    nsgName: nsgConfig.aiSearchNsgName
    securityRules: []
    tags: tags
  }
}

module serviceBusNsg '../modules/networking/nsg.bicep' = {
  name: 'serviceBusNsg'
  params: {
    location: location
    nsgName: nsgConfig.serviceBusNsgName
    securityRules: []
    tags: tags
  }
}

module cognitiveServiceNsg '../modules/networking/nsg.bicep' = {
  name: 'cognitiveServiceNsg'
  params: {
    location: location
    nsgName: nsgConfig.cognitiveServiceNsgName
    securityRules: []
    tags: tags
  }
}

module appServiceNsg '../modules/networking/nsg.bicep' = {
  name: 'appServiceNsg'
  params: {
    location: location
    nsgName: nsgConfig.appServiceNsgName
    securityRules: []
    tags: tags
  }
}

module appGatewayNsg '../modules/networking/nsg.bicep' = {
  name: 'appGatewayNsg'
  params: {
    location: location
    nsgName: nsgConfig.appGatewayNsgName
    securityRules: []
    tags: tags
  }
}

module vnet '../modules/networking/vnet.bicep' = {
  name: 'vnet'
  params: {
    location: location
    virtualNetworkName: vnetConfig.name
    virtualNetworkAddressPrefixes: vnetConfig.addressPrefixes
    vmSubnet: vnetConfig.vmSubnet
    keyVaultSubnet: vnetConfig.keyVaultSubnet
    storageSubnet: vnetConfig.storageSubnet
    cosmosDbSubnet: vnetConfig.cosmosDbSubnet
    aiSearchSubnet: vnetConfig.aiSearchSubnet
    serviceBusSubnet: vnetConfig.serviceBusSubnet
    cognitiveServiceSubnet: vnetConfig.cognitiveServiceSubnet
    appServiceSubnet: vnetConfig.appServiceSubnet
    privateEndpointsSubnet: vnetConfig.privateEndpointsSubnet
    appGatewaySubnet: vnetConfig.appGatewaySubnet
    vmSubnetNsgId: vmNsg.outputs.nsgId
    keyVaultSubnetNsgId: keyVaultNsg.outputs.nsgId
    storageSubnetNsgId: storageNsg.outputs.nsgId
    cosmosDbSubnetNsgId: cosmosDbNsg.outputs.nsgId
    aiSearchSubnetNsgId: aiSearchNsg.outputs.nsgId
    serviceBusSubnetNsgId: serviceBusNsg.outputs.nsgId
    cognitiveServiceSubnetNsgId: cognitiveServiceNsg.outputs.nsgId
    appServiceSubnetNsgId: appServiceNsg.outputs.nsgId
    appGatewaySubnetNsgId: appGatewayNsg.outputs.nsgId
    tags: tags
  }
}

module vmNsgDiagnostics '../modules/networking/diagnostic-settings.bicep' = if (enableDiagnostics) {
  name: 'vmNsgDiagnostics'
  params: {
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceResourceId
    targetResourceName: vmNsg.outputs.nsgName
    resourceType: 'nsg'
  }
}

module keyVaultNsgDiagnostics '../modules/networking/diagnostic-settings.bicep' = if (enableDiagnostics) {
  name: 'keyVaultNsgDiagnostics'
  params: {
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceResourceId
    targetResourceName: keyVaultNsg.outputs.nsgName
    resourceType: 'nsg'
  }
}

module vnetDiagnostics '../modules/networking/diagnostic-settings.bicep' = if (enableDiagnostics) {
  name: 'vnetDiagnostics'
  params: {
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceResourceId
    targetResourceName: vnet.outputs.virtualNetworkName
    resourceType: 'vnet'
  }
}

output virtualNetworkId string = vnet.outputs.virtualNetworkId
output virtualNetworkName string = vnet.outputs.virtualNetworkName
output vmSubnetId string = vnet.outputs.vmSubnetId
output keyVaultSubnetId string = vnet.outputs.keyVaultSubnetId
output storageSubnetId string = vnet.outputs.storageSubnetId
output cosmosDbSubnetId string = vnet.outputs.cosmosDbSubnetId
output aiSearchSubnetId string = vnet.outputs.aiSearchSubnetId
output serviceBusSubnetId string = vnet.outputs.serviceBusSubnetId
output cognitiveServiceSubnetId string = vnet.outputs.cognitiveServiceSubnetId
output appServiceSubnetId string = vnet.outputs.appServiceSubnetId
output privateEndpointsSubnetId string = vnet.outputs.privateEndpointsSubnetId
output vmSubnetName string = vnet.outputs.vmSubnetName
output keyVaultSubnetName string = vnet.outputs.keyVaultSubnetName
output storageSubnetName string = vnet.outputs.storageSubnetName
output cosmosDbSubnetName string = vnet.outputs.cosmosDbSubnetName
output aiSearchSubnetName string = vnet.outputs.aiSearchSubnetName
output serviceBusSubnetName string = vnet.outputs.serviceBusSubnetName
output cognitiveServiceSubnetName string = vnet.outputs.cognitiveServiceSubnetName
output appServiceSubnetName string = vnet.outputs.appServiceSubnetName
output privateEndpointsSubnetName string = vnet.outputs.privateEndpointSubnetName
output vmNsgId string = vmNsg.outputs.nsgId
output keyVaultNsgId string = keyVaultNsg.outputs.nsgId
output storageNsgId string = storageNsg.outputs.nsgId
output cosmosDbNsgId string = cosmosDbNsg.outputs.nsgId
output aiSearchNsgId string = aiSearchNsg.outputs.nsgId
output serviceBusNsgId string = serviceBusNsg.outputs.nsgId
output cognitiveServiceNsgId string = cognitiveServiceNsg.outputs.nsgId
output appServiceNsgId string = appServiceNsg.outputs.nsgId
