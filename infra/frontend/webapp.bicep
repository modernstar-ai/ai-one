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

@description('App Service Plan name')
param appServicePlanName string

@description('API App name')
param apiAppName string

@description('Web App name')
param webappName string = toLower('${resourcePrefix}-webapp')

@description('Log Analytics Workspace ID')
param logAnalyticsWorkspaceResourceId string

@description('Application Insights name')
param applicationInsightsName string = toLower('${resourcePrefix}-webapp')

@description('Whether to enable network isolation for resources')
param networkIsolation bool = false

@description('Specifies whether the app service should be accessible only through private network')
param allowPrivateAccessOnly bool = false

@description('Azure Virtual Network name')
param virtualNetworkName string = toLower('${resourcePrefix}-vnet')

@description('App Service subnet name')
param appServiceSubnetName string = 'AppServiceSubnet'

@description('Private Endpoints subnet name')
param privateEndpointsSubnetName string = 'PrivateEndpointsSubnet'

@description('Optional. Enable IP restrictions for the App Service to restrict access to Application Gateway only.')
param enableIpRestrictions bool = false

@description('Optional. Array of allowed IP addresses/ranges for App Service access (e.g., Application Gateway public IP).')
param allowedIpAddresses array = []

resource appServicePlan 'Microsoft.Web/serverfarms@2021-02-01' existing = {
  name: appServicePlanName
}

resource webAppManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-${webappName}'
  location: location
}

resource vnet 'Microsoft.Network/virtualNetworks@2023-05-01' existing = if (networkIsolation) {
  name: virtualNetworkName
}

var virtualNetworkResourceId = networkIsolation ? vnet.id : ''
var appServiceSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${appServiceSubnetName}' : ''
var privateEndpointsSubnetResourceId = networkIsolation ? '${vnet.id}/subnets/${privateEndpointsSubnetName}' : ''

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspaceResourceId
  }
}

module webAppModule '../modules/site.bicep' = {
  name: 'webAppModule'
  params: {
    name: webappName
    location: location
    tags: union(tags, { 'azd-service-name': 'agilechat-web' })
    serverFarmResourceId: appServicePlan.id
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspaceResourceId
    userAssignedIdentityId: webAppManagedIdentity.id
    networkIsolation: networkIsolation
    allowPrivateAccessOnly: allowPrivateAccessOnly
    virtualNetworkResourceId: virtualNetworkResourceId
    virtualNetworkSubnetResourceId: appServiceSubnetResourceId
    privateEndpointsSubnetResourceId: privateEndpointsSubnetResourceId
    enableIpRestrictions: enableIpRestrictions
    allowedIpAddresses: allowedIpAddresses
    siteConfig: {
      linuxFxVersion: 'node|22-lts'
      alwaysOn: true
      appCommandLine: 'npx serve -s dist'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
    }
    appSettings: [
      {
        name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
        value: 'false'
      }
      {
        name: 'ALLOWED_ORIGINS'
        value: 'https://${apiAppName}.azurewebsites.net'
      }
    ]
  }
}

output webAppName string = webAppModule.outputs.name
output webAppDefaultHostName string = webAppModule.outputs.defaultHostName
output webAppManagedIdentityId string = webAppManagedIdentity.id
