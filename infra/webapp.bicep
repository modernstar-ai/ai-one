@description('Azure region for resource deployment')
param location string

@description('Resource tags')
param tags object

@minLength(1)
@maxLength(12)
@description('The name of the solution.')
param projectName string

@minLength(1)
@maxLength(4)
@description('The type of environment. e.g. local, dev, uat, prod.')
param environmentName string

@description('App Service Plan name')
param appServicePlanName string

@description('API App name')
param apiAppName string = toLower('${resourcePrefix}-apiapp')

@description('Resource prefix for naming resources')
param resourcePrefix string = toLower('${projectName}-${environmentName}')

@description('Web App name')
param webappName string = toLower('${resourcePrefix}-webapp')

@description('Log Analytics Workspace name')
param logAnalyticsWorkspaceName string

resource appServicePlan 'Microsoft.Web/serverfarms@2021-02-01' existing = {
  name: appServicePlanName
}

resource webAppManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-${webappName}'
  location: location
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' existing = {
  name: logAnalyticsWorkspaceName
}

module webAppModule './modules/site.bicep' = {
  name: 'webAppModule'
  params: {
    name: webappName
    location: location
    tags: union(tags, { 'azd-service-name': 'agilechat-web' })
    serverFarmResourceId: appServicePlan.id
    logAnalyticsWorkspaceResourceId: logAnalyticsWorkspace.id
    userAssignedIdentityId: webAppManagedIdentity.id
    siteConfig: {
      linuxFxVersion: 'node|18-lts'
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

output webAppDefaultHostName string = webAppModule.outputs.defaultHostName
output webAppManagedIdentityId string = webAppManagedIdentity.id
