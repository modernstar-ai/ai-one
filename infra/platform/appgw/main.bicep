@description('Name of the Application Gateway')
param name string = toLower('${resourcePrefix}-appgw')

@description('Primary location for all resources')
param location string = resourceGroup().location

@description('The name of the solution.')
@minLength(3)
@maxLength(12)
param projectName string

@description('The type of environment. e.g. local, dev, uat, prod.')
@minLength(1)
@maxLength(4)
param environmentName string

@description('Tags to apply to all resources.')
param tags object = {}

@description('Resource prefix for naming resources')
param resourcePrefix string = toLower('${projectName}-${environmentName}')

@description('Azure Virtual Network name')
param virtualNetworkName string = toLower('${resourcePrefix}-vnet')

@description('Name of the subnet where App Gateway will be deployed.')
param virtualNetworkSubnetName string = 'AppGatewaySubnet'

@description('Resource ID of the Log Analytics workspace to use for diagnostic settings.')
param logAnalyticsWorkspaceResourceId string = ''

@description('Address prefix for the Application Gateway subnet.')
param subnetAddressPrefix string

@description('Enable diagnostic settings.')
param enableDiagnostics bool = false

module networking './networking.bicep' = {
  name: 'appGwNetworking'
  params: {
    location: location
    tags: tags
    virtualNetworkName: virtualNetworkName
    virtualNetworkSubnetName: virtualNetworkSubnetName
    appGwSubnetAddressPrefix: subnetAddressPrefix
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceResourceId
    enableDiagnostics: enableDiagnostics
  }
}

resource applicationGateway 'Microsoft.Network/applicationGateways@2023-06-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'Standard_v2'
      tier: 'Standard_v2'
      capacity: 2
    }
    gatewayIPConfigurations: [
      {
        name: 'appGatewayIpConfig'
        properties: {
          subnet: {
            id: networking.outputs.subnetId
          }
        }
      }
    ]
    frontendIPConfigurations: [
      {
        name: 'appGatewayFrontendIP'
        properties: {
          privateIPAllocationMethod: 'Dynamic'
          subnet: {
            id: networking.outputs.subnetId
          }
        }
      }
    ]
    frontendPorts: [
      {
        name: 'appGatewayFrontendPort'
        properties: {
          port: 80
        }
      }
    ]
    backendAddressPools: [
      {
        name: 'appGatewayBackendPool'
        properties: {}
      }
    ]
    backendHttpSettingsCollection: [
      {
        name: 'appGatewayBackendHttpSettings'
        properties: {
          port: 80
          protocol: 'Http'
          cookieBasedAffinity: 'Disabled'
          pickHostNameFromBackendAddress: true
          requestTimeout: 20
        }
      }
    ]
    httpListeners: [
      {
        name: 'appGatewayHttpListener'
        properties: {
          frontendIPConfiguration: {
            id: resourceId(
              'Microsoft.Network/applicationGateways/frontendIPConfigurations',
              name,
              'appGatewayFrontendIP'
            )
          }
          frontendPort: {
            id: resourceId('Microsoft.Network/applicationGateways/frontendPorts', name, 'appGatewayFrontendPort')
          }
          protocol: 'Http'
        }
      }
    ]
    requestRoutingRules: [
      {
        name: 'appGatewayRoutingRule'
        properties: {
          ruleType: 'Basic'
          priority: 100
          httpListener: {
            id: resourceId('Microsoft.Network/applicationGateways/httpListeners', name, 'appGatewayHttpListener')
          }
          backendAddressPool: {
            id: resourceId('Microsoft.Network/applicationGateways/backendAddressPools', name, 'appGatewayBackendPool')
          }
          backendHttpSettings: {
            id: resourceId(
              'Microsoft.Network/applicationGateways/backendHttpSettingsCollection',
              name,
              'appGatewayBackendHttpSettings'
            )
          }
        }
      }
    ]
  }
}

output applicationGatewayId string = applicationGateway.id
output applicationGatewayName string = applicationGateway.name
