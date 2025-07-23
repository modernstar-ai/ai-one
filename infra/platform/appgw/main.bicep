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
param logAnalyticsWorkspaceResourceId string

@description('Address prefix for the Application Gateway subnet.')
param subnetAddressPrefix string

@description('Enable diagnostic settings.')
param enableDiagnostics bool = false

@description('Enable WAF (Web Application Firewall)')
param enableWaf bool = true

@description('WAF mode: Detection or Prevention')
@allowed(['Detection', 'Prevention'])
param wafMode string = 'Prevention'

@description('WAF rule set type')
param wafRuleSetType string = 'OWASP'

@description('WAF rule set version')
param wafRuleSetVersion string = '3.2'

@description('Key Vault name for SSL certificate')
param keyVaultName string = ''

@description('Name of the SSL certificate secret in Key Vault')
param sslCertificateSecretName string

@description('Enable HTTPS with Key Vault certificate')
param enableHttps bool = false

// Calculate a default private IP address based on the subnet
var calculatedPrivateIP = cidrHost(subnetAddressPrefix, 4)

// Reference existing Key Vault (if provided)
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = if (!empty(keyVaultName)) {
  name: keyVaultName
}

// Create user-assigned managed identity for Application Gateway
resource appGatewayManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${name}-identity'
  location: location
  tags: tags
}

// Role assignment for Key Vault Secrets User (if Key Vault is provided)
resource keyVaultSecretsUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(keyVaultName)) {
  name: guid(resourceGroup().id, keyVault.id, appGatewayManagedIdentity.id, '4633458b-17de-408a-b874-0445c86b69e6')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '4633458b-17de-408a-b874-0445c86b69e6'
    ) // Key Vault Secrets User
    principalId: appGatewayManagedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Role assignment for Key Vault Certificate User (if Key Vault is provided)
resource keyVaultCertificateUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(keyVaultName)) {
  name: guid(resourceGroup().id, keyVault.id, appGatewayManagedIdentity.id, 'db79e9a7-68ee-4b58-9aeb-b90e7c24fcba')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      'db79e9a7-68ee-4b58-9aeb-b90e7c24fcba'
    ) // Key Vault Certificate User
    principalId: appGatewayManagedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Create a public IP for the Application Gateway
resource publicIP 'Microsoft.Network/publicIPAddresses@2023-09-01' = {
  name: '${name}-pip'
  location: location
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Regional'
  }
  properties: {
    publicIPAllocationMethod: 'Static'
    publicIPAddressVersion: 'IPv4'
    idleTimeoutInMinutes: 4
  }
}

module networking './networking.bicep' = {
  name: 'appGwNetworking'
  params: {
    location: location
    tags: tags
    virtualNetworkName: virtualNetworkName
    virtualNetworkSubnetName: virtualNetworkSubnetName
    subnetAddressPrefix: subnetAddressPrefix
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceResourceId
    enableDiagnostics: enableDiagnostics
  }
}

resource applicationGateway 'Microsoft.Network/applicationGateways@2023-09-01' = {
  name: name
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${appGatewayManagedIdentity.id}': {}
    }
  }
  properties: {
    sku: {
      name: enableWaf ? 'WAF_v2' : 'Standard_v2'
      tier: enableWaf ? 'WAF_v2' : 'Standard_v2'
    }
    autoscaleConfiguration: {
      minCapacity: 2
      maxCapacity: 10
    }
    webApplicationFirewallConfiguration: enableWaf
      ? {
          enabled: true
          firewallMode: wafMode
          ruleSetType: wafRuleSetType
          ruleSetVersion: wafRuleSetVersion
          disabledRuleGroups: []
          requestBodyCheck: true
          maxRequestBodySizeInKb: 128
          fileUploadLimitInMb: 100
        }
      : null
    sslCertificates: enableHttps && !empty(keyVault.id) && !empty(sslCertificateSecretName)
      ? [
          {
            name: 'ssl-certificate-from-keyvault'
            properties: {
              keyVaultSecretId: '${keyVault.id}/secrets/${sslCertificateSecretName}'
            }
          }
        ]
      : []
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
          publicIPAddress: {
            id: publicIP.id
          }
        }
      }
      {
        name: 'appGatewayPrivateFrontendIP'
        properties: {
          privateIPAllocationMethod: 'Static'
          privateIPAddress: calculatedPrivateIP
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
      {
        name: 'appGatewayFrontendPortHttps'
        properties: {
          port: 443
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
      {
        name: 'appGatewayBackendHttpsSettings'
        properties: {
          port: 443
          protocol: 'Https'
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
      {
        name: 'appGatewayHttpsListener'
        properties: {
          frontendIPConfiguration: {
            id: resourceId(
              'Microsoft.Network/applicationGateways/frontendIPConfigurations',
              name,
              'appGatewayFrontendIP'
            )
          }
          frontendPort: {
            id: resourceId('Microsoft.Network/applicationGateways/frontendPorts', name, 'appGatewayFrontendPortHttps')
          }
          protocol: 'Https'
          sslCertificate: enableHttps && !empty(keyVault.id) && !empty(sslCertificateSecretName)
            ? {
                id: resourceId(
                  'Microsoft.Network/applicationGateways/sslCertificates',
                  name,
                  'ssl-certificate-from-keyvault'
                )
              }
            : null
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
      {
        name: 'appGatewayHttpsRoutingRule'
        properties: {
          ruleType: 'Basic'
          priority: 200
          httpListener: {
            id: resourceId('Microsoft.Network/applicationGateways/httpListeners', name, 'appGatewayHttpsListener')
          }
          backendAddressPool: {
            id: resourceId('Microsoft.Network/applicationGateways/backendAddressPools', name, 'appGatewayBackendPool')
          }
          backendHttpSettings: {
            id: resourceId(
              'Microsoft.Network/applicationGateways/backendHttpSettingsCollection',
              name,
              'appGatewayBackendHttpsSettings'
            )
          }
        }
      }
    ]
  }
}

output applicationGatewayName string = applicationGateway.name
output publicIPAddress string = publicIP.properties.ipAddress
