//parms
param name string = 'azurechat-demo'
param location string = resourceGroup().location
param tags object = {}

//calculate names
var appservice_name = toLower('${name}-app')
var webapp_name = toLower('${name}-webapp')
var apiapp_name = toLower('${name}-apiapp')

resource appServicePlan 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: appservice_name
  location: location
  tags: tags
  properties: {
    reserved: true
  }
  sku: {
    name: 'P0v3'
    tier: 'Premium0V3'
    size: 'P0v3'
    family: 'Pv3'
    capacity: 1
  }
  kind: 'linux'
}

resource webApp 'Microsoft.Web/sites@2020-06-01' = {
  name: webapp_name
  location: location
  tags: union(tags, { 'azd-service-name': 'agilechat-web' })
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'node|18-lts'
      alwaysOn: true
      appCommandLine: 'npx serve -s dist'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'      
    }
  }
  identity: { type: 'SystemAssigned'}

  resource configLogs 'config' = {
    name: 'logs'
    properties: {
      applicationLogs: { fileSystem: { level: 'Verbose' } }
      detailedErrorMessages: { enabled: true }
      failedRequestsTracing: { enabled: true }
      httpLogs: { fileSystem: { enabled: true, retentionInDays: 1, retentionInMb: 35 } }
    }
  }
}

resource apiApp 'Microsoft.Web/sites@2020-06-01' = {
  name: apiapp_name
  location: location
  tags: union(tags, { 'azd-service-name': 'agilechat-api' })
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNET|8.0'
      alwaysOn: true
      appCommandLine: 'dotnet agile-chat-api.dll'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'      
    }
  }
  identity: { type: 'SystemAssigned'}
}

output url string = 'https://${webApp.properties.defaultHostName}'
output api_url string = 'https://${apiApp.properties.defaultHostName}'
