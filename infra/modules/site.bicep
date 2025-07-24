@description('Name of the  Azure App Service site.')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('Resource ID of the Log Analytics workspace to use for diagnostic settings.')
param logAnalyticsWorkspaceResourceId string

@description('Resource ID of the virtual network to link the private DNS zones.')
param virtualNetworkResourceId string = ''

@description('Resource ID of the subnet for the Vnet integration.')
param virtualNetworkSubnetResourceId string = ''

@description('Specifies whether network isolation is enabled. This will create a private endpoint for the Service Bus and link the private DNS zone.')
param networkIsolation bool = false

@description('Specifies whether the app service should be accessible only through private network')
param allowPrivateAccessOnly bool = false

@description('Resource ID of the subnet for the private endpoints.')
param privateEndpointsSubnetResourceId string = ''

@description('App Service Plan resource ID')
param serverFarmResourceId string

@description('User Assigned Managed Identity resource ID (optional)')
param userAssignedIdentityId string = ''

@description('App Service specific siteConfig')
param siteConfig object

@description('App settings array')
param appSettings array = []

@description('Optional. Enable IP restrictions for the App Service.')
param enableIpRestrictions bool = false

@description('Optional. Array of allowed IP addresses/ranges for App Service access.')
param allowedIpAddresses array = []

@description('Optional. Default action when IP restrictions are enabled.')
@allowed(['Allow', 'Deny'])
param ipRestrictionDefaultAction string = 'Deny'

// Build IP security restrictions array
var ipAllowRules = [
  for (ipAddress, index) in allowedIpAddresses: {
    ipAddress: ipAddress
    action: 'Allow'
    priority: 100 + index
    name: 'AllowedIP-${ipAddress}'
    description: 'Allowed IP address for -${ipAddress}'
  }
]

var ipRestrictionsArray = enableIpRestrictions ? ipAllowRules : []

module privateDnsZone 'br/public:avm/res/network/private-dns-zone:0.7.0' = if (networkIsolation && allowPrivateAccessOnly) {
  name: 'private-dns-site-deployment'
  params: {
    name: 'privatelink.azurewebsites.net'
    virtualNetworkLinks: [
      {
        virtualNetworkResourceId: virtualNetworkResourceId
      }
    ]
    tags: tags
  }
}

module site 'br/public:avm/res/web/site:0.16.0' = {
  name: take('${take(toLower(name), 24)}-site-deployment', 64)
  params: {
    name: name
    location: location
    tags: tags
    kind: 'app,linux'
    serverFarmResourceId: serverFarmResourceId
    keyVaultAccessIdentityResourceId: !empty(userAssignedIdentityId) ? userAssignedIdentityId : null
    httpsOnly: true
    clientAffinityEnabled: false
    virtualNetworkSubnetId: virtualNetworkSubnetResourceId
    publicNetworkAccess: enableIpRestrictions ? 'Enabled' : null
    siteConfig: union(siteConfig, {
      appSettings: appSettings
      ipSecurityRestrictions: ipRestrictionsArray
      ipSecurityRestrictionsDefaultAction: enableIpRestrictions ? ipRestrictionDefaultAction : 'Allow'
    })
    diagnosticSettings: [
      {
        name: 'default'
        workspaceResourceId: logAnalyticsWorkspaceResourceId
      }
    ]
    managedIdentities: {
      systemAssigned: !empty(userAssignedIdentityId) ? false : true
      userAssignedResourceIds: !empty(userAssignedIdentityId) ? [userAssignedIdentityId] : []
    }
    privateEndpoints: (networkIsolation && allowPrivateAccessOnly)
      ? [
          {
            privateDnsZoneGroup: {
              privateDnsZoneGroupConfigs: [
                {
                  name: 'privatelink.azurewebsites.net'
                  privateDnsZoneResourceId: privateDnsZone!.outputs.resourceId
                }
              ]
            }
            subnetResourceId: privateEndpointsSubnetResourceId
          }
        ]
      : []
  }
}

output defaultHostName string = site.outputs.defaultHostname
output name string = site.outputs.name
output resourceId string = site.outputs.resourceId
