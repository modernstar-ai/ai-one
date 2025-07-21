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

@description('Resource ID of the subnet for the private endpoint.')
param virtualNetworkSubnetResourceId string = ''

@description('Specifies whether network isolation is enabled. This will create a private endpoint for the Service Bus and link the private DNS zone.')
param networkIsolation bool = false

@description('App Service Plan resource ID')
param serverFarmResourceId string

@description('User Assigned Managed Identity resource ID (optional)')
param userAssignedIdentityId string = ''

@description('App Service specific siteConfig')
param siteConfig object

@description('App settings array')
param appSettings array = []

// module privateDnsZone 'br/public:avm/res/network/private-dns-zone:0.7.0' = if (networkIsolation) {
//   name: 'private-dns-site-deployment'
//   params: {
//     name: 'privatelink.azurewebsites.net'
//     virtualNetworkLinks: [
//       {
//         virtualNetworkResourceId: virtualNetworkResourceId
//       }
//     ]
//     tags: tags
//   }
// }

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
    siteConfig: union(siteConfig, {
      appSettings: appSettings
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
    // privateEndpoints: networkIsolation
    //   ? [
    //       {
    //         privateDnsZoneGroup: {
    //           privateDnsZoneGroupConfigs: [
    //             {
    //               name: 'privatelink.azurewebsites.net'
    //               privateDnsZoneResourceId: privateDnsZone.outputs.resourceId
    //             }
    //           ]
    //         }
    //         subnetResourceId: virtualNetworkSubnetResourceId
    //       }
    //     ]
    //   : []
  }
}

output defaultHostName string = site.outputs.defaultHostname
output name string = site.outputs.name
output resourceId string = site.outputs.resourceId
