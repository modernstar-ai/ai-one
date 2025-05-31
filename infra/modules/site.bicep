@description('Name of the  Azure App Service site.')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('App Service Plan resource ID')
param serverFarmId string

@description('The name of the workload\'s existing Log Analytics workspace.')
param logWorkspaceName string

@description('User Assigned Managed Identity resource ID (optional)')
param userAssignedIdentityId string = ''

@description('App Service specific siteConfig')
param siteConfig object

@description('App settings array')
param appSettings array = []

@description('Diagnostic settings name')
param diagnosticSettingsName string = 'AppServiceConsoleLogs'

resource logWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: logWorkspaceName
}

resource site 'Microsoft.Web/sites@2023-12-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    serverFarmId: serverFarmId
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: union(siteConfig, {
      appSettings: appSettings
    })
  }
  identity: !empty(userAssignedIdentityId)
    ? {
        type: 'UserAssigned'
        userAssignedIdentities: {
          '${userAssignedIdentityId}': {}
        }
      }
    : {
        type: 'SystemAssigned'
      }
}

resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: diagnosticSettingsName
  scope: site
  properties: {
    workspaceId: logWorkspace.id
    logs: [
      {
        category: 'AppServiceConsoleLogs'
        enabled: true
      }
    ]
    metrics: []
  }
}

output defaultHostName string = site.properties.defaultHostName
output name string = site.name
output resourceId string = site.id
