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
param enableDiagnostics bool = true

@description('Enable WAF (Web Application Firewall)')
param enableWaf bool = true

@description('WAF mode: Detection or Prevention')
@allowed(['Detection', 'Prevention'])
param wafMode string = 'Detection'

@description('WAF rule set type')
param wafRuleSetType string = 'OWASP'

@description('WAF rule set version')
param wafRuleSetVersion string = '3.2'

@description('Key Vault name for SSL certificate')
param keyVaultName string

@description('Name of the SSL certificate secret in Key Vault')
param sslCertificateSecretName string

@description('Backend App Service name (private network)')
param webAppServiceName string = ''

@description('Backend App Service custom domain (if using custom domain)')
param webAppServiceDomain string = ''

@description('Backend API App Service name (private network)')
param apiAppServiceName string = ''

@description('Backend API App Service custom domain (if using custom domain)')
param apiAppServiceDomain string = ''

// Calculate a default private IP address based on the subnet
var calculatedPrivateIP = cidrHost(subnetAddressPrefix, 4)

// Reference existing Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// Create user-assigned managed identity for Application Gateway
resource appGatewayManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${name}-identity'
  location: location
  tags: tags
}

// Role assignment for Key Vault Secrets User
resource keyVaultSecretsUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
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

// Role assignment for Key Vault Certificate User
resource keyVaultCertificateUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
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
    sslCertificates: [
      {
        name: 'ssl-certificate-from-keyvault'
        properties: {
          keyVaultSecretId: '${keyVault.properties.vaultUri}secrets/${sslCertificateSecretName}'
        }
      }
    ]
    probes: [
      {
        name: 'webAppHealthProbe'
        properties: {
          protocol: 'Https'
          path: '/'
          interval: 30
          timeout: 30
          unhealthyThreshold: 3
          pickHostNameFromBackendHttpSettings: true
          minServers: 0
          match: {
            statusCodes: ['200-399']
          }
        }
      }
      {
        name: 'apiAppHealthProbe'
        properties: {
          protocol: 'Https'
          path: '/swagger'
          interval: 30
          timeout: 30
          unhealthyThreshold: 3
          pickHostNameFromBackendHttpSettings: true
          minServers: 0
          match: {
            statusCodes: ['200-399']
          }
        }
      }
    ]
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
        name: 'appGatewayAiOneWebBackendPool'
        properties: {
          backendAddresses: !empty(webAppServiceName)
            ? [
                {
                  fqdn: !empty(webAppServiceDomain) ? webAppServiceDomain : '${webAppServiceName}.azurewebsites.net'
                }
              ]
            : []
        }
      }
      {
        name: 'appGatewayAiOneApiBackendPool'
        properties: {
          backendAddresses: !empty(apiAppServiceName)
            ? [
                {
                  fqdn: !empty(apiAppServiceDomain) ? apiAppServiceDomain : '${apiAppServiceName}.azurewebsites.net'
                }
              ]
            : []
        }
      }
    ]
    backendHttpSettingsCollection: [
      {
        name: 'appGatewayAiOneWebBackendHttpSettings'
        properties: {
          port: 443
          protocol: 'Https'
          cookieBasedAffinity: 'Disabled'
          pickHostNameFromBackendAddress: false
          hostName: !empty(webAppServiceDomain) ? webAppServiceDomain : '${webAppServiceName}.azurewebsites.net'
          requestTimeout: 20
          connectionDraining: {
            enabled: true
            drainTimeoutInSec: 120
          }
          probe: {
            id: resourceId('Microsoft.Network/applicationGateways/probes', name, 'webAppHealthProbe')
          }
          affinityCookieName: 'ApplicationGatewayAffinity'
        }
      }
      {
        name: 'appGatewayAiOneApiBackendHttpSettings'
        properties: {
          port: 443
          protocol: 'Https'
          cookieBasedAffinity: 'Disabled'
          pickHostNameFromBackendAddress: false
          hostName: !empty(apiAppServiceDomain) ? apiAppServiceDomain : '${apiAppServiceName}.azurewebsites.net'
          requestTimeout: 30
          connectionDraining: {
            enabled: true
            drainTimeoutInSec: 120
          }
          probe: {
            id: resourceId('Microsoft.Network/applicationGateways/probes', name, 'apiAppHealthProbe')
          }
          affinityCookieName: 'ApplicationGatewayAffinity'
        }
      }
    ]
    rewriteRuleSets: [
      {
        name: 'webAppRewriteRuleSet'
        properties: {
          rewriteRules: [
            {
              ruleSequence: 100
              name: 'stripWebAppPath'
              conditions: [
                {
                  variable: 'var_uri_path'
                  pattern: '^/aione/webapp/(.*)'
                  ignoreCase: true
                }
              ]
              actionSet: {
                urlConfiguration: {
                  modifiedPath: '/{var_uri_path_1}'
                  reroute: false
                }
                requestHeaderConfigurations: [
                  {
                    headerName: 'X-Forwarded-Host'
                    headerValue: '{var_host}'
                  }
                  {
                    headerName: 'X-Forwarded-Proto'
                    headerValue: 'https'
                  }
                  {
                    headerName: 'X-Original-URL'
                    headerValue: '{var_uri_path}'
                  }
                  {
                    headerName: 'X-Forwarded-Prefix'
                    headerValue: '/aione/webapp'
                  }
                ]
              }
            }
          ]
        }
      }
      {
        name: 'apiRewriteRuleSet'
        properties: {
          rewriteRules: [
            {
              ruleSequence: 100
              name: 'stripApiPath'
              conditions: [
                {
                  variable: 'var_uri_path'
                  pattern: '^/aione/apiapp/(.*)'
                  ignoreCase: true
                }
              ]
              actionSet: {
                urlConfiguration: {
                  modifiedPath: '/{var_uri_path_1}'
                  reroute: false
                }
                requestHeaderConfigurations: [
                  {
                    headerName: 'X-Forwarded-Host'
                    headerValue: '{var_host}'
                  }
                  {
                    headerName: 'X-Forwarded-Proto'
                    headerValue: 'https'
                  }
                  {
                    headerName: 'X-Original-URL'
                    headerValue: '{var_uri_path}'
                  }
                  {
                    headerName: 'X-Forwarded-Prefix'
                    headerValue: '/aione/apiapp'
                  }
                  {
                    headerName: 'X-Forwarded-For'
                    headerValue: '{var_client_ip}'
                  }
                ]
              }
            }
          ]
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
          sslCertificate: {
            id: resourceId(
              'Microsoft.Network/applicationGateways/sslCertificates',
              name,
              'ssl-certificate-from-keyvault'
            )
          }
        }
      }
    ]
    requestRoutingRules: [
      {
        name: 'appGatewayRoutingRule'
        properties: {
          ruleType: 'PathBasedRouting'
          priority: 100
          httpListener: {
            id: resourceId('Microsoft.Network/applicationGateways/httpListeners', name, 'appGatewayHttpListener')
          }
          urlPathMap: {
            id: resourceId('Microsoft.Network/applicationGateways/urlPathMaps', name, 'appGatewayUrlPathMap')
          }
        }
      }
      {
        name: 'appGatewayHttpsRoutingRule'
        properties: {
          ruleType: 'PathBasedRouting'
          priority: 200
          httpListener: {
            id: resourceId('Microsoft.Network/applicationGateways/httpListeners', name, 'appGatewayHttpsListener')
          }
          urlPathMap: {
            id: resourceId('Microsoft.Network/applicationGateways/urlPathMaps', name, 'appGatewayUrlPathMap')
          }
        }
      }
    ]
    urlPathMaps: [
      {
        name: 'appGatewayUrlPathMap'
        properties: {
          defaultBackendAddressPool: {
            id: resourceId(
              'Microsoft.Network/applicationGateways/backendAddressPools',
              name,
              'appGatewayAiOneWebBackendPool'
            )
          }
          defaultBackendHttpSettings: {
            id: resourceId(
              'Microsoft.Network/applicationGateways/backendHttpSettingsCollection',
              name,
              'appGatewayAiOneWebBackendHttpSettings'
            )
          }
          pathRules: [
            {
              name: 'apiPathRule'
              properties: {
                paths: ['/aione/apiapp/*']
                backendAddressPool: {
                  id: resourceId(
                    'Microsoft.Network/applicationGateways/backendAddressPools',
                    name,
                    'appGatewayAiOneApiBackendPool'
                  )
                }
                backendHttpSettings: {
                  id: resourceId(
                    'Microsoft.Network/applicationGateways/backendHttpSettingsCollection',
                    name,
                    'appGatewayAiOneApiBackendHttpSettings'
                  )
                }
                rewriteRuleSet: {
                  id: resourceId('Microsoft.Network/applicationGateways/rewriteRuleSets', name, 'apiRewriteRuleSet')
                }
              }
            }
            {
              name: 'webAppPathRule'
              properties: {
                paths: ['/aione/webapp/*']
                backendAddressPool: {
                  id: resourceId(
                    'Microsoft.Network/applicationGateways/backendAddressPools',
                    name,
                    'appGatewayAiOneWebBackendPool'
                  )
                }
                backendHttpSettings: {
                  id: resourceId(
                    'Microsoft.Network/applicationGateways/backendHttpSettingsCollection',
                    name,
                    'appGatewayAiOneWebBackendHttpSettings'
                  )
                }
                rewriteRuleSet: {
                  id: resourceId('Microsoft.Network/applicationGateways/rewriteRuleSets', name, 'webAppRewriteRuleSet')
                }
              }
            }
          ]
        }
      }
    ]
  }
}

// Add diagnostic settings for Application Gateway
resource appGatewayDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = if (enableDiagnostics) {
  name: 'appGatewayDiagnostics'
  scope: applicationGateway
  properties: {
    workspaceId: logAnalyticsWorkspaceResourceId
    logs: [
      {
        categoryGroup: 'allLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
  }
}

output applicationGatewayId string = applicationGateway.id
output applicationGatewayName string = applicationGateway.name
output publicIPAddress string = publicIP.properties.ipAddress
